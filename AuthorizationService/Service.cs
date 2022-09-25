using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AuthorizationService.Types;
using Isopoh.Cryptography.Argon2;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NetRoad;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuthorizationService;

public class Service
{
    private readonly Database _database;
    private readonly TcpListener _listener;

    private readonly string _clientAddress;
    
    public Service(string clientAddress)
    {
        WriteLine("Starting Auth Service...");
        
        // Connect to database
        try
        {
            // ReSharper disable once StringLiteralTypo
            _database = new Database("livedeck");
            
            // Test connection
            var usernameFilter = Builders<BsonDocument>.Filter.Eq("Username", $"test");
            _database.Exists("deck_accounts", usernameFilter);
        }
        catch (Exception e)
        {
            WriteLine("Connection to database failed!");
            WriteLine("Reason:");
            WriteLine(e.ToString());
            throw;
        }
        // ReSharper disable once StringLiteralTypo
        WriteLine("Connected to livedeck database");

        // Init new socket listener
        _listener = new TcpListener(IPAddress.Any, 1111);
        WriteLine("Listener created!");
        
        _clientAddress = clientAddress;
    }

    public void Start()
    {
        // Try to start listener
        WriteLine("Starting listener...");
        _listener.Start();
        WriteLine("Listener started!");
        
        // Start Client-Accept Forwarder
        Forward();
    }

    private void Forward()
    {
        while (true)
        {
            // Accept incoming client
            var client = _listener.AcceptTcpClient();
            var ip = ((IPEndPoint) client.Client.RemoteEndPoint!).Address;

            // Validate client
            if (!_clientAddress.Equals(ip.ToString()))
            {
                // Send error message
                try
                {
                    var errMsg = Encoding.UTF8.GetBytes("Authorization failed");
                    client.Client.Send(errMsg);
                    client.Client.Close();
                }
                finally
                {
                    WriteLine("Connection Attempt Failed!\n\t->Authorization failed");
                }
                continue;
            }

            // Forward to new thread handler with client-parameter
            var thread = new Thread(Handler);
            thread.Start(client);
        }
    }

    private void Handler(object? clientObj)
    {
        var listening = true;
        
        if (clientObj is null)
            return;
        
        // Current tcp client
        var client = (TcpClient)clientObj;

        // Network operator
        var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
        //var writer = new StreamWriter(client.GetStream(), Encoding.UTF8);
        
        while (listening)
        {
            try
            {
                while (!reader.EndOfStream)
                {
                    var result = reader.ReadLine();
                    
                    if (result == null)
                    {
                        var errorResult = new ResultType()
                        {
                            Result = "Error"
                        };
                                        
                        // Send error result
                        var errorResultMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorResult));
                        client.Client.Send(errorResultMsg);
                        break;
                    }
                    
                    // Check if content is valid json
                    var validJson = IsValidJson(result);
                    
                    dynamic data = JObject.Parse(result);
                    
                    // If not send error and break, check if json type-value is correct
                    if (!validJson || data.Type == null || data.Username == null)
                    {
                        // Build error message
                        var errorResult = new ResultType()
                        {
                            Result = "Error"
                        };
                                        
                        // Send error result
                        var errorResultMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorResult));
                        client.Client.Send(errorResultMsg);

                        // Kill thread
                        break;
                    }
                    
                    switch (data.Type.ToString())
                    {
                        case "login":
                            // Set user filter
                            var usernameFilter = Builders<BsonDocument>.Filter.Eq("Username", data.Username.ToString());

                            // Check if document with username credential is in database
                            var isUserExist = (bool)_database.Exists("accounts", usernameFilter);
                            
                            if (!isUserExist)
                            {
                                // Build error message
                                var error = new ErrorType()
                                {
                                    Reason = "USER_NOT_EXIST"
                                };

                                var errorMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(error));

                                // Send error
                                client.Client.Send(errorMsg);

                                // Kill thread
                                break;
                            }

                            // Account exist, get user doc and check password-hash
                            var userDocument = _database.Find("accounts", usernameFilter);
                            var loginDocument = BsonSerializer.Deserialize<LoginType>(userDocument);
                            var passwordHash = loginDocument.Hash;

                            if (!Argon2.Verify(passwordHash, data.Password.ToString()))
                            {
                                // Build error message
                                var error = new ErrorType()
                                {
                                    Reason = "PASSWORD_INCORRECT"
                                };

                                var errorMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(error));

                                // Send error
                                client.Client.Send(errorMsg);

                                // Kill thread
                                break;
                            }
                            
                            // Check for admin login
                            if (Equals(data.Username.ToString(), "admin"))
                            {
                                // Get admin token from db
                                var adminDocumentObj = (LoginType)BsonSerializer.Deserialize<LoginType>(userDocument);
                                var adminToken = adminDocumentObj.AdminToken;
                                
                                var successResult = new AdminType()
                                {
                                    Result = "Admin",
                                    AdminToken = adminToken
                                };
                                
                                var errorResultMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(successResult));
                                client.Client.Send(errorResultMsg);
                                return;
                            }
                            
                            // Generate token
                            var token = new Token(_database, loginDocument.Username);
                            token.Push();

                            try
                            {
                                // Create network client
                                var nClient = new NRoadTcpClient("127.0.0.1", 1112, Encoding.UTF8);
                                
                                var localToken = token;
                                var localDocument = loginDocument;
                                
                                nClient.DataReceived += (_, s) =>
                                {
                                    dynamic dynData = JObject.Parse(s);
                                    
                                    // Check if response is a result-response
                                    if (dynData.Result == null)
                                    {
                                        var errorResult = new ResultType()
                                        {
                                            Result = "Error"
                                        };
                                        
                                        // Send error result
                                        var errorResultMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorResult));
                                        client.Client.Send(errorResultMsg);
                                        return;
                                    }
                                    
                                    // Parse to result
                                    var resultData = JsonConvert.DeserializeObject<ResultType>(s);

                                    switch (resultData?.Result)
                                    {
                                        case "SUCCESS":
                                            var successResult = new ResultType()
                                            {
                                                Result = "Success",
                                                Token = localToken.AuthToken,
                                                UToken = localDocument.UToken
                                            };
                                        
                                            // Send success result
                                            var successResultMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(successResult));
                                            client.Client.Send(successResultMsg);
                                            break;
                                    }
                                };
                                
                                nClient.Connect();
                                
                                var build = new BuildType
                                {
                                    Type = "build",
                                    Username = data.Username,
                                    Token = token.AuthToken
                                };
                                
                                var panelBuildRequestMsg = JsonConvert.SerializeObject(build);
                                nClient.Send(panelBuildRequestMsg);
                            }
                            catch (Exception e)
                            {
                                WriteLine(e.ToString());
                                throw;
                            }
                            
                            break;
                    }
                }
            }
            finally
            {
                listening = false;
            }
        }
        try
        {
            client.Close();
        }
        catch
        {
            WriteLine("Error Client cannot be closed");
        } 
    }
    
    private static bool IsValidJson(string s)
    {
        try
        {
            JToken.Parse(s);
            return true;
        }
        catch (JsonReaderException ex)
        {
            Trace.WriteLine(ex);
            return false;
        }
    }

    private static void WriteLine(string message)
    {
        var prefix = DateTime.Now;
        Console.WriteLine($"[{prefix:G}]\t{message}");
    }
}
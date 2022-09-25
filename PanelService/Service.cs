using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanelService.Types;

namespace PanelService;

public class Service
{
    private readonly TcpListener _listener;

    private readonly string _clientAddress;
    public Service(string clientAddress)
    {
        WriteLine("Starting Panel Service...");

        // Init new socket listener
        _listener = new TcpListener(IPAddress.Any, 1112);
        WriteLine("Listener created.");
        
        _clientAddress = clientAddress;
    }

    public void Start()
    {
        // Try to start listener
        _listener.Start();
        WriteLine("Listener started.");
        
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
        var writer = new StreamWriter(client.GetStream(), Encoding.UTF8);
        
        while (listening)
        {
            try
            {
                while (!reader.EndOfStream && listening)
                {
                    var result = reader.ReadLine();

                    if (result == null)
                        return;
                    
                    // Check if content is valid json
                    var validJson = IsValidJson(result);

                    // If not send error and break
                    if (!validJson)
                    {
                        // Build error message
                        var errorResult = new ResultType()
                        {
                            Result = "ERROR_JSON_INVALID"
                        };
                                
                        // Send error
                        writer.WriteLine(JsonConvert.SerializeObject(errorResult));
                        writer.Flush();

                        // Kill thread
                        break;
                    }

                    dynamic data = JObject.Parse(result);

                    // Check if json type-value is correct
                    if (data.Type == null)
                    {
                        // Build error message
                        var errorResult = new ResultType()
                        {
                            Result = "ERROR_TYPE_VALUE_INCORRECT"
                        };
                                
                        // Send error
                        writer.WriteLine(JsonConvert.SerializeObject(errorResult));
                        writer.Flush();
                        
                        // Kill thread
                        break;
                    }

                    switch (data.Type.ToString())
                    {
                        case "build":
                            var buildType = JsonConvert.DeserializeObject<BuildType>(result);
                            var templatePath = $"/home/[your domain]/PanelService/templates/{buildType?.Username}";
                            
                            // Check if directory exists
                            if (!Directory.Exists(templatePath))
                            {
                                // Build error message
                                var errorResult = new ResultType()
                                {
                                    Result = "ERROR_TEMPLATE_NOT_EXIST"
                                };

                                writer.WriteLine(JsonConvert.SerializeObject(errorResult));
                                writer.Flush();
                                break;
                            }

                            // Copy directory 
                            Copy(templatePath, $"/var/www/[your domain]/public/deck/{buildType?.Token}");
                            
                            // Send success
                            var resultType = new ResultType()
                            {
                                Result = "SUCCESS"
                            };
                            
                            writer.WriteLine(JsonConvert.SerializeObject(resultType));
                            writer.Flush();
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
            WriteLine("Error! Client cannot be closed");
        } 
    }
    
    private void Copy(string sourceDirectory, string targetDirectory)
    {
        var diSource = new DirectoryInfo(sourceDirectory);
        var diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    private void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        // Copy each file into the new directory.
        foreach (var fi in source.GetFiles())
        {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (var diSourceSubDir in source.GetDirectories())
        {
            var nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
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
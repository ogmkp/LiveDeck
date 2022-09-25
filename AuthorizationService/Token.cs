using AuthorizationService.Types;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace AuthorizationService;

public class Token
{
    private readonly Database _database;
    
    private readonly string _username;
    public readonly string AuthToken;
 
    public Token(Database db, string username)
    {
        _database = db;
        _username = username;
        
        AuthToken = GenerateToken();
    }

    public void Push()
    {
        // Kill possible other session on the user
        KillRestSession();
        
        // Build Session Details
        var information = new BsonDocument()
        {
            { "Username", $"{_username}" },
            { "Token", $"{AuthToken}" }
        };

        // Insert session in database
        _database.Insert("sessions", information);
    }
    
    private void KillRestSession()
    {
        const string collection = "sessions";
        
        // Set user filter
        var usernameFilter = Builders<BsonDocument>.Filter.Eq("Username", $"{_username}");

        // Check if session exists in database
        if (_database.Exists(collection, usernameFilter))
        {
            // Fetch current database document
            var document = _database.Find(collection, usernameFilter);
            var buildType = BsonSerializer.Deserialize<SessionType>(document);

            // Get current token
            var currentToken = buildType?.Token;
            
            // Delete session if exists in directory
            var sessionPath = $"/var/www/[your domain]/public/deck/{currentToken}";
            if (Directory.Exists(sessionPath)) 
                DeleteDirectory(sessionPath);
        }

        // Delete session in database
        _database.Delete(collection, usernameFilter);
    }
    
    private static void DeleteDirectory(string directory)
    {
        var files = Directory.GetFiles(directory);
        var dirs = Directory.GetDirectories(directory);

        foreach (var file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(directory, false);
    }
    
    private string GenerateToken()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}
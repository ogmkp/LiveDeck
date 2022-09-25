namespace AuthorizationService.Types;

public class LoginType
{
    public object _id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Hash { get; set; }
    public string UToken { get; set; }
    public string AdminToken { get; set; }
}
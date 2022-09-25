namespace AuthorizationService.Types;

public class ErrorType
{
    public string Type { get; } = "Error";
    public string Reason { get; set; }
}
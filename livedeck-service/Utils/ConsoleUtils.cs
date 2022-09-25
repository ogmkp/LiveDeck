namespace livedeck_service.Utils;

public static class ConsoleUtils
{
    private static readonly string[] Prefix = {"[INFO]\t", "[WARN]\t", "[ERR]\t"};
    
    public static void WriteLine(string message, WriteType type)
    {
        // Variable Message
        Console.WriteLine(Prefix[(int)type] + "{0}", message);
    }
    public static void WriteLine(string message)
    {
        // Standard: Info Message
        Console.WriteLine(Prefix[0] + "{0}", message);
    }
}
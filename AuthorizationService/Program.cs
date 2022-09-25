namespace AuthorizationService;

static class Program
{
    public static void Main(string[] arg) => new Service(arg[0]).Start();
}
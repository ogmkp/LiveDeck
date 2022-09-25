using livedeck_service.Config;
using livedeck_service.Module;
using RequestInterpreter;

namespace livedeck_service;

internal static class Program
{
    private static ConfigLoader? _cLoader;
    private static Interpreter? _interpreter;
    
    private static RefreshModule? _refreshModule;

    private static RefreshInfo _lastRefreshInfo = new (new bool[8]);
    
    private static void Main()
    {
        Intro();
        
        _lastRefreshInfo.DataState = new bool[8];
        
        // Initialize config
        ConfigLoader.Init();
        _cLoader = new ConfigLoader();
        
        var sCfg = _cLoader.GetConfig<SettingsConfig>();

        WriteLine("Creating and registering interpreter...");
        _interpreter = new Interpreter(sCfg.ObsWebsocketAddress, sCfg.ObsWebsocketPort, sCfg.ObsWebsocketPassword);
        _interpreter.WebsocketConnected += WebsocketConnected;
        _interpreter.WebsocketDisconnected += WebsocketDisconnected;
        WriteLine("Interpreter -> OK");

        _interpreter.ConnectToObsWebsocket() ;
        
        Console.ReadLine();
    }

    private static void WebsocketConnected(object sender)
    {
        if (_cLoader == null)
            throw new Exception("Config Loader cannot load");
               
        _refreshModule = new RefreshModule(_cLoader);
        _refreshModule.RestDataReceived += RestDataReceived;
        _refreshModule.Start();
    }
    
    private static void WebsocketDisconnected(object sender)
    {
        Console.WriteLine("WebSocket disconnected");

        if (_refreshModule == null)
            return;
        
        Thread.Sleep(3000);
        
        _refreshModule.AllowRefresh = false;
        _interpreter?.ConnectToObsWebsocket();
    }

    private static void RestDataReceived(object sender, RefreshInfo e)
    {
        if (ArrayEquals(_lastRefreshInfo.DataState.ToList(), e.DataState.ToList())) 
            return;

        _interpreter?.Handle(e.DataState);
        _lastRefreshInfo = e;
    }
    
    private static bool ArrayEquals(IEnumerable<bool> arr1, IReadOnlyList<bool> arr2)
    {
        return !arr1.Where((t, i) => t != arr2[i]).Any();
    }
    
    private static void WriteLine(string message)
    {
        var prefix = DateTime.Now;
        Console.WriteLine($"[{prefix:G}]\t{message}");
    }

    private static void Intro()
    {
        Console.WriteLine("                                                            ");
        Console.WriteLine("                       ██████████████████                   ");
        Console.WriteLine("             █████████████████████████████████              ");
        Console.WriteLine("            █████████████████████████████████████           ");
        Console.WriteLine("            ███████████████████████████████████████         ");
        Console.WriteLine("             ████████████             ███████████████       ");
        Console.WriteLine("     ████████████████████                █████████████      ");
        Console.WriteLine("     ███      ████████████                 ████████████     ");
        Console.WriteLine("      ███      ███████████                  ████████████    ");
        Console.WriteLine("       ███     ████████████                 ████████████    ");
        Console.WriteLine("       ███      ███████████                 ████████████    ");
        Console.WriteLine("        ███     ████████████                ████████████    ");
        Console.WriteLine("        ███      ████████████              ████████████     ");
        Console.WriteLine("         ███      ███████████            █████████████      ");
        Console.WriteLine("          ███     ████████████        ███████████████       ");
        Console.WriteLine("          ███      ████████████████████████████████         ");
        Console.WriteLine("           ███     ██████████████████████████████           ");
        Console.WriteLine("           ███      ██████████████████████████              ");
        Console.WriteLine("            ███      ███████████████████████                ");
        Console.WriteLine("             ██      ███████████          ██                ");
        Console.WriteLine("             ███                          ███               ");
        Console.WriteLine("              ███                 ██████████                ");
        Console.WriteLine("              ███       ██████████                          ");
        Console.WriteLine("               █████████                                    ");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("█████████████████████████████████████████");
        Console.WriteLine("█            LiveDeck Service           █");
        Console.WriteLine("█               by timfvb               █");
        Console.WriteLine("█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█");
        Console.WriteLine("█ version b1.7                          █");
        Console.WriteLine("█████████████████████████████████████████");
        Console.WriteLine();
        Console.WriteLine();
    }
}
using OBSWebsocketDotNet;

namespace RequestInterpreter;

public class Interpreter
{
    private readonly PanelConfig _config;
    
    public delegate void Websocket(object sender);

    public Websocket? WebsocketConnected;
    public Websocket? WebsocketDisconnected;

    private readonly OBSWebsocket _websocket;
    
    private readonly Handler _handler;
    
    private string ObsWebsocketUri { get; set; }
    private string ObsPassword { get; set; }

    public Interpreter(string obsAddress, int obsPort, string obsPassword)
    { 
        _websocket = new OBSWebsocket();

        _websocket.Connected += (sender, _) => WebsocketConnected?.Invoke(sender!); 
        _websocket.Disconnected += (sender, _) => WebsocketDisconnected?.Invoke(sender!);

        _handler = new Handler(ref _websocket);
        
        ObsWebsocketUri = $"ws://{obsAddress}:{obsPort}";
        ObsPassword = obsPassword;
       
       _config = GetPanelConfig();
    }

    public void Handle(bool[] request)
    {
        _handler.Handle(_config, request);
    }

    private static PanelConfig GetPanelConfig()
    {
        var config = new StreamReader("config/panel.livedeck");
        var content = config.ReadToEnd();
        config.Close();

        var cfg = new PanelConfig(new Dictionary<int, List<Dictionary<string, Tuple<string, string>>>>());

        var lines = content.Split("\n");
        var count = 0;

        foreach (var line in lines)
        {
            var res = GetStrBetweenTags(line, "[c]", "[/c]");

            var actions = res?.Split(",");
            var actionsList = new List<Dictionary<string, Tuple<string, string>>>();

            if (res == " " | res == "-")
            {
                var actionList = new Dictionary<string, Tuple<string, string>> { { "-", new Tuple<string, string>("-", "-") } };
                actionsList.Add(actionList);
                cfg.Configuration.Add(count++, actionsList);
                
                continue;
            }
            
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    var actionList = new Dictionary<string, Tuple<string, string>>();

                    var command = action.Split(':')[0];
                    var details = action.Split(':')[1];

                    // Multi details
                    if (details.Contains('|'))
                    {
                        var d1 = details.Split('|')[0];
                        var d2 = details.Split('|')[1];

                        actionList.Add(command, new Tuple<string, string>(d1, d2));
                        actionsList.Add(actionList);
                        continue;
                    }

                    // Single Details
                    var d = details.Split('|')[0];
                    actionList.Add(command, new Tuple<string, string>(d, ""));
                    actionsList.Add(actionList);
                }
            }
            else
                throw new Exception("Config is wrong");

            // Add new line
            cfg.Configuration.Add(count++, actionsList);
        }

        return cfg;
    }
    
    private static string? GetStrBetweenTags(string value, string startTag, string endTag)
    {
        if (!value.Contains(startTag) || !value.Contains(endTag)) return null;
        var index = value.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
        return value.Substring(index, value.IndexOf(endTag, StringComparison.Ordinal) - index);
    }
    
    public void ConnectToObsWebsocket()
    {
        try
        {
            WriteLine("Connecting OBS Web-Socket...");
            _websocket.Connect(ObsWebsocketUri, ObsPassword);
            WriteLine("OBS Web-Socket -> Connected");
        }
        catch (AuthFailureException)
        {
            WriteLine("OBS Websocket -> Error: No Auth");
            Console.Read();
            Environment.Exit(0x1);
        }
        catch
        {
            Thread.Sleep(2500);
            ConnectToObsWebsocket();
        }
    }
    
    private static void WriteLine(string message)
    {
        var prefix = DateTime.Now;
        Console.WriteLine($"[{prefix:G}]\t{message}");
    }
}
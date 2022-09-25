namespace RequestInterpreter;

public class PanelConfig
{
    public PanelConfig(Dictionary<int, List<Dictionary<string, Tuple<string, string>>>> configuration)
    {
        Configuration = configuration;
    }

    public Dictionary<int, List<Dictionary<string, Tuple<string, string>>>> Configuration { get; set; }
}
using System.Text;
using livedeck_service.Utils;
using Newtonsoft.Json;
using static livedeck_service.Utils.ConsoleUtils;

namespace livedeck_service.Config;

public class ConfigLoader
{
    
    public static void Init()
    {
        var configDirectoryExists = Directory.Exists("config");
        WriteLine(configDirectoryExists ? "Config Directory Found" : "Config Directory not found",
            configDirectoryExists ? WriteType.Info : WriteType.Warn);
        
        // Check for config folder existence
        if (Directory.Exists("config")) return;
        
        // Create configs
        WriteLine("Creating config...");
        Directory.CreateDirectory("config");
        
        var settingsCfg = File.Create("config/settings.json");
        settingsCfg.Write(Encoding.UTF8.GetBytes(ToJsonConfigFormat(JsonConvert.SerializeObject(new SettingsConfig()))));
        settingsCfg.Close();

        WriteLine("Config created");
    }

    public T GetConfig<T>()
    {
        var type = typeof(T);
        StreamReader reader;

        if (type == typeof(SettingsConfig))
            reader = new StreamReader("config/settings.json");
        else
            return default!;
        
        var content = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(content)!;
    }

    private static string ToJsonConfigFormat(string json)
    {
        json = json.Replace("{", "{\n\t");
        json = json.Replace("\":", "\": ");
        json = json.Replace(",", ",\n\t");
        json = json.Replace("}", "\n}");
        return json;
    }
}
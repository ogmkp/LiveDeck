namespace livedeck_service.Config;

public class SettingsConfig
{
    public string UToken { get; set; } = "";
    public int RestRefreshInterval { get; set; } = 800;
    public string RestControllerUri { get; set; } = "https://[your website]/api/controller";
    public string ObsWebsocketAddress { get; set; } = "127.0.0.1";
    public int ObsWebsocketPort { get; set; } = 4444;
    public string ObsWebsocketPassword {get; set; } = "";
}
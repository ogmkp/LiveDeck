using livedeck_service.Config;
using Newtonsoft.Json;

namespace livedeck_service.Module;

public class RefreshModule
{
    public delegate void RestData<in TEventArgs>(object sender, TEventArgs e);

    public RestData<RefreshInfo>? RestDataReceived;

    private readonly SettingsConfig _settingsConfig;
    private readonly HttpClient _client;
    
    public bool AllowRefresh = true;
    
    public RefreshModule(ConfigLoader cLoader)
    {
        _settingsConfig = cLoader.GetConfig<SettingsConfig>();
        _client = new HttpClient();
    }
    

    public void Start()
    {
        if (_settingsConfig.RestRefreshInterval <= -1)
            throw new Exception("Config Value 'RestRefreshInterval' is invalid");
        var restInterval = _settingsConfig.RestRefreshInterval;

        async void ThreadStart()
        {
            while (AllowRefresh)
            {
                // Get result from task
                var refreshResult = await Refresh();

                if (refreshResult == null)
                    continue;
                
                // Invoke Rest Event
                RestDataReceived?.Invoke(this, refreshResult);

                // Sleep thread in ms for given rest-interval
                Thread.Sleep(restInterval);
            }
        }

        var refresherThread = new Thread(ThreadStart);
        
        // Start refresh thread
        refresherThread.Start();
    }

    private async Task<RefreshInfo?> Refresh()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settingsConfig.UToken))
                throw new Exception("Config Value 'UToken' is invalid");
            if (string.IsNullOrWhiteSpace(_settingsConfig.RestControllerUri))
                throw new Exception("Config Value 'RestControllerUri' is invalid");

            // Get details from config and build rest uri
            var uToken = _settingsConfig.UToken;
            var restUri = $"{_settingsConfig.RestControllerUri}?utoken={uToken}";

            // GET-Request to rest uri 
            var res = await _client.GetStringAsync(restUri);

            // UToken is wrong case
            if (res.Equals("Not found"))
                throw new AggregateException("UToken is wrong");

            // Deserialize response and invoke
            var refreshObj = JsonConvert.DeserializeObject<RefreshInfo>(res);
            if (refreshObj == null)
                throw new Exception("JSON is invalid");

            return refreshObj;
        }
        catch
        {
            Thread.Sleep(4500);
        }
        
        return null!;
    }
    
}
using OBSWebsocketDotNet;

namespace RequestInterpreter;

class Handler
{
    private readonly OBSWebsocket _websocket;

    private bool[] _lastResults = new bool[8];
    
    public Handler(ref OBSWebsocket websocket)
    {
        _websocket = websocket;
    }

    public void Handle(PanelConfig config, bool[] requestResults)
    {
        var diffIndexes = GetDifferentIndexes(requestResults, _lastResults);

        if (diffIndexes.Count == 1)
            Execute(config.Configuration[diffIndexes[0]], requestResults[diffIndexes[0]]);
        else
            for (var i = 0; i < requestResults.Length; i++)
                Execute(config.Configuration[i], requestResults[i]);
        
        _lastResults = requestResults;
    }
    
    private static List<int> GetDifferentIndexes(IReadOnlyList<bool> arr1, IReadOnlyList<bool> arr2)
    {
        // List to hold indexes of differences
        var lstDiffs = new List<int>();

        // Assure neither array is null and lengths match
        if (arr1.Count != arr2.Count) return (lstDiffs);
        
        // Loop through both arrays and check each value
        for (var idx = 0; idx < arr1.Count; idx++)
        {
            if (arr2 != null && arr1[idx] != arr2[idx])
            {
                // Add index to list since values do not match
                lstDiffs.Add(idx);
            }
        }

        // Your list of different indexes
        return lstDiffs;
    }
    
    private void Execute(List<Dictionary<string, Tuple<string, string>>> actions, bool state)
    {
        state = !state;
        
        foreach (var action in actions)
        {
            foreach (var (command, detail) in action)
            {
                if (string.IsNullOrWhiteSpace(command))
                    return;
                if (string.IsNullOrWhiteSpace(detail.Item1))
                    throw new Exception("Config cannot be interpreted");

                try
                {
                    switch (command)
                    {
                        case "SetSourceFilterVisibility":
                            if (string.IsNullOrWhiteSpace(detail.Item2))
                                throw new Exception("Config cannot be interpreted");
                            _websocket.SetSourceFilterVisibility(detail.Item1, detail.Item2, state);
                            break;
                        case "SetMute":
                            _websocket.SetMute(detail.Item1, state);
                            break;
                        case "SetSourceVisibility":
                            _websocket.SetSourceRender(detail.Item1, state);
                            break;
                        case "SetInvertedSourceVisibility":
                            _websocket.SetSourceRender(detail.Item1, !state);
                            break;
                        case "SetSourceVisibilitySwitch":
                            _websocket.SetSourceRender(detail.Item1, state);
                            _websocket.SetSourceRender(detail.Item2, !state);
                            break;
                        case "SetSourceVisibilityStatic":
                            _websocket.SetSourceRender(detail.Item1, bool.Parse(detail.Item2));
                            break;
                        case "SetSceneSwitch":
                            _websocket.SetCurrentScene(state ? detail.Item1 : detail.Item2);
                            break;
                        case "-":
                            // No action
                            break;
                        default:
                            throw new Exception("Config cannot be interpreted");
                    }
                }
                catch (ErrorResponseException)
                {
                    Console.WriteLine("Interpret Error");
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
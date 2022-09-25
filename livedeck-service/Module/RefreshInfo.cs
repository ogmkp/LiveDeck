namespace livedeck_service.Module;

public class RefreshInfo : EventArgs
{
    public RefreshInfo(bool[] dataState)
    {
        DataState = dataState;
    }

    public bool[] DataState { get; set; }
}
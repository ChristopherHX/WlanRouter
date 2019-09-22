public interface IWlanRouter
{
    string SSID { get; set; }
    string Key { get; set; }
    void Start();
    bool IsRunning();
    void Stop();
}
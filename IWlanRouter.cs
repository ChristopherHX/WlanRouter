public interface IWlanRouter : IRouter {
    string SSID { get; set; }
    string Key { get; set; }
}
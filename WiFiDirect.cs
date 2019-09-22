using Windows.Devices.WiFiDirect;
using Windows.Security.Credentials;
public class WiFiDirect : IWlanRouter {
    WiFiDirectAdvertisementPublisher _publisher;
    public WiFiDirect() {
        // Begin advertising for legacy clients
        _publisher = new WiFiDirectAdvertisementPublisher();

        // Note: If this flag is not set, the legacy parameters are ignored
        _publisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;

        // Setup Advertisement to use a custom SSID and WPA2 passphrase
        _publisher.Advertisement.LegacySettings.IsEnabled = true;

        _publisher.Advertisement.LegacySettings.Passphrase = new PasswordCredential();

        try {
            var nw = new NativeWiFi();
            SSID = nw.SSID;
            Key = nw.Key;
        } catch {

        }
    }

    public string SSID {
        get => _publisher.Advertisement.LegacySettings.Ssid;
        set => _publisher.Advertisement.LegacySettings.Ssid = value;
    }

    public string Key { get => _publisher.Advertisement.LegacySettings.Passphrase.Password ?? ""; set => _publisher.Advertisement.LegacySettings.Passphrase.Password = value; }

    public void Start() {
        if (!IsRunning())
        {
            _publisher.Start();
        }
    }

    public bool IsRunning() {
        return _publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started;
    }

    public void Stop() {
        if (IsRunning())
        {
            _publisher.Stop();
        }
    }

    ~WiFiDirect() {
        Stop();
        try {
            var nw = new NativeWiFi();
            nw.SSID = SSID;
            nw.Key = Key;
        } catch {

        }
    }
}
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
        if (_publisher.Status != WiFiDirectAdvertisementPublisherStatus.Started)
        {
            _publisher.Start();
        }
    }

    public void Stop() {
        if (_publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started)
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
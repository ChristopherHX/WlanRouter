using Windows.Devices.WiFiDirect;
using Windows.Security.Credentials;
public class WiFiDirect {
    WiFiDirectAdvertisementPublisher _publisher;
    public WiFiDirect() {
        // Begin advertising for legacy clients
        _publisher = new WiFiDirectAdvertisementPublisher();

        // Note: If this flag is not set, the legacy parameters are ignored
        _publisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;

        // Setup Advertisement to use a custom SSID and WPA2 passphrase
        _publisher.Advertisement.LegacySettings.IsEnabled = true;
    }

    public void SetSSID(string ssid) {
        _publisher.Advertisement.LegacySettings.Ssid = ssid;
    }

    public void SetPassword(string password) {
        _publisher.Advertisement.LegacySettings.Passphrase = new PasswordCredential { Password = password };
    }

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
}
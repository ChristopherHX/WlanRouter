using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

public class NetworkOperatorTetheringManager : IWlanRouter, INetRouter {
    private Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager manager = null;
    public NetworkOperatorTetheringManager() {

    }

    public string[] GetConnections() {
        return (from c in NetworkInformation.GetConnectionProfiles() select c.ProfileName + System.Environment.NewLine + c.NetworkAdapter.NetworkAdapterId.ToString()).ToArray();
    }

    public void SetConnection(string name) {
        var desc = name.Split(System.Environment.NewLine[0]);
        manager = Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager.CreateFromConnectionProfile((from c in NetworkInformation.GetConnectionProfiles() where c.ProfileName == desc[0] select c).First());
    }

    public string SSID { get; set; }
    public string Key { get; set; }

    public async Task Start() {
        await manager.ConfigureAccessPointAsync(new Windows.Networking.NetworkOperators.NetworkOperatorTetheringAccessPointConfiguration { Ssid = SSID, Passphrase = Key });
        await manager.StartTetheringAsync();
    }

    public async Task Stop() {
        await manager.StopTetheringAsync();
    }

    public bool IsRunning() {
        return manager?.TetheringOperationalState == Windows.Networking.NetworkOperators.TetheringOperationalState.On;
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class WlanRouterWrapper : IWlanRouter, INetRouter {
    private IWlanRouter wrouter;
    private NetCon netcon;

    public WlanRouterWrapper(IWlanRouter wrouter, NetCon netcon) {
        this.wrouter = wrouter;
        this.netcon = netcon;
    }

    public string SSID {
        get => wrouter.SSID;
        set => wrouter.SSID = value;
    }

    public string Key {
        get => wrouter.Key;
        set => wrouter.Key = value;
    }

    public async Task Start() {
        await wrouter.Start();
        var privatec = (from c in NetCon.GetConnections() where c.DeviceName.StartsWith("Microsoft ") select c).First();
        netcon.SetPrivateConnection(privatec);
        await netcon.Start();
    }

    public async Task Stop() {
        await netcon.Stop();
        await wrouter.Stop();
    }

    public bool IsRunning() {
        return wrouter.IsRunning();
    }

    public string[] GetConnections() {
        var connections = new List<string> { "No Sharing" };
        var cons = ((INetRouter)netcon)?.GetConnections();
        return (cons != null ? connections.Concat(cons) : connections).ToArray();
    }

    public void SetConnection(string name) {
        ((INetRouter)netcon).SetConnection(name);
    }
}
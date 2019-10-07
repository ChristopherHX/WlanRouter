using System.Threading.Tasks;
public class NetCon : INetRouter {
    private INetConnection[] endpoints = new INetConnection[2];

    public NetCon() {

    }

    public string[] GetConnections() {
        INetSharingManager sharingManager = new NetSharingManager();
        (from INetConnection c in sharingManager.EnumEveryConnection
                                    where sharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                                    select sharingManager.NetConnectionProps[c].Name + System.Environment.NewLine + sharingManager.NetConnectionProps[c].DeviceName).ToArray();
    }

    // public string GetConnection() {
    //     ret
    // }

    public void SetConnection(string name) {
        INetSharingManager sharingManager = new NetSharingManager();
        endpoints[1] = (from INetConnection con in sharingManager.EnumEveryConnection where sharingManager.get_NetConnectionProps(con).DeviceName == share select con).First();                            
    }

    public async Task Start() {
        Stop();
        if(endpoints[1] != null) {
            INetSharingManager sharingManager = new NetSharingManager();
            sharingManager.INetSharingConfigurationForINetConnection[endpoints[1]].EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
            sharingManager.INetSharingConfigurationForINetConnection[endpoints[0]].EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
        }
    }

    public async Task Stop() {
        INetSharingManager sharingManager = new NetSharingManager();
        foreach (var con in from INetSharingConfiguration conf in from INetConnection c in sharingManager.EnumEveryConnection
                            select sharingManager.INetSharingConfigurationForINetConnection[c]
                            where conf.SharingEnabled
                            select conf) {
            con.DisableSharing();
        }

        var scope = new ManagementScope("root\\Microsoft\\HomeNet");
        scope.Connect();

        foreach (var type in new string[] { "HNet_ConnectionProperties", "HNet_Connection" }) {
            var query = new ObjectQuery("SELECT * FROM " + type);
            var srchr = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject entry in srchr.Get()) {
                entry.Dispose();
                entry.Delete();
            }
        }
        {
            var options = new PutOptions();
            options.Type = PutType.UpdateOnly;

            var query = new ObjectQuery("SELECT * FROM HNet_ConnectionProperties");
            var srchr = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject entry in srchr.Get()) {
                entry["IsIcsPrivate"] = false;
                entry["IsIcsPublic"] = false;
                entry.Put(options);
            }
        }
    }
}
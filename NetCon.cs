using NETCONLib;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Management;

public class NetCon : INetRouter {
    public struct Connection {
        public string Name;
        public string DeviceName;
    }

    private INetConnection[] endpoints = new INetConnection[2];

    public NetCon() {

    }

    string[] INetRouter.GetConnections() {
        INetSharingManager sharingManager = new NetSharingManager();
        return (from INetConnection c in sharingManager.EnumEveryConnection
                                    where sharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                                    select sharingManager.NetConnectionProps[c].Name + System.Environment.NewLine + sharingManager.NetConnectionProps[c].DeviceName).ToArray();
    }

    public static Connection[] GetConnections() {
        INetSharingManager sharingManager = new NetSharingManager();
        return (from INetConnection c in sharingManager.EnumEveryConnection
                                    where sharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                                    select new Connection { Name = sharingManager.NetConnectionProps[c].Name, DeviceName = sharingManager.NetConnectionProps[c].DeviceName}).ToArray();
    }

    // public string GetConnection() {
    //     ret
    // }

    public void SetPrivateConnection(Connection connection) {
        INetSharingManager sharingManager = new NetSharingManager();
        endpoints[0] = (from INetConnection con in sharingManager.EnumEveryConnection where sharingManager.get_NetConnectionProps(con).DeviceName == connection.DeviceName select con).First();                            
    }

    void INetRouter.SetConnection(string name) {
        var desc = name.Split(System.Environment.NewLine[0]);
        INetSharingManager sharingManager = new NetSharingManager();
        endpoints[1] = (from INetConnection con in sharingManager.EnumEveryConnection where sharingManager.get_NetConnectionProps(con).Name == desc[0] select con).FirstOrDefault();                            
    }

    public void SetConnection(Connection connection) {
        INetSharingManager sharingManager = new NetSharingManager();
        endpoints[1] = (from INetConnection con in sharingManager.EnumEveryConnection where sharingManager.get_NetConnectionProps(con).Name == connection.Name select con).First();                            
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

    public bool IsRunning() {
        INetSharingManager sharingManager = new NetSharingManager();
        return endpoints[0] != null && sharingManager.INetSharingConfigurationForINetConnection[endpoints[0]].SharingEnabled && endpoints[1] != null && sharingManager.INetSharingConfigurationForINetConnection[endpoints[1]].SharingEnabled;
    }
}
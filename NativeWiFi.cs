using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DWORD = System.Int32;
using HANDLE = System.Int32;
using BOOL = System.Boolean;
using ULONG = System.UInt64;
enum WLAN_OPCODE_VALUE_TYPE {

}

enum WLAN_HOSTED_NETWORK_STATE {
  wlan_hosted_network_unavailable,
  wlan_hosted_network_idle,
  wlan_hosted_network_active
}

enum WLAN_HOSTED_NETWORK_PEER_AUTH_STATE {
  wlan_hosted_network_peer_state_invalid,
  wlan_hosted_network_peer_state_authenticated
}

[StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
struct WLAN_HOSTED_NETWORK_PEER_STATE {
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
  byte[]                              PeerMacAddress;
  WLAN_HOSTED_NETWORK_PEER_AUTH_STATE PeerAuthState;
}

enum DOT11_PHY_TYPE : uint { 
  dot11_phy_type_unknown     = 0,
  dot11_phy_type_any         = 0,
  dot11_phy_type_fhss        = 1,
  dot11_phy_type_dsss        = 2,
  dot11_phy_type_irbaseband  = 3,
  dot11_phy_type_ofdm        = 4,
  dot11_phy_type_hrdsss      = 5,
  dot11_phy_type_erp         = 6,
  dot11_phy_type_ht          = 7,
  dot11_phy_type_vht         = 8,
  dot11_phy_type_IHV_start   = 0x80000000,
  dot11_phy_type_IHV_end     = 0xffffffff
}

[StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
struct GUID {
  ulong          Data1;
  ushort         Data2;
  ushort         Data3;
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
  byte[]         Data4;
}

[StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
struct WLAN_HOSTED_NETWORK_STATUS {
  public WLAN_HOSTED_NETWORK_STATE      HostedNetworkState;
  GUID                           IPDeviceID;
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
  byte[]                         wlanHostedNetworkBSSID;
  DOT11_PHY_TYPE                 dot11PhyType;
  ULONG                          ulChannelFrequency;
  DWORD                          dwNumberOfPeers;
//   WLAN_HOSTED_NETWORK_PEER_STATE **PeerList;
  IntPtr PeerList;
}

enum WLAN_HOSTED_NETWORK_REASON {
  wlan_hosted_network_reason_success,
  wlan_hosted_network_reason_unspecified,
  wlan_hosted_network_reason_bad_parameters,
  wlan_hosted_network_reason_service_shutting_down,
  wlan_hosted_network_reason_insufficient_resources,
  wlan_hosted_network_reason_elevation_required,
  wlan_hosted_network_reason_read_only,
  wlan_hosted_network_reason_persistence_failed,
  wlan_hosted_network_reason_crypt_error,
  wlan_hosted_network_reason_impersonation,
  wlan_hosted_network_reason_stop_before_start,
  wlan_hosted_network_reason_interface_available,
  wlan_hosted_network_reason_interface_unavailable,
  wlan_hosted_network_reason_miniport_stopped,
  wlan_hosted_network_reason_miniport_started,
  wlan_hosted_network_reason_incompatible_connection_started,
  wlan_hosted_network_reason_incompatible_connection_stopped,
  wlan_hosted_network_reason_user_action,
  wlan_hosted_network_reason_client_abort,
  wlan_hosted_network_reason_ap_start_failed,
  wlan_hosted_network_reason_peer_arrived,
  wlan_hosted_network_reason_peer_departed,
  wlan_hosted_network_reason_peer_timeout,
  wlan_hosted_network_reason_gp_denied,
  wlan_hosted_network_reason_service_unavailable,
  wlan_hosted_network_reason_device_change,
  wlan_hosted_network_reason_properties_change,
  wlan_hosted_network_reason_virtual_station_blocking_use,
  wlan_hosted_network_reason_service_available_on_virtual_station,
  v1_enum
}
enum WLAN_HOSTED_NETWORK_OPCODE {
  wlan_hosted_network_opcode_connection_settings,
  wlan_hosted_network_opcode_security_settings,
  wlan_hosted_network_opcode_station_profile,
  wlan_hosted_network_opcode_enable,
  v1_enum
}

[StructLayout(LayoutKind.Sequential)]
unsafe struct WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS {
    public uint uSSIDLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] ucSSID;
    public DWORD      dwMaxNumberOfPeers;
}

// [StructLayout(LayoutKind.Sequential)]
// unsafe struct _WLAN_HOSTED_NETWORK_SECURITY_SETTINGS {
//   DOT11_AUTH_ALGORITHM   dot11AuthAlgo;
//   DOT11_CIPHER_ALGORITHM dot11CipherAlgo;
// }

public unsafe class NativeWiFi : IWlanRouter {
    DWORD clientHandle = 0;

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanOpenHandle(
    DWORD   dwClientVersion,
    void*   pReserved,
    int*  pdwNegotiatedVersion,
    HANDLE* phClientHandle
    );

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanCloseHandle(
    HANDLE hClientHandle,
    void*  pReserved
    );

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkForceStart(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkForceStop(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkInitSettings(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );

    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkQueryProperty(
    HANDLE                     hClientHandle,
    WLAN_HOSTED_NETWORK_OPCODE OpCode,
    int*                     pdwDataSize,
    void*                      *ppvData,
    WLAN_OPCODE_VALUE_TYPE*    pWlanOpcodeValueType,
    void*                      pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkQuerySecondaryKey(
    HANDLE                      hClientHandle,
    int*                      pdwKeyLength,
    byte*                      *ppucKeyData,
    bool*                       pbIsPassPhrase,
    bool*                       pbPersistent,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkQueryStatus(
    HANDLE                      hClientHandle,
    void**                      ppWlanHostedNetworkStatus,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkRefreshSecuritySettings(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkSetProperty(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_OPCODE  OpCode,
    DWORD                       dwDataSize,
    void*                       pvData,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkSetSecondaryKey(
    HANDLE                      hClientHandle,
    DWORD                       dwKeyLength,
    byte*                      pucKeyData,
    BOOL                        bIsPassPhrase,
    BOOL                        bPersistent,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkStartUsing(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );
    [DllImport("Wlanapi.dll")]
    static extern DWORD WlanHostedNetworkStopUsing(
    HANDLE                      hClientHandle,
    WLAN_HOSTED_NETWORK_REASON* pFailReason,
    void*                       pvReserved
    );

    [DllImport("Wlanapi.dll")]
    static extern void WlanFreeMemory(
        void* pMemory
    );

    // Encoding oemencoding = Encoding.GetEncoding(850/* Convert.ToInt32(Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Nls\\CodePage", "OEMCP", 437)) */);

    // [DllImport("Kernel32.dll")]
    // static extern UInt32 GetConsoleOutputCP();

    public NativeWiFi() {
        DWORD version = 0;
        DWORD error;
        unsafe {
            var handle = clientHandle;            
            error = WlanOpenHandle(2, null, &version, &handle);
            clientHandle = handle;
        }
        if(error != 0) {
            clientHandle = 0;
            throw new Exception("WlanOpenHandle failed with error (" + error + ")");
        }
    }

    public void Allow() {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            int enable = 1;
            if(WlanHostedNetworkSetProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_enable, 4, &enable, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkSetProperty failed with (" + reason + ")");
            }
        }
    }

    public void InitializeSettings() {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            if(WlanHostedNetworkInitSettings(clientHandle, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkInitSettings failed with (" + reason + ")");
            }
        }
    }

    private WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS NetworkConnectionSettings {
        get {
            unsafe {
                DWORD size = 0;
                void* psettings;
                WLAN_OPCODE_VALUE_TYPE type;
                var res = WlanHostedNetworkQueryProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_connection_settings, &size, &psettings, &type, null);
                if(res != 0) {
                    throw new Exception("WlanHostedNetworkQueryProperty failed with (" + res + ")");
                }
                return Marshal.PtrToStructure<WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>((IntPtr)psettings);
            }
        }
        set {
            unsafe {
                WLAN_HOSTED_NETWORK_REASON reason;
                var size = Marshal.SizeOf<WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>();
                var psettings = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, psettings, false);
                var res = WlanHostedNetworkSetProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_connection_settings, size, (void*)psettings, &reason, null);
                Marshal.FreeHGlobal(psettings);
                if(res != 0) {
                    throw new Exception("WlanHostedNetworkSetProperty failed with (" + res + ":" + reason + ")");
                }
            }
        }
    }

    // private WLAN_HOSTED_NETWORK_S NetworkSecuritySettings {
    //     get {
    //         unsafe {
    //             DWORD size = 0;
    //             void* psettings;
    //             WLAN_OPCODE_VALUE_TYPE type;
    //             var res = WlanHostedNetworkQueryProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_security_settings, &size, &psettings, &type, null);
    //             if(res != 0) {
    //                 throw new Exception("WlanHostedNetworkQueryProperty failed with (" + res + ")");
    //             }
    //             return Marshal.PtrToStructure<WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>((IntPtr)psettings);
    //         }
    //     }
    //     set {
    //         unsafe {
    //             WLAN_HOSTED_NETWORK_REASON reason;
    //             var size = Marshal.SizeOf<WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>();
    //             var psettings = Marshal.AllocHGlobal(size);
    //             Marshal.StructureToPtr(value, psettings, false);
    //             var res = WlanHostedNetworkSetProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_connection_settings, size, (void*)psettings, &reason, null);
    //             Marshal.FreeHGlobal(psettings);
    //             if(res != 0) {
    //                 throw new Exception("WlanHostedNetworkSetProperty failed with (" + res + ":" + reason + ")");
    //             }
    //         }
    //     }
    // } 

    public String SSID {
        get {
            var settings = NetworkConnectionSettings;
            StringBuilder builder = new StringBuilder();
            for (uint i = 0; i < settings.uSSIDLength; i++) {
                builder.Append((char)settings.ucSSID[i]);
            }
            return builder.ToString();
            // throw new Exception(settings.ucSSID[6].ToString());
            // return oemencoding.GetString(settings.ucSSID, 0, (int)settings.uSSIDLength);
        }
        set {
            var settings = NetworkConnectionSettings;
            for (int i = 0; i < value.Length; i++) {
                settings.ucSSID[i] = (byte)value[i];
            }
            settings.uSSIDLength = (uint)value.Length; //(uint)oemencoding.GetBytes(value, 0, Math.Min(value.Length, 32), settings.ucSSID, 0);
            NetworkConnectionSettings = settings;
        }
    }

    public DWORD MaxNumberOfPeers {
        get {
            return NetworkConnectionSettings.dwMaxNumberOfPeers;
        }
        set {
            var settings = NetworkConnectionSettings;
            settings.dwMaxNumberOfPeers = value;
            NetworkConnectionSettings = settings;
        }
    }

    public string Key { get {
        WLAN_HOSTED_NETWORK_REASON reason;
        DWORD dwKeyLength = 0;
        byte * key = null;
        bool isPassPhrase = true;
        bool persisten = true;
        DWORD ret = WlanHostedNetworkQuerySecondaryKey(clientHandle, &dwKeyLength, &key, &isPassPhrase, &persisten, &reason, null);
        if(ret != 0) {
            throw new Exception("WlanHostedNetworkQuerySecondaryKey failed with (" + reason + ")");
        }
        StringBuilder builder = new StringBuilder(dwKeyLength);
        for (int i = 0; i < dwKeyLength; i++) {
            builder.Append((char)key[i]);
        }
        return builder.ToString();
    }
        set {
            unsafe {
                WLAN_HOSTED_NETWORK_REASON reason;
                byte* ptr = (byte*)Marshal.AllocHGlobal(value.Length + 1);
                // IntPtr ptr = Marshal.StringToHGlobalAnsi(value);
                for (int i = 0; i < value.Length; i++) {
                    ptr[i] = (byte)value[i];
                }
                ptr[value.Length] = 0;
                DWORD ret = WlanHostedNetworkSetSecondaryKey(clientHandle, value.Length + 1, ptr/* Encoding.ASCII.GetBytes(value) */, true, true, &reason, null);
                Marshal.FreeHGlobal((IntPtr)ptr);
                if(ret != 0) {
                    throw new Exception("WlanHostedNetworkSetSecondaryKey failed with (" + reason + ")");
                }
            }
        }
    }

    public async Task Start()
    {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            if(WlanHostedNetworkForceStart(clientHandle, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkForceStart failed with (" + reason + ")");
            }
        }
    }

    public bool IsRunning() {
        unsafe {
            void* nstate;
            var code = WlanHostedNetworkQueryStatus(clientHandle, &nstate, null);
            if(code != 0) {
                throw new Exception("WlanHostedNetworkForceStop failed with (" + code + ")");
            }
            var status = Marshal.PtrToStructure<WLAN_HOSTED_NETWORK_STATUS>((IntPtr)nstate);
            WlanFreeMemory(nstate);
            var state = status.HostedNetworkState;
            return state == WLAN_HOSTED_NETWORK_STATE.wlan_hosted_network_active;
        }
    }

    public async Task Stop() {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            if(WlanHostedNetworkForceStop(clientHandle, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkForceStop failed with (" + reason + ")");
            }
        }
    }

    ~NativeWiFi() {
        Stop().RunSynchronously();
        if(clientHandle != 0) {
            unsafe {
                WlanCloseHandle(clientHandle, null);                
            }
        }
    }
}
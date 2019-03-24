using System;
using System.Text;
using System.Runtime.InteropServices;
using DWORD = System.Int32;
using HANDLE = System.Int32;
using BOOL = System.Boolean;
using ULONG = System.UInt64;
enum WLAN_OPCODE_VALUE_TYPE {

}

enum WLAN_HOSTED_NETWORK_STATUS {

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


unsafe struct DOT11_SSID {

}

[StructLayout(LayoutKind.Sequential)]
unsafe struct WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS {
    public uint uSSIDLength;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 32)]
    public byte[] ucSSID;
    public DWORD      dwMaxNumberOfPeers;
}

public unsafe class NativeWiFi {
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
    WLAN_HOSTED_NETWORK_STATUS** ppWlanHostedNetworkStatus,
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

    public String SSID {
        set {
            unsafe {
                WLAN_HOSTED_NETWORK_REASON reason;
                var settings = new WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS();
                var oemencoding = Encoding.GetEncoding(Convert.ToInt32(Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Nls\\CodePage", "OEMCP", 437)));
                settings.ucSSID = new byte[32];
                settings.uSSIDLength = (uint)oemencoding.GetBytes(value, 0, Math.Min(value.Length, 32), settings.ucSSID, 0);
                settings.dwMaxNumberOfPeers = 20;
                var size = Marshal.SizeOf<WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS>();
                var psettings = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(settings, psettings, false);
                var res = WlanHostedNetworkSetProperty(clientHandle, WLAN_HOSTED_NETWORK_OPCODE.wlan_hosted_network_opcode_connection_settings, size, /* (void*)GCHandle.ToIntPtr(handle) */(void*)psettings, &reason, null);
                Marshal.FreeHGlobal(psettings);
                if(res != 0) {
                    throw new Exception("WlanHostedNetworkSetProperty failed with (" + reason + ")");
                }
            }
        }
    }

    public void StartHostedNetwork() {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            if(WlanHostedNetworkForceStart(clientHandle, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkForceStart failed with (" + reason + ")");
            }
        }
    }

    public void StopHostedNetwork() {
        unsafe {
            WLAN_HOSTED_NETWORK_REASON reason;
            if(WlanHostedNetworkForceStop(clientHandle, &reason, null) != 0) {
                throw new Exception("WlanHostedNetworkForceStart failed with (" + reason + ")");
            }
        }
    }

    ~NativeWiFi() {
        if(clientHandle != 0) {
            unsafe
            {
                WlanCloseHandle(clientHandle, null);                
            }
        }
    }
}
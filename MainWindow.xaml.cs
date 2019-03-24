using NETCONLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WlanRouter
{
 
    public partial class MainWindow : Window
    {

        int Starten_btn_state;
        string vbss;

        public MainWindow()
        {
            InitializeComponent();
            Starten_btn_state = 2;
            foreach (var nic in GetAllIPv4Interfaces())
            {
                if (nic.Description.StartsWith("Microsoft"))
                {
                    Starten_btn_state = 1;
                    Starten_btn.Content = "Stoppen";
                }
                else if(Starten_btn_state != 1)
                    Starten_btn_state = 0;
            }
        }

        private static readonly INetSharingManager SharingManager = new NetSharingManager();

        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = System.IO.Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString() + extension;
            return System.IO.Path.Combine(path, fileName);
        }

        private void Delete_tmp_files()
        {
            try
            {
                if (vbss != string.Empty)
                    File.Delete(vbss);
            }
            catch { }
        }

        public static IEnumerable<NetworkInterface> GetAllIPv4Interfaces()
        {
            return
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.Supports(NetworkInterfaceComponent.IPv4)
                where nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                where nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                where nic.OperationalStatus == OperationalStatus.Up
                select nic;
        }

        private void Remove_ICS()
        {
            INetConnection sharedConnection = (from INetConnection c in SharingManager.EnumEveryConnection
                                               where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                                               where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingConnectionType ==
                                                   tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC
                                               select c).DefaultIfEmpty(null).First();
            INetConnection homeConnection = (
                from INetConnection c in SharingManager.EnumEveryConnection
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingConnectionType ==
                    tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE
                select c).DefaultIfEmpty(null).First();
            if (sharedConnection != null)
                SharingManager.get_INetSharingConfigurationForINetConnection(sharedConnection).DisableSharing();
            if (homeConnection != null)
                SharingManager.get_INetSharingConfigurationForINetConnection(homeConnection).DisableSharing();
        }

        private async void Starten_Klick(object sender, RoutedEventArgs e)
        {
            string Share = null;
            string Router = null;
            if (Starten_btn_state == 0)
            {
                try
                {
                    Starten_btn_state = 2;
                    Starten_btn.Content = "Warten";
                    Cursor = Cursors.Wait;
                    Delete_tmp_files();
                    vbss = GetTempFilePathWithExtension(".vbs");
                    File.WriteAllText(vbss, "CreateObject(\"Wscript.Shell\").Run \"cmd /c netsh wlan set hostednetwork mode=allow ssid=\"\"" + SSID.Text + "\"\" key=\"\"" + WPA_PSK.Text + "\"\" & netsh wlan start hostednetwork\", 0, False", Encoding.ASCII);
                    Process.Start("wscript", vbss);
                    await Task.Run(() =>
                    {
                        while (Router == null)
                            foreach (var nic in GetAllIPv4Interfaces())
                            {
                                if (nic.Description.StartsWith("Microsoft "))
                                    Router = nic.Name;
                                else
                                    Share = nic.Name;
                            }
                        Remove_ICS();
                        error1:
                        try
                        {
                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Share select c).DefaultIfEmpty(null).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Router select c).DefaultIfEmpty(null).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                        }
                        catch
                        {
                            Starten_btn_state = 0;
                            goto error1;
                        }
                    });
                    Starten_btn_state = 1;
                    Starten_btn.Content = "Stoppen";
                    Cursor = Cursors.Arrow;
                }
                catch
                {
                    Starten_btn_state = 0;
                    MessageBox.Show("Keine Adminrechte?" + System.Environment.NewLine + "Freigabe: " + Share + System.Environment.NewLine + "Router: " + Router + System.Environment.NewLine + "Internetfreigabe konnte nicht erstellt werden", "Fehler");
                }

            }
            else if (Starten_btn_state == 1)
            {
                try
                {
                    Starten_btn_state = 2;
                    Starten_btn.Content = "Warten";
                    Cursor = Cursors.Wait;

                    Delete_tmp_files();
                    vbss = GetTempFilePathWithExtension(".vbs");
                    File.WriteAllText(vbss, "CreateObject(\"Wscript.Shell\").Run \"netsh wlan stop hostednetwork\", 0, False", Encoding.ASCII);
                    Process.Start("wscript", vbss);
                    await Task.Run(() =>
                    {
                        bool Router_b = true;
                        while (Router_b)
                        {
                            Router_b = false;
                            foreach (var nic in GetAllIPv4Interfaces())
                            {
                                if (nic.Description.StartsWith("Microsoft"))
                                {
                                    Router_b = true;
                                }
                                else if (Router_b != true)
                                    Router_b = false;
                            }
                        }
                        Remove_ICS();
                    });
                    Starten_btn.Content = "Starten";
                    Starten_btn_state = 0;
                    Cursor = Cursors.Arrow;
                }
                catch
                {
                    Starten_btn_state = 1;
                    MessageBox.Show("Keine Adminrechte?" + System.Environment.NewLine + "Internetfreigabe konnte nicht entfernt werden", "Fehler");
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Delete_tmp_files();
        }
    }
}

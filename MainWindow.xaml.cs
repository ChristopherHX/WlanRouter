using NETCONLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WlanRouter
{
    public partial class MainWindow : Window
    {
        private static readonly INetSharingManager SharingManager = new NetSharingManager();
        bool control_btn_state = false;
        string vbs;

        public MainWindow()
        {
            InitializeComponent();
            SSID_tbox.Text = Properties.Settings.Default.SSID;
            key_pbox.Password = Properties.Settings.Default.Passwort;
            key_pbox.PasswordChanged += (x, y) => key_pbox_PasswordChanged();
            key_tbox.TextChanged += (x, y) => key_pbox_PasswordChanged();
            SSID_tbox.TextChanged += (x, y) => key_pbox_PasswordChanged();
            key_b.Click += (x, y) => key_b_Klick();
            Inet_share_com.Items.Add(new ComboBoxItem { Content = "Keine Internetfreigabe" });
            try 
            {
                foreach (var nic in GetAllIPv4Interfaces())
                {
                    if (nic.Description.StartsWith("Microsoft"))
                    {
                        control_btn_change(1, false);
                    }
                    else
                    {
                        Inet_share_com.Items.Add(new ComboBoxItem { Content = nic.Name });
                    }
                }
                INetConnection sharedConnection = (
                    from INetConnection c in SharingManager.EnumEveryConnection
                    where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                    where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC
                    select c).DefaultIfEmpty(null).First();
                Inet_share_com.SelectedValue = (sharedConnection != null) ? SharingManager.get_NetConnectionProps(sharedConnection).Name : "Keine Internetfreigabe";
                key_pbox_PasswordChanged();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Exception", MessageBoxButton.OK);
            }
            control_btn_change(2, true);
        }

        private void key_pbox_PasswordChanged()
        {
            Brush b1 = new SolidColorBrush(Colors.White);
            Brush b2 = new VisualBrush(new Label { Content = "Name (SSID)", Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Bottom };
            Brush b3 = new VisualBrush(new Label { Content = "Passwort (min 8 Zeichen)", Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Bottom };
            Brush b4 = new VisualBrush(new Label { Content = "(noch min " + Convert.ToString(8 - (key_pbox.IsVisible ? key_pbox.Password.Length : key_tbox.Text.Length)) + " Zeichen)", Foreground = new SolidColorBrush(Colors.Red) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Right, AlignmentY = AlignmentY.Bottom };
            SSID_tbox.Background = (SSID_tbox.Text.Length == 0) ? b2 : b1;
            key_pbox.Background = (key_pbox.Password.Length == 0) ? b3 : ((key_pbox.Password.Length < 8) ? b4 : b1);
            key_tbox.Background = (key_tbox.Text.Length == 0) ? b3 : ((key_tbox.Text.Length < 8) ? b4 : b1);
            control_btn.IsEnabled = !((SSID_tbox.Text.Length == 0 || (key_pbox.IsVisible ? key_pbox.Password.Length : key_tbox.Text.Length) < 8) && !control_btn_state);
        }

        private static string GetTempFilePathWithExtension(string extension)
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + extension);
        }

        private void Delete_tmp_files()
        {
            if (vbs != null)
                File.Delete(vbs);
            vbs = null;
        }

        private static IEnumerable<NetworkInterface> GetAllIPv4Interfaces()
        {
            return from nic in NetworkInterface.GetAllNetworkInterfaces()
                   where nic.Supports(NetworkInterfaceComponent.IPv4)
                   where nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                   where nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                   where nic.OperationalStatus == OperationalStatus.Up
                   select nic;
        }

        private void Remove_ICS()
        {
            INetConnection sharedConnection = (
                from INetConnection c in SharingManager.EnumEveryConnection
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC
                select c).DefaultIfEmpty(null).First();
            INetConnection homeConnection = (
                from INetConnection c in SharingManager.EnumEveryConnection
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE
                select c).DefaultIfEmpty(null).First();
            if (sharedConnection != null)
                SharingManager.get_INetSharingConfigurationForINetConnection(sharedConnection).DisableSharing();
            if (homeConnection != null)
                SharingManager.get_INetSharingConfigurationForINetConnection(homeConnection).DisableSharing();
        }

        private void vbs_host(string command)
        {
            Delete_tmp_files();
            vbs = GetTempFilePathWithExtension(".vbs");
            File.WriteAllText(vbs, "CreateObject(\"Wscript.Shell\").Run \"" + command + "\", 0, False", Encoding.ASCII);
            Process.Start("wscript", vbs);
        }


        private void control_btn_Klick(object sender, RoutedEventArgs e)
        {
            control_btn_change(0, true);
            string Share = null;
            string Router = null;
            int zw;
            DispatcherTimer timer;
            switch (control_btn_state)
            {
                case false:
                    vbs_host("cmd /c netsh wlan set hostednetwork mode=allow ssid=\"\"" + SSID_tbox.Text + "\"\" key=\"\"" + key_pbox.Password + "\"\" & netsh wlan start hostednetwork");
                    timer = new DispatcherTimer(DispatcherPriority.Background);
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    zw = 0;
                    timer.Tick += (x, y) =>
                    {
                        try
                        {
                            string[] test = (from nic in GetAllIPv4Interfaces() where nic.Description.StartsWith("Microsoft") select nic.Name).ToArray();
                            if (test.Length != 0)
                            {
                                timer.Stop();
                                Delete_tmp_files();
                                Router = test[0];
                                if (Inet_share_com.SelectedIndex != 0)
                                {
                                    Share = Inet_share_com.SelectedValue.ToString();
                                    while (true)
                                    {
                                        try
                                        {
                                            Remove_ICS();
                                            break;
                                        }
                                        catch
                                        {
                                            if (MessageBox.Show("Entfernen von der Internetfreigabe fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                                throw new Exception("Fehler"); ;
                                        }
                                    }
                                    while (true)
                                    {
                                        try
                                        {
                                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Share select c).DefaultIfEmpty(null).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
                                            break;
                                        }
                                        catch
                                        {
                                            if (MessageBox.Show("Erstellen von der Internetfreigabe fehlgeschlagen (Freigabe Netzwerk)" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                                throw new Exception("Fehler");
                                        }
                                    }
                                    while (true)
                                    {
                                        try
                                        {
                                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Router select c).DefaultIfEmpty(null).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                                            break;
                                        }
                                        catch
                                        {

                                            if (MessageBox.Show("Erstellen von der Internetfreigabe fehlgeschlagen (Einrichtung Router)" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                                throw new Exception("Fehler");
                                        }
                                    }
                                }
                                else
                                {
                                    Remove_ICS();
                                }
                                control_btn_change(1, true);
                            }
                            else if (zw > 20)
                            {
                                timer.Stop();
                                if (MessageBox.Show("Starten des Routers fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                {
                                    throw new Exception("Fehler");
                                }
                                zw = 0;
                                vbs_host("cmd /c netsh wlan set hostednetwork mode=allow ssid=\"\"" + SSID_tbox.Text + "\"\" key=\"\"" + key_pbox.Password + "\"\" & netsh wlan start hostednetwork");
                                timer.Start();
                            }
                            zw++;
                        }
                        catch {
                            timer.Stop();
                            control_btn_change(2, true);
                        }
                    };
                    timer.Start();
                    break;
                case true:
                    vbs_host("netsh wlan stop hostednetwork");
                    timer = new DispatcherTimer(DispatcherPriority.Background);
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    zw = 0;
                    timer.Tick += (x, y) =>
                    {
                        try
                        {
                            if ((from nic in GetAllIPv4Interfaces() where nic.Description.StartsWith("Microsoft") select nic.Name).ToArray().Length == 0)
                            {
                                timer.Stop();
                                Delete_tmp_files();
                                control_btn_change(2, true);
                            }
                            else if (zw > 20)
                            {
                                timer.Stop();
                                if (MessageBox.Show("Stoppen des Routers fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                {
                                    zw = 0;
                                    vbs_host("netsh wlan stop hostednetwork");
                                    timer.Start();
                                }
                                else
                                    throw new Exception("Fehler");
                            }
                            zw++;
                        }
                        catch {
                            timer.Stop();
                            control_btn_change(1, true);
                        }
                    };
                    timer.Start();
                    break;
            }
        }

        private void control_btn_change(int state, bool mode)
        {
            switch (state)
            {
                case 0:
                    if (mode)
                    {
                        Cursor = Cursors.Wait;
                        control_btn.IsEnabled = false;
                        control_btn.Content = "Warten";
                        SSID_tbox.IsEnabled = false;
                        key_b.IsEnabled = false;
                        key_tbox.IsEnabled = false;
                        if (key_tbox.IsVisible)
                            key_b_Klick();
                    }
                    Inet_share_com.IsEnabled = false;
                    key_pbox.IsEnabled = false;
                    break;
                case 1:
                    control_btn_state = true;
                    control_btn.Content = "Stoppen";
                    SSID_tbox.IsReadOnly = true;
                    key_tbox.IsReadOnly = true;
                    if (mode)
                    {
                        key_b.IsEnabled = true;
                        key_tbox.IsEnabled = true;
                        SSID_tbox.IsEnabled = true;
                        control_btn.IsEnabled = true;
                        Cursor = Cursors.Arrow;
                    }
                    break;
                case 2:
                    
                    control_btn_state = false;
                    control_btn.Content = "Starten";
                    SSID_tbox.IsReadOnly = false;
                    key_tbox.IsReadOnly = false;
                    if (mode)
                    {
                        Inet_share_com.IsEnabled = true;
                        key_b.IsEnabled = true;
                        SSID_tbox.IsEnabled = true;
                        key_tbox.IsEnabled = true;
                        key_pbox.IsEnabled = true;
                        control_btn.IsEnabled = true;
                        Cursor = Cursors.Arrow;
                    }
                    break;
            }
        }

        private void key_b_Klick()
        {
            switch (Convert.ToString(key_b.Content))
            {
                case "●●●":
                    key_b.Content = "abc";
                    key_pbox.Password = key_tbox.Text;
                    key_tbox.Visibility = Visibility.Collapsed;
                    key_pbox.Visibility = Visibility.Visible;
                    break;
                case "abc":
                    key_b.Content = "●●●";
                    key_tbox.Text = key_pbox.Password;
                    key_pbox.Visibility = Visibility.Collapsed;
                    key_tbox.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SSID = SSID_tbox.Text;
            Properties.Settings.Default.Passwort = (key_tbox.IsVisible) ? key_tbox.Text : key_pbox.Password;
            Properties.Settings.Default.Save();
        }
    }
}

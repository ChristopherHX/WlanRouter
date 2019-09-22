using NETCONLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;

namespace WlanRouter
{
    public partial class MainWindow : Window
    {
        private static string TITLE = "Wlan Router";
        private static string FAILED = "Failed";
        private static string UNSUPPORTED = "Unsupported";
        private static string NO_INTERNET_SHARING = "Keine Internetfreigabe";
        private static string FAILDED_TO_STOP = "Failed to stop hostednetwork";
        private static string START_WLAN_ROUTER = "Wlan Router starten";
        private static string WAIT = "Warten";
        private static string STOP_WLAN_ROUTER = "Wlan Router stoppen";
        private static string SSID_HINT = "Name (SSID) des Wlan Routers";
        private static string KEY_HINT = "Passwort (WPA2PSK) des Wlan Routers";
        private static string KEY_HINT_FORMAT = "(noch min {0} Zeichen)";
        private IWlanRouter router;
        private bool ctrl_key_state = true;
        private Thread background;
        private Dispatcher background_dispatcher;

        public MainWindow()
        {
            InitializeComponent();
            Title = TITLE;
            var uithread = Dispatcher;
            background = new Thread(new ThreadStart(() => {
                try
                {
                    background_dispatcher = Dispatcher.CurrentDispatcher;
                    router = (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1) ? (IWlanRouter)new WiFiDirect() : new NativeWiFi();
                    var ssid = router.SSID;
                    var key = router.Key;
                    uithread.InvokeAsync(() => {
                        ssid_box.Text = ssid;
                        password_box.Password = key;
                    }, DispatcherPriority.Input);
                }
                catch (Exception ex)
                {
                    Dispatcher.InvokeAsync(() => {
                        MessageBox.Show(UNSUPPORTED + Environment.NewLine + ex.Message, FAILED, MessageBoxButton.OK);
                        System.Windows.Application.Current.Shutdown();
                    }, DispatcherPriority.Normal);
                }
                Dispatcher.Run();
            }));
            background.Start();
            try
            {
                // Use System Router Icon
                Icon = NativeIcon.ExtractIcon("DDORes.dll", -2378);
            }
            catch { /* Symbol not found (no requirement to run) */ }
            ctrl_key.Click += (sender, e) => ctrl_button_click();
            ssid_box.TextChanged += (sender, e) => input_changed();
            password_box.PasswordChanged += (sender, e) => input_changed();
            password_cleartext_box.TextChanged += (sender, e) => input_changed();
            password_cleartext_switch.Click += (sender, e) => password_show_hide_click();
            refresh();
            control_btn_change(2, true);            
        }

        private void refresh()
        {
            try
            {
                internet_sharing_box.Items.Clear();
                internet_sharing_box.Items.Add(new ComboBoxItem { Content = NO_INTERNET_SHARING });
                internet_sharing_box.SelectedIndex = 0;
                INetSharingManager sharingManager = new NetSharingManager();
                foreach (var con in from INetConnection c in sharingManager.EnumEveryConnection
                                    where sharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                                    select c)
                {
                    if (sharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft "))
                    {
                        string con_name = sharingManager.NetConnectionProps[con].Name + System.Environment.NewLine + sharingManager.NetConnectionProps[con].DeviceName;
                        internet_sharing_box.Items.Add(new ComboBoxItem { Content = con_name });
                        control_btn_change(1, true);
                    }
                    else
                    {
                        string con_name = sharingManager.NetConnectionProps[con].Name + System.Environment.NewLine + sharingManager.NetConnectionProps[con].DeviceName;
                        internet_sharing_box.Items.Add(new ComboBoxItem { Content = con_name });
                        if (sharingManager.INetSharingConfigurationForINetConnection[con].SharingEnabled && sharingManager.INetSharingConfigurationForINetConnection[con].SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC)
                            internet_sharing_box.SelectedValue = con_name;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Exception", MessageBoxButton.OK);
            }
        }

        private void DisableSharing()
        {
            if(router.IsRunning()) {
                INetSharingManager sharingManager = new NetSharingManager();
                foreach (var con in from INetSharingConfiguration conf in from INetConnection c in sharingManager.EnumEveryConnection
                                    select sharingManager.INetSharingConfigurationForINetConnection[c]
                                    where conf.SharingEnabled
                                    select conf)
                {
                    con.DisableSharing();
                }

                var scope = new ManagementScope("root\\Microsoft\\HomeNet");
                scope.Connect();

                foreach (var type in new string[] { "HNet_ConnectionProperties", "HNet_Connection" })
                {
                    var query = new ObjectQuery("SELECT * FROM " + type);
                    var srchr = new ManagementObjectSearcher(scope, query);
                    foreach (ManagementObject entry in srchr.Get())
                    {
                        entry.Dispose();
                        entry.Delete();
                    }
                }
                {
                    var options = new PutOptions();
                    options.Type = PutType.UpdateOnly;

                    var query = new ObjectQuery("SELECT * FROM HNet_ConnectionProperties");
                    var srchr = new ManagementObjectSearcher(scope, query);
                    foreach (ManagementObject entry in srchr.Get())
                    {
                        if ((bool)entry["IsIcsPrivate"])
                            entry["IsIcsPrivate"] = false;
                        if ((bool)entry["IsIcsPublic"])
                            entry["IsIcsPublic"] = false;
                        entry.Put(options);
                    }
                }
            }
        }

        private async void ctrl_button_click()
        {
            control_btn_change(0, true);
            var ssid = ssid_box.Text;
            var key = password_box.Password;
            var share = internet_sharing_box.SelectedIndex != 0 ? internet_sharing_box.SelectedValue.ToString().Split(Environment.NewLine.ToArray(), StringSplitOptions.None).Last() : null;
            await background_dispatcher.InvokeAsync(async () =>
            {
                switch (ctrl_key_state)
                {
                    case true:
                        try
                        {
                            router.SSID = ssid;
                            router.Key = key;
                            router.Start();
                            DisableSharing();
                            INetSharingManager sharingManager = new NetSharingManager();
                            INetConnection Router = (from INetConnection con in sharingManager.EnumEveryConnection
                                                     where sharingManager.NetConnectionProps[con].Status == tagNETCON_STATUS.NCS_CONNECTED
                                                     where sharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft ")
                                                     select con).First();
                            if(share != null)
                            {
                                INetConnection Freigabe = (from INetConnection con in sharingManager.EnumEveryConnection where sharingManager.get_NetConnectionProps(con).DeviceName == share select con).First();                            
                                sharingManager.INetSharingConfigurationForINetConnection[Freigabe].EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
                                sharingManager.INetSharingConfigurationForINetConnection[Router].EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.InvokeAsync(() => {
                                MessageBox.Show(ex.Message, FAILED, MessageBoxButton.OK);
                                control_btn_change(2, true);
                            }, DispatcherPriority.Normal);
                        }
                        await Dispatcher.InvokeAsync(refresh, DispatcherPriority.Normal);
                        break;
                    case false:
                        try
                        {
                            DisableSharing();
                            router.Stop();
                            await Dispatcher.InvokeAsync(() => {
                                refresh();
                                control_btn_change(2, true);
                            }, DispatcherPriority.Normal);
                        }
                        catch(Exception ex)
                        {
                            await Dispatcher.InvokeAsync(() => {
                                MessageBox.Show(FAILDED_TO_STOP + System.Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK);
                                control_btn_change(1, true);
                            }, DispatcherPriority.Normal);
                        }
                        break;
                }
            },  DispatcherPriority.Normal);
        }

        private void control_btn_change(int state, bool mode)
        {
            switch (state)
            {
                case 0:
                    Cursor = Cursors.Wait;
                    ctrl_key.IsEnabled = false;
                    ctrl_key.Content = WAIT;
                    ssid_box.IsEnabled = false;
                    password_cleartext_switch.IsEnabled = false;
                    password_cleartext_box.IsEnabled = false;
                    if (password_cleartext_box.IsVisible)
                        password_show_hide_click();
                    internet_sharing_box.IsEnabled = false;
                    password_box.IsEnabled = false;
                    break;
                case 1:
                    ctrl_key_state = false;
                    ctrl_key.Content = STOP_WLAN_ROUTER;
                    ssid_box.IsReadOnly = true;
                    password_cleartext_box.IsReadOnly = true;
                    if (mode)
                    {
                        password_cleartext_switch.IsEnabled = true;
                        password_cleartext_box.IsEnabled = true;
                        ssid_box.IsEnabled = true;
                        ctrl_key.IsEnabled = true;
                        Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        internet_sharing_box.IsEnabled = false;
                        password_box.IsEnabled = false;
                    }
                    break;
                case 2:
                    ctrl_key_state = true;
                    ctrl_key.Content = START_WLAN_ROUTER;
                    ssid_box.IsReadOnly = false;
                    password_cleartext_box.IsReadOnly = false;
                    internet_sharing_box.IsEnabled = true;
                    password_cleartext_switch.IsEnabled = true;
                    ssid_box.IsEnabled = true;
                    password_cleartext_box.IsEnabled = true;
                    password_box.IsEnabled = true;
                    ctrl_key.IsEnabled = true;
                    Cursor = Cursors.Arrow;
                    break;
            }
        }

        private void input_changed()
        {
            Brush Brush_None = new SolidColorBrush();
            Brush ssid_box_hint = new VisualBrush(new Label() { Content = SSID_HINT, Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.0125, 0, 1, 1) };
            Brush Passwort_hint = new VisualBrush(new Label() { Content = KEY_HINT, Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.0125, 0, 1, 1) };
            Brush Passwort_Info = new VisualBrush(new Label() { Content = String.Format(KEY_HINT_FORMAT, Convert.ToString(8 - (password_box.Visibility == Visibility.Visible && password_cleartext_box.Visibility != Visibility.Visible ? password_box.Password.Length : password_cleartext_box.Text.Length))), Foreground = new SolidColorBrush(Colors.Red) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Right, Viewbox = new Rect(0, 0.05, 1, 1) };
            ssid_box.Background = ssid_box.Text.Length == 0 ? ssid_box_hint : Brush_None;
            password_box.Background = password_box.Visibility == Visibility.Visible && password_box.Password.Length < 8 ? password_box.Password.Length == 0 ? Passwort_hint : Passwort_Info : Brush_None;
            password_cleartext_box.Background = password_cleartext_box.Visibility == Visibility.Visible && password_cleartext_box.Text.Length < 8 ? password_cleartext_box.Text.Length == 0 ? Passwort_hint : Passwort_Info : Brush_None;
            ctrl_key.IsEnabled = ctrl_key_state == true ? ((password_box.Visibility == Visibility.Visible && password_cleartext_box.Visibility != Visibility.Visible ? password_box.Password.Length >= 8 : password_cleartext_box.Text.Length >= 8) && ssid_box.Text.Length != 0) : true;
        }

        private void password_show_hide_click()
        {
            switch (Convert.ToString(password_cleartext_switch.Content))
            {
                case "●●●":
                    password_cleartext_switch.Content = "abc";
                    password_box.Password = password_cleartext_box.Text;
                    password_cleartext_box.Visibility = Visibility.Collapsed;
                    password_box.Visibility = Visibility.Visible;
                    break;
                case "abc":
                    password_cleartext_switch.Content = "●●●";
                    password_cleartext_box.Text = password_box.Password;
                    password_box.Visibility = Visibility.Collapsed;
                    password_cleartext_box.Visibility = Visibility.Visible;
                    break;
            }
            input_changed();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ssid = ssid_box.Text;
            var key = password_box.Password;
            background_dispatcher.InvokeAsync(() => {
                router.SSID = ssid;
                router.Key = key;
                DisableSharing();
                background_dispatcher.InvokeShutdown();
            }, DispatcherPriority.Normal);
        }
    }
}

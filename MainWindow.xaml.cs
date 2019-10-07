using NETCONLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;
using Microsoft.Win32;

namespace WlanRouter {
    public partial class MainWindow : Window {
        private static string TITLE = "Router";
        private static string FAILED = "Failed";
        private static string UNSUPPORTED = "Unsupported";
        private static string NO_INTERNET_SHARING = "Keine Internetfreigabe";
        private static string FAILDED_TO_STOP = "Failed to stop hostednetwork";
        private static string START_WLAN_ROUTER = "Router starten";
        private static string WAIT = "Warten";
        private static string STOP_WLAN_ROUTER = "Wlan Router stoppen";
        private static string SSID_HINT = "Name (SSID) des Wlan Routers";
        private static string KEY_HINT = "Passwort (WPA2PSK) des Wlan Routers";
        private static string KEY_HINT_FORMAT = "(noch min {0} Zeichen)";
        private IRouter router;
        private bool ctrl_key_state = true;
        private Thread background;
        private Dispatcher background_dispatcher;
        private RegistryKey sharedaccess;
        private RegistryKey tcpip;

        public MainWindow() {
            InitializeComponent();
            Title = TITLE;
            var uithread = Dispatcher;
            background = new Thread(new ThreadStart(() => {
                try {
                    background_dispatcher = Dispatcher.CurrentDispatcher;
                    IList<Object> items = new List<Object>();
                    try {
                        items.Add(new RoutingProvider { Content = "NetSh", Router = new NetSh() });
                        items.Add(new RoutingProvider { Content = "NativeWiFi", Router = new NativeWiFi() });
                        items.Add(new RoutingProvider { Content = "WiFiDirect", Router = new WiFiDirect() });
                        items.Add(new RoutingProvider { Content = "NetworkOperatorTetheringProvider", Router = new NetworkOperatorTetheringManager() });
                    } catch {

                    }
                    
                    // Router = (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1) ? (IWlanRouter)new WiFiDirect() : new NativeWiFi();
                    // var ssid = Router.SSID;
                    // var key = Router.Key;
                    uithread.InvokeAsync(() => {
                    //     ssid_box.Text = ssid;
                    //     password_box.Password = key;
                        foreach(var item in items) {
                            router_box.Items.Add(item);
                        }
                    }, DispatcherPriority.Input);
                }
                catch (Exception ex) {
                    Dispatcher.InvokeAsync(() => {
                        MessageBox.Show(UNSUPPORTED + Environment.NewLine + ex.Message, FAILED, MessageBoxButton.OK);
                        System.Windows.Application.Current.Shutdown();
                    }, DispatcherPriority.Normal);
                }
                Dispatcher.Run();
            }));
            background.Start();
            try {
                // Use System Router Icon
                Icon = NativeIcon.ExtractIcon("DDORes.dll", -2378);
            }
            catch { /* Symbol not found (no requirement to run) */ }
            ctrl_key.Click += (sender, e) => ctrl_button_click();
            ssid_box.TextChanged += (sender, e) => input_changed();
            password_box.PasswordChanged += (sender, e) => input_changed();
            password_cleartext_box.TextChanged += (sender, e) => input_changed();
            password_cleartext_switch.Click += (sender, e) => password_show_hide_click();
            sharedaccess = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters", true);
            RouterIP.Text = sharedaccess.GetValue("ScopeAddress", "192.168.137.1").ToString();
            tcpip = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", true);
            Domain.Text = tcpip.GetValue("ICSDomain").ToString();
            router_box.SelectionChanged += (sender, e) => {
                router = (router_box.SelectedItem as RoutingProvider)?.Router;
                refresh();
            };
            internet_sharing_box.SelectionChanged += (sender, e) => {
                var inetrouter = router as INetRouter;
                if(inetrouter != null) {
                    var selected = internet_sharing_box.SelectedValue as string;
                    if(selected != null) {
                        inetrouter.SetConnection(selected);
                    }
                }
            };
        }

        private void refresh() {
            try {
                internet_sharing_box.Items.Clear();
                var inetrouter = router as INetRouter;
                if(inetrouter != null) {
                    foreach (var item in inetrouter.GetConnections()) {
                        internet_sharing_box.Items.Add(new ComboBoxItem { Content = item });
                    }
                } else {
                    internet_sharing_box.Items.Add(new ComboBoxItem { Content = NO_INTERNET_SHARING });
                }
                internet_sharing_box.SelectedIndex = 0;
                control_btn_change(router?.IsRunning() ?? false ? 1 : 2);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Exception", MessageBoxButton.OK);
            }
        }

        private async void ctrl_button_click() {
            control_btn_change(0);
            var ssid = ssid_box.Text;
            var key = password_box.Password;
            var share = internet_sharing_box.SelectedIndex != 0 ? internet_sharing_box.SelectedValue.ToString().Split(Environment.NewLine.ToArray(), StringSplitOptions.None).Last() : null;
            sharedaccess.SetValue("ScopeAddress", RouterIP.Text);
            tcpip.SetValue("ICSDomain", Domain.Text);
            await background_dispatcher.InvokeAsync(async () => {
                switch (ctrl_key_state) {
                    case true:
                        try {
                            var wrouter = router as IWlanRouter;
                            if(wrouter != null) {
                                wrouter.SSID = ssid;
                                wrouter.Key = key;
                            }
                            await router.Start();
                        }
                        catch (Exception ex) {
                            await Dispatcher.InvokeAsync(() => {
                                MessageBox.Show(ex.Message, FAILED, MessageBoxButton.OK);
                                control_btn_change(2);
                            }, DispatcherPriority.Normal);
                        }
                        await Dispatcher.InvokeAsync(refresh, DispatcherPriority.Normal);
                        break;
                    case false:
                        try {
                            await router.Stop();
                            await Dispatcher.InvokeAsync(() => {
                                refresh();
                                control_btn_change(2);
                            }, DispatcherPriority.Normal);
                        }
                        catch(Exception ex) {
                            await Dispatcher.InvokeAsync(() => {
                                MessageBox.Show(FAILDED_TO_STOP + System.Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK);
                                control_btn_change(1);
                            }, DispatcherPriority.Normal);
                        }
                        break;
                }
            },  DispatcherPriority.Normal);
        }

        private void control_btn_change(int state) {
            switch (state) {
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
                    RouterIP.IsEnabled = false;
                    Domain.IsEnabled = false;
                    break;
                case 1:
                    ctrl_key_state = false;
                    ctrl_key.Content = STOP_WLAN_ROUTER;
                    ssid_box.IsReadOnly = true;
                    password_cleartext_box.IsReadOnly = true;
                    internet_sharing_box.IsEnabled = false;
                    password_box.IsEnabled = false;
                    password_cleartext_switch.IsEnabled = true;
                    password_cleartext_box.IsEnabled = false;
                    ssid_box.IsEnabled = false;
                    ctrl_key.IsEnabled = true;
                    RouterIP.IsEnabled = false;
                    Domain.IsEnabled = false;
                    Cursor = Cursors.Arrow;
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
                    RouterIP.IsEnabled = true;
                    Domain.IsEnabled = true;
                    ctrl_key.IsEnabled = true;
                    Cursor = Cursors.Arrow;
                    break;
            }
        }

        private void input_changed() {
            Brush default_background = new SolidColorBrush();
            Brush ssid_placeholder = new VisualBrush(new Label() { Content = SSID_HINT, Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.0125, 0, 1, 1) };
            Brush password_placeholder = new VisualBrush(new Label() { Content = KEY_HINT, Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.0125, 0, 1, 1) };
            Brush password_hint = new VisualBrush(new Label() { Content = String.Format(KEY_HINT_FORMAT, Convert.ToString(8 - (password_box.Visibility == Visibility.Visible && password_cleartext_box.Visibility != Visibility.Visible ? password_box.Password.Length : password_cleartext_box.Text.Length))), Foreground = new SolidColorBrush(Colors.Red) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Right, Viewbox = new Rect(0, 0.05, 1, 1) };
            ssid_box.Background = ssid_box.Text.Length == 0 ? ssid_placeholder : default_background;
            password_box.Background = password_cleartext_box.Background = password_box.Visibility == Visibility.Visible && password_box.Password.Length < 8 ? password_box.Password.Length == 0 ? password_placeholder : password_hint : default_background;
            ctrl_key.IsEnabled = ctrl_key_state == true ? ((password_box.Visibility == Visibility.Visible && password_cleartext_box.Visibility != Visibility.Visible ? password_box.Password.Length >= 8 : password_cleartext_box.Text.Length >= 8) && ssid_box.Text.Length != 0) : true;
        }

        private void password_show_hide_click() {
            switch (Convert.ToString(password_cleartext_switch.Content)) {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            var ssid = ssid_box.Text;
            var key = password_box.Password;
            background_dispatcher.InvokeAsync(() => {
                // Router.SSID = ssid;
                // Router.Key = key;
                background_dispatcher.InvokeShutdown();
            }, DispatcherPriority.Normal);
        }
    }
}

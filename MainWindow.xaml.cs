using NETCONLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        string cmd_tmp;
        bool Steuer_Taste_state = true;
        private static readonly INetSharingManager SharingManager = new NetSharingManager();

        public MainWindow()
        {
            InitializeComponent();
            Name_Textbox.Text = Properties.Settings.Default.Name;
            Passwort_Pbox.Password = Properties.Settings.Default.Password;
            Steuer_Taste.Click += (sender, e) => Steuer_Taste_Klick();
            Name_Textbox.TextChanged += (sender, e) => Eingabe_changed();
            Passwort_Pbox.PasswordChanged += (sender, e) => Eingabe_changed();
            Passwort_Tbox.TextChanged += (sender, e) => Eingabe_changed();
            Passwort_STaste.Click += (sender, e) => Passwort_STaste_Klick();
            try 
            {
                var cons = (from INetConnection c in SharingManager.EnumEveryConnection
                            where SharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                            select c).ToArray();
                if (cons.Length != 0)
                {
                    foreach (var con in cons)
                    {
                        if (SharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft "))
                        control_btn_change(1, false);
                    else
                    {
                            string con_name = SharingManager.NetConnectionProps[con].Name + System.Environment.NewLine + SharingManager.NetConnectionProps[con].DeviceName;
                            Internet_Freigabe_Auswahlbox.Items.Add(new ComboBoxItem { Content = con_name });
                            if (SharingManager.INetSharingConfigurationForINetConnection[con].SharingEnabled)
                                Internet_Freigabe_Auswahlbox.SelectedValue = con_name;
                    }
                }
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Exception", MessageBoxButton.OK);
            }
            Eingabe_changed();
        }

        private void Steuer_Taste_Klick()
        {
            control_btn_change(0, true);
            int d_z;
            DispatcherTimer d_timer;
            string Freigabe = null, Router = null, command;
            switch (Steuer_Taste_state)
            {
                case true:
                    command = "cmd /c netsh wlan set hostednetwork mode=allow ssid=\"\"" + Name_Textbox.Text + "\"\" key=\"\"" + Passwort_Pbox.Password + "\"\" & netsh wlan start hostednetwork";
                    cmd(command);
                    d_timer = new DispatcherTimer(DispatcherPriority.Background);
                    d_timer.Interval = TimeSpan.FromSeconds(0.1);
                    d_z = 0;
                    d_timer.Tick += (x, y) =>
                    {
                        try
                        {
                            d_timer.Stop();
                            string[] test = (from INetConnection con in SharingManager.EnumEveryConnection
                                            where SharingManager.NetConnectionProps[con].Status == tagNETCON_STATUS.NCS_CONNECTED
                                            where SharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft ")
                                            select SharingManager.NetConnectionProps[con].Name).ToArray();
                            if (test.Length != 0)
                            {
                                cmd("del tmp");
                                Router = test.First();
                                if (Internet_Freigabe_Auswahlbox.SelectedIndex != 0)
                                {
                                    Freigabe = Internet_Freigabe_Auswahlbox.SelectedValue.ToString().Split(Environment.NewLine.ToArray(), StringSplitOptions.None).First();
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
                                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Freigabe select c).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
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
                                            SharingManager.get_INetSharingConfigurationForINetConnection((from INetConnection c in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(c).Name == Router select c).First()).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                                            break;
                                        }
                                        catch
                                        {

                                            if (MessageBox.Show("Erstellen von der Internetfreigabe fehlgeschlagen (Einrichtung Router)" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                                throw new Exception("Fehler");
                                        }
                                    }
                                }
                                control_btn_change(1, true);
                            }
                            else if (d_z >= 20)
                            {
                                if (MessageBox.Show("Starten des Routers fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                                {
                                    throw new Exception("Fehler");
                                }
                                d_z = 0;
                                cmd(command);
                                d_timer.Start();
                            }
                            else
                            {
                                d_z++;
                                d_timer.Start();
                            }
                        }
                        catch
                        {
                            cmd("del tmp");
                            control_btn_change(2, true);
                        }
                    };
                    d_timer.Start();
                    break;
                case false:
                    command = "netsh wlan stop hostednetwork";
                    cmd(command);
                    d_timer = new DispatcherTimer(DispatcherPriority.Background);
                    d_timer.Interval = TimeSpan.FromSeconds(0.1);
                    d_z = 0;
                    d_timer.Tick += (x, y) =>
                    {
                        try
                        {
                            d_timer.Stop();
                            string[] test = (from INetConnection con in SharingManager.EnumEveryConnection
                                            where SharingManager.NetConnectionProps[con].Status == tagNETCON_STATUS.NCS_CONNECTED
                                            where SharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft ")
                                            select SharingManager.NetConnectionProps[con].Name).ToArray();

                            if (test.Length == 0)
                            {
                                cmd("del tmp");
                                control_btn_change(2, true);
                            }
                            else if (d_z >= 20)
                            {
                                if (MessageBox.Show("Stoppen des Routers fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                {
                                    d_z = 0;
                                    cmd(command);
                                    d_timer.Start();
                                }
                                else
                                    throw new Exception("Fehler");
                            }
                            else
                            {
                                d_z++;
                                d_timer.Start();
                            }
                        }
                        catch
                        {
                            cmd("del tmp");
                            control_btn_change(1, true);
                        }
                    };
                    try {
                        Remove_ICS();
                    } catch {

                    }
                    d_timer.Start();
                    break;
            }
        }

        private void control_btn_change(int state, bool mode)
        {
            switch (state)
            {
                case 0:
                    Cursor = Cursors.Wait;
                    Steuer_Taste.IsEnabled = false;
                    Steuer_Taste.Content = "Warten";
                    Name_Textbox.IsEnabled = false;
                    Passwort_STaste.IsEnabled = false;
                    Passwort_Tbox.IsEnabled = false;
                    if (Passwort_Tbox.IsVisible)
                        Passwort_STaste_Klick();
                    Internet_Freigabe_Auswahlbox.IsEnabled = false;
                    Passwort_Pbox.IsEnabled = false;
                    break;
                case 1:
                    Steuer_Taste_state = false;
                    Steuer_Taste.Content = "Wlan Router stoppen";
                    Name_Textbox.IsReadOnly = true;
                    Passwort_Tbox.IsReadOnly = true;
                    if (mode)
                    {
                        Passwort_STaste.IsEnabled = true;
                        Passwort_Tbox.IsEnabled = true;
                        Name_Textbox.IsEnabled = true;
                        Steuer_Taste.IsEnabled = true;
                        Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        Internet_Freigabe_Auswahlbox.IsEnabled = false;
                        Passwort_Pbox.IsEnabled = false;
                    }
                    break;
                case 2:
                    
                    Steuer_Taste_state = true;
                    Steuer_Taste.Content = "Wlan Router starten";
                    Name_Textbox.IsReadOnly = false;
                    Passwort_Tbox.IsReadOnly = false;
                    Internet_Freigabe_Auswahlbox.IsEnabled = true;
                    Passwort_STaste.IsEnabled = true;
                    Name_Textbox.IsEnabled = true;
                    Passwort_Tbox.IsEnabled = true;
                    Passwort_Pbox.IsEnabled = true;
                    Steuer_Taste.IsEnabled = true;
                        Cursor = Cursors.Arrow;
                    break;
                    }
            }

        private void Eingabe_changed()
        {
            Brush Brush_None = new SolidColorBrush();
            Brush Name_Textbox_Hint = new VisualBrush(new Label() { Content = "Name (SSID) des Wlan Routers", Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.0175, 0.05, 1, 1) };
            Brush Passwort_Hint = new VisualBrush(new Label() { Content = "Passwort (WPA2PSK) des Wlan Routers", Foreground = new SolidColorBrush(Colors.Gray) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, Viewbox = new Rect(0.015, 0.05, 1, 1) };
            Brush Passwort_Info = new VisualBrush(new Label() { Content = "(noch min " + Convert.ToString(8 - (Passwort_Pbox.Visibility == Visibility.Visible && Passwort_Tbox.Visibility != Visibility.Visible ? Passwort_Pbox.Password.Length : Passwort_Tbox.Text.Length)) + " Zeichen)", Foreground = new SolidColorBrush(Colors.Red) }) { Stretch = Stretch.None, AlignmentX = AlignmentX.Right, Viewbox = new Rect(0, 0.05, 1, 1) };
            Name_Textbox.Background = Name_Textbox.Text.Length == 0 ? Name_Textbox_Hint : Brush_None;
            Passwort_Pbox.Background = Passwort_Pbox.Visibility == Visibility.Visible && Passwort_Pbox.Password.Length < 8 ? Passwort_Pbox.Password.Length == 0 ? Passwort_Hint : Passwort_Info : Brush_None;
            Passwort_Tbox.Background = Passwort_Tbox.Visibility == Visibility.Visible && Passwort_Tbox.Text.Length < 8 ? Passwort_Tbox.Text.Length == 0 ? Passwort_Hint : Passwort_Info : Brush_None;
            Steuer_Taste.IsEnabled = Steuer_Taste_state == true ? ((Passwort_Pbox.Visibility == Visibility.Visible && Passwort_Tbox.Visibility != Visibility.Visible ? Passwort_Pbox.Password.Length >= 8 : Passwort_Tbox.Text.Length >= 8) && Name_Textbox.Text.Length != 0) : true;
        }

        private void Passwort_STaste_Klick()
        {
            switch (Convert.ToString(Passwort_STaste.Content))
            {
                case "●●●":
                    Passwort_STaste.Content = "abc";
                    Passwort_Pbox.Password = Passwort_Tbox.Text;
                    Passwort_Tbox.Visibility = Visibility.Collapsed;
                    Passwort_Pbox.Visibility = Visibility.Visible;
                    break;
                case "abc":
                    Passwort_STaste.Content = "●●●";
                    Passwort_Tbox.Text = Passwort_Pbox.Password;
                    Passwort_Pbox.Visibility = Visibility.Collapsed;
                    Passwort_Tbox.Visibility = Visibility.Visible;
                    break;
            }
            Eingabe_changed();
        }

        private void cmd(string command)
        {
            switch (command)
            {
                case "del tmp":
                    if (cmd_tmp != null)
                    {
                        File.Delete(cmd_tmp);
                        cmd_tmp = null;
                    }
                    break;
                default:
                    cmd_tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".vbs");
                    File.WriteAllText(cmd_tmp, "CreateObject(\"Wscript.Shell\").Run \"" + command + "\", 0, False", Encoding.ASCII);
                    Process.Start("wscript", cmd_tmp);
                    break;
            }
        }

        private void Remove_ICS()
        {
            INetConnection[] cons = (from INetConnection c in SharingManager.EnumEveryConnection
                                    where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                                    select c).ToArray();
            if (cons.Length != 0)
            {
                foreach (var con in cons)
                {
                    SharingManager.get_INetSharingConfigurationForINetConnection(con).DisableSharing();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Name = Name_Textbox.Text;
            Properties.Settings.Default.Password = (Passwort_Tbox.IsVisible) ? Passwort_Tbox.Text : Passwort_Pbox.Password;
            Properties.Settings.Default.Save();
        }
    }
}

using NETCONLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WlanRouter
{
    public partial class MainWindow : Window
    {
        Netzwerkshell netsh;
        bool Steuer_Taste_state = true;
        private static readonly INetSharingManager SharingManager = new NetSharingManager();

        public MainWindow()
        {
            InitializeComponent();
            netsh = new Netzwerkshell();
            Steuer_Taste.Click += (sender, e) => Steuer_Taste_Klick();
            Name_Textbox.TextChanged += (sender, e) => Eingabe_changed();
            Passwort_Pbox.PasswordChanged += (sender, e) => Eingabe_changed();
            Passwort_Tbox.TextChanged += (sender, e) => Eingabe_changed();
            Passwort_STaste.Click += (sender, e) => Passwort_STaste_Klick();
            refresh();
        }

        private void refresh()
        {
            try
            {
                Name_Textbox.Text = netsh.get_ssid();
                Passwort_Pbox.Password = netsh.get_password();
                var cons = (from INetConnection c in SharingManager.EnumEveryConnection
                            where SharingManager.NetConnectionProps[c].Status == tagNETCON_STATUS.NCS_CONNECTED
                            select c).ToArray();
                Internet_Freigabe_Auswahlbox.Items.Clear();
                Internet_Freigabe_Auswahlbox.Items.Add(new ComboBoxItem { Content = "Keine Internetfreigabe" });
                Internet_Freigabe_Auswahlbox.SelectedIndex = 0;
                if (cons.Length != 0)
                {
                    foreach (var con in cons)
                    {
                        if (SharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft "))
                        {
                            string con_name = SharingManager.NetConnectionProps[con].Name + System.Environment.NewLine + SharingManager.NetConnectionProps[con].DeviceName;
                            Internet_Freigabe_Auswahlbox.Items.Add(new ComboBoxItem { Content = con_name });
                            control_btn_change(1, false);
                        }
                        else
                        {
                            string con_name = SharingManager.NetConnectionProps[con].Name + System.Environment.NewLine + SharingManager.NetConnectionProps[con].DeviceName;
                            Internet_Freigabe_Auswahlbox.Items.Add(new ComboBoxItem { Content = con_name });
                            if (SharingManager.INetSharingConfigurationForINetConnection[con].SharingEnabled && SharingManager.INetSharingConfigurationForINetConnection[con].SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC)
                                Internet_Freigabe_Auswahlbox.SelectedValue = con_name;
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Exception", MessageBoxButton.OK);
            }
        }

        private void Remove_ICS()
        {
            INetConnection[] cons = (from INetConnection c in SharingManager.EnumEveryConnection
                                     where SharingManager.get_INetSharingConfigurationForINetConnection(c).SharingEnabled
                                     select c).ToArray();
            if (cons.Length == 0)
                return;
            foreach (var con in cons)
            {
                SharingManager.get_INetSharingConfigurationForINetConnection(con).DisableSharing();
            }
        }

        private void Steuer_Taste_Klick()
        {
            control_btn_change(0, true);
            Dispatcher.InvokeAsync(() =>
            {
                switch (Steuer_Taste_state)
                {
                    case true:
                        try
                        {
                            netsh.set_hostednetwork(Name_Textbox.Text, Passwort_Pbox.Password);
                            netsh.start_hostednetwork();
                            INetConnection Router = (from INetConnection con in SharingManager.EnumEveryConnection
                                                     where SharingManager.NetConnectionProps[con].Status == tagNETCON_STATUS.NCS_CONNECTED
                                                     where SharingManager.NetConnectionProps[con].DeviceName.StartsWith("Microsoft ")
                                                     select con).ToArray().First();
                            bool isrightstate = SharingManager.INetSharingConfigurationForINetConnection[Router].SharingEnabled && SharingManager.INetSharingConfigurationForINetConnection[Router].SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE;
                            if (Internet_Freigabe_Auswahlbox.SelectedIndex != 0 && !isrightstate)
                            {
                                INetConnection Freigabe = (from INetConnection con in SharingManager.EnumEveryConnection where SharingManager.get_NetConnectionProps(con).DeviceName == Internet_Freigabe_Auswahlbox.SelectedValue.ToString().Split(Environment.NewLine.ToArray(), StringSplitOptions.None).Last() select con).First();
                                if (!(SharingManager.INetSharingConfigurationForINetConnection[Freigabe].SharingEnabled && SharingManager.INetSharingConfigurationForINetConnection[Freigabe].SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC))
                                {
                                    while (true)
                                    {
                                        try
                                        {
                                            Remove_ICS();
                                            break;
                                        }
                                        catch
                                        {
                                            if (MessageBox.Show("Entfernen von der Internetfreigabe fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                                continue;
                                            else
                                                throw new Exception("Fehler");
                                        }
                                    }
                                    while (true)
                                    {
                                        try
                                        {
                                            SharingManager.get_INetSharingConfigurationForINetConnection(Freigabe).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
                                            break;
                                        }
                                        catch
                                        {
                                            if (MessageBox.Show("Erstellen von der Internetfreigabe fehlgeschlagen (Freigabe Netzwerk)" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                                continue;
                                            else
                                                throw new Exception("Fehler");
                                        }
                                    }
                                    while (true)
                                    {
                                        try
                                        {
                                            SharingManager.get_INetSharingConfigurationForINetConnection(Router).EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                                            break;
                                        }
                                        catch
                                        {
                                            if (MessageBox.Show("Erstellen von der Internetfreigabe fehlgeschlagen (Einrichtung Router)" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                                continue;
                                            else
                                                throw new Exception("Fehler");
                                        }
                                    }
                                }
                            }
                            else if (Internet_Freigabe_Auswahlbox.SelectedIndex == 0 && isrightstate)
                                while (true)
                                {
                                    try
                                    {
                                        Remove_ICS();
                                        break;
                                    }
                                    catch
                                    {
                                        if (MessageBox.Show("Entfernen von der Internetfreigabe fehlgeschlagen" + System.Environment.NewLine + "Erneut Versuchen?", "Fehler", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                                            continue;
                                        else
                                            throw new Exception("Fehler");
                                    }
                                }
                            control_btn_change(1, true);
                            break;
                        }
                        catch
                        {
                            MessageBox.Show("Failed to start hostednetwork probably missing driver support", "Error", MessageBoxButton.OK);
                            control_btn_change(2, true);
                        }
                        refresh();
                        break;
                    case false:
                        try
                        {
                            netsh.stop_hostednetwork();
                            refresh();
                            control_btn_change(2, true);
                        }
                        catch
                        {
                            MessageBox.Show("Failed to stop hostednetwork", "Error", MessageBoxButton.OK);
                            control_btn_change(1, true);
                        }
                        break;
                }
            }, DispatcherPriority.Background);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Steuer_Taste.IsEnabled)
                netsh.set_hostednetwork(Name_Textbox.Text, Passwort_Pbox.Visibility == Visibility.Visible ? Passwort_Pbox.Password : Passwort_Tbox.Text);
        }
    }
}

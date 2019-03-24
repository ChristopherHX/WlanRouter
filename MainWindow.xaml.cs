using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        string vbss;
        string bats;

        public MainWindow()
        {
            InitializeComponent();
        }

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
                if (vbss != string.Empty)
                    File.Delete(bats);
            }
            catch { }
        }

        private void Button_click(object sender, RoutedEventArgs e)
        {
            Delete_tmp_files();
            vbss = GetTempFilePathWithExtension(".vbs");
            bats = GetTempFilePathWithExtension(".bat");
            File.WriteAllText(vbss, "CreateObject(\"Wscript.Shell\").Run \"\"\"\" & WScript.Arguments(0) & \"\"\"\", 0, False");
            File.WriteAllText(bats, "netsh wlan start hostednetwork"/*+ System.Environment.NewLine + "Test"*/);
            Process.Start("wscript", vbss + " " + bats);
        }

        private void Windows_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Delete_tmp_files();
        }
    }
}

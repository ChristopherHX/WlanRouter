using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WlanRouter {
    public partial class App : Application {
        // private void Application_Startup(object sender, StartupEventArgs e) {
        //     int verbose = 0;
        //     var optionSet = new OptionSet {
        //         { "v|verbose", "verbose output, repeat for more verbosity.",   
        //                 arg => verbose++ }
        //     };

        //     var extra = optionSet.Parse(e.Args);
        //     var mainWindow = new MainWindow(verbose);
        //     mainWindow.Show();
        // }

        // [STAThread]
        // public static void Main(string[] args) {
        //     if (args != null && args.Length > 0) {
        //         for (int i = 0; i < args.Length; i++) {
        //             switch(args[i]) {
        //                 case "starthn":
        //                     new NativeWiFi().StartHostedNetwork();
        //                 break;
        //                 case "stophn":
        //                     new NativeWiFi().StopHostedNetwork();
        //                 break;
        //             }
        //         }
        //     } else {
        //         var app = new App();
        //         app.InitializeComponent();
        //         app.Run();
        //     }
        // }
    }
}

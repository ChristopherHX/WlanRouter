using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class NativeIcon {   
    [DllImport("shell32.dll")]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    public static ImageSource ExtractIcon(string fileName, int id) {
        IntPtr hIcon = ExtractIcon(Process.GetCurrentProcess().Handle, fileName, id);
        return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }
}
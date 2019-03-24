using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class Netzwerkshell
{
    public int codepage = 437;

    public Netzwerkshell()
    {
        try {
            codepage = Convert.ToInt32(this.info("cmd", "/c chcp")[0].TrimEnd('.'));
        }
        catch {
            
        }
    }

    private string cmd(string filename, string command)
    {
        Process cmd = new Process();
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(codepage);
        cmd.StartInfo.FileName = filename;
            cmd.StartInfo.Arguments = command;
        cmd.Start();
        string output = cmd.StandardOutput.ReadToEnd();
        cmd.WaitForExit();
        int code = cmd.ExitCode;
        cmd.Close();
        return (code == 0) ? output : null;
    }

    private string[] info(string filename, string command)
    {
        string Out = cmd(filename, command);
        if (Out != null)
        {
            char last = ' ';
            string line = null;
            bool escaped = true, ignore = false;
            List<string> l = new List<string>();
            foreach (char c in Out.ToArray())
            {
                if (c == ' ' && last == ':' && escaped)
                {
                    escaped = false;
                    ignore = true;
                }
                else if (c == Environment.NewLine.ToArray().Last() && !escaped)
                {
                    if (line != null)
                        l.Add(line);
                    line = null;
                    escaped = true;
                    ignore = false;
                }
                else if (c != Environment.NewLine.ToArray().First() && c != Environment.NewLine.ToArray().Last() && ignore)
                    line += c;
                last = c;
            }
            return l.ToArray();
        }
        else
            return null;
    }

    public string get_password()
    {
        return info("netsh", "wlan show hostednetwork setting=security")[3];
    }

    public string get_ssid()
    {
        return info("netsh", "wlan show hostednetwork")[1].Replace("\"", null);
    }

    public bool get_state()
    {
        return info("netsh", "wlan show hostednetwork").Length > 6;
    }

    public string get_client_count()
    {
        var result = info("netsh", "wlan show hostednetwork");
        return result[9] + "/" + result[2];
    }

    public void set_hostednetwork(string ssid, string key)
    {
        if (cmd("netsh", "wlan set hostednetwork ssid=\"" + ssid.Replace("\"", null) + "\" key=\"" + key.Replace("\"", null) + "\" mode=allow") == null)
            throw new Exception("Error");
    }

    public void start_hostednetwork()
    {
        if (cmd("netsh", "wlan start hostednetwork") == null)
            throw new Exception("Error");
    }

    public void stop_hostednetwork()
    {
        if (cmd("netsh", "wlan stop hostednetwork") == null)
            throw new Exception("Error");
    }
}

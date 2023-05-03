using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using adbgui.Adb.Models;

namespace adbgui.Adb;

public class Adb
{
    class CommandResult
    {
        public int ExitCode { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
    }

    public static Adb? Instance = null;

    public Adb(string? adbFullPath)
    {
        if (string.IsNullOrEmpty(adbFullPath))
            throw new ArgumentNullException(nameof(adbFullPath));

        AdbFullPath = adbFullPath;
    }

    private string AdbFullPath { get; set; }

    public string? Version { get; private set; }

    #region public operations
    public async Task<bool> GetVersion()
    {
        var cmdRes = await RunCommand("version");
        if (cmdRes.ExitCode == 0 && !string.IsNullOrEmpty(cmdRes.Output)) {
            var lines = cmdRes.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0) {
                var m = Regex.Match(lines[0], "Android Debug Bridge version ([0-9\\.]+)");
                if (m.Success) {
                    Version = m.Groups[1].Value;
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<List<Device>> ListDevices()
    {
        var res = new List<Device>();

        var cmdRes = await RunCommand("devices -l");
        if (cmdRes.ExitCode == 0 && !string.IsNullOrEmpty(cmdRes.Output)) {
            var lines = cmdRes.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1) {
                for (var i = 1; i < lines.Length; i++) {
                    var elem = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (elem.Length > 1) {
                        var device = new Device
                        {
                            Id = elem[0],
                            Authorized = elem[1] == "device"
                        };

                        for (var j = 2; j < elem.Length; j++) {
                            var e = elem[j];
                            if (e.StartsWith("product:")) {
                                device.Product = e.Substring(8);
                            } else if (e.StartsWith("model:")) {
                                device.Model = e.Substring(6);
                            } else if (e.StartsWith("device:")) {
                                device.Name = e.Substring(7);
                            } else if (e.StartsWith("transport_id:")) {
                                device.TransportId = e.Substring(13);
                            }
                        }
                        res.Add(device);
                    }
                }
            }
        }

        return res;
    }

    public async Task<List<Package>> ListPackages(string? deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var res = new List<Package>();

        var pkgs = await GetPackages($"-s {deviceId} shell cmd package list packages -U -f -d");
        foreach (var p in pkgs) {
            p.Enabled = false;
        }
        res.AddRange(pkgs);

        pkgs = await GetPackages($"-s {deviceId} shell cmd package list packages -U -f -e");
        foreach (var p in pkgs) {
            p.Enabled = true;
        }
        res.AddRange(pkgs);

        pkgs = await GetPackages($"-s {deviceId} shell cmd package list packages -U -f -s");
        foreach (var p in pkgs) {
            var ep = res.FirstOrDefault(ep => ep.Uid == p.Uid);
            if (ep != null) {
                ep.System = true;
            } else {
                p.System = true;
                res.Add(p);
            }
        }

        pkgs = await GetPackages($"-s {deviceId} shell cmd package list packages -U -f -3");
        foreach (var p in pkgs) {
            var ep = res.FirstOrDefault(ep => ep.Uid == p.Uid);
            if (ep != null) {
                ep.ThirdParty = true;
            } else {
                p.ThirdParty = true;
                res.Add(p);
            }
        }

        return res;
    }

    #endregion

    private async Task<List<Package>> GetPackages(string args)
    {
        var res = new List<Package>();
        var cmdRes = await RunCommand(args);
        if (cmdRes.ExitCode == 0 && !string.IsNullOrEmpty(cmdRes.Output)) {
            var lines = cmdRes.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex("package:(.*)=(.*) uid:(.*)");
            foreach (var line in lines) {
                var m = regex.Match(line);
                if (m.Success) {
                    res.Add(new Package()
                    {
                        File = m.Groups[1].Value,
                        Name = m.Groups[2].Value,
                        Uid = m.Groups[3].Value
                    });
                }
            }
        }

        return res;
    }

    private async Task<CommandResult> RunCommand(string args)
    {
        var si = new ProcessStartInfo
        {
            FileName = AdbFullPath,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(AdbFullPath),
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var res = new CommandResult();
        using (var process = new Process()) {
            process.StartInfo = si;

            process.Start();

            res.Output = await process.StandardOutput.ReadToEndAsync();
            res.Error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            res.ExitCode = process.ExitCode;
        }

        if (!string.IsNullOrEmpty(res.Output))
            res.Output = res.Output.Replace("\r", string.Empty);
        if (!string.IsNullOrEmpty(res.Error))
            res.Error = res.Error.Replace("\r", string.Empty);
        return res;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// <summary>
    /// Get ADB version
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get devices
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// List installed packages
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
                p.Enabled = true;
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
                p.Enabled = true;
                p.ThirdParty = true;
                res.Add(p);
            }
        }

        return res;
    }

    /// <summary>
    /// Open a new terminal window
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void OpenTerminal(string? deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Process.Start($"cmd", $"/C \"\"{AdbFullPath}\" -s {deviceId} shell\"");
        }
    }

    /// <summary>
    /// Install an APK
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="apkPath">The path to the APK to install</param>
    /// <returns></returns>
    public async Task<PackageOperationResult> InstallApk(string? deviceId, string apkPath)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} install -r \"{apkPath}\"");

        return new PackageOperationResult()
        {
            Result = cmdRes.ExitCode == 0,
            Output = cmdRes.Output,
            Error = cmdRes.Error
        };
    }

    /// <summary>
    /// Uninstall a package
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="packageName">The package name to uninstall</param>
    /// <returns></returns>
    public async Task<PackageOperationResult> UninstallPackage(string? deviceId, string packageName)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} uninstall {packageName}");

        return new PackageOperationResult()
        {
            Result = cmdRes.ExitCode == 0,
            Output = cmdRes.Output,
            Error = cmdRes.Error
        };
    }

    /// <summary>
    /// Enable a package
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="packageName">The package name to enable</param>
    /// <returns></returns>
    public async Task<PackageOperationResult> EnablePackage(string? deviceId, string packageName)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} shell cmd package enable {packageName}");

        return new PackageOperationResult()
        {
            Result = cmdRes.ExitCode == 0,
            Output = cmdRes.Output,
            Error = cmdRes.Error
        };
    }

    /// <summary>
    /// Disable a package
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="packageName">The package name to disable</param>
    /// <returns></returns>
    public async Task<PackageOperationResult> DisablePackage(string? deviceId, string packageName)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} shell cmd package disable {packageName}");

        return new PackageOperationResult()
        {
            Result = cmdRes.ExitCode == 0,
            Output = cmdRes.Output,
            Error = cmdRes.Error
        };
    }

    public async Task<bool> PullFile(string? deviceId, string deviceFilePath, string localFilePath)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} pull \"{deviceFilePath}\" \"{localFilePath}\"");

        return cmdRes.ExitCode == 0;
    }

    public async Task<bool> PushFile(string? deviceId, string localFilePath, string deviceFilePath)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} push \"{localFilePath}\" \"{deviceFilePath}\"");

        return cmdRes.ExitCode == 0;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using adbgui.Adb.Models;

namespace adbgui.Adb;

public partial class Adb
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
                var m = AdbVersionRegex().Match(lines[0]);
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

        // Get packages version
        var cmdRes = await RunCommand($"-s {deviceId} shell dumpsys package packages ");
        if (cmdRes.ExitCode == 0 && !string.IsNullOrEmpty(cmdRes.Output)) {
            var pkgName = string.Empty;
            var lines = cmdRes.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var pkgNameMatch = PackageNameRegex().Match(line);
                if (pkgNameMatch.Success) {
                    pkgName = pkgNameMatch.Groups[1].Value;
                } else {
                    var pkgVersionMatch = PackageVersionRegex().Match(line);
                    if (pkgVersionMatch.Success && !string.IsNullOrEmpty(pkgName)) {
                        var ep = res.FirstOrDefault(ep => ep.Name == pkgName);
                        if (ep != null) {
                            ep.Version = pkgVersionMatch.Groups[1].Value;
                        }

                    }
                }
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

    /// <summary>
    /// Download a package APK
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="packageName">The package name to download</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<PackageOperationResult> DownloadPackage(string? deviceId, string packageName)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} shell cmd package path {packageName}");
        if (cmdRes.ExitCode != 0 || string.IsNullOrEmpty(cmdRes.Output)) {
            return new PackageOperationResult()
            {
                Result = cmdRes.ExitCode == 0,
                Output = cmdRes.Output,
                Error = cmdRes.Error
            };
        }

        var m = PackageRegex().Match(cmdRes.Output);
        if (m.Success) {
            var file = m.Groups[1].Value;
            if (await PullFile(deviceId, file, Path.Combine(GetDownloadFolder(), $"{packageName}.apk"))) {
                return new PackageOperationResult()
                {
                    Result = true,
                    Output = string.Empty,
                    Error = string.Empty
                };
            }
        }

        return new PackageOperationResult()
        {
            Result = false,
            Output = string.Empty,
            Error = string.Empty
        };
    }

    /// <summary>
    /// Disable a package
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="packageName">The package name to disable</param>
    /// <returns></returns>
    public async Task<string> GetPackageVersion(string? deviceId, string packageName)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} shell dumpsys package {packageName}");
        if (cmdRes.ExitCode != 0 || string.IsNullOrEmpty(cmdRes.Output))
            return string.Empty;

        var m = Regex.Match(cmdRes.Output, @"\s+versionName=(.*)$");
        if (m.Success) {
            return m.Groups[1].Value;
        }

        return string.Empty;
    }

    /// <summary>
    /// Download a file from the device
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="deviceFilePath">The path of the file on the device</param>
    /// <param name="localFilePath">The local location to download the file to</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> PullFile(string? deviceId, string deviceFilePath, string localFilePath)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} pull \"{deviceFilePath}\" \"{localFilePath}\"");

        return cmdRes.ExitCode == 0;
    }

    /// <summary>
    /// Upload a file from the device
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="localFilePath">The local file to upload to the device</param>
    /// <param name="deviceFilePath">The path of the file on the device</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> PushFile(string? deviceId, string localFilePath, string deviceFilePath)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} push \"{localFilePath}\" \"{deviceFilePath}\"");

        return cmdRes.ExitCode == 0;
    }

    /// <summary>
    /// List the content of a directory
    /// </summary>
    /// <param name="deviceId">The device id</param>
    /// <param name="path">The path</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<List<FileSystemItem>> ListDirectoryContent(string? deviceId, string path)
    {
        if (string.IsNullOrEmpty(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        var cmdRes = await RunCommand($"-s {deviceId} shell ls -la \"{path}\"");
        if (cmdRes.ExitCode != 0 && string.IsNullOrEmpty(cmdRes.Output)) {
            throw new Exception($"Error listing {path}: {cmdRes.Error}");
        }

        var res = new List<FileSystemItem>();
        if (!string.IsNullOrEmpty(cmdRes.Output)) {
            var lines = cmdRes.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var m = LsLineRegex().Match(line);
                if (m.Success) {
                    var item = new FileSystemItem();
                    item.Type = m.Groups[1].Value[0] switch
                    {
                        '-' => FileSystemItem.FileTypes.File,
                        'd' => FileSystemItem.FileTypes.Directory,
                        'l' => FileSystemItem.FileTypes.Symlink,
                        _ => item.Type
                    };
                    item.OwnerUser = m.Groups[3].Value;
                    item.OwnerGroup = m.Groups[4].Value;
                    item.Size = long.Parse(m.Groups[5].Value);
                    if (DateTime.TryParseExact(m.Groups[6].Value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) {
                        item.LastModifiedDate = dt;
                    }
                    item.Name = m.Groups[7].Value;

                    res.Add(item);
                }
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
            foreach (var line in lines) {
                var m = PackageListRegex().Match(line);
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

    private string GetDownloadFolder()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folders = new List<string>()
        {
            Path.Combine(home, "Downloads"),
            Path.Combine(home, "downloads"),
            Path.Combine(home, "Download"),
            Path.Combine(home, "download"),
        };

        foreach (var f in folders) {
            if (Directory.Exists(f))
                return f;
        }
        return home;
    } // GetDownloadFolder

    [GeneratedRegex("package:(.*)")]
    private static partial Regex PackageRegex();

    [GeneratedRegex("Android Debug Bridge version ([0-9\\.]+)")]
    private static partial Regex AdbVersionRegex();

    [GeneratedRegex("package:(.*)=(.*) uid:(.*)")]
    private static partial Regex PackageListRegex();

    [GeneratedRegex(@" +Package \[(.*)\] \((.*)\)")]
    private static partial Regex PackageNameRegex();

    [GeneratedRegex(" +versionName=(.*)")]
    private static partial Regex PackageVersionRegex();

    [GeneratedRegex("(.*?) +([0-9]+) +(.*?) +(.*?) +([0-9]+) +([0-9\\-: ]+)+ +(.*)")]
    private static partial Regex LsLineRegex();
}
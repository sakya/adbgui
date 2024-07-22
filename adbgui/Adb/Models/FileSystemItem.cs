using System;
using System.Security.Cryptography;

namespace adbgui.Adb.Models;

public class FileSystemItem
{
    public enum FileTypes
    {
        Directory,
        File,
        Symlink
    }

    public string Name { get; set; } = null!;
    public FileTypes Type { get; set; }
    public string? OwnerUser { get; set; }
    public string? OwnerGroup { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public long Size { get; set; }
}
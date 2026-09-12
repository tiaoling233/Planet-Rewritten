using System.IO;
using Planet.Layout;

namespace Planet.Main.Services;

/// <summary>
/// 应用路径中心：负责解析便携版根目录，并据此推导 Data、Logs、
/// Applets、Extensions、Temp 等路径。
///
/// 根目录解析优先级：
///   1. 命令行参数 --root=&lt;路径&gt;（由启动器 Planet.exe 传入）；
///   2. 从程序所在目录向上查找首个包含 Data 文件夹的目录（应对直接运行本体/开发调试场景）；
///   3. 兜底使用程序所在目录。
/// </summary>
public static class AppPaths
{
    private static readonly object SyncRoot = new();

    private static string _root = string.Empty;

    /// <summary>便携版根目录（绝对路径，末尾无分隔符）。</summary>
    public static string Root => _root;

    /// <summary>Data 文件夹路径。</summary>
    public static string DataDirectory { get; private set; } = string.Empty;

    /// <summary>日志文件夹路径（Data\Logs）。</summary>
    public static string LogsDirectory { get; private set; } = string.Empty;

    /// <summary>功能性插件文件夹路径（Data\Applets）。</summary>
    public static string AppletsDirectory { get; private set; } = string.Empty;

    /// <summary>非功能性插件文件夹路径（Data\Extensions）。</summary>
    public static string ExtensionsDirectory { get; private set; } = string.Empty;

    /// <summary>临时文件文件夹路径（Data\Temp）。</summary>
    public static string TempDirectory { get; private set; } = string.Empty;

    /// <summary>设置等配置所用文本文件路径（Data\settings.txt）。</summary>
    public static string SettingsFile { get; private set; } = string.Empty;

    /// <summary>
    /// 从命令行参数与环境解析便携版根目录并完成初始化。
    /// 应在程序启动最早阶段调用且仅调用一次。
    /// </summary>
    public static void InitializeFromCommandLine(string[] args)
    {
        string root = ResolveRoot(args);
        Initialize(root);
    }

    /// <summary>
    /// 以指定根目录初始化路径信息。
    /// </summary>
    public static void Initialize(string root)
    {
        lock (SyncRoot)
        {
            _root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            DataDirectory = Path.Combine(_root, AppLayout.DataFolderName);
            LogsDirectory = Path.Combine(DataDirectory, AppLayout.LogsFolderName);
            AppletsDirectory = Path.Combine(DataDirectory, AppLayout.AppletsFolderName);
            ExtensionsDirectory = Path.Combine(DataDirectory, AppLayout.ExtensionsFolderName);
            TempDirectory = Path.Combine(DataDirectory, AppLayout.TempFolderName);
            SettingsFile = Path.Combine(DataDirectory, AppLayout.SettingsFileName);
        }
    }

    /// <summary>
    /// 确保 Data 及全部子文件夹存在（幂等），可安全重复调用。
    /// </summary>
    public static void EnsureDataStructure()
    {
        Directory.CreateDirectory(DataDirectory);
        foreach (string subfolder in AppLayout.DataSubfolders)
        {
            Directory.CreateDirectory(Path.Combine(DataDirectory, subfolder));
        }
    }

    /// <summary>解析根目录：--root 参数优先，其次向上查找包含 Data 的目录，最后兜底程序所在目录。</summary>
    private static string ResolveRoot(string[] args)
    {
        foreach (string argument in args)
        {
            const string prefix = "--root=";
            if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string value = argument[prefix.Length..].Trim().Trim('"');
                if (value.Length > 0)
                {
                    return value;
                }
            }
        }

        // 从可执行文件所在目录向上查找首个包含 Data 文件夹的目录。
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, AppLayout.DataFolderName)))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
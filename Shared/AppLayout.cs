namespace Planet.Layout;

/// <summary>
/// 便携化部署目录布局统一约定。
///
/// 最终便携版根目录（命名为 Planet）仅包含两个顶层条目：
///   Planet.exe                     —— 启动器（Launcher）
///   Data/                          —— 数据存储文件夹
///       Planet.Main.exe            —— 软件本体
///       settings.txt               —— 文本文件（存储设置等配置）
///       Logs/                      —— 日志文件夹
///       Applets/                   —— 功能性插件存储文件夹
///       Extensions/                —— 非功能性插件存储文件夹
///       Temp/                      —— 临时文件文件夹（含下载插件时的 zip 文件）
///
/// Applets 与 Extensions 中每个插件文件夹内均需包含对应的信息文件与文本文件，
/// 该约定由插件安装逻辑（后续阶段实现）保证。
/// </summary>
public static class AppLayout
{
    /// <summary>数据存储文件夹名。</summary>
    public const string DataFolderName = "Data";

    /// <summary>日志文件夹名。</summary>
    public const string LogsFolderName = "Logs";

    /// <summary>功能性插件存储文件夹名。</summary>
    public const string AppletsFolderName = "Applets";

    /// <summary>非功能性插件存储文件夹名。</summary>
    public const string ExtensionsFolderName = "Extensions";

    /// <summary>临时文件文件夹名（含下载插件时的 zip 文件）。</summary>
    public const string TempFolderName = "Temp";

    /// <summary>设置等配置所用的文本文件名。</summary>
    public const string SettingsFileName = "settings.txt";

    /// <summary>软件本体文件名（位于 Data 文件夹中）。</summary>
    public const string MainExecutableName = "Planet.Main.exe";

    /// <summary>Data 文件夹下的全部子文件夹（不含各自更深层内容）。</summary>
    public static string[] DataSubfolders => new string[]
    {
        LogsFolderName,
        AppletsFolderName,
        ExtensionsFolderName,
        TempFolderName,
    };
}
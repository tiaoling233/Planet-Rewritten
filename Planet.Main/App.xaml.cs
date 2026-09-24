using System.Reflection;
using System.Windows;
using Planet.Main.Services;

namespace Planet.Main;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // 先注册全局异常钩子，确保任何后续初始化异常都能被记录。
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

        base.OnStartup(e);

        // 1. 解析便携版根目录（--root 由启动器传入）并初始化全部路径。
        AppPaths.InitializeFromCommandLine(e.Args);

        // 2. 补齐 Data 目录结构（与启动器互补，保证直接运行本体同样可用）。
        AppPaths.EnsureDataStructure();

        // 3. 初始化日志系统：生成本次启动的独立日志文件。
        LogService.Initialize(AppPaths.LogsDirectory);

        // 4. 加载设置文件（Data\settings.txt）。
        SettingsService.Load(AppPaths.SettingsFile);

        // 5. 恢复上次使用的主题；未知主题名称会安全回退到“默认”。
        string requestedTheme = SettingsService.ThemeName;
        bool themeRestored = ThemeService.ApplyTheme(requestedTheme);
        if (!themeRestored)
        {
            LogService.Warn($"未知主题“{requestedTheme}”，已回退到“{ThemeService.CurrentThemeName}”");
            SettingsService.ThemeName = ThemeService.CurrentThemeName;
        }

        // 6. 记录本次启动的基本信息。
        LogService.Info($"Planet {GetVersionText()} 启动");
        LogService.Info($"操作系统：{Environment.OSVersion}");
        LogService.Info($"运行时：{Environment.Version}");
        LogService.Info($"便携版根目录：{AppPaths.Root}");
        LogService.Info($"数据目录：{AppPaths.DataDirectory}");
        LogService.Info($"设置文件：{AppPaths.SettingsFile}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogService.Info($"Planet 退出，退出码：{e.ApplicationExitCode}");
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogService.Error("UI 线程发生未处理异常", e.Exception);
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogService.Error("发生未处理异常（非 UI 线程）", e.ExceptionObject as Exception);
    }

    private static string GetVersionText()
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? string.Empty : version.ToString(3);
    }
}


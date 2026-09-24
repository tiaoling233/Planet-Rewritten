using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 设置页：主题设置、下载源、标签页选择器视效。
/// 主题选择会立即应用并持久化；其余选项当前仍只记录日志。
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // App 启动时已应用持久化主题；这里只同步按钮选中状态和页面提示。
        RadioButton? selectedTheme = ThemeWrapPanel.Children
            .OfType<RadioButton>()
            .FirstOrDefault(button => string.Equals(
                button.Content?.ToString(),
                ThemeService.CurrentThemeName,
                StringComparison.Ordinal));
        (selectedTheme ??= ThemeWrapPanel.Children.OfType<RadioButton>().First()).IsChecked = true;
        CurrentThemeText.Text = $"当前选中：{ThemeService.CurrentThemeName}";

        SettingsHintText.Text = "加速网站前缀将用于 Github 资源下载，如 ghproxy.dev/";
        VisualHintText.Text = "提示：标签页选择器可独立于主窗口，支持合并/独立模式，位置可设为上/下/左/右。";
        LogService.Info($"设置页已加载，当前主题：{ThemeService.CurrentThemeName}");
    }

    /// <summary>主题项选中：更新提示与日志（互斥与高亮由 GroupName + ThemeRadioStyle 模板负责）。</summary>
    private void OnThemeButtonChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radio)
        {
            return;
        }

        // OnLoaded 会依据 ThemeService.CurrentThemeName 设置选中项；控件完整时正常应用并记录。
        if (CurrentThemeText is null)
        {
            return;
        }

        string themeName = radio.Content?.ToString() ?? ThemeService.DefaultThemeName;
        if (!ThemeService.ApplyTheme(themeName))
        {
            LogService.Warn($"未知主题“{themeName}”，已回退到“{ThemeService.CurrentThemeName}”");
        }

        string appliedTheme = ThemeService.CurrentThemeName;
        SettingsService.ThemeName = appliedTheme;
        CurrentThemeText.Text = $"当前选中：{appliedTheme}";
        LogService.Info($"切换主题：{appliedTheme}");
    }

    /// <summary>下载源变更：记录日志。</summary>
    private void OnDownloadSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DownloadSourceCombo.SelectedItem is ComboBoxItem item)
        {
            string source = item.Content?.ToString() ?? "未知";
            LogService.Info($"切换下载源：{source}");
        }
    }

    /// <summary>加速网站输入变更：记录日志。</summary>
    private void OnProxyTextChanged(object sender, TextChangedEventArgs e)
    {
        string proxy = ProxyTextBox.Text;
        if (!string.IsNullOrEmpty(proxy))
        {
            LogService.Info($"加速网站前缀变更：{proxy}");
        }
    }

    /// <summary>检查更新按钮：记录日志。</summary>
    private void OnCheckUpdateClick(object sender, RoutedEventArgs e)
    {
        LogService.Info("点击检查更新");
    }

    /// <summary>显示模式变更：记录日志。</summary>
    private void OnDisplayModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DisplayModeCombo.SelectedItem is ComboBoxItem item)
        {
            string mode = item.Content?.ToString() ?? "未知";
            LogService.Info($"标签页显示模式变更：{mode}");
        }
    }

    /// <summary>位置变更：记录日志。</summary>
    private void OnPositionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PositionCombo.SelectedItem is ComboBoxItem item)
        {
            string position = item.Content?.ToString() ?? "未知";
            LogService.Info($"标签页位置变更：{position}");
        }
    }

    /// <summary>自动隐藏模式开关：记录日志。</summary>
    private void OnAutoHideToggled(object sender, RoutedEventArgs e)
    {
        bool isChecked = AutoHideCheck.IsChecked == true;
        LogService.Info($"自动隐藏模式：{(isChecked ? "开启" : "关闭")}");
    }
}
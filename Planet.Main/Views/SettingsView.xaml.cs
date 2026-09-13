using System.Windows;
using System.Windows.Controls;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 设置页：主题设置、下载源、标签页选择器视效。
/// 当前为 UI 骨架，所有交互仅记录日志，不实际修改布局或主题。
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
        // 初始化主题选中提示
        CurrentThemeText.Text = "当前选中：默认";
        SettingsHintText.Text = "加速网站前缀将用于 Github 资源下载，如 ghproxy.dev/";
        VisualHintText.Text = "提示：标签页选择器可独立于主窗口，支持合并/独立模式，位置可设为上/下/左/右。";
        LogService.Info("设置页已加载");
    }

    /// <summary>主题按钮点击：仅记录日志并更新选中状态提示。</summary>
    private void OnThemeButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // 清除其他按钮的选中状态
            foreach (var child in ThemeWrapPanel.Children)
            {
                if (child is Button b)
                {
                    b.Tag = null;
                }
            }

            // 设置当前按钮为选中
            button.Tag = "Selected";

            string themeName = button.Content?.ToString() ?? "未知";
            CurrentThemeText.Text = $"当前选中：{themeName}";
            LogService.Info($"切换主题：{themeName}");
        }
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
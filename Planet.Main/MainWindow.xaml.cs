using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Planet.Main.Services;

namespace Planet.Main;

/// <summary>
/// 主窗口：左侧 200px 标签栏（主页/功能/???/信息/设置）+ 右侧内容区。
/// 标签栏空白处按住可拖动窗口；右侧内容区由 ContentControl 承接页面切换。
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush _defaultNavBrush = Brushes.Transparent;
    private static readonly Brush _selectedNavBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x6F, 0x96));
    private static readonly Brush _pageTextBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x4A, 0x53));

    private readonly Button[] _navButtons;

    public MainWindow()
    {
        InitializeComponent();
        _navButtons = new[] { BtnHome, BtnFunct, BtnMystery, BtnInfo, BtnSettings };

        Loaded += OnLoaded;
        Closed += (_, _) => LogService.Info("主窗口已关闭");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LogService.Info("主窗口已加载");
        SwitchPage(0);
    }

    private void OnNavButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button
            && int.TryParse(button.Tag?.ToString(), out int index))
        {
            SwitchPage(index);
        }
    }

    private void SwitchPage(int index)
    {
        // 刷新左侧选中高亮
        for (int i = 0; i < _navButtons.Length; i++)
        {
            _navButtons[i].Background = i == index ? _selectedNavBrush : _defaultNavBrush;
        }

        // 页面占位：后续每个标签页会替换为独立的 UserControl
        MainContent.Content = index switch
        {
            0 => CreatePlaceholder("这是主页"),
            1 => CreatePlaceholder("这是功能"),
            2 => CreatePlaceholder("这是 ???"),
            3 => CreatePlaceholder("这是信息"),
            4 => CreatePlaceholder("这是设置"),
            _ => CreatePlaceholder("未知页面"),
        };
    }

    private static TextBlock CreatePlaceholder(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 20,
            Foreground = _pageTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private void OnHideClick(object sender, RoutedEventArgs e)
    {
        // 隐藏窗口：最小化（后续可改为隐藏到系统托盘）
        WindowState = WindowState.Minimized;
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        // 退出应用
        Application.Current.Shutdown();
    }

    private void OnSidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 点击在导航按钮上时不触发窗口拖动（按钮自行处理点击）。
        if (e.OriginalSource is DependencyObject source
            && FindAncestor<Button>(source) is not null)
        {
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // 窗口处于少数不允许拖动的状态（如刚最小化）时忽略，避免程序异常退出。
            }
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
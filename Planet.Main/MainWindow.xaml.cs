using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    private static readonly Brush _pageTextBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x4A, 0x53));

    private readonly RadioButton[] _navButtons;

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

        // 默认选中“主页”（索引 0）：设置 IsChecked 会触发 Checked 事件 → SwitchPage(0)，同时完成初始高亮
        _navButtons[0].IsChecked = true;
    }

    private void OnNavButtonChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: not null } radio
            && int.TryParse(radio.Tag.ToString(), out int index))
        {
            SwitchPage(index);
        }
    }

    private void SwitchPage(int index)
    {
        // 选中高亮由 NavRadioStyle 模板依据 IsChecked 自动呈现（GroupName 保证互斥），此处只负责切换页面内容
        MainContent.Content = index switch
        {
            0 => new Views.HomeView(),
            1 => CreatePlaceholder("这是功能"),
            2 => new Views.MysteryView(),
            3 => new Views.InfoView(),
            4 => new Views.SettingsView(),
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

    /// <summary>
    /// 显示或隐藏背景暗化遮罩（弹窗弹出时调用）。
    /// </summary>
    public void ShowDimOverlay(bool isVisible)
    {
        DimOverlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnSidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 点击在导航项或按钮上时不触发窗口拖动（控件自行处理点击）。
        if (e.OriginalSource is DependencyObject source
            && FindAncestor<ButtonBase>(source) is not null)
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
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

    /// <summary>
    /// 窗口内标签页切换快捷键：A / ← 上一页，D / → 下一页（0↔4 循环）。
    /// 输入控件聚焦时放行；Win 组合键不参与页面切换。
    /// </summary>
    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 防误触：文本框等输入控件聚焦时（如设置页“加速网站”输入框），字母键应正常输入字符
        if (Keyboard.FocusedElement is TextBoxBase)
        {
            return;
        }

        // 中文输入法激活时字母键会以 Key.ImeProcessed 送达，真实按键保存在 ImeProcessedKey 中
        Key key = e.Key == Key.ImeProcessed ? e.ImeProcessedKey : e.Key;

        ModifierKeys modifiers = Keyboard.Modifiers;

        // Win 组合键交给系统处理，不参与页面切换
        if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
        {
            if (key is Key.Left or Key.Right or Key.Up)
            {
                // Win+←/→/↑：标记已处理，阻止本窗口后续键盘路由将其识别为标签页切换。
                // 说明：系统级窗口吸附由 shell 处理，需全局键盘钩子才能拦截；本项目按需求不引入全局钩子。
                e.Handled = true;
            }

            return;
        }

        // 带 Ctrl / Alt 的组合键（如 Ctrl+A）交由系统或其他功能处理，避免被快捷键劫持
        if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != 0)
        {
            return;
        }

        int current = GetCurrentPageIndex();
        int? target = key switch
        {
            Key.A or Key.Left => current - 1,
            Key.D or Key.Right => current + 1,
            _ => null,
        };

        if (target is null)
        {
            return;   // 未命中快捷键映射
        }

        // 循环切换：0 → 上一页为 4，4 → 下一页为 0
        int index = ((target.Value % _navButtons.Length) + _navButtons.Length) % _navButtons.Length;

        // 复用既有链路：IsChecked=true → Checked → OnNavButtonChecked → SwitchPage
        _navButtons[index].IsChecked = true;
        e.Handled = true;
    }

    /// <summary>读取当前选中标签页的索引（来自 RadioButton 的 Tag）；无选中项时回退到主页（0）。</summary>
    private int GetCurrentPageIndex()
    {
        foreach (RadioButton button in _navButtons)
        {
            if (button.IsChecked == true
                && int.TryParse(button.Tag?.ToString(), out int index))
            {
                return index;
            }
        }

        return 0;
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
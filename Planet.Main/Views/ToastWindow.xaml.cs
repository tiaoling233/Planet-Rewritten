using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 条形信息提醒（Toast）：固定屏幕左上角、左滑入/左滑出、无圆角。
/// 由 NotificationService 负责位置计算、堆叠与生命周期，本窗口只负责外观与自身动画。
/// </summary>
public partial class ToastWindow : Window
{
    private const int SlideMs = 300;
    private const int ShiftMs = 250;

    public static readonly DependencyProperty MessageTextProperty =
        DependencyProperty.Register(
            nameof(MessageText),
            typeof(string),
            typeof(ToastWindow),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToastTypeProperty =
        DependencyProperty.Register(
            nameof(ToastType),
            typeof(ToastType),
            typeof(ToastWindow),
            new PropertyMetadata(ToastType.Blue, OnToastTypeChanged));

    public static readonly DependencyProperty ToastBackgroundProperty =
        DependencyProperty.Register(
            nameof(ToastBackground),
            typeof(Brush),
            typeof(ToastWindow),
            new PropertyMetadata(Brushes.White));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(
            nameof(AccentBrush),
            typeof(Brush),
            typeof(ToastWindow),
            new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty TextBrushProperty =
        DependencyProperty.Register(
            nameof(TextBrush),
            typeof(Brush),
            typeof(ToastWindow),
            new PropertyMetadata(Brushes.Black));

    /// <summary>提醒文本（自动换行，行数增长时窗口高度自适应）。</summary>
    public string MessageText
    {
        get => (string)GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }

    /// <summary>提醒类型（Green / Blue / Red），决定左侧色条与配色。</summary>
    public ToastType ToastType
    {
        get => (ToastType)GetValue(ToastTypeProperty);
        set => SetValue(ToastTypeProperty, value);
    }

    public Brush ToastBackground
    {
        get => (Brush)GetValue(ToastBackgroundProperty);
        set => SetValue(ToastBackgroundProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public Brush TextBrush
    {
        get => (Brush)GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    private bool _dismissRequested;

    /// <summary>显示前由 NotificationService 预测量的内容高度（ActualHeight 生效前用于堆叠计算）。</summary>
    internal double MeasuredHeight { get; set; } = 60;

    /// <summary>堆叠布局用高度：加载后取实际高度，否则取预测量值。</summary>
    internal double LayoutHeight => ActualHeight > 0 ? ActualHeight : MeasuredHeight;

    public ToastWindow()
    {
        InitializeComponent();
        ApplyTheme(ToastType);
    }

    private static void OnToastTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ToastWindow)d).ApplyTheme((ToastType)e.NewValue);
    }

    /// <summary>按类型设置配色：左侧色条 + 背景 + 文本，颜色对比度均按可读性选定。</summary>
    private void ApplyTheme(ToastType type)
    {
        switch (type)
        {
            case ToastType.Green:
                ToastBackground = new SolidColorBrush(Color.FromRgb(0xEF, 0xF7, 0xEE));
                AccentBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
                TextBrush = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20));
                break;
            case ToastType.Red:
                ToastBackground = new SolidColorBrush(Color.FromRgb(0xFD, 0xEC, 0xEC));
                AccentBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0x32, 0x2B));
                TextBrush = new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C));
                break;
            case ToastType.Blue:
            default:
                ToastBackground = new SolidColorBrush(Color.FromRgb(0xEB, 0xF3, 0xFC));
                AccentBrush = new SolidColorBrush(Color.FromRgb(0x2B, 0x6C, 0xB0));
                TextBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x4E, 0x7A));
                break;
        }
    }

    /// <summary>从屏幕外滑入到目标位置（service 在 Loaded 后调用）。</summary>
    internal void BeginEnter(double targetLeft)
    {
        var anim = new DoubleAnimation(targetLeft, TimeSpan.FromMilliseconds(SlideMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
        };
        BeginAnimation(LeftProperty, anim);
    }

    /// <summary>滑出屏幕（可与其他 Toast 的位移动画并行，互不冲突），完成后自动关闭。</summary>
    internal void Dismiss()
    {
        if (_dismissRequested)
        {
            return;
        }
        _dismissRequested = true;

        var wa = SystemParameters.WorkArea;
        double offscreen = wa.Left - Math.Max(ActualWidth, 1) - 60;

        var anim = new DoubleAnimation(offscreen, TimeSpan.FromMilliseconds(SlideMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
        };
        anim.Completed += (_, _) => Close();
        BeginAnimation(LeftProperty, anim);
    }

    /// <summary>平滑移动到新的堆叠位置（不指定 From，自动从当前动画值续接，避免闪烁）。</summary>
    internal void ShiftTo(double top)
    {
        var anim = new DoubleAnimation(top, TimeSpan.FromMilliseconds(ShiftMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
        };
        BeginAnimation(TopProperty, anim);
    }
}

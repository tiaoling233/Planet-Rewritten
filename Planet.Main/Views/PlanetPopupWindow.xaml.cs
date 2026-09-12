using System.Windows;
using System.Windows.Media;

namespace Planet.Main.Views;

/// <summary>
/// 全局预设弹窗：用于统一风格地承载各类提示信息。
/// 标题、分割线、背景等颜色与文本全部通过依赖属性注入，调用方灵活设置。
/// </summary>
public partial class PlanetPopupWindow : Window
{
    // 默认配色：白色背景 / 黑色标题 / 浅灰分割线
    private static readonly Brush DefaultTitleForeground = Brushes.Black;
    private static readonly Brush DefaultDividerBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB));
    private static readonly Brush DefaultPopupBackground = Brushes.White;

    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(
            nameof(TitleText),
            typeof(string),
            typeof(PlanetPopupWindow),
            new PropertyMetadata("提示"));

    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register(
            nameof(TitleForeground),
            typeof(Brush),
            typeof(PlanetPopupWindow),
            new PropertyMetadata(DefaultTitleForeground));

    public static readonly DependencyProperty DividerBrushProperty =
        DependencyProperty.Register(
            nameof(DividerBrush),
            typeof(Brush),
            typeof(PlanetPopupWindow),
            new PropertyMetadata(DefaultDividerBrush));

    public static readonly DependencyProperty PopupBackgroundProperty =
        DependencyProperty.Register(
            nameof(PopupBackground),
            typeof(Brush),
            typeof(PlanetPopupWindow),
            new PropertyMetadata(DefaultPopupBackground));

    public static readonly DependencyProperty MessageTextProperty =
        DependencyProperty.Register(
            nameof(MessageText),
            typeof(string),
            typeof(PlanetPopupWindow),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register(
            nameof(ButtonText),
            typeof(string),
            typeof(PlanetPopupWindow),
            new PropertyMetadata("确定"));

    /// <summary>弹窗标题文本。</summary>
    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    /// <summary>标题前景色（默认黑色）。</summary>
    public Brush TitleForeground
    {
        get => (Brush)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    /// <summary>分割线颜色（默认浅灰）。</summary>
    public Brush DividerBrush
    {
        get => (Brush)GetValue(DividerBrushProperty);
        set => SetValue(DividerBrushProperty, value);
    }

    /// <summary>弹窗背景色（默认白色）。</summary>
    public Brush PopupBackground
    {
        get => (Brush)GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    /// <summary>弹窗内容文本。</summary>
    public string MessageText
    {
        get => (string)GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }

    /// <summary>右下角按钮文本（默认“确定”）。</summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public PlanetPopupWindow()
    {
        InitializeComponent();
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
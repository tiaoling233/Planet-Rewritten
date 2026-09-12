using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

    public static readonly DependencyProperty ButtonTextsProperty =
        DependencyProperty.Register(
            nameof(ButtonTexts),
            typeof(List<string>),
            typeof(PlanetPopupWindow),
            new PropertyMetadata(new List<string> { "确定" }, OnButtonTextsChanged));

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

    /// <summary>右下角按钮文本列表（默认仅一个“确定”，向后兼容单按钮用法）。</summary>
    public List<string> ButtonTexts
    {
        get => (List<string>)GetValue(ButtonTextsProperty);
        set => SetValue(ButtonTextsProperty, value);
    }

    /// <summary>用户点击的按钮索引（0 起，未点击任何按钮为 -1，例如通过 Alt+F4 关闭）。</summary>
    public int SelectedButtonIndex { get; private set; } = -1;

    public PlanetPopupWindow()
    {
        InitializeComponent();
        RebuildButtons();
    }

    private static void OnButtonTextsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PlanetPopupWindow)d).RebuildButtons();
    }

    /// <summary>依据 ButtonTexts 重建右下角按钮区（横向排列，右下角对齐）。</summary>
    private void RebuildButtons()
    {
        if (ButtonPanel is null)
        {
            return; // XAML 尚未加载（InitializeComponent 之前触发），加载后构造函数会再次调用
        }

        ButtonPanel.Children.Clear();
        List<string> texts = ButtonTexts;
        if (texts is null || texts.Count == 0)
        {
            texts = new List<string> { "确定" };
        }

        for (int i = 0; i < texts.Count; i++)
        {
            var button = new Button
            {
                Content = texts[i],
                Style = (Style)Application.Current.FindResource("RoundedButtonStyle"),
                MinWidth = 84,
                Margin = i == 0 ? new Thickness(0, 0, 0, 0) : new Thickness(8, 0, 0, 0),
                Tag = i,
            };
            button.Click += OnButtonButtonClick;
            ButtonPanel.Children.Add(button);
        }
    }

    private void OnButtonButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int index)
        {
            SelectedButtonIndex = index;
        }

        DialogResult = true;
        Close();
    }
}
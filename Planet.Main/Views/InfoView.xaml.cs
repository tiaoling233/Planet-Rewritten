using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Planet.Main.Views;

/// <summary>
/// 信息页：开发者信息 + 致谢名单/开源声明 可展开卡片。
/// 展开收起使用 Height 动画，箭头旋转 0°(收起) ↔ 90°(展开)。
/// </summary>
public partial class InfoView : UserControl
{
    private bool _creditsExpanded;
    private bool _licenseExpanded;

    private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(250);

    public InfoView()
    {
        InitializeComponent();
    }

    private void OnCreditsHeaderClick(object sender, RoutedEventArgs e)
    {
        _creditsExpanded = !_creditsExpanded;
        ToggleCard(CreditsBody, CreditsArrowRotate, _creditsExpanded);
    }

    private void OnLicenseHeaderClick(object sender, RoutedEventArgs e)
    {
        _licenseExpanded = !_licenseExpanded;
        ToggleCard(LicenseBody, LicenseArrowRotate, _licenseExpanded);
    }

    /// <summary>切换卡片：高度 0 ↔ 内容自然高度，箭头 0° ↔ 90°。</summary>
    private static void ToggleCard(Grid body, RotateTransform arrow, bool expand)
    {
        double target = expand ? MeasureNaturalHeight(body) : 0;
        var heightAnim = new DoubleAnimation
        {
            To = target,
            Duration = AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        };
        body.BeginAnimation(HeightProperty, heightAnim);

        var angleAnim = new DoubleAnimation
        {
            To = expand ? 90 : 0,
            Duration = AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        };
        arrow.BeginAnimation(RotateTransform.AngleProperty, angleAnim);
    }

    /// <summary>测量内容自然高度（不受当前 Height 收起状态影响）。</summary>
    /// <remarks>
    /// 注意：Grid 设有显式 Height（收起时为 0）时，WPF 测量会把其 DesiredSize 钳制为该显式值，
    /// 直接测量 Grid 永远得到 0；因此改为测量其唯一内容子元素，并加回子元素的外边距。
    /// </remarks>
    private static double MeasureNaturalHeight(FrameworkElement element)
    {
        if (element is Panel { Children.Count: 1 } panel
            && panel.Children[0] is FrameworkElement child)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return child.DesiredSize.Height + child.Margin.Top + child.Margin.Bottom;
        }

        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return element.DesiredSize.Height;
    }
}

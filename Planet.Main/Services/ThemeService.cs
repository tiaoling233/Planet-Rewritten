using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Planet.Main.Services;

/// <summary>一个全局主题的基础视觉定义。</summary>
/// <param name="Name">主题名称。</param>
/// <param name="BaseBrush">主题纯色；同时用于强调控件和主背景。</param>
public sealed record Theme(string Name, Brush BaseBrush);

/// <summary>
/// 全局主题服务：集中维护主题，并在运行时更新 Application 级动态资源。
/// ThemeBaseBrush 用于强调控件，ThemeBackgroundBrush 用于侧栏和主内容区背景。
/// </summary>
public static class ThemeService
{
    public const string DefaultThemeName = "默认";

    /// <summary>设置页可用的八个内置主题。</summary>
    public static readonly List<Theme> Themes = new()
    {
        new(DefaultThemeName, CreateBrush("#3E6F96")),
        new("玄素黑", CreateBrush("#2B2F36")),
        new("神秘紫", CreateBrush("#7B4B8A")),
        new("深海蓝", CreateBrush("#1E3A8A")),
        new("森林绿", CreateBrush("#2E7D32")),
        new("夕阳橙", CreateBrush("#E65100")),
        new("樱花粉", CreateBrush("#D81B60")),
        new("星空银", CreateBrush("#6B7280")),
    };

    /// <summary>当前已应用的主题名称；未知名称会回退到“默认”。</summary>
    public static string CurrentThemeName { get; private set; } = DefaultThemeName;

    /// <summary>
    /// 应用指定主题并更新全局动态资源。找不到名称时回退到默认主题。
    /// </summary>
    /// <returns>true 表示找到了指定主题；false 表示已回退到默认主题。</returns>
    public static bool ApplyTheme(string? themeName)
    {
        Theme? requested = Themes.FirstOrDefault(theme =>
            string.Equals(theme.Name, themeName?.Trim(), StringComparison.Ordinal));
        Theme theme = requested ?? Themes[0];

        if (Application.Current is null)
        {
            return false;
        }

        Color baseColor = theme.BaseBrush is SolidColorBrush solidBrush
            ? solidBrush.Color
            : Colors.Blue;

        ResourceDictionary resources = Application.Current.Resources;
        resources["ThemeBaseBrush"] = theme.BaseBrush;
        resources["ThemeBackgroundBrush"] = theme.BaseBrush;
        resources["ThemeGlowColor"] = baseColor;
        resources["ThemeHoverBrush"] = CreateBrush(Color.FromArgb(0x1F, baseColor.R, baseColor.G, baseColor.B));
        resources["ThemePressedBrush"] = CreateBrush(Color.FromArgb(0x38, baseColor.R, baseColor.G, baseColor.B));

        CurrentThemeName = theme.Name;
        return requested is not null;
    }

    private static Brush CreateBrush(string colorText)
    {
        return CreateBrush((Color)ColorConverter.ConvertFromString(colorText));
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}

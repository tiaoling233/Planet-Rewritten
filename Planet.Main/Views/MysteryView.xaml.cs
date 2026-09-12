using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Planet.Main.Views;

/// <summary>
/// ??? 页：暂无页面入口。当前完成“今日人品”功能。
/// 随机数种子 = 计算机名称 + 当前日期（确保每天只对应一个数值）。
/// </summary>
public partial class MysteryView : UserControl
{
    public MysteryView()
    {
        InitializeComponent();
    }

    private void OnLuckyButtonClick(object sender, RoutedEventArgs e)
    {
        int value = CalculateTodayLucky();

        var popup = new PlanetPopupWindow
        {
            Owner = Window.GetWindow(this),
            TitleText = $"{DateTime.Now:yyyy年MM月dd日} - 今日人品",
            MessageText = value switch
            {
                100 => "你的今日人品是...100！这就是欧皇吗！",
                0 => "你的今日人品是...诶？！怎么是0！",
                _ => $"你的今日人品是...{value}！",
            },
            ButtonText = "确定",
        };

        // 结果为 0 时：标题与分割线变红、背景泛红；其余情况保持默认配色
        // （默认配色已在依赖属性默认值中定义：白底 / 黑标题 / 灰分割线）。
        if (value == 0)
        {
            Brush red = new SolidColorBrush(Color.FromRgb(0xD0, 0x32, 0x2B));
            popup.TitleForeground = red;
            popup.DividerBrush = red;
            popup.PopupBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xF0));
        }

        popup.ShowDialog();
    }

    /// <summary>按 计算机名称 + 当前日期 计算今日人品（0~100）。</summary>
    private static int CalculateTodayLucky()
    {
        string seedText = $"{Environment.MachineName}|{DateTime.Now:yyyyMMdd}";
        return new Random(StableStringHash(seedText)).Next(0, 101);
    }

    /// <summary>
    /// FNV-1a 稳定哈希：跨进程、跨启动结果一致。
    /// 不用 string.GetHashCode()——它在 .NET Core 中带有进程级随机化。
    /// </summary>
    private static int StableStringHash(string text)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return (int)hash;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Planet.Main;

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
            ButtonTexts = new List<string> { "确定" },
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

        // 背景暗化 + 模态弹窗：try/finally 保证无论弹窗如何关闭都会隐藏遮罩
        var mainWindow = Window.GetWindow(this) as MainWindow;

        mainWindow?.ShowDimOverlay(true);
        try
        {
            popup.ShowDialog();
        }
        finally
        {
            mainWindow?.ShowDimOverlay(false);
        }
    }

    /// <summary>“千万别点”：免责声明弹窗，确认/确...认？触发随机彩蛋，何意味？/我要下车！直接关闭。</summary>
    private void OnDoNotClickButtonClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;

        var popup = new PlanetPopupWindow
        {
            Owner = mainWindow,
            TitleText = "免责声明",
            MessageText = "该功能可能会引发光敏性癫痫，如果因为使用此功能导致身体异常，该软件及其开发者对此不负任何责任",
            ButtonTexts = new List<string> { "确认", "确...认？", "何意味？", "我要下车！" },
            TitleForeground = new SolidColorBrush(Color.FromRgb(0xD0, 0x32, 0x2B)),
            DividerBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0x32, 0x2B)),
            // 浅红背景（保证正文深色文本可读）
            PopupBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xE5, 0xE5)),
        };

        mainWindow?.ShowDimOverlay(true);
        try
        {
            popup.ShowDialog();
        }
        finally
        {
            mainWindow?.ShowDimOverlay(false);
        }

        // 仅“确认”(0) 与“确...认？”(1) 触发随机彩蛋；“何意味？”(2) / “我要下车！”(3) 直接关闭
        if (popup.DialogResult == true && popup.SelectedButtonIndex is 0 or 1)
        {
            TriggerRandomEgg(mainWindow);
        }
    }

    /// <summary>随机彩蛋占位：蓝色信息提醒弹窗（正式彩蛋后续接入）。</summary>
    private static void TriggerRandomEgg(MainWindow? mainWindow)
    {
        var eggPopup = new PlanetPopupWindow
        {
            Owner = mainWindow,
            TitleText = "信息",
            MessageText = "（占位）随机彩蛋已触发！",
            ButtonTexts = new List<string> { "确定" },
            TitleForeground = new SolidColorBrush(Color.FromRgb(0x2B, 0x6C, 0xB0)),
            DividerBrush = new SolidColorBrush(Color.FromRgb(0x2B, 0x6C, 0xB0)),
            PopupBackground = new SolidColorBrush(Color.FromRgb(0xEA, 0xF3, 0xFC)),
        };

        mainWindow?.ShowDimOverlay(true);
        try
        {
            eggPopup.ShowDialog();
        }
        finally
        {
            mainWindow?.ShowDimOverlay(false);
        }
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
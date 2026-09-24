using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 功能页：功能卡片网格。排序规则为“插件卡片优先 → 其余按标题升序（不变文化排序）”。
/// 当前卡片点击仅弹出占位提示，具体功能后续实现。
/// </summary>
public partial class FunctionView : UserControl
{
    /// <summary>标题排序比较器：不变文化升序，与系统区域设置无关，排序结果稳定可预期。</summary>
    private static readonly StringComparer TitleComparer = StringComparer.InvariantCulture;

    /// <summary>已实现功能的卡片标题（摩斯密码编解码）。</summary>
    private const string MorseCodeTitle = "摩斯密码编解码";

    public FunctionView()
    {
        InitializeComponent();
        CardItems.ItemsSource = CreateSortedCards();
    }

    /// <summary>构建功能卡片列表并按“插件优先 → 标题升序”排序。</summary>
    private static List<FunctionCard> CreateSortedCards()
    {
        var cards = new List<FunctionCard>
        {
            // 插件卡片：IsPlugin = true，排序后始终位于首位
            new() { Title = "插件卡片", Description = "浏览与安装插件", Icon = "📦", IsPlugin = true },

            new() { Title = "快捷跳转", Description = "打开常用位置", Icon = "🔗" },
            new() { Title = "搜索", Description = "快速查找内容", Icon = "🔍" },
            new() { Title = "音乐播放", Description = "播放本地音乐", Icon = "🎵" },
            new() { Title = "TTS", Description = "文本转语音朗读", Icon = "🔊" },
            new() { Title = "计算器", Description = "基础数学计算", Icon = "🧮" },
            new() { Title = "键盘宏", Description = "录制按键序列", Icon = "⌨️" },
            new() { Title = "摩斯密码编解码", Description = "文本与电码互转", Icon = "📡" },
        };

        return cards
            .OrderByDescending(card => card.IsPlugin)   // 插件卡片排第一
            .ThenBy(card => card.Title, TitleComparer)  // 其余按标题升序（不变文化）
            .ToList();
    }

    /// <summary>卡片点击：已实现的功能打开独立窗口，其余弹出占位提示。</summary>
    private void OnCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { CommandParameter: FunctionCard card })
        {
            return;
        }

        LogService.Info($"功能页点击卡片：{card.Title}（{(card.IsPlugin ? "插件" : "功能")}）");

        // 已实现的功能：打开独立窗口（不破坏功能页的卡片网格布局）
        if (card.Title == MorseCodeTitle)
        {
            OpenChildWindow(new MorseCodeWindow());
            return;
        }

        ShowPlaceholderPopup(card);
    }

    /// <summary>以模态方式打开子窗口（带主窗口背景暗化）。</summary>
    private void OpenChildWindow(Window window)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        window.Owner = mainWindow;

        mainWindow?.ShowDimOverlay(true);
        try
        {
            window.ShowDialog();
        }
        finally
        {
            mainWindow?.ShowDimOverlay(false);
        }
    }

    /// <summary>占位提示弹窗（插件卡片提示跳转插件市场）。</summary>
    private void ShowPlaceholderPopup(FunctionCard card)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        string message = card.IsPlugin ? "跳转到插件市场" : "功能待实现";

        var popup = new PlanetPopupWindow
        {
            Owner = mainWindow,
            TitleText = card.Title,
            MessageText = message,
            ButtonTexts = new List<string> { "确定" },
        };

        // 背景暗化 + 模态弹窗：try/finally 保证无论弹窗如何关闭都会隐藏遮罩
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
}

/// <summary>功能卡片数据模型。</summary>
public sealed class FunctionCard
{
    /// <summary>卡片标题。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>简短介绍文本。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>图标（当前为 Emoji 占位，后续统一替换为 SVG）。</summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>是否为插件卡片（true 时排序优先，点击提示跳转插件市场）。</summary>
    public bool IsPlugin { get; init; }
}

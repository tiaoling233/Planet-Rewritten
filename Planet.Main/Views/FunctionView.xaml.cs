using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.Text.RegularExpressions;

using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 功能页：功能卡片网格。排序规则为“插件卡片优先 → 其余按标题升序（不变文化排序）”。
/// 已实现的功能以内联子视图显示；其余卡片仍弹出占位提示。
/// </summary>
public partial class FunctionView : UserControl
{
    /// <summary>标题排序比较器：不变文化升序，与系统区域设置无关，排序结果稳定可预期。</summary>
    private static readonly StringComparer TitleComparer = StringComparer.InvariantCulture;

    /// <summary>当前是否显示功能卡片网格（false 表示正在显示内联功能视图）。</summary>
    private bool _isShowingGrid = true;

    /// <summary>已实现功能的卡片标题。</summary>
    private const string MorseCodeTitle = "摩斯密码编解码";

    /// <summary>已实现功能的卡片标题。</summary>
    private const string CalculatorTitle = "计算器";

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
            new()
            {
                Title = "插件卡片",
                Description = "浏览与安装插件",
                IconData = "M452.9 598.91L135.86 414.14c-15.67-9.06-34.85 1.52-36.52 19.29v315.4c0.78 8 5.27 15.18 12.16 19.25l317.02 186.64c16.36 9.65 36.65-2.43 36.65-21.64V620.66c0.01-9-4.65-17.26-12.27-21.75z m460.72-196.08a24.1 24.1 0 0 0-24.46-0.02L572.09 589.45c-7.56 4.43-12.27 12.73-12.27 21.69v321.93c0 8.89 4.71 17.17 12.27 21.61 7.56 4.48 16.85 4.48 24.48 0.03l317.02-186.64c7.59-4.49 12.24-12.7 12.24-21.64V424.44c0-8.89-4.65-17.15-12.21-21.61z m-50.19-116.34v-5.72c0-8.97-4.71-17.2-12.3-21.66L524.75 66.74c-7.57-4.48-16.88-4.48-24.44 0l-339.4 199.98c-7.56 4.46-12.22 12.72-12.22 21.64 0 8.94 4.65 17.21 12.22 21.66l330.07 194.3c7.56 4.43 16.82 4.43 24.41 0.05l335.67-196.2c7.64-4.48 12.37-12.72 12.37-21.68z",
                IsPlugin = true
            },

            new()
            {
                Title = "快捷跳转",
                Description = "打开常用位置",
                IconData = "M517.674667 169.429333a10.666667 10.666667 0 0 0-11.349334 0L180.864 375.04a10.666667 10.666667 0 0 0 0 18.005333l325.461333 205.568a10.666667 10.666667 0 0 0 11.349334 0l325.461333-205.568a10.666667 10.666667 0 0 0 0-18.005333L517.717333 169.386667z m-45.525334-54.101333a74.666667 74.666667 0 0 1 79.701334 0l325.461333 205.525333a74.666667 74.666667 0 0 1 0 126.293334l-325.461333 205.525333a74.666667 74.666667 0 0 1-79.701334 0L146.688 447.146667a74.666667 74.666667 0 0 1 0-126.293334l325.418667-205.525333z M699.434667 502.357333a32 32 0 0 1 44.16-9.941333l133.717333 84.48a74.666667 74.666667 0 0 1 0 126.250667l-325.461333 205.525333a74.666667 74.666667 0 0 1-79.701334 0L146.688 703.146667a74.666667 74.666667 0 0 1 0-126.293334l138.24-87.296 34.218667 54.101334-138.24 87.338666a10.666667 10.666667 0 0 0 0 18.005334l325.418666 205.568a10.666667 10.666667 0 0 0 11.349334 0l325.461333-205.568a10.666667 10.666667 0 0 0 0-18.048l-133.717333-84.437334a32 32 0 0 1-9.984-44.117333z"
            },
            new()
            {
                Title = "搜索",
                Description = "快速查找内容",
                IconData = "M886.720915 793.185101 777.209756 683.684174c-10.07138-10.083659-26.412556-10.083659-36.492122 0l-2.949168 2.947122c0 0-54.103222-53.791114-55.537897-54.692646 42.85811-53.354162 68.520583-121.108289 68.520583-194.864106 0-171.993213-139.423423-311.413566-311.407426-311.413566-171.984003 0-311.406402 139.4224-311.406402 311.413566 0 171.97377 139.4224 311.406402 311.406402 311.406402 73.768097 0 141.53041-25.668613 194.883549-68.536956 0.902556 1.431605 54.689576 55.524594 54.689576 55.524594l-2.953262 2.951215c-10.07138 10.084683-10.07138 26.437116 0 36.506449l109.50502 109.503996c10.081613 10.083659 26.421766 10.083659 36.503379 0l54.750975-54.751998C896.795365 819.594587 896.795365 803.26569 886.720915 793.185101zM439.343725 628.522854c-105.736183 0-191.447287-85.712127-191.447287-191.44831 0-105.760742 85.711104-191.472869 191.447287-191.472869 105.748463 0 191.470823 85.712127 191.470823 191.472869C630.814548 542.809703 545.093211 628.522854 439.343725 628.522854z"
            },
            new()
            {
                Title = "音乐播放",
                Description = "播放本地音乐",
                IconData = "M864 157.333333V725.333333a117.333333 117.333333 0 1 1-64-104.533333V376l-469.333333 37.546667V768a117.333333 117.333333 0 0 1-112.618667 117.248L213.333333 885.333333a117.333333 117.333333 0 1 1 53.333334-221.866666V205.12l597.333333-47.786667zM213.333333 714.666667a53.333333 53.333333 0 1 0 0 106.666666 53.333333 53.333333 0 0 0 0-106.666666z m533.333334-42.666667a53.333333 53.333333 0 1 0 0 106.666667 53.333333 53.333333 0 0 0 0-106.666667z m53.333333-445.333333l-469.333333 37.546666v85.12l469.333333-37.546666v-85.12z"
            },
            new()
            {
                Title = "TTS",
                Description = "文本转语音朗读",
                IconData = "M1010.551467 424.925867c0 86.016-65.365333 155.886933-147.0464 166.638933H819.882667c-81.681067-10.752-147.0464-80.622933-147.0464-166.638933H580.266667c0 123.630933 92.603733 231.1168 212.411733 252.6208V785.066667h87.176533v-107.52C999.662933 656.042667 1092.266667 553.915733 1092.266667 424.96h-81.7152z m-76.253867-231.150934C934.2976 140.014933 890.743467 102.4 841.728 102.4a91.204267 91.204267 0 0 0-92.603733 91.374933v91.374934h190.634666V193.774933h-5.461333z m-190.634667 231.150934c0 53.76 43.588267 91.374933 92.603734 91.374933a91.204267 91.204267 0 0 0 92.603733-91.374933V333.550933h-185.207467v91.374934zM464.213333 150.698667L324.266667 10.24l-6.826667-6.826667L314.026667 0l-3.413334 3.413333-6.826666 6.8608-20.48 20.548267-3.413334 3.413333 3.413334 6.8608 13.653333 13.687467 75.093333 75.3664H218.453333c-122.88 6.826667-218.453333 109.568-218.453333 229.444267v10.274133h51.2v-10.24c0-92.501333 75.093333-171.281067 167.253333-178.107733h153.6L296.96 256.853333l-10.24 10.274134-3.413333 6.826666-3.413334 3.447467 3.413334 3.413334 27.306666 27.409067 3.413334 3.413334-3.413333 30.72-30.8224 116.053333-116.4288 3.413333-3.413334-6.8608zM58.026667 534.254933v130.1504h64.853333v-65.058133h129.706667v359.594667H187.733333V1024h194.56v-65.058133H317.44V599.3472h129.706667v65.058133H512v-130.1504H58.026667z"
            },
            new()
            {
                Title = "计算器",
                Description = "基础数学计算",
                IconData = "M824.888889 0h-625.777778a113.777778 113.777778 0 0 0-113.777778 113.777778v796.444444a113.777778 113.777778 0 0 0 113.777778 113.777778h625.777778a113.777778 113.777778 0 0 0 113.777778-113.777778V113.777778a113.777778 113.777778 0 0 0-113.777778-113.777778zM392.533333 863.573333a11.377778 11.377778 0 0 1-11.377777 11.377778H242.346667a11.377778 11.377778 0 0 1-11.377778-11.377778v-80.782222a11.377778 11.377778 0 0 1 11.377778-11.377778H381.155556a11.377778 11.377778 0 0 1 11.377777 11.377778z m0-146.204444a11.377778 11.377778 0 0 1-11.377777 11.377778H242.346667a11.377778 11.377778 0 0 1-11.377778-11.377778v-80.782222a11.377778 11.377778 0 0 1 11.377778-11.377778H381.155556a11.377778 11.377778 0 0 1 11.377777 11.377778z m0-145.635556a11.377778 11.377778 0 0 1-11.377777 11.377778H242.346667a11.377778 11.377778 0 0 1-11.377778-11.377778V490.951111a11.377778 11.377778 0 0 1 11.377778-11.377778H381.155556a11.377778 11.377778 0 0 1 11.377777 11.377778z m204.8 291.84a11.377778 11.377778 0 0 1-11.377777 11.377778H447.146667a10.808889 10.808889 0 0 1-11.377778-11.377778v-80.782222a10.808889 10.808889 0 0 1 11.377778-11.377778h138.808889a10.808889 10.808889 0 0 1 11.377777 11.377778z m0-146.204444a11.377778 11.377778 0 0 1-11.377777 11.377778H447.146667a10.808889 10.808889 0 0 1-11.377778-11.377778v-80.782222a10.808889 10.808889 0 0 1 11.377778-11.377778h138.808889a10.808889 10.808889 0 0 1 11.377777 11.377778z m0-145.635556a11.377778 11.377778 0 0 1-11.377777 11.377778H447.146667a10.808889 10.808889 0 0 1-11.377778-11.377778V490.951111a10.808889 10.808889 0 0 1 11.377778-11.377778h138.808889a10.808889 10.808889 0 0 1 11.377777 11.377778z m204.8 291.84a11.377778 11.377778 0 0 1-11.377777 11.377778h-139.377778a11.377778 11.377778 0 0 1-11.377778-11.377778v-80.782222a11.377778 11.377778 0 0 1 11.377778-11.377778h139.377778a11.377778 11.377778 0 0 1 11.377777 11.377778z m0-146.204444a11.377778 11.377778 0 0 1-11.377777 11.377778h-139.377778a11.377778 11.377778 0 0 1-11.377778-11.377778v-80.782222a11.377778 11.377778 0 0 1 11.377778-11.377778h139.377778a11.377778 11.377778 0 0 1 11.377777 11.377778z m0-145.635556a11.377778 11.377778 0 0 1-11.377777 11.377778h-139.377778a11.377778 11.377778 0 0 1-11.377778-11.377778V490.951111a11.377778 11.377778 0 0 1 11.377778-11.377778h139.377778a11.377778 11.377778 0 0 1 11.377777 11.377778z m0-244.053333a56.888889 56.888889 0 0 1-56.888889 56.888889H278.755556a56.888889 56.888889 0 0 1-56.888889-56.888889v-113.777778a56.888889 56.888889 0 0 1 56.888889-56.888889h466.488888a56.888889 56.888889 0 0 1 56.888889 56.888889z"
            },
            new()
            {
                Title = "键盘宏",
                Description = "录制按键序列",
                IconData = "M960 224H64c-35.3 0-64 28.6-64 64v464c0 35.3 28.7 64 64 64h896c35.3 0 64-28.7 64-64V288c0-35.4-28.7-64-64-64zM136 480h48c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8z m-8-56v-48c0-4.4 3.6-8 8-8h80c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-80c-4.4 0-8-3.6-8-8z m8 184h48c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8z m176-128c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8h48z m-40-56v-48c0-4.4 3.6-8 8-8h80c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-80c-4.4 0-8-3.6-8-8z m-8 184h368c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8H264c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8z m448 0h48c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8z m-8-184v-48c0-4.4 3.6-8 8-8h80c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-80c-4.4 0-8-3.6-8-8z m0 112c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8h48c4.4 0 8 3.6 8 8v48z m-48-112c0-4.4-3.6-8-8-8h-80c-4.4 0-8 3.6-8 8v48c0 4.4 3.6 8 8 8h80c4.4 0 8-3.6 8-8z m-88 56c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8h48z m-56-56c0-4.4-3.6-8-8-8h-80c-4.4 0-8 3.6-8 8v48c0 4.4 3.6 8 8 8h80c4.4 0 8-3.6 8-8z m-72 56c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8h48c4.4 0 8 3.6 8 8v48z m392 184v-48c0-4.4 3.6-8 8-8h48c4.4 0 8 3.6 8 8v48c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8z m64-152c0 17.7-14.3 32-32 32H760c-4.4 0-8-3.6-8-8v-48c0-4.4 3.6-8 8-8h64c4.4 0 8-3.6 8-8v-96c0-4.4 3.6-8 8-8h48c4.4 0 8 3.6 8 8v136z"
            },
            new()
            {
                Title = "摩斯密码编解码",
                Description = "文本与电码互转",
                IconData = "M942.5 613.3H795.9V473.2c0-15.5-13.1-28-29.4-28s-29.4 12.5-29.4 28v140.1H590.6c-16.2 0-29.4 12.5-29.4 28s13.1 28 29.4 28h146.7v140.1c0 15.5 13.1 28 29.4 28 16.2 0 29.4-12.5 29.4-28v-140h146.7c16.2 0 29.4-12.5 29.4-28-0.2-15.6-13.3-28.1-29.7-28.1zM150.6 218.9v580.7c0 17.8-13.2 32.3-29.3 32.3S92 817.5 92 799.6V218.9c0-17.8 13.2-32.3 29.3-32.3s29.3 14.4 29.3 32.3z m175.8 263.9v-264c0-17.9 13.2-32.3 29.3-32.3s29.3 14.4 29.3 32.3v264c0 17.9-13.2 32.3-29.3 32.3s-29.3-14.5-29.3-32.3z m234.4-50v-214c0-17.9 13.2-32.3 29.3-32.3s29.3 14.4 29.3 32.3v214c0 17.9-13.2 32.3-29.3 32.3s-29.4-14.5-29.3-32.3z"
            },
        };

        foreach (FunctionCard card in cards)
        {
            card.IconData = NormalizePathData(card.IconData);
        }

        return cards
            .OrderByDescending(card => card.IsPlugin)   // 插件卡片排第一
            .ThenBy(card => card.Title, TitleComparer)  // 其余按标题升序（不变文化）
            .ToList();
    }

    /// <summary>卡片点击：已实现的功能以内联视图显示，其余弹出占位提示。</summary>
    private void OnCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { CommandParameter: FunctionCard card })
        {
            return;
        }

        LogService.Info($"功能页点击卡片：{card.Title}（{(card.IsPlugin ? "插件" : "功能")}）");

        if (card.Title == MorseCodeTitle)
        {
            ShowFunctionView(new MorseCodeView());
            return;
        }

        if (card.Title == CalculatorTitle)
        {
            ShowFunctionView(new CalculatorView());
            return;
        }

        ShowPlaceholderPopup(card);
    }

    /// <summary>在主内容区内显示功能子视图，并切换左侧栏为“返回”状态。</summary>
    private void ShowFunctionView(UserControl view)
    {
        _isShowingGrid = false;
        FunctionHost.Content = view;
        FunctionHost.Visibility = Visibility.Visible;
        GridView.Visibility = Visibility.Collapsed;

        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.SetNavigationMode(true);
        }
    }

    /// <summary>返回功能卡片网格，并恢复左侧栏的 Planet 标题。</summary>
    public void GoBackToGrid()
    {
        if (_isShowingGrid)
        {
            return;
        }

        _isShowingGrid = true;
        FunctionHost.Content = null;
        FunctionHost.Visibility = Visibility.Collapsed;
        GridView.Visibility = Visibility.Visible;

        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.SetNavigationMode(false);
        }

        LogService.Info("功能页已返回卡片网格");
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

    /// <summary>
    /// 将 SVG Path 数据规范化为 WPF Path 可解析的显式命令形式。
    /// SVG 允许同一 c/C 命令后连续出现多组参数，WPF 的 Geometry.Parse 不接受这种隐式重复写法。
    /// </summary>
    private static string NormalizePathData(string pathData)
    {
        if (string.IsNullOrWhiteSpace(pathData))
        {
            return string.Empty;
        }

        const string tokenPattern = "(?<command>[AaCcHhLlMmQqSsTtVvZz])|(?<number>[-+]?(?:\\d*\\.\\d+|\\d+\\.?)(?:[eE][-+]?\\d+)?)";
        MatchCollection tokens = Regex.Matches(pathData, tokenPattern);
        if (tokens.Count == 0)
        {
            return pathData;
        }

        var result = new StringBuilder(pathData.Length + 32);
        var numbers = new List<string>();
        char currentCommand = '\0';

        foreach (Match token in tokens)
        {
            if (token.Groups["command"].Success)
            {
                AppendCommand(result, currentCommand, numbers);
                numbers.Clear();
                currentCommand = token.Groups["command"].Value[0];
                continue;
            }

            numbers.Add(token.Groups["number"].Value);
        }

        AppendCommand(result, currentCommand, numbers);
        return result.ToString();
    }

    /// <summary>按 WPF Path 语法要求输出一个命令及其参数组。</summary>
    private static void AppendCommand(StringBuilder result, char command, List<string> numbers)
    {
        if (command == '\0')
        {
            return;
        }

        int argumentCount = GetArgumentCount(command);
        if (argumentCount == 0)
        {
            result.Append(command);
            return;
        }

        if (numbers.Count == 0)
        {
            result.Append(command);
            return;
        }

        for (int offset = 0; offset < numbers.Count; offset += argumentCount)
        {
            int count = Math.Min(argumentCount, numbers.Count - offset);
            char outputCommand = command;

            // SVG 中 M/m 后续的坐标对表示线段；转换为 WPF 更容易解析的 L/l。
            if (offset > 0 && (command == 'M' || command == 'm'))
            {
                outputCommand = command == 'M' ? 'L' : 'l';
            }

            result.Append(outputCommand);
            for (int index = 0; index < count; index++)
            {
                result.Append(' ').Append(numbers[offset + index]);
            }
        }
    }

    /// <summary>返回 WPF Path 命令每组参数数量。</summary>
    private static int GetArgumentCount(char command)
    {
        return command switch
        {
            'A' or 'a' => 7,
            'C' or 'c' => 6,
            'S' or 's' or 'Q' or 'q' => 4,
            'H' or 'h' or 'V' or 'v' => 1,
            'L' or 'l' or 'M' or 'm' or 'T' or 't' => 2,
            _ => 0,
        };
    }

    /// <summary>功能卡片数据模型。</summary>
}


public sealed class FunctionCard
{
    /// <summary>卡片标题。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>简短介绍文本。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>图标 Path 数据（由 iconfont SVG 的 path d 转换而来）。</summary>
    public string IconData { get; set; } = string.Empty;

    /// <summary>是否为插件卡片（true 时排序优先，点击提示跳转插件市场）。</summary>
    public bool IsPlugin { get; init; }
}

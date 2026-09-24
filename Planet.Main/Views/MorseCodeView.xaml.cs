using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 摩斯密码编解码：左侧输入原文或电码，中间按钮触发，右侧显示结果。
/// 字母间以空格分隔，单词间以 / 分隔；大小写不敏感，支持字母、数字与常用标点。
/// </summary>
public partial class MorseCodeView : UserControl
{
    /// <summary>文字 → 电码映射表（标准国际摩斯电码：A-Z、0-9、常用标点）。</summary>
    private static readonly Dictionary<char, string> CharToMorse = new()
    {
        // ---- 字母 A-Z ----
        ['A'] = ".-",
        ['B'] = "-...",
        ['C'] = "-.-.",
        ['D'] = "-..",
        ['E'] = ".",
        ['F'] = "..-.",
        ['G'] = "--.",
        ['H'] = "....",
        ['I'] = "..",
        ['J'] = ".---",
        ['K'] = "-.-",
        ['L'] = ".-..",
        ['M'] = "--",
        ['N'] = "-.",
        ['O'] = "---",
        ['P'] = ".--.",
        ['Q'] = "--.-",
        ['R'] = ".-.",
        ['S'] = "...",
        ['T'] = "-",
        ['U'] = "..-",
        ['V'] = "...-",
        ['W'] = ".--",
        ['X'] = "-..-",
        ['Y'] = "-.--",
        ['Z'] = "--..",

        // ---- 数字 0-9 ----
        ['0'] = "-----",
        ['1'] = ".----",
        ['2'] = "..---",
        ['3'] = "...--",
        ['4'] = "....-",
        ['5'] = ".....",
        ['6'] = "-....",
        ['7'] = "--...",
        ['8'] = "---..",
        ['9'] = "----.",

        // ---- 常用标点 ----
        ['.'] = ".-.-.-",
        [','] = "--..--",
        ['?'] = "..--..",
        ['\''] = ".----.",
        ['!'] = "-.-.--",
        ['/'] = "-..-.",
        ['('] = "-.--.",
        [')'] = "-.--.-",
        ['&'] = ".-...",
        [':'] = "---...",
        [';'] = "-.-.-.",
        ['='] = "-...-",
        ['+'] = ".-.-.",
        ['-'] = "-....-",
        ['_'] = "..--.-",
        ['"'] = ".-..-.",
        ['$'] = "...-..-",
        ['@'] = ".--.-.",
    };

    /// <summary>电码 → 文字反向映射表（标准电码互不重复，可安全反转）。</summary>
    private static readonly Dictionary<string, char> MorseToChar = BuildReverseMap();

    public MorseCodeView()
    {
        InitializeComponent();
    }

    private static Dictionary<string, char> BuildReverseMap()
    {
        var map = new Dictionary<string, char>();
        foreach (KeyValuePair<char, string> pair in CharToMorse)
        {
            map[pair.Value] = pair.Key;
        }

        return map;
    }

    // ==== 编解码入口 ====

    /// <summary>编码：文字 → 电码（字母间空格分隔，单词间 / 分隔）。</summary>
    private void OnEncodeClick(object sender, RoutedEventArgs e)
    {
        string input = InputBox.Text ?? string.Empty;
        var words = new List<string>();
        var currentWord = new List<string>();
        int skipped = 0;

        foreach (char c in input)
        {
            // 空白字符作为单词分隔符（连续空白折叠为一个 /）
            if (char.IsWhiteSpace(c))
            {
                FlushWord(words, currentWord);
                continue;
            }

            if (CharToMorse.TryGetValue(char.ToUpperInvariant(c), out string? code))
            {
                currentWord.Add(code);
            }
            else
            {
                skipped++;
            }
        }

        FlushWord(words, currentWord);

        ResultBox.Text = string.Join(" / ", words);
        SetHint(skipped == 0
            ? $"编码完成：{words.Count} 个单词。"
            : $"编码完成：{words.Count} 个单词；已忽略 {skipped} 个不支持字符。");
        LogService.Info($"摩斯密码编码：{words.Count} 个单词（忽略 {skipped} 个字符）");
    }

    /// <summary>解码：电码 → 文字（/ 分隔单词，空格分隔字母；无法识别的编码以 ? 占位）。</summary>
    private void OnDecodeClick(object sender, RoutedEventArgs e)
    {
        string input = InputBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            ResultBox.Text = string.Empty;
            SetHint("请先在左侧输入要解码的电码。");
            return;
        }

        var result = new StringBuilder();
        var invalidSamples = new List<string>();
        int invalidCount = 0;

        string[] words = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (string word in words)
        {
            string[] tokens = word.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (MorseToChar.TryGetValue(token, out char ch))
                {
                    result.Append(ch);
                }
                else
                {
                    invalidCount++;
                    if (invalidSamples.Count < 3)
                    {
                        invalidSamples.Add(token);
                    }

                    result.Append('?');
                }
            }

            result.Append(' ');
        }

        ResultBox.Text = result.ToString().TrimEnd();

        if (invalidCount == 0)
        {
            SetHint("解码完成。");
        }
        else
        {
            SetHint($"解码完成：有 {invalidCount} 个无法识别的编码（已用 ? 占位）：{string.Join("、", invalidSamples)}");
        }

        LogService.Info($"摩斯密码解码：{words.Length} 个单词（无效编码 {invalidCount} 个）");
    }

    /// <summary>把当前累积的单词写入列表并清空（编码内部辅助）。</summary>
    private static void FlushWord(List<string> words, List<string> currentWord)
    {
        if (currentWord.Count == 0)
        {
            return;
        }

        words.Add(string.Join(' ', currentWord));
        currentWord.Clear();
    }

    /// <summary>更新底部提示行。</summary>
    private void SetHint(string message)
    {
        HintText.Text = message;
    }
}

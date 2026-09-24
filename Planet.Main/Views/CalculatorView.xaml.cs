using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Planet.Main.Services;

namespace Planet.Main.Views;

/// <summary>
/// 计算器：顶部只读显示框展示表达式与结果，中部按键网格输入，底部提示行反馈状态。
/// 支持四则运算、小数点与括号，求值使用 <see cref="DataTable.Compute(string, string?)"/>。
/// </summary>
public partial class CalculatorView : UserControl
{
    /// <summary>表达式白名单：数字、四则运算符、小数点、括号、百分号、π 与空格。</summary>
    private static readonly Regex SafeChars = new(@"^[0-9+\-*/(). %π]*$", RegexOptions.Compiled);

    /// <summary>把数字后的百分号展开为除以 100。</summary>
    private static readonly Regex NumberPercent = new(@"(?<value>\d+(?:\.\d+)?|\.\d+)\s*%", RegexOptions.Compiled);

    /// <summary>把括号后的百分号展开为除以 100。</summary>
    private static readonly Regex ParenthesisPercent = new(@"\)\s*%", RegexOptions.Compiled);

    private const string PiLiteral = "3.14159265358979323846";

    /// <summary>求值实例复用（DataTable 仅作表达式解析，无状态残留风险）。</summary>
    private static readonly DataTable Evaluator = new();

    /// <summary>是否刚完成一次求值（决定下一个按键是“续算”还是“开新式”）。</summary>
    private bool _justEvaluated;

    public CalculatorView()
    {
        InitializeComponent();
    }

    // ==== 按键入口 ====

    /// <summary>数字 / 运算符 / 括号 / 小数点按键：追加到显示框。</summary>
    private void OnKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string token } || string.IsNullOrEmpty(token))
        {
            return;
        }

        // 求值后：按数字 / π / 小数点 / 左括号开新式；按二元运算符或右括号续算。
        if (_justEvaluated)
        {
            bool startsNew = token is "(" or "." or "π" || char.IsDigit(token[0]);
            if (startsNew)
            {
                HistoryText.Text = string.Empty;
                DisplayBox.Text = string.Empty;
            }

            _justEvaluated = false;
        }

        DisplayBox.Text += token;
        SetHint(DefaultHint);
    }

    /// <summary>C 清空显示与状态。</summary>
    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        DisplayBox.Text = string.Empty;
        HistoryText.Text = string.Empty;
        _justEvaluated = false;
        SetHint(DefaultHint);
    }

    /// <summary>退格：删除末位字符。</summary>
    private void OnBackspaceClick(object sender, RoutedEventArgs e)
    {
        if (DisplayBox.Text.Length == 0)
        {
            return;
        }

        DisplayBox.Text = DisplayBox.Text[..^1];
        _justEvaluated = false;
        SetHint(DefaultHint);
    }

    /// <summary>百分号：仅允许跟在数字、π、右括号或已计算结果后。</summary>
    private void OnPercentClick(object sender, RoutedEventArgs e)
    {
        string current = DisplayBox.Text;
        if (string.IsNullOrEmpty(current)
            || current.EndsWith('%')
            || (!char.IsDigit(current[^1]) && current[^1] is not ')' and not 'π'))
        {
            SetHint("百分号必须跟在数字、π 或右括号后。");
            return;
        }

        DisplayBox.Text += "%";
        _justEvaluated = false;
        SetHint(DefaultHint);
    }

    /// <summary>正负号：切换当前完整表达式的正负。</summary>
    private void OnToggleSignClick(object sender, RoutedEventArgs e)
    {
        string current = DisplayBox.Text;
        DisplayBox.Text = current.StartsWith('-') ? current[1..] : "-" + current;
        _justEvaluated = false;
        SetHint(DefaultHint);
    }

    private void OnReciprocalClick(object sender, RoutedEventArgs e)
    {
        ApplyUnary("1/x", value => Math.Abs(value) < double.Epsilon
            ? throw new DivideByZeroException()
            : 1d / value);
    }

    private void OnSquareClick(object sender, RoutedEventArgs e)
    {
        ApplyUnary("x²", value => value * value);
    }

    private void OnSquareRootClick(object sender, RoutedEventArgs e)
    {
        ApplyUnary("√", value => value < 0
            ? throw new ArgumentOutOfRangeException(nameof(value), "负数没有实数平方根。")
            : Math.Sqrt(value));
    }

    /// <summary>= 求值：白名单 → π/% 预处理 → DataTable.Compute → 结果写回显示框。</summary>
    private void OnEqualsClick(object sender, RoutedEventArgs e)
    {
        string input = DisplayBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(input))
        {
            SetHint("请先输入表达式。");
            return;
        }

        if (!TryEvaluate(input, out object? value, out string error))
        {
            SetHint(error);
            LogService.Info($"计算器求值失败：{input} → {error}");
            return;
        }

        string result = FormatResult(value);
        HistoryText.Text = $"{input} =";
        DisplayBox.Text = result;
        _justEvaluated = true;
        SetHint($"计算完成：{input} = {result}");
        LogService.Info($"计算器求值：{input} = {result}");
    }

    /// <summary>统一求值入口：校验、展开 π/%、调用 DataTable，并拒绝非有限结果。</summary>
    private static bool TryEvaluate(string input, out object? value, out string error)
    {
        value = null;
        error = string.Empty;

        if (!SafeChars.IsMatch(input))
        {
            error = "表达式包含非法字符。";
            return false;
        }

        try
        {
            string normalized = input.Replace("π", PiLiteral, StringComparison.Ordinal);
            normalized = NumberPercent.Replace(normalized, "(${value}/100)");
            normalized = ParenthesisPercent.Replace(normalized, ")/100");

            if (!IsValidExpressionSyntax(normalized))
            {
                error = "表达式无效：请检查运算符、百分号和括号。";
                return false;
            }

            value = Evaluator.Compute(normalized, null);
            if (!IsFiniteNumber(value))
            {
                error = "错误：运算结果不是有限数，请检查是否除以 0。";
                return false;
            }

            return true;
        }
        catch (DivideByZeroException)
        {
            error = "错误：除数不能为 0。";
            return false;
        }
        catch (Exception ex)
        {
            error = $"表达式无效：{ex.Message}";
            return false;
        }
    }

    /// <summary>对当前完整表达式应用一元函数，并立即显示结果。</summary>
    private void ApplyUnary(string symbol, Func<double, double> operation)
    {
        string input = DisplayBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(input))
        {
            SetHint("请先输入要计算的数值或表达式。");
            return;
        }

        if (!TryEvaluate(input, out object? raw, out string error))
        {
            SetHint(error);
            return;
        }

        try
        {
            double value = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
            double result = operation(value);
            if (!double.IsFinite(result))
            {
                SetHint("错误：该函数返回了非有限结果。");
                return;
            }

            HistoryText.Text = $"{input} {symbol}";
            DisplayBox.Text = FormatResult(result);
            _justEvaluated = true;
            SetHint($"{symbol}({input}) = {DisplayBox.Text}");
        }
        catch (Exception ex)
        {
            SetHint($"无法计算 {symbol}：{ex.Message}");
        }
    }

    /// <summary>
    /// 校验表达式词法与括号结构。DataTable.Compute 会接受 1++2 等形式，
    /// 因此求值前先拒绝连续运算符、缺少数值、括号不配对等无效输入。
    /// </summary>
    private static bool IsValidExpressionSyntax(string expression)
    {
        bool expectOperand = true;
        bool unarySignUsed = false;
        int parenthesisDepth = 0;
        int index = 0;

        while (index < expression.Length)
        {
            char current = expression[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (char.IsDigit(current) || current == '.')
            {
                if (!expectOperand)
                {
                    return false;
                }

                int numberStart = index;
                int decimalPointCount = 0;
                while (index < expression.Length
                       && (char.IsDigit(expression[index]) || expression[index] == '.'))
                {
                    if (expression[index] == '.')
                    {
                        decimalPointCount++;
                    }

                    index++;
                }

                string number = expression[numberStart..index];
                if (decimalPointCount > 1 || number == ".")
                {
                    return false;
                }

                expectOperand = false;
                unarySignUsed = false;
                continue;
            }

            switch (current)
            {
                case '(':
                    if (!expectOperand)
                    {
                        return false;
                    }

                    parenthesisDepth++;
                    unarySignUsed = false;
                    break;

                case ')':
                    if (expectOperand || parenthesisDepth == 0)
                    {
                        return false;
                    }

                    parenthesisDepth--;
                    expectOperand = false;
                    unarySignUsed = false;
                    break;

                case '+':
                case '-':
                    if (expectOperand)
                    {
                        if (current is not ('+' or '-') || unarySignUsed)
                        {
                            return false;
                        }

                        unarySignUsed = true;
                    }
                    else
                    {
                        expectOperand = true;
                        unarySignUsed = false;
                    }

                    break;

                case '*':
                case '/':
                    if (expectOperand)
                    {
                        return false;
                    }

                    expectOperand = true;
                    unarySignUsed = false;
                    break;

                default:
                    return false;
            }

            index++;
        }

        return !expectOperand && parenthesisDepth == 0;
    }

    /// <summary>DataTable 在部分除零场景会返回 Infinity/NaN，而非抛出异常。</summary>
    private static bool IsFiniteNumber(object? value)
    {
        return value switch
        {
            double number => double.IsFinite(number),
            float number => float.IsFinite(number),
            _ => value is not null,
        };
    }

    // ==== 内部辅助 ====

    /// <summary>默认底部提示文本。</summary>
    private const string DefaultHint = "提示：支持 + - * /、括号、%、±、1/x、√、x²、π；C 清空，⌫ 删除末位，= 求值。";

    /// <summary>格式化求值结果：使用不变文化，保留最多 15 位有效数字。</summary>
    private static string FormatResult(object? raw)
    {
        return raw switch
        {
            null or DBNull => string.Empty,
            decimal dec => dec.ToString("G15", CultureInfo.InvariantCulture),
            double dbl => dbl.ToString("G15", CultureInfo.InvariantCulture),
            float flt => flt.ToString("G7", CultureInfo.InvariantCulture),
            _ => Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty,
        };
    }

    /// <summary>更新底部提示 / 状态行。</summary>
    private void SetHint(string message)
    {
        HintText.Text = message;
    }
}
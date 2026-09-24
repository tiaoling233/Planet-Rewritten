using System;
using System.Data;
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
    /// <summary>表达式白名单：数字、四则运算符、小数点、括号与空格（求值前的粗筛）。</summary>
    private static readonly Regex SafeChars = new("^[0-9+\\-*/(). ]*$", RegexOptions.Compiled);

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

        // 求值后：按数字 / 括号 / 小数点 → 开新表达式；按运算符 → 基于结果续算
        if (_justEvaluated)
        {
            bool startsNew = token is "(" or "." || char.IsDigit(token[0]);
            DisplayBox.Text = startsNew ? string.Empty : DisplayBox.Text;
            _justEvaluated = false;
        }

        DisplayBox.Text += token;
        SetHint(DefaultHint);
    }

    /// <summary>C 清空显示与状态。</summary>
    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        DisplayBox.Text = string.Empty;
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

    /// <summary>= 求值：白名单校验 → DataTable.Compute → 结果写回显示框。</summary>
    private void OnEqualsClick(object sender, RoutedEventArgs e)
    {
        string expression = DisplayBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(expression))
        {
            SetHint("请先输入表达式。");
            return;
        }

        if (!SafeChars.IsMatch(expression) || !IsValidExpressionSyntax(expression))
        {
            SetHint("表达式无效，仅支持数字、+ - * / 与成对括号。");
            LogService.Info("计算器求值被拒：表达式格式无效");
            return;
        }

        try
        {
            Object? raw = Evaluator.Compute(expression, null);
            if (!IsFiniteNumber(raw))
            {
                SetHint("错误：运算结果不是有限数，请检查是否除以 0。");
                LogService.Info($"计算器求值失败：{expression} → 非有限结果");
                return;
            }

            string result = FormatResult(raw);
            DisplayBox.Text = result;
            _justEvaluated = true;
            SetHint($"计算完成：{expression} = {result}");
            LogService.Info($"计算器求值：{expression} = {result}");
        }
        catch (DivideByZeroException)
        {
            SetHint("错误：除数不能为 0。");
            LogService.Info($"计算器除零错误：{expression}");
        }
        catch (Exception ex)
        {
            SetHint($"表达式无效：{ex.Message}");
            LogService.Info($"计算器求值失败：{expression} → {ex.Message}");
        }
    }

    /// <summary>
    /// 校验表达式词法与括号结构。DataTable.Compute 会接受 1++2 等形式，
    /// 因此求值前先拒绝连续运算符、缺少数值、括号不配对等无效输入。
    /// </summary>
    private static bool IsValidExpressionSyntax(string expression)
    {
        bool expectOperand = true;
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
                if (decimalPointCount > 1
                    || number == "."
                    || number.StartsWith('.') && number.Length == 1)
                {
                    return false;
                }

                expectOperand = false;
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
                    break;

                case ')':
                    if (expectOperand || parenthesisDepth == 0)
                    {
                        return false;
                    }

                    parenthesisDepth--;
                    break;

                case '+':
                case '-':
                case '*':
                case '/':
                    if (expectOperand)
                    {
                        return false;
                    }

                    expectOperand = true;
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
            double number => !double.IsNaN(number) && !double.IsInfinity(number),
            float number => !float.IsNaN(number) && !float.IsInfinity(number),
            decimal => true,
            _ => true,
        };
    }

    // ==== 内部辅助 ====

    /// <summary>默认底部提示文本。</summary>
    private const string DefaultHint = "提示：支持 + - * / 与括号；C 清空，⌫ 删除末位，= 求值。";

    /// <summary>格式化求值结果：去掉无意义的尾随零（如 6.0 → 6、0.30000…04 → 0.3）。</summary>
    private static string FormatResult(object? raw)
    {
        return raw switch
        {
            null or DBNull => string.Empty,
            decimal dec => dec.ToString("0.################"),
            double dbl => dbl.ToString("0.################"),
            _ => raw.ToString() ?? string.Empty,
        };
    }

    /// <summary>更新底部提示 / 状态行。</summary>
    private void SetHint(string message)
    {
        HintText.Text = message;
    }
}
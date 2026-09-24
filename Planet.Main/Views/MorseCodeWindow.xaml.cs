using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Planet.Main.Views;

/// <summary>
/// 摩斯密码编解码独立窗口：承载 <see cref="MorseCodeView"/>，
/// 提供无边框圆角外观、右上角关闭按钮与空白区域拖动。
/// </summary>
public partial class MorseCodeWindow : Window
{
    public MorseCodeWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnChromeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 在输入框/按钮上按下时不拖动窗口（交给控件自身处理光标与点击）
        if (e.OriginalSource is DependencyObject source
            && (FindAncestor<TextBoxBase>(source) is not null
                || FindAncestor<ButtonBase>(source) is not null))
        {
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // 窗口处于少数不允许拖动的状态时忽略，避免异常退出
            }
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Planet.Main.Views;

namespace Planet.Main.Services;

/// <summary>信息提醒类型（对应 todo「条形信息提醒」三种配色）。</summary>
public enum ToastType
{
    Green,
    Blue,
    Red,
}

/// <summary>
/// 条形信息提醒服务（单例）：负责 Toast 的位置计算、堆叠排布与生命周期。
/// 约束：仅在 UI 线程调用（WPF UI 事件回调天然满足）。
/// 规则：新 Toast 插入栈顶（左上角第一条位置），已有 Toast 以动画整体下移；
///       Toast 关闭后，其余 Toast 自动收拢上移。
/// </summary>
public static class NotificationService
{
    private const double ScreenMargin = 20;   // 距工作区左/上边缘
    private const double Gap = 12;            // 相邻 Toast 间距
    private const int SlideMs = 300;          // 滑入/滑出时长
    private const int ShiftMs = 250;          // 堆叠位移时长
    private const int StayMs = 3000;          // 停留时长
    private const double EstimatedHeight = 60; // 显示前无法获取实际高度时的估算值

    // 当前显示中的 Toast，index 0 为栈顶（最靠上）
    private static readonly List<ToastWindow> Active = new();

    public static void ShowMessage(string message, ToastType type)
    {
        var wa = SystemParameters.WorkArea;

        var toast = new ToastWindow
        {
            MessageText = message,
            ToastType = type,
        };

        // 显示前预测量内容尺寸：用于离屏初始 Left 与堆叠位移量
        // （Window 在 Show 之前 ActualWidth/Height 均为 0，只能 Measure 内容）。
        double width = 320;
        double height = EstimatedHeight;
        if (toast.Content is Border border)
        {
            border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (border.DesiredSize.Width > 0)
            {
                width = border.DesiredSize.Width;
                height = border.DesiredSize.Height;
            }
        }

        // 初始位置：Top = 栈顶槽位；Left = 工作区左边界外（随后滑入）
        toast.Left = wa.Left - width - 60;
        toast.Top = wa.Top + ScreenMargin;
        toast.MeasuredHeight = height;

        // 插入栈顶，其余 Toast 平滑下移
        Active.Insert(0, toast);
        Relayout(exclude: toast);

        toast.Closed += (_, _) =>
        {
            Active.Remove(toast);
            Relayout(exclude: null); // 收拢上移
        };
        // 加载后：滑入 + 用实际高度重算堆叠（消除预测量误差）
        toast.Loaded += (_, _) =>
        {
            toast.BeginEnter(wa.Left + ScreenMargin);
            Relayout(exclude: toast);
        };

        toast.Show();

        // 停留 3 秒后滑出并自动关闭
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(StayMs) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            toast.Dismiss();
        };
        timer.Start();
    }

    /// <summary>
    /// 重算堆叠布局：按列表顺序自上而下给每个 Toast 分配槽位并用动画移动。
    /// 位移动画不指定 From，从当前值平滑续接——即使某条 Toast 正在下移途中
    /// 又触发收拢/退出，多个动画也会同步进行，不闪烁不卡顿。
    /// </summary>
    private static void Relayout(ToastWindow? exclude)
    {
        var wa = SystemParameters.WorkArea;
        double top = wa.Top + ScreenMargin;

        foreach (var toast in Active)
        {
            if (!ReferenceEquals(toast, exclude))
            {
                toast.ShiftTo(top);
            }
            top += toast.LayoutHeight + Gap;
        }
    }
}

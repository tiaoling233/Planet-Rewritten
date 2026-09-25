using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Planet.Main.Views;

/// <summary>
/// 主页：动态问候语、用户名、实时日期与时间。
/// DispatcherTimer 仅在页面 Loaded 期间运行，离开主页后自动停止。
/// </summary>
public partial class HomeView : UserControl
{
    private readonly DispatcherTimer _clockTimer;
    private readonly string _userName;

    public HomeView()
    {
        InitializeComponent();

        _userName = Environment.UserName;
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _clockTimer.Tick += (_, _) => RefreshClock();

        // 仅在主页实际加载时运行；ContentControl 切走会触发 Unloaded 并停止计时器。
        Loaded += (_, _) =>
        {
            RefreshClock();
            _clockTimer.Start();
        };
        Unloaded += (_, _) => _clockTimer.Stop();
    }

    private void RefreshClock()
    {
        DateTime now = DateTime.Now;
        GreetingText.Text = $"{GetGreeting(now)}，{_userName}";
        DateText.Text = now.ToString("yyyy年MM月dd日");
        ClockText.Text = now.ToString("HH:mm:ss");
    }

    private static string GetGreeting(DateTime now)
    {
        int hour = now.Hour;
        if (hour is >= 5 and < 12)
        {
            return "早上好";
        }
        if (hour is >= 12 and < 14)
        {
            return "中午好";
        }
        if (hour is >= 14 and < 18)
        {
            return "下午好";
        }
        return "晚上好";
    }
}
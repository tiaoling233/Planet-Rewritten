using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Planet.Main.Views;

/// <summary>
/// 主页：动态问候语、用户名、实时日期与时间。
/// 使用 DispatcherTimer 每秒刷新，避免后台线程直接操作 UI。
/// </summary>
public partial class HomeView : UserControl
{
    private readonly DispatcherTimer _clockTimer;
    private readonly string _userName;

    public HomeView()
    {
        InitializeComponent();

        _userName = Environment.UserName;
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => RefreshClock();
        _clockTimer.Start();

        Loaded += (_, _) => RefreshClock();
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
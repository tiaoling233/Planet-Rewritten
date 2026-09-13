using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Planet.Main.Services;

namespace Planet.Main;

public partial class MainWindow : Window
{
    private static readonly Brush _pageTextBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x4A, 0x53));

    private readonly RadioButton[] _navButtons;

    public MainWindow()
    {
        InitializeComponent();
        _navButtons = new[] { BtnHome, BtnFunct, BtnMystery, BtnInfo, BtnSettings };

        Loaded += OnLoaded;
        Closed += (_, _) => LogService.Info("主窗口已关闭");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LogService.Info("主窗口已加载");
        SwitchPage(0);
    }

    private void OnNavButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button
            && int.TryParse(button.Tag?.ToString(), out int index))
        {
            SwitchPage(index);
        }
    }

    private void SwitchPage(int index)
    {
        for (int i = 0; i < _navButtons.Length; i++)
        {
            ((ToggleButton)_navButtons[i]).IsChecked = (i == index);
        }

        MainContent.Content = index switch
        {
            0 => new Views.HomeView(),
            1 => CreatePlaceholder("这是功能"),
            2 => new Views.MysteryView(),
            3 => new Views.InfoView(),
            4 => new Views.SettingsView(),
            _ => CreatePlaceholder("未知页面"),
        };
    }

    private static TextBlock CreatePlaceholder(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 20,
            Foreground = _pageTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private void OnHideClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    public void ShowDimOverlay(bool isVisible)
    {
        DimOverlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnSidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source
            && FindAncestor<RadioButton>(source) is not null)
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
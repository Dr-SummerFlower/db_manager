using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Styling;
using db_manager.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;


namespace db_manager.Views;

public partial class MainWindow : Window
{
    private TrayIcon? _trayIcon;
    private const string TrayLightIcon = "avares://db_manager/Assets/tray_light.png";
    private const string TrayDarkIcon = "avares://db_manager/Assets/tray_dark.png";
    private readonly DispatcherTimer _focusCheckTimer;
    // private bool _isAnimating;
    // private readonly TranslateTransform _slideTransform = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        // 设置窗口的渲染变换
        // RenderTransform = _slideTransform;

        // 固定窗口大小
        CanResize = false;
        Width = 300;
        Height = 450;
        MinWidth = 300;
        MinHeight = 450;
        MaxWidth = 300;
        MaxHeight = 450;

        // 任务栏不显示图标
        ShowInTaskbar = false;

        var notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };
        ((MainWindowViewModel)DataContext).SetNotificationManager(notificationManager);

        // 监听窗口状态改变
        if (_focusCheckTimer != null)
        {
            Activated += (_, _) => _focusCheckTimer.Stop();
            Deactivated += (_, _) => _focusCheckTimer.Start();
        }

        // 隐藏原生标题栏
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = -1;
        SystemDecorations = SystemDecorations.None;

        // 初始化托盘图标
        InitializeTrayIcon();

        // 设置初始主题
        SetTheme(ThemeVariant.Light);

        // 创建定时器检查窗口焦点状态
        _focusCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _focusCheckTimer.Tick += CheckWindowFocus;
        _focusCheckTimer.Start();

        Closing += (_, e) =>
        {
            e.Cancel = true;
            MainWindowViewModel.ExitApplication();
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        _focusCheckTimer.Stop();
        _trayIcon?.Dispose();
        base.OnClosed(e);
    }

    private void CheckWindowFocus(object? sender, EventArgs e)
    {
        // 当窗口不在焦点时自动最小化到托盘
        if (!IsActive &&
            WindowState == WindowState.Normal &&
            IsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(MinimizeToTray);
        }
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new TrayIcon
        {
            ToolTipText = "数据库管理器",
            Icon = new WindowIcon(AssetLoader.Open(new Uri(TrayLightIcon)))
        };

        // 创建简化后的菜单项
        var menu = new NativeMenu();

        // 显示主窗口菜单项
        var showItem = new NativeMenuItem("显示主窗口");
        showItem.Click += async (_, _) => await RestoreFromTray();
        menu.Add(showItem);

        // 退出菜单项
        var exitItem = new NativeMenuItem("退出");
        exitItem.Click += (_, _) =>
        {
            MainWindowViewModel.ExitApplication();
        };
        menu.Add(exitItem);

        _trayIcon.Menu = menu;

        // 设置托盘图标单击事件
        _trayIcon.Clicked += (_, _) => Dispatcher.UIThread.InvokeAsync(RestoreFromTray);

        // 设置托盘图标到应用程序
        if (Application.Current == null) return;
        var trayIcons = new TrayIcons { _trayIcon };
        TrayIcon.SetIcons(Application.Current, trayIcons);
    }

    // 从托盘恢复窗口
    private Task RestoreFromTray()
    {
        // if (_isAnimating) return;
        // _isAnimating = true;
        //
        // try
        // {
            // 重置变换位置
            // _slideTransform.Y = 0;

            // 确保窗口显示在右下角
            SetWindowPositionToBottomRight();

            // 显示窗口但不在任务栏显示
            Show();
            WindowState = WindowState.Normal;
            Topmost = true; // 确保窗口在前

            // // 创建动画
            // var animation = new Animation
            // {
            //     Duration = TimeSpan.FromMilliseconds(300),
            //     Easing = new CubicEaseOut(),
            //     Children =
            //     {
            //         new KeyFrame
            //         {
            //             Cue = new Cue(0.0),
            //             Setters =
            //             {
            //                 new Setter(TranslateTransform.YProperty, Height)
            //             }
            //         },
            //         new KeyFrame
            //         {
            //             Cue = new Cue(1.0),
            //             Setters =
            //             {
            //                 new Setter(TranslateTransform.YProperty, 0.0)
            //             }
            //         }
            //     }
            // };
            //
            // // 运行动画
            // await animation.RunAsync(this, CancellationToken.None);

            // 完成恢复
            Activate();
            Focus(); // 确保获得焦点
            return Task.CompletedTask;
        // }
        // finally
        // {
        //     _isAnimating = false;
        //     Topmost = false; // 恢复正常层级
        // }
    }

    // 最小化到托盘
    private Task MinimizeToTray()
    {
        // if (_isAnimating) return;
        // _isAnimating = true;

        // try
        // {
            // // 创建滑动动画
            // var animation = new Animation
            // {
            //     Duration = TimeSpan.FromMilliseconds(300),
            //     Easing = new CubicEaseIn(),
            //     Children =
            //     {
            //         new KeyFrame
            //         {
            //             Cue = new Cue(0.0),
            //             Setters =
            //             {
            //                 new Setter(TranslateTransform.YProperty, 0.0)
            //             }
            //         },
            //         new KeyFrame
            //         {
            //             Cue = new Cue(1.0),
            //             Setters =
            //             {
            //                 new Setter(TranslateTransform.YProperty, Height)
            //             }
            //         }
            //     }
            // };
            //
            // // 运行动画
            // await animation.RunAsync(this, CancellationToken.None);

            // 完成最小化
            WindowState = WindowState.Minimized;
            Hide();
            return Task.CompletedTask;
        // }
        // finally
        // {
        //     _isAnimating = false;
        // }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        SetWindowPositionToBottomRight();
    }

    // 设置窗口位置到右下角
    private void SetWindowPositionToBottomRight()
    {
        // 获取主屏幕信息 - 使用当前窗口所在的屏幕
        var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary ?? Screens.All[0];
        var workingArea = screen.WorkingArea;

        // 添加边距防止黑边
        const int margin = 2;

        // 计算右下角位置（考虑任务栏位置）
        var newX = workingArea.X + workingArea.Width - (int)Width - margin;
        var newY = workingArea.Y + workingArea.Height - (int)Height - margin;

        // 确保位置在屏幕内
        newX = Math.Max(workingArea.X, newX);
        newY = Math.Max(workingArea.Y, newY);

        Position = new PixelPoint(newX, newY);
    }

    private void SetTheme(ThemeVariant theme)
    {
        // 设置应用主题
        if (Application.Current != null) Application.Current.RequestedThemeVariant = theme;

        // 更新标题栏背景色
        if (Application.Current != null)
            Application.Current.Resources["ThemeBackgroundBrush"] =
                theme == ThemeVariant.Dark
                    ? Application.Current.Resources["DarkBackground"]
                    : Application.Current.Resources["LightBackground"];

        // 更新标题栏背景色
        if (this.FindControl<Border>("TitleBar") is { } titleBar)
        {
            titleBar.Background = theme == ThemeVariant.Dark
                ? new SolidColorBrush(Color.Parse("#202020"))
                : new SolidColorBrush(Color.Parse("#F0F0F0"));
        }

        // 更新托盘图标
        UpdateTrayIcon(theme);
    }

    // 更新托盘图标
    private void UpdateTrayIcon(ThemeVariant theme)
    {
        if (_trayIcon != null)
        {
            _trayIcon.Icon = new WindowIcon(
                AssetLoader.Open(new Uri(
                    theme == ThemeVariant.Dark ? TrayDarkIcon : TrayLightIcon
                ))
            );
        }
    }

    private void OnLightThemeClick(object? sender, RoutedEventArgs? e)
    {
        SetTheme(ThemeVariant.Light);
        ((MainWindowViewModel)DataContext!).ShowNotification("主题切换", "已切换到亮色主题");
    }

    private void OnDarkThemeClick(object? sender, RoutedEventArgs? e)
    {
        SetTheme(ThemeVariant.Dark);
        ((MainWindowViewModel)DataContext!).ShowNotification("主题切换", "已切换到暗色主题");
    }

    private void OnSettingsButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { ContextMenu: not null } button) return;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Open();
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnViewLogsClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (Directory.Exists(logDir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logDir,
                    UseShellExecute = true
                });
            }
            else
            {
                ((MainWindowViewModel)DataContext!).ShowNotification("日志", "日志目录不存在");
            }
        }
        catch (Exception ex)
        {
            ((MainWindowViewModel)DataContext!).ShowNotification("错误", $"打开日志失败: {ex.Message}");
        }
    }
}
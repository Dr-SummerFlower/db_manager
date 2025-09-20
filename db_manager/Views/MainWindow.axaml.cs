using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using db_manager.ViewModels;

namespace db_manager.Views;

public partial class MainWindow : Window
{
  private const int MarginPx = 2;
  private const string TrayLightIcon = "avares://db_manager/Assets/tray_light.png";
  private const string TrayDarkIcon = "avares://db_manager/Assets/tray_dark.png";
  private readonly DispatcherTimer _focusCheckTimer;
  private TrayIcon? _trayIcon;

  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainWindowViewModel();

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

    ScalingChanged += OnScalingChanged;
    PositionChanged += OnPositionChanged;

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
      Dispatcher.UIThread.InvokeAsync(MinimizeToTray);
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
    exitItem.Click += (_, _) => { MainWindowViewModel.ExitApplication(); };
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
    SetWindowPositionToBottomRight();

    // 显示窗口但不在任务栏显示
    Show();
    WindowState = WindowState.Normal;
    Topmost = true;

    // 完成恢复
    Activate();
    Focus(); // 确保获得焦点
    return Task.CompletedTask;
  }

  // 最小化到托盘
  private Task MinimizeToTray()
  {
    // 完成最小化
    WindowState = WindowState.Minimized;
    Hide();
    return Task.CompletedTask;
  }

  protected override void OnOpened(EventArgs e)
  {
    base.OnOpened(e);
    AdjustSizeForDpiAndFit();
    SetWindowPositionToBottomRight();
  }

  private void OnScalingChanged(object? sender, EventArgs e)
  {
    AdjustSizeForDpiAndFit();
    SetWindowPositionToBottomRight();
  }

  private void OnPositionChanged(object? sender, PixelPointEventArgs e)
  {
    // 跨屏移动 / 工作区改变时，重新收敛尺寸+定位
    AdjustSizeForDpiAndFit();
    SetWindowPositionToBottomRight();
  }

  private void AdjustSizeForDpiAndFit()
  {
    // 目标设计尺寸（DIP）
    const double desiredDipW = 300d;
    const double desiredDipH = 450d;

    var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary ?? Screens.All[0];
    var wa = screen.WorkingArea; // 像素(px)
    var scale = RenderScaling <= 0 ? 1.0 : RenderScaling; // px / dip

    // 将工作区从 px 转成 DIP，用于和 Width/Height 比较
    var maxDipW = Math.Floor(wa.Width / scale) - MarginPx * 2.0 / scale;
    var maxDipH = Math.Floor(wa.Height / scale) - MarginPx * 2.0 / scale;

    // 设定最小的可接受窗口（防止被压得太小）
    const double minDipW = 260d;
    const double minDipH = 380d;

    // 实际采用的 DIP 尺寸 = 在 [min, max] 中夹紧设计尺寸
    var finalDipW = Math.Clamp(desiredDipW, minDipW, Math.Max(minDipW, maxDipW));
    var finalDipH = Math.Clamp(desiredDipH, minDipH, Math.Max(minDipH, maxDipH));

    // 应用到窗口大小，并把 Min/Max 固定为同值，继续保持“不可拉伸”的产品设计
    Width = finalDipW;
    Height = finalDipH;
    MinWidth = finalDipW;
    MinHeight = finalDipH;
    MaxWidth = finalDipW;
    MaxHeight = finalDipH;
  }

  // 设置窗口位置到右下角
  private void SetWindowPositionToBottomRight()
  {
    var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary ?? Screens.All[0];
    var wa = screen.WorkingArea; // px
    var scale = RenderScaling <= 0 ? 1.0 : RenderScaling;

    var widthPx = (int)Math.Round(Width * scale);
    var heightPx = (int)Math.Round(Height * scale);

    var newX = wa.X + wa.Width - widthPx - MarginPx;
    var newY = wa.Y + wa.Height - heightPx - MarginPx;

    // 确保在屏内
    newX = Math.Max(wa.X, newX);
    newY = Math.Max(wa.Y, newY);

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
      titleBar.Background = theme == ThemeVariant.Dark
        ? new SolidColorBrush(Color.Parse("#202020"))
        : new SolidColorBrush(Color.Parse("#F0F0F0"));

    // 更新托盘图标
    UpdateTrayIcon(theme);
  }

  // 更新托盘图标
  private void UpdateTrayIcon(ThemeVariant theme)
  {
    if (_trayIcon != null)
      _trayIcon.Icon = new WindowIcon(
        AssetLoader.Open(new Uri(
          theme == ThemeVariant.Dark ? TrayDarkIcon : TrayLightIcon
        ))
      );
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
        Process.Start(new ProcessStartInfo
        {
          FileName = logDir,
          UseShellExecute = true
        });
      else
        ((MainWindowViewModel)DataContext!).ShowNotification("日志", "日志目录不存在");
    }
    catch (Exception ex)
    {
      ((MainWindowViewModel)DataContext!).ShowNotification("错误", $"打开日志失败: {ex.Message}");
    }
  }
}

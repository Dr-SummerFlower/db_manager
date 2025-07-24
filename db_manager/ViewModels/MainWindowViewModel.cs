using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using db_manager.Models;
using db_manager.Services;
using db_manager.Utilities;

namespace db_manager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private WindowNotificationManager? _notificationManager;
    private readonly ConfigService _configService = new();
    private readonly List<CardItem> _runningCards = [];

    public ObservableCollection<CardItem> Cards { get; } = [];

    public MainWindowViewModel()
    {
        LogManager.Log("应用程序启动");
        _configService.ValidatePaths();
        LoadDatabaseConfigs();
    }

    private void LoadDatabaseConfigs()
    {
        Cards.Clear();
        var configs = _configService.LoadDatabaseConfigs();

        foreach (var card in from config in configs let isRunning = DatabaseManager.IsRunning(config) select new CardItem
                 {
                     Config = config,
                     LogoPath = LoadIconBitmap(config.Type), // 使用位图加载方法
                     Name = config.Name,
                     Status = isRunning ? "运行中" : "已停止",
                     State = isRunning ? ButtonState.Stop : ButtonState.Start,
                 })
        {
            card.ToggleCommand = new RelayCommand(() => ToggleDatabase(card));
            Cards.Add(card);
        }
    }

    private Bitmap LoadIconBitmap(DatabaseType type)
    {
        var uri = type switch
        {
            DatabaseType.MySql => "avares://db_manager/Assets/mysql.png",
            DatabaseType.MongoDb => "avares://db_manager/Assets/mongodb.png",
            DatabaseType.Redis => "avares://db_manager/Assets/redis.png",
            _ => "avares://db_manager/Assets/default.png"
        };

        try
        {
            return new Bitmap(AssetLoader.Open(new Uri(uri)));
        }
        catch
        {
            // 如果找不到资源，返回默认图标
            return new Bitmap(AssetLoader.Open(new Uri("avares://db_manager/Assets/default.png")));
        }
    }

    public void SetNotificationManager(WindowNotificationManager manager) =>
        _notificationManager = manager;

    public void ShowNotification(string title, string message) =>
        _notificationManager?.Show(new Notification(title, message));

    private void ToggleDatabase(CardItem card)
    {
        if (card.State == ButtonState.Loading) return;

        var isStartOperation = card.State == ButtonState.Start;
        card.State = ButtonState.Loading;
        card.Status = isStartOperation ? "启动中..." : "停止中...";

        Task.Run(() =>
        {
            var config = card.Config!;
            bool success;
            var operation = isStartOperation ? "启动" : "停止";

            try
            {
                success = isStartOperation
                    ? DatabaseManager.StartDatabase(config)
                    : DatabaseManager.StopDatabase(config);
            }
            catch (Exception ex)
            {
                LogManager.Log($"{operation} {config.Name} 时出错: {ex.Message}");
                Dispatcher.UIThread.InvokeAsync(() =>
                    card.Status = $"{operation}失败: {ex.Message}");
                return;
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (success)
                {
                    // 更新运行状态列表
                    if (isStartOperation)
                    {
                        _runningCards.Add(card);
                    }
                    else
                    {
                        _runningCards.Remove(card);
                    }
                    card.State = isStartOperation ? ButtonState.Stop : ButtonState.Start;
                    card.Status = isStartOperation ? "运行中" : "已停止";
                    ShowNotification("操作成功", $"{config.Name} 已{operation}");
                }
                else
                {
                    card.State = isStartOperation ? ButtonState.Start : ButtonState.Stop;
                    card.Status = "操作失败";
                    ShowNotification("操作失败", $"{config.Name} {operation}失败");
                }
            });
        });
    }

    public void StopAllDatabases()
    {
        foreach (var card in _runningCards.ToList()) // 使用ToList避免修改集合
        {
            if (card.State == ButtonState.Stop)
            {
                card.State = ButtonState.Loading;
                card.Status = "停止中...";

                // 直接停止数据库
                Task.Run(() =>
                {
                    try
                    {
                        bool success = DatabaseManager.StopDatabase(card.Config);
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (success)
                            {
                                card.State = ButtonState.Start;
                                card.Status = "已停止";
                                _runningCards.Remove(card);
                            }
                            else
                            {
                                card.State = ButtonState.Stop;
                                card.Status = "停止失败";
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log($"停止{card.Name}时出错: {ex.Message}");
                    }
                });
            }
        }
    }

    public static void ExitApplication(){
        // 先停止所有数据库
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow.DataContext: MainWindowViewModel vm })
        {
            vm.StopAllDatabases();
        }

        // 延迟退出以确保停止命令执行
        Task.Delay(1000).ContinueWith(_ =>
        {
            Environment.Exit(0);
        });
    }
}
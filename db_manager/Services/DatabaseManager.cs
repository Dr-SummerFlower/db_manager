using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using db_manager.Models;

namespace db_manager.Services;

public abstract class DatabaseManager
{
  public static bool StartDatabase(DatabaseConfig config)
  {
    try
    {
      var baseDir = AppDomain.CurrentDomain.BaseDirectory;
      var exeFullPath = Path.GetFullPath(Path.Combine(baseDir, config.ExePath));
      var configFullPath = Path.GetFullPath(Path.Combine(baseDir, config.ConfigPath));
      var pidPath = Path.GetFullPath(Path.Combine(baseDir, config.PidPath));

      // 验证路径
      if (!File.Exists(exeFullPath) || !File.Exists(configFullPath))
        return false;

      // 确保目录存在
      Directory.CreateDirectory(Path.GetDirectoryName(pidPath)!);
      Directory.CreateDirectory(Path.Combine(baseDir, config.DataPath));
      Directory.CreateDirectory(Path.Combine(baseDir, config.LogPath));

      var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          WorkingDirectory = Path.GetFullPath(Path.Combine(baseDir, config.Bin)),
          UseShellExecute = false,
          CreateNoWindow = true,
          RedirectStandardOutput = true,
          RedirectStandardError = true
        }
      };

      switch (config.Type)
      {
        case DatabaseType.MySql:
          InitializeMySql(config, baseDir);
          process.StartInfo.FileName = exeFullPath;
          process.StartInfo.Arguments = $"--defaults-file=\"{configFullPath}\"";
          break;

        case DatabaseType.MongoDb:
          process.StartInfo.FileName = exeFullPath;
          process.StartInfo.Arguments = $"--config \"{configFullPath}\"";
          break;

        case DatabaseType.Redis:
          process.StartInfo.FileName = exeFullPath;
          process.StartInfo.Arguments = "redis.conf";
          CleanPidFile(pidPath);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      if (!process.Start()) return false;

      // 等待进程初始化
      Thread.Sleep(3000);

      if (process.HasExited) return false;
      File.WriteAllText(pidPath, process.Id.ToString());
      return true;
    }
    catch (Exception ex)
    {
      LogManager.Log($"启动{config.Name}出错: {ex.Message}");
      return false;
    }
  }

  private static void InitializeMySql(DatabaseConfig config, string baseDir)
  {
    var dataPath = Path.GetFullPath(Path.Combine(baseDir, config.DataPath));
    var mysqlSystemDir = Path.Combine(dataPath, "mysql");

    if (Directory.Exists(mysqlSystemDir)) return;

    Directory.CreateDirectory(dataPath);
    var initProcess = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = Path.Combine(baseDir, config.Bin, "mysqld.exe"),
        Arguments = $"--initialize-insecure --datadir=\"{dataPath}\"",
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
      }
    };

    initProcess.Start();
    initProcess.WaitForExit(30000);
  }

  private static void CleanPidFile(string pidPath)
  {
    try
    {
      if (File.Exists(pidPath)) File.Delete(pidPath);
    }
    catch
    {
      /* 忽略删除异常 */
    }
  }

  public static bool StopDatabase(DatabaseConfig config)
  {
    try
    {
      var baseDir = AppDomain.CurrentDomain.BaseDirectory;
      var pidPath = Path.Combine(baseDir, config.PidPath);

      if (!File.Exists(pidPath) || !int.TryParse(File.ReadAllText(pidPath), out var pid))
        return KillProcessByName(config);
      if (!KillProcessTree(pid)) return KillProcessByName(config);
      File.Delete(pidPath);
      return true;
    }
    catch (Exception ex)
    {
      LogManager.Log($"停止{config.Name}出错: {ex.Message}");
      return false;
    }
  }

  private static bool KillProcessTree(int rootPid)
  {
    try
    {
      using var process = new Process();
      process.StartInfo.FileName = "taskkill";
      process.StartInfo.Arguments = $"/PID {rootPid} /T /F";
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.Start();
      return process.WaitForExit(5000) && process.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }

  private static bool KillProcessByName(DatabaseConfig config)
  {
    var processName = config.Type switch
    {
      DatabaseType.MySql => "mysqld",
      DatabaseType.MongoDb => "mongod",
      DatabaseType.Redis => "redis-server",
      _ => null
    };

    if (string.IsNullOrEmpty(processName)) return false;

    using var process = new Process();
    process.StartInfo.FileName = "taskkill";
    process.StartInfo.Arguments = $"/IM {processName}.exe /F /T";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;
    process.Start();
    return process.WaitForExit(5000) && process.ExitCode == 0;
  }

  public static bool IsRunning(DatabaseConfig config)
  {
    var pidPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.PidPath);

    if (File.Exists(pidPath) && int.TryParse(File.ReadAllText(pidPath), out var pid))
      try
      {
        using var process = Process.GetProcessById(pid);
        return !process.HasExited;
      }
      catch
      {
        /* 进程不存在 */
      }

    // 后备检查
    var processName = config.Type switch
    {
      DatabaseType.MySql => "mysqld",
      DatabaseType.MongoDb => "mongod",
      DatabaseType.Redis => "redis-server",
      _ => null
    };

    return !string.IsNullOrEmpty(processName) &&
           Process.GetProcessesByName(processName).Length > 0;
  }
}

using System;
using System.IO;

namespace db_manager.Services;

public static class LogManager
{
    public static void Log(string message)
    {
        try
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            var logPath = Path.Combine(logDir, $"db_manager_{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch { /* 忽略日志记录失败 */ }
    }
}
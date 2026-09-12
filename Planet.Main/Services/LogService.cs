using System.Diagnostics;
using System.IO;

namespace Planet.Main.Services;

/// <summary>
/// 日志服务：每次启动写一个独立的文本日志文件到 Data\Logs 下
/// （文件名为 Planet_yyyyMMdd-HHmmss.log），并按天数清理过期日志。
///
/// 线程安全：所有写操作经由同一把锁串行化。
/// 后续阶段将在此基础上实现应用内日志查看器。
/// </summary>
public static class LogService
{
    private static readonly object SyncRoot = new();

    private static string? _currentLogFile;
    private static int _retentionDays = 30;

    /// <summary>当前启动会话的日志文件完整路径；未初始化时为 null。</summary>
    public static string? CurrentLogFile => _currentLogFile;

    /// <summary>
    /// 初始化日志服务：创建日志目录、清理过期日志并打开本次会话的日志文件。
    /// 可重复调用，只有首次调用生效（以先到者为准）。
    /// </summary>
    public static void Initialize(string logDirectory, int retentionDays = 30)
    {
        lock (SyncRoot)
        {
            if (_currentLogFile is not null)
            {
                return;
            }

            _retentionDays = Math.Max(1, retentionDays);
            Directory.CreateDirectory(logDirectory);
            CleanupExpiredLogs(logDirectory);

            _currentLogFile = Path.Combine(logDirectory, $"Planet_{DateTime.Now:yyyyMMdd-HHmmss}.log");
            WriteCore("INFO", "日志系统已初始化，日志文件：" + _currentLogFile);
        }
    }

    /// <summary>记录一条信息级别日志。</summary>
    public static void Info(string message) => WriteCore("INFO", message);

    /// <summary>记录一条警告级别日志。</summary>
    public static void Warn(string message) => WriteCore("WARN", message);

    /// <summary>记录一条错误级别日志，可附带异常对象。</summary>
    public static void Error(string message, Exception? exception = null)
    {
        WriteCore("ERROR", message + FormatException(exception));
    }

    private static void WriteCore(string level, string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";

        lock (SyncRoot)
        {
            if (_currentLogFile is not null)
            {
                try
                {
                    File.AppendAllText(_currentLogFile, line + Environment.NewLine);
                }
                catch
                {
                    // 日志写入失败不应导致应用崩溃。
                }
            }
        }

        // 便于在 VS Code / 调试器中直接看到输出。
        Trace.WriteLine(line);
    }

    /// <summary>删除超过保留天数的历史日志文件。</summary>
    private static void CleanupExpiredLogs(string logDirectory)
    {
        DateTime deadline = DateTime.Now.AddDays(-_retentionDays);
        try
        {
            foreach (string file in Directory.EnumerateFiles(logDirectory, "Planet_*.log"))
            {
                try
                {
                    if (File.GetLastWriteTime(file) < deadline)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // 单个文件删除失败不影响整体清理。
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
            // 目录可能尚未创建，忽略。
        }
    }

    private static string FormatException(Exception? exception) =>
        exception is null ? string.Empty : Environment.NewLine + exception;
}
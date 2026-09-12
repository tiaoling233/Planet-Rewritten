using System.IO;
using System.Linq;
using System.Text;

namespace Planet.Main.Services;

/// <summary>
/// 设置服务：以纯文本键值对形式读写 Data 目录下的配置文件（settings.txt）。
///
/// 文件格式约定：
///   - 每行一个条目：key=value（等号两侧允许空格，读取时自动裁剪）；
///   - 以 # 或 ; 开头的行视为注释；
///   - 键名不区分大小写。
///
/// 线程安全：所有读写经由同一把锁串行化；Set 立即落盘。
/// </summary>
public static class SettingsService
{
    private static readonly object SyncRoot = new();

    private static readonly Dictionary<string, string> Values =
        new(StringComparer.OrdinalIgnoreCase);

    private static string _filePath = string.Empty;

    /// <summary>当前设置文件完整路径；未加载时为空字符串。</summary>
    public static string FilePath => _filePath;

    /// <summary>是否已成功加载设置文件（无论文件原本是否存在）。</summary>
    public static bool IsLoaded { get; private set; }

    /// <summary>
    /// 从指定文件加载设置。若文件不存在则视为空设置（不报错），
    /// 后续首次 Set 时会自动创建文件。
    /// </summary>
    public static void Load(string filePath)
    {
        lock (SyncRoot)
        {
            _filePath = filePath;
            Values.Clear();
            IsLoaded = false;

            if (!File.Exists(filePath))
            {
                IsLoaded = true;
                return;
            }

            foreach (string rawLine in File.ReadAllLines(filePath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == ';')
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = line[..separatorIndex].Trim();
                string value = line[(separatorIndex + 1)..].Trim();
                Values[key] = value;
            }

            IsLoaded = true;
        }
    }

    /// <summary>读取键对应的值；不存在时返回 null。</summary>
    public static string? Get(string key) => Get(key, null);

    /// <summary>读取键对应的值；不存在时返回默认值。</summary>
    public static string? Get(string key, string? defaultValue)
    {
        lock (SyncRoot)
        {
            return Values.TryGetValue(key, out string? value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// 读取键对应的值；不存在时写入默认值并立即落盘。
    /// 适用于需要"首次运行建立默认配置"的场景。
    /// </summary>
    public static string GetOrCreate(string key, string defaultValue)
    {
        lock (SyncRoot)
        {
            if (Values.TryGetValue(key, out string? existing))
            {
                return existing;
            }

            Values[key] = defaultValue;
            SaveLocked();
            return defaultValue;
        }
    }

    /// <summary>设置键值并立即写入文件。</summary>
    public static void Set(string key, string value)
    {
        lock (SyncRoot)
        {
            Values[key] = value;
            SaveLocked();
        }
    }

    /// <summary>移除指定键（若存在）并立即写入文件。</summary>
    public static void Remove(string key)
    {
        lock (SyncRoot)
        {
            if (Values.Remove(key))
            {
                SaveLocked();
            }
        }
    }

    /// <summary>把当前全部设置写入文件（若尚未加载则视为未初始化，直接忽略）。</summary>
    public static void Save()
    {
        lock (SyncRoot)
        {
            SaveLocked();
        }
    }

    /// <summary>调用方必须持有 SyncRoot 锁。</summary>
    private static void SaveLocked()
    {
        if (!IsLoaded || _filePath.Length == 0)
        {
            return;
        }

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new StringBuilder();
        builder.AppendLine("# Planet 设置文件（键值对格式：key=value）");
        builder.AppendLine("# 删除本文件可恢复全部默认设置。");
        foreach (KeyValuePair<string, string> pair in Values.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(pair.Key).Append('=').AppendLine(pair.Value);
        }

        File.WriteAllText(_filePath, builder.ToString(), Encoding.UTF8);
    }
}
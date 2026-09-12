using System.Diagnostics;
using System.Runtime.InteropServices;
using Planet.Layout;

namespace Planet.Launcher;

/// <summary>
/// Planet 便携版启动器。
///
/// 职责（保持轻量、无 UI 框架依赖）：
///   1. 以自身所在目录为便携版根目录；
///   2. 校验 Data 文件夹及软件本体 Planet.Main.exe 是否存在；
///   3. 不存在时补齐 Data 目录结构，存在但缺本体时给出明确错误提示；
///   4. 以 --root=&lt;根目录&gt; 参数拉起 Data\Planet.Main.exe 后退出。
/// </summary>
internal static class Program
{
    // 原生 MessageBox（user32.dll）：避免为一条错误提示引入整个 UI 框架。
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    private const uint MbIconError = 0x00000010;
    private const uint MbOk = 0x00000000;

    private static int Main()
    {
        // 便携版根目录 = 启动器所在目录（AppContext.BaseDirectory 以"\"结尾）。
        string root = Path.GetFullPath(AppContext.BaseDirectory);

        try
        {
            string dataDirectory = Path.Combine(root, AppLayout.DataFolderName);
            string mainExecutable = Path.Combine(dataDirectory, AppLayout.MainExecutableName);

            // 先补齐 Data 目录结构，保证即使首次解压缺少空文件夹也能正常运行。
            EnsureDataStructure(dataDirectory);

            if (!File.Exists(mainExecutable))
            {
                _ = MessageBoxW(
                    IntPtr.Zero,
                    "未找到软件本体 Planet.Main.exe，无法启动！" + Environment.NewLine +
                    "请确认 Planet.exe 与 Data 文件夹位于同一目录下，且未修改 Data 文件夹内容。",
                    "Planet - 启动失败",
                    MbOk | MbIconError);
                return 1;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = mainExecutable,
                UseShellExecute = false,
                WorkingDirectory = dataDirectory,
            };
            startInfo.ArgumentList.Add($"--root={root}");

            using (Process.Start(startInfo))
            {
                // 启动器不驻留内存，启动软件本体后随即退出。
            }

            return 0;
        }
        catch (Exception exception)
        {
            _ = MessageBoxW(
                IntPtr.Zero,
                "启动过程中发生异常：" + Environment.NewLine + exception.Message,
                "Planet - 启动失败",
                MbOk | MbIconError);
            return 2;
        }
    }

    /// <summary>确保 Data 目录及其子目录存在（幂等操作）。</summary>
    private static void EnsureDataStructure(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        foreach (string subfolder in AppLayout.DataSubfolders)
        {
            Directory.CreateDirectory(Path.Combine(dataDirectory, subfolder));
        }
    }
}
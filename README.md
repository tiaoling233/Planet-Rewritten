# Planet

> 一款面向 Windows 的桌面效率工具。原始 VBS 版（v2.2）由 **乱码vvv** 开发，
> 本仓库为使用 **C# / .NET 8 + WPF** 的完整重写版（代号 Planet-Rewritten）。

## ✨ 功能总览

- **主页**：问候语、日期时间、用户信息（按当前时间动态显示）
- **??? 页**：今日人品、千万别点（彩蛋）
- **信息页**：开发者信息、致谢名单、开源声明（可展开卡片）
- **设置页**：主题设置、下载源设置、标签页选择器视效

> 完整的开发任务清单见 `ref/todo.md`（位于仓库外的原始参考材料，仅存档不修改）。

## 💻 系统要求

| 项目 | 要求 |
| --- | --- |
| 操作系统 | Windows 10 1703 及以上 / Windows 11 |
| 运行时 | .NET 8 Desktop Runtime（使用 `-SelfContained` 发布则无需安装） |

## 🛠 技术栈

- **语言/平台**：C# 12 / .NET 8
- **UI 框架**：WPF（XAML），`net8.0-windows` 目标框架
- **第三方依赖**：[HandyControl](https://github.com/HandyOrg/HandyControl) 3.5.1（现代化 UI 控件库）
- **版本管理**：Git；编译输出与本地缓存一律不入库

## 📁 目录结构

本仓库根目录为 `src/`，其与工作区的关系如下：

```dir
D:\AAAPlanet-Rewritten\
├── ref\                 # 原始 VBS 代码与 todo.md（只读存档，不入库）
├── src\                 # 全部源代码（Git 仓库根目录）
│   ├── Planet.sln       # 解决方案
│   ├── Planet.Launcher\ # 启动器工程 → 输出 Planet.exe
│   ├── Planet.Main\     # 软件本体工程 → 输出 Planet.Main.exe
│   │   ├── Services\    # AppPaths / LogService / SettingsService / NotificationService
│   │   ├── Views\       # HomeView / MysteryView / InfoView / SettingsView / PlanetPopupWindow / ToastWindow
│   │   ├── App.xaml     # 应用入口
│   │   └── MainWindow.xaml
│   ├── Shared\          # 跨工程共享代码（如目录布局约定 AppLayout）
│   ├── build.ps1        # 一键打包脚本
│   ├── .gitignore
│   └── README.md
└── app\                 # 最终打包生成的便携版应用（不入库）
    └── Planet\
```

### 便携版目录（app\Planet）

```dir
Planet\                  # 便携版根目录：仅两个顶层条目
├── Planet.exe           # 启动器（校验目录、拉起软件本体）
└── Data\                # 全部数据存放在此，移动/备份只需带走该文件夹
    ├── Planet.Main.exe  # 软件本体
    ├── settings.txt     # 设置等配置（文本文件，可手工编辑）
    ├── Logs\            # 日志（每次启动生成一个独立文件，自动清理过期日志）
    ├── Applets\         # 功能性插件（每插件一个子文件夹，含信息文件）
    ├── Extensions\      # 非功能性插件（每插件一个子文件夹，含信息文件）
    └── Temp\            # 临时文件（插件下载的 zip 等）
```

## 🖥 开发环境

| 项目 | 建议 |
| --- | --- |
| 编辑器 | Visual Studio Code（安装 **C# Dev Kit** / C# 扩展）或 Visual Studio 2022（.NET 8 工作负载） |
| SDK | .NET 8 SDK（本机已装更高版本 SDK 亦可，目标框架仍为 `net8.0-windows`） |
| 运行/测试平台 | Windows 10 1703 及以上或 Windows 11 |

## 🔨 构建与运行（VS Code 终端，dotnet CLI）

```powershell
cd D:\AAAPlanet-Rewritten\src

# 1. 还原 NuGet 依赖
dotnet restore Planet.sln

# 2. 编译整个解决方案（Launcher + Main）
dotnet build Planet.sln

# 3. 单独运行软件本体（WPF 窗口，配置为无边框透明）
dotnet run --project Planet.Main

# 4. 以 Release 编译
dotnet build Planet.sln -c Release
```

一键打包便携版到 `app\Planet\`：

```powershell
.\build.ps1                # 框架依赖（需目标机已装 .NET 8 运行时）
.\build.ps1 -SelfContained # 自包含（免安装运行时，体积较大）
```

> `build.ps1` 会自动补齐 `Data\Logs`、`Data\Applets` 等子目录并清理调试符号。

## ©️ 版权与致谢

- 原始 VBS 版开发者：**乱码vvv**（感谢原作者的创意）
- 重写版开发者：tiaoling233

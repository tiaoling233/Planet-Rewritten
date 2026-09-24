#requires -Version 5.1
<#
.SYNOPSIS
构建 Planet 便携版并输出到 app\Planet。

目录结构：
    app\Planet\
        Planet.exe          —— 启动器
        Data\               —— 数据文件夹
            Planet.Main.exe —— 软件本体
            Logs\           —— 日志
            Applets\        —— 功能性插件
            Extensions\     —— 非功能性插件
            Temp\           —— 临时文件

.PARAMETER SelfContained
使用 win-x64 自包含发布（免安装 .NET 8 运行时，体积较大）。
默认使用框架依赖发布（需目标机器已安装 .NET 8 Desktop Runtime）。
.EXAMPLE
./build.ps1
.EXAMPLE
./build.ps1 -SelfContained
#>
[CmdletBinding()]
param(
    [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'

$srcRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $srcRoot
$target = Join-Path (Join-Path $repoRoot 'app') 'Planet'
$dataDirectory = Join-Path $target 'Data'

Write-Host "=== Planet 便携版构建 ===" -ForegroundColor Cyan

# ---------------- 1. 清理并创建目标目录 ----------------
Write-Host "`n[1/5] 清理旧的打包目录..." -ForegroundColor Yellow
if (Test-Path $target) { Remove-Item -Path $target -Recurse -Force }
New-Item -ItemType Directory -Path $target, $dataDirectory -Force | Out-Null

# ---------------- 2. 确定发布参数 ----------------
$publishFlags = @('--nologo', '--no-restore')
if ($SelfContained) {
    Write-Host "`n[2/5] 以 win-x64 自包含模式发布（免安装 .NET 8 运行时）..." -ForegroundColor Yellow
    $publishFlags = @('--nologo', '--runtime', 'win-x64', '--self-contained', 'true')
} else {
    Write-Host "`n[2/5] 以框架依赖模式发布（需目标机已安装 .NET 8 运行时）..." -ForegroundColor Yellow
    Write-Host "      提示：加 -SelfContained 参数可生成免安装运行时版本。" -ForegroundColor DarkGray
}

# ---------------- 3. 恢复并发布两个项目 ----------------
Write-Host "`n[3/5] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore (Join-Path $srcRoot 'Planet.sln') | Out-Host
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore 失败' }

Write-Host "`n[4/5] 发布启动器 Planet.exe..." -ForegroundColor Yellow
dotnet publish (Join-Path $srcRoot 'Planet.Launcher') -c Release @publishFlags -o $target | Out-Host
if ($LASTEXITCODE -ne 0) { throw '启动器发布失败' }

Write-Host "`n[5/5] 发布软件本体 Data\Planet.Main.exe..." -ForegroundColor Yellow
dotnet publish (Join-Path (Join-Path $srcRoot 'Planet.Main') 'Planet.Main.csproj') -c Release @publishFlags -o $dataDirectory | Out-Host
if ($LASTEXITCODE -ne 0) { throw '软件本体发布失败' }

# ---------------- 4. 建立 Data 子文件夹 ----------------
Write-Host "`n[6/6] 建立 Data 子文件夹..." -ForegroundColor Yellow
foreach ($sub in 'Logs', 'Applets', 'Extensions', 'Temp') {
    New-Item -ItemType Directory -Path (Join-Path $dataDirectory $sub) -Force | Out-Null
}

# ---------------- 5. 清理调试符号与文档 ----------------
Get-ChildItem -Path $target -Recurse -Include '*.pdb', '*.xml' -File | Remove-Item -Force

# ---------------- 6. 输出结果 ----------------
Write-Host "`n构建完成：" -ForegroundColor Green
$target -join ''
Write-Host ''

Get-ChildItem -Path $target -Recurse -File |
    Select-Object @{ Name = '相对路径'; Expression = { $_.FullName.Substring($target.Length + 1) } },
                  @{ Name = '大小(KB)'; Expression = { [math]::Round($_.Length / 1KB, 1) } } |
    Format-Table -AutoSize
# PowerToys CmdPal 日志查看脚本
# 用于快速查看最新的日志文件

param(
    [int]$Lines = 50,  # 默认显示最后50行
    [switch]$Follow,   # 是否持续监控日志
    [switch]$All       # 是否显示所有日志内容
)

# 日志文件路径
$LogBasePath = "$env:LOCALAPPDATA\Microsoft\PowerToys\CmdPal\Logs"

Write-Host "Searching for PowerToys CmdPal logs..." -ForegroundColor Green

# 检查日志目录是否存在
if (-not (Test-Path $LogBasePath)) {
    Write-Host "Error: Log directory does not exist: $LogBasePath" -ForegroundColor Red
    Write-Host "Please ensure PowerToys has been installed and run at least once." -ForegroundColor Yellow
    exit 1
}

# 查找最新的版本目录
$VersionDirs = Get-ChildItem -Path $LogBasePath -Directory | Sort-Object Name -Descending
if ($VersionDirs.Count -eq 0) {
    Write-Host "Error: No version directories found in $LogBasePath" -ForegroundColor Red
    exit 1
}

$LatestVersionDir = $VersionDirs[0].FullName
Write-Host "Latest version directory: $($VersionDirs[0].Name)" -ForegroundColor Cyan

# 查找最新的日志文件
$LogFiles = Get-ChildItem -Path $LatestVersionDir -Filter "Log_*.log" | Sort-Object LastWriteTime -Descending
if ($LogFiles.Count -eq 0) {
    Write-Host "Error: No log files found in $LatestVersionDir" -ForegroundColor Red
    exit 1
}

$LatestLogFile = $LogFiles[0].FullName
$LogFileName = $LogFiles[0].Name
Write-Host "Latest log file: $LogFileName" -ForegroundColor Cyan
Write-Host "File path: $LatestLogFile" -ForegroundColor Gray
Write-Host "File size: $([math]::Round($LogFiles[0].Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "Last modified: $($LogFiles[0].LastWriteTime)" -ForegroundColor Gray
Write-Host ("-" * 80) -ForegroundColor DarkGray

# 根据参数显示日志内容
try {
    if ($All) {
        Write-Host "Displaying full log content:" -ForegroundColor Yellow
        Get-Content -Path $LatestLogFile -Encoding UTF8
    } elseif ($Follow) {
        Write-Host "Monitoring log file for updates (press Ctrl+C to stop):" -ForegroundColor Yellow
        Get-Content -Path $LatestLogFile -Tail $Lines -Wait -Encoding UTF8
    } else {
        Write-Host "Displaying last $Lines lines:" -ForegroundColor Yellow
        Get-Content -Path $LatestLogFile -Tail $Lines -Encoding UTF8
    }
} catch {
    Write-Host "Error: Failed to read log file: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ("-" * 80) -ForegroundColor DarkGray
Write-Host "Log viewing completed." -ForegroundColor Green

# 显示使用帮助
Write-Host "`nUsage:" -ForegroundColor Cyan
Write-Host "  .\ViewLogs.ps1                    # Display last 50 lines of log" -ForegroundColor White
Write-Host "  .\ViewLogs.ps1 -Lines 100         # Display last 100 lines of log" -ForegroundColor White
Write-Host "  .\ViewLogs.ps1 -Follow            # Monitor log file for updates" -ForegroundColor White
Write-Host "  .\ViewLogs.ps1 -All               # Display full log content" -ForegroundColor White

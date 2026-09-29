// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Reflection;
using System.Runtime.InteropServices;
using Fast.DynamicApplication;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Core;

/// <summary>
/// 程序信息
/// </summary>
[ApiDescriptionSettings(false)]
public class ProgramApplication : IDynamicApplication
{
    /// <summary>
    /// 程序信息
    /// </summary>
    [HttpGet("/program")]
    [HttpGet("/program/index")]
    [ApiInfo("程序信息", HttpRequestActionEnum.Other)]
    [PlatformOnly]
    [ResponseEncipher]
    public async Task<IActionResult> Index()
    {
        DateTime dateTime = DateTime.Now;

        var entryAssembly = Assembly.GetEntryAssembly();

        decimal cpuUsage = await MachineUtil.GetProgramCpuUsage();

        (decimal working, decimal peakWorking, decimal virtualMemory, decimal peakVirtualMemory, decimal pagedMemory,
            decimal peakPagedMemory) = MachineUtil.GetProgramMemoryInfo();

        const decimal relation = 1024;

        return new JsonResult(new
        {
            // 当前时间
            CurrentTime = dateTime,
            // 启动时间
            StartTime = MachineUtil.GetProgramStartTime(),
            // 运行时间
            RunTimes = MachineUtil.GetProgramRunTimes(),
            // 程序名称
            ProgramName = entryAssembly?.GetName().Name,
            // 程序版本
            ProgramVersion = entryAssembly?.GetName().Version,
            // 框架版本
            FrameworkVersion = $"{nameof(Fast)}.{nameof(NET)} {typeof(MAppContext).Assembly.GetName().Version}",
            // 运行时版本
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            // CPU使用率(%)
            CpuUsage = cpuUsage,
            // CPU使用率(%)
            CpuUsagePercent = $"{cpuUsage} %",
            // 物理内存(MB)
            WorkingMemory = Math.Round(working, 2, MidpointRounding.AwayFromZero),
            // 物理内存(MB)
            WorkingMemoryMB = $"{working:F2} MB",
            // 最大物理内存(MB)
            PeakWorkingMemory = Math.Round(peakWorking, 2, MidpointRounding.AwayFromZero),
            // 最大物理内存(MB)
            PeakWorkingMemoryMB = $"{peakWorking:F2} MB",
            // 物理内存使用率(%)
            WorkingMemoryRate = Math.Round(working / peakWorking * 100, 2, MidpointRounding.AwayFromZero),
            // 物理内存使用率(%)
            WorkingMemoryRatePercent = $"{working / peakWorking * 100:F2} %",
            // 分页内存(MB)
            PagedMemoryMemory = Math.Round(pagedMemory, 2, MidpointRounding.AwayFromZero),
            // 分页内存(MB)
            PagedMemoryMemoryMB = $"{pagedMemory:F2} MB",
            // 最大分页内存(MB)
            PeakPagedMemoryMemory = Math.Round(peakPagedMemory, 2, MidpointRounding.AwayFromZero),
            // 最大分页内存(MB)
            PeakPagedMemoryMemoryMB = $"{peakPagedMemory:F2} MB",
            // 虚拟内存(GB)
            VirtualMemory = Math.Round(virtualMemory / relation, 2, MidpointRounding.AwayFromZero),
            // 虚拟内存(GB)
            VirtualMemoryGB = $"{virtualMemory / relation:F2} GB",
            // 最大虚拟内存(GB)
            PeakVirtualMemoryMemory = Math.Round(peakVirtualMemory / relation, 2, MidpointRounding.AwayFromZero),
            // 最大虚拟内存(GB)
            PeakVirtualMemoryMemoryGB = $"{peakVirtualMemory / relation:F2} GB",
            // 主要程序集
            Assemblys = MAppContext.RuntimeLibraries.Where(wh => wh.Type == "package")
                .Select(sl => new {sl.Name, sl.Version})
                .ToList()
        });
    }
}

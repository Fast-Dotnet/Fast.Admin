// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Runtime.InteropServices;
using Fast.DynamicApplication;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Core;

/// <summary>
/// 服务器信息
/// </summary>
[ApiDescriptionSettings(false)]
public class MachineApplication : IDynamicApplication
{
    /// <summary>
    /// 服务器信息
    /// </summary>
    [HttpGet("/machine")]
    [HttpGet("/machine/index")]
    [ApiInfo("服务器信息", HttpRequestActionEnum.Other)]
    [PlatformOnly]
    [ResponseEncipher]
    public IActionResult Index()
    {
        DateTime dateTime = DateTime.Now;

        List<decimal> cpuRate = MachineUtil.GetSystemCpuRate();

        (decimal ramTotal, decimal ramUsed, decimal ramFree) = MachineUtil.GetSystemRamInfo();

        const decimal relation = 1024;

        return new JsonResult(new
        {
            // 当前时间
            CurrentTime = dateTime,
            // 运行时间
            RunTimes = MachineUtil.GetSystemRunTimes(),
            // 主机名称
            HostName = Environment.MachineName,
            // 操作系统
            OSName = RuntimeInformation.OSDescription,
            // 系统架构
            OSArchitecture = $"{Environment.OSVersion.Platform} {RuntimeInformation.OSArchitecture}",
            // CPU核数
            CpuCount = $"{Environment.ProcessorCount} 核",
            // CPU使用率(%)
            CpuRate = cpuRate,
            // CPU使用率(%)
            CpuRatePercent = cpuRate.Select(sl => $"{sl} %").ToList(),
            // 总内存(GB)
            RamTotal = Math.Round(ramTotal / relation, 2, MidpointRounding.AwayFromZero),
            // 总内存(GB)
            RamTotalGB = $"{ramTotal / relation:F2} GB",
            // 已用内存(GB)
            RamUsed = Math.Round(ramUsed / relation, 2, MidpointRounding.AwayFromZero),
            // 已用内存(GB)
            RamUsedGB = $"{ramUsed / relation:F2} GB",
            // 可用内存(GB)
            RamFree = Math.Round(ramFree / relation, 2, MidpointRounding.AwayFromZero),
            // 可用内存(GB)
            RamFreeGB = $"{ramFree / relation:F2} GB",
            // 内存使用率(%)
            RamRate = Math.Round(ramUsed / ramTotal * 100, 2, MidpointRounding.AwayFromZero),
            // 内存使用率(%)
            RamRatePercent = $"{ramUsed / ramTotal * 100:F2} %",
            // 硬盘信息
            DiskInfos = MachineUtil.GetDiskInfos()
        });
    }
}

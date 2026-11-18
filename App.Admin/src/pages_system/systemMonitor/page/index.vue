<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-tabs v-model="state.activeTab" swipeable animated>
			<wd-tab title="概览" :name="1">
				<wd-cell-group title="系统信息">
					<view class="monitor-metric">
						<view class="monitor-metric__header">
							<text>内存</text>
							<text class="monitor-metric__value">{{ state.machineDetail.ramRate }} %</text>
						</view>
						<wd-progress :percentage="state.machineDetail.ramRate" hide-text :color="state.colors" />
						<view class="monitor-metric__detail">
							已用 {{ state.machineDetail.ramUsedGB }} / {{ state.machineDetail.ramTotalGB }} · 可用
							{{ state.machineDetail.ramFreeGB }}
						</view>
					</view>

					<wd-cell title="当前时间" :value="state.machineDetail.currentTime" />
					<wd-cell title="主机名称" :value="state.machineDetail.hostName" />
					<wd-cell title="操作系统" :value="state.machineDetail.osName" />
					<wd-cell title="系统架构" :value="state.machineDetail.osArchitecture" />
					<wd-cell title="CPU核数" :value="state.machineDetail.cpuCount" />
					<wd-cell title="运行时长" :value="state.machineDetail.runTimes" />
				</wd-cell-group>

				<wd-cell-group title="应用信息">
					<view class="monitor-metric">
						<view class="monitor-metric__header">
							<text>CPU</text>
							<text class="monitor-metric__value">{{ state.programDetail.cpuUsage }}%</text>
						</view>
						<wd-progress :percentage="state.programDetail.cpuUsage" hide-text :color="state.colors" />
					</view>

					<view class="monitor-metric">
						<view class="monitor-metric__header">
							<text>内存</text>
							<text class="monitor-metric__value">{{ state.programDetail.workingMemoryRate }}%</text>
						</view>
						<wd-progress :percentage="state.programDetail.workingMemoryRate" hide-text :color="state.colors" />
						<view class="monitor-metric__detail">
							当前 {{ state.programDetail.workingMemoryMB }} / 峰值 {{ state.programDetail.peakWorkingMemoryMB }} · 分页
							{{ state.programDetail.pagedMemoryMemoryMB }}
						</view>
					</view>

					<wd-cell title="启动时间" :value="state.programDetail.startTime" />
					<wd-cell title="应用名称" :value="state.programDetail.programName" />
					<wd-cell title="应用版本" :value="state.programDetail.programVersion" />
					<wd-cell title="框架版本" :value="state.programDetail.frameworkVersion" />
					<wd-cell title="运行版本" :value="state.programDetail.runtimeVersion" />
					<wd-cell title="运行时长" :value="state.programDetail.runTimes" />
				</wd-cell-group>

				<wd-cell-group title="CPU信息">
					<view v-for="(item, index) in state.machineDetail.cpuRate" :key="index" class="monitor-metric">
						<view class="monitor-metric__header">
							<text>核心 {{ index + 1 }}</text>
							<text class="monitor-metric__value">{{ item }}%</text>
						</view>
						<wd-progress :percentage="item" hide-text :color="state.colors" />
					</view>
				</wd-cell-group>

				<wd-cell-group title="硬盘信息">
					<view v-for="(item, index) in state.machineDetail.diskInfos" :key="index" class="monitor-metric">
						<view class="monitor-metric__header">
							<text>{{ item.diskName }}</text>
							<text class="monitor-metric__value">{{ item.availablePercent }}%</text>
						</view>
						<wd-progress :percentage="item.availablePercent" hide-text :color="state.colors" />
						<view class="monitor-metric__detail">
							已用 {{ item.used }} GB / {{ item.totalSize }} GB · 可用 {{ item.availableFreeSpace }} GB
						</view>
					</view>
				</wd-cell-group>
			</wd-tab>

			<wd-tab title="程序集" :name="2">
				<wd-cell-group :title="`程序集（${state.programDetail.assemblys.length}）`" border>
					<wd-cell v-for="(item, index) in state.programDetail.assemblys" :key="index" :title="item.name">
						<wd-tag type="primary" variant="plain" size="small">v{{ item.version }}</wd-tag>
					</wd-cell>
				</wd-cell-group>
			</wd-tab>
		</wd-tabs>
	</view>
</template>

<script setup lang="ts">
import { onHide, onLoad, onShow, onUnload } from "@dcloudio/uni-app";
import { reactive } from "vue";
import { axiosUtil } from "@fast-china/axios";
import { logger, withDefineType } from "@fast-china/utils";

definePage({
	name: "SystemSystemMonitor",
	authCode: "System:Monitor",
	style: {
		navigationBarTitleText: "系统监控",
	},
});

const state = reactive({
	/** 加载状态 */
	loading: false,
	/** 轮询状态 */
	polling: false,
	/** 定时器 */
	interval: withDefineType<NodeJS.Timeout>(),
	machineDetail: withDefineType<{
		/** 当前时间 */
		currentTime?: string;
		/** 运行时间 */
		runTimes?: string;
		/** 主机名称 */
		hostName?: string;
		/** 操作系统 */
		osName?: string;
		/** 系统架构 */
		osArchitecture?: string;
		/** CPU核数 */
		cpuCount?: string;
		/** CPU使用率(%) */
		cpuRate?: number[];
		/** 总内存(GB) */
		ramTotal?: number;
		/** 总内存(GB) */
		ramTotalGB?: string;
		/** 已用内存(GB) */
		ramUsed?: number;
		/** 已用内存(GB) */
		ramUsedGB?: string;
		/** 可用内存(GB) */
		ramFree?: number;
		/** 可用内存(GB) */
		ramFreeGB?: string;
		/** 内存使用率(%) */
		ramRate?: number;
		/** 内存使用率(%) */
		ramRatePercent?: string;
		/** 硬盘信息 */
		diskInfos?: {
			/** 磁盘名 */
			diskName?: string;
			/** 类型名 */
			typeName?: string;
			/** 总剩余 */
			totalFree: number;
			/** 总量 */
			totalSize: number;
			/** 已使用 */
			used: number;
			/** 可使用 */
			availableFreeSpace: number;
			/** 使用百分比 */
			availablePercent: number;
		}[];
	}>({}),
	programDetail: withDefineType<{
		/** 当前时间 */
		currentTime?: string;
		/** 启动时间 */
		startTime?: string;
		/** 运行时间 */
		runTimes?: string;
		/** 程序名称 */
		programName?: string;
		/** 程序版本 */
		programVersion?: string;
		/** 框架版本 */
		frameworkVersion?: string;
		/** 运行时版本 */
		runtimeVersion?: string;
		/** CPU使用率(%) */
		cpuUsage?: number;
		/** CPU使用率(%) */
		cpuUsagePercent?: string;
		/** 物理内存(MB) */
		workingMemory?: number;
		/** 物理内存(MB) */
		workingMemoryMB?: string;
		/** 最大物理内存(MB) */
		peakWorkingMemory?: number;
		/** 最大物理内存(MB) */
		peakWorkingMemoryMB?: string;
		/** 物理内存使用率(%) */
		workingMemoryRate?: number;
		/** 物理内存使用率(%) */
		workingMemoryRatePercent?: string;
		/** 分页内存(MB) */
		pagedMemoryMemory?: number;
		/** 分页内存(MB) */
		pagedMemoryMemoryMB?: string;
		/** 最大分页内存(MB) */
		peakPagedMemoryMemory?: number;
		/** 最大分页内存(MB) */
		peakPagedMemoryMemoryMB?: string;
		/** 虚拟内存(GB) */
		virtualMemory?: number;
		/** 虚拟内存(GB) */
		virtualMemoryGB?: string;
		/** 最大虚拟内存(GB) */
		peakVirtualMemoryMemory?: number;
		/** 最大虚拟内存(GB) */
		peakVirtualMemoryMemoryGB?: string;
		/** 程序集信息 */
		assemblys?: {
			/** 程序集名称 */
			name?: string;
			/** 程序集版本 */
			version?: string;
		}[];
	}>({}),
	colors: [
		{ color: "var(--wot-success-main)", percentage: 20 },
		{ color: "var(--wot-primary-6)", percentage: 40 },
		{ color: "var(--wot-warning-main)", percentage: 60 },
		{ color: "var(--wot-danger-main)", percentage: 80 },
		{ color: "red", percentage: 100 },
	],
	activeTab: 1,
});

/** 页面当前是否处于活动状态 */
let isActive = false;

/** 活动会话编号，用于阻止失活后的异步回调重启轮询 */
let activeSession = 0;

const fetchData = async () => {
	[state.machineDetail, state.programDetail] = await Promise.all([
		axiosUtil.request({
			url: "/machine",
			method: "get",
			requestType: "query",
			requestCipher: true,
		}),
		axiosUtil.request({
			url: "/program",
			method: "get",
			requestType: "query",
			requestCipher: true,
		}),
	]);
};

const stopInterval = () => {
	state.polling = false;
	if (state.interval) {
		clearTimeout(state.interval);
		state.interval = null;
	}
};

const startInterval = () => {
	if (state.polling) return;
	state.polling = true;
	const schedule = () => {
		if (!state.polling) return;
		state.interval = setTimeout(() => {
			fetchData()
				.catch((error: unknown) => logger.error("System", "刷新系统监控数据失败。", error))
				.finally(schedule);
		}, 5 * 1000);
	};
	schedule();
};

/** 激活页面并在首次请求完成后启动轮询 */
const activate = (showLoading = false) => {
	if (isActive) return;
	isActive = true;
	const session = ++activeSession;
	if (showLoading) state.loading = true;
	fetchData()
		.catch((error: unknown) => logger.error("System", "加载系统监控数据失败。", error))
		.finally(() => {
			if (!isActive || session !== activeSession) return;
			state.loading = false;
			startInterval();
		});
};

/** 停用页面并使尚未完成的激活流程失效 */
const deactivate = () => {
	if (!isActive) return;
	isActive = false;
	activeSession++;
	state.loading = false;
	stopInterval();
};

onLoad(() => activate(true));
onShow(() => activate());
onUnload(deactivate);
onHide(deactivate);
</script>

<style lang="scss" scoped>
:deep(.wd-tabs) {
	--wot-tabs-nav-bg: transparent;
}

:deep(.wd-tabs__nav) {
	position: sticky;
	top: var(--window-top, 0px);
	z-index: 10;
	background-color: var(--wot-filled-oppo);
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

:deep(.wd-progress) {
	--wot-progress-bar-height: 6px;
}

:deep(.wd-cell__right) {
	flex: 0 0 auto;
}

.monitor-metric {
	display: flex;
	flex-direction: column;
	gap: 2px;
	padding: 10rpx 20rpx;
	margin: 0 20rpx;
	background-color: var(--wot-card-bg);
	border-radius: var(--wot-radius-large);
	box-shadow: var(--fa-shadow-subtle);
	overflow: hidden;
	transition: box-shadow var(--fa-motion-duration-normal) var(--fa-motion-easing-standard);
	border: var(--wot-stroke-main) solid var(--wot-border-main);

	&:not(:last-child) {
		margin-bottom: 20rpx;
	}
}

.monitor-metric__header {
	display: flex;
	align-items: baseline;
	justify-content: space-between;
	gap: 8px;
	color: var(--wot-text-main);
}

.monitor-metric__value {
	font-weight: var(--wot-font-weight-semibold, 600);
	font-variant-numeric: tabular-nums;
}

.monitor-metric__detail {
	color: var(--wot-text-secondary);
	font-size: 12px;
	line-height: 1.5;
}
</style>

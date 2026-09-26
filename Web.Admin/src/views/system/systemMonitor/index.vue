<template>
	<div v-loading="state.loading" element-loading-text="加载中...">
		<el-row class="monitor-overview w100 mb10" :gutter="10">
			<el-col :span="12" :xs="24" :sm="24" :md="12">
				<el-card shadow="hover" header="系统信息">
					<el-progress :width="150" type="dashboard" :percentage="state.machineDetail.ramRate" :color="state.colors">
						<template #default="{ percentage }">
							<span class="percentage_value">{{ percentage }}%</span>
							<span>总计: {{ state.machineDetail.ramTotalGB }}</span>
							<span>已用: {{ state.machineDetail.ramUsedGB }}</span>
							<span>剩余: {{ state.machineDetail.ramFreeGB }}</span>
							<span class="percentage_label">内存使用率</span>
						</template>
					</el-progress>
					<el-form label-width="100%" label-suffix="：">
						<el-form-item label="当前时间">
							<el-text>{{ state.machineDetail.currentTime }}</el-text>
						</el-form-item>
						<el-form-item label="主机名称">
							<el-text>{{ state.machineDetail.hostName }}</el-text>
						</el-form-item>
						<el-form-item label="操作系统">
							<el-text>{{ state.machineDetail.osName }}</el-text>
						</el-form-item>
						<el-form-item label="系统架构">
							<el-text>{{ state.machineDetail.osArchitecture }}</el-text>
						</el-form-item>
						<el-form-item label="CPU核数">
							<el-text>{{ state.machineDetail.cpuCount }}</el-text>
						</el-form-item>
						<el-form-item label="运行时长">
							<el-text>{{ state.machineDetail.runTimes }}</el-text>
						</el-form-item>
					</el-form>
				</el-card>
			</el-col>
			<el-col :span="12" :xs="24" :sm="24" :md="12">
				<el-card shadow="hover" header="应用信息">
					<el-row>
						<el-col :span="10" :offset="2" :xs="24" :sm="10">
							<el-progress :width="150" type="dashboard" :percentage="state.programDetail.cpuUsage" :color="state.colors">
								<template #default="{ percentage }">
									<span class="percentage_value">{{ percentage }}%</span>
									<span class="percentage_label">CPU使用率</span>
								</template>
							</el-progress>
						</el-col>
						<el-col :span="10" :xs="24" :sm="10">
							<el-progress :width="150" type="dashboard" :percentage="state.programDetail.workingMemoryRate" :color="state.colors">
								<template #default="{ percentage }">
									<span class="percentage_value">{{ percentage }}%</span>
									<span>已用: {{ state.programDetail.workingMemoryMB }}</span>
									<span>最大: {{ state.programDetail.peakWorkingMemoryMB }}</span>
									<span>分页: {{ state.programDetail.pagedMemoryMemoryMB }}</span>
									<span class="percentage_label">内存使用率</span>
								</template>
							</el-progress>
						</el-col>
					</el-row>
					<el-form label-width="100%" label-suffix="：">
						<el-form-item label="启动时间">
							<el-text>{{ state.programDetail.startTime }}</el-text>
						</el-form-item>
						<el-form-item label="应用名称">
							<el-text>{{ state.programDetail.programName }}</el-text>
						</el-form-item>
						<el-form-item label="应用版本">
							<el-text>{{ state.programDetail.programVersion }}</el-text>
						</el-form-item>
						<el-form-item label="框架版本">
							<el-text>{{ state.programDetail.frameworkVersion }}</el-text>
						</el-form-item>
						<el-form-item label="运行版本">
							<el-text>{{ state.programDetail.runtimeVersion }}</el-text>
						</el-form-item>
						<el-form-item label="运行时长">
							<el-text>{{ state.programDetail.runTimes }}</el-text>
						</el-form-item>
					</el-form>
				</el-card>
			</el-col>
		</el-row>
		<el-card class="mb10" shadow="hover" header="CPU信息">
			<el-progress
				v-for="(item, index) in state.machineDetail.cpuRate"
				:key="index"
				:width="150"
				type="dashboard"
				:percentage="item"
				:color="state.colors"
			>
				<template #default="{ percentage }">
					<span class="percentage_value">{{ percentage }}%</span>
					<span>核心 {{ index + 1 }}</span>
					<span class="percentage_label">CPU使用率</span>
				</template>
			</el-progress>
		</el-card>
		<el-card class="mb10" shadow="hover" header="硬盘信息">
			<div class="diskInfo">
				<el-progress
					v-for="(item, index) in state.machineDetail.diskInfos"
					:key="index"
					:width="150"
					type="dashboard"
					:percentage="item.availablePercent"
					:color="state.colors"
				>
					<template #default="{ percentage }">
						<span class="percentage_value">{{ percentage }}%</span>
						<span>总计: {{ item.totalSize }}GB</span>
						<span>已用: {{ item.used }}GB</span>
						<span>剩余: {{ item.availableFreeSpace }}GB</span>
						<span>{{ item.diskName }}</span>
					</template>
				</el-progress>
			</div>
		</el-card>
		<el-card shadow="hover" header="程序集信息">
			<el-tag v-for="(item, index) in state.programDetail.assemblys" :key="index" class="assemblys" type="info" size="large" round>
				<el-text>
					{{ item.name }}
					<el-text type="danger" tag="sup" size="small">v{{ item.version }}</el-text>
				</el-text>
			</el-tag>
		</el-card>
	</div>
</template>

<script lang="ts" setup>
import { onActivated, onDeactivated, onMounted, onUnmounted, reactive } from "vue";
import { axiosUtil } from "@fast-china/axios";
import { logger, withDefineType } from "@fast-china/utils";

defineOptions({
	name: "SystemMonitor",
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
		currentTime?: Date;
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
		currentTime?: Date;
		/** 启动时间 */
		startTime?: Date;
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
		{ color: "var(--el-color-success)", percentage: 20 },
		{ color: "var(--el-color-primary)", percentage: 40 },
		{ color: "var(--el-color-warning)", percentage: 60 },
		{ color: "var(--el-color-danger)", percentage: 80 },
		{ color: "red", percentage: 100 },
	],
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
				.catch((error: unknown) => logger.error("Admin", "刷新系统监控数据失败。", error))
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
		.catch((error: unknown) => logger.error("Admin", "加载系统监控数据失败。", error))
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

onMounted(() => activate(true));
onActivated(() => activate());
onDeactivated(deactivate);

onUnmounted(deactivate);
</script>

<style lang="scss" scoped>
.el-card {
	:deep(.el-card__header) {
		padding: 5px 5px 15px;
		font-size: 15px;
		font-weight: 600;
	}
	:deep(.el-card__body) {
		--el-card-padding: 20px 10px;
	}
}
.el-form {
	.el-form-item {
		margin-bottom: 0;
		padding-top: 5px;
		padding-bottom: 5px;
		border-bottom: var(--el-border);
	}
	:deep() {
		.el-form-item__label {
			padding: 0 6px 0 0;
			flex: 0.3;
		}
		.el-form-item__content {
			text-align: center;
			font-weight: var(--el-font-weight-primary);
		}
	}
}
.el-progress {
	display: flex;
	justify-content: center;
}
:deep(.el-progress__text) {
	display: flex;
	flex-direction: column;
	font-size: 11px !important;
	gap: 5px;
	.percentage_value {
		font-size: 16px !important;
		font-weight: 600;
		padding-bottom: 2px;
	}
	.percentage_label {
		font-weight: 600;
		font-size: 12px !important;
	}
}
.diskInfo {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: space-evenly;
	gap: 10px;
}
.monitor-overview {
	row-gap: 10px;
}
.assemblys {
	max-width: 100%;
	margin-right: 20px;
	margin-bottom: 10px;
}

@media (max-width: 767px) {
	.el-card {
		:deep(.el-card__body) {
			--el-card-padding: 14px 6px;
		}
	}

	.assemblys {
		margin-right: 8px;
	}
}
</style>

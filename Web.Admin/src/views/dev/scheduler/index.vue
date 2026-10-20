<template>
	<el-container v-loading="state.loading" element-loading-text="加载中...">
		<el-aside width="450px">
			<el-card class="h100">
				<el-scrollbar>
					<div style="text-align: center">
						<el-text tag="b" size="large" type="primary"> {{ state.schedulerDetail.schedulerName }}</el-text>
					</div>

					<div v-if="state.tenantName" style="padding-bottom: 10px; text-align: center">
						<el-text tag="b" size="large" type="primary"> {{ state.tenantName }}</el-text>
					</div>

					<div style="border-bottom: var(--el-border); text-align: right; margin-bottom: 10px !important">
						<el-text type="info">{{ dayjs(state.lastUpdateTime).format("YYYY-MM-DD HH:mm:ss") }}</el-text>
					</div>

					<el-form label-width="auto" label-suffix="：">
						<el-form-item label="版本">
							<el-text tag="b" type="danger">v{{ state.schedulerDetail.quartzVersion }}</el-text>
						</el-form-item>
						<el-form-item label="实际状态">
							<div>
								<el-text tag="b">{{ state.schedulerDetail.actualStatus }}</el-text>
								<el-button
									v-if="state.schedulerDetail.schedulerInStandbyMode"
									v-auth="'Scheduler:Start'"
									style="margin-left: 10px"
									size="small"
									plain
									type="primary"
									@click="handleStart"
								>
									启动
								</el-button>
								<el-button
									v-else
									v-auth="'Scheduler:Stop'"
									style="margin-left: 10px"
									size="small"
									plain
									type="danger"
									@click="handleStop"
								>
									待机
								</el-button>
							</div>
						</el-form-item>
						<el-form-item label="期望状态">
							<el-text tag="b">{{ state.schedulerDetail.desiredStatus }}</el-text>
						</el-form-item>
						<el-form-item label="执行宿主">
							<div>
								<el-tag :type="state.schedulerDetail.executionOnline ? 'success' : 'danger'">
									{{ state.schedulerDetail.executionOnline ? "在线" : "离线" }}
								</el-tag>
							</div>
						</el-form-item>
						<el-form-item label="集群">
							<el-text tag="b">{{ state.schedulerDetail.clustered ? "True" : "False" }}</el-text>
						</el-form-item>
						<el-form-item label="实例Id">
							<el-text tag="b">{{ state.schedulerDetail.schedulerInstanceId }}</el-text>
						</el-form-item>
						<el-form-item label="运行时间">
							<el-text>{{ state.schedulerDetail.runTimes }}</el-text>
						</el-form-item>
						<el-form-item label="远程调度器">
							<el-text>{{ state.schedulerDetail.schedulerRemote ? "True" : "False" }}</el-text>
						</el-form-item>
						<el-form-item label="存储字符串">
							<el-text>{{ state.schedulerDetail.supportsPersistence ? "True" : "False" }}</el-text>
						</el-form-item>
						<el-form-item label="调度器类型">
							<el-text>{{ state.schedulerDetail.schedulerType }}</el-text>
						</el-form-item>
						<el-form-item label="持久化类型">
							<el-text>{{ state.schedulerDetail.jobStoreType }}</el-text>
						</el-form-item>
						<el-form-item label="线程池大小">
							<el-text>{{ state.schedulerDetail.threadPoolSize }}</el-text>
						</el-form-item>
						<el-form-item label="线程池类型">
							<el-text>{{ state.schedulerDetail.threadPoolType }}</el-text>
						</el-form-item>
						<el-form-item label="作业">
							<el-text type="info">{{ state.schedulerDetail.jobCountNumber }}个</el-text>
						</el-form-item>
						<el-form-item label="触发器">
							<el-text type="info">{{ state.schedulerDetail.triggerCountNumber }}个</el-text>
						</el-form-item>
						<el-form-item label="正在执行">
							<el-text tag="b" type="warning">{{ state.schedulerDetail.jobExecuteNumber }}个</el-text>
						</el-form-item>
					</el-form>
				</el-scrollbar>
			</el-card>
		</el-aside>
		<el-main>
			<FaTable row-key="jobName" :data="state.schedulerJobList" :tool-btn="false">
				<!-- 表格顶部操作区域 -->
				<template #topHeader>
					<el-menu :default-active="`${state.activeJobGroup}`" mode="horizontal" :ellipsis="false">
						<el-menu-item
							v-for="(item, idx) in schedulerJobGroupEnum"
							:key="idx"
							:index="item.value.toString()"
							@click="handleJobGroupChange(item.value as SchedulerJobGroupEnum)"
						>
							{{ item.label }}
						</el-menu-item>
					</el-menu>
				</template>
				<!-- 表格按钮操作区域 -->
				<template #header>
					<TenantSelectPage style="padding-right: 12px" width="280px" v-model="state.tenantId" />
					<el-button v-auth="'Scheduler:Add'" type="primary" :icon="Plus" @click="editFormRef.add(state.tenantId, state.activeJobGroup)">
						添加作业
					</el-button>
				</template>

				<FaTableColumn prop="jobName" label="作业名称" fixed="left" width="300" small-width="280" />
				<FaTableColumn
					prop="triggerState"
					label="状态"
					width="100"
					small-width="80"
					tag
					:enum="[
						{
							label: '正常',
							value: TriggerState.Normal,
						},
						{
							label: '暂停',
							value: TriggerState.Paused,
						},
						{
							label: '完成',
							value: TriggerState.Complete,
						},
						{
							label: '异常',
							value: TriggerState.Error,
						},
						{
							label: '阻塞',
							value: TriggerState.Blocked,
						},
						{
							label: '不存在',
							value: TriggerState.None,
						},
					]"
				/>
				<FaTableColumn prop="runNumber" label="触发次数" width="100" small-width="80" />
				<FaTableColumn
					prop="jobType"
					label="任务类型"
					width="100"
					small-width="80"
					tag
					:enum="appStore.getDictionary('SchedulerJobTypeEnum')"
				/>
				<FaTableColumn prop="requestUrl" label="请求地址" width="300" small-width="280">
					<template #default="{ row }: { row?: SchedulerJobInfoDto }">
						<Tag v-if="row.requestMethod" name="HttpRequestMethodEnum" :value="row.requestMethod" />
						<br v-if="row.requestUrl" />
						<span v-if="row.requestUrl">{{ row.requestUrl }}</span>
						<span v-else>--</span>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="exception" label="异常信息" width="100" small-width="80">
					<template #default="{ row }: { row?: SchedulerJobInfoDto }">
						<el-tag
							v-if="row.exception"
							type="danger"
							style="cursor: pointer"
							closable
							@click="handleShowException(row)"
							@close="handleDelException(row)"
						>
							查看
						</el-tag>
						<span v-else>--</span>
					</template>
				</FaTableColumn>
				<FaTableColumn
					prop="triggerType"
					label="触发器类型"
					width="120"
					small-width="100"
					tag
					:enum="appStore.getDictionary('TriggerTypeEnum')"
				/>
				<FaTableColumn prop="fireTime" label="执行时间" width="180" small-width="160">
					<template #default="{ row }: { row?: SchedulerJobInfoDto }">
						<el-text v-if="row.previousFireTime" type="info">{{ row.previousFireTime }}</el-text>
						<span v-else>- -</span>
						<br />
						<el-text v-if="row.nextFireTime" type="primary">{{ row.nextFireTime }}</el-text>
						<span v-else>- -</span>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="time" label="开始结束时间" width="180" small-width="160">
					<template #default="{ row }: { row?: SchedulerJobInfoDto }">
						<el-text type="info">{{ row.beginTime }}</el-text>
						<br />
						<el-text v-if="row.endTime" type="primary">{{ row.endTime }}</el-text>
						<span v-else>- -</span>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="interval" label="执行计划" width="150" small-width="130" />
				<FaTableColumn prop="description" label="描述" width="300" small-width="280" />

				<!-- 表格操作 -->
				<template #operation="{ row }: { row: SchedulerJobInfoDto }">
					<div style="margin-bottom: 5px !important">
						<el-button
							v-auth="'Scheduler:Edit'"
							size="small"
							@click="editFormRef.edit(state.tenantId, row.jobName, state.activeJobGroup)"
						>
							编辑
						</el-button>
						<el-button
							v-auth="'Scheduler:Edit'"
							size="small"
							@click="editFormRef.copy(state.tenantId, row.jobName, state.activeJobGroup)"
						>
							复制
						</el-button>
						<el-button
							v-if="row.triggerState == TriggerState.Paused"
							v-auth="'Scheduler:ResumeJob'"
							size="small"
							plain
							type="primary"
							@click="handleResumeJob(row)"
						>
							恢复
						</el-button>
						<el-button v-else v-auth="'Scheduler:StopJob'" size="small" plain type="warning" @click="handleStopJob(row)">暂停</el-button>
					</div>
					<el-button v-auth="'Scheduler:Detail'" size="small" plain type="info" @click="handleLogs(row)">日志</el-button>
					<el-button v-auth="'Scheduler:Trigger'" size="small" plain type="warning" @click="handleTriggerJob(row)">执行</el-button>
					<el-button v-auth="'Scheduler:Delete'" size="small" plain type="danger" @click="handleDelJob(row)">删除</el-button>
				</template>
			</FaTable>
			<el-dialog v-model="state.exp.visible" :title="state.exp.title" width="1000px" align-center draggable destroy-on-close>
				<!-- eslint-disable-next-line vue/no-v-html -- 后端已对日志动态内容进行 HTML 编码 -->
				<div class="log_content" v-html="state.exp.content" />
			</el-dialog>
			<el-dialog v-model="state.log.visible" :title="state.log.title" width="1000px" align-center draggable destroy-on-close>
				<el-scrollbar v-loading="state.log.loading" element-loading-text="加载中..." height="500px">
					<!-- eslint-disable-next-line vue/no-v-html -- 后端已对日志动态内容进行 HTML 编码 -->
					<div v-for="(item, index) in state.log.contents" :key="index" class="log_content" v-html="item" />
				</el-scrollbar>
			</el-dialog>
			<SchedulerEdit ref="editFormRef" @ok="handleTableRefresh" />
		</el-main>
	</el-container>
</template>

<script lang="ts" setup>
import { onActivated, onDeactivated, onMounted, onUnmounted, reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox, dayjs } from "element-plus";
import { logger, withDefineType } from "@fast-china/utils";
import { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";
import { TriggerState } from "@/api/enums/TriggerState";
import { schedulerApi } from "@/api/services/Scheduler/scheduler";
import { useApp } from "@/stores";
import SchedulerEdit from "./edit/index.vue";
import type { QuerySchedulerDetailOutput } from "@/api/services/Scheduler/scheduler/models/QuerySchedulerDetailOutput";
import type { SchedulerJobInfoDto } from "@/api/services/Scheduler/scheduler/models/SchedulerJobInfoDto";

defineOptions({
	name: "DevScheduler",
});

const appStore = useApp();

const editFormRef = useTemplateRef<InstanceType<typeof SchedulerEdit>>("editFormRef");

const schedulerJobGroupEnum = appStore.getDictionary("SchedulerJobGroupEnum");

const state = reactive({
	/** 加载状态 */
	loading: false,
	/** 轮询状态 */
	polling: false,
	/** 定时器 */
	interval: withDefineType<NodeJS.Timeout>(),
	/** 租户Id */
	tenantId: withDefineType<string>(),
	/** 租户名称 */
	tenantName: "",
	/** 激活作业分组 */
	activeJobGroup: SchedulerJobGroupEnum.System,
	/** 调度器详情 */
	schedulerDetail: withDefineType<QuerySchedulerDetailOutput>({}),
	/** 调度作业 */
	schedulerJobList: withDefineType<SchedulerJobInfoDto[]>([]),
	/** 最后更新时间 */
	lastUpdateTime: new Date(),
	exp: {
		visible: false,
		title: "异常",
		content: "",
	},
	log: {
		loading: false,
		visible: false,
		title: "日志",
		contents: withDefineType<string[]>([]),
	},
});

/** 页面当前是否处于活动状态 */
let isActive = false;

/** 活动会话编号，用于阻止失活后的异步回调重启轮询 */
let activeSession = 0;

/** 表格刷新 */
const handleTableRefresh = async () => {
	const apiRes = await schedulerApi.queryAllSchedulerJob(state.activeJobGroup, state.tenantId);
	state.schedulerJobList = apiRes[0]?.jobInfoList ?? [];
};

/** 刷新调度器数据 */
const fetchData = async () => {
	[state.schedulerDetail] = await Promise.all([schedulerApi.querySchedulerDetail(state.tenantId), handleTableRefresh()]);
};

/** 停止定时器 */
const stopInterval = () => {
	state.polling = false;
	if (state.interval) {
		clearTimeout(state.interval);
		state.interval = null;
	}
};

/** 启动定时器 */
const startInterval = () => {
	if (state.polling) return;
	state.polling = true;
	const schedule = () => {
		if (!state.polling) return;
		state.interval = setTimeout(() => {
			state.lastUpdateTime = new Date();
			fetchData()
				.catch((error: unknown) => logger.error("Admin", "刷新调度器数据失败。", error))
				.finally(schedule);
		}, 5000);
	};
	schedule();
};

/** 作业分组改变 */
const handleJobGroupChange = async (value: SchedulerJobGroupEnum) => {
	if (value === state.activeJobGroup) return;
	state.activeJobGroup = value;
	await handleTableRefresh();
};

/** 调度程序启动 */
const handleStart = () => {
	ElMessageBox.confirm(`确定要启动【${state.schedulerDetail.schedulerName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.startScheduler(state.tenantId);
		ElMessage.success("启动成功！");
		state.schedulerDetail.schedulerInStandbyMode = false;
		state.schedulerDetail.desiredStatus = "Running (运行)";
	});
};

/** 调度程序停止 */
const handleStop = () => {
	ElMessageBox.confirm(`确定要待机【${state.schedulerDetail.schedulerName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.stopScheduler(state.tenantId);
		ElMessage.success("待机成功！");
		state.schedulerDetail.schedulerInStandbyMode = true;
		state.schedulerDetail.desiredStatus = "Standby (待机)";
	});
};

/** 异常查看 */
const handleShowException = (row: SchedulerJobInfoDto) => {
	state.exp.title = `异常 - ${row.jobName}`;
	state.exp.content = row.exception;
	state.exp.visible = true;
};

/** 异常删除 */
const handleDelException = (row: SchedulerJobInfoDto) => {
	ElMessageBox.confirm(`确定要删除【${row.jobName}】的异常信息？`, { type: "warning" }).then(async () => {
		await schedulerApi.deleteSchedulerJobException(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		});
		ElMessage.success("删除成功！");
		await handleTableRefresh();
	});
};

/** 处理日志查看 */
const handleLogs = async (row: SchedulerJobInfoDto) => {
	state.log.contents = [];
	state.log.title = `日志 - ${row.jobName}`;
	state.log.visible = true;
	state.log.loading = true;
	state.log.contents = await schedulerApi
		.querySchedulerJobLogs(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		})
		.finally(() => {
			state.log.loading = false;
		});
};

/** 暂停调度作业 */
const handleStopJob = (row: SchedulerJobInfoDto) => {
	ElMessageBox.confirm(`确定要暂停【${row.jobName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.stopSchedulerJob(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		});
		ElMessage.success("暂停成功！");
		await handleTableRefresh();
	});
};

/** 恢复调度作业 */
const handleResumeJob = (row: SchedulerJobInfoDto) => {
	ElMessageBox.confirm(`确定要恢复【${row.jobName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.resumeSchedulerJob(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		});
		ElMessage.success("恢复成功！");
		await handleTableRefresh();
	});
};

/** 执行调度作业 */
const handleTriggerJob = (row: SchedulerJobInfoDto) => {
	ElMessageBox.confirm(`确定要立即执行【${row.jobName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.triggerSchedulerJob(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		});
		ElMessage.success("执行成功！");
		await handleTableRefresh();
	});
};

/** 删除调度作业 */
const handleDelJob = (row: SchedulerJobInfoDto) => {
	ElMessageBox.confirm(`确定要删除【${row.jobName}】？`, { type: "warning" }).then(async () => {
		await schedulerApi.deleteSchedulerJob(state.tenantId, {
			jobName: row.jobName,
			jobGroup: state.activeJobGroup,
		});
		ElMessage.success("删除成功！");
		await handleTableRefresh();
	});
};

/** 激活页面并在首次请求完成后启动轮询 */
const activate = (showLoading = false) => {
	if (isActive) return;
	isActive = true;
	const session = ++activeSession;
	if (showLoading) state.loading = true;
	fetchData()
		.catch((error: unknown) => logger.error("Admin", "加载调度器数据失败。", error))
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
	padding: 0;
}

:deep(.el-card__body) {
	height: 100%;
	padding-right: 10px;

	.el-scrollbar {
		padding-right: 10px;
	}
}

.el-main {
	--el-main-padding: 0 0 0 5px;
}

.el-form {
	.el-form-item {
		margin-bottom: 0;
	}

	:deep(.el-form-item__label) {
		padding: 0 6px 0 0;
	}
}

.fa-table {
	:deep(.fa-table__header) {
		padding: 0 10px;
		background-color: var(--el-menu-bg-color);

		.el-menu {
			--el-menu-horizontal-height: 50px;

			width: 100%;
			border: none;
		}
	}
}

.log_content {
	padding-bottom: 20px;
	padding-right: 10px;
	overflow-wrap: anywhere;

	:deep(.log-list) {
		padding-left: 10px;
		border-left: 3px solid var(--el-color-primary);

		.time {
			display: inline-block;
			padding-bottom: 3px;
			font-weight: 500;
			color: var(--el-color-success);
		}

		.execTime {
			font-weight: 500;
			color: var(--el-color-warning);
		}

		.url {
			color: var(--el-color-primary);
		}

		.params {
			font-weight: 500;
			color: var(--el-color-danger);
		}

		.headers {
			color: var(--el-color-success);
		}
	}

	:deep(.log-list__error) {
		border-left: 3px solid var(--el-color-danger);
	}

	:deep(.log-result) {
		font-weight: 500;
		color: var(--el-color-primary);
	}

	:deep(.log-error) {
		font-weight: 500;
		color: var(--el-color-danger);
	}

	:deep(.log-warn) {
		font-weight: 500;
		color: var(--el-color-warning);
	}

	:deep(pre) {
		white-space: pre-wrap;
	}
}
</style>

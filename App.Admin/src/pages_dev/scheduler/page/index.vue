<template>
	<view class="page" style="overflow: hidden">
		<FaLoading mask :loading="state.loading" />
		<view class="scheduler-header">
			<wd-cell custom-class="scheduler-overview" title="调度器" center is-link @click="faPopupRef.open()">
				<template #label>
					<view class="scheduler-summary">
						<view class="scheduler-summary__dot" :class="{ 'is-offline': !state.schedulerDetail.executionOnline }" />
						<text class="scheduler-summary__text">
							宿主{{ state.schedulerDetail.executionOnline ? "在线" : "离线" }} · 共{{
								state.schedulerDetail.jobCountNumber ?? 0
							}}个作业 · 执行{{ state.schedulerDetail.jobExecuteNumber ?? 0 }}个
						</text>
					</view>
				</template>

				<wd-tag v-if="!state.schedulerDetail.executionOnline" type="danger">
					{{ state.schedulerDetail.actualStatus }}
				</wd-tag>
				<wd-tag v-else-if="state.schedulerDetail.schedulerStarted" type="success">
					{{ state.schedulerDetail.actualStatus }}
				</wd-tag>
				<wd-tag v-else type="warning">
					{{ state.schedulerDetail.actualStatus }}
				</wd-tag>
			</wd-cell>

			<FaTenantSelect style="border: 0" v-model="state.tenantId" v-model:tenant-name="state.tenantName" border />

			<wd-cell title="刷新时间" :value="dayjs(state.lastUpdateTime).format('YYYY-MM-DD HH:mm:ss')" border />
		</view>

		<wd-tabs
			custom-class="scheduler-tabs"
			v-model="state.activeJobGroup"
			line-theme="underline"
			slidable="always"
			swipeable
			animated
			@change="handleJobGroupChange"
		>
			<wd-tab v-for="(cItem, index) in schedulerJobGroupEnum" :key="index" :title="cItem.label" :name="Number(cItem.value)">
				<scroll-view class="scheduler-list" scroll-y>
					<view class="scheduler-list__content">
						<FaAuth code="Scheduler:Add">
							<view class="scheduler-toolbar">
								<wd-button size="small" icon="plus" @click="handleEdit('add')">新增作业</wd-button>
							</view>
						</FaAuth>

						<FaCard v-for="item in state.schedulerJobList" :key="item.jobName" :title="item.jobName">
							<template #titleExtra>
								<view class="fa-card__title-view">
									<wd-tag v-if="item.triggerState === TriggerState.Normal" type="primary">正常</wd-tag>
									<wd-tag v-if="item.triggerState === TriggerState.Paused" type="default">暂停</wd-tag>
									<wd-tag v-if="item.triggerState === TriggerState.Complete" type="success">完成</wd-tag>
									<wd-tag v-if="item.triggerState === TriggerState.Error" type="danger">异常</wd-tag>
									<wd-tag v-if="item.triggerState === TriggerState.Blocked" type="warning">阻塞</wd-tag>
									<wd-tag v-if="item.triggerState === TriggerState.None" type="default">不存在</wd-tag>
								</view>
							</template>

							<FaField label="类型" center>
								<FaTag name="SchedulerJobTypeEnum" :value="item.jobType" />
								<FaTag name="TriggerTypeEnum" :value="item.triggerType" />
							</FaField>
							<FaField label="异常信息" center>
								<wd-button v-if="item.exception" size="mini" type="danger" variant="plain" @click="handleShowException(item)">
									查看
								</wd-button>
								<wd-tag v-else variant="plain">无</wd-tag>
							</FaField>
							<FaField label="执行计划" :value="item.interval" />
							<FaField label="上次执行" :value="item.previousFireTime" />
							<FaField label="下次执行" :value="item.nextFireTime" color="var(--wot-primary-6)" />
							<FaField label="触发次数" :value="item.runNumber" />
							<FaField label="描述" :value="item.description" />

							<template #footer>
								<FaAuth code="Scheduler:Edit">
									<wd-button size="mini" type="info" variant="plain" @click="handleEdit('edit', item.jobName)">编辑</wd-button>
								</FaAuth>
								<FaAuth v-if="item.triggerState == TriggerState.Paused" code="Scheduler:ResumeJob">
									<wd-button size="mini" type="primary" variant="plain" @click="handleResumeJob(item)">恢复</wd-button>
								</FaAuth>
								<FaAuth v-else code="Scheduler:StopJob">
									<wd-button size="mini" type="warning" variant="plain" @click="handleStopJob(item)">暂停</wd-button>
								</FaAuth>
								<FaAuth code="Scheduler:Detail">
									<wd-button size="mini" type="info" variant="plain" @click="handleLogs(item)">日志</wd-button>
								</FaAuth>
								<FaAuth :code="['Scheduler:Add', 'Scheduler:Trigger', 'Scheduler:Delete']">
									<wd-button size="mini" type="info" variant="plain" @click="handleMoreClick(item)">更多</wd-button>
								</FaAuth>
							</template>
						</FaCard>
					</view>

					<FaFooter v-if="configStore.layout.footer" />
				</scroll-view>
			</wd-tab>
		</wd-tabs>
	</view>

	<FaPopup ref="faPopupRef" :title="state.schedulerDetail.schedulerName" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField v-if="state.tenantName" label="租户" :value="state.tenantName" color="var(--wot-text-auxiliary)" />
			<FaField label="版本" :value="`v${state.schedulerDetail.quartzVersion}`" bold color="var(--wot-danger-main)" />
			<FaField label="实际状态" :value="state.schedulerDetail.actualStatus" bold />
			<FaField label="期望状态" :value="state.schedulerDetail.desiredStatus" bold />
			<FaField label="执行宿主" center>
				<wd-tag v-if="state.schedulerDetail.executionOnline" type="success">在线</wd-tag>
				<wd-tag v-else type="danger">离线</wd-tag>
			</FaField>
			<FaField label="集群" :value="state.schedulerDetail.clustered ? 'True' : 'False'" bold />
			<FaField label="实例Id" :value="state.schedulerDetail.schedulerInstanceId" bold />
			<FaField label="运行时间" :value="state.schedulerDetail.runTimes" color="var(--wot-text-auxiliary)" />
			<FaField label="远程调度器" :value="state.schedulerDetail.schedulerRemote ? 'True' : 'False'" color="var(--wot-text-auxiliary)" />
			<FaField label="存储字符串" :value="state.schedulerDetail.supportsPersistence ? 'True' : 'False'" color="var(--wot-text-auxiliary)" />
			<FaField label="调度器类型" :value="state.schedulerDetail.schedulerType" color="var(--wot-text-auxiliary)" />
			<FaField label="持久化类型" :value="state.schedulerDetail.jobStoreType" color="var(--wot-text-auxiliary)" />
			<FaField label="线程池大小" :value="state.schedulerDetail.threadPoolSize" color="var(--wot-text-auxiliary)" />
			<FaField label="线程池类型" :value="state.schedulerDetail.threadPoolType" color="var(--wot-text-auxiliary)" />
			<FaField label="作业" :value="`${state.schedulerDetail.jobCountNumber}个`" color="var(--wot-text-placeholder)" />
			<FaField label="触发器" :value="`${state.schedulerDetail.triggerCountNumber}个`" color="var(--wot-text-placeholder)" />
			<FaField label="正在执行" :value="`${state.schedulerDetail.jobExecuteNumber}个`" bold color="var(--wot-warning-main)" />
		</view>

		<template #footer="{ loading }">
			<FaAuth v-if="state.schedulerDetail.schedulerInStandbyMode" code="Scheduler:Start">
				<wd-button custom-style="flex: 1;" type="primary" block :loading="loading" @click="handleStart">启动</wd-button>
			</FaAuth>
			<FaAuth v-else code="Scheduler:Stop">
				<wd-button custom-style="flex: 1;" type="danger" block :loading="loading" @click="handleStop">待机</wd-button>
			</FaAuth>
		</template>
	</FaPopup>

	<FaPopup ref="logFaPopupRef" :title="state.popup.title" height="70%" position="bottom" closable>
		<template v-if="state.popup.exp" #titlePrefix>
			<FaAuth code="Scheduler:Delete">
				<wd-button size="small" type="danger" variant="plain" @click="handleDelException">删除</wd-button>
			</FaAuth>
		</template>

		<view class="page-detail__popup">
			<view class="scheduler-log">
				<wd-rich-text
					v-for="(item, index) in state.popup.contents"
					:key="index"
					:content="item"
					:class-styles="logClassStyles"
					:tag-style="logTagStyle"
					selectable
				/>
			</view>
		</view>
	</FaPopup>

	<wd-action-sheet v-model="state.actionVisible" :actions="state.actionList" cancel-text="取消" @select="handleActionSelect" />
</template>

<script setup lang="ts">
import { onHide, onLoad, onShow, onUnload } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { logger, throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import dayjs from "dayjs";
import { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum.ts";
import { TriggerState } from "@/api/enums/TriggerState";
import { schedulerApi } from "@/api/services/Scheduler";
import { useMessageBox, useToast } from "@/hooks";
import { useApp, useConfig, useUserInfo } from "@/stores";
import type { Action } from "@wot-ui/ui/components/wd-action-sheet/types";
import type { QuerySchedulerDetailOutput } from "@/api/services/Scheduler/models/QuerySchedulerDetailOutput.ts";
import type { SchedulerJobInfoDto } from "@/api/services/Scheduler/models/SchedulerJobInfoDto.ts";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "DevScheduler",
	authCode: "Scheduler:Paged",
	pageScroll: false,
	footer: false,
	style: {
		navigationBarTitleText: "调度程序",
	},
});

const router = useRouter();
const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();

const faPopupRef = ref<FaPopupInstance>();
const logFaPopupRef = ref<FaPopupInstance>();

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
	popup: {
		exp: false,
		title: "异常",
		contents: withDefineType<string[]>([]),
	},
	/** 弹出菜单显示 */
	actionVisible: false,
	/** 弹出菜单集合 */
	actionList: withDefineType<{ name: string; color?: string }[]>([]),
	/** 点击的调度作业 */
	clickedSchedulerJob: withDefineType<SchedulerJobInfoDto>(),
});

/** 页面当前是否处于活动状态 */
let isActive = false;

/** 活动会话编号，用于阻止失活后的异步回调重启轮询 */
let activeSession = 0;

const logClassStyles: Record<string, string> = {
	"log-list": "display: block; padding-left: 10px; border-width: 0 0 0 3px; border-style: solid; border-color: var(--wot-primary-6);",
	"log-list__error": "border-color: var(--wot-danger-main);",
	time: "display: inline-block; padding-bottom: 3px; font-weight: 500; color: var(--wot-success-main);",
	execTime: "font-weight: 500; color: var(--wot-warning-main);",
	url: "color: var(--wot-primary-6);",
	params: "font-weight: 500; color: var(--wot-danger-main);",
	headers: "color: var(--wot-success-main);",
	"log-result": "font-weight: 500; color: var(--wot-primary-6);",
	"log-error": "font-weight: 500; color: var(--wot-danger-main);",
	"log-warn": "font-weight: 500; color: var(--wot-warning-main);",
};

const logTagStyle: Record<string, string> = {
	pre: "white-space: pre-wrap; overflow-wrap: anywhere; word-break: break-word;",
};

const handleEdit = throttle(
	async (mode: IPageStateType, jobName?: string) =>
		await router.push({
			path: "/pages_dev/scheduler/edit/index",
			query: {
				mode,
				jobName,
				jobGroup: state.activeJobGroup,
				tenantId: state.tenantId,
			},
		})
);

/** 列表刷新 */
const handleCardRefresh = async () => {
	const apiRes = await schedulerApi.queryAllSchedulerJob(state.activeJobGroup, state.tenantId);
	state.schedulerJobList = apiRes[0]?.jobInfoList ?? [];
};

/** 刷新调度器数据 */
const fetchData = async () => {
	[state.schedulerDetail] = await Promise.all([schedulerApi.querySchedulerDetail(state.tenantId), handleCardRefresh()]);
};

/** 作业分组改变 */
const handleJobGroupChange = throttle(async ({ name }: { name: SchedulerJobGroupEnum }) => {
	if (name === state.activeJobGroup) return;
	state.activeJobGroup = name;
	state.schedulerJobList = [];
	await handleCardRefresh();
});

/** 调度程序启动 */
const handleStart = throttle(async () => {
	await useMessageBox.confirm(`确定要启动【${state.schedulerDetail.schedulerName}】？`, {
		async beforeConfirm() {
			await schedulerApi.startScheduler(state.tenantId);
			useToast.success("启动成功！");
			state.schedulerDetail.schedulerInStandbyMode = false;
			state.schedulerDetail.desiredStatus = "Running (运行)";
		},
	});
});

/** 调度程序停止 */
const handleStop = throttle(async () => {
	await useMessageBox.confirm(`确定要待机【${state.schedulerDetail.schedulerName}】？`, {
		async beforeConfirm() {
			await schedulerApi.stopScheduler(state.tenantId);
			useToast.success("待机成功！");
			state.schedulerDetail.schedulerInStandbyMode = true;
			state.schedulerDetail.desiredStatus = "Standby (待机)";
		},
	});
});

/** 异常查看 */
const handleShowException = throttle(async (item: SchedulerJobInfoDto) => {
	state.clickedSchedulerJob = item;
	state.popup.exp = true;
	state.popup.title = `异常 - ${item.jobName}`;
	state.popup.contents = [item.exception];
	await logFaPopupRef.value.open();
});

/** 异常删除 */
const handleDelException = throttle(async () => {
	const { jobName } = state.clickedSchedulerJob;
	await useMessageBox.confirm(`确定要删除【${jobName}】的异常信息？`, {
		async beforeConfirm() {
			await schedulerApi.deleteSchedulerJobException(state.tenantId, {
				jobName,
				jobGroup: state.activeJobGroup,
			});
			useToast.success("删除成功！");
			logFaPopupRef.value.close();
			handleCardRefresh();
		},
	});
});

/** 处理日志查看 */
const handleLogs = throttle(async (item: SchedulerJobInfoDto) => {
	state.clickedSchedulerJob = item;
	state.popup.exp = false;
	state.popup.title = `日志 - ${item.jobName}`;
	await logFaPopupRef.value.open(async () => {
		state.popup.contents = await schedulerApi.querySchedulerJobLogs(state.tenantId, {
			jobName: item.jobName,
			jobGroup: state.activeJobGroup,
		});
	});
});

/** 暂停调度作业 */
const handleStopJob = throttle(async (item: SchedulerJobInfoDto) => {
	await useMessageBox.confirm(`确定要暂停【${item.jobName}】？`, {
		async beforeConfirm() {
			await schedulerApi.stopSchedulerJob(state.tenantId, {
				jobName: item.jobName,
				jobGroup: state.activeJobGroup,
			});
			useToast.success("暂停成功！");
			handleCardRefresh();
		},
	});
});

/** 恢复调度作业 */
const handleResumeJob = throttle(async (item: SchedulerJobInfoDto) => {
	await useMessageBox.confirm(`确定要恢复【${item.jobName}】？`, {
		async beforeConfirm() {
			await schedulerApi.resumeSchedulerJob(state.tenantId, {
				jobName: item.jobName,
				jobGroup: state.activeJobGroup,
			});
			useToast.success("恢复成功！");
			handleCardRefresh();
		},
	});
});

const handleMoreClick = throttle((item: SchedulerJobInfoDto) => {
	state.clickedSchedulerJob = item;
	state.actionList = [
		{ name: "复制作业", permission: "Scheduler:Add" },
		{ name: "执行作业", color: "var(--wot-warning-main)", permission: "Scheduler:Trigger" },
		{ name: "删除作业", color: "var(--wot-danger-main)", permission: "Scheduler:Delete" },
	]
		.filter(({ permission }) => userInfoStore.hasButtonPermission(permission))
		.map(({ name, color }) => ({ name, color }));
	state.actionVisible = true;
});

const handleActionSelect = throttle(async ({ item }: { item: Action }) => {
	const { jobName } = state.clickedSchedulerJob;
	switch (item.name) {
		case "复制作业":
			await handleEdit("copy", jobName);
			break;
		case "执行作业":
			await useMessageBox.confirm(`确定要立即执行【${jobName}】？`, {
				async beforeConfirm() {
					await schedulerApi.triggerSchedulerJob(state.tenantId, {
						jobName,
						jobGroup: state.activeJobGroup,
					});
					useToast.success("执行成功！");
					handleCardRefresh();
				},
			});
			break;
		case "删除作业":
			await useMessageBox.confirm(`确定要删除【${jobName}】？`, {
				async beforeConfirm() {
					await schedulerApi.deleteSchedulerJob(state.tenantId, {
						jobName,
						jobGroup: state.activeJobGroup,
					});
					useToast.success("删除成功！");
					handleCardRefresh();
				},
			});
			break;
	}
});

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
				.catch((error: unknown) => logger.error("Dev", "刷新调度器数据失败。", error))
				.finally(schedule);
		}, 10 * 1000);
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
		.catch((error: unknown) => logger.error("Dev", "加载系统监控数据失败。", error))
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
.page {
	display: flex;
	flex-direction: column;
	min-height: 0;
	overflow: hidden;
}

.scheduler-header {
	flex-shrink: 0;
	background-color: var(--wot-filled-oppo);
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

:deep(.scheduler-overview .wd-cell__right) {
	flex: 0 0 auto;
}

.scheduler-summary {
	display: flex;
	align-items: flex-start;
	gap: 6px;
	margin-top: 4px;
	color: var(--wot-text-auxiliary);
	font-size: var(--wot-typography-label-size-main);
	line-height: 18px;
}

.scheduler-summary__dot {
	flex-shrink: 0;
	width: 6px;
	height: 6px;
	margin-top: 6px;
	border-radius: var(--wot-radius-full);
	background-color: var(--wot-success-main);

	&.is-offline {
		background-color: var(--wot-danger-main);
	}
}

.scheduler-summary__text {
	flex: 1;
	min-width: 0;
}

:deep(.scheduler-tabs) {
	display: flex;
	flex: 1;
	flex-direction: column;
	min-height: 0;
	background-color: var(--wot-filled-bottom);

	.wd-tabs__nav {
		flex-shrink: 0;
		background-color: var(--wot-filled-oppo);
		border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
	}

	.wd-tabs__container {
		flex: 1;
		min-height: 0;
	}

	.wd-tabs__body,
	.wd-tab {
		height: 100%;
	}

	.wd-tab__body:not(.wd-tab__body--inactive) {
		height: 100%;
	}
}

.scheduler-list {
	height: 100%;
	min-height: 0;
}

.scheduler-list__content {
	padding: var(--fa-spacing-section) var(--fa-spacing-page) 0;
}

.scheduler-toolbar {
	margin-bottom: var(--fa-spacing-section);
}

.scheduler-log {
	--wot-rich-text-color: var(--wot-text-main);
	--wot-rich-text-font-size: var(--wot-typography-body-size-main);
	--wot-rich-text-line-height: 1.6;

	display: flex;
	flex-direction: column;
	gap: 20px;
	min-width: 0;
	overflow-wrap: anywhere;
}
</style>

<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		full-height
		:title="state.dialogTitle"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="handleClose"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :grid="false">
			<el-row>
				<el-col :span="16">
					<el-form-item prop="jobName" label="作业名称">
						<el-input v-model="state.formData.jobName" maxlength="50" placeholder="请输入作业名称" />
					</el-form-item>
				</el-col>
				<el-col :span="8">
					<el-form-item prop="jobGroup" label="作业分组">
						<el-select v-model="state.formData.jobGroup" placeholder="请选择作业分组">
							<el-option v-for="(item, idx) in schedulerJobGroupEnum" :key="idx" :label="item.label" :value="item.value" />
						</el-select>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row>
				<el-col :span="12">
					<el-form-item prop="beginTime" label="开始时间">
						<el-date-picker
							v-model="state.formData.beginTime"
							type="datetime"
							value-format="YYYY-MM-DD HH:mm:ss"
							placeholder="请选择开始日期"
						/>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item prop="endTime" label="结束时间">
						<el-date-picker
							v-model="state.formData.endTime"
							type="datetime"
							value-format="YYYY-MM-DD HH:mm:ss"
							placeholder="请选择结束时间"
						/>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row>
				<el-col :span="12">
					<el-form-item prop="mailMessage" label="邮件消息">
						<el-checkbox-group v-model="state.formData.mailMessages">
							<el-checkbox label="信息" :value="MailMessageEnum.Info" border />
							<el-checkbox label="警告" :value="MailMessageEnum.Warn" border />
							<el-checkbox label="错误" :value="MailMessageEnum.Error" border />
						</el-checkbox-group>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item prop="warnTime" label="警告秒数">
						<el-input-number v-model="state.formData.warnTime" :min="0" :controls="false" placeholder="请输入警告秒数">
							<template #suffix>秒</template>
						</el-input-number>
					</el-form-item>
				</el-col>
			</el-row>
			<el-row>
				<el-col :span="12">
					<el-form-item prop="retryTimes" label="重试次数">
						<el-input-number v-model="state.formData.retryTimes" :min="0" :max="5" :controls="false" placeholder="请输入重试次数">
							<template #suffix>次</template>
						</el-input-number>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item prop="retryMillisecond" label="重试间隔">
						<el-input-number
							v-model="state.formData.retryMillisecond"
							:min="500"
							:max="10000"
							:controls="false"
							placeholder="请输入重试间隔"
						>
							<template #suffix>毫秒</template>
						</el-input-number>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item prop="description" label="描述">
				<el-input v-model="state.formData.description" type="textarea" maxlength="100" placeholder="请输入描述" />
			</el-form-item>
			<el-divider content-position="left">触发器信息</el-divider>
			<el-form-item prop="triggerType" label-width="0">
				<el-radio-group v-model="state.formData.triggerType" style="justify-content: center">
					<el-radio-button v-for="(item, idx) in triggerTypeEnum" :key="idx" :label="item.label" :value="item.value" />
				</el-radio-group>
			</el-form-item>
			<template v-if="state.formData.triggerType == TriggerTypeEnum.Cron">
				<el-form-item prop="cron" label="Cron表达式">
					<div style="display: flex; align-items: center; gap: 5px">
						<el-input v-model="state.formData.cron" maxlength="50" placeholder="请输入Cron表达式">
							<template #append>
								<el-button @click="handleCronVerify">运行</el-button>
							</template>
						</el-input>
						<el-button type="primary" size="small" @click="handleCronRefer">参考</el-button>
					</div>
				</el-form-item>
				<el-form-item v-if="state.cronLogs.length > 0">
					<el-scrollbar class="cronLogs">
						<p v-for="(item, index) in state.cronLogs" :key="index">{{ item }}</p>
					</el-scrollbar>
				</el-form-item>
			</template>
			<template v-if="state.formData.triggerType == TriggerTypeEnum.Daily">
				<el-form-item prop="weeks" label="运行周期">
					<el-checkbox-group v-model="state.formData.weeks">
						<el-checkbox label="星期一" :value="WeekEnum.Monday" border />
						<el-checkbox label="星期二" :value="WeekEnum.Tuesday" border />
						<el-checkbox label="星期三" :value="WeekEnum.Wednesday" border />
						<el-checkbox label="星期四" :value="WeekEnum.Thursday" border />
						<el-checkbox label="星期五" :value="WeekEnum.Friday" border />
						<el-checkbox label="星期六" :value="WeekEnum.Saturday" border />
						<el-checkbox label="星期日" :value="WeekEnum.Sunday" border />
					</el-checkbox-group>
				</el-form-item>
				<el-row>
					<el-col :span="12">
						<el-form-item prop="dailyStartTime" label="每天开始时间">
							<el-time-picker v-model="state.formData.dailyStartTime" value-format="HH:mm:ss" placeholder="请选择每天开始时间" />
						</el-form-item>
					</el-col>
					<el-col :span="12">
						<el-form-item prop="dailyEndTime" label="每天结束时间">
							<el-time-picker v-model="state.formData.dailyEndTime" value-format="HH:mm:ss" placeholder="请选择每天结束时间" />
						</el-form-item>
					</el-col>
				</el-row>
			</template>
			<template v-if="state.formData.triggerType == TriggerTypeEnum.Daily || state.formData.triggerType == TriggerTypeEnum.Simple">
				<el-row>
					<el-col :span="12">
						<el-form-item prop="intervalSecond" label="执行间隔时间">
							<el-input-number v-model="state.formData.intervalSecond" :min="30" :controls="false" placeholder="请输入执行间隔时间">
								<template #suffix>秒</template>
							</el-input-number>
						</el-form-item>
					</el-col>
					<el-col :span="12">
						<el-form-item prop="runTimes" label="执行次数">
							<el-input-number v-model="state.formData.runTimes" :min="0" :controls="false" placeholder="请输入执行次数">
								<template #suffix>次</template>
							</el-input-number>
						</el-form-item>
					</el-col>
				</el-row>
			</template>
			<el-divider content-position="left">作业信息</el-divider>
			<el-form-item prop="jobType" label="作业类型">
				<el-radio-group v-model="state.formData.jobType">
					<el-radio-button v-for="(item, idx) in schedulerJobTypeEnum" :key="idx" :label="item.label" :value="item.value" />
				</el-radio-group>
			</el-form-item>
			<template v-if="state.formData.jobType == SchedulerJobTypeEnum.IntranetUrl || state.formData.jobType == SchedulerJobTypeEnum.OuterNetUrl">
				<el-row>
					<el-col :span="12">
						<el-form-item prop="requestMethod" label="请求方式">
							<el-radio-group v-model="state.formData.requestMethod">
								<el-radio label="Get" :value="HttpRequestMethodEnum.Get" />
								<el-radio label="Post" :value="HttpRequestMethodEnum.Post" />
								<el-radio label="Put" :value="HttpRequestMethodEnum.Put" />
								<el-radio label="Delete" :value="HttpRequestMethodEnum.Delete" />
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :span="12">
						<el-form-item prop="requestTimeout" label="请求超时时间">
							<el-input-number v-model="state.formData.requestTimeout" :min="0" :controls="false" placeholder="请输入请求超时时间">
								<template #suffix>秒</template>
							</el-input-number>
						</el-form-item>
					</el-col>
				</el-row>
				<el-form-item prop="requestUrl" label="请求Url">
					<el-input v-model="state.formData.requestUrl" type="textarea" maxlength="100" placeholder="请输入请求Url" />
				</el-form-item>
				<el-form-item prop="requestHeader" label="请求头">
					<div class="dynamic-obj">
						<el-button type="primary" size="small" @click="handleAddObj('requestHeaderObj')">添加</el-button>
						<el-scrollbar v-if="state.requestHeaderObj.length > 0">
							<el-row v-for="(item, index) in state.requestHeaderObj" :key="index" style="margin-bottom: 5px">
								<el-col :span="8">
									<el-input v-model="item.key" maxlength="20" placeholder="请输入Key" />
								</el-col>
								<el-col :span="14">
									<el-input v-model="item.value" maxlength="100" placeholder="请输入Value" />
								</el-col>
								<el-col :span="1">
									<el-button type="danger" size="small" :icon="Delete" circle @click="handleDelObj('requestHeaderObj', index)" />
								</el-col>
							</el-row>
						</el-scrollbar>
					</div>
				</el-form-item>
				<el-form-item prop="requestParams" label="请求参数">
					<div class="dynamic-obj">
						<el-button type="primary" size="small" @click="handleAddObj('requestParamsObj')">添加</el-button>
						<el-scrollbar v-if="state.requestParamsObj.length > 0">
							<el-row v-for="(item, index) in state.requestParamsObj" :key="index" style="margin-bottom: 5px">
								<el-col :span="8">
									<el-input v-model="item.key" maxlength="20" placeholder="请输入Key" />
								</el-col>
								<el-col :span="14">
									<el-input v-model="item.value" maxlength="100" placeholder="请输入Value" />
								</el-col>
								<el-col :span="1">
									<el-button type="danger" size="small" :icon="Delete" circle @click="handleDelObj('requestParamsObj', index)" />
								</el-col>
							</el-row>
						</el-scrollbar>
					</div>
				</el-form-item>
			</template>
		</FaForm>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { Delete } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";
import { MailMessageEnum } from "@/api/enums/MailMessageEnum";
import { SchedulerJobTypeEnum } from "@/api/enums/SchedulerJobTypeEnum";
import { TriggerTypeEnum } from "@/api/enums/TriggerTypeEnum";
import { WeekEnum } from "@/api/enums/WeekEnum";
import { schedulerApi } from "@/api/services/Scheduler/scheduler";
import { useApp } from "@/stores";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";
import type { EditSchedulerJobInput } from "@/api/services/Scheduler/scheduler/models/EditSchedulerJobInput";

defineOptions({
	name: "DevSchedulerEdit",
});

const emit = defineEmits(["ok"]);

const appStore = useApp();

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const schedulerJobGroupEnum = appStore.getDictionary("SchedulerJobGroupEnum");
const triggerTypeEnum = appStore.getDictionary("TriggerTypeEnum");
const schedulerJobTypeEnum = appStore.getDictionary("SchedulerJobTypeEnum");

type IFormData = EditSchedulerJobInput & { mailMessages?: MailMessageEnum[]; weeks?: WeekEnum[] };

const state = reactive({
	formData: withDefineType<IFormData>({
		jobType: SchedulerJobTypeEnum.IntranetUrl,
		beginTime: "1970-01-01 00:00:00",
		triggerType: TriggerTypeEnum.Cron,
		mailMessages: [MailMessageEnum.None],
	}),
	formRules: withDefineType<FormRules<IFormData>>({
		jobName: [{ required: true, message: "请输入作业名称", trigger: "blur" }],
		jobGroup: [{ required: true, message: "请选择作业分组", trigger: "change" }],
		jobType: [{ required: true, message: "请选择作业类型", trigger: "change" }],
		beginTime: [{ required: true, message: "请选择开始时间", trigger: "change" }],
		triggerType: [{ required: true, message: "请选择触发器类型", trigger: "change" }],
		cron: [{ required: true, message: "请输入Cron表达式", trigger: "blur" }],
		weeks: [{ required: true, message: "请选择运行周期", trigger: "change" }],
		dailyStartTime: [{ required: true, message: "请选择每天开始时间", trigger: "change" }],
		dailyEndTime: [{ required: true, message: "请选择每天结束时间", trigger: "change" }],
		intervalSecond: [{ required: true, message: "请输入执行间隔时间", trigger: "blur" }],
		mailMessages: [{ required: true, message: "请选择邮件消息", trigger: "change" }],
		requestUrl: [{ required: true, message: "请输入请求Url", trigger: "blur" }],
		requestMethod: [{ required: true, message: "请选择请求方式", trigger: "change" }],
	}),
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "调度作业",
	cronLogs: withDefineType<string[]>([]),
	requestHeaderObj: withDefineType<{ key: string; value: string }[]>([]),
	requestParamsObj: withDefineType<{ key: string; value: string }[]>([]),
});

const handleClose = () => {
	state.cronLogs = [];
	state.requestHeaderObj = [];
	state.requestParamsObj = [];
	faFormRef?.value?.resetFields();
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		const { dialogState, formData, requestHeaderObj, requestParamsObj } = state;
		if (formData.mailMessages?.length > 0) {
			let _mailMessages = MailMessageEnum.None;
			formData.mailMessages.forEach((item) => (_mailMessages |= item));
			formData.mailMessage = _mailMessages;
		}
		if (formData.weeks?.length > 0) {
			let _week = WeekEnum.None;
			formData.weeks.forEach((item) => (_week |= item));
			formData.week = _week;
		}
		if (requestHeaderObj?.length > 0) {
			const requestHeader: Record<string, string> = {};
			requestHeaderObj.forEach((item) => {
				requestHeader[item.key] = item.value;
			});
			formData.requestHeader = requestHeader;
		}
		if (requestParamsObj?.length > 0) {
			const requestParams: Record<string, string> = {};
			requestParamsObj.forEach((item) => {
				requestParams[item.key] = item.value;
			});
			formData.requestParams = requestParams;
		}
		switch (dialogState) {
			case "add":
				await schedulerApi.addSchedulerJob(formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
			case "copy":
				await schedulerApi.editSchedulerJob(formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const handleFlagsEnum = () => {
	state.formData.mailMessages = [];
	if (state.formData.mailMessage) {
		for (const key in MailMessageEnum) {
			const item = MailMessageEnum[key];
			if (typeof item !== "number") {
				continue;
			}
			if (item === MailMessageEnum.None) {
				continue;
			}
			if ((state.formData.mailMessage & item) !== 0) {
				state.formData.mailMessages.push(item);
			}
		}
	}
	state.formData.weeks = [];
	if (state.formData.week) {
		for (const key in WeekEnum) {
			const item = WeekEnum[key];
			if (typeof item !== "number") {
				continue;
			}
			if (item === WeekEnum.None) {
				continue;
			}
			if ((state.formData.week & item) !== 0) {
				state.formData.weeks.push(item);
			}
		}
	}
};

const handleObj = () => {
	state.requestHeaderObj = [];
	state.requestParamsObj = [];
	state.requestHeaderObj = Object.entries(state.formData.requestHeader ?? {}).map(([key, value]) => ({ key, value }));
	state.requestParamsObj = Object.entries(state.formData.requestParams ?? {}).map(([key, value]) => ({
		key,
		value: typeof value === "string" ? value : (JSON.stringify(value) ?? ""),
	}));
};

const handleCronRefer = () => {
	window.open("https://www.bejson.com/othertools/cron", "_blank");
};

const handleCronVerify = () => {
	faFormRef.value.validateField("cron", async (isValid) => {
		if (!isValid) return;
		const { cron } = state.formData;
		state.cronLogs = await schedulerApi.runVerifyCron(cron);
	});
};

const handleAddObj = (name: "requestHeaderObj" | "requestParamsObj") => {
	state[name].push({ key: "", value: "" });
};

const handleDelObj = (name: "requestHeaderObj" | "requestParamsObj", index: number) => {
	state[name].splice(index, 1);
};

const add = (tenantId: string, jobGroup: SchedulerJobGroupEnum) => {
	state.dialogState = "add";
	state.dialogTitle = "添加调度作业";
	state.formData.tenantId = tenantId;
	state.formData.jobGroup = jobGroup;
	state.formData.oldJobName = undefined;
	state.formData.oldJobGroup = undefined;
};

const edit = (tenantId: string, jobName: string, jobGroup: SchedulerJobGroupEnum) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		const apiRes = await schedulerApi.querySchedulerJob(tenantId, {
			jobName,
			jobGroup,
		});
		state.formData = apiRes;
		state.dialogTitle = `编辑调度作业 - ${apiRes.jobName}`;
		state.formData.oldJobName = jobName;
		state.formData.oldJobGroup = jobGroup;
		handleFlagsEnum();
		handleObj();
	});
};

const copy = (tenantId: string, jobName: string, jobGroup: SchedulerJobGroupEnum) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "copy";
		const apiRes = await schedulerApi.querySchedulerJob(tenantId, {
			jobName,
			jobGroup,
		});
		state.formData = apiRes;
		state.dialogTitle = `复制调度作业 - ${apiRes.jobName}`;
		state.formData.oldJobName = undefined;
		state.formData.oldJobGroup = undefined;
		handleFlagsEnum();
		handleObj();
	});
};

defineExpose({
	element: faDialogRef,
	add,
	edit,
	copy,
});
</script>

<style lang="scss" scoped>
.cronLogs {
	height: 100px;
	padding: 5px 10px;
	line-height: normal;
	background-color: var(--el-color-info-light-7);
}

.dynamic-obj {
	display: flex;
	flex-direction: column;
	align-items: start;
	gap: 5px;

	.el-scrollbar {
		width: 100%;
		height: 120px;
		padding: 5px 10px;
		border: var(--el-border);

		.el-row {
			gap: 5px;
		}
	}
}
</style>

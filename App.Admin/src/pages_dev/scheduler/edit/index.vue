<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="作业名称" prop="jobName">
				<wd-input v-model="state.formData.jobName" clearable :maxlength="50" placeholder="请输入作业名称" />
			</wd-form-item>
			<wd-form-item title="作业分组" prop="jobGroup">
				<FaRadioGroup name="SchedulerJobGroupEnum" v-model="state.formData.jobGroup" />
			</wd-form-item>
			<wd-form-item
				title="开始时间"
				prop="beginTime"
				is-link
				:value="state.formData.beginTime"
				placeholder="请选择开始时间"
				@click="handleDateOpen('beginTime')"
			/>
			<wd-form-item
				title="结束时间"
				prop="endTime"
				is-link
				:value="state.formData.endTime"
				placeholder="请选择结束时间"
				@click="handleDateOpen('endTime')"
			/>
			<wd-form-item title="邮件消息" prop="mailMessage">
				<FaCheckboxGroup :name="false" v-model="state.formData.mailMessage" type="square" direction="horizontal">
					<wd-checkbox :name="MailMessageEnum.Info">信息</wd-checkbox>
					<wd-checkbox :name="MailMessageEnum.Warn">警告</wd-checkbox>
					<wd-checkbox :name="MailMessageEnum.Error">错误</wd-checkbox>
				</FaCheckboxGroup>
			</wd-form-item>
			<wd-form-item title="警告秒数" prop="warnTime">
				<wd-input v-model="state.formData.warnTime" type="number" placeholder="请输入警告秒数">
					<template #suffix>秒</template>
				</wd-input>
			</wd-form-item>
			<wd-form-item title="重试次数" prop="retryTimes">
				<wd-input v-model="state.formData.retryTimes" type="number" placeholder="请输入重试次数">
					<template #suffix>次</template>
				</wd-input>
			</wd-form-item>
			<wd-form-item title="重试间隔" prop="retryMillisecond">
				<wd-input v-model="state.formData.retryMillisecond" type="number" placeholder="请输入重试间隔">
					<template #suffix>毫秒</template>
				</wd-input>
			</wd-form-item>
			<wd-form-item title="描述" prop="description">
				<wd-textarea
					v-model="state.formData.description"
					custom-style="--fa-textarea-height: 75px"
					clearable
					:maxlength="100"
					placeholder="请输入描述"
				/>
			</wd-form-item>

			<wd-cell-group title="触发器信息">
				<wd-form-item title="触发器类型" prop="triggerType">
					<FaRadioGroup name="TriggerTypeEnum" v-model="state.formData.triggerType" direction="horizontal" />
				</wd-form-item>

				<template v-if="state.formData.triggerType == TriggerTypeEnum.Cron">
					<wd-form-item title="Cron表达式" prop="cron" center>
						<wd-input
							v-model="state.formData.cron"
							clearable
							:maxlength="50"
							placeholder="请输入Cron表达式"
							@update:model-value="state.cronLogs = []"
						>
							<template #suffix>
								<wd-button size="mini" variant="plain" @click="handleCronVerify">验证</wd-button>
							</template>
						</wd-input>
					</wd-form-item>
					<wd-form-item v-if="state.cronLogs.length" title="验证结果">
						<view style="display: flex; flex-direction: column">
							<text v-for="(item, index) in state.cronLogs" :key="index">{{ item }}</text>
						</view>
					</wd-form-item>
					<!-- #ifdef H5 -->
					<wd-form-item title="参考" value-align="right" placeholder="点击跳转参考页面" is-link @click="handleCronRefer" />
					<!-- #endif -->
					<!-- #ifndef H5 -->
					<wd-form-item title="参考" value-align="right" placeholder="点击赋值参考链接" is-link @click="handleCronRefer" />
					<!-- #endif -->
				</template>

				<template v-if="state.formData.triggerType == TriggerTypeEnum.Daily">
					<wd-form-item title="运行周期" prop="week">
						<FaCheckboxGroup :name="false" v-model="state.formData.week" type="square">
							<wd-checkbox :name="WeekEnum.Monday">星期一</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Tuesday">星期二</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Wednesday">星期三</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Thursday">星期四</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Friday">星期五</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Saturday">星期六</wd-checkbox>
							<wd-checkbox :name="WeekEnum.Sunday">星期日</wd-checkbox>
						</FaCheckboxGroup>
					</wd-form-item>
					<wd-form-item
						title="每天开始时间"
						prop="dailyStartTime"
						is-link
						:value="state.formData.dailyStartTime"
						placeholder="请选择每天开始时间"
						@click="handleTimeOpen('dailyStartTime')"
					/>
					<wd-form-item
						title="每天结束时间"
						prop="dailyEndTime"
						is-link
						:value="state.formData.dailyEndTime"
						placeholder="请选择每天结束时间"
						@click="handleTimeOpen('dailyEndTime')"
					/>
				</template>

				<template v-if="state.formData.triggerType == TriggerTypeEnum.Daily || state.formData.triggerType == TriggerTypeEnum.Simple">
					<wd-form-item title="执行间隔" prop="intervalSecond">
						<wd-input v-model="state.formData.intervalSecond" type="number" placeholder="请输入执行间隔时间">
							<template #suffix>秒</template>
						</wd-input>
					</wd-form-item>
					<wd-form-item title="执行次数" prop="runTimes">
						<wd-input v-model="state.formData.runTimes" type="number" placeholder="留空为无限循环">
							<template #suffix>次</template>
						</wd-input>
					</wd-form-item>
				</template>
			</wd-cell-group>

			<wd-cell-group title="作业信息">
				<wd-form-item title="作业类型" prop="jobType">
					<FaRadioGroup name="SchedulerJobTypeEnum" v-model="state.formData.jobType" />
				</wd-form-item>

				<template
					v-if="state.formData.jobType == SchedulerJobTypeEnum.IntranetUrl || state.formData.jobType == SchedulerJobTypeEnum.OuterNetUrl"
				>
					<wd-form-item title="请求方式" prop="requestMethod">
						<wd-radio-group v-model="state.formData.requestMethod">
							<wd-radio :value="HttpRequestMethodEnum.Get">Get</wd-radio>
							<wd-radio :value="HttpRequestMethodEnum.Post">Post</wd-radio>
							<wd-radio :value="HttpRequestMethodEnum.Put">Put</wd-radio>
							<wd-radio :value="HttpRequestMethodEnum.Delete">Delete</wd-radio>
						</wd-radio-group>
					</wd-form-item>
					<wd-form-item title="请求超时" prop="requestTimeout">
						<wd-input v-model="state.formData.requestTimeout" type="number" placeholder="留空为不超时">
							<template #suffix>秒</template>
						</wd-input>
					</wd-form-item>
					<wd-form-item title="请求Url" prop="requestUrl">
						<wd-textarea
							v-model="state.formData.requestUrl"
							custom-style="--fa-textarea-height: 75px"
							clearable
							:maxlength="100"
							placeholder="请输入请求Url"
						/>
					</wd-form-item>
				</template>
			</wd-cell-group>
		</wd-form>

		<template v-if="state.formData.jobType == SchedulerJobTypeEnum.IntranetUrl || state.formData.jobType == SchedulerJobTypeEnum.OuterNetUrl">
			<wd-divider content-position="left" custom-style="--wot-divider-padding: 0">请求头</wd-divider>
			<FaCard v-for="(item, index) in state.requestHeaderObj" :key="index" custom-style="margin-left: 20rpx; margin-right: 20rpx;">
				<FaField label="Key" :value="item.key" />
				<FaField label="Value" :value="item.value" />

				<template #footer>
					<wd-button size="mini" type="primary" variant="plain" @click="handleRequestEdit('requestHeaderObj', item, index)">编辑</wd-button>
					<wd-button size="mini" type="danger" variant="plain" @click="handleRequestDelete('requestHeaderObj', index)">删除</wd-button>
				</template>
			</FaCard>
			<wd-button custom-class="fa-form-submit" type="primary" variant="plain" icon="plus" @click="handleRequestAdd('requestHeaderObj')">
				新增请求头
			</wd-button>

			<wd-divider content-position="left" custom-style="--wot-divider-padding: 0">请求参数</wd-divider>
			<FaCard v-for="(item, index) in state.requestParamsObj" :key="index" custom-style="margin-left: 20rpx; margin-right: 20rpx;">
				<FaField label="Key" :value="item.key" />
				<FaField label="Value" :value="item.value" />

				<template #footer>
					<wd-button size="mini" type="primary" variant="plain" @click="handleRequestEdit('requestParamsObj', item, index)">编辑</wd-button>
					<wd-button size="mini" type="danger" variant="plain" @click="handleRequestDelete('requestParamsObj', index)">删除</wd-button>
				</template>
			</FaCard>
			<wd-button custom-class="fa-form-submit" type="primary" variant="plain" icon="plus" @click="handleRequestAdd('requestParamsObj')">
				新增请求参数
			</wd-button>
		</template>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>

		<wd-datetime-picker
			v-model="state.dateTimestamp"
			v-model:visible="state.datePickerVisible"
			:title="state.dateField === 'beginTime' ? '开始时间' : '结束时间'"
			:min-date="dayjs('1970-01-01').valueOf()"
			use-second
			@confirm="handleDateConfirm"
		/>
		<wd-datetime-picker
			v-model="state.timeValue"
			v-model:visible="state.timePickerVisible"
			:title="state.timeField === 'dailyStartTime' ? '每天开始时间' : '每天结束时间'"
			type="time"
			use-second
			@confirm="handleTimeConfirm"
		/>

		<FaPopup ref="faPopupRef" :title="state.popupTitle" height="70%" position="bottom" closable>
			<wd-form :model="state.requestFormData" border>
				<wd-form-item title="Key" prop="key">
					<wd-input v-model="state.requestFormData.key" clearable :maxlength="20" placeholder="请输入Key" />
				</wd-form-item>
				<wd-form-item title="Value" prop="value">
					<wd-textarea
						v-model="state.requestFormData.value"
						custom-style="--fa-textarea-height: 75px"
						clearable
						:maxlength="100"
						placeholder="请输入Value"
					/>
				</wd-form-item>
			</wd-form>

			<wd-button custom-class="fa-form-submit" type="primary" @click="handleRequestConfirm">保存</wd-button>
		</FaPopup>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import dayjs from "dayjs";
import { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";
import { MailMessageEnum } from "@/api/enums/MailMessageEnum";
import { SchedulerJobTypeEnum } from "@/api/enums/SchedulerJobTypeEnum";
import { TriggerTypeEnum } from "@/api/enums/TriggerTypeEnum";
import { WeekEnum } from "@/api/enums/WeekEnum";
import { schedulerApi } from "@/api/services/Scheduler";
import { CommonRoute } from "@/common";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";
import type { EditSchedulerJobInput } from "@/api/services/Scheduler/models/EditSchedulerJobInput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "DevSchedulerEdit",
	authCode: ["Scheduler:Add", "Scheduler:Edit"],
	style: {
		navigationBarTitleText: "调度作业",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();
const faPopupRef = ref<FaPopupInstance>();

type IRequestItem = { key: string; value: string };
type IRequestType = "requestHeaderObj" | "requestParamsObj";

const state = reactive({
	loading: false,
	formData: withDefineType<EditSchedulerJobInput>({}),
	formSchema: useFormSchema<EditSchedulerJobInput>({
		jobName: { required: true, message: "请输入作业名称" },
		jobGroup: { required: true, message: "请选择作业分组" },
		jobType: { required: true, message: "请选择作业类型" },
		beginTime: { required: true, message: "请选择开始时间" },
		endTime: {
			validator: (value, model) => (value && !dayjs(value).isAfter(dayjs(model.beginTime)) ? "结束时间必须晚于开始时间" : undefined),
		},
		warnTime: {
			validator: (value) => (value == null || (Number.isInteger(value) && value >= 0) ? undefined : "警告秒数必须是大于等于 0 的整数"),
		},
		retryTimes: {
			validator: (value) =>
				value == null || (Number.isInteger(value) && value >= 0 && value <= 5) ? undefined : "重试次数必须是 0 到 5 之间的整数",
		},
		retryMillisecond: {
			validator: (value) =>
				value == null || (Number.isInteger(value) && value >= 500 && value <= 10000) ? undefined : "重试间隔必须是 500 到 10000 之间的整数",
		},
		triggerType: { required: true, message: "请选择触发器类型" },
		cron: { required: true, message: "请输入Cron表达式" },
		week: { required: true, message: "请选择运行周期" },
		dailyStartTime: { required: true, message: "请选择每天开始时间" },
		dailyEndTime: {
			required: true,
			message: "请选择每天结束时间",
			validator: (value, model) => (value <= model.dailyStartTime ? "每天结束时间必须晚于开始时间" : undefined),
		},
		intervalSecond: {
			required: true,
			message: "请输入执行间隔时间",
			validator: (value) => (Number.isInteger(value) && value >= 30 ? undefined : "执行间隔时间必须是大于等于 30 的整数"),
		},
		runTimes: {
			validator: (value) => (value == null || (Number.isInteger(value) && value >= 0) ? undefined : "执行次数必须是大于等于 0 的整数"),
		},
		requestUrl: { required: true, message: "请输入请求Url" },
		requestMethod: { required: true, message: "请选择请求方式" },
		requestTimeout: {
			validator: (value) => (value == null || (Number.isInteger(value) && value >= 0) ? undefined : "请求超时必须是大于等于 0 的整数"),
		},
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	tenantId: withDefineType<string>(),
	jobName: withDefineType<string>(),
	jobGroup: withDefineType<SchedulerJobGroupEnum>(),
	cronLogs: withDefineType<string[]>([]),
	dateField: withDefineType<"beginTime" | "endTime">("beginTime"),
	dateTimestamp: Date.now(),
	datePickerVisible: false,
	timeField: withDefineType<"dailyStartTime" | "dailyEndTime">("dailyStartTime"),
	timeValue: "00:00:00",
	timePickerVisible: false,
	requestHeaderObj: withDefineType<IRequestItem[]>([]),
	requestParamsObj: withDefineType<IRequestItem[]>([]),
	popupTitle: "新增请求头",
	requestType: withDefineType<IRequestType>("requestHeaderObj"),
	requestIndex: -1,
	requestFormData: withDefineType<IRequestItem>({ key: "", value: "" }),
});

const handleDateOpen = (field: "beginTime" | "endTime") => {
	state.dateField = field;
	state.dateTimestamp = dayjs(state.formData[field] || undefined).valueOf();
	state.datePickerVisible = true;
};

const handleDateConfirm = ({ value }: { value: number | string | (number | string)[] }) => {
	if (typeof value !== "number") return;
	state.formData[state.dateField] = dayjs(value).format("YYYY-MM-DD HH:mm:ss");
};

const handleTimeOpen = (field: "dailyStartTime" | "dailyEndTime") => {
	state.timeField = field;
	state.timeValue = state.formData[field] || "00:00:00";
	state.timePickerVisible = true;
};

const handleTimeConfirm = ({ value }: { value: number | string | (number | string)[] }) => {
	if (typeof value !== "string") return;
	state.formData[state.timeField] = value;
};

const handleCronVerify = throttle(async () => {
	const { valid } = await formRef.value.validate("cron");
	if (!valid) return;
	state.loading = true;
	try {
		state.cronLogs = await schedulerApi.runVerifyCron(state.formData.cron);
	} finally {
		state.loading = false;
	}
});

const handleCronRefer = throttle(async () => {
	// #ifdef H5
	window.open("https://www.bejson.com/othertools/cron", "_blank");
	// #endif

	// #ifndef H5
	await uni.setClipboardData({ data: "https://www.bejson.com/othertools/cron" });
	// #endif
});

const handleRequestAdd = throttle(async (name: IRequestType) => {
	state.popupTitle = name === "requestHeaderObj" ? "新增请求头" : "新增请求参数";
	state.requestType = name;
	state.requestIndex = -1;
	state.requestFormData = { key: "", value: "" };
	await faPopupRef.value.open();
});

const handleRequestEdit = throttle(async (name: IRequestType, item: IRequestItem, index: number) => {
	state.popupTitle = name === "requestHeaderObj" ? "编辑请求头" : "编辑请求参数";
	state.requestType = name;
	state.requestIndex = index;
	state.requestFormData = { ...item };
	await faPopupRef.value.open();
});

const handleRequestDelete = throttle(async (name: IRequestType, index: number) => {
	await useMessageBox.confirm(name === "requestHeaderObj" ? "确定要删除请求头？" : "确定要删除请求参数？");
	state[name].splice(index, 1);
});

const handleRequestConfirm = throttle(async () => {
	await faPopupRef.value.close(() => {
		if (state.requestIndex !== -1) {
			state[state.requestType][state.requestIndex] = { ...state.requestFormData };
		} else {
			state[state.requestType].push({ ...state.requestFormData });
		}
	});
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		const formData = {
			...state.formData,
			mailMessage: state.formData.mailMessage ?? MailMessageEnum.None,
			requestHeader: Object.fromEntries(state.requestHeaderObj.map((item) => [item.key, item.value])),
			requestParams: Object.fromEntries(state.requestParamsObj.map((item) => [item.key, item.value])),
		};
		switch (state.pageMode) {
			case "add":
			case "copy":
				await schedulerApi.addSchedulerJob(formData);
				useToast.success(state.pageMode === "copy" ? "复制成功！" : "新增成功！");
				break;
			case "edit":
				await schedulerApi.editSchedulerJob(formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Scheduler");
	if (getCurrentPages().length > 1) router.back();
	else await router.pushTab(CommonRoute.Workbench);
});

const handleLoad = async () => {
	state.loading = true;
	try {
		state.cronLogs = [];
		state.requestHeaderObj = [];
		state.requestParamsObj = [];
		switch (state.pageMode) {
			case "add":
				state.formData = {
					tenantId: state.tenantId,
					jobGroup: state.jobGroup,
					oldJobName: undefined,
					oldJobGroup: undefined,
					jobType: SchedulerJobTypeEnum.IntranetUrl,
					beginTime: "1970-01-01 00:00:00",
					triggerType: TriggerTypeEnum.Cron,
					mailMessage: MailMessageEnum.None,
				};
				uni.setNavigationBarTitle({ title: "添加调度作业" });
				break;
			case "edit":
				{
					const apiRes = await schedulerApi.querySchedulerJob(state.tenantId, { jobName: state.jobName, jobGroup: state.jobGroup });
					state.formData = apiRes;
					state.formData.oldJobName = state.jobName;
					state.formData.oldJobGroup = state.jobGroup;
					state.requestHeaderObj = Object.entries(apiRes.requestHeader ?? {}).map(([key, value]) => ({ key, value }));
					state.requestParamsObj = Object.entries(apiRes.requestParams ?? {}).map(([key, value]) => ({
						key,
						value: typeof value === "string" ? value : (JSON.stringify(value) ?? ""),
					}));
					uni.setNavigationBarTitle({ title: `编辑调度作业 - ${apiRes.jobName}` });
				}
				break;
			case "copy":
				{
					const apiRes = await schedulerApi.querySchedulerJob(state.tenantId, { jobName: state.jobName, jobGroup: state.jobGroup });
					state.formData = apiRes;
					state.formData.oldJobName = undefined;
					state.formData.oldJobGroup = undefined;
					state.requestHeaderObj = Object.entries(apiRes.requestHeader ?? {}).map(([key, value]) => ({ key, value }));
					state.requestParamsObj = Object.entries(apiRes.requestParams ?? {}).map(([key, value]) => ({
						key,
						value: typeof value === "string" ? value : (JSON.stringify(value) ?? ""),
					}));
					uni.setNavigationBarTitle({ title: `复制调度作业 - ${apiRes.jobName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; tenantId?: string; jobName?: string; jobGroup?: SchedulerJobGroupEnum }) => {
	if (!options || (options.mode === "edit" && (!options.jobName || !options.jobGroup))) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.tenantId = options.tenantId;
	state.jobName = options.jobName;
	state.jobGroup = options.jobGroup;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

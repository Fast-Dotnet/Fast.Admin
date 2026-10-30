import { axiosUtil } from "@fast-china/axios";
import type { AddSchedulerJobInput } from "./models/AddSchedulerJobInput";
import type { EditSchedulerJobInput } from "./models/EditSchedulerJobInput";
import type { QueryAllSchedulerJobOutput } from "./models/QueryAllSchedulerJobOutput";
import type { QuerySchedulerDetailOutput } from "./models/QuerySchedulerDetailOutput";
import type { SchedulerJobInfo } from "./models/SchedulerJobInfo";
import type { SchedulerJobKeyInput } from "./models/SchedulerJobKeyInput";
import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";

/**
 * 调度作业Api
 */
export const schedulerApi = {
	/**
	 * 运行并验证Cron表达式
	 */
	runVerifyCron(cron: string): Promise<string[]> {
		return axiosUtil.request<string[]>({
			url: "/scheduler/runVerifyCron",
			method: "get",
			params: {
				cron,
			},
			requestType: "other",
		});
	},
	/**
	 * 获取调度器详情
	 */
	querySchedulerDetail(tenantId: string): Promise<QuerySchedulerDetailOutput> {
		return axiosUtil.request<QuerySchedulerDetailOutput>({
			url: "/scheduler/querySchedulerDetail",
			method: "get",
			params: {
				tenantId,
			},
			requestType: "query",
		});
	},
	/**
	 * 启动调度器
	 */
	startScheduler(tenantId: string): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/startScheduler",
			method: "post",
			params: {
				tenantId,
			},
			requestType: "other",
		});
	},
	/**
	 * 停止调度器
	 */
	stopScheduler(tenantId: string): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/stopScheduler",
			method: "post",
			params: {
				tenantId,
			},
			requestType: "other",
		});
	},
	/**
	 * 暂停调度作业
	 */
	stopSchedulerJob(tenantId: string, data: SchedulerJobKeyInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/stopSchedulerJob",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "other",
		});
	},
	/**
	 * 恢复调度作业
	 */
	resumeSchedulerJob(tenantId: string, data: SchedulerJobKeyInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/resumeSchedulerJob",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "other",
		});
	},
	/**
	 * 立即执行调度作业
	 */
	triggerSchedulerJob(tenantId: string, data: SchedulerJobKeyInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/triggerSchedulerJob",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "other",
		});
	},
	/**
	 * 获取调度作业日志
	 */
	querySchedulerJobLogs(tenantId: string, data: SchedulerJobKeyInput): Promise<string[]> {
		return axiosUtil.request<string[]>({
			url: "/scheduler/querySchedulerJobLogs",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取调度作业运行次数
	 */
	querySchedulerJobRunNumber(tenantId: string, data: SchedulerJobKeyInput): Promise<string> {
		return axiosUtil.request<string>({
			url: "/scheduler/querySchedulerJobRunNumber",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "query",
		});
	},
	/**
	 * 获取全部调度作业
	 */
	queryAllSchedulerJob(jobGroup: SchedulerJobGroupEnum, tenantId: string): Promise<QueryAllSchedulerJobOutput[]> {
		return axiosUtil.request<QueryAllSchedulerJobOutput[]>({
			url: "/scheduler/queryAllSchedulerJob",
			method: "get",
			params: {
				jobGroup,
				tenantId,
			},
			requestType: "query",
		});
	},
	/**
	 * 获取调度作业
	 */
	querySchedulerJob(tenantId: string, data: SchedulerJobKeyInput): Promise<SchedulerJobInfo> {
		return axiosUtil.request<SchedulerJobInfo>({
			url: "/scheduler/querySchedulerJob",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "query",
		});
	},
	/**
	 * 添加调度作业
	 */
	addSchedulerJob(data: AddSchedulerJobInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/addSchedulerJob",
			method: "post",
			data,
			requestType: "add",
		});
	},
	/**
	 * 编辑调度作业
	 */
	editSchedulerJob(data: EditSchedulerJobInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/editSchedulerJob",
			method: "post",
			data,
			requestType: "edit",
		});
	},
	/**
	 * 删除调度作业
	 */
	deleteSchedulerJob(tenantId: string, data: SchedulerJobKeyInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/deleteSchedulerJob",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "delete",
		});
	},
	/**
	 * 移除调度作业异常信息
	 */
	deleteSchedulerJobException(tenantId: string, data: SchedulerJobKeyInput): Promise<void> {
		return axiosUtil.request<void>({
			url: "/scheduler/deleteSchedulerJobException",
			method: "post",
			params: {
				tenantId,
			},
			data,
			requestType: "delete",
		});
	},
};

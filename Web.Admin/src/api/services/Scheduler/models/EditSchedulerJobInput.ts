import type { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";
import type { MailMessageEnum } from "@/api/enums/MailMessageEnum";
import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";
import type { SchedulerJobTypeEnum } from "@/api/enums/SchedulerJobTypeEnum";
import type { TriggerTypeEnum } from "@/api/enums/TriggerTypeEnum";
import type { WeekEnum } from "@/api/enums/WeekEnum";

/**
 * 编辑调度作业输入
 */
export interface EditSchedulerJobInput {
	/**
	 * 旧的作业名称
	 */
	oldJobName?: string;
	/**
	 * 
	 */
	oldJobGroup?: SchedulerJobGroupEnum;
	/**
	 * 租户Id
	 */
	tenantId?: string;
	/**
	 * 作业名称
	 */
	jobName?: string;
	/**
	 * 
	 */
	jobGroup?: SchedulerJobGroupEnum;
	/**
	 * 
	 */
	jobType?: SchedulerJobTypeEnum;
	/**
	 * 开始时间
	 */
	beginTime?: string;
	/**
	 * 结束时间
	 */
	endTime?: string;
	/**
	 * 
	 */
	triggerType?: TriggerTypeEnum;
	/**
	 * Cron表达式
	 */
	cron?: string;
	/**
	 * 
	 */
	week?: WeekEnum;
	/**
	 * 每天开始时间
	 */
	dailyStartTime?: string;
	/**
	 * 每天结束时间
	 */
	dailyEndTime?: string;
	/**
	 * 执行间隔时间，单位秒
	 */
	intervalSecond?: number;
	/**
	 * 执行次数（默认无限循环）
	 */
	runTimes?: number;
	/**
	 * 警告秒数
	 */
	warnTime?: number;
	/**
	 * 重试次数
	 */
	retryTimes?: number;
	/**
	 * 重试间隔，单位毫秒
	 */
	retryMillisecond?: number;
	/**
	 * 
	 */
	mailMessage?: MailMessageEnum;
	/**
	 * 描述
	 */
	description?: string;
	/**
	 * 请求 URL
	 */
	requestUrl?: string;
	/**
	 * 
	 */
	requestMethod?: HttpRequestMethodEnum;
	/**
	 * 请求超时时间，单位秒（默认不超时）
	 */
	requestTimeout?: number;
	/**
	 * 请求参数
	 */
	requestParams?: Record<string, unknown>;
	/**
	 * 请求头部
	 */
	requestHeader?: Record<string, string>;
}


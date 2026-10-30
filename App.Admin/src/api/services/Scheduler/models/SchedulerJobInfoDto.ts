import type { HttpRequestMethodEnum } from "@/api/enums/HttpRequestMethodEnum";
import type { SchedulerJobTypeEnum } from "@/api/enums/SchedulerJobTypeEnum";
import type { TriggerState } from "@/api/enums/TriggerState";
import type { TriggerTypeEnum } from "@/api/enums/TriggerTypeEnum";

/**
 * 调度作业信息
 */
export interface SchedulerJobInfoDto {
	/**
	 * 作业名称
	 */
	jobName?: string;
	/**
	 * 上次执行时间
	 */
	previousFireTime?: string;
	/**
	 * 下次执行时间
	 */
	nextFireTime?: string;
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
	 * 时间间隔
	 */
	interval?: string;
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
	 * 
	 */
	triggerState?: TriggerState;
	/**
	 * 运行次数
	 */
	runNumber?: string;
	/**
	 * 异常
	 */
	exception?: string;
}


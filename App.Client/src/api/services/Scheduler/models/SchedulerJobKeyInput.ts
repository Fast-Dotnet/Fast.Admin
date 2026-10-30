import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";

/**
 * 调度作业标识输入
 */
export interface SchedulerJobKeyInput {
	/**
	 * 作业名称
	 */
	jobName?: string;
	/**
	 * 
	 */
	jobGroup?: SchedulerJobGroupEnum;
}


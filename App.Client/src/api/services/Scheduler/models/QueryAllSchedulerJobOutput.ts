import type { SchedulerJobInfoDto } from "./SchedulerJobInfoDto";
import type { SchedulerJobGroupEnum } from "@/api/enums/SchedulerJobGroupEnum";

/**
 * 获取全部调度作业输出
 */
export interface QueryAllSchedulerJobOutput {
	/**
	 * 
	 */
	jobGroup?: SchedulerJobGroupEnum;
	/**
	 * 作业信息
	 */
	jobInfoList?: SchedulerJobInfoDto[];
}


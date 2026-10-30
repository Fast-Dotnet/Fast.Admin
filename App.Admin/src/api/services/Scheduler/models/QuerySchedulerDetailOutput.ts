/**
 * 获取调度器详情输出
 */
export interface QuerySchedulerDetailOutput {
	/**
	 * 是否实际运行
	 */
	isStarted?: boolean;
	/**
	 * Quartz 版本
	 */
	quartzVersion?: string;
	/**
	 * 期望状态
	 */
	desiredStatus?: string;
	/**
	 * 调度执行宿主是否在线
	 */
	executionOnline?: boolean;
	/**
	 * 实际状态
	 */
	actualStatus?: string;
	/**
	 * 调度器实际状态
	 */
	schedulerStatus?: string;
	/**
	 * 调度执行宿主是否离线
	 */
	schedulerShutdown?: boolean;
	/**
	 * 是否期望调度器待机
	 */
	schedulerInStandbyMode?: boolean;
	/**
	 * 调度器是否实际运行
	 */
	schedulerStarted?: boolean;
	/**
	 * 调度器实例Id
	 */
	schedulerInstanceId?: string;
	/**
	 * 调度器名称
	 */
	schedulerName?: string;
	/**
	 * 是否远程调度器
	 */
	schedulerRemote?: boolean;
	/**
	 * 调度器类型
	 */
	schedulerType?: string;
	/**
	 * 持久化类型
	 */
	jobStoreType?: string;
	/**
	 * 强制作业数据映射的值被视为字符串
	 */
	supportsPersistence?: boolean;
	/**
	 * 集群
	 */
	clustered?: boolean;
	/**
	 * 线程池大小
	 */
	threadPoolSize?: number;
	/**
	 * 线程池类型
	 */
	threadPoolType?: string;
	/**
	 * 调度作业执行数
	 */
	jobExecutedNumber?: number;
	/**
	 * 运行时间
	 */
	runTimes?: string;
	/**
	 * 调度作业总数量
	 */
	jobCountNumber?: number;
	/**
	 * 触发器总数量
	 */
	triggerCountNumber?: number;
	/**
	 * 正在执行的调度作业数
	 */
	jobExecuteNumber?: number;
}


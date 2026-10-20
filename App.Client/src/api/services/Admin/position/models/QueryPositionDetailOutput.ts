/**
 * 获取职位详情输出
 */
export interface QueryPositionDetailOutput {
	/**
	 * 职位Id
	 */
	positionId?: string;
	/**
	 * 职位名称
	 */
	positionName?: string;
	/**
	 * 排序
	 */
	sort?: number;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 
	 */
	createdUserName?: string;
	/**
	 * 
	 */
	createdTime?: string;
	/**
	 * 
	 */
	updatedUserName?: string;
	/**
	 * 
	 */
	updatedTime?: string;
	/**
	 * 
	 */
	rowVersion?: string;
}


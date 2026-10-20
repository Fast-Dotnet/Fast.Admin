/**
 * 获取机构详情输出
 */
export interface QueryOrganizationDetailOutput {
	/**
	 * 机构Id
	 */
	orgId?: string;
	/**
	 * 父级Id
	 */
	parentId?: string;
	/**
	 * 父级名称
	 */
	parentName?: string;
	/**
	 * 父级Id集合
	 */
	parentIds?: string[];
	/**
	 * 父级名称集合
	 */
	parentNames?: string[];
	/**
	 * 机构名称
	 */
	orgName?: string;
	/**
	 * 机构编码
	 */
	orgCode?: string;
	/**
	 * 联系人
	 */
	contacts?: string;
	/**
	 * 电话
	 */
	phone?: string;
	/**
	 * 邮箱
	 */
	email?: string;
	/**
	 * 排序
	 */
	sort?: number;
	/**
	 * 数据公开
	 */
	dataPublic?: boolean;
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


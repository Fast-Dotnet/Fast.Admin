/**
 * 获取部门分页列表输入
 */
export interface QueryDepartmentPagedInput {
	/**
	 * 搜索值
	 */
	searchValue?: string;
	/**
	 * 机构Id
	 */
	orgId?: string;
	/**
	 * 树形数据
	 */
	treeData?: boolean;
}


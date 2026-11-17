import type { PagedInput } from "fast-element-plus";

/**
 * 获取部门分页列表输入
 */
export interface QueryDepartmentPagedInput extends PagedInput  {
	/**
	 * 机构Id
	 */
	orgId?: string;
	/**
	 * 树形数据
	 */
	treeData?: boolean;
}


import type { PagedInput } from "fast-element-plus";
import type { EmployeeStatusEnum } from "@/api/enums/EmployeeStatusEnum";
import type { GenderEnum } from "@/api/enums/GenderEnum";

/**
 * 获取职员分页列表输入
 */
export interface QueryEmployeePagedInput extends PagedInput  {
	/**
	 * 
	 */
	status?: EmployeeStatusEnum;
	/**
	 * 
	 */
	sex?: GenderEnum;
	/**
	 * 部门Id
	 */
	departmentId?: string;
}


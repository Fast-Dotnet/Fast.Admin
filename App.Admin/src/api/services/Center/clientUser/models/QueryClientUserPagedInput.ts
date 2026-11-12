import type { GenderEnum } from "@/api/enums/GenderEnum";

/**
 * 获取客户端用户分页列表输入
 */
export interface QueryClientUserPagedInput extends PagedInput  {
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 
	 */
	sex?: GenderEnum;
}


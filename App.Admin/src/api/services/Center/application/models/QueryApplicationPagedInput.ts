import type { EditionEnum } from "@/api/enums/EditionEnum";

/**
 * 获取应用分页列表输入
 */
export interface QueryApplicationPagedInput extends PagedInput  {
	/**
	 * 
	 */
	edition?: EditionEnum;
}


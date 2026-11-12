import type { PasswordOperationTypeEnum } from "@/api/enums/PasswordOperationTypeEnum";
import type { PasswordTypeEnum } from "@/api/enums/PasswordTypeEnum";

/**
 * 获取密码记录分页列表输入
 */
export interface QueryPasswordRecordPagedInput extends PagedInput  {
	/**
	 * 账号Id
	 */
	accountId?: string;
	/**
	 * 
	 */
	operationType?: PasswordOperationTypeEnum;
	/**
	 * 
	 */
	type?: PasswordTypeEnum;
}


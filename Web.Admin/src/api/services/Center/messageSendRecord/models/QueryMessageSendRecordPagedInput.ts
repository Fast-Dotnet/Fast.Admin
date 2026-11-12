import type { PagedInput } from "fast-element-plus";
import type { MessageSendChannelEnum } from "@/api/enums/MessageSendChannelEnum";

/**
 * 获取消息发送记录分页列表输入
 */
export interface QueryMessageSendRecordPagedInput extends PagedInput  {
	/**
	 * 
	 */
	channel?: MessageSendChannelEnum;
	/**
	 * 是否成功
	 */
	isSuccess?: boolean;
}


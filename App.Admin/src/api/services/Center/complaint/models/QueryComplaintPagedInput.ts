import type { ComplaintTypeEnum } from "@/api/enums/ComplaintTypeEnum";

/**
 * 获取投诉分页列表输入
 */
export interface QueryComplaintPagedInput extends PagedInput  {
	/**
	 * 
	 */
	complaintType?: ComplaintTypeEnum;
}


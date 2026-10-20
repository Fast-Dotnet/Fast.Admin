import type { ComplaintTypeEnum } from "@/api/enums/ComplaintTypeEnum";

/**
 * 获取投诉分页列表输出
 */
export interface QueryComplaintPagedOutput {
	/**
	 * 投诉Id
	 */
	complaintId?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * 应用标识
	 */
	appKey?: string;
	/**
	 * 昵称
	 */
	nickName?: string;
	/**
	 * 
	 */
	complaintType?: ComplaintTypeEnum;
	/**
	 * 手机
	 */
	mobile?: string;
	/**
	 * 联系电话
	 */
	contactPhone?: string;
	/**
	 * 联系邮箱
	 */
	contactEmail?: string;
	/**
	 * 投诉描述
	 */
	description?: string;
	/**
	 * 附件图片
	 */
	attachmentImages?: string[];
	/**
	 * 处理时间
	 */
	handleTime?: string;
	/**
	 * 处理描述
	 */
	handleDescription?: string;
	/**
	 * 备注
	 */
	remark?: string;
	/**
	 * 创建时间
	 */
	createdTime?: string;
	/**
	 * 租户名称
	 */
	tenantName?: string;
	/**
	 * 更新版本控制字段
	 */
	rowVersion?: string;
}


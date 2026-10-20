import type { SerialDateTypeEnum } from "@/api/enums/SerialDateTypeEnum";
import type { SerialRuleTypeEnum } from "@/api/enums/SerialRuleTypeEnum";
import type { SerialSpacerEnum } from "@/api/enums/SerialSpacerEnum";

/**
 * 获取序号规则分页列表输出
 */
export interface QuerySerialRulePagedOutput {
	/**
	 * 序号规则Id
	 */
	serialRuleId?: string;
	/**
	 * 
	 */
	ruleType?: SerialRuleTypeEnum;
	/**
	 * 前缀
	 */
	prefix?: string;
	/**
	 * 
	 */
	dateType?: SerialDateTypeEnum;
	/**
	 * 
	 */
	spacer?: SerialSpacerEnum;
	/**
	 * 长度
	 */
	length?: number;
	/**
	 * 最后一个序号
	 */
	lastSerial?: string;
	/**
	 * 最后一个序号编号
	 */
	lastSerialNo?: string;
	/**
	 * 最后一个序号生成时间
	 */
	lastTime?: string;
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


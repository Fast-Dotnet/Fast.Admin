import type { SerialDateTypeEnum } from "@/api/enums/SerialDateTypeEnum";
import type { SerialRuleTypeEnum } from "@/api/enums/SerialRuleTypeEnum";
import type { SerialSpacerEnum } from "@/api/enums/SerialSpacerEnum";

/**
 * 获取序号规则详情输出
 */
export interface QuerySerialRuleDetailOutput {
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


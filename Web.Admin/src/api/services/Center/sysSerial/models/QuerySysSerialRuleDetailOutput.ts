import type { SerialDateTypeEnum } from "@/api/enums/SerialDateTypeEnum";
import type { SerialSpacerEnum } from "@/api/enums/SerialSpacerEnum";
import type { SysSerialRuleTypeEnum } from "@/api/enums/SysSerialRuleTypeEnum";

/**
 * 获取系统序号规则详情输出
 */
export interface QuerySysSerialRuleDetailOutput {
	/**
	 * 序号规则Id
	 */
	serialRuleId?: string;
	/**
	 * 
	 */
	ruleType?: SysSerialRuleTypeEnum;
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


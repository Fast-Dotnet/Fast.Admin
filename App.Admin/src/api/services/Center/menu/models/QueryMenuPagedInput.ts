import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EditionEnum } from "@/api/enums/EditionEnum";
import type { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";

/**
 * 获取菜单列表输入
 */
export interface QueryMenuPagedInput extends PagedInput  {
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 
	 */
	menuType?: MenuTypeEnum;
	/**
	 * 是否桌面端
	 */
	hasDesktop?: boolean;
	/**
	 * 是否Web端
	 */
	hasWeb?: boolean;
	/**
	 * 是否移动端
	 */
	hasMobile?: boolean;
	/**
	 * 是否显示
	 */
	visible?: boolean;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
}


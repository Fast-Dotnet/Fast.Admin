import type { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import type { EditionEnum } from "@/api/enums/EditionEnum";
import type { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import type { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";

/**
 * 获取菜单列表输出
 */
export interface QueryMenuPagedOutput {
	/**
	 * 菜单Id
	 */
	menuId?: string;
	/**
	 * 
	 */
	edition?: EditionEnum;
	/**
	 * 应用Id
	 */
	appId?: string;
	/**
	 * 应用名称
	 */
	appName?: string;
	/**
	 * 菜单编码
	 */
	menuCode?: string;
	/**
	 * 菜单名称
	 */
	menuName?: string;
	/**
	 * 菜单标题
	 */
	menuTitle?: string;
	/**
	 * 父级Id
	 */
	parentId?: string;
	/**
	 * 
	 */
	menuType?: MenuTypeEnum;
	/**
	 * 
	 */
	roleType?: RoleTypeEnum;
	/**
	 * 是否桌面端
	 */
	hasDesktop?: boolean;
	/**
	 * 桌面端图标
	 */
	desktopIcon?: string;
	/**
	 * 桌面端路由地址
	 */
	desktopRouter?: string;
	/**
	 * 是否Web端
	 */
	hasWeb?: boolean;
	/**
	 * Web端图标
	 */
	webIcon?: string;
	/**
	 * Web端路由地址
	 */
	webRouter?: string;
	/**
	 * Web端组件地址
	 */
	webComponent?: string;
	/**
	 * Web端页面是否在导航栏显示
	 */
	webTab?: boolean;
	/**
	 * Web端页面是否缓存
	 */
	webKeepAlive?: boolean;
	/**
	 * 是否移动端
	 */
	hasMobile?: boolean;
	/**
	 * 移动端图标
	 */
	mobileIcon?: string;
	/**
	 * 移动端路由地址
	 */
	mobileRouter?: string;
	/**
	 * 内链/外链地址
	 */
	link?: string;
	/**
	 * 是否显示
	 */
	visible?: boolean;
	/**
	 * 排序
	 */
	sort?: number;
	/**
	 * 
	 */
	status?: CommonStatusEnum;
	/**
	 * 子节点
	 */
	children?: QueryMenuPagedOutput[];
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


import type { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";

/**
 * 授权菜单信息Dto
 */
export interface AuthMenuInfoDto {
	/**
	 * 菜单Id
	 */
	menuId?: string;
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
	 * 父级Id
	 */
	parentIds?: string[];
	/**
	 * 
	 */
	menuType?: MenuTypeEnum;
	/**
	 * 图标
	 */
	icon?: string;
	/**
	 * 路由地址
	 */
	router?: string;
	/**
	 * 组件地址
	 */
	component?: string;
	/**
	 * 导航栏显示
	 */
	tab?: boolean;
	/**
	 * 缓存页面
	 */
	keepAlive?: boolean;
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
	 * 子节点
	 */
	children?: AuthMenuInfoDto[];
}


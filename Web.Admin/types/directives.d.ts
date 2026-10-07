import type { Directive } from "vue";

declare module "vue" {
	export interface GlobalDirectives {
		/**
		 * 权限指令
		 * 用于判断某个按钮或者点击方法是否存在权限，自带防抖
		 * @remarks 绑定值为权限码，支持字符串或字符串数组。
		 * @example
		 * ```vue
		 * <!-- 单个权限 -->
		 * <el-button v-auth="'user:add'">添加</el-button>
		 *
		 * <!-- 多个权限（满足其一即可） -->
		 * <el-button v-auth="['user:edit', 'user:update']">编辑</el-button>
		 *
		 * <!-- 指定权限类型 -->
		 * <el-button v-auth="'user:delete'" authType="hide">删除</el-button>
		 * <el-button v-auth="'user:export'" authType="disabled">导出</el-button>
		 * <el-button v-auth="'user:import'" authType="disablePointer">导入</el-button>
		 * ```
		 */
		vAuth: Directive<HTMLElement, string | string[]>;
	}

	export interface ComponentCustomProps {
		/**
		 * 权限类型
		 * @defaultValue 'hide'
		 */
		authType?: "hide" | "disabled" | "disablePointer" | "none";
	}

	export interface HTMLAttributes {
		/**
		 * 权限类型
		 * @defaultValue 'hide'
		 */
		authType?: "hide" | "disabled" | "disablePointer" | "none";
	}
}

export {};

/// <reference types="vite/client" />

declare module "*.vue" {
	import type { DefineComponent } from "vue";

	const component: DefineComponent<Record<string, unknown>, Record<string, unknown>, Record<string, unknown>>;
	export default component;
}

declare module "*.scss" {
	const scss: Record<string, string>;
	export default scss;
}

declare module "vue" {
	interface ComponentCustomOptions {
		options?: {
			/**
			 * 是否启用多 Slot 支持
			 *
			 * @defaultValue true
			 */
			multipleSlots?: boolean;
			/**
			 * 组件样式隔离方式
			 *
			 * - `isolated`：启用样式隔离，组件内外的 class 样式互不影响。
			 * - `apply-shared`：页面样式可影响组件，但组件样式不会影响页面。
			 * - `shared`：页面样式与组件样式可相互影响，也会影响其他设置为
			 *   `apply-shared` 或 `shared` 的自定义组件。
			 *
			 * 支持微信小程序、支付宝小程序（基础库 2.7.2+）。
			 *
			 * @defaultValue "apply-shared"
			 */
			styleIsolation?: "isolated" | "apply-shared" | "shared";
			/**
			 * 是否允许页面全局样式影响当前组件
			 *
			 * 等价于设置 `styleIsolation: "apply-shared"`。
			 * 当同时设置 `styleIsolation` 时，以 `styleIsolation` 为准。
			 *
			 * @defaultValue true
			 */
			addGlobalClass?: boolean;
			/**
			 * 是否将组件根节点声明为虚拟节点
			 *
			 * 启用后，组件根节点本身不参与样式和布局计算，组件内部第一层节点可直接参与外部 Flex 布局等场景。
			 *
			 * 可配合 `mergeVirtualHostAttributes` 合并虚拟节点外层属性。
			 *
			 * 支持支付宝小程序、微信小程序、抖音小程序（4.02+）。
			 *
			 * @defaultValue false
			 */
			virtualHost?: boolean;
		};
	}
}

export {};

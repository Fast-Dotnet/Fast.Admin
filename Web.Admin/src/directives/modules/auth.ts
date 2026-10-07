/**
 * v-auth
 * 用于判断某个 按钮 或者 点击方法是否存在权限
 * 自带防抖
 * 默认使用隐藏或者显示，也可以再标签上 增加 authType="xxx"
 * 接收参数：string | string[] 类型
 */
import { ElMessage } from "element-plus";
import { withInstallDirective } from "@fast-china/utils";
import { useUserInfo } from "@/stores";
import type { Directive, DirectiveBinding, VNode } from "vue";

interface AuthElement extends HTMLElement {
	__tag__: string[];
	__authType: "hide" | "disabled" | "disablePointer" | "none";
	__auth_isAuth__: boolean;
	__auth_timer__?: ReturnType<typeof setTimeout>;
	__tag_originClick__?: () => void;
}

interface AuthVNode extends VNode {
	props: {
		/** 点击事件 */
		onClick?: () => void;
		/** 类型 */
		authType?: "hide" | "disabled" | "disablePointer" | "none";
		/** 禁用 */
		disabled?: boolean;
		[key: string]: unknown;
	};
}

/**
 * 无权限提示
 */
const notAuthMsg = () => {
	ElMessage({
		type: "warning",
		message: "无操作权限",
		duration: 1500,
	});
};

/**
 * 无权限操作
 */
const notAuthAction = (el: AuthElement, vNode: AuthVNode) => {
	const authType = vNode.props.authType || "hide";
	el.__authType = authType;
	// 不存在权限，根据系统配置执行
	switch (authType) {
		case "disabled":
			// 禁用
			vNode.props.disabled = true;
			// 添加一个禁用的样式，这个样式是 ElPlus 自带的
			el.classList.add("is-disabled");
			break;
		case "hide":
			// 隐藏
			el.remove();
			break;
		case "disablePointer":
			// 禁止点击
			el.style.pointerEvents = "none";
			break;
		case "none":
			// 啥也不干
			break;
		default:
			throw new Error("The auth type is incorrect");
	}
};

const AuthDirective: Directive = {
	created(el: AuthElement, binding: DirectiveBinding<string | string[]>, vNode: AuthVNode) {
		if (!binding.value) return;

		const userInfoStore = useUserInfo();

		// 记录 tag
		if (Array.isArray(binding.value)) {
			el.__tag__ = binding.value;
		} else if (typeof binding.value === "string") {
			el.__tag__ = [binding.value];
		} else {
			throw new TypeError("callback must be a string or string array");
		}
		// 记录原来的点击事件方法
		el.__tag_originClick__ = vNode.props?.onClick;

		// 权限判断
		let isAuth = false;
		// 管理员判断
		if (userInfoStore.isSuperAdmin || userInfoStore.isAdmin) {
			isAuth = true;
		} else {
			// 这里只要判断存在一个Code即可
			el.__tag__.forEach((item) => {
				if (userInfoStore.buttonCodeList.includes(item)) {
					isAuth = true;
				}
			});
		}
		el.__auth_isAuth__ = isAuth;

		// 权限判断
		if (el.__auth_isAuth__) {
			if (el?.__tag_originClick__) {
				// 原来的事件
				vNode.props.onClick = function (): void {
					if (el.__auth_timer__) {
						clearTimeout(el.__auth_timer__);
					}
					// 防抖处理
					el.__auth_timer__ = setTimeout(() => {
						el.__tag_originClick__?.();
					}, 500);
				};
			}
		} else {
			// 替换点击事件
			vNode.props.onClick = notAuthMsg;
			notAuthAction(el, vNode);
		}
	},
	mounted(el: AuthElement, _binding: DirectiveBinding, vNode: AuthVNode) {
		// 判断权限是否通过
		if (!el.__auth_isAuth__) {
			notAuthAction(el, vNode);
		}
	},
	beforeUnmount(el: AuthElement) {
		if (el.__auth_timer__) {
			clearTimeout(el.__auth_timer__);
			delete el.__auth_timer__;
		}
	},
};

export const vAuth = withInstallDirective(AuthDirective, "auth");
export default vAuth;

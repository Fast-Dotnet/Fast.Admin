<template>
	<component
		:is="activeLoginComponent"
		:background="getThemeGradient(configStore.layout.themeColor, configStore.layout.isDark ? 'dark' : 'light')"
		:footer-height="configStore.layout.footerHeight"
		:form-rules="state.formRules"
	>
		<template #help>
			<el-dropdown ref="helpDropdownRef" class="help_dropdown" size="default" trigger="click" @command="handleDropdownClick">
				<el-button class="help-trigger" text circle aria-label="打开显示设置">
					<el-icon :size="18">
						<Moon v-if="configStore.layout.isDark" />
						<Sunny v-else />
					</el-icon>
				</el-button>
				<template #dropdown>
					<el-dropdown-menu>
						<el-dropdown-item disabled>主题</el-dropdown-item>
						<el-dropdown-item divided :icon="Light" command="浅色模式" :disabled="!configStore.layout.isDark">浅色模式</el-dropdown-item>
						<el-dropdown-item :icon="Dark" command="深色模式" :disabled="configStore.layout.isDark">深色模式</el-dropdown-item>
						<el-dropdown-item :icon="Operation" command="系统设置" :disabled="configStore.layout.autoThemMode">
							系统设置
						</el-dropdown-item>
						<el-dropdown-item disabled>谨慎使用</el-dropdown-item>
						<el-dropdown-item divided :icon="Refresh" command="重置系统">重置系统</el-dropdown-item>
					</el-dropdown-menu>
				</template>
			</el-dropdown>
			<el-tour v-model="state.helpTourValue" :show-close="false">
				<el-tour-step :target="helpDropdownRef?.$el" title="显示异常处理" placement="bottom-end">
					<span>如果存在异常显示。</span>
					<br />
					<span>点击右侧图标可进行重置系统操作。</span>
				</el-tour-step>
			</el-tour>
		</template>
	</component>
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent, onMounted, provide, reactive, toRef, useTemplateRef } from "vue";
import { Moon, Operation, Refresh, Sunny } from "@element-plus/icons-vue";
import { ElMessageBox } from "element-plus";
import { Dark, Light } from "@fast-element-plus/icons-vue";
import { Local, Session, getOrCreateInstallationId, installationIdentity, logger, withDefineType } from "@fast-china/utils";
import { defaultThemeColor, useApp, useConfig } from "@/stores";
import type { DropdownInstance, FormRules } from "element-plus";
import type { Component } from "vue";
import type { IFormData, IFormStep, ITenantData } from "./useLogin";
import type { ILoginComponent } from "@/stores";

defineOptions({
	name: "Login",
});

// 登录组件
const loginComponents = withDefineType<Record<ILoginComponent, Component>>({
	ClassicLogin: defineAsyncComponent(() => import("./classicLogin/index.vue")),
	ModernLogin: defineAsyncComponent(() => import("./modernLogin/index.vue")),
	SimpleLogin: defineAsyncComponent(() => import("./simpleLogin/index.vue")),
	SplitLogin: defineAsyncComponent(() => import("./splitLogin/index.vue")),
});

const appStore = useApp();
const configStore = useConfig();

const helpDropdownRef = useTemplateRef<DropdownInstance>("helpDropdownRef");

// 后端返回未知组件名时回退到经典登录页，避免出现空白页面。
const activeLoginComponent = computed(() =>
	Object.hasOwn(loginComponents, appStore.loginComponent)
		? loginComponents[appStore.loginComponent as ILoginComponent]
		: loginComponents.ClassicLogin
);

const state = reactive({
	/** 帮助漫游式引导值 */
	helpTourValue: false,
	/** 表单数据 */
	formData: withDefineType<IFormData>({}),
	/** 表单规则 */
	formRules: withDefineType<FormRules<IFormData>>({
		account: [
			{ required: true, message: "请输入账号", trigger: "blur" },
			{ min: 1, max: 50, message: "账号长度必须为 1~50 个字符", trigger: "blur" },
		],
		password: [
			{ required: true, message: "请输入密码", trigger: "blur" },
			{ min: 6, max: 20, message: "密码长度必须为 6~20 个字符", trigger: "blur" },
		],
		userKey: [{ required: true, message: "请选择租户", trigger: "change" }],
	}),
	/** 租户集合 */
	tenantList: withDefineType<ITenantData[]>([]),
	/** 表单步骤 */
	formStep: withDefineType<IFormStep>("Account"),
	/** 缓存Key */
	cFormKey: "LOGIN_FORM",
});

provide("formData", toRef(state, "formData"));
provide("tenantList", toRef(state, "tenantList"));
provide("formStep", toRef(state, "formStep"));
provide("cFormKey", state.cFormKey);

onMounted(() => {
	try {
		const tenantList = Local.get<ITenantData[]>(state.cFormKey);
		if (tenantList?.length > 0) {
			state.tenantList = tenantList;
			const { formData, tenant } = tenantList[0];
			state.formData = { ...formData, userKey: tenant.userKey };
			state.formStep = "TenantAccount";
		}
	} catch (error) {
		state.helpTourValue = true;
		logger.error("Login", "读取登录缓存失败", error);
	}
});

const handleDropdownClick = (command: string) => {
	switch (command) {
		case "浅色模式":
			configStore.layout.autoThemMode = false;
			configStore.layout.isDark = false;
			configStore.switchDark();
			break;
		case "深色模式":
			configStore.layout.autoThemMode = false;
			configStore.layout.isDark = true;
			configStore.switchDark();
			break;
		case "系统设置":
			configStore.layout.autoThemMode = true;
			configStore.switchAutoThemMode();
			break;
		case "重置系统":
			ElMessageBox.confirm(
				`确定重置系统？<br/><span class="el-text el-text--danger">重置系统将清除所有缓存信息，系统将进行初始化处理，确定要继续执行吗？</span>`,
				{
					dangerouslyUseHTMLString: true,
					type: "warning",
					beforeClose(_, instance) {
						instance.confirmButtonText = "重置中...";
						setTimeout(() => {
							// 获取设备Id
							const deviceId = installationIdentity.deviceId;
							// 清空 Local 缓存
							Local.clear();
							// 清空 Session 缓存
							Session.clear();
							// 重新设置设备Id
							getOrCreateInstallationId(deviceId);
							// 刷新App
							window.location.reload();
						}, 2000);
					},
				}
			);
			break;
	}
};

/**
 * 根据主题色生成向蓝、紫两侧延伸的线性渐变，并适配浅色与深色模式。
 *
 * @param baseColor - HEX 主题色，如 "#0487d0"
 * @param mode - 'light' | 'dark'，默认 'light'
 * @param angleDeg - 渐变角度，默认 165°
 * @returns CSS linear-gradient 字符串
 */
const getThemeGradient = (baseColor: string, mode: "light" | "dark" = "light", angleDeg = 165) => {
	/** 无效 HEX 色值使用默认主题色，再转换为 HSL。 */
	const hexToHsl = (hex: string) => {
		if (!/^#[\da-f]{6}$/iu.test(hex)) hex = defaultThemeColor;
		hex = hex.replace("#", "");
		const r = parseInt(hex.substring(0, 2), 16) / 255;
		const g = parseInt(hex.substring(2, 4), 16) / 255;
		const b = parseInt(hex.substring(4, 6), 16) / 255;

		const max = Math.max(r, g, b);
		const min = Math.min(r, g, b);
		const l = (max + min) / 2;
		const d = max - min;
		let h = 0,
			s = 0;

		if (d !== 0) {
			s = d / (1 - Math.abs(2 * l - 1));
			switch (max) {
				case r:
					h = ((g - b) / d) % 6;
					break;
				case g:
					h = (b - r) / d + 2;
					break;
				case b:
					h = (r - g) / d + 4;
					break;
			}
			h = Math.round(h * 60);
			if (h < 0) h += 360;
		}

		return { h, s: s * 100, l: l * 100 };
	};

	/** 将调整后的 HSL 转回 HEX。 */
	const hslToHex = (h: number, s: number, l: number) => {
		s /= 100;
		l /= 100;
		const k = (n: number) => (n + h / 30) % 12;
		const a = s * Math.min(l, 1 - l);
		const f = (n: number) => l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
		const toHex = (x: number) =>
			Math.round(x * 255)
				.toString(16)
				.padStart(2, "0");
		return `#${toHex(f(0))}${toHex(f(8))}${toHex(f(4))}`;
	};

	const base = hexToHsl(baseColor);

	// 调整系数（light / dark 模式下不同）
	const tone =
		mode === "light"
			? { l: 1.0, s: 1.0 } // 浅色：保持原亮度
			: { l: 0.7, s: 0.9 }; // 深色：整体降低亮度饱和度

	/**
	 * 渐变层定义（蓝紫系层次感）
	 */
	const variations = [
		// 深紫蓝
		{ h: base.h + 30, s: base.s * 0.6 * tone.s, l: base.l * 0.85 * tone.l },
		// 主色
		{ h: base.h, s: base.s * 1.0 * tone.s, l: base.l * 1.0 * tone.l },
		// 冷蓝
		{ h: base.h - 10, s: base.s * 0.8 * tone.s, l: base.l * 0.9 * tone.l },
		// 亮蓝紫
		{ h: base.h + 45, s: base.s * 0.6 * tone.s, l: base.l * 1.4 * tone.l },
	];

	const colors = variations.map((variation) =>
		hslToHex(((variation.h % 360) + 360) % 360, Math.max(0, Math.min(variation.s, 100)), Math.max(0, Math.min(variation.l, 100)))
	);

	return `linear-gradient(${angleDeg}deg, ${colors.join(", ")})`;
};
</script>

<style lang="scss" scoped>
.help_dropdown {
	position: fixed;
	top: max(16px, env(safe-area-inset-top));
	right: max(16px, env(safe-area-inset-right));
	z-index: 2001;
	cursor: pointer;
}

.help-trigger {
	width: 40px;
	height: 40px;
	color: var(--el-text-color-primary);
	border: 1px solid rgb(255 255 255 / 58%);
	background-color: rgb(255 255 255 / 62%);
	box-shadow: 0 8px 24px rgb(15 23 42 / 12%);
	backdrop-filter: blur(14px) saturate(1.15);
	transition:
		color 180ms ease,
		background-color 180ms ease,
		box-shadow 180ms ease,
		transform 180ms ease;

	&:hover,
	&:focus-visible {
		color: var(--el-color-primary);
		background-color: rgb(255 255 255 / 82%);
		box-shadow: 0 12px 28px rgb(15 23 42 / 16%);
		transform: translateY(-2px) rotate(8deg);
	}
}

html.dark .help_dropdown .help-trigger {
	color: rgb(226 232 240 / 86%);
	border-color: rgb(255 255 255 / 10%);
	background-color: rgb(12 20 35 / 66%);
	box-shadow: 0 10px 28px rgb(0 0 0 / 32%);

	&:hover,
	&:focus-visible {
		color: #fff;
		background-color: rgb(20 31 51 / 86%);
	}
}
</style>

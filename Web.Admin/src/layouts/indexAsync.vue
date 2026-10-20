<template>
	<suspense>
		<template #default>
			<Watermark v-if="configStore.layout.watermark">
				<component
					:is="layoutComponents[layoutMode]"
					class="layout"
					:class="{
						'is-mobile': isMobile,
						'is-tablet': isTablet,
					}"
				/>
			</Watermark>
			<component
				:is="layoutComponents[layoutMode]"
				v-else
				class="layout"
				:class="{
					'is-mobile': isMobile,
					'is-tablet': isTablet,
				}"
			/>
		</template>
		<template #fallback>
			<Loading loading-text="系统初始化中..." />
		</template>
	</suspense>
	<LayoutConfig ref="layoutConfigRef" />
	<MenuSearch ref="menuSearchRef" />
	<ChangePassword ref="changePasswordRef" />
	<AccountEdit ref="accountEditRef" />
	<EmployeeEdit ref="employeeEditRef" />
	<ChangeMobile ref="changeMobileRef" />
	<ChangeEmail ref="changeEmailRef" />
	<IdentityVerification />
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent, provide, useTemplateRef } from "vue";
import { useWindowSize, withDefineType } from "@fast-china/utils";
import AccountEdit from "@/layouts/components/AccountEdit/index.vue";
import ChangeEmail from "@/layouts/components/ChangeEmail/index.vue";
import ChangeMobile from "@/layouts/components/ChangeMobile/index.vue";
import ChangePassword from "@/layouts/components/ChangePassword/index.vue";
import LayoutConfig from "@/layouts/components/Config/index.vue";
import EmployeeEdit from "@/layouts/components/EmployeeEdit/index.vue";
import IdentityVerification from "@/layouts/components/IdentityVerification/index.vue";
import MenuSearch from "@/layouts/components/MenuSearch/index.vue";
import { useConfig } from "@/stores";
import { accountEditKey, changeEmailKey, changeMobileKey, changePasswordKey, employeeEditKey, layoutConfigKey, menuSearchKey } from "./index";
import type { Component } from "vue";
import type { IModeName } from "@/stores";

defineOptions({
	name: "LayoutAsync",
});

const configStore = useConfig();
const windowSize = useWindowSize();

const layoutConfigRef = useTemplateRef<InstanceType<typeof LayoutConfig>>("layoutConfigRef");
const menuSearchRef = useTemplateRef<InstanceType<typeof MenuSearch>>("menuSearchRef");
const changePasswordRef = useTemplateRef<InstanceType<typeof ChangePassword>>("changePasswordRef");
const accountEditRef = useTemplateRef<InstanceType<typeof AccountEdit>>("accountEditRef");
const employeeEditRef = useTemplateRef<InstanceType<typeof EmployeeEdit>>("employeeEditRef");
const changeMobileRef = useTemplateRef<InstanceType<typeof ChangeMobile>>("changeMobileRef");
const changeEmailRef = useTemplateRef<InstanceType<typeof ChangeEmail>>("changeEmailRef");
provide(layoutConfigKey, layoutConfigRef);
provide(menuSearchKey, menuSearchRef);
provide(changePasswordKey, changePasswordRef);
provide(accountEditKey, accountEditRef);
provide(employeeEditKey, employeeEditRef);
provide(changeMobileKey, changeMobileRef);
provide(changeEmailKey, changeEmailRef);

/** 是否手机端。 */
const isMobile = computed(() => windowSize.width.value < 768);
/** 是否平板端。 */
const isTablet = computed(() => windowSize.width.value >= 768 && windowSize.width.value < 1200);

/** 实际使用的布局模式（手机、平板端使用经典布局，不覆盖用户的桌面端偏好） */
const layoutMode = computed<IModeName>(() => {
	if (isMobile.value || isTablet.value) return "Classic";
	return configStore.layout.layoutMode;
});

const layoutComponents = withDefineType<Record<IModeName, Component>>({
	Classic: defineAsyncComponent(() => import("@/layouts/LayoutClassic/index.vue")),
	Horizontal: defineAsyncComponent(() => import("@/layouts/LayoutHorizontal/index.vue")),
	Mixed: defineAsyncComponent(() => import("@/layouts/LayoutMixed/index.vue")),
});
</script>

<style lang="scss" scoped>
.layout {
	width: 100%;
	height: 100%;
}
</style>

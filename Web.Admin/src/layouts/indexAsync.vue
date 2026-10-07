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
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent, provide, useTemplateRef } from "vue";
import { useWindowSize, withDefineType } from "@fast-china/utils";
import ChangePassword from "@/layouts/components/ChangePassword/index.vue";
import LayoutConfig from "@/layouts/components/Config/index.vue";
import MenuSearch from "@/layouts/components/MenuSearch/index.vue";
import { useConfig } from "@/stores";
import { changePasswordKey, layoutConfigKey, menuSearchKey } from "./index";
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
provide(layoutConfigKey, layoutConfigRef);
provide(menuSearchKey, menuSearchRef);
provide(changePasswordKey, changePasswordRef);

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

<style scoped lang="scss">
.layout {
	width: 100%;
	height: 100%;
}
</style>

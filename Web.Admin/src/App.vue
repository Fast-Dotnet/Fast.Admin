<template>
	<el-config-provider v-bind="elConfigProviderProps">
		<RouterView></RouterView>
	</el-config-provider>
</template>

<script lang="ts" setup>
import { onMounted, reactive, watch } from "vue";
import { RouterView } from "vue-router";
import { useWindowSize } from "@fast-china/utils";
import { useConfig } from "@/stores";
import type { ConfigProviderProps } from "element-plus";

defineOptions({
	name: "App",
});

const windowSize = useWindowSize();
const configStore = useConfig();

type Mutable<T> = {
	-readonly [K in keyof T]: T[K];
};

const elConfigProviderProps = reactive<Partial<Mutable<ConfigProviderProps>>>({
	// 语言
	locale: undefined,
	button: {
		// 自动插入空格
		autoInsertSpace: false,
	},
	emptyValues: [undefined, null],
	valueOnClear: null,
	size: "default",
});

// 初始化主题
configStore.initTheme();

/** 自动大小 */
const handleAutoSize = () => {
	const html = document.documentElement;
	if (windowSize.width.value > 1366) {
		// 1366 以上使用默认
		elConfigProviderProps.size = "default";
		html.classList.remove("small");
		html.classList.add("default");
		configStore.layout.layoutSize = "default";
		configStore.setDefaultLayoutSize();
	} else {
		elConfigProviderProps.size = "small";
		html.classList.remove("default");
		html.classList.add("small");
		configStore.layout.layoutSize = "small";
		configStore.setSmallLayoutSize();
	}
};

/** 监听页面宽度，设置组件大小 */
watch(
	() => [windowSize.width.value, windowSize.height.value],
	() => {
		configStore.layout.autoSize && handleAutoSize();
	},
	{ immediate: true }
);

/** 监听是否自动大小 */
watch(
	() => [configStore.layout.autoSize, configStore.layout.layoutSize],
	() => {
		if (configStore.layout.autoSize) {
			handleAutoSize();
		} else {
			const html = document.documentElement;
			if (configStore.layout.layoutSize === "default") {
				elConfigProviderProps.size = "default";
				html.classList.remove("small");
				html.classList.remove("large");
				html.classList.add("default");
				configStore.setDefaultLayoutSize();
			} else if (configStore.layout.layoutSize === "small") {
				elConfigProviderProps.size = "small";
				html.classList.remove("default");
				html.classList.remove("large");
				html.classList.add("small");
				configStore.setSmallLayoutSize();
			} else if (configStore.layout.layoutSize === "large") {
				elConfigProviderProps.size = "large";
				html.classList.remove("default");
				html.classList.remove("small");
				html.classList.add("large");
				configStore.setDefaultLayoutSize();
			}
		}
	},
	{ immediate: true }
);

onMounted(async () => {
	if (import.meta.env.DEV) {
		const { default: zhCn } = await import("element-plus/es/locale/lang/zh-cn");
		elConfigProviderProps.locale = zhCn;
	} else {
		elConfigProviderProps.locale = ElementPlusLocaleZhCn;
	}
});
</script>

<template>
	<RouterLink class="logo" :style="{ '--height': addCssUnit(configStore.layout.navBarHeight) }" title="首页" to="/">
		<img :src="logo" alt="Logo" @error="setFallBack" />
		<span v-if="!configStore.layout.menuCollapse" :title="appStore.appName">
			{{ userInfoStore.shortName || appStore.appName }}
		</span>
	</RouterLink>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { RouterLink } from "vue-router";
import { addCssUnit } from "@fast-china/utils";
import LogoImg from "@/assets/logo.png";
import { useApp, useConfig, useUserInfo } from "@/stores";

defineOptions({
	name: "Logo",
});

const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();

const logo = computed(() => userInfoStore.logoUrl || appStore.logoUrl || LogoImg);

const setFallBack = (event: Event) => {
	const target = event.target as HTMLImageElement;
	target.src = LogoImg;
};
</script>

<style lang="scss" scoped>
html.small {
	.logo {
		span {
			font-size: var(--el-font-size-medium);
		}
	}
}

.logo {
	display: flex;
	flex-shrink: 0;
	align-items: center;
	justify-content: center;
	gap: 10px;
	width: var(--width);
	min-width: 0;
	max-width: 100%;
	height: var(--height);
	padding: 0 10px;
	text-decoration: none;
	color: inherit;
	// border-bottom: var(--el-border);
	cursor: pointer;

	img {
		flex-shrink: 0;
		max-width: 80px;
		height: 80%;
		object-fit: contain;
	}

	span {
		display: block;
		overflow: hidden;
		min-width: 0;
		max-width: 100%;
		font-size: var(--el-font-size-large);
		font-weight: 600;
		white-space: nowrap;
		text-overflow: ellipsis;
	}
}
</style>

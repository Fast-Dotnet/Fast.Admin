<template>
	<div class="logo" :style="{ '--height': addCssUnit(configStore.layout.navBarHeight) }" title="首页" @click="router.push('/')">
		<img :src="logo" alt="Logo" @error="setFallBack" />
		<span v-if="!configStore.layout.menuCollapse" :title="appStore.appName">
			{{ userInfoStore.shortName || appStore.appName }}
		</span>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { addCssUnit } from "@fast-china/utils";
import LogoImg from "@/assets/logo.png";
import { useApp, useConfig, useUserInfo } from "@/stores";

defineOptions({
	name: "Logo",
});

const router = useRouter();
const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();

const logo = computed(() => userInfoStore.logoUrl || appStore.logoUrl || LogoImg);

const setFallBack = (event: Event) => {
	const target = event.target as HTMLImageElement;
	target.src = LogoImg;
};
</script>

<style scoped lang="scss">
html.small {
	.logo {
		span {
			font-size: var(--el-font-size-medium);
		}
	}
}

.logo {
	width: var(--width);
	height: var(--height);
	display: flex;
	align-items: center;
	justify-content: center;
	gap: 10px;
	padding: 0 10px;
	// border-bottom: var(--el-border);
	cursor: pointer;
	img {
		height: 80%;
	}
	span {
		font-size: var(--el-font-size-large);
		font-weight: 600;
		max-width: 100%;
		display: block;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}
}
</style>

<template>
	<wd-watermark :rotate="-38" :opacity="0.5" :width="100" :gutter-x="50" :height="150" :size="12" :color="color" :content="content" />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useApp, useConfig, useUserInfo } from "@/stores";

defineOptions({
	name: "Watermark",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const appStore = useApp();
const configStore = useConfig();
const userInfoStore = useUserInfo();

const color = computed(() => (configStore.layout.isDark ? "rgba(255,255,255,0.2)" : "rgba(0,0,0,0.2)"));
const content = computed(() => {
	let watermarkContent = appStore.appName;
	if (userInfoStore.tenantName) {
		watermarkContent = userInfoStore.tenantName;
	}
	if (userInfoStore.hasUserInfo) {
		watermarkContent += ` - ${userInfoStore.employeeName || userInfoStore.nickName}`;
	}

	return watermarkContent;
});
</script>

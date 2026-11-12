<template>
	<web-view :src="webUrl" />
</template>

<script setup lang="ts">
import { onLoad } from "@dcloudio/uni-app";
import { ref } from "vue";

definePage({
	name: "WebView",
	layout: false,
	noLogin: true,
});

const webUrl = ref("");

onLoad((option) => {
	if (option?.title) {
		uni.setNavigationBarTitle({
			title: decodeURIComponent(option.title),
		});
	}
	if (option?.url) {
		webUrl.value = decodeURIComponent(option.url);
	}
});
</script>

<template>
	<iframe :src="decodeURIComponent(route.query.url as string)" frameborder="0" style="width: 100%; height: 100%" />
</template>

<script lang="ts" setup>
import { useRoute } from "vue-router";

defineOptions({
	// eslint-disable-next-line vue/no-reserved-component-names -- 保持现有组件名称与项目引用兼容
	name: "Iframe",
});

const route = useRoute();
</script>

<template>
	<div class="fa-error-page">
		<Page403 />
		<router-link to="/">
			<el-button type="primary">回到首页</el-button>
		</router-link>
	</div>
</template>

<script setup lang="ts">
import { Page403 } from "@fast-element-plus/icons-vue";

defineOptions({
	name: "403",
});
</script>

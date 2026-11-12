<template>
	<div class="fa-error-page">
		<Page404 />
		<router-link to="/">
			<el-button type="primary">回到首页</el-button>
		</router-link>
	</div>
</template>

<script setup lang="ts">
import { Page404 } from "@fast-element-plus/icons-vue";

defineOptions({
	name: "NotFound",
});
</script>

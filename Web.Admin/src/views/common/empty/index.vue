<template>
	<div class="fa-error-page">
		<el-empty description="这是一个空的，什么都没有的页面~" />
		<router-link to="/">
			<el-button type="primary">回到首页</el-button>
		</router-link>
	</div>
</template>

<script setup lang="ts">
defineOptions({
	name: "Empty",
});
</script>

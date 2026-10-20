<template>
	<el-breadcrumb separator="/">
		<transition-group appear name="slide-left" leave-active-class="">
			<el-breadcrumb-item v-if="!isHome" key="/dashboard" to="/dashboard">首页</el-breadcrumb-item>
			<el-breadcrumb-item v-for="item in breadcrumbs" :key="item.path" :to="item.redirect ? item.redirect.toString() : undefined">
				{{ item.meta.title }}
			</el-breadcrumb-item>
		</transition-group>
	</el-breadcrumb>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

defineOptions({
	name: "Breadcrumb",
});

const route = useRoute();

const isHome = computed(() => route.path === "/dashboard");

const breadcrumbs = computed(() => {
	const matched = route.matched.filter((item) => item.meta?.title && item.path !== "/dashboard");
	return matched.filter((item, index) => item.meta.title !== matched[index - 1]?.meta.title);
});
</script>

<style lang="scss" scoped>
.el-breadcrumb {
	display: flex;
	overflow: hidden;
	min-width: 0;
	padding-left: 10px;
	font-weight: var(--el-font-weight-primary);
	white-space: nowrap;
}

.el-breadcrumb__item {
	flex-shrink: 0;

	:deep(.el-breadcrumb__inner) {
		display: inline-flex;
		gap: 5px;
		transition: var(--el-transition-color);
	}

	&:last-child {
		flex-shrink: 1;
		min-width: 0;

		:deep(.el-breadcrumb__inner) {
			display: block;
			overflow: hidden;
			text-overflow: ellipsis;

			.fa-icon {
				color: var(--el-text-color-primary);
			}

			span {
				color: var(--el-text-color-placeholder);
			}
		}
	}
}

html.small {
	.el-breadcrumb {
		font-size: var(--el-font-size-extra-small);
	}
}
</style>

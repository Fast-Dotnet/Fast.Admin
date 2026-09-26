<template>
	<el-menu
		router
		mode="horizontal"
		:default-active="activeMenu"
		:style="{ '--el-menu-horizontal-height': addCssUnit(configStore.layout.navBarHeight) }"
	>
		<el-menu-item index="/dashboard" @click="router.push('/dashboard')">
			<FaIcon name="fa-icon-Dashboard" />
			<template #title>
				<span>首页</span>
			</template>
		</el-menu-item>
		<el-menu-item
			v-for="(item, idx) in topMenuList"
			:key="idx"
			:index="item.firstChild?.router || item.firstChild?.link"
			@click="handleMenuClick(item)"
		>
			<FaIcon v-if="item.icon" :name="item.icon" />
			<template #title>
				<span>{{ item.menuName }}</span>
			</template>
		</el-menu-item>
	</el-menu>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { addCssUnit } from "@fast-china/utils";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import { useConfig, useUserInfo } from "@/stores";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

defineOptions({
	name: "LayoutMixedTopMenu",
});

const router = useRouter();
const configStore = useConfig();
const userInfoStore = useUserInfo();

const activeMenu = ref("/dashboard");

const emit = defineEmits<{
	menuChange: [menu: AuthMenuInfoDto];
}>();

/** 查找第一个叶子菜单 */
const findFirstLeaf = (menu: AuthMenuInfoDto): AuthMenuInfoDto => {
	if (!menu.children || menu.children.length === 0) {
		return menu;
	}
	for (const child of menu.children) {
		const leaf = findFirstLeaf(child);
		if (leaf) return leaf;
	}
	return null;
};

const topMenuList = computed(() => {
	return userInfoStore.menuList
		.filter((f) => f.visible)
		.map((item) => {
			const firstChild = findFirstLeaf(item);
			return {
				...item,
				firstChild,
			};
		});
});

/** 判断路径是否在模块中 */
const isPathInModule = (menu: AuthMenuInfoDto, path: string) => {
	if (menu.router === path) return true;
	if (menu.children) {
		for (const child of menu.children) {
			if (isPathInModule(child, path)) return true;
		}
	}
	return false;
};

/** 根据当前路由匹配顶部模块 */
const matchModule = () => {
	for (const item of topMenuList.value) {
		if (isPathInModule(item, router.currentRoute.value.path)) {
			activeMenu.value = item.firstChild?.router || item.firstChild?.link;
			emit("menuChange", item);
			return;
		}
	}
	activeMenu.value = "/dashboard";
};

const handleMenuClick = (item: AuthMenuInfoDto & { firstChild: AuthMenuInfoDto }) => {
	emit("menuChange", item);
	switch (item.menuType) {
		case MenuTypeEnum.Catalog:
			break;
		case MenuTypeEnum.Menu:
			router.push(item.firstChild.router);
			break;
		case MenuTypeEnum.Internal:
			router.push({
				path: "/iframe",
				query: { url: item.firstChild.link },
			});
			break;
		case MenuTypeEnum.Outside:
			window.open(item.firstChild.link, "_blank");
			break;
	}
};

watch(() => router.currentRoute.value.path, matchModule, { immediate: true });
</script>

<style scoped lang="scss">
.el-menu {
	width: 100%;
	border: none;
	white-space: nowrap;
	overflow: hidden;
	:deep() {
		.el-sub-menu {
			&.is-active {
				.el-sub-menu__title {
					font-weight: 600;
					border-bottom: 3px solid var(--el-menu-active-color);
				}
			}
		}
		.el-menu-item {
			&.is-active {
				font-weight: 600;
				border-bottom: 3px solid var(--el-menu-active-color);
			}
		}
	}
}
</style>

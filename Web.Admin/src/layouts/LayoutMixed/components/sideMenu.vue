<template>
	<el-scrollbar>
		<el-menu
			unique-opened
			:default-active="activeMenu"
			:collapse="configStore.layout.menuCollapse"
			:style="{ '--el-menu-item-height': addCssUnit(configStore.layout.menuHeight) }"
		>
			<MenuItem v-for="(item, idx) in sideMenuList" :key="idx" :menu="item" />
		</el-menu>
	</el-scrollbar>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { addCssUnit, definePropType } from "@fast-china/utils";
import MenuItem from "@/layouts/components/MenuItem/index.vue";
import { useConfig } from "@/stores";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

defineOptions({
	name: "LayoutMixedSideMenu",
});

const router = useRouter();
const configStore = useConfig();

const props = defineProps({
	/** 当前激活的顶部菜单 */
	activeTopMenu: definePropType<AuthMenuInfoDto>(Object),
});

const activeMenu = computed(() => router.currentRoute.value.path);

const sideMenuList = computed(() => {
	if (!props.activeTopMenu) return [];
	// 如果顶部菜单有子菜单，显示子菜单；否则显示顶部菜单自身（避免左侧空白）
	if (props.activeTopMenu.children?.length > 0) {
		return props.activeTopMenu.children.filter((f) => f.visible);
	}
	// 顶部菜单没有子菜单时，显示自身
	return props.activeTopMenu.visible ? [props.activeTopMenu] : [];
});
</script>

<style lang="scss" scoped>
.el-scrollbar {
	flex: 1;
}

:deep(.el-scrollbar__view) {
	padding: 0 5px;
}

.el-menu {
	height: 100%;
	border: none;
}

:deep(.el-sub-menu.is-active .el-sub-menu__title.el-tooltip__trigger) {
	font-weight: 600;
	// background-color: var(--el-menu-active-color);
	background: none;

	.el-icon {
		color: var(--el-color-white);
	}

	* {
		z-index: 2;
	}

	&::before {
		position: absolute;
		top: 5px;
		bottom: 5px;
		left: 0;
		right: 0;
		z-index: 1;
		content: "";
		border-radius: 3px;
		background-color: var(--el-menu-active-color);
	}
}

:deep(.el-sub-menu .el-sub-menu__title) {
	* {
		z-index: 2;
	}

	&:hover {
		// background-color: var(--el-menu-hover-bg-color);
		background: none;

		.el-icon {
			color: var(--el-menu-text-color);
		}

		&::before {
			position: absolute;
			top: 5px;
			bottom: 5px;
			left: 0;
			right: 0;
			z-index: 1;
			content: "";
			border-radius: 3px;
			background-color: var(--el-menu-hover-bg-color);
		}
	}
}

:deep(.el-menu-item) {
	* {
		z-index: 2;
	}

	&:hover {
		// background-color: var(--el-menu-hover-bg-color);
		background: none;

		&::before {
			position: absolute;
			top: 5px;
			bottom: 5px;
			left: 0;
			right: 0;
			z-index: 1;
			content: "";
			border-radius: 3px;
			background-color: var(--el-menu-hover-bg-color);
		}
	}

	&.is-active {
		font-weight: 600;
		color: var(--el-color-white);
		// background-color: var(--el-menu-active-color);
		background: none;
	}

	&.is-active::before {
		position: absolute;
		top: 5px;
		bottom: 5px;
		left: 0;
		right: 0;
		z-index: 1;
		content: "";
		border-radius: 3px;
		background-color: var(--el-menu-active-color);
	}
}

.el-menu--collapse {
	--el-menu-base-level-padding: 10px;
}

html.small .el-menu {
	--el-menu-item-font-size: var(--el-font-size-small);
}
</style>

<template>
	<el-sub-menu
		v-if="props.menu.children?.length > 0"
		:index="props.menu.children[0].router || props.menu.children[0].link"
		:title="props.menu.menuTitle || props.menu.menuName"
	>
		<template #title>
			<FaIcon v-if="props.menu.icon" :name="props.menu.icon" />
			<span>{{ props.menu.menuName }}</span>
		</template>
		<MenuItem v-for="(item, idx) in props.menu.children" :key="idx" :menu="item" />
	</el-sub-menu>
	<el-menu-item v-else :index="props.menu.router || props.menu.link" @click="handleMenuClick">
		<FaIcon v-if="props.menu.icon" :name="props.menu.icon" />
		<template #title>
			<span>{{ props.menu.menuName }}</span>
		</template>
	</el-menu-item>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { definePropType } from "@fast-china/utils";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import MenuItem from "./index.vue";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

defineOptions({
	name: "LayoutMenuItem",
});

const router = useRouter();

const props = defineProps({
	/** 菜单 */
	menu: definePropType<AuthMenuInfoDto>(Object),
});

const handleMenuClick = () => {
	switch (props.menu.menuType) {
		case MenuTypeEnum.Catalog:
			break;
		case MenuTypeEnum.Menu:
			router.push(props.menu.router);
			break;
		case MenuTypeEnum.Internal:
			router.push({
				path: "/iframe",
				query: { url: props.menu.link },
			});
			break;
		case MenuTypeEnum.Outside:
			window.open(props.menu.link, "_blank");
			break;
	}
};
</script>

<template>
	<div>
		<el-dialog
			v-model="state.visible"
			:show-close="false"
			width="600px"
			top="15vh"
			:close-on-click-modal="true"
			:close-on-press-escape="true"
			@opened="handleDialogOpened"
		>
			<el-input
				ref="inputRef"
				v-model="state.keyword"
				size="large"
				placeholder="搜索菜单..."
				:prefix-icon="Search"
				clearable
				@input="handleSearch"
			/>

			<div class="search-result">
				<el-scrollbar max-height="min(400px, 50dvh)">
					<div v-if="state.resultList.length === 0 && state.keyword" class="search-empty">
						<el-empty description="暂无搜索结果" :image-size="80" />
					</div>

					<div
						v-for="(item, idx) in state.resultList"
						:key="idx"
						ref="itemRefs"
						:class="['search-item', { 'is-active': state.activeIndex === idx }]"
						@click="handleSelect(item)"
						@mouseenter="state.activeIndex = idx"
					>
						<el-icon>
							<FaIcon v-if="item.icon" :name="item.icon" />
							<Menu v-else />
						</el-icon>

						<div class="search-item__info">
							<span class="search-item__title">{{ item.menuName }}</span>
							<span v-if="item.parentName" class="search-item__path">
								{{ item.parentName }}
							</span>
						</div>

						<el-icon class="search-item__enter">
							<Right />
						</el-icon>
					</div>
				</el-scrollbar>
			</div>

			<div class="search-footer">
				<span class="search-footer__tip">
					<kbd>↑</kbd>
					<kbd>↓</kbd>
					导航
				</span>

				<span class="search-footer__tip">
					<kbd>↵</kbd>
					确认
				</span>

				<span class="search-footer__tip">
					<kbd>Esc</kbd>
					关闭
				</span>
			</div>
		</el-dialog>
	</div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, reactive, useTemplateRef, watch } from "vue";
import { useRouter } from "vue-router";
import { Menu, Right, Search } from "@element-plus/icons-vue";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import { useUserInfo } from "@/stores";
import type { InputInstance } from "element-plus";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

defineOptions({
	name: "MenuSearch",
});

type ISearchItem = {
	menuName: string;
	icon?: string;
	router?: string;
	link?: string;
	menuType?: MenuTypeEnum;
	parentName?: string;
};

const router = useRouter();
const userInfoStore = useUserInfo();

const inputRef = useTemplateRef<InputInstance>("inputRef");
const itemRefs = useTemplateRef<HTMLDivElement[]>("itemRefs");

const state = reactive({
	visible: false,
	keyword: "",
	resultList: [] as ISearchItem[],
	activeIndex: 0,
	allMenus: [] as ISearchItem[],
});

/** 扁平化菜单 */
const flattenMenus = (menus: AuthMenuInfoDto[], parentName = "") => {
	const result: ISearchItem[] = [];

	for (const menu of menus) {
		if (!menu.visible) continue;

		if (menu.children?.length) {
			result.push(...flattenMenus(menu.children, menu.menuName));
		} else if (menu.router || menu.link) {
			result.push({
				menuName: menu.menuName,
				icon: menu.icon,
				router: menu.router,
				link: menu.link,
				menuType: menu.menuType,
				parentName,
			});
		}
	}

	return result;
};

/** 打开搜索 */
const handleOpen = () => {
	state.visible = true;
	state.keyword = "";
	state.resultList = [];
	state.activeIndex = 0;

	state.allMenus = flattenMenus(userInfoStore.menuList);
};

/** dialog 打开完成 */
const handleDialogOpened = () => {
	nextTick(() => {
		inputRef.value?.focus();
	});
};

/** 搜索 */
const handleSearch = () => {
	state.activeIndex = 0;

	if (!state.keyword.trim()) {
		state.resultList = [];
		return;
	}

	const keyword = state.keyword.toLowerCase();

	state.resultList = state.allMenus.filter((i) => i.menuName.toLowerCase().includes(keyword) || i.parentName?.toLowerCase().includes(keyword));
};

/** 选择 */
const handleSelect = (item: ISearchItem) => {
	state.visible = false;

	switch (item.menuType) {
		case MenuTypeEnum.Catalog:
			break;

		case MenuTypeEnum.Menu:
			router.push(item.router);
			break;

		case MenuTypeEnum.Internal:
			router.push({
				path: "/iframe",
				query: { url: item.link },
			});
			break;

		case MenuTypeEnum.Outside:
			window.open(item.link, "_blank");
			break;
	}
};

/** 滚动到选中项 */
const scrollToActive = () => {
	nextTick(() => {
		const el = itemRefs.value[state.activeIndex];
		if (!el) return;

		el.scrollIntoView({
			block: "nearest",
		});
	});
};

/** 键盘控制 */
const handleKeydown = (event: KeyboardEvent) => {
	if (!state.visible) {
		if ((event.ctrlKey || event.metaKey) && event.key === "k") {
			event.preventDefault();
			handleOpen();
		}
		return;
	}

	switch (event.key) {
		case "ArrowUp":
			event.preventDefault();
			state.activeIndex = state.activeIndex > 0 ? state.activeIndex - 1 : state.resultList.length - 1;
			break;

		case "ArrowDown":
			event.preventDefault();
			state.activeIndex = state.activeIndex < state.resultList.length - 1 ? state.activeIndex + 1 : 0;
			break;

		case "Enter":
			event.preventDefault();
			if (state.resultList.length) {
				handleSelect(state.resultList[state.activeIndex]);
			}
			break;
	}
};

watch(
	() => state.activeIndex,
	() => scrollToActive()
);

onMounted(() => {
	document.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
	document.removeEventListener("keydown", handleKeydown);
});

defineExpose({
	open: handleOpen,
});
</script>

<style lang="scss" scoped>
:deep(.el-dialog) {
	border-radius: 12px;

	.el-dialog__header {
		display: none;
	}

	.el-dialog__body {
		padding: 0;
	}

	.el-input {
		padding: 0 16px 14px;
		border-bottom: var(--el-border);
	}
}

.search-result {
	min-height: 80px;
}

.search-empty {
	padding: 20px 0;
}

.search-item {
	display: flex;
	align-items: center;
	gap: 10px;
	padding: 10px 16px;
	cursor: pointer;
	transition: background-color var(--el-transition-duration);

	&:hover,
	&.is-active {
		background-color: var(--el-fill-color-light);
	}

	.el-icon {
		flex-shrink: 0;
		font-size: 18px;
		color: var(--el-text-color-secondary);
	}
}

.search-item__info {
	display: flex;
	flex: 1;
	flex-direction: column;
	min-width: 0;
}

.search-item__title {
	font-size: var(--el-font-size-base);
	color: var(--el-text-color-primary);
}

.search-item__path {
	font-size: var(--el-font-size-extra-small);
	color: var(--el-text-color-secondary);
}

.search-item__title,
.search-item__path {
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.search-item__enter {
	font-size: 14px;
	color: var(--el-text-color-placeholder);
}

.search-footer {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: center;
	row-gap: 6px;
	gap: 16px;
	padding: 10px 16px 0;
	border-top: var(--el-border);
}

.search-footer__tip {
	display: flex;
	align-items: center;
	gap: 4px;
	font-size: var(--el-font-size-extra-small);
	color: var(--el-text-color-secondary);

	kbd {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-width: 20px;
		height: 20px;
		padding: 0 4px;
		font-size: 11px;
		border: 1px solid var(--el-border-color);
		border-radius: 4px;
		background-color: var(--el-fill-color);
	}
}

@media (prefers-reduced-motion: reduce) {
	.search-item {
		transition: none;
	}
}
</style>

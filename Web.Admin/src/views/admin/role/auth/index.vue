<template>
	<FaDialog
		ref="faDialogRef"
		style="height: min(620px, calc(100dvh - 64px))"
		width="1000"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="handleClose"
	>
		<template #default="{ loading }">
			<div class="role-auth">
				<nav class="menu-pane">
					<el-input v-model="state.searchKeyword" :prefix-icon="Search" placeholder="搜索页面" clearable />
					<el-scrollbar class="menu-scrollbar">
						<div v-for="node in filteredMenuList" :key="node.value" class="menu-node">
							<button
								v-if="node.data?.isModule"
								class="module-item"
								:class="{ 'is-expanded': isModuleExpanded(node.value) }"
								type="button"
								:disabled="Boolean(state.searchKeyword)"
								@click="toggleModule(node.value)"
							>
								<el-icon class="module-arrow"><ArrowRight /></el-icon>
								<span class="menu-label">{{ node.label }}</span>
								<span class="operation-count">{{ getModuleCount(node.value) }}</span>
							</button>
							<div v-show="isModuleExpanded(node.value)" :class="{ 'module-menus': node.data?.isModule }">
								<button
									v-for="menu in node.data?.isModule ? node.children : [node]"
									:key="menu.value"
									class="menu-item"
									:class="{ 'is-current': selectedMenu?.value === menu.value }"
									type="button"
									@click="state.selectedMenuId = menu.value"
								>
									<span class="menu-status" :class="{ 'is-authorized': isMenuChecked(menu.value) }" />
									<span class="menu-label">{{ menu.label }}</span>
									<span class="operation-count">{{ getButtonCheckedCount(menu) }} / {{ menu.children?.length ?? 0 }}</span>
								</button>
							</div>
						</div>
					</el-scrollbar>
				</nav>
				<section v-if="selectedMenu" :key="selectedMenu.value" class="permission-pane">
					<header class="page-header">
						<div>
							<p v-if="selectedModule" class="page-module">{{ selectedModule.label }}</p>
							<h2>{{ selectedMenu.label }}</h2>
							<div class="page-platforms">
								<el-tag v-if="selectedMenu.data?.hasWeb" type="info" effect="plain">
									<FaIcon name="el-icon-ChromeFilled" />
									Web
								</el-tag>
								<el-tag v-if="selectedMenu.data?.hasMobile" type="info" effect="plain">
									<FaIcon name="el-icon-Iphone" />
									移动端
								</el-tag>
								<el-tag v-if="selectedMenu.data?.hasDesktop" type="info" effect="plain">
									<FaIcon name="el-icon-Monitor" />
									Windows
								</el-tag>
							</div>
						</div>
						<div class="page-toggle">
							<label>页面授权</label>
							<el-switch :model-value="isMenuChecked(selectedMenu.value)" @change="handleMenuChange(selectedMenu.value, $event)" />
						</div>
					</header>
					<div class="operation-header">
						<h3>操作权限</h3>
						<span v-if="selectedButtonList.length" class="operation-count">
							已选操作 {{ selectedButtonCount }} / {{ selectedButtonList.length }}
						</span>
						<el-checkbox
							v-if="selectedButtonList.length"
							class="check-all"
							:model-value="selectedButtonCount === selectedButtonList.length"
							:indeterminate="selectedButtonCount > 0 && selectedButtonCount < selectedButtonList.length"
							@change="handleButtonAllChange"
						>
							全选
						</el-checkbox>
					</div>
					<el-checkbox-group v-if="selectedButtonList.length" v-model="state.formData.buttonIds" class="operation-list">
						<el-checkbox v-for="button in selectedButtonList" :key="button.value" :value="button.value" border>
							<span>{{ button.label }}</span>
							<span v-if="button.data?.hasDesktop || button.data?.hasWeb || button.data?.hasMobile" class="platform-icons">
								<FaIcon v-if="button.data?.hasWeb" name="el-icon-ChromeFilled" />
								<FaIcon v-if="button.data?.hasMobile" name="el-icon-Iphone" />
								<FaIcon v-if="button.data?.hasDesktop" name="el-icon-Monitor" />
							</span>
						</el-checkbox>
					</el-checkbox-group>
					<p v-else class="empty-message">此页面没有可配置的操作</p>
				</section>
				<p v-else-if="!loading" class="empty-message menu-empty">
					{{ state.searchKeyword ? "未找到匹配的页面" : "暂无可授权页面" }}
				</p>
			</div>
		</template>
	</FaDialog>
</template>

<script setup lang="ts">
import { computed, reactive, useTemplateRef } from "vue";
import { ArrowRight, Search } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { roleApi } from "@/api/services/Admin/role";
import type { CheckboxValueType } from "element-plus";
import type { ElSelectorOutput, FaDialogInstance } from "fast-element-plus";
import type { RoleAuthInput } from "@/api/services/Admin/role/models/RoleAuthInput";

defineOptions({
	name: "AdminRoleAuth",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<RoleAuthInput>({
		menuIds: [],
		buttonIds: [],
	}),
	dialogTitle: "角色授权",
	menuList: withDefineType<ElSelectorOutput<string>[]>([]),
	searchKeyword: "",
	selectedMenuId: withDefineType<string | undefined>(undefined),
	/** 折叠模块 */
	collapsedModuleList: new Set<string>(),
});

/** 过滤的菜单 */
const filteredMenuList = computed(() => {
	const keyword = state.searchKeyword.toLowerCase();
	if (!keyword) return state.menuList;

	return state.menuList.flatMap((node) => {
		if (node.label?.toLowerCase().includes(keyword)) return [node];
		if (!node.data?.isModule) return [];
		const children = node.children?.filter((menu) => menu.label?.toLowerCase().includes(keyword));
		return children?.length ? [{ ...node, children }] : [];
	});
});
/** 选中菜单 */
const selectedMenu = computed(() => {
	const flatMenuList = filteredMenuList.value.flatMap((node) => (node.data?.isModule ? (node.children ?? []) : [node]));
	return flatMenuList.find((menu) => menu.value === state.selectedMenuId) ?? flatMenuList[0];
});
/** 选中模块 */
const selectedModule = computed(() =>
	state.menuList.find((node) => node.data?.isModule && node.children?.some((menu) => menu.value === selectedMenu.value?.value))
);
/** 选中按钮集合 */
const selectedButtonList = computed(() => selectedMenu.value?.children ?? []);
/** 选中的菜单Id */
const checkedMenuIds = computed(() => new Set(state.formData.menuIds));
/** 选中的按钮Id */
const checkedButtonIds = computed(() => new Set(state.formData.buttonIds));
/** 判断模块是否展开 */
const isModuleExpanded = (moduleId: string) => Boolean(state.searchKeyword) || !state.collapsedModuleList.has(moduleId);
/** 判断菜单是否选中 */
const isMenuChecked = (menuId: string) => checkedMenuIds.value.has(menuId);
/** 获取按钮选中数量 */
const getButtonCheckedCount = (menu: ElSelectorOutput<string>) =>
	menu.children?.filter((button) => checkedButtonIds.value.has(button.value)).length ?? 0;
const selectedButtonCount = computed(() => getButtonCheckedCount(selectedMenu.value));

const getModuleCount = (moduleId: string) => {
	const menus = state.menuList.find((node) => node.value === moduleId)?.children ?? [];
	return `${menus.filter((menu) => isMenuChecked(menu.value)).length} / ${menus.length}`;
};

const toggleModule = (moduleId: string) => {
	const { collapsedModuleList } = state;
	if (!collapsedModuleList.delete(moduleId)) collapsedModuleList.add(moduleId);
};

const handleMenuChange = (menuId: string, checked: CheckboxValueType) => {
	if (checked && isMenuChecked(menuId)) return;

	const menuIds = state.formData.menuIds ?? [];
	state.formData.menuIds = checked ? [...menuIds, menuId] : menuIds.filter((id) => id !== menuId);
};

const handleButtonAllChange = (checked: CheckboxValueType) => {
	const buttonIds = new Set(state.formData.buttonIds);
	for (const button of selectedButtonList.value) {
		if (checked) buttonIds.add(button.value);
		else buttonIds.delete(button.value);
	}
	state.formData.buttonIds = [...buttonIds];
};

const handleConfirm = () => {
	const unGrantedMenu = state.menuList
		.flatMap((node) => (node.data?.isModule ? (node.children ?? []) : [node]))
		.find((menu) => !isMenuChecked(menu.value) && getButtonCheckedCount(menu) > 0);
	if (unGrantedMenu) {
		state.searchKeyword = "";
		state.selectedMenuId = unGrantedMenu.value;
		if (selectedModule.value) state.collapsedModuleList.delete(selectedModule.value.value);
		ElMessage.warning(`请先授权「${unGrantedMenu.label}」页面，或取消该页面的操作权限。`);
		return;
	}

	faDialogRef.value.close(async () => {
		await roleApi.roleAuth(state.formData);
		ElMessage.success("授权成功！");
		emit("ok");
	});
};

const handleClose = () => {
	state.formData = { menuIds: [], buttonIds: [] };
	state.menuList = [];
	state.searchKeyword = "";
	state.selectedMenuId = undefined;
	state.collapsedModuleList.clear();
};

const open = (roleId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await roleApi.queryRoleAuthMenu({ roleId });
		state.formData = apiRes;
		state.menuList = await roleApi.queryAuthMenu();
		state.dialogTitle = `角色授权 - ${apiRes.roleName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>

<style lang="scss" scoped>
.role-auth {
	display: grid;
	grid-template-columns: minmax(190px, 29%) minmax(0, 1fr);
	grid-template-rows: minmax(0, 1fr);
	height: 100%;
	min-height: 0;
}

.menu-pane {
	display: flex;
	flex-direction: column;
	gap: 12px;
	min-height: 0;
	padding: 12px;
	border-inline-end: 1px solid var(--el-border-color-lighter);
}

.menu-scrollbar {
	flex: 1;
	min-height: 0;
}

.menu-node + .menu-node {
	margin-top: 5px;
}

.module-item,
.menu-item {
	display: flex;
	align-items: center;
	gap: 8px;
	width: 100%;
	min-height: 32px;
	padding: 5px 10px;
	border: 0;
	border-radius: var(--el-border-radius-base);
	font: inherit;
	text-align: start;
	color: var(--el-text-color-primary);
	background: transparent;
	cursor: pointer;

	&:not(:disabled):hover {
		background: var(--el-fill-color-light);
	}

	&:focus-visible {
		outline: 2px solid var(--el-color-primary);
		outline-offset: -2px;
	}
}

.module-item {
	color: var(--el-text-color-regular);
	font-weight: 600;

	&:disabled {
		cursor: default;
	}

	&.is-expanded .module-arrow {
		transform: rotate(90deg);
	}
}

.module-menus .menu-item {
	padding-inline-start: 24px;
}

.menu-item {
	&.is-current {
		color: var(--el-color-primary);
		background: var(--el-color-primary-light-9);
	}
}

.menu-status {
	flex: 0 0 7px;
	height: 7px;
	border-radius: 50%;
	background: var(--el-border-color-dark);

	&.is-authorized {
		background: var(--el-color-primary);
	}
}

.menu-label {
	flex: 1;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.operation-count {
	font-size: var(--el-font-size-small);
	font-variant-numeric: tabular-nums;
	color: var(--el-text-color-secondary);
	white-space: nowrap;
}

.permission-pane {
	overflow-y: auto;
	min-height: 0;
	padding: 18px 24px 20px;
	scrollbar-width: thin;
}

.page-header,
.page-toggle,
.operation-header {
	display: flex;
	align-items: center;
	gap: 10px;
}

.page-header {
	justify-content: space-between;
	padding-bottom: 16px;
	border-bottom: 1px solid var(--el-border-color-lighter);

	h2 {
		margin: 0;
		font-size: var(--el-font-size-extra-large);
		line-height: 1.4;
		overflow-wrap: anywhere;
	}
}

.page-module {
	margin: 0 0 4px;
	color: var(--el-text-color-secondary);
	font-size: var(--el-font-size-small);
}

.page-platforms {
	display: flex;
	flex-wrap: wrap;
	gap: 6px;
	margin-top: 8px;

	:deep(.el-tag__content) {
		display: inline-flex;
		align-items: center;
		gap: 4px;
	}
}

.page-toggle {
	flex: none;
	color: var(--el-text-color-regular);
}

.operation-header {
	flex-wrap: wrap;
	margin-top: 12px;

	h3 {
		margin: 0;
		font-size: var(--el-font-size-medium);
	}

	.check-all {
		margin-inline: auto 0;
	}
}

.operation-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px 14px;
	margin-top: 8px;

	.el-checkbox {
		height: auto;
		min-height: 50px;
		margin: 0;
		padding: 8px 10px;
		background: var(--el-fill-color-extra-light);

		&:hover {
			border-color: var(--el-border-color-darker);
		}

		&.is-checked {
			border-color: var(--el-color-primary);
			background: var(--el-color-primary-light-9);
		}

		:deep(.el-checkbox__label) {
			display: flex;
			flex: 1;
			flex-wrap: wrap;
			align-items: center;
			gap: 4px 8px;
			min-width: 0;
			white-space: normal;
		}
	}
}

.platform-icons {
	display: inline-flex;
	align-items: center;
	gap: 4px;
	margin-inline-start: auto;
	color: var(--el-text-color-secondary);
}

.empty-message {
	margin: 12px 0 0;
	color: var(--el-text-color-secondary);
}

.menu-empty {
	align-self: center;
	justify-self: center;
	margin: 0;
	padding: 12px;
}

@media (max-width: 767px) {
	.role-auth {
		grid-template-columns: minmax(0, 1fr);
		grid-template-rows: minmax(90px, 34%) minmax(0, 1fr);
	}

	.menu-pane {
		padding: 8px;
		border-inline-end: none;
		border-bottom: 1px solid var(--el-border-color-lighter);
	}

	.permission-pane {
		padding: 12px 8px;
	}

	.operation-list {
		gap: 8px;
	}
}
</style>

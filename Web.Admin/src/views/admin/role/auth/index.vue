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
					<el-input v-model="state.menuKeyword" :prefix-icon="Search" placeholder="搜索页面" clearable />
					<el-scrollbar class="menu-scrollbar">
						<div v-for="node in filteredMenus" :key="node.value" class="menu-node">
							<button
								v-if="node.data?.isModule"
								class="module-item"
								:class="{ 'is-expanded': !visibleCollapsedModules.has(node.value) }"
								type="button"
								@click="toggleModule(node.value)"
							>
								<el-icon class="module-arrow"><ArrowRight /></el-icon>
								<span class="menu-label">{{ node.label }}</span>
								<span class="operation-count">{{ moduleCounts.get(node.value) }}</span>
							</button>
							<div v-show="!visibleCollapsedModules.has(node.value)" :class="{ 'module-menus': node.data?.isModule }">
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
						<div class="page-title">
							<p v-if="selectedModule" class="page-module">{{ selectedModule.label }}</p>
							<h2>{{ selectedMenu.label }}</h2>
							<div v-if="selectedPlatforms.length" class="page-platforms">
								<el-tag v-for="platform in selectedPlatforms" :key="platform.key" type="info" effect="plain">
									<FaIcon :name="platform.icon" />
									{{ platform.label }}
								</el-tag>
							</div>
						</div>
						<div class="page-toggle">
							<label :for="pageGrantId">页面授权</label>
							<el-switch
								:id="pageGrantId"
								:model-value="isMenuChecked(selectedMenu.value)"
								@change="handleMenuChange(selectedMenu.value, $event)"
							/>
						</div>
					</header>
					<div class="operation-header">
						<h3>操作权限</h3>
						<span v-if="selectedButtons.length" class="operation-count">
							已选操作 {{ selectedButtonCount }} / {{ selectedButtons.length }}
						</span>
						<el-checkbox
							v-if="selectedButtons.length"
							class="check-all"
							:model-value="selectedButtonCount === selectedButtons.length"
							:indeterminate="selectedButtonCount > 0 && selectedButtonCount < selectedButtons.length"
							@change="handleButtonAllChange"
						>
							全选
						</el-checkbox>
					</div>
					<el-checkbox-group v-if="selectedButtons.length" v-model="state.formData.buttonIds" class="operation-list">
						<el-checkbox v-for="button in selectedButtons" :key="button.value" :value="button.value" border>
							<span class="operation-label">{{ button.label }}</span>
							<span v-if="button.data?.hasDesktop || button.data?.hasWeb || button.data?.hasMobile" class="platform-icons">
								<FaIcon v-if="button.data?.hasDesktop" name="el-icon-Monitor" />
								<FaIcon v-if="button.data?.hasWeb" name="el-icon-ChromeFilled" />
								<FaIcon v-if="button.data?.hasMobile" name="el-icon-Iphone" />
							</span>
						</el-checkbox>
					</el-checkbox-group>
					<p v-else class="empty-message">此页面没有可配置的操作</p>
				</section>
				<p v-else-if="!loading" class="empty-message menu-empty" role="status">
					{{ searchKeyword ? "未找到匹配的页面" : "暂无可授权页面" }}
				</p>
			</div>
		</template>
	</FaDialog>
</template>

<script setup lang="ts">
import { computed, reactive, useId, useTemplateRef, watch } from "vue";
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
const pageGrantId = useId();

const state = reactive({
	formData: withDefineType<RoleAuthInput>({
		menuIds: [],
		buttonIds: [],
	}),
	dialogTitle: "角色授权",
	menuList: withDefineType<ElSelectorOutput<string>[]>([]),
	menuKeyword: "",
	selectedMenuId: withDefineType<string | undefined>(undefined),
	collapsedModules: new Set<string>(),
	searchCollapsedModules: new Set<string>(),
});

const searchKeyword = computed(() => state.menuKeyword.trim().toLowerCase());
const visibleCollapsedModules = computed(() => (searchKeyword.value ? state.searchCollapsedModules : state.collapsedModules));
watch(searchKeyword, () => state.searchCollapsedModules.clear());

const filteredMenus = computed(() => {
	const keyword = searchKeyword.value;
	if (!keyword) return state.menuList;

	return state.menuList.flatMap((node) => {
		if (node.label?.toLowerCase().includes(keyword)) return [node];
		if (!node.data?.isModule) return [];
		const children = node.children?.filter((menu) => menu.label?.toLowerCase().includes(keyword));
		return children?.length ? [{ ...node, children }] : [];
	});
});
const menus = computed(() => state.menuList.flatMap((node) => (node.data?.isModule ? (node.children ?? []) : [node])));
const visibleMenus = computed(() => filteredMenus.value.flatMap((node) => (node.data?.isModule ? (node.children ?? []) : [node])));
const selectedMenu = computed(() => visibleMenus.value.find((menu) => menu.value === state.selectedMenuId) ?? visibleMenus.value[0]);
const selectedModule = computed(() =>
	state.menuList.find((node) => node.data?.isModule && node.children?.some((menu) => menu.value === selectedMenu.value?.value))
);
const selectedButtons = computed(() => selectedMenu.value?.children ?? []);
const platforms = [
	{ key: "hasWeb", label: "Web", icon: "el-icon-ChromeFilled" },
	{ key: "hasMobile", label: "移动端", icon: "el-icon-Iphone" },
	{ key: "hasDesktop", label: "Windows", icon: "el-icon-Monitor" },
] as const;
const selectedPlatforms = computed(() => platforms.filter((platform) => selectedMenu.value?.data?.[platform.key]));
const checkedMenuIds = computed(() => new Set(state.formData.menuIds));
const checkedButtonIds = computed(() => new Set(state.formData.buttonIds));

const isMenuChecked = (menuId: string) => checkedMenuIds.value.has(menuId);
const getButtonCheckedCount = (menu: ElSelectorOutput<string>) =>
	menu.children?.filter((button) => checkedButtonIds.value.has(button.value)).length ?? 0;
const selectedButtonCount = computed(() => (selectedMenu.value ? getButtonCheckedCount(selectedMenu.value) : 0));
const moduleCounts = computed(() => {
	const counts = new Map<string, string>();
	for (const node of state.menuList) {
		if (!node.data?.isModule) continue;
		const children = node.children ?? [];
		counts.set(node.value, `${children.filter((menu) => isMenuChecked(menu.value)).length} / ${children.length}`);
	}
	return counts;
});

const toggleModule = (moduleId: string) => {
	const collapsed = visibleCollapsedModules.value;
	if (collapsed.has(moduleId)) collapsed.delete(moduleId);
	else collapsed.add(moduleId);
};

const handleMenuChange = (menuId: string, val: CheckboxValueType) => {
	if (val) {
		// 选中菜单
		if (!state.formData.menuIds?.includes(menuId)) {
			state.formData.menuIds = [...(state.formData.menuIds || []), menuId];
		}
	} else {
		// 取消选中菜单
		state.formData.menuIds = state.formData.menuIds?.filter((id) => id !== menuId) || [];
	}
};

const handleButtonAllChange = (checked: CheckboxValueType) => {
	const buttonIds = new Set(state.formData.buttonIds);
	for (const button of selectedButtons.value) {
		if (checked) buttonIds.add(button.value);
		else buttonIds.delete(button.value);
	}
	state.formData.buttonIds = [...buttonIds];
};

const handleConfirm = () => {
	const ungrantedMenu = menus.value.find((menu) => !isMenuChecked(menu.value) && getButtonCheckedCount(menu) > 0);
	if (ungrantedMenu) {
		state.menuKeyword = "";
		state.selectedMenuId = ungrantedMenu.value;
		if (selectedModule.value) state.collapsedModules.delete(selectedModule.value.value);
		ElMessage.warning(`请先授权「${ungrantedMenu.label}」页面，或取消该页面的操作权限。`);
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
	state.menuKeyword = "";
	state.selectedMenuId = undefined;
	state.collapsedModules.clear();
	state.searchCollapsedModules.clear();
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

	&:hover {
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

.page-title {
	min-width: 0;
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

	.fa-icon {
		margin-inline-end: 4px;
		vertical-align: middle;
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
		width: 100%;
		min-width: 0;
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

.operation-label {
	min-width: 0;
	overflow-wrap: anywhere;
}

.platform-icons {
	display: inline-flex;
	align-items: center;
	gap: 4px;
	margin-inline-start: auto;
	vertical-align: middle;
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

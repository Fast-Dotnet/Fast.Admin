<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-input v-model="state.searchKeyword" clearable placeholder="搜索页面">
			<template #prefix>
				<wd-icon custom-class="wd-input__icon" name="search-line" />
			</template>
		</wd-input>

		<wd-cell-group
			v-for="node in filteredMenuList"
			:key="node.value"
			:title="node.data?.isModule ? node.label : undefined"
			:value="node.data?.isModule ? getModuleCount(node.value) : undefined"
		>
			<wd-collapse>
				<wd-collapse-item v-for="menu in node.data?.isModule ? node.children : [node]" :key="menu.value" :name="menu.value">
					<template #title="{ expanded }">
						<view class="role-auth__title-left">
							<text class="wd-collapse-item__title">{{ menu.label }}</text>
							<wd-icon v-if="menu.data?.hasWeb" name="compass" />
							<wd-icon v-if="menu.data?.hasMobile" name="mobile" />
							<wd-icon v-if="menu.data?.hasDesktop" name="desktop" />
						</view>
						<view class="role-auth__title-right">
							<wd-tag v-if="isMenuChecked(menu.value)" size="small" type="primary" variant="plain">已授权</wd-tag>
							<text class="role-auth__count">{{ getButtonCheckedCount(menu) }} / {{ menu.children?.length ?? 0 }}</text>
							<wd-icon name="down" :custom-class="`wd-collapse-item__arrow ${expanded ? 'is-retract' : ''}`" />
						</view>
					</template>

					<view class="role-auth__toggle">
						<text>页面授权</text>
						<wd-switch :model-value="isMenuChecked(menu.value)" @update:model-value="(checked) => handleMenuChange(menu, checked)" />
					</view>

					<wd-divider />

					<template v-if="menu.children?.length">
						<view class="role-auth__operations">
							<text>操作权限</text>
							<wd-checkbox
								type="square"
								:model-value="getButtonCheckedCount(menu) === menu.children.length"
								:indeterminate="getButtonCheckedCount(menu) > 0 && getButtonCheckedCount(menu) < menu.children.length"
								@update:model-value="(checked) => handleButtonChange(menu.children ?? [], checked)"
							>
								全选
							</wd-checkbox>
						</view>
						<wd-checkbox-group v-model="state.formData.buttonIds" type="square">
							<wd-checkbox v-for="button in menu.children" :key="button.value" :name="button.value">
								<text>{{ button.label }}</text>
								<view class="role-auth__platforms">
									<wd-icon v-if="button.data?.hasDesktop" name="desktop" aria-label="Windows" size="12px" />
									<wd-icon v-if="button.data?.hasWeb" name="compass" aria-label="Web" size="12px" />
									<wd-icon v-if="button.data?.hasMobile" name="mobile" aria-label="移动端" size="12px" />
								</view>
							</wd-checkbox>
						</wd-checkbox-group>
					</template>
					<text v-else class="role-auth__empty">此页面没有可配置的操作</text>
				</wd-collapse-item>
			</wd-collapse>
		</wd-cell-group>

		<wd-empty v-if="!filteredMenuList.length" icon="no-content" :tip="state.searchKeyword ? '未找到匹配的页面' : '暂无可授权页面'" />
		<wd-button v-else custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { computed, reactive } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { roleApi } from "@/api/services/Admin/role";
import { useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { RoleAuthInput } from "@/api/services/Admin/role/models/RoleAuthInput";

definePage({
	name: "AdminRoleAuth",
	authCode: "Role:Edit",
	style: {
		navigationBarTitleText: "角色授权",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const state = reactive({
	loading: false,
	formData: withDefineType<RoleAuthInput>({ menuIds: [], buttonIds: [] }),
	roleId: withDefineType<string>(),
	menuList: withDefineType<ElSelectorOutput<string>[]>([]),
	searchKeyword: "",
});

/** 过滤的菜单 */
const filteredMenuList = computed(() => {
	const keyword = state.searchKeyword.trim().toLowerCase();
	if (!keyword) return state.menuList;

	return state.menuList.flatMap((node) => {
		if (node.label?.toLowerCase().includes(keyword)) return [node];
		if (!node.data?.isModule) return [];
		const children = node.children?.filter((menu) => menu.label?.toLowerCase().includes(keyword));
		return children?.length ? [{ ...node, children }] : [];
	});
});
/** 选中的菜单Id */
const checkedMenuIds = computed(() => new Set(state.formData.menuIds));
/** 选中的按钮Id */
const checkedButtonIds = computed(() => new Set(state.formData.buttonIds));
/** 判断菜单是否选中 */
const isMenuChecked = (menuId: string) => checkedMenuIds.value.has(menuId);
/** 获取按钮选中数量 */
const getButtonCheckedCount = (menu: ElSelectorOutput<string>) =>
	menu.children?.filter((button) => checkedButtonIds.value.has(button.value)).length ?? 0;

const getModuleCount = (moduleId: string) => {
	const menus = state.menuList.find((node) => node.value === moduleId)?.children ?? [];
	return `${menus.filter((menu) => isMenuChecked(menu.value)).length} / ${menus.length}`;
};

const handleMenuChange = (menu: ElSelectorOutput<string>, checked: boolean) => {
	if (checked && isMenuChecked(menu.value)) return;

	const menuIds = state.formData.menuIds ?? [];
	state.formData.menuIds = checked ? [...menuIds, menu.value] : menuIds.filter((id) => id !== menu.value);
};

const handleButtonChange = (buttons: ElSelectorOutput<string>[], checked: boolean) => {
	const buttonIds = new Set(state.formData.buttonIds);
	for (const button of buttons) {
		if (checked) buttonIds.add(button.value);
		else buttonIds.delete(button.value);
	}
	state.formData.buttonIds = [...buttonIds];
};

const handleConfirm = throttle(async () => {
	const unGrantedMenu = state.menuList
		?.flatMap((node) => (node.data?.isModule ? (node.children ?? []) : [node]))
		.find((menu) => !isMenuChecked(menu.value) && getButtonCheckedCount(menu) > 0);
	if (unGrantedMenu) {
		state.searchKeyword = "";
		useToast.warning(`请先授权「${unGrantedMenu.label}」页面，或取消该页面的操作权限。`);
		return;
	}
	state.loading = true;
	try {
		await roleApi.roleAuth(state.formData);
		useToast.success("授权成功！");
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Role");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/role/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		const apiRes = await roleApi.queryRoleAuthMenu({ roleId: state.roleId });
		state.formData = apiRes;
		state.menuList = await roleApi.queryAuthMenu();
		uni.setNavigationBarTitle({ title: `角色授权 - ${apiRes.roleName}` });
	} finally {
		state.loading = false;
	}
};

onLoad((options: { roleId?: string }) => {
	if (!options?.roleId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.roleId = options.roleId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

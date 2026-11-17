<template>
	<FaPaging
		ref="faPagingRef"
		:auto="false"
		:request-api="departmentApi.queryDepartmentPaged"
		:init-param="{ orgId: state.selectedOrganization?.value }"
	>
		<template #top="{ searchParam }">
			<view class="department-header">
				<wd-cell
					custom-class="department-header__organization"
					title="机构"
					:value="state.selectedOrganization?.label"
					placeholder="请选择机构"
					is-link
					@click="faPopupRef?.open()"
				/>
				<view class="department-header__search-row">
					<wd-input
						custom-class="department-header__search"
						v-model="searchParam.value.searchValue"
						clearable
						placeholder="搜索部门名称或编码"
						@confirm="handleRefresh"
						@clear="handleRefresh()"
					>
						<template #prefix>
							<wd-icon custom-class="wd-input__icon" name="search-line" />
						</template>
					</wd-input>
					<FaAuth code="Department:Add">
						<wd-button size="small" icon="plus" @click="handleAdd">新增</wd-button>
					</FaAuth>
				</view>
			</view>
		</template>

		<template #default="{ searchParam, pagedList, totalRows }">
			<scroll-view v-if="!searchParam.value.searchValue" class="department-breadcrumb" scroll-x :show-scrollbar="false">
				<view class="department-breadcrumb__content">
					<text
						class="department-breadcrumb__item department-breadcrumb__item--root"
						:class="{ 'is-active': !state.selectedDepartmentList.length }"
					>
						全部部门
					</text>

					<template v-for="(item, index) in state.selectedDepartmentList" :key="item.departmentId">
						<wd-icon name="arrow-right" size="24rpx" color="var(--wot-icon-auxiliary)" />
						<text class="department-breadcrumb__item" :class="{ 'is-active': index === state.selectedDepartmentList.length - 1 }">
							{{ item.departmentName }}
						</text>
					</template>
				</view>
			</scroll-view>

			<view class="department-summary">
				<text>{{ searchParam.value.searchValue ? "搜索结果" : state.selectedDepartmentList.length ? "下级部门" : "部门列表" }}</text>
				<text class="department-summary__count">{{ totalRows }} 个部门</text>
			</view>

			<FaCard v-for="item in pagedList" :key="item.departmentId" code="Department:Detail">
				<view class="department-card">
					<view class="department-card__info">
						<text class="department-card__name">{{ item.departmentName }}</text>
						<text class="department-card__code">{{ item.departmentCode }}</text>
					</view>

					<view class="department-card__actions">
						<wd-tag v-if="item.dataPublic" type="danger" variant="plain">公开数据</wd-tag>
						<wd-tag v-if="item.children?.length" type="primary" variant="plain">
							<view class="department-card__children">
								<text>下级 {{ item.children.length }}</text>
								<wd-icon name="arrow-right" size="24rpx" />
							</view>
						</wd-tag>
						<view
							v-if="userInfoStore.hasButtonPermission('Department:Edit', 'Department:Delete')"
							class="department-card__more"
							@click.stop="handleMoreClick(item)"
						>
							<wd-icon name="more" size="36rpx" color="var(--wot-icon-secondary)" />
						</view>
					</view>
				</view>
			</FaCard>
		</template>
	</FaPaging>

	<FaPopup ref="faPopupRef" title="机构列表" height="70%" position="bottom" closable>
		<template #titlePrefix>
			<FaAuth code="Department:Add">
				<wd-button size="small" variant="plain" icon="plus" @click="handleOrgAdd">新增</wd-button>
			</FaAuth>
		</template>

		<wd-cell-group v-for="item in state.orgList" :key="item.value" :title="item.label" border>
			<wd-cell
				v-for="cItem in item.children"
				:key="cItem.value"
				center
				:clickable="!cItem.disabled"
				:custom-class="
					[
						'department-organization__cell',
						state.selectedOrganization?.value === cItem.value ? 'fa-select-page__cell--selected' : '',
						cItem.disabled ? 'is-disabled' : '',
					].join(' ')
				"
				@click="handleOrgChange(cItem)"
			>
				<template #prefix>
					<view class="department-organization__check">
						<wd-icon v-if="state.selectedOrganization?.value === cItem.value" name="check" size="32rpx" color="var(--wot-primary-6)" />
					</view>
				</template>
				<template #title>
					<view class="department-organization__info">
						<text class="department-organization__name">{{ cItem.treeLabel }}</text>
						<text class="department-organization__code">{{ cItem.data?.orgCode }}</text>
					</view>
				</template>
				<template #suffix>
					<view
						v-if="userInfoStore.hasButtonPermission('Department:Edit', 'Department:Delete')"
						class="department-organization__more"
						@click.stop="handleOrgMoreClick(cItem)"
					>
						<wd-icon name="more" size="36rpx" color="var(--wot-icon-secondary)" />
					</view>
				</template>
			</wd-cell>
		</wd-cell-group>
	</FaPopup>

	<wd-action-sheet v-model="state.actionVisible" :actions="state.actionList" cancel-text="取消" @select="handleActionSelect" />
</template>

<script setup lang="ts">
import { onShow } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { departmentApi } from "@/api/services/Admin/department";
import { organizationApi } from "@/api/services/Admin/organization";
import { useLoading, useMessageBox, useToast } from "@/hooks";
import { useUserInfo } from "@/stores";
import type { Action } from "@wot-ui/ui/components/wd-action-sheet/types";
import type { QueryDepartmentPagedOutput } from "@/api/services/Admin/department/models/QueryDepartmentPagedOutput";
import type { FaPagingInstance, FaPopupInstance } from "@/components";

definePage({
	name: "AdminDepartment",
	authCode: "Department:Paged",
	style: {
		navigationBarTitleText: "部门管理",
	},
});

const router = useRouter();
const userInfoStore = useUserInfo();

const faPagingRef = ref<FaPagingInstance>();
const faPopupRef = ref<FaPopupInstance>();

const state = reactive({
	/** 机构集合 */
	orgList: withDefineType<ElSelectorOutput<string>[]>([]),
	/** 弹出菜单显示 */
	actionVisible: false,
	/** 弹出菜单集合 */
	actionList: withDefineType<{ name: string }[]>([]),
	/** 点击的机构 */
	clickedOrganization: withDefineType<ElSelectorOutput<string>>(),
	/** 点击的部门 */
	clickedDepartment: withDefineType<QueryDepartmentPagedOutput>(),
	/** 选中的机构 */
	selectedOrganization: withDefineType<ElSelectorOutput<string>>(),
	/** 选中的部门集合 */
	selectedDepartmentList: withDefineType<QueryDepartmentPagedOutput[]>([]),
});

/** 扁平化机构 */
const flattenOrganizations = (items: ElSelectorOutput<string>[], parentLabel = ""): ElSelectorOutput<string>[] =>
	items.flatMap(({ children, ...item }) => {
		const treeLabel = [parentLabel, item.label].filter(Boolean).join(" / ");
		return [{ ...item, treeLabel }, ...flattenOrganizations(children ?? [], treeLabel)];
	});

/** 加载机构 */
const loadOrg = async (isFirst = false) => {
	useLoading.show();
	try {
		const apiRes = await organizationApi.organizationSelector();
		state.orgList = apiRes.map((item) => ({
			...item,
			children: flattenOrganizations([item]),
		}));
	} finally {
		useLoading.hide();
	}
	const hasOrgId = (orgList: ElSelectorOutput<string>[]): boolean =>
		orgList.some((item) => item.value === state.selectedOrganization.value || hasOrgId(item.children ?? []));
	if (!state.selectedOrganization || !hasOrgId(state.orgList)) {
		state.selectedOrganization = state.orgList[0]?.children?.[0] ?? null;
	}
	if (isFirst) {
		faPagingRef.value.reload();
	} else {
		faPagingRef.value.refresh();
	}
};

const handleRefresh = ({ value }: { value?: string }) => {
	faPagingRef.value.searchParam.searchValue = value;
	state.selectedDepartmentList = [];
	faPagingRef.value.refresh();
};

const handleOrgChange = (item: ElSelectorOutput<string>) => {
	if (item.value === state.selectedOrganization?.value) return;
	state.selectedOrganization = item;
	faPopupRef.value.close();
	faPagingRef.value.refresh();
};

const handleOrgAdd = throttle(
	async () =>
		await router.push({
			path: "/pages_admin/department/orgEdit/index",
			query: {
				mode: "add",
			},
		})
);

const handleOrgMoreClick = throttle((item: ElSelectorOutput<string>) => {
	state.clickedOrganization = item;
	state.actionList = [
		{ name: "编辑机构", permission: "Department:Edit" },
		{ name: "删除机构", permission: "Department:Delete" },
	]
		.filter(({ permission }) => userInfoStore.hasButtonPermission(permission))
		.map(({ name }) => ({ name }));
	state.actionVisible = true;
});

const handleAdd = throttle(
	async () =>
		await router.push({
			path: "/pages_admin/department/edit/index",
			query: {
				mode: "add",
			},
		})
);

const handleMoreClick = throttle((item: QueryDepartmentPagedOutput) => {
	state.clickedDepartment = item;
	state.actionList = [
		{ name: "编辑部门", permission: "Department:Edit" },
		{ name: "删除部门", permission: "Department:Delete" },
	]
		.filter(({ permission }) => userInfoStore.hasButtonPermission(permission))
		.map(({ name }) => ({ name }));
	state.actionVisible = true;
});

const handleActionSelect = throttle(async ({ item }: { item: Action }) => {
	switch (item.name) {
		case "编辑机构":
			await router.push({
				path: "/pages_admin/department/orgEdit/index",
				query: {
					mode: "edit",
					orgId: state.clickedOrganization.value,
				},
			});
			break;
		case "删除机构":
			await useMessageBox.confirm("确定要删除机构？", {
				async beforeConfirm() {
					await organizationApi.deleteOrganization({
						orgId: state.clickedOrganization.value,
						rowVersion: state.clickedOrganization.rowVersion,
					});
					useToast.success("删除成功！");
					loadOrg();
				},
			});
			break;
		case "编辑部门":
			await router.push({
				path: "/pages_admin/department/edit/index",
				query: {
					mode: "edit",
					departmentId: state.clickedDepartment.departmentId,
				},
			});
			break;
		case "删除部门":
			await useMessageBox.confirm("确定要删除部门？", {
				async beforeConfirm() {
					await departmentApi.deleteDepartment({
						departmentId: state.clickedDepartment.departmentId,
						rowVersion: state.clickedDepartment.rowVersion,
					});
					useToast.success("删除成功！");
					faPagingRef.value.refresh();
				},
			});
			break;
	}
});

onShow(() => {
	loadOrg(true);
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

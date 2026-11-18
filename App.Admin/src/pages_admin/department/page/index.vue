<template>
	<FaPaging
		ref="faPagingRef"
		page-key="Department"
		:auto="false"
		:request-api="departmentApi.queryDepartmentPaged"
		:pagination="false"
		:init-param="{ orgId: state.selectedOrganization?.value, treeData: false }"
	>
		<template #top="{ searchParam }">
			<view class="department-header">
				<wd-cell
					custom-class="department-header__organization"
					title="机构"
					:value="state.selectedOrganization?.label"
					placeholder="请选择机构"
					is-link
					@click="faPopupRef.open()"
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
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</view>
			</view>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.departmentId" code="Department:Detail" @click="detailFormRef.detail(item.departmentId)">
				<FaField label="部门名称" :value="item.departmentName" />
				<FaField label="部门编码" :value="item.departmentCode" />
				<FaField label="父级" :value="item.parentName" />
				<FaField label="排序" :value="item.sort" />
				<FaField label="数据公开" center>
					<FaTag name="BooleanEnum" :value="item.dataPublic" />
				</FaField>

				<template #footer>
					<FaAuth code="Department:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.departmentId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Department:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
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

	<DepartmentDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { onShow } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { departmentApi } from "@/api/services/Admin/department";
import { organizationApi } from "@/api/services/Admin/organization";
import { useLoading, useMessageBox, useToast } from "@/hooks";
import { usePaging, useUserInfo } from "@/stores";
import DepartmentDetail from "./components/detail.vue";
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
const pagingStore = usePaging();
const userInfoStore = useUserInfo();

const faPagingRef = ref<FaPagingInstance>();
const faPopupRef = ref<FaPopupInstance>();
const detailFormRef = ref<InstanceType<typeof DepartmentDetail>>();

const state = reactive({
	/** 机构集合 */
	orgList: withDefineType<ElSelectorOutput<string>[]>([]),
	/** 弹出菜单显示 */
	actionVisible: false,
	/** 弹出菜单集合 */
	actionList: withDefineType<{ name: string; color?: string }[]>([]),
	/** 点击的机构 */
	clickedOrganization: withDefineType<ElSelectorOutput<string>>(),
	/** 选中的机构 */
	selectedOrganization: withDefineType<ElSelectorOutput<string>>(),
});

const handleEdit = throttle(
	async (mode: IPageStateType, departmentId?: string) =>
		await router.push({
			path: "/pages_admin/department/edit/index",
			query: {
				mode,
				departmentId,
			},
		})
);

const handleDelete = throttle(async (item: QueryDepartmentPagedOutput) => {
	const { departmentId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除部门？", {
		async beforeConfirm() {
			await departmentApi.deleteDepartment({ departmentId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});

/** 扁平化机构 */
const flattenOrganizations = (items: ElSelectorOutput<string>[], parentLabel = ""): ElSelectorOutput<string>[] =>
	items.flatMap(({ children, ...item }) => {
		const treeLabel = [parentLabel, item.label].filter(Boolean).join(" / ");
		return [{ ...item, treeLabel }, ...flattenOrganizations(children ?? [], treeLabel)];
	});

/** 加载机构 */
const loadOrg = async () => {
	if (faPagingRef.value?.pagingVersion === pagingStore.getPageVersion("Department")) return;
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
	const orgId = state.selectedOrganization?.value;
	const orgList = state.orgList.flatMap((item) => item.children ?? []);
	state.selectedOrganization = orgList.find((item) => item.value === orgId) ?? orgList[0] ?? null;
	faPagingRef.value.reload();
};

const handleRefresh = (event?: { value?: string }) => {
	faPagingRef.value.searchParam.searchValue = event?.value ?? null;
	faPagingRef.value.reload();
};

const handleOrgChange = throttle(async (item: ElSelectorOutput<string>) => {
	if (item.value === state.selectedOrganization?.value) return;
	state.selectedOrganization = item;
	faPagingRef.value.clearSearchParam();
	await faPopupRef.value.close();
	faPagingRef.value.reload();
});

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
		{ name: "删除机构", color: "var(--wot-danger-main)", permission: "Department:Delete" },
	]
		.filter(({ permission }) => userInfoStore.hasButtonPermission(permission))
		.map(({ name, color }) => ({ name, color }));
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
					pagingStore.incrementPageVersion("Department");
					loadOrg();
				},
			});
			break;
	}
});

onShow(() => {
	loadOrg();
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

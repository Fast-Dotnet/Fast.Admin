<template>
	<view class="page">
		<view class="application-header">
			<wd-cell
				custom-class="application-header__application"
				title="应用"
				:value="state.selectedApplication?.appName"
				placeholder="请选择应用"
				is-link
				@click="faPopupRef.open()"
			/>
			<FaAuth code="AppClient:Add">
				<view class="application-header__actions">
					<wd-button size="small" icon="plus" :disabled="!state.selectedApplication" @click="handleClientEdit('add')">新增标识</wd-button>
				</view>
			</FaAuth>
		</view>

		<view class="application-list">
			<FaCard
				v-for="item in state.selectedApplication?.clientList"
				:key="item.recordId"
				:title="item.appKey"
				code="AppClient:Detail"
				@click="clientDetailFormRef?.detail(item.recordId)"
			>
				<template #titleExtra>
					<FaTag name="EnvironmentTypeEnum" :value="item.environmentType" />
				</template>

				<FaField label="应用类型">
					<FaTagFlag name="AppEnvironmentEnum" :value="item.appType" />
				</FaField>
				<FaField label="登录组件" :value="item.loginComponent" />
				<FaField label="请求超时" :value="`${item.requestTimeout ?? 0}毫秒`" />
				<FaField label="请求加密" center>
					<FaTag name="BooleanEnum" :value="item.requestEncipher" />
				</FaField>
				<FaField label="备注" :value="item.remark" />

				<template #footer>
					<FaAuth code="AppClient:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleClientEdit('edit', item.recordId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="AppClient:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleClientDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
			<wd-empty
				v-if="!state.selectedApplication?.clientList?.length"
				icon="no-content"
				:tip="state.selectedApplication ? '暂无应用标识' : '暂无应用'"
			/>
		</view>
	</view>

	<FaPopup ref="faPopupRef" title="应用列表" height="70%" position="bottom" closable>
		<template #titlePrefix>
			<FaAuth code="App:Add">
				<wd-button size="small" variant="plain" icon="plus" @click="handleEdit('add')">新增</wd-button>
			</FaAuth>
		</template>

		<wd-cell-group border>
			<wd-cell
				v-for="item in state.appList"
				:key="item.appId"
				:title="item.appName"
				:label="item.appNo"
				center
				clickable
				:custom-class="
					['application-option', state.selectedApplication?.appId === item.appId ? 'fa-select-page__cell--selected' : ''].join(' ')
				"
				@click="handleApplicationChange(item)"
			>
				<template #prefix>
					<view class="application-option__logo">
						<FaImage :src="item.logoUrl" width="72rpx" height="72rpx" />
					</view>
				</template>
				<template #suffix>
					<view class="application-option__actions">
						<wd-icon v-if="state.selectedApplication?.appId === item.appId" name="check" size="32rpx" color="var(--wot-primary-6)" />
						<view
							v-if="userInfoStore.hasButtonPermission('App:Detail', 'App:Edit', 'App:Delete')"
							class="application-option__more"
							@click.stop="handleMoreClick(item)"
						>
							<wd-icon name="more" size="36rpx" color="var(--wot-icon-secondary)" />
						</view>
					</view>
				</template>
			</wd-cell>
		</wd-cell-group>
		<wd-empty v-if="!state.appList.length" icon="no-content" tip="暂无应用" />
	</FaPopup>

	<wd-action-sheet v-model="state.actionVisible" :actions="state.actionList" cancel-text="取消" @select="handleActionSelect" />

	<ApplicationDetail ref="detailFormRef" />
	<ApplicationClientDetail ref="clientDetailFormRef" />
</template>

<script setup lang="ts">
import { onPullDownRefresh, onShow } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { applicationApi } from "@/api/services/Center/application";
import { applicationClientApi } from "@/api/services/Center/applicationClient";
import { useLoading, useMessageBox, useToast } from "@/hooks";
import { usePaging, useUserInfo } from "@/stores";
import ApplicationClientDetail from "./components/clientDetail.vue";
import ApplicationDetail from "./components/detail.vue";
import type { Action } from "@wot-ui/ui/components/wd-action-sheet/types";
import type { QueryApplicationClientOutput } from "@/api/services/Center/application/models/QueryApplicationClientOutput";
import type { QueryApplicationPagedOutput } from "@/api/services/Center/application/models/QueryApplicationPagedOutput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "SystemApplication",
	authCode: "App:Paged",
	style: {
		navigationBarTitleText: "应用管理",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();
const userInfoStore = useUserInfo();

const faPopupRef = ref<FaPopupInstance>();
const detailFormRef = ref<InstanceType<typeof ApplicationDetail>>();
const clientDetailFormRef = ref<InstanceType<typeof ApplicationClientDetail>>();

const state = reactive({
	appList: withDefineType<QueryApplicationPagedOutput[]>([]),
	selectedApplication: withDefineType<QueryApplicationPagedOutput>(),
	clickedApplication: withDefineType<QueryApplicationPagedOutput>(),
	actionVisible: false,
	actionList: withDefineType<{ name: string }[]>([]),
});

let pageVersion: number;

const handleLoad = async () => {
	const version = pagingStore.getPageVersion("Application");
	useLoading.show();
	try {
		const apiRes = await applicationApi.queryApplicationPaged({ enablePaged: false });
		state.appList = apiRes.rows;
		const appId = state.selectedApplication?.appId;
		state.selectedApplication = state.appList.find((item) => item.appId === appId) ?? state.appList[0];
		pageVersion = version;
	} finally {
		useLoading.hide();
	}
};

const handleApplicationChange = throttle(async (item: QueryApplicationPagedOutput) => {
	state.selectedApplication = item;
	await faPopupRef.value.close();
});

const handleEdit = throttle(
	async (mode: IPageStateType, appId?: string) =>
		await router.push({
			path: "/pages_system/application/edit/index",
			query: {
				mode,
				appId,
			},
		})
);

const handleClientEdit = throttle(
	async (mode: IPageStateType, recordId?: string) =>
		await router.push({
			path: "/pages_system/application/clientEdit/index",
			query: {
				mode,
				recordId,
				appId: state.selectedApplication.appId,
				appName: state.selectedApplication.appName,
			},
		})
);

const handleClientDelete = throttle(async (item: QueryApplicationClientOutput) => {
	const { recordId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除应用标识？", {
		async beforeConfirm() {
			await applicationClientApi.deleteApplicationClient({ recordId, rowVersion });
			useToast.success("删除成功！");
			await handleLoad();
		},
	});
});

const handleMoreClick = throttle((item: QueryApplicationPagedOutput) => {
	state.clickedApplication = item;
	state.actionList = [
		{ name: "应用详情", permission: "App:Detail" },
		{ name: "编辑应用", permission: "App:Edit" },
		{ name: "删除应用", permission: "App:Delete" },
	]
		.filter(({ permission }) => userInfoStore.hasButtonPermission(permission))
		.map(({ name }) => ({ name }));
	state.actionVisible = true;
});

const handleActionSelect = throttle(async ({ item }: { item: Action }) => {
	switch (item.name) {
		case "应用详情":
			await faPopupRef.value.close();
			await detailFormRef.value.detail(state.clickedApplication.appId);
			break;
		case "编辑应用":
			await handleEdit("edit", state.clickedApplication.appId);
			break;
		case "删除应用":
			await useMessageBox.confirm("确定要删除应用？", {
				async beforeConfirm() {
					await applicationApi.deleteApplication({
						appId: state.clickedApplication.appId,
						rowVersion: state.clickedApplication.rowVersion,
					});
					useToast.success("删除成功！");
					await handleLoad();
				},
			});
			break;
	}
});

onShow(() => {
	if (pageVersion !== pagingStore.getPageVersion("Application")) handleLoad();
});

onPullDownRefresh(() => handleLoad().finally(() => uni.stopPullDownRefresh()));
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

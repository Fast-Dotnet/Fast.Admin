<template>
	<FaPaging ref="faPagingRef" page-key="Menu" :request-api="menuApi.queryMenuPaged" :pagination="false" :init-param="{ treeData: false }">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput hide-search-time v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaApplicationSelect v-model="searchParam.value.appId" />
				</template>
				<template #footer>
					<FaAuth code="Menu:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.menuId" :title="item.menuName" code="Menu:Detail" @click="detailFormRef.detail(item.menuId)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="MenuTypeEnum" :value="item.menuType" />
						<FaTag name="CommonStatusEnum" :value="item.status" />
					</view>
				</template>

				<FaField label="菜单编码" :value="item.menuCode" />
				<FaField label="菜单标题" :value="item.menuTitle" />
				<FaField label="应用" :value="item.appName" />
				<FaField label="显示" center>
					<FaTag name="BooleanEnum" :value="item.visible" />
				</FaField>
				<FaField label="版本" center>
					<FaTag name="EditionEnum" :value="item.edition" />
				</FaField>
				<FaField label="平台">
					<view class="fa-card__title-view">
						<wd-icon v-if="item.hasDesktop" name="desktop" />
						<wd-icon v-if="item.hasMobile" name="mobile" />
						<wd-icon v-if="item.hasWeb" name="compass" />
					</view>
				</FaField>
				<FaField label="排序" :value="item.sort" />

				<template #footer>
					<FaAuth code="Menu:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.menuId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Menu:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
					<FaAuth code="Menu:Status">
						<wd-button
							v-if="item.status === CommonStatusEnum.Enable"
							size="mini"
							type="danger"
							variant="plain"
							@click="handleChangeStatus(item)"
						>
							禁用
						</wd-button>
						<wd-button v-else size="mini" type="warning" variant="plain" @click="handleChangeStatus(item)">启用</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<MenuDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { menuApi } from "@/api/services/Center/menu";
import { useMessageBox, useToast } from "@/hooks";
import MenuDetail from "./components/detail.vue";
import type { QueryMenuPagedOutput } from "@/api/services/Center/menu/models/QueryMenuPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "DevMenu",
	authCode: "Menu:Paged",
	style: {
		navigationBarTitleText: "菜单管理",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof MenuDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, menuId?: string) =>
		await router.push({
			path: "/pages_dev/menu/edit/index",
			query: {
				mode,
				menuId,
			},
		})
);

const handleDelete = throttle(async (item: QueryMenuPagedOutput) => {
	const { menuId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除菜单？", {
		async beforeConfirm() {
			await menuApi.deleteMenu({ menuId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});

const handleChangeStatus = throttle(async (item: QueryMenuPagedOutput) => {
	const { menuId, status, rowVersion } = item;
	await useMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}菜单？`, {
		async beforeConfirm() {
			await menuApi.changeStatus({ menuId, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

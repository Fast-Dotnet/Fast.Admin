<template>
	<FaPaging ref="faPagingRef" :request-api="configApi.queryConfigPaged">
		<template #top="{ searchParam, setParam, refresh }">
			<FaSearchInput
				:filter="false"
				:search-param="searchParam"
				placeholder="搜索配置编码、名称"
				@update:search-param="setParam"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #footer>
					<FaAuth code="Config:Edit">
						<wd-button size="small" icon="plus" @click="handleOpen('add')">新增</wd-button>
						<wd-button size="small" icon="delete" type="warning" variant="plain" @click="handleDeleteAll">清除全部缓存</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.configId"
				:title="item.configCode"
				code="Config:Detail"
				@click="handleOpen('detail', item.configId)"
			>
				<FaField label="配置名称" :value="item.configName" />
				<FaField label="配置值">
					<text v-if="item.configValue" class="config-value">{{ item.configValue }}</text>
				</FaField>
				<FaField label="备注" :value="item.remark" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Config:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleOpen('edit', item.configId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Config:Edit">
						<wd-button size="mini" type="warning" variant="plain" @click="handleDelete(item)">清除缓存</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "@wot-ui/router";
import { configApi } from "@/api/services/Center/config";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryConfigPagedOutput } from "@/api/services/Center/config/models/QueryConfigPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "DevConfig",
	layout: "layout",
	authCode: "Config:Paged",
	style: {
		navigationBarTitleText: "系统配置",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleOpen = (mode: IPageStateType, configId?: string) => router.push(`/pages_dev/config/edit/index?mode=${mode}&configId=${configId}`);

const handleDelete = (item: QueryConfigPagedOutput) => {
	const { configCode } = item;
	useMessageBox.confirm("确定要删除缓存？", {
		async beforeConfirm() {
			await configApi.deleteConfigCache({ configCode });
			useToast.success("删除成功！");
			faPagingRef.value.reload();
		},
	});
};

const handleDeleteAll = () => {
	useMessageBox.confirm("确定要删除全部缓存？", {
		async beforeConfirm() {
			await configApi.deleteAllConfigCache();
			useToast.success("删除成功！");
			faPagingRef.value.reload();
		},
	});
};
</script>

<style lang="scss" scoped>
.config-value {
	margin: var(--fa-page-section-gap) 0;
	padding: 20rpx;
	border-radius: var(--wot-radius-main);
	background: var(--wot-filled-bottom);
	font-size: var(--wot-typography-body-size-main);
	white-space: pre-wrap;
	overflow-wrap: anywhere;
	display: -webkit-box;
	-webkit-line-clamp: 3;
	-webkit-box-orient: vertical;
	overflow: hidden;
}
</style>

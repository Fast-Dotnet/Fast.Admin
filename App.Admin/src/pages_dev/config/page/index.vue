<template>
	<FaPaging ref="faPagingRef" :request-api="configApi.queryConfigPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Config:Edit">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
						<wd-button size="small" icon="delete" type="warning" variant="plain" @click="handleDeleteAll()">清除全部缓存</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.configId"
				:title="item.configName"
				code="Config:Detail"
				@click="detailFormRef?.detail(item.configId)"
			>
				<FaField label="配置编码" :value="item.configCode" />
				<FaField label="配置值">
					<text class="config-value">{{ item.configValue }}</text>
				</FaField>
				<FaField label="备注" :value="item.remark" />

				<template #footer>
					<FaAuth code="Config:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.configId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Config:Edit">
						<wd-button size="mini" type="warning" variant="plain" @click="handleDelete(item)">清除缓存</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<ConfigDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { configApi } from "@/api/services/Center/config";
import { useMessageBox, useToast } from "@/hooks";
import ConfigDetail from "./components/detail.vue";
import type { QueryConfigPagedOutput } from "@/api/services/Center/config/models/QueryConfigPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "DevConfig",
	authCode: "Config:Paged",
	style: {
		navigationBarTitleText: "系统配置",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof ConfigDetail>>();

const handleEdit = throttle((mode: IPageStateType, configId?: string) =>
	router.push({
		path: "/pages_dev/config/edit/index",
		query: {
			mode,
			configId,
		},
	})
);

const handleDelete = (item: QueryConfigPagedOutput) => {
	const { configCode } = item;
	useMessageBox.confirm("确定要删除缓存？", {
		async beforeConfirm() {
			await configApi.deleteConfigCache({ configCode });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
};

const handleDeleteAll = () => {
	useMessageBox.confirm("确定要删除全部缓存？", {
		async beforeConfirm() {
			await configApi.deleteAllConfigCache();
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
};
</script>

<style lang="scss" scoped>
.config-value {
	flex: 1;
	margin: 12rpx 0;
	padding: 20rpx 10rpx;
	border-radius: var(--wot-radius-main);
	background: var(--wot-filled-bottom);
	font-size: var(--wot-typography-body-size-main);
	white-space: pre-wrap;
	overflow-wrap: anywhere;
}
</style>

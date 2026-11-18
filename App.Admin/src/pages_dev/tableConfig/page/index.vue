<template>
	<FaPaging ref="faPagingRef" page-key="TableConfig" :request-api="tableApi.queryTableConfigPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Table:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.tableId" :title="item.tableName">
				<FaField label="表格Key" :value="item.tableKey" selectable />
				<FaField label="备注" :value="item.remark" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Table:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.tableId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Table:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('copy', item.tableId)">复制</wd-button>
					</FaAuth>
					<FaAuth code="Table:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleConfig(item.tableId)">配置列</wd-button>
					</FaAuth>
					<FaAuth code="Table:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { tableApi } from "@/api/services/Center/table";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryTableConfigPagedOutput } from "@/api/services/Center/table/models/QueryTableConfigPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "DevTableConfig",
	authCode: "Table:Paged",
	style: {
		navigationBarTitleText: "表格配置",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleEdit = throttle(
	async (mode: IPageStateType, tableId?: string) =>
		await router.push({
			path: "/pages_dev/tableConfig/edit/index",
			query: {
				mode,
				tableId,
			},
		})
);

const handleConfig = throttle(async (tableId: string) => await router.push(`/pages_dev/tableConfig/config/index?tableId=${tableId}`));

const handleDelete = throttle(async (item: QueryTableConfigPagedOutput) => {
	const { tableId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除表格配置？", {
		async beforeConfirm() {
			await tableApi.deleteTableConfig({ tableId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

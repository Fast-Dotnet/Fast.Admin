<template>
	<FaPaging ref="faPagingRef" page-key="Position" :request-api="positionApi.queryPositionPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Position:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.positionId" :title="item.positionName">
				<FaField label="排序" :value="item.sort" />
				<FaField label="备注" :value="item.remark" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Position:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.positionId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Position:Delete">
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
import { positionApi } from "@/api/services/Admin/position";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryPositionPagedOutput } from "@/api/services/Admin/position/models/QueryPositionPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "AdminPosition",
	authCode: "Position:Paged",
	style: {
		navigationBarTitleText: "职位管理",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleEdit = throttle(
	async (mode: IPageStateType, positionId?: string) =>
		await router.push({
			path: "/pages_admin/position/edit/index",
			query: {
				mode,
				positionId,
			},
		})
);

const handleDelete = throttle(async (item: QueryPositionPagedOutput) => {
	const { positionId, rowVersion } = item;
	await useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await positionApi.deletePosition({ positionId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

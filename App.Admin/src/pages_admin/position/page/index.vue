<template>
	<FaPaging :request-api="positionApi.queryPositionPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Position:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard
				v-for="item in pagedList"
				:key="item.positionId"
				:title="item.positionName"
				code="Position:Detail"
				@click="handleDetail(item.positionId)"
			>
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
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item, refresh)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { positionApi } from "@/api/services/Admin/position";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryPositionPagedOutput } from "@/api/services/Admin/position/models/QueryPositionPagedOutput";

definePage({
	name: "AdminPosition",
	authCode: "Position:Paged",
	style: {
		navigationBarTitleText: "职位管理",
	},
});

const router = useRouter();

const handleDetail = throttle((positionId: string) => router.push(`/pages_admin/position/detail/index?positionId=${positionId}`));

const handleEdit = throttle((mode: IPageStateType, positionId?: string) =>
	router.push(`/pages_admin/position/edit/index?mode=${mode}&positionId=${positionId}`)
);

const handleDelete = (item: QueryPositionPagedOutput, refresh: () => Promise<void>) => {
	const { positionId, rowVersion } = item;
	useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await positionApi.deletePosition({ positionId, rowVersion });
			useToast.success("删除成功！");
			refresh();
		},
	});
};
</script>

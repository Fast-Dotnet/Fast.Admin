<template>
	<FaPaging ref="faPagingRef" page-key="JobLevel" :request-api="jobLevelApi.queryJobLevelPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="JobLevel:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.jobLevelId" :title="item.jobLevelName">
				<FaField label="等级" :value="item.level" selectable />
				<FaField label="备注" :value="item.remark" selectable />
				<FaField label="创建人" :value="item.createdUserName" selectable />
				<FaField label="创建时间" :value="item.createdTime" selectable />
				<FaField label="更新人" :value="item.updatedUserName" selectable />
				<FaField label="更新时间" :value="item.updatedTime" selectable />

				<template #footer>
					<FaAuth code="JobLevel:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.jobLevelId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="JobLevel:Delete">
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryJobLevelPagedOutput } from "@/api/services/Admin/jobLevel/models/QueryJobLevelPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "AdminJobLevel",
	authCode: "JobLevel:Paged",
	style: {
		navigationBarTitleText: "职级管理",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleEdit = throttle(
	async (mode: IPageStateType, jobLevelId?: string) =>
		await router.push({
			path: "/pages_admin/jobLevel/edit/index",
			query: {
				mode,
				jobLevelId,
			},
		})
);

const handleDelete = throttle(async (item: QueryJobLevelPagedOutput) => {
	const { jobLevelId, rowVersion } = item;
	await useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await jobLevelApi.deleteJobLevel({ jobLevelId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

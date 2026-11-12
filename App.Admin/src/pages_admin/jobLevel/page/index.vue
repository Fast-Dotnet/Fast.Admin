<template>
	<FaPaging :request-api="jobLevelApi.queryJobLevelPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="JobLevel:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard
				v-for="item in pagedList"
				:key="item.jobLevelId"
				:title="item.jobLevelName"
				code="JobLevel:Detail"
				@click="handleDetail(item.jobLevelId)"
			>
				<FaField label="等级" :value="item.level" />
				<FaField label="备注" :value="item.remark" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="JobLevel:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.jobLevelId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="JobLevel:Delete">
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryJobLevelPagedOutput } from "@/api/services/Admin/jobLevel/models/QueryJobLevelPagedOutput";

definePage({
	name: "AdminJobLevel",
	authCode: "JobLevel:Paged",
	style: {
		navigationBarTitleText: "职级管理",
	},
});

const router = useRouter();

const handleDetail = throttle((jobLevelId: string) => router.push(`/pages_admin/jobLevel/detail/index?jobLevelId=${jobLevelId}`));

const handleEdit = throttle((mode: IPageStateType, jobLevelId?: string) =>
	router.push(`/pages_admin/jobLevel/edit/index?mode=${mode}&jobLevelId=${jobLevelId}`)
);

const handleDelete = (item: QueryJobLevelPagedOutput, refresh: () => Promise<void>) => {
	const { jobLevelId, rowVersion } = item;
	useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await jobLevelApi.deleteJobLevel({ jobLevelId, rowVersion });
			useToast.success("删除成功！");
			refresh();
		},
	});
};
</script>

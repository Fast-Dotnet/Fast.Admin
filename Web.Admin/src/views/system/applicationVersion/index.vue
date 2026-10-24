<template>
	<div>
		<FaTable
			ref="faTableRef"
			row-key="recordId"
			:request-api="applicationVersionApi.queryApplicationVersionPaged"
			:columns="searchColumns"
			hide-search-time
		>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'AppVersion:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="appKey" label="应用标识" fixed="left" width="240" small-width="220" copy sortable />
			<FaTableColumn prop="appName" label="应用名称" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="appType"
				label="应用类型"
				width="150"
				small-width="130"
				sortable
				tag
				:enum="appStore.getDictionary('AppEnvironmentEnum')"
			/>
			<FaTableColumn
				prop="environmentType"
				label="环境类型"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('EnvironmentTypeEnum')"
			/>
			<FaTableColumn prop="packageType" label="包类型" width="150" small-width="130" sortable tag :enum="applicationPackageTypeEnum" />
			<FaTableColumn prop="versionName" label="版本名称" width="140" small-width="120" copy sortable />
			<FaTableColumn prop="versionCode" label="版本号" width="120" small-width="100" sortable />
			<FaTableColumn prop="status" label="状态" width="100" small-width="80" sortable tag :enum="commonStatusEnum" />
			<FaTableColumn prop="minNativeVersionCode" label="原生版本兼容区间" width="200" small-width="180">
				<template #default="{ row }: { row: QueryApplicationVersionPagedOutput }">
					<template v-if="row.packageType === ApplicationPackageTypeEnum.Wgt">
						{{ row.minNativeVersionCode }} ~ {{ row.maxNativeVersionCode }}
					</template>
					<template v-else>—</template>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="isForceUpdate"
				label="强制更新"
				width="110"
				small-width="90"
				sortable
				tag
				:enum="appStore.getDictionary('BooleanEnum')"
			/>
			<FaTableColumn prop="updateContent" label="更新说明" width="300" small-width="280" />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" />
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				width="240"
				small-width="220"
				sortable
				type="timeInfo"
				:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
			/>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryApplicationVersionPagedOutput }">
				<el-button v-auth="'AppVersion:Detail'" size="small" plain @click="editFormRef.detail(row.recordId)">详情</el-button>
				<el-button v-auth="'AppVersion:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.recordId)">编辑</el-button>
				<el-button v-auth="'AppVersion:Detail'" size="small" plain type="success" @click="handleDownload(row)">下载</el-button>
			</template>
		</FaTable>
		<ApplicationVersionEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script setup lang="ts">
import { computed, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import { fileApi } from "@/api/services/File";
import { useApp } from "@/stores";
import ApplicationVersionEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryApplicationVersionPagedOutput } from "@/api/services/Center/applicationVersion/models/QueryApplicationVersionPagedOutput";

defineOptions({
	name: "SystemApplicationVersion",
});

const appStore = useApp();
const applicationPackageTypeEnum = appStore.getDictionary("ApplicationPackageTypeEnum");
const commonStatusEnum = appStore.getDictionary("CommonStatusEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof ApplicationVersionEdit>>("editFormRef");

/** 下载 */
const handleDownload = (row: QueryApplicationVersionPagedOutput) => {
	faTableRef.value.doLoading(async () => {
		const apiRes = await applicationVersionApi.queryApplicationVersionDetail(row.recordId);
		await fileApi.download({ fileUrl: apiRes.fileUrl });
	});
};

const searchColumns = computed<FaTableColumnCtx[]>(() => [
	{
		pureSearch: true,
		prop: "packageType",
		label: "包类型",
		enum: applicationPackageTypeEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "status",
		label: "状态",
		enum: commonStatusEnum,
		search: { el: "el-select" },
	},
]);
</script>

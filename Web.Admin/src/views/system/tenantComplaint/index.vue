<template>
	<div>
		<FaTable
			ref="faTableRef"
			row-key="complaintId"
			:request-api="complaintApi.queryTenantComplaintPaged"
			:columns="searchColumns"
			hide-search-time
		>
			<FaTableColumn prop="appName" label="应用名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="appKey" label="应用标识" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="complaintType"
				label="投诉类型"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('ComplaintTypeEnum')"
			/>
			<FaTableColumn prop="nickName" label="昵称" width="150" small-width="130" sortable />
			<FaTableColumn prop="mobile" label="手机" width="150" small-width="130" sortable />
			<FaTableColumn prop="contactPhone" label="联系电话" width="150" small-width="130" sortable />
			<FaTableColumn prop="contactEmail" label="联系邮箱" width="200" small-width="180" sortable />
			<FaTableColumn prop="description" label="投诉描述" width="200" small-width="180" sortable />
			<FaTableColumn prop="attachmentImages" label="附件图片" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: QueryComplaintPagedOutput }">
					<el-button
						size="small"
						plain
						:disabled="!row.attachmentImages?.length"
						@click="state.previewSrcList = row.attachmentImages ?? []"
					>
						查看
					</el-button>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="handleTime" label="处理时间" width="180" small-width="160" sortable type="dateTime" date-fix />
			<FaTableColumn prop="handleDescription" label="处理描述" width="200" small-width="180" sortable />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" width="180" small-width="160" sortable type="dateTime" date-fix />

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryComplaintPagedOutput }">
				<el-button
					v-if="!row.handleTime"
					v-auth="'Complaint:TenantHandle'"
					size="small"
					plain
					type="primary"
					@click="editFormRef.open(row.complaintId)"
				>
					处理
				</el-button>
			</template>
		</FaTable>
		<TenantComplaintEdit ref="editFormRef" @ok="faTableRef.refresh()" />
		<el-image-viewer
			v-if="state.previewSrcList.length > 0"
			:url-list="state.previewSrcList"
			hide-on-click-modal
			teleported
			show-progress
			@close="state.previewSrcList = []"
		/>
	</div>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { complaintApi } from "@/api/services/Center/complaint";
import { useApp } from "@/stores";
import TenantComplaintEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryComplaintPagedOutput } from "@/api/services/Center/complaint/models/QueryComplaintPagedOutput";

defineOptions({
	name: "SystemTenantComplaint",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof TenantComplaintEdit>>("editFormRef");

const state = reactive({
	previewSrcList: withDefineType<string[]>([]),
});

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "complaintType",
		label: "投诉类型",
		enum: appStore.getDictionary("ComplaintTypeEnum"),
		search: { el: "el-select" },
	},
];
</script>

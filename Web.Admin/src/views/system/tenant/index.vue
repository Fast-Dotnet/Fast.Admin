<template>
	<div>
		<FaTable ref="faTableRef" row-key="tenantId" :request-api="tenantApi.queryTenantPaged" :columns="searchColumns" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Tenant:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="logoUrl" label="Logo" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn
				prop="tenantName"
				label="租户名称"
				fixed="left"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.tenantId)"
			/>
			<FaTableColumn prop="tenantNo" label="租户编号" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="tenantCode" label="租户编码" width="120" small-width="100" sortable />
			<FaTableColumn prop="tenantType" label="租户类型" width="120" small-width="100" sortable tag :enum="tenantTypeEnum" />
			<FaTableColumn prop="status" label="状态" width="100" small-width="80" sortable tag :enum="commonStatusEnum" />
			<FaTableColumn prop="edition" label="版本" width="100" small-width="80" sortable tag :enum="editionEnum" />
			<FaTableColumn prop="shortName" label="租户简称" width="150" small-width="130" sortable />
			<FaTableColumn prop="spellName" label="租户英文名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="adminName" label="管理员名称" width="150" small-width="130" sortable />
			<FaTableColumn prop="adminMobile" label="管理员手机" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="adminEmail" label="管理员邮箱" width="200" small-width="180" copy sortable />
			<FaTableColumn prop="adminPhone" label="管理员电话" width="150" small-width="130" sortable />
			<FaTableColumn prop="robotName" label="机器人名称" width="150" small-width="130" sortable />
			<FaTableColumn prop="aliyunSignName" label="阿里云签名名称" width="200" small-width="180" copy sortable />
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
			<template #operation="{ row }: { row: QueryTenantPagedOutput }">
				<el-button v-auth="'Tenant:Detail'" size="small" plain @click="editFormRef.detail(row.tenantId)">详情</el-button>
				<el-button v-auth="'Tenant:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.tenantId)">编辑</el-button>
				<el-button
					v-if="row.status == CommonStatusEnum.Enable"
					v-auth="'Tenant:Status'"
					size="small"
					plain
					type="danger"
					@click="handleChangeStatus(row)"
				>
					禁用
				</el-button>
				<el-button v-else v-auth="'Tenant:Status'" size="small" plain type="warning" @click="handleChangeStatus(row)">启用</el-button>
			</template>
		</FaTable>
		<TenantEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { tenantApi } from "@/api/services/Center/tenant";
import { useApp } from "@/stores";
import TenantEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryTenantPagedOutput } from "@/api/services/Center/tenant/models/QueryTenantPagedOutput";

defineOptions({
	name: "SystemTenant",
});

const appStore = useApp();
const tenantTypeEnum = appStore.getDictionary("TenantTypeEnum");
const commonStatusEnum = appStore.getDictionary("CommonStatusEnum");
const editionEnum = appStore.getDictionary("EditionEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof TenantEdit>>("editFormRef");

/** 处理状态变更 */
const handleChangeStatus = (row: QueryTenantPagedOutput) => {
	const { tenantId, status, rowVersion } = row;
	ElMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}租户？`, { type: "warning" }).then(async () => {
		await tenantApi.changeStatus({
			tenantId,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "status",
		label: "状态",
		enum: commonStatusEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "edition",
		label: "版本",
		enum: editionEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "adminMobile",
		label: "管理员手机",
		search: { el: "el-input" },
	},
	{
		pureSearch: true,
		prop: "adminEmail",
		label: "管理员邮箱",
		search: { el: "el-input" },
	},
	{
		pureSearch: true,
		prop: "tenantType",
		label: "租户类型",
		enum: tenantTypeEnum,
		search: { el: "el-select" },
	},
];
</script>

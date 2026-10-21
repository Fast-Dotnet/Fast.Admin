<template>
	<div>
		<FaTable ref="faTableRef" row-key="roleId" :request-api="roleApi.queryRolePaged" :columns="searchColumns" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Role:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="roleName"
				label="角色名称"
				fixed="left"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.roleId)"
			/>
			<FaTableColumn prop="roleCode" label="角色编码" width="200" small-width="180" copy sortable />
			<FaTableColumn prop="roleType" label="角色类型" width="120" small-width="100" sortable tag :enum="roleTypeEnum" />
			<FaTableColumn prop="dataScopeType" label="数据范围" width="150" small-width="130" sortable tag :enum="dataScopeTypeEnum" />
			<FaTableColumn prop="sort" label="排序" width="100" small-width="80" sortable />
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
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
			<template #operation="{ row }: { row: QueryRolePagedOutput }">
				<el-button v-auth="'Role:Detail'" size="small" plain @click="editFormRef.detail(row.roleId)">详情</el-button>
				<el-button v-auth="'Role:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.roleId)">编辑</el-button>
				<el-button v-if="!row.isSystemMenu" v-auth="'Role:Edit'" size="small" plain type="success" @click="authEditRef.open(row.roleId)">
					授权
				</el-button>
				<el-button v-auth="'Role:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<RoleEdit ref="editFormRef" @ok="faTableRef.refresh()" />
		<AuthEdit ref="authEditRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { roleApi } from "@/api/services/Admin/role";
import { useApp } from "@/stores";
import AuthEdit from "./edit/authEdit.vue";
import RoleEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryRolePagedOutput } from "@/api/services/Admin/role/models/QueryRolePagedOutput";

defineOptions({
	name: "AdminRole",
});

const appStore = useApp();
const roleTypeEnum = appStore.getDictionary("RoleTypeEnum");
const dataScopeTypeEnum = appStore.getDictionary("DataScopeTypeEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof RoleEdit>>("editFormRef");
const authEditRef = useTemplateRef<InstanceType<typeof AuthEdit>>("authEditRef");

/** 处理删除 */
const handleDelete = (row: QueryRolePagedOutput) => {
	const { roleId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除角色？", { type: "warning" }).then(async () => {
		await roleApi.deleteRole({ roleId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "roleType",
		label: "角色类型",
		enum: roleTypeEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "dataScopeType",
		label: "数据范围",
		enum: dataScopeTypeEnum,
		search: { el: "el-select" },
	},
];
</script>

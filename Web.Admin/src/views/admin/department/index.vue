<template>
	<div>
		<div class="fa__display_lr-r">
			<FaTree
				ref="orgTreeRef"
				title="机构列表"
				width="240"
				:request-api="organizationApi.organizationSelector"
				@change="handleOrgChange"
				@node-contextmenu="(event, data) => handleOrgContextmenu(event as MouseEvent, data)"
			>
				<template #label="{ data }">
					<span>{{ data.label }}</span>
				</template>
				<template #default="{ data }">
					<el-text type="info">{{ data.data?.orgCode }}</el-text>
				</template>
			</FaTree>
			<FaTable
				ref="faTableRef"
				row-key="departmentId"
				:request-api="departmentApi.queryDepartmentPaged"
				hide-search-time
				:pagination="false"
				default-expand-all
			>
				<!-- 表格按钮操作区域 -->
				<template #header>
					<el-button v-auth="'Department:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
				</template>

				<FaTableColumn
					prop="departmentName"
					label="部门名称"
					fixed="left"
					width="200"
					small-width="180"
					copy
					sortable
					link
					:click="({ row }) => editFormRef.detail(row.departmentId)"
				/>
				<FaTableColumn
					prop="departmentCode"
					label="部门编码"
					width="200"
					small-width="180"
					copy
					sortable
					link
					:click="({ row }) => editFormRef.detail(row.departmentId)"
				/>
				<FaTableColumn prop="contacts" label="联系人" width="150" small-width="130" sortable />
				<FaTableColumn prop="phone" label="电话" width="150" small-width="130" sortable />
				<FaTableColumn prop="email" label="邮箱" width="150" small-width="130" sortable />
				<FaTableColumn prop="sort" label="排序" width="100" small-width="80" sortable />
				<FaTableColumn
					prop="dataPublic"
					label="数据公开"
					width="120"
					small-width="100"
					sortable
					tag
					:enum="appStore.getDictionary('BooleanEnum')"
				/>
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
				<template #operation="{ row }: { row: QueryDepartmentPagedOutput }">
					<el-button v-auth="'Department:Detail'" size="small" plain @click="editFormRef.detail(row.departmentId)">详情</el-button>
					<el-button v-auth="'Department:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.departmentId)">
						编辑
					</el-button>
					<el-button v-auth="'Department:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
				</template>
			</FaTable>
		</div>
		<FaContextMenu ref="faContextMenuRef" :data="state.contextMenuList" />
		<DepartmentEdit ref="editFormRef" @ok="faTableRef.refresh()" />
		<OrgEdit ref="orgEditFormRef" @ok="orgTreeRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { departmentApi } from "@/api/services/Admin/department";
import { organizationApi } from "@/api/services/Admin/organization";
import { useApp } from "@/stores";
import DepartmentEdit from "./edit/index.vue";
import OrgEdit from "./edit/orgEdit.vue";
import type { ElTreeOutput, FaContextMenuData, FaContextMenuInstance, FaTableInstance, FaTreeInstance } from "fast-element-plus";
import type { QueryDepartmentPagedOutput } from "@/api/services/Admin/department/models/QueryDepartmentPagedOutput";

defineOptions({
	name: "AdminDepartment",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const orgTreeRef = useTemplateRef<FaTreeInstance>("orgTreeRef");
const faContextMenuRef = useTemplateRef<FaContextMenuInstance>("faContextMenuRef");
const editFormRef = useTemplateRef<InstanceType<typeof DepartmentEdit>>("editFormRef");
const orgEditFormRef = useTemplateRef<InstanceType<typeof OrgEdit>>("orgEditFormRef");

const state = reactive({
	contextMenuList: withDefineType<FaContextMenuData[]>([
		{
			name: "add",
			label: "添加机构",
			icon: "el-icon-FolderAdd",
			click: () => {
				orgEditFormRef.value.add();
			},
		},
		{
			name: "edit",
			label: "编辑机构",
			icon: "el-icon-EditPen",
			click: (_, { data }: { data?: ElTreeOutput }) => {
				if (!data || typeof data.value !== "string") return;
				orgEditFormRef.value.edit(data.value);
			},
		},
		{
			name: "delete",
			label: "删除机构",
			icon: "el-icon-Delete",
			click: (_, { data }: { data?: ElTreeOutput }) => {
				if (!data || typeof data.value !== "string") return;
				const { value: orgId, data: orgData } = data;
				ElMessageBox.confirm("确定要删除机构？", { type: "warning" }).then(async () => {
					await organizationApi.deleteOrganization({ orgId, rowVersion: orgData?.rowVersion });
					ElMessage.success("删除成功！");
					await orgTreeRef.value?.refresh();
				});
			},
		},
	]),
});

/** 机构更改 */
const handleOrgChange = async (data: ElTreeOutput) => {
	faTableRef.value.searchParam.orgId = data.value;
	await faTableRef.value.refresh();
};

const handleOrgContextmenu = (event: MouseEvent, data: ElTreeOutput) => {
	if (data.all) {
		state.contextMenuList[0].hide = false;
		state.contextMenuList[1].hide = true;
		state.contextMenuList[2].hide = true;
	} else {
		state.contextMenuList[0].hide = true;
		state.contextMenuList[1].hide = false;
		state.contextMenuList[2].hide = false;
	}
	state.contextMenuList.forEach((item) => {
		item.data = data;
	});
	faContextMenuRef.value.open({ x: event.clientX, y: event.clientY });
};

/** 处理删除 */
const handleDelete = (row: QueryDepartmentPagedOutput) => {
	const { departmentId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除部门？", { type: "warning" }).then(async () => {
		await departmentApi.deleteDepartment({ departmentId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};
</script>

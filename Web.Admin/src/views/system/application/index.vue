<template>
	<div>
		<FaTable
			ref="faTableRef"
			row-key="appId"
			:request-api="applicationApi.queryApplicationPaged"
			:columns="searchColumns"
			hide-search-time
			default-expand-all
		>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'App:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<template #expand="{ row }: { row: QueryApplicationPagedOutput }">
				<FaTable row-key="recordId" :data="row.clientList" :pagination="false" :tool-btn="false">
					<!-- 表格按钮操作区域 -->
					<template #header>
						<el-button
							v-auth="'AppClient:Add'"
							type="primary"
							:icon="Plus"
							@click="
								handleClientData(row);
								clientEditFormRef.add();
							"
						>
							新增标识
						</el-button>
					</template>

					<FaTableColumn
						prop="appKey"
						label="应用标识"
						fixed="left"
						width="220"
						small-width="200"
						copy
						link
						:click="({ row }) => clientEditFormRef.detail(row.recordId)"
					/>
					<FaTableColumn
						prop="appType"
						label="应用类型"
						width="160"
						small-width="140"
						tag
						:enum="appStore.getDictionary('AppEnvironmentEnum')"
					/>
					<FaTableColumn
						prop="environmentType"
						label="环境类型"
						width="130"
						small-width="110"
						tag
						:enum="appStore.getDictionary('EnvironmentTypeEnum')"
					/>
					<FaTableColumn prop="loginComponent" label="登录组件" width="160" small-width="140">
						<template #default="{ row: clientRow }: { row: QueryApplicationClientOutput }">
							<el-tag v-if="clientRow.loginComponent" type="primary">{{ clientRow.loginComponent }}</el-tag>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="requestTimeout" label="请求超时（毫秒）" width="160" small-width="140" />
					<FaTableColumn
						prop="requestEncipher"
						label="请求加密"
						width="120"
						small-width="100"
						tag
						:enum="appStore.getDictionary('BooleanEnum')"
					/>
					<FaTableColumn prop="weChatMerchantNo" label="微信商户号" width="180" small-width="160" copy />
					<FaTableColumn prop="alipayMerchantNo" label="支付宝商户号" width="180" small-width="160" copy />
					<FaTableColumn prop="remark" label="备注" width="200" small-width="180" />
					<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" type="timeInfo" />
					<FaTableColumn
						prop="updatedTime"
						label="更新时间"
						width="240"
						small-width="220"
						type="timeInfo"
						:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
					/>

					<template #operation="{ row: clientRow }: { row: QueryApplicationClientOutput }">
						<el-button
							v-auth="'AppClient:Detail'"
							size="small"
							plain
							@click="
								handleClientData(row);
								clientEditFormRef.detail(clientRow.recordId);
							"
						>
							详情
						</el-button>
						<el-button
							v-auth="'AppClient:Edit'"
							size="small"
							plain
							type="primary"
							@click="
								handleClientData(row);
								clientEditFormRef.edit(clientRow.recordId);
							"
						>
							编辑
						</el-button>
						<el-button v-auth="'AppClient:Delete'" size="small" plain type="danger" @click="handleClientDelete(clientRow)">
							删除
						</el-button>
					</template>
				</FaTable>
			</template>

			<FaTableColumn prop="logoUrl" label="Logo" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn
				prop="appName"
				label="应用名称"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.appId)"
			/>
			<FaTableColumn prop="appNo" label="应用编号" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="edition" label="版本" width="120" small-width="100" sortable tag :enum="appStore.getDictionary('EditionEnum')" />
			<FaTableColumn prop="themeColor" label="主题色" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: QueryApplicationPagedOutput }">
					<el-tag v-if="row?.themeColor" effect="dark" :color="row.themeColor" :style="{ borderColor: row.themeColor }">
						{{ row.themeColor }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="tenantName" label="租户名称" width="150" small-width="130" sortable />
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
			<template #operation="{ row }: { row: QueryApplicationPagedOutput }">
				<el-button v-auth="'App:Detail'" size="small" plain @click="editFormRef.detail(row.appId)">详情</el-button>
				<el-button v-auth="'App:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.appId)">编辑</el-button>
				<el-button v-auth="'App:Delete'" size="small" plain type="danger" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<ApplicationEdit ref="editFormRef" @ok="faTableRef.refresh()" />
		<ApplicationClientEdit ref="clientEditFormRef" :app-id="state.appId" :app-name="state.appName" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { applicationApi } from "@/api/services/Center/application";
import { applicationClientApi } from "@/api/services/Center/applicationClient/index.ts";
import { useApp } from "@/stores";
import ApplicationClientEdit from "./edit/clientEdit.vue";
import ApplicationEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryApplicationClientOutput } from "@/api/services/Center/application/models/QueryApplicationClientOutput.ts";
import type { QueryApplicationPagedOutput } from "@/api/services/Center/application/models/QueryApplicationPagedOutput";

defineOptions({
	name: "SystemApplication",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof ApplicationEdit>>("editFormRef");
const clientEditFormRef = useTemplateRef<InstanceType<typeof ApplicationClientEdit>>("clientEditFormRef");

const state = reactive({
	appId: "",
	appName: "",
});

const handleClientData = (row: QueryApplicationPagedOutput) => {
	state.appId = row.appId;
	state.appName = row.appName;
};

/** 处理删除 */
const handleDelete = (row: QueryApplicationPagedOutput) => {
	const { appId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除应用？", { type: "warning" }).then(async () => {
		await applicationApi.deleteApplication({ appId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理删除 */
const handleClientDelete = (row: QueryApplicationClientOutput) => {
	const { recordId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除应用标识？", { type: "warning" }).then(async () => {
		await applicationClientApi.deleteApplicationClient({ recordId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "edition",
		label: "版本",
		enum: appStore.getDictionary("EditionEnum"),
		search: { el: "el-select" },
	},
];
</script>

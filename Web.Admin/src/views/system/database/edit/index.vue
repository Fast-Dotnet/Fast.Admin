<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		:title="state.dialogTitle"
		:show-fullscreen="false"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="2">
			<FaFormItem prop="tenantId" label="租户">
				{{ state.formData.tenantName }}
			</FaFormItem>
			<FaFormItem prop="dbName" label="数据库名称">
				<div style="display: flex; gap: 3px">
					<el-input v-model="state.formData.dbName" maxlength="50" placeholder="请输入数据库名称" />
					<el-checkbox v-model="state.formData.isCreateDatabase">创建</el-checkbox>
				</div>
			</FaFormItem>
			<FaFormItem prop="publicIp" label="公网Ip地址" tips="可外网访问的公网Ip地址，一般情况下不建议配置，直接配置为内网Ip地址即可">
				<el-input v-model="state.formData.publicIp" maxlength="15" placeholder="请输入公网Ip地址" />
			</FaFormItem>
			<FaFormItem prop="intranetIp" label="内网Ip地址" tips="服务器内网Ip地址">
				<el-input v-model="state.formData.intranetIp" maxlength="15" placeholder="请输入内网Ip地址" />
			</FaFormItem>
			<FaFormItem prop="port" label="端口号" tips="如需外网访问需要开放防火墙">
				<el-input-number v-model="state.formData.port" :min="1" :max="65535" placeholder="请输入端口号" />
			</FaFormItem>
			<FaFormItem prop="dbType" label="Db类型" span="2">
				<RadioGroup name="SugarDbType" v-model="state.formData.dbType" />
			</FaFormItem>
			<FaFormItem prop="dbUser" label="数据库用户">
				<el-input v-model="state.formData.dbUser" maxlength="20" placeholder="请输入数据库用户" autocomplete="off" />
			</FaFormItem>
			<FaFormItem prop="dbPwd" label="数据库密码">
				<el-input
					type="password"
					v-model="state.formData.dbPwd"
					show-password
					maxlength="64"
					placeholder="请输入数据库密码"
					autocomplete="new-password"
				/>
			</FaFormItem>
			<FaFormItem prop="customConnectionStr" label="连接字符串" span="2">
				<el-input
					type="textarea"
					v-model="state.formData.customConnectionStr"
					:rows="2"
					maxlength="200"
					placeholder="请输入自定义连接字符串"
				/>
			</FaFormItem>
			<FaFormItem prop="commandTimeOut" label="超时时间">
				<el-input-number v-model="state.formData.commandTimeOut" :min="10" :max="600" placeholder="请输入超时时间">
					<template #suffix>秒</template>
				</el-input-number>
			</FaFormItem>
			<FaFormItem prop="sugarSqlExecMaxSeconds" label="警告时间">
				<el-input-number v-model="state.formData.sugarSqlExecMaxSeconds" :min="30" :max="600" placeholder="请输入警告时间">
					<template #suffix>秒</template>
				</el-input-number>
			</FaFormItem>
			<FaFormItem prop="diffLog" label="差异日志">
				<RadioGroup button name="BooleanEnum" v-model="state.formData.diffLog" />
			</FaFormItem>
			<FaFormItem prop="disableAop" label="禁用Aop">
				<RadioGroup button name="BooleanEnum" v-model="state.formData.disableAop" />
			</FaFormItem>

			<FaLayoutGridItem span="2" style="min-height: 300px; max-height: 500px">
				<FaTable row-key="buttonId" :data="state.formData.slaveDatabaseList" span="2">
					<!-- 表格按钮操作区域 -->
					<template #header>
						<el-button type="primary" :icon="Plus" @click="handleSlaveDatabaseAdd">新增</el-button>
					</template>
					<FaTableColumn prop="hitRate" label="命中率" width="100">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.hitRate`"
								:rules="[{ required: true, message: '请输入从库命中率', trigger: 'blur' }]"
							>
								<el-input-number v-model="row.hitRate" :min="1" :max="100" placeholder="请输入从库命中率" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="publicIp" label="公网Ip地址" width="220">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.publicIp`"
								:rules="[{ required: true, message: '请输入公网Ip地址', trigger: 'blur' }]"
							>
								<el-input v-model="row.publicIp" maxlength="15" placeholder="请输入公网Ip地址" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="intranetIp" label="内网Ip地址" width="220">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.intranetIp`"
								:rules="[{ required: true, message: '请输入内网Ip地址', trigger: 'blur' }]"
							>
								<el-input v-model="row.intranetIp" maxlength="15" placeholder="请输入内网Ip地址" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="port" label="端口号" width="150">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.port`"
								:rules="[{ required: true, message: '请输入端口号', trigger: 'blur' }]"
							>
								<el-input-number v-model="row.port" :min="1" :max="65535" placeholder="请输入端口号" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="dbName" label="数据库名称" width="280">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.dbName`"
								:rules="[{ required: true, message: '请输入数据库名称', trigger: 'blur' }]"
							>
								<el-input v-model="row.dbName" maxlength="50" placeholder="请输入数据库名称" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="dbUser" label="数据库用户" width="220">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.dbUser`"
								:rules="[{ required: true, message: '请输入数据库用户', trigger: 'blur' }]"
							>
								<el-input v-model="row.dbUser" maxlength="20" placeholder="请输入数据库用户" />
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="dbPwd" label="数据库密码" width="280">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item
								:prop="`slaveDatabaseList.${$index}.dbPwd`"
								:rules="[{ required: true, message: '请输入数据库密码', trigger: 'blur' }]"
							>
								<el-input
									type="password"
									v-model="row.dbPwd"
									show-password
									maxlength="64"
									placeholder="请输入数据库密码"
									autocomplete="new-password"
								/>
							</el-form-item>
						</template>
					</FaTableColumn>
					<FaTableColumn prop="customConnectionStr" label="连接字符串" width="300">
						<template #default="{ row, $index }: { row: EditSlaveDatabaseInput; $index: number }">
							<el-form-item :prop="`slaveDatabaseList.${$index}.customConnectionStr`">
								<el-input
									type="textarea"
									v-model="row.customConnectionStr"
									:rows="2"
									maxlength="200"
									placeholder="请输入自定义连接字符串"
								/>
							</el-form-item>
						</template>
					</FaTableColumn>
					<!-- 表格操作 -->
					<template #operation="{ $index }: { $index: number }">
						<el-button size="small" plain type="danger" @click="handleSlaveDatabaseDelete($index)">删除</el-button>
					</template>
				</FaTable>
			</FaLayoutGridItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { databaseApi } from "@/api/services/Center/database";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditDatabaseInput } from "@/api/services/Center/database/models/EditDatabaseInput";
import type { EditSlaveDatabaseInput } from "@/api/services/Center/database/models/EditSlaveDatabaseInput";
import type { QueryDatabasePagedInput } from "@/api/services/Center/database/models/QueryDatabasePagedInput";

defineOptions({
	name: "SystemDatabaseEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditDatabaseInput & QueryDatabasePagedInput>({}),
	formRules: withDefineType<FormRules<EditDatabaseInput>>({
		publicIp: [{ required: true, message: "请输入公网Ip地址", trigger: "blur" }],
		intranetIp: [{ required: true, message: "请输入内网Ip地址", trigger: "blur" }],
		port: [{ required: true, message: "请输入端口号", trigger: "blur" }],
		dbName: [{ required: true, message: "请输入数据库名称", trigger: "blur" }],
		dbUser: [{ required: true, message: "请输入数据库用户", trigger: "blur" }],
		dbPwd: [{ required: true, message: "请输入数据库密码", trigger: "blur" }],
		commandTimeOut: [{ required: true, message: "请输入超时时间", trigger: "blur" }],
		sugarSqlExecMaxSeconds: [{ required: true, message: "请输入警告时间", trigger: "blur" }],
	}),
	dialogTitle: "数据库",
});

const handleSlaveDatabaseAdd = () => {
	state.formData.slaveDatabaseList.push({
		hitRate: 10,
	});
};

const handleSlaveDatabaseDelete = (index: number) => {
	state.formData.slaveDatabaseList.splice(index, 1);
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await databaseApi.editDatabase(state.formData);
		ElMessage.success("保存成功！");
		emit("ok");
	});
};

const edit = (mainId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await databaseApi.queryDatabaseDetail(mainId);
		state.formData = apiRes;
		state.dialogTitle = `编辑数据库 - ${apiRes.dbName}`;
	});
};

defineExpose({
	element: faDialogRef,
	edit,
});
</script>

<style lang="scss" scoped>
.el-table__cell .el-form-item {
	margin-bottom: 0;
}
</style>

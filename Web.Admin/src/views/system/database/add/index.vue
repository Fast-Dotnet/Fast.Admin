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
				<TenantSelectPage v-model="state.formData.tenantId" />
			</FaFormItem>
			<FaFormItem prop="dbName" label="数据库名称">
				<div style="display: flex; gap: 3px">
					<el-input v-model="state.formData.dbName" maxlength="50" placeholder="请输入数据库名称" />
					<el-checkbox v-model="state.formData.isCreateDatabase">创建</el-checkbox>
				</div>
			</FaFormItem>
			<FaFormItem prop="databaseType" label="数据库类型" span="2">
				<RadioGroup name="DatabaseTypeEnum" v-model="state.formData.databaseType" />
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
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { DatabaseTypeEnum } from "@/api/enums/DatabaseTypeEnum";
import { SugarDbType } from "@/api/enums/SugarDbType";
import { databaseApi } from "@/api/services/Center/database";
import type { FormRules } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddDatabaseInput } from "@/api/services/Center/database/models/AddDatabaseInput";

defineOptions({
	name: "SystemDatabaseAdd",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<AddDatabaseInput>({}),
	formRules: withDefineType<FormRules<AddDatabaseInput>>({
		tenantId: [{ required: true, message: "请选择租户", trigger: "change" }],
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

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		await databaseApi.addDatabase(state.formData);
		ElMessage.success("新增成功！");
		emit("ok");
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogTitle = "添加数据库";
		state.formData = {
			databaseType: DatabaseTypeEnum.Admin,
			dbType: SugarDbType.SqlServer,
			publicIp: "127.0.0.1",
			intranetIp: "127.0.0.1",
			port: 1433,
			commandTimeOut: 60,
			sugarSqlExecMaxSeconds: 30,
			diffLog: true,
			disableAop: false,
		};
	});
};

defineExpose({
	element: faDialogRef,
	add,
});
</script>

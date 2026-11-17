<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<FaTenantSelect v-model="state.formData.tenantId" prop="tenantId" />
			<wd-form-item title="数据库名称" prop="dbName">
				<wd-input v-model="state.formData.dbName" clearable :maxlength="50" placeholder="请输入数据库名称" />
			</wd-form-item>
			<wd-form-item title="创建数据库" prop="isCreateDatabase">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.isCreateDatabase" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="数据库类型" prop="databaseType">
				<FaRadioGroup name="DatabaseTypeEnum" v-model="state.formData.databaseType" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="公网IP地址" prop="publicIp">
				<wd-input v-model="state.formData.publicIp" clearable :maxlength="15" placeholder="请输入公网IP地址" />
			</wd-form-item>
			<wd-form-item title="内网IP地址" prop="intranetIp">
				<wd-input v-model="state.formData.intranetIp" clearable :maxlength="15" placeholder="请输入内网IP地址" />
			</wd-form-item>
			<wd-form-item title="端口号" prop="port">
				<wd-input v-model="state.formData.port" type="number" placeholder="请输入端口号" />
			</wd-form-item>
			<wd-form-item title="Db类型" prop="dbType">
				<FaRadioGroup name="SugarDbType" v-model="state.formData.dbType" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="数据库用户" prop="dbUser">
				<wd-input v-model="state.formData.dbUser" clearable :maxlength="20" placeholder="请输入数据库用户" />
			</wd-form-item>
			<wd-form-item title="数据库密码" prop="dbPwd">
				<wd-input v-model="state.formData.dbPwd" clearable :maxlength="64" placeholder="请输入数据库密码" show-password />
			</wd-form-item>
			<wd-form-item title="连接字符串" prop="customConnectionStr">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.customConnectionStr"
					clearable
					:maxlength="200"
					placeholder="请输入连接字符串"
				/>
			</wd-form-item>
			<wd-form-item title="超时时间" prop="commandTimeOut">
				<wd-input v-model="state.formData.commandTimeOut" type="number" placeholder="请输入超时时间">
					<template #suffix>秒</template>
				</wd-input>
			</wd-form-item>
			<wd-form-item title="警告时间" prop="sugarSqlExecMaxSeconds">
				<wd-input v-model="state.formData.sugarSqlExecMaxSeconds" type="number" placeholder="请输入警告时间">
					<template #suffix>秒</template>
				</wd-input>
			</wd-form-item>
			<wd-form-item title="差异日志" prop="diffLog">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.diffLog" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="禁用Aop" prop="disableAop">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.disableAop" direction="horizontal" />
			</wd-form-item>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { DatabaseTypeEnum } from "@/api/enums/DatabaseTypeEnum";
import { SugarDbType } from "@/api/enums/SugarDbType";
import { databaseApi } from "@/api/services/Center/database";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddDatabaseInput } from "@/api/services/Center/database/models/AddDatabaseInput";

definePage({
	name: "SystemDatabaseAdd",
	authCode: "Database:Add",
	style: {
		navigationBarTitleText: "添加数据库",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddDatabaseInput>({}),
	formSchema: useFormSchema<AddDatabaseInput>({
		tenantId: { required: true, message: "请选择租户" },
		publicIp: { required: true, message: "请输入公网IP地址" },
		intranetIp: { required: true, message: "请输入内网IP地址" },
		port: {
			required: true,
			message: "请输入端口号",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 65535 ? undefined : "端口号必须是 1 到 65535 之间的整数"),
		},
		dbName: { required: true, message: "请输入数据库名称" },
		dbUser: { required: true, message: "请输入数据库用户" },
		dbPwd: { required: true, message: "请输入数据库密码" },
		commandTimeOut: {
			required: true,
			message: "请输入超时时间",
			validator: (value) => (Number.isInteger(value) && value >= 10 && value <= 600 ? undefined : "超时时间必须是 10 到 600 秒之间的整数"),
		},
		sugarSqlExecMaxSeconds: {
			required: true,
			message: "请输入警告时间",
			validator: (value) => (Number.isInteger(value) && value >= 30 && value <= 600 ? undefined : "警告时间必须是 30 到 600 秒之间的整数"),
		},
	}),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		await databaseApi.addDatabase(state.formData);
		useToast.success("新增成功！");
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Database");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/database/page/index");
});

const handleLoad = () => {
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
		isCreateDatabase: false,
	};
};

onLoad(() => {
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

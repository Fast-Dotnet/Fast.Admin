<template>
	<FaDialog ref="faDialogRef" width="1000" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<el-row>
			<el-col :span="4" style="text-align: center">
				<FaImage style="height: 100px; width: 100px" :src="state.formData.avatar" />
			</el-col>

			<el-col :span="20">
				<FaForm :model="state.formData" detail-form cols="2">
					<FaFormItem label="手机">
						<el-text type="primary">{{ state.formData.mobile }}</el-text>
					</FaFormItem>
					<FaFormItem label="邮箱">
						<el-text type="primary">{{ state.formData.email }}</el-text>
					</FaFormItem>
					<FaFormItem label="昵称" :value="state.formData.nickName" />
					<FaFormItem label="状态">
						<Text name="CommonStatusEnum" :value="state.formData.status" />
					</FaFormItem>
					<FaFormItem label="创建时间" :value="state.formData.createdTime" />
					<FaFormItem label="更新时间" :value="state.formData.updatedTime || '-'" />
				</FaForm>
			</el-col>
		</el-row>

		<FaForm :model="state.formData" detail-form cols="2">
			<FaLayoutGridItem span="2">
				<el-divider content-position="left">初次登录信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="租户">
				<el-text type="primary">{{ state.formData.firstLoginTenantName }}</el-text>
			</FaFormItem>
			<FaFormItem label="Ip">
				<el-text type="success">{{ state.formData.firstLoginIp }}</el-text>
			</FaFormItem>
			<FaFormItem label="设备" :value="state.formData.firstLoginDevice" />
			<FaFormItem label="操作系统" :value="state.formData.firstLoginOS" />
			<FaFormItem label="浏览器" :value="state.formData.firstLoginBrowser" />
			<FaFormItem label="地区">{{ state.formData.firstLoginProvince }} - {{ state.formData.firstLoginCity }}</FaFormItem>
			<FaFormItem label="时间" :value="state.formData.firstLoginTime || '-'" />

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">最后登录信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="租户">
				<el-text type="primary">{{ state.formData.lastLoginTenantName }}</el-text>
			</FaFormItem>
			<FaFormItem label="Ip">
				<el-text type="success">{{ state.formData.lastLoginIp }}</el-text>
			</FaFormItem>
			<FaFormItem label="设备" :value="state.formData.lastLoginDevice" />
			<FaFormItem label="操作系统" :value="state.formData.lastLoginOS" />
			<FaFormItem label="浏览器" :value="state.formData.lastLoginBrowser" />
			<FaFormItem label="地区">{{ state.formData.lastLoginProvince }} - {{ state.formData.lastLoginCity }}</FaFormItem>
			<FaFormItem label="时间" :value="state.formData.lastLoginTime || '-'" />

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">验证信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="错误次数">
				<template v-if="state.formData.passwordErrorTime > 0">
					<el-text type="warning">{{ state.formData.passwordErrorTime }}次</el-text>
				</template>
				<template v-else>-</template>
			</FaFormItem>
			<FaFormItem label="锁定时间">
				<template v-if="state.formData.lockStartTime">
					<el-text type="warning">
						{{ state.formData.lockStartTime }}
						~
						{{ state.formData.lockEndTime }}
					</el-text>
				</template>
				<template v-else>-</template>
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryAccountDetailOutput } from "@/api/services/Center/account/models/QueryAccountDetailOutput";

defineOptions({
	name: "SystemAccountDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryAccountDetailOutput>({}),
	dialogTitle: "账号",
});

const detail = (accountId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await accountApi.queryAccountDetail(accountId);
		state.formData = apiRes;
		state.dialogTitle = `账号详情 - ${apiRes.mobile}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>

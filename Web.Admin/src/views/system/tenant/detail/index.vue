<template>
	<FaDialog ref="faDialogRef" width="1000" :title="state.dialogTitle" :show-fullscreen="false" :show-confirm-button="false">
		<el-row>
			<el-col :span="4" style="text-align: center">
				<FaImage style="width: 100px; height: 100px" :src="state.formData.logoUrl" />
			</el-col>

			<el-col :span="20">
				<FaForm :model="state.formData" detail-form cols="2">
					<FaFormItem label="租户名称" :value="state.formData.tenantName" />
					<FaFormItem label="租户编号" :value="state.formData.tenantNo" />
					<FaFormItem label="租户编码" :value="state.formData.tenantCode" />
					<FaFormItem label="租户简称" :value="state.formData.shortName" />
					<FaFormItem label="租户英文名称" :value="state.formData.spellName" span="2" />
					<FaFormItem label="版本">
						<Tag name="EditionEnum" :value="state.formData.edition" />
					</FaFormItem>
					<FaFormItem label="租户类型">
						<Tag name="TenantTypeEnum" :value="state.formData.tenantType" />
					</FaFormItem>
					<FaFormItem label="机器人名称" :value="state.formData.robotName" />
					<FaFormItem label="状态">
						<Tag name="CommonStatusEnum" :value="state.formData.status" />
					</FaFormItem>
				</FaForm>
			</el-col>
		</el-row>

		<FaForm :model="state.formData" detail-form cols="2">
			<FaLayoutGridItem span="2">
				<el-divider content-position="left">管理员信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="管理员名称" :value="state.formData.adminName" />
			<FaFormItem label="管理员手机" :value="state.formData.adminMobile" />
			<FaFormItem label="管理员邮箱" :value="state.formData.adminEmail" />
			<FaFormItem label="管理员电话" :value="state.formData.adminPhone" />

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">阿里云</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="签名名称" :value="state.formData.aliyunSignName" span="2" />

			<FaLayoutGridItem span="2">
				<el-divider content-position="left">其他信息</el-divider>
			</FaLayoutGridItem>
			<FaFormItem label="创建人" :value="state.formData.createdUserName" />
			<FaFormItem label="创建时间" :value="state.formData.createdTime" />
			<FaFormItem label="更新人" :value="state.formData.updatedUserName" />
			<FaFormItem label="更新时间" :value="state.formData.updatedTime" />
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { reactive, useTemplateRef } from "vue";
import { withDefineType } from "@fast-china/utils";
import { tenantApi } from "@/api/services/Center/tenant";
import type { FaDialogInstance } from "fast-element-plus";
import type { QueryTenantDetailOutput } from "@/api/services/Center/tenant/models/QueryTenantDetailOutput";

defineOptions({
	name: "SystemTenantDetail",
});

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<QueryTenantDetailOutput>({}),
	dialogTitle: "租户",
});

const detail = (tenantId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await tenantApi.queryTenantDetail(tenantId);
		state.formData = apiRes;
		state.dialogTitle = `租户详情 - ${apiRes.tenantName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
});
</script>

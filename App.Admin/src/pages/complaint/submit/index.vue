<template>
	<view class="page">
		<wd-text
			custom-class="pl20 pt20 mb20"
			custom-style="display: block;"
			text="本渠道为平台自有渠道，非官方监管渠道"
			size="var(--wot-font-size-small)"
		/>

		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell-group>
				<wd-form-item title="投诉类型" prop="complaintType" required>
					<wd-radio-group v-model="state.formData.complaintType" direction="horizontal">
						<wd-radio v-for="(item, index) in complaintTypeEnum" :key="index" :value="item.value" :disabled="item.disabled">
							{{ item.label }}
						</wd-radio>
					</wd-radio-group>
				</wd-form-item>

				<wd-form-item title="联系电话" prop="contactPhone" required>
					<wd-input
						v-model="state.formData.contactPhone"
						clearable
						type="number"
						inputmode="numeric"
						:maxlength="11"
						placeholder="请输入联系电话"
					/>
				</wd-form-item>

				<wd-form-item title="联系邮箱" prop="contactEmail" required>
					<wd-input v-model="state.formData.contactEmail" clearable type="text" inputmode="email" placeholder="请输入联系邮箱" />
				</wd-form-item>

				<wd-form-item title="投诉描述" prop="description" required>
					<wd-textarea v-model="state.formData.description" clearable :maxlength="200" placeholder="请简单描述内容" />
				</wd-form-item>
			</wd-cell-group>

			<wd-form-item title="附件图片" prop="attachmentImages" required>
				<wd-upload v-model:file-list="state.fileList" multiple :limit="4" image-mode="aspectFill" :upload-method="handleUpload" />
			</wd-form-item>

			<wd-button custom-class="btn__submit" type="primary" @click="handleSubmit">提交</wd-button>
		</wd-form>
	</view>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { complaintApi } from "@/api/services/Center/complaint";
import { fileApi } from "@/api/services/File";
import { RegExps } from "@/constants";
import { useLoading, useToast } from "@/hooks";
import { useApp } from "@/stores";
import type { FormSchema } from "@wot-ui/ui";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { UploadFile, UploadMethod } from "@wot-ui/ui/components/wd-upload/types";
import type { AddComplaintInput } from "@/api/services/Center/complaint/models/AddComplaintInput";

definePage({
	name: "ComplaintSubmit",
	layout: "layout",
	style: { navigationBarTitleText: "意见反馈" },
});

const appStore = useApp();
const router = useRouter();

const formRef = ref<FormInstance>();

const complaintTypeEnum = appStore.getDictionary("ComplaintTypeEnum");

const state = reactive({
	formData: withDefineType<AddComplaintInput>({}),
	formSchema: withDefineType<FormSchema>({
		validate(formModel: AddComplaintInput) {
			return [
				...(!formModel.complaintType ? [{ path: ["complaintType"], message: "请选择投诉类型" }] : []),
				...(!formModel.contactPhone ? [{ path: ["contactPhone"], message: "请输入联系手机" }] : []),
				...(formModel.contactPhone && !RegExps.Mobile.test(formModel.contactPhone)
					? [{ path: ["contactPhone"], message: "请输入有效的联系手机" }]
					: []),
				...(!formModel.contactEmail ? [{ path: ["contactEmail"], message: "请输入联系邮箱" }] : []),
				...(formModel.contactEmail && !RegExps.Email.test(formModel.contactEmail)
					? [{ path: ["contactEmail"], message: "请输入有效的联系邮箱" }]
					: []),
				...(!formModel.description ? [{ path: ["description"], message: "请输入投诉描述" }] : []),
			];
		},
	}),
	fileList: withDefineType<UploadFile[]>([]),
});

/** 处理上传 */
const handleUpload: UploadMethod = (file, formData, options) => {
	fileApi
		.uploadFile(file.url)
		.then((res) => {
			options.onSuccess({ data: res, statusCode: 200 }, file, formData);
		})
		.catch((err: unknown) => {
			options.onError({ errMsg: (err as { message: string }).message ?? "上传失败" }, file, formData);
		});
};

/** 处理提交 */
const handleSubmit = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	const { complaintType, contactPhone, contactEmail, description } = state.formData;
	if (state.fileList?.length === 0) {
		useToast.warning("请上传附件图片");
		return;
	}
	useLoading.show("提交中...");
	await complaintApi
		.addComplaint({
			complaintType,
			contactPhone,
			contactEmail,
			description,
			attachmentImages: state.fileList.map((m) => m.response as string),
		})
		.finally(() => useLoading.hide());
	useToast.success("提交成功");
	setTimeout(router.back, 2000);
});
</script>

<style scoped lang="scss">
:deep(.wd-textarea__inner) {
	height: 150rpx;
}

.btn__submit {
	display: block;
	margin: 30rpx 50rpx !important;
}
</style>

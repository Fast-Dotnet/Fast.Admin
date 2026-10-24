<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		:title="state.dialogTitle"
		:show-confirm-button="!state.formDisabled"
		:show-before-close="!state.formDisabled"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" :disabled="state.formDisabled" cols="2">
			<FaFormItem prop="appKey" label="应用标识">
				<el-input v-model="state.formData.appKey" maxlength="50" placeholder="请输入应用标识" />
			</FaFormItem>
			<FaFormItem prop="packageType" label="包类型">
				<RadioGroup name="ApplicationPackageTypeEnum" v-model="state.formData.packageType" @change="handlePackageTypeChange" />
			</FaFormItem>
			<FaFormItem prop="versionName" label="版本名称" tips="例如：1.0.1">
				<el-input v-model="state.formData.versionName" maxlength="30" placeholder="请输入版本名称" />
			</FaFormItem>
			<FaFormItem prop="versionCode" label="版本号" tips="使用递增正整数；同一目标版本的整包与 WGT 使用相同版本号和名称">
				<el-input-number v-model="state.formData.versionCode" :min="1" placeholder="请输入版本号" />
			</FaFormItem>
			<template v-if="state.formData.packageType === ApplicationPackageTypeEnum.Wgt">
				<FaFormItem prop="minNativeVersionCode" label="最低版本" tips="与客户端安装包版本号比较，包含边界值">
					<el-input-number v-model="state.formData.minNativeVersionCode" :min="1" placeholder="请输入最低兼容原生版本号" />
				</FaFormItem>
				<FaFormItem prop="maxNativeVersionCode" label="最高版本" tips="不得低于最低兼容原生版本号，包含边界值">
					<el-input-number v-model="state.formData.maxNativeVersionCode" :min="1" placeholder="请输入最高兼容原生版本号" />
				</FaFormItem>
			</template>
			<FaFormItem prop="isForceUpdate" label="强制更新">
				<el-checkbox v-model="state.formData.isForceUpdate">强制更新</el-checkbox>
			</FaFormItem>
			<FaFormItem v-if="state.dialogState !== 'add'" prop="status" label="状态">
				<RadioGroup name="CommonStatusEnum" v-model="state.formData.status" />
			</FaFormItem>
			<FaFormItem prop="fileUrl" label="应用包" span="2" tips="更换包类型后需要重新上传。新增保存后版本即启用。">
				<el-input v-if="state.formDisabled" :model-value="state.formData.fileUrl" type="textarea" :rows="2" />
				<template v-else>
					<FaUpload
						v-model="state.formData.fileUrl"
						:upload-api="handleUpload"
						:before-upload="handleBeforeUpload"
						:accept="packageAccept"
						:max-size="512 * 1024"
					>
						<div class="el-upload__text">将应用包拖到此处，或 <em>点击上传</em></div>
						<template #tip>
							<div class="el-upload__tip">支持 {{ packageAccept }}，单个文件不超过 512 MB</div>
						</template>
					</FaUpload>
					<el-progress v-if="state.uploading" :percentage="state.uploadProgress" indeterminate />
				</template>
			</FaFormItem>
			<FaFormItem prop="updateContent" label="更新说明" span="2">
				<el-input type="textarea" v-model="state.formData.updateContent" :rows="4" maxlength="4000" placeholder="请输入更新说明" />
			</FaFormItem>
			<FaFormItem prop="remark" label="备注">
				<el-input type="textarea" v-model="state.formData.remark" :rows="2" maxlength="200" placeholder="请输入备注" />
			</FaFormItem>
		</FaForm>
	</FaDialog>
</template>

<script setup lang="ts">
import { computed, reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import { fileApi } from "@/api/services/File";
import type { FormRules, UploadRawFile } from "element-plus";
import type { FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { EditApplicationVersionInput } from "@/api/services/Center/applicationVersion/models/EditApplicationVersionInput";

defineOptions({
	name: "SystemApplicationVersionEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

const state = reactive({
	formData: withDefineType<EditApplicationVersionInput>({}),
	formRules: withDefineType<FormRules<EditApplicationVersionInput>>({
		appKey: [{ required: true, message: "请输入应用标识", trigger: "blur" }],
		packageType: [{ required: true, message: "请选择包类型", trigger: "blur" }],
		versionName: [{ required: true, message: "请输入版本名称", trigger: "blur" }],
		versionCode: [{ required: true, type: "integer", min: 1, max: 2147483647, message: "版本号必须是有效的正整数", trigger: "change" }],
		fileUrl: [{ required: true, message: "请上传应用包", trigger: "change" }],
		minNativeVersionCode: [
			{ required: true, type: "integer", min: 1, max: 2147483647, message: "请输入有效的最低兼容原生版本号", trigger: "change" },
		],
		maxNativeVersionCode: [
			{ required: true, type: "integer", min: 1, max: 2147483647, message: "请输入有效的最高兼容原生版本号", trigger: "change" },
			{
				validator: (_rule, value: number, callback) => {
					if (state.formData.minNativeVersionCode != null && value < state.formData.minNativeVersionCode) {
						callback(new Error("最高兼容原生版本号不能低于最低兼容原生版本号"));
						return;
					}
					callback();
				},
				trigger: "change",
			},
		],
	}),
	formDisabled: false,
	dialogState: withDefineType<IPageStateType>("detail"),
	dialogTitle: "应用版本",
	uploading: false,
	uploadProgress: 0,
});

const packageAccept = computed(() => (state.formData.packageType === ApplicationPackageTypeEnum.Wgt ? ".wgt" : ".apk,.app,.hap,.ipa,.exe,.msi"));

const handlePackageTypeChange = () => {
	state.formData.fileUrl = undefined;
	state.formData.minNativeVersionCode = undefined;
	state.formData.maxNativeVersionCode = undefined;
	faFormRef.value?.clearValidate(["fileUrl", "minNativeVersionCode", "maxNativeVersionCode"]);
};

const handleBeforeUpload = (file: UploadRawFile) => {
	if (file.size === 0) {
		ElMessage.warning("上传文件不能为空！");
		return false;
	}
	const suffix = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
	// 浏览器对安装包的 MIME 识别不一致，按服务端合同设置；包内容仍由服务端校验。
	const type =
		suffix === ".apk"
			? "application/vnd.android.package-archive"
			: suffix === ".exe"
				? "application/vnd.microsoft.portable-executable"
				: "application/octet-stream";
	return new File([file], file.name, { type, lastModified: file.lastModified });
};

const handleUpload = async (data: FormData) => {
	state.uploading = true;
	state.uploadProgress = 0;
	try {
		return await fileApi.uploadPackage(data, ({ loaded, total }) => {
			if (total) state.uploadProgress = Math.min(100, Math.round((loaded / total) * 100));
		});
	} finally {
		state.uploading = false;
	}
};

const handleConfirm = () => {
	if (state.uploading) return;
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		const formData = { ...state.formData };
		// 整包不提交原生兼容区间，避免历史数据或切换包类型时携带残留值。
		if (formData.packageType === ApplicationPackageTypeEnum.FullPackage) {
			formData.minNativeVersionCode = undefined;
			formData.maxNativeVersionCode = undefined;
		}
		if (state.dialogState === "add") {
			await applicationVersionApi.addApplicationVersion(formData);
			ElMessage.success("新增成功！");
		} else {
			await applicationVersionApi.editApplicationVersion(formData);
			ElMessage.success("保存成功！");
		}
		emit("ok");
	});
};

const detail = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.formDisabled = true;
		const apiRes = await applicationVersionApi.queryApplicationVersionDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `应用版本详情 - ${apiRes.versionName}`;
	});
};

const add = () => {
	faDialogRef.value.open(() => {
		state.dialogState = "add";
		state.dialogTitle = "添加应用版本";
		state.formDisabled = false;
		state.uploadProgress = 0;
		state.formData = {
			packageType: ApplicationPackageTypeEnum.FullPackage,
			isForceUpdate: false,
		};
	});
};

const edit = (recordId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		state.formDisabled = false;
		state.uploadProgress = 0;
		const apiRes = await applicationVersionApi.queryApplicationVersionDetail(recordId);
		state.formData = apiRes;
		state.dialogTitle = `编辑应用版本 - ${apiRes.versionName}`;
	});
};

defineExpose({
	element: faDialogRef,
	detail,
	add,
	edit,
});
</script>

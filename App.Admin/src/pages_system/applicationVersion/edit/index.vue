<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<FaApplicationClientSelect v-model="state.formData.appKey" prop="appKey" />
			<wd-form-item title="包类型" prop="packageType">
				<FaRadioGroup
					name="ApplicationPackageTypeEnum"
					v-model="state.formData.packageType"
					direction="horizontal"
					@change="handlePackageTypeChange"
				/>
			</wd-form-item>
			<wd-form-item title="应用类型" prop="appType">
				<wd-radio-group v-if="state.formData.packageType === ApplicationPackageTypeEnum.FullPackage" v-model="state.formData.appType">
					<wd-radio :value="AppEnvironmentEnum.Windows">Windows</wd-radio>
					<wd-radio :value="AppEnvironmentEnum.Android">Android</wd-radio>
					<wd-radio :value="AppEnvironmentEnum.HarmonyOS">鸿蒙</wd-radio>
					<wd-radio :value="AppEnvironmentEnum.IOS">IOS</wd-radio>
				</wd-radio-group>
				<wd-checkbox-group v-else v-model="appTypes" type="square">
					<wd-checkbox :name="AppEnvironmentEnum.Windows">Windows</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.Android">Android</wd-checkbox>
					<wd-checkbox :name="AppEnvironmentEnum.IOS">IOS</wd-checkbox>
				</wd-checkbox-group>
			</wd-form-item>
			<wd-form-item title="版本名称" prop="versionName">
				<wd-input v-model="state.formData.versionName" clearable :maxlength="30" placeholder="请输入版本名称" />
			</wd-form-item>
			<wd-form-item title="版本号" prop="versionCode">
				<wd-input v-model.number="state.formData.versionCode" type="number" placeholder="请输入版本号" />
			</wd-form-item>
			<template v-if="state.formData.packageType === ApplicationPackageTypeEnum.Wgt">
				<wd-form-item title="最低版本" prop="minNativeVersionCode">
					<wd-input v-model.number="state.formData.minNativeVersionCode" type="number" placeholder="请输入最低兼容原生版本号" />
				</wd-form-item>
				<wd-form-item title="最高版本" prop="maxNativeVersionCode">
					<wd-input v-model.number="state.formData.maxNativeVersionCode" type="number" placeholder="请输入最高兼容原生版本号" />
				</wd-form-item>
			</template>
			<wd-form-item title="强制更新" prop="isForceUpdate">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.isForceUpdate" direction="horizontal" />
			</wd-form-item>
			<wd-form-item v-if="state.pageMode !== 'add'" title="状态" prop="status">
				<FaRadioGroup name="CommonStatusEnum" v-model="state.formData.status" direction="horizontal" />
			</wd-form-item>

			<wd-form-item
				title="应用包"
				prop="fileUrl"
				:is-link="!state.uploading"
				:value="state.uploading ? undefined : state.fileName"
				:placeholder="state.uploading ? `上传中... ${state.uploadProgress}%` : '选择文件并上传'"
				@click="handleUpload"
			/>
			<wd-form-item title="更新说明" prop="updateContent">
				<wd-textarea
					custom-style="--fa-textarea-height: 150px"
					v-model="state.formData.updateContent"
					clearable
					:maxlength="4000"
					placeholder="请输入更新说明"
				/>
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.remark"
					clearable
					:maxlength="200"
					placeholder="请输入备注"
				/>
			</wd-form-item>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { axiosUtil } from "@fast-china/axios";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { AppEnvironmentEnum } from "@/api/enums/AppEnvironmentEnum";
import { ApplicationPackageTypeEnum } from "@/api/enums/ApplicationPackageTypeEnum";
import { applicationVersionApi } from "@/api/services/Center/applicationVersion";
import { fileApi } from "@/api/services/File";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { useApp, usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { EditApplicationVersionInput } from "@/api/services/Center/applicationVersion/models/EditApplicationVersionInput";

definePage({
	name: "SystemApplicationVersionEdit",
	authCode: ["AppVersion:Add", "AppVersion:Edit"],
	style: {
		navigationBarTitleText: "应用版本",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();
const appStore = useApp();

const formRef = ref<FormInstance>();

const appEnvironmentEnum = appStore.getDictionary("AppEnvironmentEnum").filter((item) => {
	const flag = BigInt(item.value);
	return (flag & (flag - 1n)) === 0n;
});

const state = reactive({
	loading: false,
	formData: withDefineType<EditApplicationVersionInput>({}),
	formSchema: useFormSchema<EditApplicationVersionInput>({
		appKey: { required: true, message: "请选择应用标识" },
		appType: { required: true, message: "请选择应用类型" },
		packageType: { required: true, message: "请选择包类型" },
		versionName: { required: true, message: "请输入版本名称" },
		versionCode: {
			required: true,
			validator: (value) => {
				return typeof value === "number" && Number.isInteger(value) && value > 0 && value <= 2147483647
					? undefined
					: "版本号必须是有效的正整数";
			},
		},
		minNativeVersionCode: {
			validator: (value, formData) => {
				if (formData.packageType !== ApplicationPackageTypeEnum.Wgt) return undefined;
				return typeof value === "number" && Number.isInteger(value) && value > 0 && value <= 2147483647
					? undefined
					: "请输入有效的最低兼容原生版本号";
			},
		},
		maxNativeVersionCode: {
			validator: (value, formData) => {
				if (formData.packageType !== ApplicationPackageTypeEnum.Wgt) return undefined;
				if (typeof value !== "number" || !Number.isInteger(value) || value <= 0 || value > 2147483647) {
					return "请输入有效的最高兼容原生版本号";
				}
				if (formData.minNativeVersionCode != null && value < formData.minNativeVersionCode) {
					return "最高兼容原生版本号不能低于最低兼容原生版本号";
				}
				return undefined;
			},
		},
		fileUrl: { required: true, message: "请上传应用包" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	recordId: withDefineType<string>(),
	uploading: false,
	uploadProgress: 0,
	fileName: "",
});

const appTypes = computed({
	get: () => {
		const value = BigInt(state.formData.appType || 0n);
		return appEnvironmentEnum.filter((item) => (value & BigInt(item.value)) !== 0n).map((item) => String(item.value));
	},
	set: (values: string[]) => {
		state.formData.appType = values.length
			? (values.reduce((value, item) => value | BigInt(item), 0n).toString() as AppEnvironmentEnum)
			: undefined;
	},
});
const packageExtensionList = computed(() =>
	state.formData.packageType === ApplicationPackageTypeEnum.Wgt ? [".wgt"] : [".apk", ".app", ".hap", ".ipa", ".exe", ".msi"]
);

const handlePackageTypeChange = () => {
	state.formData.appType = undefined;
	state.formData.fileUrl = undefined;
	state.formData.minNativeVersionCode = undefined;
	state.formData.maxNativeVersionCode = undefined;
	state.fileName = "";
	state.uploadProgress = 0;
	formRef.value.reset();
};

const handleUpload = throttle(async () => {
	state.uploading = true;
	state.uploadProgress = 0;
	try {
		const resFile = await new Promise<{ path: string; name: string; size: number; file?: File }>((resolve, reject) => {
			// #ifdef MP-WEIXIN
			wx.chooseMessageFile({
				count: 1,
				type: "file",
				extension: packageExtensionList.value.map((extension) => extension.slice(1)),
				success({ tempFiles }) {
					const tempFile = tempFiles[0];
					resolve({ path: tempFile.path, name: tempFile.name, size: tempFile.size });
				},
				fail(error) {
					reject(new Error(error.errMsg));
				},
			});
			// #endif

			// #ifndef MP-WEIXIN
			uni.chooseFile({
				count: 1,
				type: "all",
				extension: packageExtensionList.value,
				success({ tempFilePaths, tempFiles }) {
					const tempFile = Array.isArray(tempFiles) ? tempFiles[0] : tempFiles;
					const path = Array.isArray(tempFilePaths) ? tempFilePaths[0] : tempFilePaths;
					if (!tempFile || !path) {
						resolve(undefined);
						return;
					}
					const name = "name" in tempFile ? tempFile.name : path.split("/").at(-1);
					const file = typeof File !== "undefined" && tempFile instanceof File ? tempFile : undefined;
					resolve({ path, name: name ?? "", size: tempFile.size, file });
				},
				fail(error) {
					if (typeof error === "object" && error !== null && "errMsg" in error && typeof error.errMsg === "string") {
						if (error.errMsg !== "chooseFile:fail cancel") {
							reject(new Error(error.errMsg));
						} else {
							resolve(undefined);
						}
					} else {
						reject(new Error("文件选择失败！"));
					}
				},
			});
			// #endif
		});
		if (!resFile) return;
		const suffix = resFile.name.match(/\.[^.]+$/)?.[0].toLowerCase();
		if (!suffix || !packageExtensionList.value.includes(suffix)) {
			useToast.warning(`请选择 ${packageExtensionList.value.join("，")} 文件`);
			return;
		}
		if (!resFile.size || resFile.size > 512 * 1024 * 1024) {
			useToast.warning("应用包不能为空，且不能超过 512 MB");
			return;
		}

		let fileUrl: string;

		// #ifdef H5
		// 浏览器对安装包的 MIME 识别不一致，按服务端合同设置；包内容仍由服务端校验。
		const type =
			suffix === ".apk"
				? "application/vnd.android.package-archive"
				: suffix === ".exe"
					? "application/vnd.microsoft.portable-executable"
					: "application/octet-stream";
		const uploadFile = new File([resFile.file], resFile.name, { type, lastModified: resFile.file.lastModified });
		fileUrl = await axiosUtil.request<string>({
			url: "/file/uploadPackage",
			method: "upload",
			name: "file",
			filePath: resFile.path,
			file: uploadFile,
			onUploadProgress: ({ loaded, total }) => {
				if (total) state.uploadProgress = Math.min(100, Math.round((loaded / total) * 100));
			},
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
		// #endif

		// #ifndef H5
		fileUrl = await fileApi.uploadPackage(resFile.path, null, ({ loaded, total }) => {
			if (total) state.uploadProgress = Math.min(100, Math.round((loaded / total) * 100));
		});
		// #endif

		if (!fileUrl) {
			useToast.warning("未获取到应用包地址");
			return;
		}
		state.formData.fileUrl = fileUrl;
		state.fileName = resFile.name;
		useToast.success("上传成功！");
	} finally {
		state.uploading = false;
	}
});

const handleConfirm = throttle(async () => {
	if (state.uploading) return;
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		const formData = { ...state.formData };
		// 整包不提交原生兼容区间，避免历史数据或切换包类型时携带残留值。
		if (formData.packageType === ApplicationPackageTypeEnum.FullPackage) {
			formData.minNativeVersionCode = undefined;
			formData.maxNativeVersionCode = undefined;
		}
		switch (state.pageMode) {
			case "add":
				await applicationVersionApi.addApplicationVersion(formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await applicationVersionApi.editApplicationVersion(formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("ApplicationVersion");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/applicationVersion/page/index");
});

const handleLoad = async () => {
	if (state.uploading) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加应用版本" });
				state.formData = {
					packageType: ApplicationPackageTypeEnum.FullPackage,
					isForceUpdate: false,
				};
				break;
			case "edit":
				{
					const apiRes = await applicationVersionApi.queryApplicationVersionDetail(state.recordId);
					state.formData = apiRes;
					state.fileName = apiRes.fileUrl.split("/").at(-1);
					uni.setNavigationBarTitle({ title: `编辑应用版本 - ${apiRes.versionName}` });
				}
				break;
		}
		state.uploadProgress = 0;
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; recordId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.recordId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.recordId = options.recordId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

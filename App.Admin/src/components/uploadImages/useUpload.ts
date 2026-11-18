import { type ComputedRef, type Ref, computed, onMounted, useModel, watch } from "vue";
import { randomInt } from "@fast-china/utils";
import { CommonUtil, useFormDisabled } from "@wot-ui/ui";
import Decimal from "decimal.js";
import type {
	UploadChangeEvent,
	UploadFile,
	UploadFileItem,
	UploadFormData,
	UploadMethod,
	UploadOversizeEvent,
	UploadProps,
} from "@wot-ui/ui/components/wd-upload/types";
import type { AxiosProgressEvent } from "axios";

interface UploadComposable {
	fileList: Ref<UploadFile[]>;
	formDisabled: ComputedRef<boolean>;
	maxSizeByte: ComputedRef<number>;
	handleUpload: (file: UploadFileItem, formData: UploadFormData, options: Parameters<UploadMethod>[2]) => Promise<void>;
	handleOnOversize: (event: UploadOversizeEvent) => void;
	handleOnChange: (event: UploadChangeEvent) => void;
}

/**
 * 创建单文件和多文件上传组件共用的状态与操作方法。
 *
 * @typeParam T - 单文件资源地址或多文件资源地址数组
 * @param componentName - 用于日志提示的组件名称
 * @param fileTypeName - 用于提示文案的文件类型名称
 * @param props - 上传组件的 Props，包含文件列表及模型值
 * @param emit - 模型更新、超限和列表变化事件的触发器
 * @param data - 上传接口、直传地址及大小配置，可用 getter 保持响应性
 * @returns 文件列表、禁用状态、大小上限及上传事件处理方法
 */
export const useUpload = <T extends string | string[]>(
	componentName: string,
	fileTypeName: string,
	props: Partial<Omit<UploadProps, "maxSize">> & {
		maxSize?: string | number;
		modelValue?: T | null;
	},
	emit: ((event: "update:modelValue", value: T | null) => void) &
		((event: "oversize", value: UploadOversizeEvent) => void) &
		((event: "change", value: UploadChangeEvent) => void),
	data?: {
		multiple?: boolean;
		maxSize?: string | number;
		uploadApi?: (
			filePath: string,
			formData?: Record<string, unknown>,
			onUploadProgress?: (progressEvent: AxiosProgressEvent) => void
		) => Promise<string>;
		uploadUrl?: string;
	}
): UploadComposable => {
	const fileList = useModel(props, "fileList");
	const formDisabled = useFormDisabled(props);
	const statusKey = computed(() => props.statusKey ?? "status");

	const maxSizeKB = computed(() => new Decimal(typeof data?.maxSize === "number" ? data?.maxSize : Number(data?.maxSize)));
	const maxSizeByte = computed(() => maxSizeKB.value.mul(1024).toNumber());
	const maxSizeMB = computed(() => maxSizeKB.value.div(1024));

	onMounted(() => {
		if (!formDisabled.value && props.autoUpload && !data?.uploadApi && !data?.uploadUrl) {
			console.error(`[Fast:${componentName}]`, "uploadApi 和 action 至少需要提供一个。");
		}
	});

	const getUploadedUrls = (files: UploadFile[]): string[] =>
		files.flatMap((file) => (file.url && (!file[statusKey.value] || file[statusKey.value] === "success") ? [file.url] : []));

	const handleOnChange = (event: UploadChangeEvent) => {
		let value: T | null;
		const { fileList } = event;
		if (fileList.length > 0) {
			if (props.multiple === true) {
				value = getUploadedUrls(fileList) as T;
			} else {
				const fileUrl = fileList[0]?.url;
				if (!fileUrl) return;
				value = fileUrl as T;
			}
		} else {
			value = props.multiple === true ? ([] as unknown as T) : null;
		}
		emit("update:modelValue", value);
		emit("change", event);
	};

	const getPreviewName = (url: string) => {
		const path = url.split(/[?#]/u)[0] ?? "";
		const name = path.slice(path.lastIndexOf("/") + 1);
		if (CommonUtil.isImageUrl(name) || CommonUtil.isVideoUrl(name)) return name;
		// 无扩展名的地址在单一媒体类型下也要能使用 Wot 原生预览。
		if (props.accept === "image") return "image.image";
		if (props.accept === "video") return "video.video";
		return name;
	};

	const handleUpload = async (file: UploadFileItem, formData: UploadFormData, options: Parameters<UploadMethod>[2]) => {
		if (!data?.uploadApi && !data?.uploadUrl) {
			const error = new Error(`上传${fileTypeName}接口 uploadApi 或地址 action 不能为空。`);
			uni.showToast({ title: `上传${fileTypeName}Api或地址不能为空`, icon: "error" });
			console.error(`[Fast:${componentName}]`, error.message);
			throw error;
		}

		let fileUrl: string;
		try {
			if (data.uploadApi) {
				fileUrl = await data.uploadApi(file.url, formData, (progress) =>
					options.onProgress(
						{
							progress: Math.round((progress.progress ?? 0) * 100),
							totalBytesSent: progress.loaded,
							totalBytesExpectedToSend: progress.total ?? 0,
						},
						file
					)
				);
			} else {
				formData = { ...formData, fileName: formData.fileName ?? file.name ?? options.fileName };
				fileUrl = await new Promise((resolve, reject) => {
					const task = uni.uploadFile({
						url: data.uploadUrl ?? options.action,
						filePath: file.url,
						name: options.name || "file",
						fileType: options.fileType,
						header: options.header,
						formData,
						success(response) {
							const statusCodes = Array.isArray(options.statusCode) ? options.statusCode : [options.statusCode];
							if (statusCodes.includes(response.statusCode)) {
								resolve(response.data);
							} else {
								reject(new Error(`文件上传失败（HTTP ${response.statusCode.toString()}）`));
							}
						},
						fail: ({ errMsg }) => reject(new Error(errMsg)),
					});
					task.onProgressUpdate((progress) => options.onProgress(progress, file));
				});
			}
		} catch (error) {
			console.error(`[Fast:${componentName}]`, "文件上传失败。", error);
			const message = error instanceof Error ? error.message : "文件上传失败！";
			options.onError({ errMsg: message }, file, formData);
			return;
		}
		const originalName = file.name || file.url;
		file.url = fileUrl;
		if (!CommonUtil.isImageUrl(file.url) && !CommonUtil.isVideoUrl(file.url)) {
			file.name = getPreviewName(originalName);
		}
		options.onSuccess({ data: file.url, statusCode: 200 }, file, formData);
	};

	const handleOnOversize = (event: UploadOversizeEvent) => {
		console.warn(`[Fast:${componentName}]`, `${fileTypeName}上传大小不能超过 ${maxSizeMB.value.toString()}MB。`);
		uni.showToast({ title: `${fileTypeName}上传大小不能超过 ${maxSizeMB.value.toString()}MB`, icon: "none" });
		emit("oversize", event);
	};

	watch(
		() => props.modelValue,
		(newValue) => {
			if (newValue) {
				if (Array.isArray(newValue)) {
					const uploadedUrls = getUploadedUrls(fileList.value);
					// 组件回写已完成的 URL 时，保留文件顺序和仍在上传的卡片。
					if (newValue.length === uploadedUrls.length && newValue.every((url, index) => url === uploadedUrls[index])) return;
					fileList.value = newValue.map((m) => {
						const find = fileList.value.find((f) => f.url === m);
						return (
							find ?? {
								name: getPreviewName(m),
								[props.statusKey ?? "status"]: "success",
								uid: randomInt(1000, 10000),
								url: m,
							}
						);
					});
				} else {
					const find = fileList.value.find((f) => f.url === newValue);
					fileList.value = [
						find ?? {
							name: getPreviewName(newValue),
							[props.statusKey ?? "status"]: "success",
							uid: randomInt(1000, 10000),
							url: newValue,
						},
					];
				}
			} else {
				fileList.value = [];
			}
		},
		{
			immediate: true,
		}
	);

	return {
		fileList,
		formDisabled,
		maxSizeByte,
		handleUpload,
		handleOnOversize,
		handleOnChange,
	};
};

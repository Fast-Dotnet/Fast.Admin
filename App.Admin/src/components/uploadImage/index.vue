<template>
	<wd-upload
		v-bind="wdUploadProps"
		ref="uploadRef"
		:custom-class="`fa-upload-image ${props.customClass}`"
		:custom-style="`${uploadStyle} ${props.customStyle}`"
		v-model:file-list="fileList"
		:multiple="false"
		:limit="1"
		:disabled="disabled"
		:max-size="maxSizeByte"
		:upload-method="handleUpload"
		@oversize="handleOnOversize"
		@change="handleOnChange"
		@success="emit('success', $event)"
		@fail="emit('fail', $event)"
		@progress="emit('progress', $event)"
		@chooseerror="emit('chooseerror', $event)"
		@remove="emit('remove', $event)"
	>
		<template v-if="$slots.default" #default>
			<slot />
		</template>
		<template v-if="$slots['preview-cover']" #preview-cover="{ file, index }">
			<slot name="preview-cover" :file="file" :index="index" />
		</template>
	</wd-upload>
</template>

<script setup lang="ts">
import { type PropType, computed, ref } from "vue";
import { addCssUnit, useProps } from "@fast-china/utils";
import { uploadProps } from "@wot-ui/ui/components/wd-upload/types";
import { useUpload } from "../uploadImages/useUpload";
import type {
	UploadChangeEvent,
	UploadErrorEvent,
	UploadFile,
	UploadFileType,
	UploadOversizeEvent,
	UploadProgressEvent,
	UploadRemoveEvent,
	UploadSourceType,
	UploadSuccessEvent,
} from "@wot-ui/ui/components/wd-upload/types";
import type WdUpload from "@wot-ui/ui/components/wd-upload/wd-upload.vue";
import type { AxiosProgressEvent } from "axios";

defineOptions({
	name: "UploadImage",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...uploadProps,
	/** 允许选择的文件类型，默认 image */
	accept: {
		type: String as PropType<UploadFileType>,
		default: "image",
	},
	/** 固定为单文件选择，传 true 不生效 */
	multiple: {
		type: Boolean,
		default: false,
		validator: (value: boolean) => {
			if (value) {
				console.warn("[Fast:FaUploadImage]", "'multiple' 属性固定为 false，外部设置不会生效。");
				return false;
			}
			return true;
		},
	},
	/** 单个文件的资源地址；移除后回写 null */
	modelValue: String as PropType<string | null>,
	/** 单个文件大小上限，单位 KB，1 KB 按 1024 字节计算 */
	maxSize: {
		type: [String, Number],
		default: 2048,
	},
	/** 文件来源，默认仅从相册选择 */
	sourceType: {
		type: Array as PropType<UploadSourceType[]>,
		default: () => ["album"],
	},
	/** 自定义上传函数，返回资源地址字符串；优先于 action，第三个参数用于上报上传进度 */
	uploadApi: {
		type: Function as PropType<
			(filePath: string, formData?: Record<string, unknown>, onUploadProgress?: (event: AxiosProgressEvent) => void) => Promise<string>
		>,
	},
	/** 上传项的展示尺寸，数字按 px 处理 */
	size: [String, Number],
});

const wdUploadProps = useProps(props, uploadProps, [
	"customClass",
	"customStyle",
	"fileList",
	"multiple",
	"limit",
	"maxSize",
	"disabled",
	"uploadMethod",
]);

const emit = defineEmits<{
	/** 资源地址变化时回写，移除文件后为 null */
	"update:modelValue": [value: string | null];
	/** 同步 Wot 上传文件列表，包含上传状态和进度 */
	"update:fileList": [value: UploadFile[]];
	/** 底层上传成功时触发，响应 data 为资源地址字符串 */
	success: [event: UploadSuccessEvent];
	/** 上传失败时触发 */
	fail: [event: UploadErrorEvent];
	/** 底层上传进度变化时触发，沿用 Wot 事件参数 */
	progress: [event: UploadProgressEvent];
	/** 文件大小超过限制时触发 */
	oversize: [event: UploadOversizeEvent];
	/** 选择文件失败时触发 */
	chooseerror: [event: { error: unknown }];
	/** 文件列表变化后触发，同时同步资源地址模型 */
	change: [event: UploadChangeEvent];
	/** 移除文件时触发 */
	remove: [event: UploadRemoveEvent];
}>();

const { fileList, formDisabled, maxSizeByte, handleUpload, handleOnOversize, handleOnChange } = useUpload<string>(
	"FaUploadImage",
	"图片",
	props,
	emit,
	{
		get maxSize() {
			return props.maxSize;
		},
		get uploadApi() {
			return props.uploadApi;
		},
		get uploadUrl() {
			return props.action;
		},
	}
);

const uploadRef = ref<InstanceType<typeof WdUpload>>();

const uploadStyle = computed(() => (props.size == null ? "" : `--wot-upload-size: ${addCssUnit(props.size)};`));

const disabled = computed(() => {
	return props.disabled === true || formDisabled.value;
});

defineExpose({
	/** 调用底层 wd-upload 手动提交待上传文件，适用于 autoUpload=false。 */
	submit: computed(() => uploadRef.value?.submit),
	/** 调用底层 wd-upload 中止上传；自定义上传方式的中止能力取决于其具体实现。 */
	abort: computed(() => uploadRef.value?.abort),
});
</script>

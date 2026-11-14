<template>
	<wd-upload
		v-bind="wdUploadProps"
		ref="uploadRef"
		:custom-class="`fa-upload-images ${props.customClass}`"
		:custom-style="`${uploadStyle} ${props.customStyle}`"
		v-model:file-list="fileList"
		multiple
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
import { useUpload } from "./useUpload";
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
	name: "UploadImages",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...uploadProps,
	accept: {
		type: String as PropType<UploadFileType>,
		default: "image",
	},
	multiple: {
		type: Boolean,
		default: true,
		validator: (value: boolean) => {
			if (!value) {
				console.warn("[Fast:FaUploadImages]", "'multiple' 属性固定为 true，外部设置不会生效。");
				return false;
			}
			return true;
		},
	},
	limit: {
		type: Number,
		default: 9,
	},
	/** v-model 绑定值 */
	modelValue: Array as PropType<string[]>,
	/** 大小限制，单位kb */
	maxSize: {
		type: [String, Number],
		default: 2048,
	},
	sourceType: {
		type: Array as PropType<UploadSourceType[]>,
		default: () => ["album"],
	},
	/** 文件上传接口 */
	uploadApi: {
		type: Function as PropType<
			(filePath: string, formData?: Record<string, unknown>, onUploadProgress?: (event: AxiosProgressEvent) => void) => Promise<string>
		>,
	},
	size: [String, Number],
});

const wdUploadProps = useProps(props, uploadProps, ["customClass", "customStyle", "fileList", "multiple", "maxSize", "disabled", "uploadMethod"]);

const emit = defineEmits<{
	/** v-model 回调 */
	"update:modelValue": [value: string[] | null];
	/** v-model:fileList 回调 */
	"update:fileList": [value: UploadFile[]];
	/** 上传成功时触发 */
	success: [event: UploadSuccessEvent];
	/** 上传失败时触发 */
	fail: [event: UploadErrorEvent];
	/** 上传进度变化时触发 */
	progress: [event: UploadProgressEvent];
	/** 文件大小超过限制时触发 */
	oversize: [event: UploadOversizeEvent];
	/** 选择文件失败时触发 */
	chooseerror: [event: { error: unknown }];
	/** 上传列表变化时触发 */
	change: [event: UploadChangeEvent];
	/** 移除文件时触发 */
	remove: [event: UploadRemoveEvent];
}>();

const { fileList, formDisabled, maxSizeByte, handleUpload, handleOnOversize, handleOnChange } = useUpload<string[]>(
	"FaUploadImages",
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
	submit: computed(() => uploadRef.value?.submit),
	abort: computed(() => uploadRef.value?.abort),
});
</script>

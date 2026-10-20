<template>
	<div class="wang-editor">
		<Toolbar class="wang-editor__toolbar" :editor="editorRef" :mode="props.mode" />
		<Editor
			class="wang-editor__warp"
			:style="{
				'--height': addCssUnit(props.height),
			}"
			:default-config="{
				placeholder: props.placeholder,
				readOnly: props.readOnly,
				MENU_CONF: {
					uploadImage: {
						// 单个文件的最大体积限制
						maxFileSize: 5 * 1024 * 1024,
						// 小于该值就插入 base64 格式（而不上传）
						base64LimitSize: 5 * 1024,
						customUpload: handleUploadImage,
					},
					uploadVideo: {
						// 单个文件的最大体积限制
						maxFileSize: 10 * 1024 * 1024,
						customUpload: handleUploadVideo,
					},
				},
			}"
			:mode="props.mode"
			v-model="modelValue"
			@on-created="handleCreated"
			@custom-alert="handleCustomAlert"
		/>
	</div>
</template>

<script lang="ts" setup>
import { inject, onBeforeUnmount, shallowRef, watch } from "vue";
import { ElMessage, formContextKey } from "element-plus";
import { addCssUnit, definePropType, logger } from "@fast-china/utils";
import { Editor, Toolbar } from "@wangeditor-next/editor-for-vue";
import { fileApi } from "@/api/services/File";
import type { IDomEditor } from "@wangeditor-next/editor";
import "@wangeditor-next/editor/dist/css/style.css";

defineOptions({
	name: "Editor",
});

const props = defineProps({
	/** 编辑器工具栏模式 */
	mode: {
		type: definePropType<"default" | "simple">(String),
		default: "default",
	},
	/** 编辑区占位文本 */
	placeholder: String,
	/** 是否使用只读模式 */
	readOnly: Boolean,
	/** 是否禁用编辑器 */
	disabled: Boolean,
	/** 编辑区高度 */
	height: {
		type: [String, Number],
		default: 300,
	},
});

/** 编辑器 HTML 内容 */
const modelValue = defineModel<string>();

// 编辑器实例；使用 shallowRef 避免代理第三方编辑器对象
const editorRef = shallowRef<IDomEditor | null>(null);

// 所属 Element Plus 表单上下文
const formContext = inject(formContextKey, undefined);

/** WangEditor 自定义上传成功后的资源插入函数 */
type InsertFnType = (url: string, alt: string, href: string) => void;

/** 上传图片并将返回的资源地址插入编辑器。 */
const handleUploadImage = async (file: File, insertFn: InsertFnType) => {
	if (file.size > 5 * 1024 * 1024) {
		ElMessage.error(`文件过大，最大允许 5MB`);
		return;
	}
	try {
		const formData = new FormData();
		formData.append("file", file);
		const apiRes = await fileApi.uploadEditor(formData);
		insertFn(apiRes, "", "");
	} catch (error) {
		logger.error("Editor", "图片上传失败", error);
		ElMessage.error("图片上传失败");
	}
};

/** 上传视频并将返回的资源地址插入编辑器。 */
const handleUploadVideo = async (file: File, insertFn: InsertFnType) => {
	if (file.size > 10 * 1024 * 1024) {
		ElMessage.error(`文件过大，最大允许 10MB`);
		return;
	}
	try {
		const formData = new FormData();
		formData.append("file", file);
		const apiRes = await fileApi.uploadEditor(formData);
		insertFn(apiRes, "", "");
	} catch (error) {
		logger.error("Editor", "视频上传失败", error);
		ElMessage.error("视频上传失败");
	}
};

/** 保存 WangEditor 创建完成后的实例。 */
const handleCreated = (editor: IDomEditor) => {
	editorRef.value = editor;
};

/** 将 WangEditor 的自定义提示映射为 Element Plus 消息。 */
const handleCustomAlert = (info: string, type: string) => {
	switch (type) {
		case "success":
			ElMessage.success(info);
			break;
		case "warning":
			ElMessage.warning(info);
			break;
		case "error":
			ElMessage.error(info);
			break;
		case "info":
		default:
			ElMessage.info(info);
			break;
	}
};

/** 在当前光标位置插入文本。 */
const insertText = (text: string) => {
	const editor = editorRef.value;
	if (editor == null) return;
	editor.insertText(text);
};

// 跟随所属表单的禁用状态切换编辑器可用性。
watch(
	() => formContext?.disabled,
	(newValue) => {
		if (newValue) editorRef.value?.disable();
		else editorRef.value?.enable();
	}
);

// 跟随组件 disabled 属性切换编辑器可用性。
watch(
	() => props.disabled,
	(newValue) => {
		if (newValue) editorRef.value?.disable();
		else editorRef.value?.enable();
	}
);

// 组件卸载时销毁第三方编辑器实例。
onBeforeUnmount(() => {
	const editor = editorRef.value;
	if (editor == null) return;
	editor.destroy();
});

defineExpose({
	/** 编辑器实例 */
	editorRef,
	/** 在当前光标位置插入文本；编辑器未就绪时不执行操作。 */
	insertText,
});
</script>

<style lang="scss" scoped>
.wang-editor {
	/* 编辑区背景 */
	--w-e-textarea-bg-color: var(--el-bg-color);
	/* 编辑区文字 */
	--w-e-textarea-color: var(--el-text-color-primary);
	/* 边框 */
	--w-e-textarea-border-color: var(--el-border-color);
	/* 弱边框 */
	--w-e-textarea-slight-border-color: var(--el-border-color-light);
	/* 次要文字 */
	--w-e-textarea-slight-color: var(--el-text-color-secondary);
	/* 次要背景 */
	--w-e-textarea-slight-bg-color: var(--el-bg-color-page);
	/* 选中边框 */
	--w-e-textarea-selected-border-color: var(--el-color-primary);
	/* 拖拽点 */
	--w-e-textarea-handler-bg-color: var(--el-color-primary);
	/* 工具栏文字 */
	--w-e-toolbar-color: var(--el-text-color-regular);
	/* 工具栏背景 */
	--w-e-toolbar-bg-color: var(--el-bg-color);
	/* 激活文字 */
	--w-e-toolbar-active-color: var(--el-text-color-primary);
	/* 激活背景 */
	--w-e-toolbar-active-bg-color: var(--el-color-primary-light-9);
	/* 禁用文字 */
	--w-e-toolbar-disabled-color: var(--el-text-color-disabled);
	/* 工具栏边框 */
	--w-e-toolbar-border-color: var(--el-border-color);
	/* 弹窗按钮背景 */
	--w-e-modal-button-bg-color: var(--el-fill-color-light);
	/* 弹窗按钮边框 */
	--w-e-modal-button-border-color: var(--el-border-color-light);

	border: var(--el-border);
}

.wang-editor__toolbar {
	border-bottom: var(--el-border);
}

.wang-editor__warp {
	overflow-y: hidden;
	height: var(--height) !important;
	min-height: 100px;
}
</style>

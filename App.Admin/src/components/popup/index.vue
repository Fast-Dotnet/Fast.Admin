<template>
	<wd-popup
		v-bind="wdPopupProps"
		:custom-class="`fa-popup ${props.customClass}`"
		:custom-style="`${state.style} ${props.customStyle}`"
		v-model="state.visible"
		:transition="state.transition"
		safe-area-inset-bottom
		@before-enter="() => emit('beforeEnter')"
		@enter="() => emit('enter')"
		@after-enter="() => emit('afterEnter')"
		@before-leave="() => emit('beforeLeave')"
		@leave="() => emit('leave')"
		@after-leave="() => emit('afterLeave')"
		@click-modal="() => emit('clickModal')"
		@close="handleClose"
	>
		<slot />
		<FaLoading mask :loading="state.loading" />
	</wd-popup>
</template>

<script setup lang="ts">
import { computed, nextTick, reactive, toRef } from "vue";
import { addCssUnit, callOptionalFunction, definePropType, useProps } from "@fast-china/utils";
import { popupProps } from "@wot-ui/ui/components/wd-popup/types";
import FaLoading from "../loading/index.vue";

defineOptions({
	name: "Popup",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...popupProps,
	/** 弹窗宽度，数字按 px 处理 */
	width: [String, Number],
	/** 弹窗高度，数字按 px 处理 */
	height: [String, Number],
	/** 保留的关闭配置；当前实现未读取此属性 */
	showBeforeClose: Boolean,
	/** 弹窗显示后的初始化回调，优先于 open 方法传入的回调 */
	afterOpen: {
		type: definePropType<() => void>(Function),
	},
});

const wdPopupProps = useProps(props, popupProps);

const emit = defineEmits<{
	/** 进入前触发 */
	beforeEnter: [];
	/** 进入时触发 */
	enter: [];
	/** 进入后触发 */
	afterEnter: [];
	/** 离开前触发 */
	beforeLeave: [];
	/** 离开时触发 */
	leave: [];
	/** 离开后触发 */
	afterLeave: [];
	/** 点击遮罩时触发 */
	clickModal: [];
	/** 弹出层打开时触发 */
	open: [];
	/** 弹出层关闭时触发 */
	close: [];
}>();

const state = reactive({
	loading: false,
	visible: false,
	style: computed(() => {
		let result = "";
		if (props.width) {
			result += `width: ${addCssUnit(props.width)};`;
		}
		if (props.height) {
			result += `height: ${addCssUnit(props.height)};`;
		}

		return result;
	}),
	transition: computed(() => {
		switch (props.position) {
			case "center":
				return "zoom-in";
			case "top":
				return "fade-down";
			case "bottom":
				return "fade-up";
			case "left":
				return "fade-left";
			case "right":
				return "fade-right";
			default:
				return "fade";
		}
	}),
});

const handleOpen = async (openFunction?: () => void | Promise<void>) => {
	state.visible = true;
	await nextTick();
	state.loading = true;
	try {
		await callOptionalFunction(props.afterOpen ?? openFunction);
		emit("open");
	} catch (error) {
		state.visible = false;
		throw error;
	} finally {
		state.loading = false;
	}
};

const handleClose = async (closeFunction?: () => void | Promise<void>) => {
	state.loading = true;
	try {
		await callOptionalFunction(closeFunction);
		emit("close");
		state.visible = false;
	} finally {
		state.loading = false;
	}
};

const handleLoading = async (loadingFunction: () => void | Promise<void>) => {
	state.loading = true;
	try {
		await callOptionalFunction(loadingFunction);
	} finally {
		state.loading = false;
	}
};

defineExpose({
	/** 加载状态 */
	loading: toRef(state, "loading"),
	/** 是否显示 */
	visible: toRef(state, "visible"),
	/** 显示弹窗并等待初始化回调；失败时关闭弹窗并向调用方抛出异常。 */
	open: handleOpen,
	/** 等待关闭回调成功后隐藏弹窗；回调失败时保留显示状态并抛出异常。 */
	close: handleClose,
	/** 执行异步操作并在结束时恢复加载状态；异常继续向调用方传播。 */
	doLoading: handleLoading,
});
</script>

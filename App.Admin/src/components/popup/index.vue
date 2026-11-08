<template>
	<wd-popup
		v-bind="wdPopupProps"
		:custom-class="`fa-popup ${hasFooter() ? 'fa-popup__with-footer' : ''} ${props.customClass}`"
		:custom-style="`${state.style} ${props.customStyle}`"
		v-model="state.visible"
		:safe-area-inset-bottom="props.position !== 'center' && props.position !== 'top'"
		@before-enter="() => emit('beforeEnter')"
		@enter="() => emit('enter')"
		@after-enter="() => emit('afterEnter')"
		@before-leave="() => emit('beforeLeave')"
		@leave="() => emit('leave')"
		@after-leave="() => emit('afterLeave')"
		@click-modal="() => emit('clickModal')"
		@close="handleClose"
	>
		<view v-if="hasTitle" class="fa-popup__header">
			<text class="fa-popup__title">{{ props.title }}</text>
		</view>
		<button
			v-if="hasCloseIcon"
			class="fa-popup__close"
			:class="{ 'fa-popup__close--floating': !hasTitle }"
			hover-class="none"
			:disabled="state.loading"
			@click="handleCloseClick"
		>
			<view class="fa-popup__close-icon">
				<wd-icon name="close" size="18px" />
			</view>
		</button>
		<view class="fa-popup__body">
			<slot :loading="state.loading" />
		</view>
		<view v-if="hasFooter()" class="fa-popup__footer">
			<slot name="footer" :loading="state.loading" :close="handleCloseClick" />
			<wd-button
				v-if="props.showCloseButton"
				custom-style="flex: 1;"
				type="info"
				variant="plain"
				block
				:disabled="state.loading"
				@click="handleCloseClick"
			>
				{{ props.closeButtonText }}
			</wd-button>
			<wd-button
				v-if="props.showConfirmButton"
				custom-style="flex: 1;"
				type="primary"
				block
				:loading="state.loading"
				:disabled="props.disabledConfirmButton"
				@click="handleConfirmClick"
			>
				{{ props.confirmButtonText }}
			</wd-button>
		</view>
		<FaLoading mask :loading="state.loading" />
	</wd-popup>
</template>

<script setup lang="ts">
import { computed, nextTick, reactive, toRef } from "vue";
import { addCssUnit, callOptionalFunction, definePropType, useProps } from "@fast-china/utils";
import { popupProps } from "@wot-ui/ui/components/wd-popup/types";
import FaLoading from "../loading/index.vue";
import type { VNode } from "vue";

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
	/** 是否开启圆角,开启后根据弹出位置自动适配（底部弹出→上圆角，顶部弹出→下圆角，居中→四圆角）*/
	round: {
		type: Boolean,
		default: true,
	},
	/** 弹窗宽度，数字按 px 处理 */
	width: [String, Number],
	/** 弹窗高度，数字按 px 处理 */
	height: [String, Number],
	/** 弹窗显示后的初始化回调，优先于 open 方法传入的回调 */
	afterOpen: {
		type: definePropType<() => void | Promise<void>>(Function),
	},
	/** 弹窗标题，仅底部和居中弹窗显示 */
	title: String,
	/** 是否显示确认按钮 */
	showConfirmButton: Boolean,
	/** 显示底部取消按钮，与右上角的 closable 独立控制 */
	showCloseButton: Boolean,
	/** 禁用确认按钮 */
	disabledConfirmButton: Boolean,
	/** 确认按钮文字，默认确认 */
	confirmButtonText: {
		type: String,
		default: "确认",
	},
	/** 关闭按钮文字，默认取消 */
	closeButtonText: {
		type: String,
		default: "取消",
	},
});

const wdPopupProps = useProps(props, popupProps, ["safeAreaInsetBottom", "closable"]);

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
	/** 确认按钮点击事件 */
	confirm: [];
}>();

const slots = defineSlots<{
	/** 默认内容插槽 */
	default?: (props: { loading: boolean }) => VNode[];
	/** 底部插槽 */
	footer?: (props: { loading: boolean; close: () => void }) => VNode[];
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
});

const hasTitle = computed(() => (props.position === "bottom" || props.position === "center") && !!props.title);
const hasCloseIcon = computed(() => (props.position === "bottom" || props.position === "center") && props.closable);

const hasFooter = () => !!slots.footer || props.showCloseButton || props.showConfirmButton;

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

const handleConfirmClick = () => {
	if (state.loading) return;
	emit("confirm");
};

const handleCloseClick = () => {
	if (state.loading) return;
	handleClose();
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

<style lang="scss" scoped>
:deep(.fa-popup) {
	--wot-popup-radius: var(--wot-radius-extra-large);
	display: flex;
	flex-direction: column;
	box-sizing: border-box;
	overflow: hidden;
}

.fa-popup__with-footer {
	--wot-popup-bg: var(--wot-filled-oppo);
}

.fa-popup__header {
	flex-shrink: 0;
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 52px;
	padding: 12rpx 68px;
	border-bottom: 1px solid var(--wot-border-light);
	background: var(--wot-filled-oppo);
}

.fa-popup__title {
	min-width: 0;
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
	text-align: center;
	overflow-wrap: anywhere;
}

.fa-popup__close {
	position: absolute;
	top: 4px;
	right: 4px;
	z-index: 1;
	display: flex;
	align-items: center;
	justify-content: center;
	width: 44px;
	height: 44px;
	margin: 0;
	padding: 0;
	border: 0;
	background: transparent;

	&::after {
		border: 0;
	}
}

.fa-popup__close--floating {
	top: 12px;
}

.fa-popup__close-icon {
	display: flex;
	align-items: center;
	justify-content: center;
	width: 32px;
	height: 32px;
	border-radius: var(--wot-radius-full);
	color: var(--wot-popup-close-color, var(--wot-icon-auxiliary));
	background: var(--wot-filled-bottom);
}

.fa-popup__body {
	display: flex;
	flex: 1;
	flex-direction: column;
	padding: 20rpx;
	background: var(--wot-filled-bottom);
	min-width: 0;
	min-height: 0;
	overflow: auto;
}

.fa-popup__footer {
	display: flex;
	flex-shrink: 0;
	align-items: center;
	gap: 20rpx;
	padding: 24rpx var(--fa-page-padding);
	border-top: 1px solid var(--wot-border-light);
	background: var(--wot-filled-oppo);
}
</style>

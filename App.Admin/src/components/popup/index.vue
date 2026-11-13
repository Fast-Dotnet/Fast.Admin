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
		<view
			v-if="hasHeaderContent() || hasCloseIcon"
			class="fa-popup__header"
			:class="{ 'fa-popup__header--floating': !hasHeaderContent(), 'fa-popup__header--with-prefix': !!slots.titlePrefix }"
		>
			<template v-if="hasHeaderContent()">
				<view class="fa-popup__title-prefix">
					<slot name="titlePrefix" :loading="state.loading" />
				</view>
				<text class="fa-popup__title">{{ props.title }}</text>
			</template>
			<view class="fa-popup__header-actions">
				<button v-if="hasCloseIcon" class="fa-popup__close" hover-class="none" :disabled="state.loading" @click="handleCloseClick">
					<view class="fa-popup__close-icon">
						<wd-icon name="close" size="18px" />
					</view>
				</button>
			</view>
		</view>
		<view
			class="fa-popup__body"
			:class="{
				'fa-popup__body--no-scroll': props.bodyScroll !== 'auto',
				'fa-popup__body--scroll-view': props.bodyScroll === 'scroll-view',
			}"
		>
			<scroll-view v-if="props.bodyScroll === 'scroll-view'" class="fa-popup__scroll-view" scroll-y>
				<view class="fa-popup__scroll-content">
					<slot :loading="state.loading" />
				</view>
			</scroll-view>
			<slot v-else :loading="state.loading" />
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
import { onHide } from "@dcloudio/uni-app";
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
	/** 正文滚动方式。auto 使用正文的 CSS 滚动，scroll-view 需要指定弹窗高度，none 由插槽内容接管滚动。 */
	bodyScroll: {
		type: definePropType<"auto" | "scroll-view" | "none">(String),
		default: "auto",
	},
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
	/** 标题前缀插槽 */
	titlePrefix?: (props: { loading: boolean }) => VNode[];
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

const supportsHeader = computed(() => props.position === "bottom" || props.position === "center");
const hasHeaderContent = () => supportsHeader.value && Boolean(props.title || slots.titlePrefix);
const hasCloseIcon = computed(() => supportsHeader.value && props.closable);

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

// #ifdef H5
onHide(() => {
	// 传送到 body 的弹窗需要随页面隐藏一起关闭。
	if (props.rootPortal && state.visible) {
		state.visible = false;
		emit("close");
	}
});
// #endif

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

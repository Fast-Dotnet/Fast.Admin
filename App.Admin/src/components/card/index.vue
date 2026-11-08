<template>
	<view class="fa-card">
		<view v-if="props.title || $slots.title || $slots.titleExtra" class="fa-card__title">
			<slot name="title">
				<text v-if="props.title" class="fa-card__title-text">{{ props.title }}</text>
				<slot v-if="$slots.titleExtra" name="titleExtra" />
			</slot>
		</view>

		<view class="fa-card__content" @click="handleClick">
			<slot />
		</view>

		<view v-if="$slots.footer" class="fa-card__footer">
			<slot name="footer" />
		</view>
	</view>
</template>

<script setup lang="ts">
import { definePropType } from "@fast-china/utils";
import { useUserInfo } from "@/stores";
import type { BaseEvent } from "@uni-helper/uni-types";

defineOptions({
	name: "Card",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 标题 */
	title: String,
	/** 内容点击事件的权限编码；数组中任意一项命中即可显示 */
	code: {
		type: definePropType<string | string[]>([String, Array]),
	},
});

const emit = defineEmits<{
	/** 内容点击事件 */
	click: [event: BaseEvent];
}>();

const userInfoStore = useUserInfo();

const handleClick = (event: BaseEvent) => {
	if (userInfoStore.isSuperAdmin) {
		emit("click", event);
		return;
	}
	const codes = Array.isArray(props.code) ? props.code : [props.code];
	if (codes.some((code) => userInfoStore.buttonCodeList.includes(code))) {
		emit("click", event);
	}
};
</script>

<style lang="scss" scoped>
.fa-card {
	background-color: var(--wot-card-bg);
	border-radius: var(--wot-radius-large);
	box-shadow: var(--fa-shadow-subtle);
	overflow: hidden;
	transition: box-shadow var(--fa-motion-duration-normal) var(--fa-motion-easing-standard);
	margin-bottom: var(--wot-spacing-extra-loose);
}

.fa-card__title {
	width: 100%;
	padding: 20rpx 20rpx 14rpx;
	box-sizing: border-box;
	min-width: 0;
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 20rpx;
	border-bottom: var(--wot-stroke-main) solid var(--wot-border-main);
}

:deep(.fa-card__title-text) {
	color: var(--wot-text-main);
	font-size: 16px;
	font-weight: var(--wot-font-weight-medium);
	flex: 1;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

:deep(.fa-card__title-view) {
	display: flex;
	align-items: center;
	gap: 10rpx;
}

.fa-card__content {
	color: var(--wot-text-secondary);
	font-size: 14px;
	line-height: 1.6;
	padding: 24rpx;
	box-sizing: border-box;
}

.fa-card__footer {
	position: relative;
	display: flex;
	align-items: center;
	justify-content: flex-end;
	padding: 20rpx 20rpx 16rpx;

	&::after {
		position: absolute;
		display: block;
		content: "";
		width: calc(100% - 24px);
		height: 1px;
		left: 12px;
		top: 0;
		transform: scaleY(0.5);
		background: var(--wot-border-main);
	}

	:deep(.wd-button + .wd-button) {
		margin-left: 20rpx;
	}
}
</style>

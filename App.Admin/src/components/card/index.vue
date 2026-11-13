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
	/** 内容点击的权限编码，省略时直接触发点击，数组命中任意一项即可 */
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
	if (props.code === undefined || userInfoStore.isSuperAdmin) {
		emit("click", event);
		return;
	}
	const codes = Array.isArray(props.code) ? props.code : [props.code];
	if (codes.some((code) => userInfoStore.buttonCodeList.includes(code))) {
		emit("click", event);
	}
};
</script>

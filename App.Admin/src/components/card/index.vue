<template>
	<view
		:class="[
			'fa-card',
			props.customClass,
			{
				'fa-card--selectable': props.selectable,
				'fa-card--selected': props.selectable && props.selected,
			},
		]"
		:style="props.customStyle"
	>
		<view v-if="props.selectable" class="fa-card__selection" @click.stop>
			<wd-checkbox :model-value="props.selected" :disabled="props.disabled" type="square" @update:model-value="handleSelectionChange" />
		</view>

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
import { baseProps } from "@wot-ui/ui/common/props";
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
	...baseProps,
	/** 卡片标题；使用 title 插槽时由插槽接管 */
	title: String,
	/** 内容点击的权限编码，省略时直接触发点击，数组命中任意一项即可 */
	code: {
		type: definePropType<string | string[]>([String, Array]),
	},
	/** 是否开启选择模式 */
	selectable: Boolean,
	/** 是否选中，配合 v-model:selected 使用 */
	selected: Boolean,
	/** 是否禁止用户修改选中状态，仅影响选择操作 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 正文区域点击并通过 code 权限校验时触发；未设置 code 时直接触发 */
	click: [event: BaseEvent];
	/** 请求更新选中状态 */
	"update:selected": [selected: boolean];
	/** 用户通过复选框或正文修改选中状态时触发 */
	change: [selected: boolean];
}>();

const userInfoStore = useUserInfo();

/** 统一处理复选框和正文选择，选中状态由父组件维护。 */
const handleSelectionChange = (value: boolean) => {
	if (!props.selectable || props.disabled || value === props.selected) return;
	emit("update:selected", value);
	emit("change", value);
};

const handleClick = (event: BaseEvent) => {
	if (props.code === undefined || userInfoStore.hasButtonPermission(Array.isArray(props.code) ? props.code : [props.code])) {
		emit("click", event);
	}
};
</script>

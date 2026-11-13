<template>
	<view class="fa-tag-flag">
		<wd-tag
			v-for="item in dictionaries"
			:key="String(item.value)"
			v-bind="wdTagProps"
			:type="item.type === 'info' ? 'default' : (item.type ?? props.type)"
			@click="(evt: MouseEvent) => emit('click', evt, item)"
			@close="(evt: MouseEvent) => emit('close', evt, item)"
			@confirm="({ value }: { value: string }) => emit('confirm', { value })"
		>
			<slot :label="item.label" :tips="item.tips" :item="item">
				{{ item.label }}
			</slot>
			<template v-if="$slots.icon" #icon>
				<slot name="icon" :item="item" />
			</template>
			<template v-if="$slots.add" #add>
				<slot name="add" :item="item" />
			</template>
		</wd-tag>
	</view>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useProps } from "@fast-china/utils";
import { tagProps } from "@wot-ui/ui/components/wd-tag/types";
import { useApp } from "@/stores";

defineOptions({
	name: "TagFlag",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	...tagProps,
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
	/** 值 */
	value: {
		type: [Number, String],
		default: undefined,
	},
});

const wdTagProps = useProps(props, tagProps, ["type"]);

const emit = defineEmits<{
	/** 标签点击时触发 */
	click: [event: MouseEvent, item: FaTableEnumColumnCtx];
	/** 点击关闭按钮时触发 */
	close: [event: MouseEvent, item: FaTableEnumColumnCtx];
	/** 新增标签输入内容确定后触发 */
	confirm: [detail: { value: string }];
}>();

const appStore = useApp();

// 只展示组合值中已启用且允许显示的单比特字典项。
const dictionaries = computed(() => {
	if (!props.name) return [];
	// 使用函数形式，兼容移动端构建目标不支持 BigInt 字面量的限制。
	const zero = BigInt(0);
	const one = BigInt(1);
	const value = BigInt(props.value || 0);
	return (appStore.getDictionary(props.name) ?? []).filter((item) => {
		const flag = BigInt(item.value);
		return item.show && (flag & (flag - one)) === zero && (value & flag) !== zero;
	});
});
</script>

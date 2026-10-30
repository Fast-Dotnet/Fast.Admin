<template>
	<ElSpace warp>
		<ElTag
			v-for="item in dictionaries"
			:key="String(item.value)"
			v-bind="elTagProps"
			:type="item.type ?? props.type"
			:title="item.tips"
			@click="(evt: MouseEvent) => emit('click', evt, item)"
			@close="(evt: MouseEvent) => emit('close', evt, item)"
		>
			{{ item.label }}
		</ElTag>
	</ElSpace>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { tagProps } from "element-plus";
import { useProps } from "@fast-china/utils";
import { useApp } from "@/stores";
import type { FaTableEnumColumnCtx } from "fast-element-plus";

defineOptions({
	name: "TagFlag",
});

const props = defineProps({
	// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
	...tagProps,
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
	/** 用于匹配字典项的值 */
	value: {
		type: [Number, String],
		default: undefined,
	},
});

// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
const elTagProps = useProps(props, tagProps, ["type"]);

const emit = defineEmits<{
	/** 点击 Tag 时触发的事件 */
	close: [evt: MouseEvent, value: FaTableEnumColumnCtx];
	/** 关闭 Tag 时触发的事件 */
	click: [evt: MouseEvent, value: FaTableEnumColumnCtx];
}>();

const appStore = useApp();

// 指定名称对应的字典项列表
const dictionaries = computed(() => {
	if (!props.name) return [];
	const value = BigInt(props.value || 0n);
	return appStore.getDictionary(props.name).filter((item) => {
		const flag = BigInt(item.value);
		return item.show && (flag & (flag - 1n)) === 0n && (value & flag) !== 0n;
	});
});
</script>

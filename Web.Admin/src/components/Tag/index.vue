<template>
	<ElTag
		v-if="props.name && dictionary?.show"
		v-bind="elTagProps"
		:type="dictionary?.type ?? props.type"
		:title="dictionary?.tips"
		@click="(evt: MouseEvent) => emit('click', evt)"
		@close="(evt: MouseEvent) => emit('close', evt)"
	>
		<slot :label="dictionary?.label" :tips="dictionary?.tips">
			{{ dictionary?.label }}
		</slot>
	</ElTag>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { tagEmits, tagProps } from "element-plus";
import { useProps } from "@fast-china/utils";
import { useApp } from "@/stores";

defineOptions({
	name: "Tag",
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
		type: [Number, String, Boolean],
		default: undefined,
	},
});

// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
const elTagProps = useProps(props, tagProps, ["type"]);

const emit = defineEmits({
	...tagEmits,
});

const appStore = useApp();

// 指定名称对应的字典项列表
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name) : []));
// 与当前值匹配的字典项
const dictionary = computed(() => (props.value == null ? null : dictionaries.value.find((f) => f.value === props.value)));
</script>

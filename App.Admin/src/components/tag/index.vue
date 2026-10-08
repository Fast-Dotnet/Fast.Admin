<template>
	<wd-tag
		v-bind="wdTagProps"
		:type="type"
		@click="(evt: MouseEvent) => emit('click', evt)"
		@close="(evt: MouseEvent) => emit('close', evt)"
		@confirm="({ value }: { value: string }) => emit('confirm', { value })"
	>
		<slot :label="dictionary?.label" :tips="dictionary?.tips">
			{{ dictionary?.label }}
		</slot>
		<template #icon>
			<slot name="icon" />
		</template>
		<template #add>
			<slot name="add" />
		</template>
	</wd-tag>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useProps } from "@fast-china/utils";
import { tagProps } from "@wot-ui/ui/components/wd-tag/types";
import { useApp } from "@/stores";

defineOptions({
	name: "Tag",
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
		type: [Number, String, Boolean],
		default: undefined,
	},
});

const wdTagProps = useProps(props, tagProps, ["type"]);

const emit = defineEmits<{
	/** 标签点击时触发 */
	click: [event: MouseEvent];
	/** 点击关闭按钮时触发 */
	close: [event: MouseEvent];
	/** 新增标签输入内容确定后触发 */
	confirm: [detail: { value: string }];
}>();

const appStore = useApp();

// 字典
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name) : []));
// 字典值
const dictionary = computed(() => (props.value == null ? null : dictionaries.value.find((f) => f.value === props.value)));

const type = computed(() => (dictionary?.value?.type === "info" ? "default" : dictionary?.value?.type));
</script>

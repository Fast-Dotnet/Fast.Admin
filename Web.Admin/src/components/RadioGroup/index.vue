<template>
	<ElRadioGroup v-bind="elRadioGroupProps" v-model="modelValue" @change="(value) => emit('change', value)">
		<template v-if="props.button">
			<ElRadioButton
				v-for="(item, index) in dictionaries"
				:key="index"
				:value="item.value"
				:disabled="item.disabled === false ? undefined : item.disabled"
				border
			>
				{{ item.label }}
			</ElRadioButton>
		</template>
		<template v-else>
			<ElRadio
				v-for="(item, index) in dictionaries"
				:key="index"
				:value="item.value"
				:disabled="item.disabled === false ? undefined : item.disabled"
			>
				{{ item.label }}
			</ElRadio>
		</template>
	</ElRadioGroup>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { radioGroupEmits, radioGroupProps } from "element-plus";
import { useProps } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { useApp } from "@/stores";

defineOptions({
	name: "RadioGroup",
});

const props = defineProps({
	// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
	...radioGroupProps,
	/** 是否使用按钮样式 */
	button: {
		type: Boolean,
		default: false,
	},
	/** 字典名称 */
	name: {
		type: String,
		required: true,
	},
});

// eslint-disable-next-line @typescript-eslint/no-deprecated -- Element Plus 2.x 暂无等价的公开运行时 Props 对象替代。
const elRadioGroupProps = useProps(props, radioGroupProps, ["modelValue"]);

const emit = defineEmits({
	...radioGroupEmits,
});

/** 当前选中的字典值，默认使用启用状态对应的值 */
const modelValue = defineModel<string | number | boolean>({ default: CommonStatusEnum.Enable });

const appStore = useApp();
// 当前字典中允许显示的选项
const dictionaries = computed(() => (props.name ? appStore.getDictionary(props.name).filter((f) => f.show) : []));
</script>

<template>
	<el-cascader v-bind="$attrs" v-model="modelValue" :options="iconList" :placeholder="props.placeholder" filterable clearable>
		<template #prefix>
			<FaIcon v-if="modelValue" size="16" :name="modelValue" />
		</template>
		<template #default="{ node, data }: { node: CascaderNode; data: ElSelectorOutput<string> }">
			<div style="display: flex; align-items: center; gap: 3px">
				<FaIcon v-if="node.isLeaf" size="16" :name="data.value" />
				<span>{{ data.label }}</span>
			</div>
		</template>
	</el-cascader>
</template>

<script lang="ts" setup>
import * as ElementPlusIconsVue from "@element-plus/icons-vue";
import * as FastElementPlusIconsVue from "@fast-element-plus/icons-vue";
import { withDefineType } from "@fast-china/utils";
import type { CascaderNode } from "element-plus";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "IconSelect",
});

const props = defineProps({
	/** 编辑区占位文本 */
	placeholder: {
		type: String,
		default: "请选择图标",
	},
});

const iconList = withDefineType<ElSelectorOutput<string>[]>([
	{
		value: "el-icon",
		label: "el-icon",
		children: Object.keys(ElementPlusIconsVue).map((item) => ({
			value: `el-icon-${item}`,
			label: item,
		})),
	},
	{
		value: "fa-icon",
		label: "fa-icon",
		children: Object.keys(FastElementPlusIconsVue)
			.filter((f) => f !== "default")
			.map((item) => ({
				value: `fa-icon-${item}`,
				label: item,
			})),
	},
]);

const modelValue = defineModel<string>();
</script>

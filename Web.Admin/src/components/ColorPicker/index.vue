<template>
	<el-color-picker v-bind="$attrs" v-model="modelValue" :predefine="predefineColorList" />
</template>

<script lang="ts" setup>
import { defaultThemeColor } from "@/stores";

defineOptions({
	name: "ColorPicker",
});

const predefineColorList = [
	defaultThemeColor,
	"#4488FE",
	"#2A3A93",
	"#EB4537", // 红色
	"#FF8225", // 橙色
	"#FAC230", // 黄色
	"#55AF7B", // 绿色
	"#FF4191", // 粉色
	"#4286F3", // 蓝色
	"#AD6DEF", // 紫色
];

/** 选中的颜色值 */
const modelValue = defineModel<string>();
</script>

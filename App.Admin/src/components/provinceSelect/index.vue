<template>
	<FaCellPicker :title="props.title" :value="provinceName" :is-link="!props.disabled" placeholder="请选择省份">
		<template #default="{ visible }">
			<FaSelectPage
				title="省份选择"
				v-model="modelValue"
				v-model:label="provinceName"
				v-model:visible="visible.value"
				:request-api="regionApi.provinceSelector"
				clearable
				:pagination="false"
				disabled
				@change="(data) => emit('change', data)"
			/>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { regionApi } from "@/api/services/Center/region";

defineOptions({
	name: "ProvinceSelect",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 标题 */
	title: {
		type: String,
		default: "省份",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的省份 ID */
const modelValue = defineModel<string>();
/** 选中省份的名称 */
const provinceName = defineModel<string>("provinceName");
</script>

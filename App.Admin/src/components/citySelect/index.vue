<template>
	<FaCellPicker :title="props.title" :value="modelValue ? `${provinceName} / ${cityName}` : ''" :is-link="!props.disabled" placeholder="请选择城市">
		<template #default="{ visible }">
			<FaSelectPage
				title="城市选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="regionApi.citySelector"
				clearable
				tree-select
				:pagination="false"
				disabled
				@change="handleChange"
			/>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { regionApi } from "@/api/services/Center/region";

defineOptions({
	name: "CitySelect",
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
		default: "城市",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null, parentItem: ElSelectorOutput<string> | null];
}>();

/** 选中的城市 ID */
const modelValue = defineModel<string>();
/** 选中城市所属的省份名称 */
const provinceName = defineModel<string>("provinceName");
/** 选中城市的名称 */
const cityName = defineModel<string>("cityName");

const handleChange = (item: ElSelectorOutput<string>, parentItem: ElSelectorOutput<string>) => {
	provinceName.value = parentItem?.label;
	cityName.value = item?.label;
	emit("change", item, parentItem);
};
</script>

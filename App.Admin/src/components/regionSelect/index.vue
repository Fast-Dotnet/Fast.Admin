<template>
	<FaCellPicker
		:title="props.title"
		:value="modelValue ? [provinceName, cityName, districtName].filter(Boolean).join(' / ') : ''"
		:is-link="!props.disabled"
		placeholder="请选择地区"
	>
		<template #default="{ visible }">
			<FaTreePicker
				title="地区选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:request-api="regionApi.regionSelector"
				clearable
				disabled-search
				@change="handleChange"
			/>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { regionApi } from "@/api/services/Center/region";

defineOptions({
	name: "RegionSelect",
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
		default: "地区",
	},
	/** 是否禁用选择 */
	disabled: Boolean,
});

const emit = defineEmits<{
	/** 选中项变化，清空时返回 null */
	change: [item: ElSelectorOutput<string> | null];
}>();

/** 选中的地区 ID */
const modelValue = defineModel<string>();
/** 选中路径中的省份名称 */
const provinceName = defineModel<string>("provinceName");
/** 选中路径中的城市名称 */
const cityName = defineModel<string>("cityName");
/** 选中区县的名称 */
const districtName = defineModel<string>("districtName");

const handleChange = (item: ElSelectorOutput<string> | null, path: ElSelectorOutput<string>[] | null) => {
	const [province, city, district] = path ?? [];
	provinceName.value = province?.label;
	cityName.value = city?.label;
	districtName.value = district?.label;
	emit("change", item ? { ...item, data: { ...item.data, provinceName: province?.label, cityName: city?.label } } : null);
};
</script>

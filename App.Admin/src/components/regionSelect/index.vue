<template>
	<FaCellPicker
		:title="props.title"
		:value="modelValue ? `${provinceName} / ${cityName} / ${districtName}` : ''"
		:is-link="!props.disabled"
		placeholder="请选择地区"
	>
		<template #default="{ visible }">
			<FaSelectPage
				title="地区选择"
				v-model="modelValue"
				v-model:visible="visible.value"
				:data="regionList"
				clearable
				tree-select
				use-virtual-list
				:open-callback="handleOpenCallback"
				@change="handleChange"
			>
				<template #default="{ label, data }">
					<view style="display: flex; flex-direction: column; gap: 4rpx">
						<text style="color: var(--wot-text-main); font-weight: var(--wot-font-weight-semibold)">
							{{ label }}
						</text>
						<text style="color: var(--wot-text-secondary); font-size: var(--wot-typography-label-size-main)">
							{{ data?.provinceName }} · {{ data?.cityName }}
						</text>
					</view>
				</template>
			</FaSelectPage>
		</template>
	</FaCellPicker>
</template>

<script setup lang="ts">
import { ref } from "vue";
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
/** 选中区县所属的省份名称 */
const provinceName = defineModel<string>("provinceName");
/** 选中区县所属的城市名称 */
const cityName = defineModel<string>("cityName");
/** 选中区县的名称 */
const districtName = defineModel<string>("districtName");

const regionList = ref([]);

const handleChange = (item: ElSelectorOutput<string>) => {
	provinceName.value = item?.data?.provinceName;
	cityName.value = item?.data?.cityName;
	districtName.value = item?.label;
	emit("change", item);
};

const handleOpenCallback = async () => {
	const apiRes = await regionApi.regionSelector();
	regionList.value = apiRes.flatMap((province) =>
		(province.children ?? []).map((city) => ({
			label: `${province.label} · ${city.label}`,
			children: (city.children ?? []).map((district) => ({
				...district,
				data: {
					...district.data,
					provinceName: province.label,
					cityName: city.label,
				},
			})),
		}))
	);
};
</script>

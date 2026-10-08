<template>
	<FaSelect
		v-bind="$attrs"
		:request-api="applicationApi.applicationSelector"
		v-model="modelValue"
		v-model:label="appName"
		placeholder="请选择应用"
		clearable
		more-detail
		@change="(data) => emit('change', data)"
	>
		<template #default="data">
			<div style="display: flex; justify-content: space-between; align-items: center; gap: 8px; width: 100%">
				<FaAvatar v-if="data.data?.logoUrl" :src="data.data.logoUrl" thumb size="small" />
				<div style="flex: 1">
					<span>{{ data.label }}</span>
					<span style="display: flex; justify-content: space-between; width: 100%">
						<span style="font-size: var(--el-font-size-extra-small); padding-right: 8px">{{ data.data?.appNo }}</span>
						<Tag name="EditionEnum" :value="data.data?.edition" size="small" />
					</span>
				</div>
			</div>
		</template>
	</FaSelect>
</template>

<script lang="ts" setup>
import { applicationApi } from "@/api/services/Center/application";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "ApplicationSelect",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput];
}>();

/** 选中的应用 ID */
const modelValue = defineModel<string>();
/** 选中应用的名称 */
const appName = defineModel<string>("appName");
</script>

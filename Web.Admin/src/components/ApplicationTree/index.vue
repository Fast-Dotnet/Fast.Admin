<template>
	<FaTree
		v-bind="$attrs"
		v-model="modelValue"
		v-model:label="appName"
		title="应用列表"
		width="240"
		:request-api="applicationApi.applicationSelector"
		@change="(data) => emit('change', data)"
	>
		<template #label="{ data }">
			<FaAvatar style="margin-right: 5px" :src="data.data?.logoUrl" thumb size="small" />
			<span>{{ data.label }}</span>
		</template>
		<template #default="{ data }">
			<Tag size="small" effect="plain" name="EditionEnum" :value="data.data?.edition" />
		</template>
	</FaTree>
</template>

<script lang="ts" setup>
import { applicationApi } from "@/api/services/Center/application";
import type { ElTreeOutput } from "fast-element-plus";

defineOptions({
	name: "ApplicationTree",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElTreeOutput];
}>();

/** 选中的应用 ID */
const modelValue = defineModel<string>();
/** 选中应用的名称 */
const appName = defineModel<string>("appName");
</script>

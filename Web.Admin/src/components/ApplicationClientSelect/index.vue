<template>
	<FaSelect
		v-bind="$attrs"
		:request-api="applicationClientApi.applicationClientSelector"
		v-model="modelValue"
		placeholder="请选择应用标识"
		clearable
		more-detail
		@change="handleChange"
	>
		<template #default="data">
			<span>{{ data.label }}</span>
			<span style="text-align: right">
				<TagFlag name="AppEnvironmentEnum" :value="data.data?.appType" size="small" />
			</span>
		</template>
	</FaSelect>
</template>

<script lang="ts" setup>
import { applicationClientApi } from "@/api/services/Center/applicationClient";
import type { ElSelectorOutput } from "fast-element-plus";

defineOptions({
	name: "ApplicationClientSelect",
});

const emit = defineEmits<{
	/** 选项变化事件，透传选中的数据 */
	change: [value: ElSelectorOutput];
}>();

/** 选中的应用标识 */
const modelValue = defineModel<string>();
/** 选中应用的Id */
const appId = defineModel<string>("appId");
/** 选中应用的名称 */
const appName = defineModel<string>("appName");

const handleChange = (data: ElSelectorOutput) => {
	if (data) {
		appId.value = data.data?.appId;
		appName.value = data.data?.appName;
		emit("change", data);
	} else {
		appId.value = undefined;
		appName.value = undefined;
		emit("change", undefined);
	}
};
</script>

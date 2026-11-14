<template>
	<FaPopup ref="popupRef" title="序号配置详情" height="70%" position="bottom" closable>
		<FaCard>
			<FaField label="规则类型">
				<FaTag name="SysSerialRuleTypeEnum" :value="formData.ruleType" />
			</FaField>
			<FaField label="前缀" :value="formData.prefix" />
			<FaField label="时间类型">
				<FaText name="SerialDateTypeEnum" :value="formData.dateType" />
			</FaField>
			<FaField label="分隔符">
				<FaText name="SerialSpacerEnum" :value="formData.spacer" />
			</FaField>
			<FaField label="长度" :value="formData.length" />
			<FaField label="最后序号" :value="formData.lastSerial" />
			<FaField label="最后编号" :value="formData.lastSerialNo" copyable />
			<FaField label="最后生成时间" :value="formData.lastTime" />
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</FaCard>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import type { QuerySysSerialRulePagedOutput } from "@/api/services/Center/sysSerial/models/QuerySysSerialRulePagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevSysSerialDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QuerySysSerialRulePagedOutput>();

const detail = (item: QuerySysSerialRulePagedOutput) => {
	popupRef.value.open(() => {
		formData.value = item;
	});
};

defineExpose({
	detail,
});
</script>

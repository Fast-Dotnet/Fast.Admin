<template>
	<FaPopup ref="faPopupRef" title="单号配置详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="规则类型" center>
				<FaTag name="SerialRuleTypeEnum" :value="formData.ruleType" />
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
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { QuerySerialRulePagedOutput } from "@/api/services/Admin/serial/models/QuerySerialRulePagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminSerialDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QuerySerialRulePagedOutput>({});

const detail = throttle(
	async (item: QuerySerialRulePagedOutput) =>
		await faPopupRef.value.open(() => {
			formData.value = item;
		})
);

defineExpose({
	detail,
});
</script>

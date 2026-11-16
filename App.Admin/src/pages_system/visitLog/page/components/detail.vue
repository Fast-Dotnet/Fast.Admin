<template>
	<FaPopup ref="popupRef" title="访问日志详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="访问类型" center>
				<FaTag name="VisitTypeEnum" :value="formData.visitType" />
			</FaField>
			<FaField label="部门" :value="formData.departmentName" />
			<FaField label="省份" :value="formData.province" />
			<FaField label="城市" :value="formData.city" />
			<FaField label="Ip" :value="formData.ip" />
			<FaField label="时间" :value="formData.createdTime" />
			<FaField label="设备" :value="formData.device" />
			<FaField label="操作系统" :value="formData.os" />
			<FaField label="浏览器" :value="formData.browser" />
			<FaField label="租户" :value="formData.tenantName" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { VisitLogModel } from "@/api/services/Center/visitLog/models/VisitLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemVisitLogDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<VisitLogModel>();

const detail = throttle((data: VisitLogModel) =>
	popupRef.value.open(() => {
		formData.value = data;
	})
);

defineExpose({
	detail,
});
</script>

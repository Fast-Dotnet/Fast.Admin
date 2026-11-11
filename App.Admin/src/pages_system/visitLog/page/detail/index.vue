<template>
	<FaPopup ref="popupRef" title="访问日志详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<FaField label="昵称" :value="state.formData.nickName" />
				<FaField label="手机" :value="state.formData.mobile" />
				<FaField label="访问类型">
					<FaTag name="VisitTypeEnum" :value="state.formData.visitType" />
				</FaField>
				<FaField label="部门" :value="state.formData.departmentName" />
				<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
				<FaField label="Ip" :value="state.formData.ip" />
				<FaField label="时间" :value="state.formData.createdTime" />
				<FaField label="设备" :value="state.formData.device" />
				<FaField label="操作系统" :value="state.formData.os" />
				<FaField label="浏览器" :value="state.formData.browser" />
				<FaField label="租户" :value="state.formData.tenantName" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import type { VisitLogModel } from "@/api/services/Center/visitLog/models/VisitLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemVisitLogDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<VisitLogModel>({}),
});

const detail = (data: VisitLogModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>

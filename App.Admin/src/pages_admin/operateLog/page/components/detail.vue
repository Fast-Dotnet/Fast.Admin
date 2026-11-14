<template>
	<FaPopup ref="popupRef" title="操作日志详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="操作人" :value="formData.createdUserName" />
			<FaField label="工号" :value="formData.employeeNo" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="标题" :value="formData.title" />
			<FaField label="操作类型">
				<FaTag name="OperateLogTypeEnum" :value="formData.operateType" />
			</FaField>
			<FaField label="业务编码" :value="formData.bizNo" />
			<FaField label="部门" :value="formData.departmentName" />
			<FaField label="省份" :value="formData.province" />
			<FaField label="城市" :value="formData.city" />
			<FaField label="Ip" :value="formData.ip" />
			<FaField label="时间" :value="formData.createdTime" />
			<FaField label="设备" :value="formData.device" />
			<FaField label="操作系统" :value="formData.os" />
			<FaField label="浏览器" :value="formData.browser" />

			<view class="log__title">描述</view>
			<rich-text :nodes="formData.description" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import type { OperateLogModel } from "@/api/services/Admin/operateLog/models/OperateLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminOperateLogDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<OperateLogModel>();

const detail = (data: OperateLogModel) => {
	popupRef.value.open(() => {
		formData.value = data;
	});
};

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log__title {
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
	overflow-wrap: anywhere;
}
</style>

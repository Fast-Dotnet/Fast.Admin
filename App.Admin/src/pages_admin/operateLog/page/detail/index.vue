<template>
	<FaPopup ref="popupRef" title="操作日志详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<FaField label="操作人" :value="state.formData.createdUserName" />
				<FaField label="工号" :value="state.formData.employeeNo" />
				<FaField label="手机" :value="state.formData.mobile" />
				<FaField label="标题" :value="state.formData.title" />
				<FaField label="操作类型">
					<FaTag name="OperateLogTypeEnum" :value="state.formData.operateType" />
				</FaField>
				<FaField label="业务编码" :value="state.formData.bizNo" />
				<FaField label="部门" :value="state.formData.departmentName" />
				<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
				<FaField label="Ip" :value="state.formData.ip" />
				<FaField label="时间" :value="state.formData.createdTime" />
				<FaField label="设备" :value="state.formData.device" />
				<FaField label="操作系统" :value="state.formData.os" />
				<FaField label="浏览器" :value="state.formData.browser" />

				<view class="log__title">描述</view>
				<rich-text :nodes="state.formData.description" />
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
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

const state = reactive({
	formData: withDefineType<OperateLogModel>({}),
});

const detail = (data: OperateLogModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
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

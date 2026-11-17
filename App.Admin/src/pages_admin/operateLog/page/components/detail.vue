<template>
	<FaPopup ref="faPopupRef" title="操作日志详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="操作人" :value="formData.createdUserName" />
			<FaField label="工号" :value="formData.employeeNo" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="标题" :value="formData.title" />
			<FaField label="操作类型" center>
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

			<wd-divider content-position="left">描述</wd-divider>
			<wd-rich-text :content="formData.description" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import type { OperateLogModel } from "@/api/services/Admin/operateLog/models/OperateLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminOperateLogDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<OperateLogModel>();

const detail = throttle(
	async (data: OperateLogModel) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>

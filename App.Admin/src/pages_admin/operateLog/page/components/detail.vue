<template>
	<FaPopup ref="faPopupRef" title="操作日志详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="操作人" :value="formData.createdUserName" selectable />
			<FaField label="工号" :value="formData.employeeNo" copyable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="标题" :value="formData.title" selectable />
			<FaField label="操作类型" center>
				<FaTag name="OperateLogTypeEnum" :value="formData.operateType" />
			</FaField>
			<FaField label="业务编码" :value="formData.bizNo" copyable />
			<FaField label="部门" :value="formData.departmentName" selectable />
			<FaField label="省份" :value="formData.province" selectable />
			<FaField label="城市" :value="formData.city" selectable />
			<FaField label="Ip" :value="formData.ip" selectable />
			<FaField label="时间" :value="formData.createdTime" selectable />
			<FaField label="设备" :value="formData.device" selectable />
			<FaField label="操作系统" :value="formData.os" selectable />
			<FaField label="浏览器" :value="formData.browser" selectable />

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

const formData = shallowRef<OperateLogModel>({});

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

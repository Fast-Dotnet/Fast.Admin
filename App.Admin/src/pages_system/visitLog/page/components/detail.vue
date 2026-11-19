<template>
	<FaPopup ref="faPopupRef" title="访问日志详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="formData.nickName" selectable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="访问类型" center>
				<FaTag name="VisitTypeEnum" :value="formData.visitType" />
			</FaField>
			<FaField label="部门" :value="formData.departmentName" selectable />
			<FaField label="省份" :value="formData.province" selectable />
			<FaField label="城市" :value="formData.city" selectable />
			<FaField label="Ip" :value="formData.ip" selectable />
			<FaField label="时间" :value="formData.createdTime" selectable />
			<FaField label="设备" :value="formData.device" selectable />
			<FaField label="操作系统" :value="formData.os" selectable />
			<FaField label="浏览器" :value="formData.browser" selectable />
			<FaField v-if="userInfoStore.isSuperAdmin" label="租户" :value="formData.tenantName" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { useUserInfo } from "@/stores";
import type { VisitLogModel } from "@/api/services/Center/visitLog/models/VisitLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemVisitLogDetail",
});

const userInfoStore = useUserInfo();

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<VisitLogModel>({});

const detail = throttle(
	async (data: VisitLogModel) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>

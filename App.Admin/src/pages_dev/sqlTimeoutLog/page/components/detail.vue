<template>
	<FaPopup ref="faPopupRef" title="Sql超时详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="formData.nickName" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="超时秒数" :value="formData.timeoutSeconds == null ? '--' : `${formData.timeoutSeconds.toFixed(4)}秒`" />
			<FaField label="文件名称" :value="formData.fileName" />
			<FaField label="文件行数" :value="formData.fileLine" />
			<FaField label="方法名" :value="formData.methodName" />
			<FaField label="部门" :value="formData.departmentName" />
			<FaField label="省份" :value="formData.province" />
			<FaField label="城市" :value="formData.city" />
			<FaField label="Ip" :value="formData.ip" />
			<FaField label="时间" :value="formData.createdTime" />
			<FaField label="设备" :value="formData.device" />
			<FaField label="操作系统" :value="formData.os" />
			<FaField label="浏览器" :value="formData.browser" />
			<FaField label="租户" :value="formData.tenantName" />

			<view class="log-content__header">
				<text class="log-content__title">纯Sql</text>
				<wd-button v-if="formData.pureSql" size="small" variant="text" @click="copy(formData.pureSql)">复制</wd-button>
			</view>
			<text v-if="formData.pureSql" class="log-content__text" selectable user-select>{{ formData.pureSql }}</text>
			<text v-else class="log-content__empty">暂无内容</text>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { copy, throttle } from "@fast-china/utils";
import type { SqlTimeoutLogModel } from "@/api/services/Center/sqlTimeoutLog/models/SqlTimeoutLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevSqlTimeoutLogDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<SqlTimeoutLogModel>();

const detail = throttle(
	async (data: SqlTimeoutLogModel) =>
		await faPopupRef.value.open(() => {
			formData.value = data;
		})
);

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log-content__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
}

.log-content__title {
	font-weight: var(--wot-font-weight-semibold);
}

.log-content__text {
	display: block;
	padding: 12px;
	border: 1px solid var(--wot-border-light);
	border-radius: 8px;
	background: var(--wot-filled-oppo);
	font-family: monospace;
	font-size: var(--wot-typography-label-size-main);
	line-height: 1.6;
	white-space: pre-wrap;
	overflow-wrap: anywhere;
}

.log-content__empty {
	color: var(--wot-text-auxiliary);
}
</style>

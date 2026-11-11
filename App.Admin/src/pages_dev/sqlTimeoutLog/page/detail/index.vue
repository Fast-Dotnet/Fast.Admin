<template>
	<FaPopup ref="popupRef" title="Sql超时详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
			<view style="display: flex; flex-direction: column; gap: 10px">
				<FaField label="昵称" :value="state.formData.nickName" />
				<FaField label="手机" :value="state.formData.mobile" />
				<FaField label="秒数" :value="state.formData.timeoutSeconds == null ? '--' : `${state.formData.timeoutSeconds.toFixed(4)}秒`" />
				<FaField label="文件名称" :value="state.formData.fileName" />
				<FaField label="文件行数" :value="state.formData.fileLine" />
				<FaField label="方法名" :value="state.formData.methodName" />
				<FaField label="部门" :value="state.formData.departmentName" />
				<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
				<FaField label="Ip" :value="state.formData.ip" />
				<FaField label="时间" :value="state.formData.createdTime" />
				<FaField label="设备" :value="state.formData.device" />
				<FaField label="操作系统" :value="state.formData.os" />
				<FaField label="浏览器" :value="state.formData.browser" />
				<FaField label="租户" :value="state.formData.tenantName" />

				<view>
					<view class="log-content__header">
						<text class="log-content__title">纯Sql</text>
						<wd-button v-if="state.formData.pureSql" size="small" variant="text" @click="copy(state.formData.pureSql)">复制</wd-button>
					</view>
					<text v-if="state.formData.pureSql" class="log-content__text" selectable>{{ state.formData.pureSql }}</text>
					<text v-else class="log-content__empty">暂无内容</text>
				</view>
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { copy, withDefineType } from "@fast-china/utils";
import type { SqlTimeoutLogModel } from "@/api/services/Center/sqlTimeoutLog/models/SqlTimeoutLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevSqlTimeoutLogDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<SqlTimeoutLogModel>({}),
});

const detail = (data: SqlTimeoutLogModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
	});
};

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log-content__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	margin-bottom: 8px;
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

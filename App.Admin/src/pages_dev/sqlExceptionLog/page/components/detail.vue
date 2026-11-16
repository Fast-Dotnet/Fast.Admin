<template>
	<FaPopup ref="popupRef" title="Sql异常详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="state.formData.nickName" />
			<FaField label="手机" :value="state.formData.mobile" />
			<FaField label="异常信息" :value="state.formData.message" color="var(--wot-danger-main)" />
			<FaField label="文件名称" :value="state.formData.fileName" />
			<FaField label="文件行数" :value="state.formData.fileLine" />
			<FaField label="方法名" :value="state.formData.methodName" />
			<FaField label="source" :value="state.formData.source" />
			<FaField label="部门" :value="state.formData.departmentName" />
			<FaField label="省份" :value="state.formData.province" />
			<FaField label="城市" :value="state.formData.city" />
			<FaField label="Ip" :value="state.formData.ip" />
			<FaField label="时间" :value="state.formData.createdTime" />
			<FaField label="设备" :value="state.formData.device" />
			<FaField label="操作系统" :value="state.formData.os" />
			<FaField label="浏览器" :value="state.formData.browser" />
			<FaField label="租户" :value="state.formData.tenantName" />

			<view class="log-content">
				<view class="log-content__section">
					<view class="log-content__header">
						<button class="log-content__toggle" hover-class="none" @click="state.isStackTraceExpanded = !state.isStackTraceExpanded">
							<text>堆栈信息</text>
							<wd-icon
								:name="state.isStackTraceExpanded ? 'arrow-down' : 'arrow-right'"
								size="16px"
								color="var(--wot-icon-secondary)"
							/>
						</button>
						<view v-if="state.formData.stackTrace" class="log-content__copy">
							<wd-button size="small" variant="text" @click="handleCopy(state.formData.stackTrace)">复制</wd-button>
						</view>
					</view>
					<view v-if="state.isStackTraceExpanded" class="log-content__body">
						<text v-if="state.formData.stackTrace" class="log-content__text" selectable user-select>{{ state.formData.stackTrace }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>
				</view>

				<view class="log-content__section">
					<view class="log-content__header">
						<button class="log-content__toggle" hover-class="none" @click="state.isPureSqlExpanded = !state.isPureSqlExpanded">
							<text>纯Sql</text>
							<wd-icon :name="state.isPureSqlExpanded ? 'arrow-down' : 'arrow-right'" size="16px" color="var(--wot-icon-secondary)" />
						</button>
						<view v-if="state.formData.pureSql" class="log-content__copy">
							<wd-button size="small" variant="text" @click="handleCopy(state.formData.pureSql)">复制</wd-button>
						</view>
					</view>
					<view v-if="state.isPureSqlExpanded" class="log-content__body">
						<text v-if="state.formData.pureSql" class="log-content__text" selectable user-select>{{ state.formData.pureSql }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>
				</view>
			</view>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { copy, throttle, withDefineType } from "@fast-china/utils";
import type { SqlExceptionLogModel } from "@/api/services/Center/sqlExceptionLog/models/SqlExceptionLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevSqlExceptionLogDetail",
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<SqlExceptionLogModel>({}),
	isStackTraceExpanded: false,
	isPureSqlExpanded: false,
});

const handleCopy = async (content: string) => {
	try {
		await copy(content);
	} catch {
		uni.showToast({ title: "复制失败", icon: "none" });
	}
};

const detail = throttle(
	async (data: SqlExceptionLogModel) =>
		await popupRef.value.open(() => {
			state.formData = data;
			state.isStackTraceExpanded = false;
			state.isPureSqlExpanded = false;
		})
);

defineExpose({
	detail,
});
</script>

<style lang="scss" scoped>
.log-content {
	border-radius: 8px;
	background: var(--wot-filled-oppo);
}

.log-content__section + .log-content__section {
	border-top: 1px solid var(--wot-border-light);
}

.log-content__header {
	position: relative;
}

.log-content__toggle {
	display: flex;
	align-items: center;
	justify-content: space-between;
	width: 100%;
	min-height: 44px;
	margin: 0;
	padding: 0 12px;
	border: 0;
	background: transparent;
	color: var(--wot-text-main);
	font-size: var(--wot-typography-body-size-main);
	font-weight: var(--wot-font-weight-semibold);
	line-height: 1.5;
	text-align: left;

	&::after {
		border: 0;
	}
}

.log-content__copy {
	position: absolute;
	top: 50%;
	right: 32px;
	transform: translateY(-50%);
}

.log-content__body {
	padding: 0 10px 10px;
}

.log-content__text {
	display: block;
	padding: 12px;
	border: 1px solid var(--wot-border-light);
	border-radius: 8px;
	background: var(--wot-filled-bottom);
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

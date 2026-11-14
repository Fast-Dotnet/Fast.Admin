<template>
	<FaPopup ref="popupRef" title="异常日志详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="昵称" :value="state.formData.nickName" />
			<FaField label="手机" :value="state.formData.mobile" />
			<FaField label="异常信息" :value="state.formData.message" color="var(--wot-danger-main)" />
			<FaField label="类名" :value="state.formData.className" />
			<FaField label="方法名" :value="state.formData.methodName" />
			<FaField label="异常源" :value="state.formData.source" />
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
						<button class="log-content__toggle" hover-class="none" @click="state.isParamsExpanded = !state.isParamsExpanded">
							<text>参数对象</text>
							<wd-icon :name="state.isParamsExpanded ? 'arrow-down' : 'arrow-right'" size="16px" color="var(--wot-icon-secondary)" />
						</button>
						<view v-if="state.paramsText" class="log-content__copy">
							<wd-button size="small" variant="text" @click="handleCopy(state.paramsText)">复制</wd-button>
						</view>
					</view>
					<view v-if="state.isParamsExpanded" class="log-content__body">
						<text v-if="state.paramsText" class="log-content__text" selectable user-select>{{ state.paramsText }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>
				</view>
			</view>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { copy, withDefineType } from "@fast-china/utils";
import type { ExceptionLogModel } from "@/api/services/Center/exceptionLog/models/ExceptionLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevExceptionLogDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<ExceptionLogModel>({}),
	paramsText: "",
	isStackTraceExpanded: false,
	isParamsExpanded: false,
});

const handleCopy = async (content: string) => {
	try {
		await copy(content);
	} catch {
		uni.showToast({ title: "复制失败", icon: "none" });
	}
};

const detail = (data: ExceptionLogModel) => {
	popupRef.value.open(() => {
		state.formData = data;
		state.paramsText = data.paramsObj ?? "";
		if (state.paramsText) {
			try {
				state.paramsText = JSON.stringify(JSON.parse(state.paramsText), null, 2);
			} catch {
				// 参数不是合法 JSON 时保留原文。
			}
		}
		state.isStackTraceExpanded = false;
		state.isParamsExpanded = false;
	});
};

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

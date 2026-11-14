<template>
	<FaPopup ref="popupRef" title="请求日志详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="昵称" :value="state.formData.nickName" />
			<FaField label="手机" :value="state.formData.mobile" />
			<FaField label="成功">
				<FaTag name="BooleanEnum" :value="state.formData.isSuccess" />
			</FaField>
			<FaField label="操作名称" :value="state.formData.operationName" />
			<FaField label="操作方式">
				<FaTag name="HttpRequestActionEnum" :value="state.formData.operationAction" />
			</FaField>
			<FaField label="请求地址" :value="state.formData.location" />
			<FaField label="请求方式">
				<FaTag name="HttpRequestMethodEnum" :value="state.formData.requestMethod" />
			</FaField>
			<FaField label="耗时" :value="`${(Number(state.formData.elapsedTime ?? 0) / 1000).toFixed(4)}秒`" />
			<FaField label="类名" :value="state.formData.className" />
			<FaField label="方法名" :value="state.formData.methodName" />
			<FaField label="部门" :value="state.formData.departmentName" />
			<FaField label="地区" :value="`${state.formData.province} - ${state.formData.city}`" />
			<FaField label="Ip" :value="state.formData.ip" />
			<FaField label="请求时间" :value="state.formData.createdTime" />
			<FaField label="设备" :value="state.formData.device" />
			<FaField label="操作系统" :value="state.formData.os" />
			<FaField label="浏览器" :value="state.formData.browser" />
			<FaField label="租户" :value="state.formData.tenantName" />

			<view v-if="userInfoStore.isSuperAdmin || userInfoStore.isAdmin" class="log-content">
				<view class="log-content__section">
					<view class="log-content__header">
						<button class="log-content__toggle" hover-class="none" @click="state.isParamExpanded = !state.isParamExpanded">
							<text>请求参数</text>
							<wd-icon :name="state.isParamExpanded ? 'arrow-down' : 'arrow-right'" size="16px" color="var(--wot-icon-secondary)" />
						</button>
						<view v-if="state.paramText" class="log-content__copy">
							<wd-button size="small" variant="text" @click="handleCopy(state.paramText)">复制</wd-button>
						</view>
					</view>
					<view v-if="state.isParamExpanded" class="log-content__body">
						<text v-if="state.paramText" class="log-content__text" selectable user-select>{{ state.paramText }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>
				</view>

				<view v-if="state.resultText" class="log-content__section">
					<view class="log-content__header">
						<button class="log-content__toggle" hover-class="none" @click="state.isResultExpanded = !state.isResultExpanded">
							<text>返回结果</text>
							<wd-icon :name="state.isResultExpanded ? 'arrow-down' : 'arrow-right'" size="16px" color="var(--wot-icon-secondary)" />
						</button>
						<view class="log-content__copy">
							<wd-button size="small" variant="text" @click="handleCopy(state.resultText)">复制</wd-button>
						</view>
					</view>
					<view v-if="state.isResultExpanded" class="log-content__body">
						<text class="log-content__text" selectable user-select>{{ state.resultText }}</text>
					</view>
				</view>
			</view>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { AESDecrypt, copy, withDefineType } from "@fast-china/utils";
import { useUserInfo } from "@/stores";
import type { RequestLogModel } from "@/api/services/Center/requestLog/models/RequestLogModel";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemRequestLogDetail",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const userInfoStore = useUserInfo();

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<RequestLogModel>({}),
	paramText: "",
	resultText: "",
	isParamExpanded: false,
	isResultExpanded: false,
});

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === "object" && value !== null;

const formatLogContent = (content?: string, decrypt = false): string => {
	if (!content) return "";

	try {
		const result: unknown = JSON.parse(content);
		if (decrypt && isRecord(result) && isRecord(result.value)) {
			const { timestamp, data } = result.value;
			if ((typeof timestamp === "string" || typeof timestamp === "number") && typeof data === "string") {
				const timestampText = String(timestamp);
				try {
					const decoded = AESDecrypt(data, timestampText, `FIV${timestampText}`);
					if (decoded) {
						const value = decoded.parseJson<unknown>();
						result.value.data = typeof value === "string" && value.startsWith('"') && value.endsWith('"') ? value.slice(1, -1) : value;
					}
				} catch {
					// 历史响应无法解密时保留原始内容。
					return JSON.stringify(result, null, 2);
				}
			}
		}
		return JSON.stringify(result, null, 2);
	} catch {
		// 新日志的 result 保存异常原文，历史记录也可能包含非 JSON 文本。
		return content;
	}
};

const handleCopy = async (content: string) => {
	try {
		await copy(content);
	} catch {
		uni.showToast({ title: "复制失败", icon: "none" });
	}
};

const detail = (data: RequestLogModel) => {
	popupRef.value?.open(() => {
		state.formData = data;
		const canViewContent = userInfoStore.isSuperAdmin || userInfoStore.isAdmin;
		state.paramText = canViewContent ? formatLogContent(data.param) : "";
		state.resultText = canViewContent ? formatLogContent(data.result, true) : "";
		state.isParamExpanded = false;
		state.isResultExpanded = false;
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

<template>
	<FaPopup ref="popupRef" title="请求日志详情" height="70%" position="bottom" closable>
		<scroll-view scroll-y>
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

				<template v-if="userInfoStore.isSuperAdmin || userInfoStore.isAdmin">
					<view>
						<view class="log-content__header">
							<text class="log-content__title">请求参数</text>
							<wd-button v-if="paramText" size="small" variant="text" @click="copy(paramText)">复制</wd-button>
						</view>
						<text v-if="paramText" class="log-content__text" selectable>{{ paramText }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>

					<view v-if="resultText">
						<view class="log-content__header">
							<text class="log-content__title">返回结果</text>
							<wd-button v-if="resultText" size="small" variant="text" @click="copy(resultText)">复制</wd-button>
						</view>
						<text v-if="resultText" class="log-content__text" selectable>{{ resultText }}</text>
						<text v-else class="log-content__empty">暂无内容</text>
					</view>
				</template>
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue";
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

const paramText = computed(() => formatLogContent(state.formData.param));
const resultText = computed(() => formatLogContent(state.formData.result, true));

const detail = (data: RequestLogModel) => {
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

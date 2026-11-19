<template>
	<view class="page page-detail">
		<FaLoading mask :loading="state.loading" />
		<wd-empty v-if="state.loading || state.loadFailed" icon="no-content" tip="暂无内容" />
		<template v-else>
			<wd-cell-group border>
				<wd-cell title="收件人">
					<text selectable user-select>{{ state.formData.receiver }}</text>
					<template #suffix>
						<button
							v-if="state.formData.receiver != null && state.formData.receiver !== ''"
							class="fa-field__copy"
							hover-class="none"
							@click.stop="handleCopy(state.formData.receiver)"
						>
							<wd-icon name="copy" size="14px" />
							<text>复制</text>
						</button>
					</template>
				</wd-cell>
				<wd-cell title="渠道">
					<FaText name="MessageSendChannelEnum" :value="state.formData.channel" />
				</wd-cell>
				<wd-cell title="标题">
					<text selectable user-select>{{ state.formData.title }}</text>
				</wd-cell>
				<wd-cell title="发送结果">
					<FaTag name="BooleanEnum" :value="state.formData.isSuccess" />
				</wd-cell>
				<wd-cell title="发送人">
					<text selectable user-select>{{ state.formData.createdUserName }}</text>
				</wd-cell>
				<wd-cell title="发送时间">
					<text selectable user-select>{{ state.formData.createdTime }}</text>
				</wd-cell>
				<wd-cell title="省份">
					<text selectable user-select>{{ state.formData.province }}</text>
				</wd-cell>
				<wd-cell title="城市">
					<text selectable user-select>{{ state.formData.city }}</text>
				</wd-cell>
				<wd-cell title="Ip">
					<text selectable user-select>{{ state.formData.ip }}</text>
				</wd-cell>
				<wd-cell title="设备">
					<text selectable user-select>{{ state.formData.device }}</text>
				</wd-cell>
				<wd-cell title="操作系统">
					<text selectable user-select>{{ state.formData.os }}</text>
				</wd-cell>
				<wd-cell title="浏览器">
					<text selectable user-select>{{ state.formData.browser }}</text>
				</wd-cell>
			</wd-cell-group>

			<view class="message-content__header">
				<text class="message-content__title">消息内容</text>
				<button
					v-if="state.formData.channel === MessageSendChannelEnum.Email ? mailContent : state.contentText"
					class="fa-field__copy"
					hover-class="none"
					@click.stop="handleCopyMessage"
				>
					<wd-icon name="copy" size="14px" />
					<text>复制</text>
				</button>
			</view>
			<wd-rich-text
				v-if="state.formData.channel === MessageSendChannelEnum.Email"
				ref="mailContentRef"
				:content="mailContent"
				:tag-style="mailTagStyle"
				:class-styles="mailClassStyles"
				custom-style="padding: 0 12px 12px;"
				selectable
			>
				<text class="log-content__empty">暂无可预览内容</text>
			</wd-rich-text>
			<text v-else-if="state.contentText" class="log-content__text" selectable user-select>{{ state.contentText }}</text>
			<text v-else class="log-content__empty">暂无内容</text>
		</template>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { computed, reactive, ref } from "vue";
import { copy, withDefineType } from "@fast-china/utils";
import { MessageSendChannelEnum } from "@/api/enums/MessageSendChannelEnum";
import { messageSendRecordApi } from "@/api/services/Center/messageSendRecord";
import { useMessageBox } from "@/hooks";
import type { RichTextInstance } from "@wot-ui/ui/components/wd-rich-text/types";
import type { QueryMessageSendRecordDetailOutput } from "@/api/services/Center/messageSendRecord/models/QueryMessageSendRecordDetailOutput";

definePage({
	name: "DevMessageSendRecordDetail",
	authCode: "MessageSendRecord:Detail",
	style: {
		navigationBarTitleText: "消息详情",
		enablePullDownRefresh: true,
	},
});

const mailContentRef = ref<RichTextInstance>();

const state = reactive({
	loading: false,
	loadFailed: false,
	formData: withDefineType<QueryMessageSendRecordDetailOutput>(),
	recordId: withDefineType<string>(),
	contentText: "",
});

const mailTagStyle = {
	p: "margin: 0 0 12px;",
	pre: "margin: 0 0 12px; padding: 12px; white-space: pre-wrap; overflow-wrap: anywhere; background-color: var(--wot-filled-bottom); border-radius: 8px;",
};

const mailClassStyles = {
	warn: "color: var(--wot-warning-main); font-weight: 700;",
	error: "color: var(--wot-danger-main); font-weight: 700;",
};

const mailContent = computed(() => {
	if (state.formData.channel !== MessageSendChannelEnum.Email) return undefined;

	const html = state.formData.recordValue?.trim() ?? "";
	if (!/<(?:!doctype\s+html\b|(?:html|body|div|p|pre|table|br|span|a|img|h[1-6]|ul|ol|blockquote|strong|em|b|i|hr)\b)/i.test(html)) {
		return undefined;
	}

	// 按 td 层级提取邮件正文，保留正文内的嵌套表格。
	const tdPattern = /<!--[\s\S]*?-->|<\/?td\b(?:"[^"]*"|'[^']*'|[^"'<>])*>/gi;
	let start = -1;
	let depth = 0;

	for (const match of html.matchAll(tdPattern)) {
		const tag = match[0];
		if (tag.startsWith("<!--")) continue;

		if (start < 0) {
			const classMatch = /\sclass\s*=\s*(?:"([^"]*)"|'([^']*)')/i.exec(tag);
			const classes = classMatch?.[1] ?? classMatch?.[2] ?? "";
			if (!classes.split(/\s+/).includes("mail-content")) continue;

			start = match.index + tag.length;
			depth = 1;
		} else if (tag.startsWith("</")) {
			depth--;
			if (depth === 0) return html.slice(start, match.index).trim();
		} else {
			depth++;
		}
	}

	return (/<body\b[^>]*>([\s\S]*?)<\/body\s*>/i.exec(html)?.[1] ?? html).trim();
});

const handleCopy = async (content: string) => {
	try {
		await copy(content);
	} catch {
		uni.showToast({ title: "复制失败", icon: "none" });
	}
};

const handleCopyMessage = () => {
	const content = state.formData.channel === MessageSendChannelEnum.Email ? mailContentRef.value?.getText() : state.contentText;
	return handleCopy(content);
};

const handleLoad = async () => {
	state.loading = true;
	state.loadFailed = false;
	try {
		const apiRes = await messageSendRecordApi.queryMessageSendRecordDetail(state.recordId);
		state.formData = apiRes;
		state.contentText = "";
		if (apiRes.channel !== MessageSendChannelEnum.Email && apiRes.recordValue) {
			state.contentText = apiRes.recordValue;
			try {
				state.contentText = JSON.stringify(JSON.parse(apiRes.recordValue), null, 2);
			} catch {
				// 内容不是合法 JSON 时保留原文。
			}
		}
		uni.setNavigationBarTitle({ title: `消息详情 - ${apiRes.receiver}` });
	} catch {
		state.loadFailed = true;
	} finally {
		state.loading = false;
	}
};

onLoad((options: { recordId?: string }) => {
	if (!options?.recordId) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.recordId = options.recordId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

<style lang="scss" scoped>
.message-content__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding: 12px;
}

.message-content__title {
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

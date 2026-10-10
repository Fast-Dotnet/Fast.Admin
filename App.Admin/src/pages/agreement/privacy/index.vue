<template>
	<view class="page">
		<view class="page__header">
			<view class="page__title">{{ appStore.appName }} 隐私政策</view>
			<view class="page__dates">
				<text>更新日期：2025年01月01日</text>
				<text>生效日期：2025年01月01日</text>
			</view>
		</view>
		<view class="page__notice"> 请在使用 {{ appStore.appName }} 前仔细阅读本政策。开始使用即表示您已阅读、理解并同意本政策的相关内容。 </view>
		<view v-for="(section, index) in sections" :key="section.title" class="page__section">
			<view class="page__section-title">{{ index + 1 }}. {{ section.title }}</view>
			<view v-for="paragraph in section.paragraphs" :key="paragraph" class="page__paragraph">{{ paragraph }}</view>
			<view v-for="item in section.items" :key="item" class="page__item">
				<text class="page__bullet">•</text>
				<text>{{ item }}</text>
			</view>
		</view>
	</view>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useApp } from "@/stores";

definePage({
	name: "PrivacyAgreement",
	layout: "layout",
	noLogin: true,
	style: { navigationBarTitleText: "隐私政策" },
});

const appStore = useApp();
const sections = computed(() => [
	{
		title: "适用范围与基本原则",
		paragraphs: [
			`本政策适用于您通过移动应用、小程序或网页使用 ${appStore.appName} 提供的服务。我们遵循合法、正当、必要和诚信原则，仅在实现产品功能所需的范围内处理个人信息。`,
			`具体服务由部署和运营 ${appStore.appName} 的组织提供。该组织可能根据实际业务另行公示补充规则；补充规则与本政策不一致时，以更有利于保护您权益的约定为准。`,
		],
	},
	{
		title: "我们如何收集和使用信息",
		paragraphs: ["为保障账号登录、组织协作、业务办理和运行安全，我们可能处理以下必要信息："],
		items: [
			"账号与身份信息：账号、昵称、头像、联系方式，以及所属组织、部门、岗位和权限信息，用于身份验证、账号展示与访问控制。",
			"业务操作信息：您主动提交、查询或处理的表单、文件、审批、反馈等业务数据，用于完成您所选择的功能。",
			"设备与日志信息：设备类型、操作系统、应用版本、网络状态、安装标识、访问时间和异常日志，用于保障服务安全、定位故障和改善体验。",
			"授权信息：仅在您使用相应功能时申请相机、相册、文件、定位、麦克风或通知等系统权限；拒绝授权不会影响与该权限无关的功能。",
		],
	},
	{
		title: "本地存储与缓存",
		paragraphs: [
			`${appStore.appName} 可能使用 Cookie、本地存储或平台缓存保存登录状态、主题偏好和必要配置，以保证服务连续性并减少重复操作。您可通过系统设置或应用提供的清理功能删除缓存，但部分功能可能需要重新登录或重新配置。`,
		],
	},
	{
		title: "信息共享、转移与公开披露",
		paragraphs: [
			"我们不会出售您的个人信息。仅在取得您的授权、履行合同所必需、遵守法律义务或保护重大合法权益的情况下，向必要的关联方、服务提供方或主管机关提供信息。",
			"如发生合并、分立、资产转让或运营主体变更，我们会要求新的处理方继续履行本政策，或重新取得您的同意。除法律另有规定外，我们不会公开披露您的个人信息。",
		],
	},
	{
		title: "信息存储与安全保护",
		paragraphs: [
			"个人信息将在实现处理目的所需的最短期限内保存；法律法规或业务审计另有要求的，按规定期限保存。超过保存期限后，我们将依法删除或匿名化处理。",
			"我们采用访问控制、传输保护、日志审计、备份和安全监测等措施降低信息泄露、篡改或丢失风险。互联网环境并非绝对安全，请您妥善保管账号、密码和验证码。",
		],
	},
	{
		title: "您的权利",
		paragraphs: ["在适用法律规定的范围内，您可以通过应用功能或联系运营方行使以下权利："],
		items: [
			"查询、复制、更正或补充您的个人信息。",
			"删除信息、撤回授权或注销账号；撤回不影响此前基于授权进行处理的合法性。",
			"对个人信息处理规则进行解释说明，或对处理结果提出异议和投诉。",
		],
	},
	{
		title: "未成年人保护",
		paragraphs: [
			`${appStore.appName} 主要面向具备相应业务权限的组织用户。未成年人应在监护人指导和同意下使用服务。若我们发现未经有效同意处理了未成年人的个人信息，将依法及时处理。`,
		],
	},
	{
		title: "政策更新与联系我们",
		paragraphs: [
			"我们可能根据功能变化或法律要求更新本政策，并通过页面提示、通知或其他合理方式告知您。重大变化将在生效前以显著方式提示。",
			`如对本政策或个人信息保护有疑问、意见或请求，请通过 ${appStore.appName} 内的“意见反馈”或运营方公开的联系方式联系我们。我们将在核实身份后依法处理。`,
		],
	},
]);
</script>

<style scoped lang="scss">
.page {
	padding: var(--fa-page-padding);
}

.page__header {
	padding-bottom: 30rpx;
	border-bottom: 1rpx solid var(--wot-divider-strong);
}

.page__title {
	font-size: 40rpx;
	font-weight: var(--wot-font-weight-semibold);
	line-height: 1.4;
	color: var(--wot-text-main);
}

.page__dates {
	display: flex;
	flex-direction: column;
	gap: 6rpx;
	margin-top: 16rpx;
	font-size: 23rpx;
	line-height: 1.5;
	color: var(--wot-text-auxiliary);
}

.page__notice {
	margin: 28rpx 0 36rpx;
	padding: 22rpx 24rpx;
	font-size: 26rpx;
	line-height: 1.75;
	color: var(--wot-text-secondary);
	background-color: var(--wot-primary-1);
	border-left: 6rpx solid var(--wot-primary-6);
	border-radius: 12rpx;
}

.page__section + .page__section {
	margin-top: 36rpx;
}

.page__section-title {
	margin-bottom: 16rpx;
	font-size: 30rpx;
	font-weight: var(--wot-font-weight-semibold);
	line-height: 1.5;
	color: var(--wot-text-main);
}

.page__paragraph,
.page__item {
	font-size: 27rpx;
	line-height: 1.85;
	text-align: justify;
}

.page__paragraph + .page__paragraph,
.page__item + .page__item,
.page__paragraph + .page__item {
	margin-top: 12rpx;
}

.page__item {
	display: flex;
	align-items: flex-start;
}

.page__bullet {
	flex: none;
	margin-right: 14rpx;
	color: var(--wot-primary-6);
}
</style>

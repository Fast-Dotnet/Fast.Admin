<template>
	<view class="page">
		<view class="page__header">
			<view class="page__title">{{ appStore.appName }} 服务协议</view>
			<view class="page__dates">
				<text>更新日期：2025年01月01日</text>
				<text>生效日期：2025年01月01日</text>
			</view>
		</view>
		<view class="page__notice"> 请在使用 {{ appStore.appName }} 前仔细阅读本协议。开始使用即表示您已阅读、理解并同意本协议的相关内容。 </view>
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
	name: "ServiceAgreement",
	layout: "layout",
	noLogin: true,
	style: { navigationBarTitleText: "服务协议" },
});

const appStore = useApp();
const sections = computed(() => [
	{
		title: "协议范围",
		paragraphs: [
			`本协议是您与 ${appStore.appName} 运营方之间关于注册、登录和使用服务所订立的协议。您所在组织配置的业务规则、权限规则和补充条款也是本协议的组成部分。`,
		],
	},
	{
		title: "账号注册与安全",
		paragraphs: [
			"您应使用真实、合法、有效的信息开通和使用账号，并及时更新发生变化的信息。账号仅限本人或经组织授权的人员使用。",
			"您应妥善保管账号、密码、验证码和设备，不得出借、转让或共享。发现异常登录、凭据泄露或未经授权的操作时，应立即修改凭据并联系管理员或运营方。",
		],
	},
	{
		title: "服务内容与使用方式",
		paragraphs: [
			`${appStore.appName} 根据运营方和您所在组织的配置提供信息展示、业务办理、协作管理及相关辅助功能。实际可用功能、数据范围和操作权限以账号授权为准。`,
			"部分服务可能依赖网络、操作系统、小程序平台或第三方服务。为提升安全性和稳定性，我们可能更新客户端、调整功能或安排维护，并在合理范围内提供提示。",
		],
	},
	{
		title: "用户行为规范",
		paragraphs: [`使用 ${appStore.appName} 时，您不得从事以下行为：`],
		items: [
			"违反法律法规、公序良俗或侵犯他人合法权益。",
			"超越授权范围访问、复制、导出、传播或修改数据。",
			"上传恶意代码，干扰服务运行，绕过安全机制，或未经许可进行扫描、测试和逆向分析。",
			"冒用他人身份、伪造信息，或利用服务实施欺诈、骚扰和其他不当行为。",
		],
	},
	{
		title: "用户内容与数据",
		paragraphs: [
			"您应确保提交的内容来源合法、真实准确，并已取得必要授权。您保留依法享有的内容权利，同时授权运营方在提供、维护和改进服务所必需的范围内处理相关内容。",
			"组织管理员可能依据内部管理要求访问、配置、留存或删除组织数据。您与所在组织之间的数据权属和管理争议，应依据双方约定解决。",
		],
	},
	{
		title: "知识产权",
		paragraphs: [
			`${appStore.appName} 的软件、界面、标识、文档及相关技术成果受法律保护。未经权利人书面许可，您不得擅自复制、修改、出租、出售、分发或用于本协议以外的目的。`,
		],
	},
	{
		title: "服务变更、中止与终止",
		paragraphs: [
			"因维护升级、安全风险、不可抗力、法律要求或第三方服务变化，部分服务可能短暂中断。我们将尽合理努力降低影响。",
			"如您违反本协议、组织授权到期或账号存在安全风险，运营方或组织管理员可依法采取限制功能、暂停或终止服务等措施。账号终止后的数据将按法律规定和组织规则处理。",
		],
	},
	{
		title: "责任范围",
		paragraphs: [
			"运营方将在法律允许的范围内保障服务稳定和数据安全。因不可抗力、基础通信故障、平台限制、用户自身原因或经合理安全措施仍无法避免的风险造成的损失，责任按适用法律和双方约定确定。",
		],
	},
	{
		title: "协议更新与争议解决",
		paragraphs: [
			"我们可能因服务或法律变化更新本协议，并通过合理方式提示。您继续使用服务即表示接受更新后的协议；如不同意，可停止使用并按规定申请注销账号。",
			"本协议的订立、履行和解释适用中华人民共和国法律。发生争议时，双方应先友好协商；协商不成的，依法向有管辖权的人民法院提起诉讼。",
		],
	},
	{
		title: "联系我们",
		paragraphs: [`如对本协议有疑问或需要投诉、反馈，请通过 ${appStore.appName} 内的“意见反馈”或运营方公开的联系方式联系我们。`],
	},
]);
</script>

<style lang="scss" scoped>
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

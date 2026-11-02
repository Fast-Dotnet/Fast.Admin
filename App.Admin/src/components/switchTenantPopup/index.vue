<template>
	<FaPopup ref="popupRef" height="70%" position="bottom" :z-index="2001">
		<view class="workspace-popup">
			<view class="workspace-popup__header">
				<text class="workspace-popup__title">切换工作空间</text>
				<wd-button type="primary" variant="text" size="small" @click="popupRef?.close()">取消</wd-button>
			</view>
			<scroll-view class="tenant-list" scroll-y>
				<view class="tenant-list__content">
					<view v-for="item in tenantList" :key="item.userKey" class="page-card tenant-card" @click="handleTenantLogin(item)">
						<image class="tenant-card__image" :src="item.logoUrl" mode="aspectFill" />
						<view class="tenant-card__content">
							<view class="tenant-card__name">{{ item.tenantName }}</view>
							<view class="tenant-card__meta">{{ item.departmentName || "无部门" }} · {{ item.employeeName }}</view>
						</view>
						<FaTag size="small" name="EditionEnum" :value="item.edition" />
						<wd-icon v-if="item.userKey === userInfoStore.userKey" name="check" size="28rpx" color="var(--wot-primary-6)" />
						<wd-icon v-else name="arrow-right" size="28rpx" color="var(--wot-text-auxiliary)" />
					</view>
				</view>
			</scroll-view>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Local } from "@fast-china/utils";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { useToast } from "@/hooks";
import { useUserInfo } from "@/stores";
import type { LoginTenantOutput } from "@/api/services/Auth/login/models/LoginTenantOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SwitchTenantPopup",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const emit = defineEmits<{
	/** 工作空间改变触发 */
	change: [data: LoginTenantOutput];
}>();

const userInfoStore = useUserInfo();

const popupRef = ref<FaPopupInstance>();
const tenantList = ref<LoginTenantOutput[]>([]);

/** 处理租户登录 */
const handleTenantLogin = (data: LoginTenantOutput) => {
	const { userKey } = userInfoStore;
	if (data.userKey === userKey) return;
	popupRef.value.close(async () => {
		const loginRes = await loginApi.tryLogin({ userKey: data.userKey });
		if (loginRes.status !== LoginStatusEnum.Success) {
			useToast.error(loginRes.message);
			return;
		}

		userInfoStore.login(loginRes);
		useToast.success(`切换工作空间【${data.tenantName}】成功`);

		// 删除 HTTP 缓存数据
		Local.removeByPrefix("HTTP_CACHE_");
		// 刷新用户信息
		await userInfoStore.refreshUserInfo();

		emit("change", data);
	});
};

const open = () => {
	popupRef.value.open(async () => {
		tenantList.value = await loginApi.queryLoginUser();
	});
};

defineExpose({
	open,
});
</script>

<style lang="scss" scoped>
.workspace-popup {
	display: flex;
	flex-direction: column;
	height: 100%;
}

.workspace-popup__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding: 16rpx 24rpx;
}

.workspace-popup__title {
	font-size: 32rpx;
	font-weight: var(--wot-font-weight-semibold);
}

.tenant-list {
	flex: 1;
	min-height: 0;
	height: 0;
}

.tenant-list__content {
	display: flex;
	flex-direction: column;
	gap: 24rpx;
	padding: 8rpx 24rpx 24rpx;
}

.tenant-card {
	display: flex;
	gap: 18rpx;
	align-items: center;
}

.tenant-card__image {
	flex: none;
	width: 84rpx;
	height: 84rpx;
	border-radius: 18rpx;
	box-shadow: var(--fa-box-shadow-light);
}

.tenant-card__content {
	flex: 1;
	overflow: hidden;
}

.tenant-card__name {
	font-weight: var(--wot-font-weight-semibold);
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.tenant-card__meta {
	margin-top: 6rpx;
	color: var(--wot-text-auxiliary);
	font-size: 24rpx;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}
</style>

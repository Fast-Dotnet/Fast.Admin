<template>
	<FaPopup ref="popupRef" title="切换工作空间" height="70%" position="bottom" closable>
		<view v-for="item in tenantList" :key="item.userKey" class="fa-tenant-card" @click="handleTenantLogin(item)">
			<image class="fa-tenant-card__image" :src="item.logoUrl" mode="aspectFill" />
			<view class="fa-tenant-card__content">
				<view class="fa-tenant-card__name">{{ item.tenantName }}</view>
				<view class="fa-tenant-card__meta">{{ item.departmentName || "无部门" }} · {{ item.employeeName }}</view>
			</view>
			<FaTag size="small" name="EditionEnum" :value="item.edition" />
			<wd-icon v-if="item.userKey === userInfoStore.userKey" name="check" size="28rpx" color="var(--wot-primary-6)" />
			<wd-icon v-else name="arrow-right" size="28rpx" color="var(--wot-text-auxiliary)" />
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

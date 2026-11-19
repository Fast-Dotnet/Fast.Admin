<template>
	<FaPopup ref="faPopupRef" title="账号详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="昵称" :value="formData.nickName" selectable />
			<FaField label="手机" :value="formData.mobile" copyable />
			<FaField label="邮箱" :value="formData.email" copyable />
			<FaField label="状态" center>
				<FaTag name="CommonStatusEnum" :value="formData.status" />
			</FaField>
			<FaField label="头像">
				<FaImage :src="formData.avatar" width="150rpx" height="150rpx" />
			</FaField>
			<FaField label="创建时间" :value="formData.createdTime" selectable />
			<FaField label="更新时间" :value="formData.updatedTime" selectable />

			<wd-divider content-position="left">初次登录信息</wd-divider>
			<FaField label="租户" :value="formData.firstLoginTenantName" selectable />
			<FaField label="省份" :value="formData.firstLoginProvince" selectable />
			<FaField label="城市" :value="formData.firstLoginCity" selectable />
			<FaField label="Ip" :value="formData.firstLoginIp" selectable />
			<FaField label="设备" :value="formData.firstLoginDevice" selectable />
			<FaField label="操作系统" :value="formData.firstLoginOS" selectable />
			<FaField label="浏览器" :value="formData.firstLoginBrowser" selectable />
			<FaField label="时间" :value="formData.firstLoginTime" selectable />

			<wd-divider content-position="left">最后登录信息</wd-divider>
			<FaField label="租户" :value="formData.lastLoginTenantName" selectable />
			<FaField label="省份" :value="formData.lastLoginProvince" selectable />
			<FaField label="城市" :value="formData.lastLoginCity" selectable />
			<FaField label="Ip" :value="formData.lastLoginIp" selectable />
			<FaField label="设备" :value="formData.lastLoginDevice" selectable />
			<FaField label="操作系统" :value="formData.lastLoginOS" selectable />
			<FaField label="浏览器" :value="formData.lastLoginBrowser" selectable />
			<FaField label="时间" :value="formData.lastLoginTime" selectable />

			<wd-divider content-position="left">锁定信息</wd-divider>
			<FaField label="锁定" center>
				<wd-tag v-if="formData.lockStartTime" type="danger">已锁定</wd-tag>
				<wd-tag v-else>未锁定</wd-tag>
			</FaField>
			<template v-if="formData.lockStartTime">
				<FaField label="错误次数" :value="`${formData.passwordErrorTime}次`" selectable />
				<FaField label="锁定时间" :value="`${formData.lockStartTime} ~ ${formData.lockEndTime}`" selectable />
			</template>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { accountApi } from "@/api/services/Center/account";
import type { QueryAccountDetailOutput } from "@/api/services/Center/account/models/QueryAccountDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemAccountDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryAccountDetailOutput>({});

const detail = throttle(
	async (accountId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await accountApi.queryAccountDetail(accountId);
		})
);

defineExpose({
	detail,
});
</script>

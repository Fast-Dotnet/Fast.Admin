<template>
	<FaPaging page-key="ClientUser" :request-api="clientUserApi.queryClientUserPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaApplicationSelect v-model="searchParam.value.appId" />
					<FaCellPicker title="性别" v-model="searchParam.value.sex" name="GenderEnum" radio clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.userId" @click="detailFormRef.detail(item)">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 10rpx">
					<FaAvatar :src="item.avatar" size="88rpx" />
					<view style="flex: 1; display: flex; flex-direction: column">
						<text
							style="
								color: var(--wot-text-main);
								font-size: var(--wot-typography-body-size-large);
								font-weight: var(--wot-font-weight-semibold);
							"
						>
							{{ item.nickName }}
						</text>
						<text v-if="item.mobile">{{ item.mobile }}</text>
						<text v-else style="color: var(--wot-text-placeholder)">未授权手机号</text>
					</view>
					<FaTag name="GenderEnum" :value="item.sex" />
				</view>

				<FaField label="地区" :value="[item.lastLoginProvince, item.lastLoginCity].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.lastLoginIp" />
				<FaField label="设备" :value="[item.lastLoginDevice, item.lastLoginOS].filter(Boolean).join(' · ')" />
				<FaField label="浏览器" :value="item.lastLoginBrowser" />
				<FaField label="登录时间" :value="item.lastLoginTime" />
			</FaCard>
		</template>
	</FaPaging>

	<ClientUserDetail ref="detailFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { clientUserApi } from "@/api/services/Center/clientUser";
import ClientUserDetail from "./components/detail.vue";

definePage({
	name: "SystemClientUser",
	authCode: "ClientUser:Paged",
	style: {
		navigationBarTitleText: "客户端用户",
	},
});

const detailFormRef = ref<InstanceType<typeof ClientUserDetail>>();
</script>

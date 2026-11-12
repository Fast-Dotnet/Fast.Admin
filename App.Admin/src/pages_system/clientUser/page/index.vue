<template>
	<FaPaging :request-api="clientUserApi.queryClientUserPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaApplicationSelect v-model="searchParam.value.appId" />
					<FaCellPicker title="性别" v-model="searchParam.value.sex" name="GenderEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.userId">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 24rpx">
					<FaImage :src="item.avatar" width="88rpx" height="88rpx" original round />
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

				<FaField label="OpenId" :value="item.openId" />
				<FaField label="UnionId" :value="item.unionId" />
				<FaField label="地区" :value="`${item.lastLoginProvince} - ${item.lastLoginCity}`" />
				<FaField label="Ip" :value="item.lastLoginIp" />
				<FaField label="设备" :value="item.lastLoginDevice" />
				<FaField label="操作系统" :value="item.lastLoginOS" />
				<FaField label="浏览器" :value="item.lastLoginBrowser" />
				<FaField label="手机号更新" :value="item.mobileUpdateTime" />
				<FaField label="登录时间" :value="item.lastLoginTime" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新时间" :value="item.updatedTime" />
			</FaCard>
		</template>
	</FaPaging>
</template>
<script setup lang="ts">
import { clientUserApi } from "@/api/services/Center/clientUser";

definePage({
	name: "SystemClientUser",
	authCode: "ClientUser:Paged",
	style: {
		navigationBarTitleText: "客户端用户",
	},
});
</script>

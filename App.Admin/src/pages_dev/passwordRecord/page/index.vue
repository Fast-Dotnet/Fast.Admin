<template>
	<FaPaging :request-api="passwordRecordApi.queryPasswordRecordPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past3M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #filter>
					<FaAccountSelect v-model="searchParam.value.accountId" />
					<FaCellPicker title="操作类型" v-model="searchParam.value.operationType" name="PasswordOperationTypeEnum" radio />
					<FaCellPicker title="类型" v-model="searchParam.value.type" name="PasswordTypeEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime">
				<template #titleExtra>
					<FaTag name="PasswordOperationTypeEnum" :value="item.operationType" />
				</template>

				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 24rpx">
					<FaAvatar :src="item.avatar" size="88rpx" original />
					<view style="display: flex; flex-direction: column">
						<text
							style="
								color: var(--wot-text-main);
								font-size: var(--wot-typography-body-size-large);
								font-weight: var(--wot-font-weight-semibold);
							"
						>
							{{ item.nickName }}
						</text>
						<text>{{ item.mobile }}</text>
					</view>
				</view>
				<FaField label="账号Key" :value="item.accountKey" />
				<FaField label="邮箱" :value="item.email" />
				<FaField label="类型">
					<FaTag name="PasswordTypeEnum" :value="item.type" />
				</FaField>
			</FaCard>
		</template>
	</FaPaging>
</template>
<script setup lang="ts">
import { passwordRecordApi } from "@/api/services/Center/passwordRecord";

definePage({
	name: "DevPasswordRecord",
	authCode: "PasswordRecord:Paged",
	style: {
		navigationBarTitleText: "密码记录",
	},
});
</script>

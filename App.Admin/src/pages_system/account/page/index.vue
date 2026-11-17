<template>
	<FaPaging ref="faPagingRef" page-key="Account" :request-api="accountApi.queryAccountPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()" />
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.accountId" code="Account:Detail" @click="detailFormRef?.detail(item.accountId)">
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
						<text>{{ item.mobile }}</text>
					</view>
					<FaTag name="CommonStatusEnum" :value="item.status" />
				</view>

				<FaField label="邮箱" :value="item.email" />
				<FaField label="登录租户" :value="item.lastLoginTenantName" />
				<FaField label="登录地区" :value="[item.lastLoginProvince, item.lastLoginCity].filter(Boolean).join(' - ')" />
				<FaField label="登录Ip" :value="item.lastLoginIp" />
				<FaField label="登录设备" :value="[item.lastLoginDevice, item.lastLoginOS].filter(Boolean).join(' · ')" />
				<FaField label="登录浏览器" :value="item.lastLoginBrowser" />
				<FaField label="登录时间" :value="item.lastLoginTime" />
				<FaField label="锁定信息" center>
					<wd-tag v-if="item.isLock" type="danger">已锁定</wd-tag>
					<wd-tag v-else>未锁定</wd-tag>
				</FaField>
				<template v-if="item.isLock">
					<FaField label="错误次数" :value="`${item.passwordErrorTime}次`" />
					<FaField label="锁定结束时间" :value="item.lockEndTime" />
				</template>

				<template #footer>
					<FaAuth code="Account:ResetPassword">
						<wd-button size="mini" type="warning" variant="plain" @click="handleResetPassword(item)">重置密码</wd-button>
					</FaAuth>
					<FaAuth v-if="item.isLock" code="Account:Unlock">
						<wd-button size="mini" type="warning" variant="plain" @click="handleUnlock(item)">解锁</wd-button>
					</FaAuth>
					<FaAuth code="Account:Status">
						<wd-button
							v-if="item.status === CommonStatusEnum.Enable"
							size="mini"
							type="danger"
							variant="plain"
							@click="handleChangeStatus(item)"
						>
							禁用
						</wd-button>
						<wd-button v-else size="mini" type="warning" variant="plain" @click="handleChangeStatus(item)">启用</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<AccountDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { accountApi } from "@/api/services/Center/account";
import { useMessageBox, useToast } from "@/hooks";
import AccountDetail from "./components/detail.vue";
import type { QueryAccountPagedOutput } from "@/api/services/Center/account/models/QueryAccountPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemAccount",
	authCode: "Account:Paged",
	style: {
		navigationBarTitleText: "账号管理",
	},
});

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof AccountDetail>>();

const handleResetPassword = throttle(async (item: QueryAccountPagedOutput) => {
	const { accountId, rowVersion } = item;
	await useMessageBox.confirm("确定重置密码？", {
		async beforeConfirm() {
			await accountApi.resetPassword({ accountId, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});

const handleUnlock = throttle(async (item: QueryAccountPagedOutput) => {
	const { accountId, rowVersion } = item;
	await useMessageBox.confirm("确定解锁账号？", {
		async beforeConfirm() {
			await accountApi.unlock({ accountId, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});

const handleChangeStatus = throttle(async (item: QueryAccountPagedOutput) => {
	const { accountId, status, rowVersion } = item;
	await useMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}账号？`, {
		async beforeConfirm() {
			await accountApi.changeStatus({ accountId, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

<template>
	<FaPaging :request-api="tenantApi.queryTenantPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="状态" v-model="searchParam.value.status" name="CommonStatusEnum" radio />
					<FaCellPicker title="版本" v-model="searchParam.value.edition" name="EditionEnum" radio />
					<FaCellPicker title="租户类型" v-model="searchParam.value.tenantType" name="TenantTypeEnum" radio />
					<wd-cell title="管理员手机" center>
						<wd-input
							custom-class="is-compact"
							v-model="searchParam.value.adminMobile"
							clearable
							type="tel"
							:maxlength="11"
							placeholder="请输入管理员手机"
						/>
					</wd-cell>
					<wd-cell title="管理员邮箱" center>
						<wd-input
							custom-class="is-compact"
							v-model="searchParam.value.adminEmail"
							clearable
							:maxlength="50"
							placeholder="请输入管理员邮箱"
						/>
					</wd-cell>
				</template>
				<template #footer>
					<FaAuth code="Tenant:Add">
						<wd-button size="small" icon="plus" @click="handleOpen('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard v-for="item in pagedList" :key="item.tenantId" code="Tenant:Detail" @click="handleOpen('detail', item.tenantId)">
				<view style="display: flex; align-items: center; gap: 20rpx; margin-bottom: 24rpx">
					<FaImage :src="item.logoUrl" width="88rpx" height="88rpx" original round />
					<view style="flex: 1; display: flex; flex-direction: column">
						<text
							style="
								color: var(--wot-text-main);
								font-size: var(--wot-typography-body-size-large);
								font-weight: var(--wot-font-weight-semibold);
							"
						>
							{{ item.tenantName }}
						</text>
						<text>{{ item.tenantNo }}</text>
					</view>
					<FaTag name="EditionEnum" :value="item.edition" />
				</view>

				<FaField label="租户类型">
					<FaText name="TenantTypeEnum" :value="item.tenantType" />
				</FaField>
				<FaField label="租户编码" :value="item.tenantCode" />
				<FaField label="状态">
					<FaTag name="CommonStatusEnum" :value="item.status" />
				</FaField>
				<FaField label="管理员" :value="item.adminName" />
				<FaField label="管理员手机" :value="item.adminMobile" />
				<FaField label="管理员邮箱" :value="item.adminEmail" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Tenant:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleOpen('edit', item.tenantId)">编辑</wd-button>
					</FaAuth>
					<FaAuth v-if="item.tenantType !== TenantTypeEnum.System" code="Tenant:Status">
						<wd-button
							v-if="item.status === CommonStatusEnum.Enable"
							size="mini"
							type="danger"
							variant="plain"
							@click="handleChangeStatus(item, refresh)"
						>
							禁用
						</wd-button>
						<wd-button v-else size="mini" type="warning" variant="plain" @click="handleChangeStatus(item, refresh)">启用</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { TenantTypeEnum } from "@/api/enums/TenantTypeEnum";
import { tenantApi } from "@/api/services/Center/tenant";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryTenantPagedOutput } from "@/api/services/Center/tenant/models/QueryTenantPagedOutput";

definePage({
	name: "SystemTenant",
	layout: "layout",
	authCode: "Tenant:Paged",
	style: {
		navigationBarTitleText: "租户管理",
	},
});

const router = useRouter();

const handleOpen = throttle((mode: IPageStateType, tenantId?: string) =>
	router.push(`/pages_system/tenant/edit/index?mode=${mode}&tenantId=${tenantId}`)
);

const handleChangeStatus = (item: QueryTenantPagedOutput, refresh: () => Promise<void>) => {
	const { tenantId, status, rowVersion } = item;
	useMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}租户？`, {
		async beforeConfirm() {
			await tenantApi.changeStatus({ tenantId, rowVersion });
			useToast.success("操作成功！");
			refresh();
		},
	});
};
</script>

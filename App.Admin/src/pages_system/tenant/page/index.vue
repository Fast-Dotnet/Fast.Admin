<template>
	<FaPaging ref="faPagingRef" :request-api="tenantApi.queryTenantPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="状态" v-model="searchParam.value.status" name="CommonStatusEnum" radio clearable />
					<FaCellPicker title="版本" v-model="searchParam.value.edition" name="EditionEnum" radio clearable />
					<FaCellPicker title="租户类型" v-model="searchParam.value.tenantType" name="TenantTypeEnum" radio clearable />
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
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.tenantId" code="Tenant:Detail" @click="handleDetail(item.tenantId)">
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
				<FaField label="管理员" :value="[item.adminName, item.adminMobile].filter(Boolean).join(' · ')" />

				<template #footer>
					<FaAuth code="Tenant:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.tenantId)">编辑</wd-button>
					</FaAuth>
					<FaAuth v-if="item.tenantType !== TenantTypeEnum.System" code="Tenant:Status">
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
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { TenantTypeEnum } from "@/api/enums/TenantTypeEnum";
import { tenantApi } from "@/api/services/Center/tenant";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryTenantPagedOutput } from "@/api/services/Center/tenant/models/QueryTenantPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemTenant",
	authCode: "Tenant:Paged",
	style: {
		navigationBarTitleText: "租户管理",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleDetail = throttle((tenantId: string) => router.push(`/pages_system/tenant/detail/index?tenantId=${tenantId}`));

const handleEdit = throttle((mode: IPageStateType, tenantId?: string) =>
	router.push({
		path: "/pages_system/tenant/edit/index",
		query: {
			mode,
			tenantId,
		},
	})
);

const handleChangeStatus = (item: QueryTenantPagedOutput) => {
	const { tenantId, status, rowVersion } = item;
	useMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}租户？`, {
		async beforeConfirm() {
			await tenantApi.changeStatus({ tenantId, rowVersion });
			useToast.success("操作成功！");
			faPagingRef.value.reload();
		},
	});
};
</script>

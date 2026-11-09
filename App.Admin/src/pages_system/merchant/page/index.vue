<template>
	<FaPaging :request-api="merchantApi.queryMerchantPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput hide-search-time v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker
						title="商户类型"
						v-model="searchParam.value.merchantType"
						name="PaymentChannelEnum"
						include-all
						placeholder="请选择商户类型"
					/>
				</template>

				<template #footer>
					<FaAuth code="Merchant:Add">
						<wd-button size="small" icon="plus" @click="handleOpen('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard
				v-for="item in pagedList"
				:key="item.merchantId"
				:title="item.merchantName"
				code="Merchant:Detail"
				@click="handleOpen('detail', item.merchantId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="PaymentChannelEnum" :value="item.merchantType" />
					</view>
				</template>

				<FaField label="商户号" :value="item.merchantNo" />
				<FaField label="备注" :value="item.remark" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Merchant:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleOpen('edit', item.merchantId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Merchant:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item, refresh)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { useRouter } from "@wot-ui/router";
import { merchantApi } from "@/api/services/Center/merchant";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryMerchantPagedOutput } from "@/api/services/Center/merchant/models/QueryMerchantPagedOutput";

definePage({
	name: "SystemMerchant",
	layout: "layout",
	authCode: "Merchant:Paged",
	style: {
		navigationBarTitleText: "商户号",
	},
});

const router = useRouter();

const handleOpen = (mode: IPageStateType, merchantId?: string) =>
	router.push(`/pages_system/merchant/edit/index?mode=${mode}&merchantId=${merchantId}`);

const handleDelete = (item: QueryMerchantPagedOutput, refresh: () => Promise<void>) => {
	const { merchantId, rowVersion } = item;
	useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await merchantApi.deleteMerchant({ merchantId, rowVersion });
			useToast.success("删除成功！");
			refresh();
		},
	});
};
</script>

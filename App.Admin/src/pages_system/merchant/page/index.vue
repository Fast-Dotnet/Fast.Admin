<template>
	<FaPaging ref="faPagingRef" :request-api="merchantApi.queryMerchantPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput hide-search-time v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="商户类型" v-model="searchParam.value.merchantType" name="PaymentChannelEnum" radio clearable />
				</template>

				<template #footer>
					<FaAuth code="Merchant:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.merchantId"
				:title="item.merchantName"
				code="Merchant:Detail"
				@click="handleDetail(item.merchantId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="PaymentChannelEnum" :value="item.merchantType" />
					</view>
				</template>

				<FaField label="商户号" :value="item.merchantNo" />
				<FaField label="备注" :value="item.remark" />

				<template #footer>
					<FaAuth code="Merchant:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.merchantId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Merchant:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
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
import { merchantApi } from "@/api/services/Center/merchant";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryMerchantPagedOutput } from "@/api/services/Center/merchant/models/QueryMerchantPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemMerchant",
	authCode: "Merchant:Paged",
	style: {
		navigationBarTitleText: "商户号",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleDetail = throttle((merchantId: string) => router.push(`/pages_system/merchant/detail/index?merchantId=${merchantId}`));

const handleEdit = throttle((mode: IPageStateType, merchantId?: string) =>
	router.push({
		path: "/pages_system/merchant/edit/index",
		query: {
			mode,
			merchantId,
		},
	})
);

const handleDelete = (item: QueryMerchantPagedOutput) => {
	const { merchantId, rowVersion } = item;
	useMessageBox.confirm("确定删除此记录？", {
		async beforeConfirm() {
			await merchantApi.deleteMerchant({ merchantId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
};
</script>

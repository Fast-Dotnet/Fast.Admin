<template>
	<div>
		<FaTable ref="faTableRef" row-key="merchantId" :request-api="merchantApi.queryMerchantPaged" :columns="searchColumns" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'Merchant:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn prop="merchantType" label="商户类型" fixed="left" width="120" small-width="100" sortable tag :enum="paymentChannelEnum" />
			<FaTableColumn prop="merchantName" label="商户名称" fixed="left" width="200" small-width="180" sortable />
			<FaTableColumn
				prop="merchantNo"
				label="商户号"
				fixed="left"
				width="200"
				small-width="180"
				copy
				sortable
				link
				:click="({ row }) => editFormRef.detail(row.merchantId)"
			/>
			<FaTableColumn prop="remark" label="备注" width="200" small-width="180" sortable />
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				width="240"
				small-width="220"
				sortable
				type="timeInfo"
				:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
			/>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QueryMerchantPagedOutput }">
				<el-button v-auth="'Merchant:Detail'" size="small" plain @click="editFormRef.detail(row.merchantId)">详情</el-button>
				<el-button v-auth="'Merchant:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.merchantId)">编辑</el-button>
				<el-button v-auth="'Merchant:Delete'" size="small" plain type="warning" @click="handleDelete(row)">删除</el-button>
			</template>
		</FaTable>
		<ConfigEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { merchantApi } from "@/api/services/Center/merchant";
import { useApp } from "@/stores";
import ConfigEdit from "./edit/index.vue";
import type { FaTableColumnCtx, FaTableInstance } from "fast-element-plus";
import type { QueryMerchantPagedOutput } from "@/api/services/Center/merchant/models/QueryMerchantPagedOutput";

defineOptions({
	name: "SystemMerchant",
});

const appStore = useApp();
const paymentChannelEnum = appStore.getDictionary("PaymentChannelEnum");

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof ConfigEdit>>("editFormRef");

/** 处理删除缓存 */
const handleDelete = (row: QueryMerchantPagedOutput) => {
	const { merchantId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除商户号？", { type: "warning" }).then(async () => {
		await merchantApi.deleteMerchant({ merchantId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "merchantType",
		label: "商户类型",
		enum: paymentChannelEnum,
		search: { el: "el-select" },
	},
];
</script>

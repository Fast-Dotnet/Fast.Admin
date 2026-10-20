<template>
	<div>
		<FaTable row-key="recordId" :request-api="payRecordApi.queryPayRecordPaged" data-search-range="Past1M">
			<FaTableColumn
				prop="mobile"
				label="用户"
				fixed="left"
				width="300"
				small-width="280"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: PayRecordModel }">
					<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
					<br />
					OpenId：<span v-iconCopy="row.openId">{{ row.openId }}</span>
					<br />
					UnionId：<span v-iconCopy="row.unionId">{{ row.unionId }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="merchantNo" label="收款商户号" width="200" small-width="180" copy sortable />
			<FaTableColumn prop="bizOrderNo" label="业务编号" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn
				prop="paymentChannel"
				label="支付渠道"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('PaymentChannelEnum')"
			/>
			<FaTableColumn prop="orderAmount" label="订单金额" width="120" small-width="100" sortable type="d2">
				<template #default="{ row }: { row: PayRecordModel }"> ￥{{ (row.orderAmount ?? 0).toFixed(2) }} </template>
			</FaTableColumn>
			<FaTableColumn prop="transactionId" label="交易流水号" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn
				prop="paymentTime"
				label="支付信息"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: PayRecordModel }">
					<span>状态：<Tag name="BooleanEnum" :value="row.isPaid" /></span>
					<br />
					<span>金额：￥{{ row.paymentAmount?.toFixed(2) || 0.0 }}</span>
					<br />
					<template v-if="row.paymentTime">
						<span>时间：{{ dayjs(row.paymentTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
						<el-tag v-if="row.paymentTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
							{{ formatChineseRelativeTime(row.paymentTime) }}
						</el-tag>
					</template>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="closeTime" label="关闭信息" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: PayRecordModel }">
					<template v-if="row.isPaid">
						<span>--</span>
					</template>
					<template v-else>
						<span>状态：<Tag name="BooleanEnum" :value="row.isClosed" /></span>
						<br />
						<template v-if="row.closeTime">
							<span>时间：{{ dayjs(row.closeTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
							<el-tag v-if="row.closeTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
								{{ formatChineseRelativeTime(row.closeTime) }}
							</el-tag>
						</template>
						<span v-else>--</span>
					</template>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="notifyUrl" label="回调地址" width="500" small-width="480" sortable :show-overflow-tooltip="false" />
			<FaTableColumn
				prop="createdTime"
				label="创建时间"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: PayRecordModel }">
					<span>地区：{{ row.province }} - {{ row.city }}</span>
					<br />
					<span>Ip：{{ row.ip }}</span>
					<br />
					<span>时间：{{ dayjs(row.createdTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.createdTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
						{{ formatChineseRelativeTime(row.createdTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="os" label="创建设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: PayRecordModel }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { dayjs } from "element-plus";
import { formatChineseRelativeTime } from "@fast-china/utils";
import { payRecordApi } from "@/api/services/Center/payRecord";
import { useApp } from "@/stores";
import type { PayRecordModel } from "@/api/services/Center/payRecord/models/PayRecordModel";

defineOptions({
	name: "SystemPayRecord",
});

const appStore = useApp();
</script>

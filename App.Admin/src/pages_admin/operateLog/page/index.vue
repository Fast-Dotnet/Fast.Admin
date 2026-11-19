<template>
	<FaPaging page-key="OperateLog" :request-api="operateLogApi.queryOperateLogPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput
				data-search-range="Past1M"
				v-model:search-param="searchParam.value"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #filter>
					<FaEmployeeSelect v-model="searchParam.value.employeeId" placeholder="全部" />
					<FaCellPicker title="操作类型" v-model="searchParam.value.operateType" name="OperateLogTypeEnum" radio clearable />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.title" @click="detailFormRef.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="OperateLogTypeEnum" :value="item.operateType" />
					</view>
				</template>

				<FaField label="业务编码" :value="item.bizNo" copyable />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="[item.province, item.city].filter(Boolean).join(' - ')" />
				<FaField label="Ip" :value="item.ip" copyable />
				<FaField label="操作人" :value="[item.createdUserName, item.employeeNo].filter(Boolean).join(' · ')" />
				<FaField label="操作时间" :value="item.createdTime" />
			</FaCard>
		</template>
	</FaPaging>

	<OperateLogDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { operateLogApi } from "@/api/services/Admin/operateLog";
import OperateLogDetail from "./components/detail.vue";

definePage({
	name: "AdminOperateLog",
	authCode: "OperateLog:Paged",
	style: {
		navigationBarTitleText: "操作日志",
	},
});

const detailFormRef = ref<InstanceType<typeof OperateLogDetail>>();
</script>

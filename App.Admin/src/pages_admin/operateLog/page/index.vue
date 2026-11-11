<template>
	<FaPaging :request-api="operateLogApi.queryOperateLogPaged">
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
					<FaCellPicker title="操作类型" v-model="searchParam.value.operateType" name="OperateLogTypeEnum" radio />
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.createdTime" @click="editFormRef?.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="OperateLogTypeEnum" :value="item.operateType" />
					</view>
				</template>

				<FaField label="姓名" :value="[item.createdUserName, item.employeeNo].filter(Boolean).join(' · ')" />
				<FaField label="手机" :value="item.mobile" />
				<FaField label="标题" :value="item.title" />
				<FaField label="业务编码" :value="item.bizNo" />
				<FaField label="部门" :value="item.departmentName" />
				<FaField label="地区" :value="`${item.province} - ${item.city}`" />
				<FaField label="Ip" :value="item.ip" />
				<FaField label="设备" :value="item.device" />
				<FaField label="操作系统" :value="item.os" />
				<FaField label="浏览器" :value="item.browser" />
			</FaCard>
		</template>
	</FaPaging>

	<OperateLogDetail ref="editFormRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { operateLogApi } from "@/api/services/Admin/operateLog";
import OperateLogDetail from "./detail/index.vue";

definePage({
	name: "AdminOperateLog",
	layout: "layout",
	authCode: "OperateLog:Paged",
	style: {
		navigationBarTitleText: "操作日志",
	},
});

const editFormRef = ref<InstanceType<typeof OperateLogDetail>>();
</script>

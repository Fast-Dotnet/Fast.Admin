<template>
	<FaPaging page-key="Serial" :request-api="serialApi.querySerialRulePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Serial:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.serialRuleId" @click="detailFormRef.detail(item)">
				<FaField label="规则类型" center>
					<FaTag name="SerialRuleTypeEnum" :value="item.ruleType" />
				</FaField>
				<FaField label="前缀" :value="item.prefix" />
				<FaField label="时间类型">
					<FaText name="SerialDateTypeEnum" :value="item.dateType" />
				</FaField>
				<FaField label="分隔符">
					<FaText name="SerialSpacerEnum" :value="item.spacer" />
				</FaField>
				<FaField label="长度" :value="item.length" />
				<FaField label="最后编号" :value="item.lastSerialNo" copyable />

				<template #footer>
					<FaAuth code="Serial:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.serialRuleId)">编辑</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<SerialDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { serialApi } from "@/api/services/Admin/serial";
import SerialDetail from "./components/detail.vue";

definePage({
	name: "AdminSerial",
	authCode: "Serial:Paged",
	style: {
		navigationBarTitleText: "单号配置",
	},
});

const router = useRouter();

const detailFormRef = ref<InstanceType<typeof SerialDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, serialRuleId?: string) =>
		await router.push({
			path: "/pages_admin/serial/edit/index",
			query: {
				mode,
				serialRuleId,
			},
		})
);
</script>

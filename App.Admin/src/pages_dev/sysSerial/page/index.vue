<template>
	<FaPaging :request-api="sysSerialApi.querySysSerialRulePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="SysSerial:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.serialRuleId" @click="detailFormRef?.detail(item)">
				<FaField label="规则类型">
					<FaTag name="SysSerialRuleTypeEnum" :value="item.ruleType" />
				</FaField>
				<FaField label="前缀" :value="item.prefix" />
				<FaField label="时间类型">
					<FaText name="SerialDateTypeEnum" :value="item.dateType" />
				</FaField>
				<FaField label="分隔符">
					<FaText name="SerialSpacerEnum" :value="item.spacer" />
				</FaField>
				<FaField label="长度" :value="item.length" />
				<FaField label="最后编号" :value="item.lastSerialNo" />

				<template #footer>
					<FaAuth code="SysSerial:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.serialRuleId)">编辑</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<SysSerialDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { sysSerialApi } from "@/api/services/Center/sysSerial";
import SysSerialDetail from "./components/detail.vue";

definePage({
	name: "DevSysSerial",
	authCode: "SysSerial:Paged",
	style: {
		navigationBarTitleText: "序号配置",
	},
});

const router = useRouter();

const detailFormRef = ref<InstanceType<typeof SysSerialDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, serialRuleId?: string) =>
		await router.push({
			path: "/pages_dev/sysSerial/edit/index",
			query: {
				mode,
				serialRuleId,
			},
		})
);
</script>

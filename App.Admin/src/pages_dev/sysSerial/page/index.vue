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
			<FaCard
				v-for="item in pagedList"
				:key="item.serialRuleId"
				:title="item.prefix"
				code="SysSerial:Detail"
				@click="handleDetail(item.serialRuleId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="SysSerialRuleTypeEnum" :value="item.ruleType" />
					</view>
				</template>

				<FaField label="时间类型">
					<FaText name="SerialDateTypeEnum" :value="item.dateType" />
				</FaField>
				<FaField label="分隔符">
					<FaText name="SerialSpacerEnum" :value="item.spacer" />
				</FaField>
				<FaField label="长度" :value="item.length" />
				<FaField label="最后序号" :value="item.lastSerial" />
				<FaField label="最后编号" :value="item.lastSerialNo" />
				<FaField label="最后生成时间" :value="item.lastTime" />

				<template #footer>
					<FaAuth code="SysSerial:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.serialRuleId)">编辑</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { sysSerialApi } from "@/api/services/Center/sysSerial";

definePage({
	name: "DevSysSerial",
	authCode: "SysSerial:Paged",
	style: {
		navigationBarTitleText: "序号配置",
	},
});

const router = useRouter();

const handleDetail = throttle((serialRuleId: string) => router.push(`/pages_dev/sysSerial/detail/index?serialRuleId=${serialRuleId}`));

const handleEdit = throttle((mode: IPageStateType, serialRuleId?: string) =>
	router.push(`/pages_dev/sysSerial/edit/index?mode=${mode}&serialRuleId=${serialRuleId}`)
);
</script>

<template>
	<FaPaging :request-api="sysSerialApi.querySysSerialRulePaged">
		<template #top="{ searchParam, setParam, refresh }">
			<FaSearchInput
				:filter="false"
				:search-param="searchParam"
				@update:search-param="setParam"
				@confirm="refresh()"
				@clear="refresh()"
				@search="refresh()"
			>
				<template #footer>
					<FaAuth code="SysSerial:Add">
						<wd-button size="small" icon="plus" @click="handleOpen('add')">新增</wd-button>
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
				@click="handleOpen('detail', item.serialRuleId)"
			>
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="SysSerialRuleTypeEnum" :value="item.ruleType" />
					</view>
				</template>

				<FaField label="时间类型">
					<FaTag name="SerialDateTypeEnum" :value="item.dateType" />
				</FaField>
				<FaField label="分隔符">
					<FaTag name="SerialSpacerEnum" :value="item.spacer" />
				</FaField>
				<FaField label="长度" :value="item.length" />
				<FaField label="最后序号" :value="item.lastSerial" />
				<FaField label="最后编号" :value="item.lastSerialNo" />
				<FaField label="最后生成时间" :value="item.lastTime" />

				<template #footer>
					<FaAuth code="SysSerial:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleOpen('edit', item.serialRuleId)">编辑</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { useRouter } from "@wot-ui/router";
import { sysSerialApi } from "@/api/services/Center/sysSerial";

definePage({
	name: "DevSysSerial",
	layout: "layout",
	authCode: "SysSerial:Paged",
	style: {
		navigationBarTitleText: "序号配置",
	},
});

const router = useRouter();

const handleOpen = (mode: IPageStateType, serialRuleId?: string) =>
	router.push(`/pages_dev/sysSerial/edit/index?mode=${mode}&serialRuleId=${serialRuleId}`);
</script>

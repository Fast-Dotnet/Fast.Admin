<template>
	<FaPaging :request-api="serialApi.querySerialRulePaged">
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
			<FaCard v-for="item in pagedList" :key="item.serialRuleId">
				<FaField label="规则类型">
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
				<FaField label="最后序号" :value="item.lastSerial" />
				<FaField label="最后编号" :value="item.lastSerialNo" />
				<FaField label="最后生成时间" :value="item.lastTime" />
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="Serial:Edit">
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
import { serialApi } from "@/api/services/Admin/serial";

definePage({
	name: "AdminSerial",
	authCode: "Serial:Paged",
	style: {
		navigationBarTitleText: "单号配置",
	},
});

const router = useRouter();

const handleEdit = throttle((mode: IPageStateType, serialRuleId?: string) =>
	router.push({
		path: "/pages_admin/serial/edit/index",
		query: {
			mode,
			serialRuleId,
		},
	})
);
</script>

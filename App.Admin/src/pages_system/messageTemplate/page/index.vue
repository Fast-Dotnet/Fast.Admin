<template>
	<FaPaging :request-api="messageTemplateApi.queryMessageTemplatePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
				<template #footer>
					<FaAuth code="MessageTemplate:Add">
						<wd-button size="small" icon="plus" @click="handleOpen('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList, refresh }">
			<FaCard
				v-for="item in pagedList"
				:key="item.recordId"
				:title="item.templateCode"
				code="MessageTemplate:Detail"
				@click="handleOpen('detail', item.recordId)"
			>
				<template #titleExtra>
					<FaTag name="MessageTemplateTypeEnum" :value="item.templateType" />
				</template>

				<FaField label="租户">
					<text v-if="item.tenantName">{{ item.tenantName }}</text>
					<text v-else style="color: var(--wot-text-placeholder)">未指定租户</text>
				</FaField>
				<FaField label="应用标识">
					<text v-if="item.appKey">{{ item.appKey }}</text>
					<text v-else style="color: var(--wot-text-placeholder)">{{ item.tenantName ? "不区分应用" : "全局模板" }}</text>
				</FaField>
				<FaField label="应用名称" :value="item.appName" />
				<FaField label="应用类型">
					<FaTagFlag name="AppEnvironmentEnum" :value="item.appType" />
				</FaField>
				<FaField label="环境类型">
					<FaTag name="EnvironmentTypeEnum" :value="item.environmentType" />
				</FaField>
				<FaField label="创建人" :value="item.createdUserName" />
				<FaField label="创建时间" :value="item.createdTime" />
				<FaField label="更新人" :value="item.updatedUserName" />
				<FaField label="更新时间" :value="item.updatedTime" />

				<template #footer>
					<FaAuth code="MessageTemplate:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleOpen('edit', item.recordId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="MessageTemplate:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item, refresh)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryMessageTemplatePagedOutput } from "@/api/services/Center/messageTemplate/models/QueryMessageTemplatePagedOutput";

definePage({
	name: "SystemMessageTemplate",
	authCode: "MessageTemplate:Paged",
	style: {
		navigationBarTitleText: "消息模板",
	},
});

const router = useRouter();

const handleOpen = throttle((mode: IPageStateType, recordId?: string) =>
	router.push(`/pages_system/messageTemplate/edit/index?mode=${mode}&recordId=${recordId}`)
);

const handleDelete = (item: QueryMessageTemplatePagedOutput, refresh: () => Promise<void>) => {
	const { recordId, rowVersion } = item;
	useMessageBox.confirm("确定要删除消息模板？", {
		async beforeConfirm() {
			await messageTemplateApi.deleteMessageTemplate({ recordId, rowVersion });
			useToast.success("删除成功！");
			refresh();
		},
	});
};
</script>

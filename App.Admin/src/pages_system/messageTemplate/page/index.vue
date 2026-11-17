<template>
	<FaPaging ref="faPagingRef" page-key="MessageTemplate" :request-api="messageTemplateApi.queryMessageTemplatePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaTenantSelect v-model="searchParam.value.tenantId" />
				</template>
				<template #footer>
					<FaAuth code="MessageTemplate:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.recordId"
				:title="item.templateCode"
				code="MessageTemplate:Detail"
				@click="detailFormRef?.detail(item.recordId)"
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
				<FaField label="环境类型" center>
					<FaTag name="EnvironmentTypeEnum" :value="item.environmentType" />
				</FaField>

				<template #footer>
					<FaAuth code="MessageTemplate:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.recordId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="MessageTemplate:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<MessageTemplateDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { messageTemplateApi } from "@/api/services/Center/messageTemplate";
import { useMessageBox, useToast } from "@/hooks";
import MessageTemplateDetail from "./components/detail.vue";
import type { QueryMessageTemplatePagedOutput } from "@/api/services/Center/messageTemplate/models/QueryMessageTemplatePagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemMessageTemplate",
	authCode: "MessageTemplate:Paged",
	style: {
		navigationBarTitleText: "消息模板",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof MessageTemplateDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, recordId?: string) =>
		await router.push({
			path: "/pages_system/messageTemplate/edit/index",
			query: {
				mode,
				recordId,
			},
		})
);

const handleDelete = throttle(async (item: QueryMessageTemplatePagedOutput) => {
	const { recordId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除消息模板？", {
		async beforeConfirm() {
			await messageTemplateApi.deleteMessageTemplate({ recordId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

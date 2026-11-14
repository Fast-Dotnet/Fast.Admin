<template>
	<FaPaging ref="faPagingRef" :request-api="applicationTokenApi.queryApplicationTokenPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="AppToken:Add">
						<wd-button size="small" icon="plus" @click="handleAdd">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.recordId" :title="item.appName">
				<template #titleExtra>
					<FaTag name="ApplicationTokenTypeEnum" :value="item.tokenType" />
				</template>

				<FaField label="应用标识" :value="item.appKey" selectable />
				<FaField label="应用类型">
					<FaTagFlag name="AppEnvironmentEnum" :value="item.appType" />
				</FaField>
				<FaField label="环境类型">
					<FaText name="EnvironmentTypeEnum" :value="item.environmentType" />
				</FaField>
				<FaField label="刷新时间" :value="item.refreshTime" />
				<FaField label="过期时间" :value="item.expireTime" />

				<template #footer>
					<FaAuth code="AppToken:Refresh">
						<wd-button size="mini" type="primary" variant="plain" @click="handleRefresh(item)">刷新</wd-button>
					</FaAuth>
					<FaAuth code="AppToken:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { applicationTokenApi } from "@/api/services/Center/applicationToken";
import { useMessageBox, useToast } from "@/hooks";
import type { QueryApplicationTokenPagedOutput } from "@/api/services/Center/applicationToken/models/QueryApplicationTokenPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemApplicationToken",
	authCode: "AppToken:Paged",
	style: {
		navigationBarTitleText: "应用凭据",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();

const handleAdd = throttle(() => router.push("/pages_system/applicationToken/add/index"));

const handleRefresh = (item: QueryApplicationTokenPagedOutput) => {
	useMessageBox.confirm("确定要刷新应用凭据？", {
		async beforeConfirm() {
			await applicationTokenApi.refreshApplicationToken({ recordId: item.recordId });
			useToast.success("刷新成功！");
			faPagingRef.value.refresh();
		},
	});
};

const handleDelete = (item: QueryApplicationTokenPagedOutput) => {
	useMessageBox.confirm("确定要删除应用凭据？", {
		async beforeConfirm() {
			await applicationTokenApi.deleteApplicationToken({ recordId: item.recordId });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
};
</script>

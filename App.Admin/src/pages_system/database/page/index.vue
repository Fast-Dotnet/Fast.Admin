<template>
	<FaPaging ref="faPagingRef" page-key="Database" :request-api="databaseApi.queryDatabasePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaTenantSelect v-model="searchParam.value.tenantId" />
					<FaCellPicker title="数据库类型" name="DatabaseTypeEnum" v-model="searchParam.value.databaseType" radio clearable />
					<FaCellPicker title="Db类型" name="SugarDbType" v-model="searchParam.value.dbType" radio clearable />
				</template>

				<template #footer>
					<FaAuth code="Database:Add">
						<wd-button size="small" icon="plus" @click="handleAdd">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.mainId" :title="item.dbName" @click="detailFormRef.detail(item)">
				<template #titleExtra>
					<view class="fa-card__title-view">
						<FaTag name="DatabaseTypeEnum" :value="item.databaseType" />
					</view>
				</template>

				<FaField label="租户" :value="item.tenantName" />
				<FaField label="Db类型" center>
					<FaTag name="SugarDbType" :value="item.dbType" />
				</FaField>
				<FaField label="公网IP" :value="item.publicIp" selectable />
				<FaField label="内网IP" :value="item.intranetIp" selectable />
				<FaField label="端口号" :value="item.port" />
				<FaField label="初始化" center>
					<wd-tag v-if="item.isInitialized" type="success">已初始化</wd-tag>
					<wd-tag v-else type="danger">未初始化</wd-tag>
				</FaField>

				<template #footer>
					<FaAuth code="Database:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit(item.mainId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Database:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
					<FaAuth v-if="!item.isInitialized" code="Database:Edit">
						<wd-button size="mini" type="warning" variant="plain" @click="handleInitDatabase(item)">初始化</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<DatabaseDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { databaseApi } from "@/api/services/Center/database";
import { useMessageBox, useToast } from "@/hooks";
import DatabaseDetail from "./components/detail.vue";
import type { QueryDatabasePagedOutput } from "@/api/services/Center/database/models/QueryDatabasePagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "SystemDatabase",
	authCode: "Database:Paged",
	style: {
		navigationBarTitleText: "数据库配置",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof DatabaseDetail>>();

const handleAdd = throttle(async () => await router.push("/pages_system/database/add/index"));

const handleEdit = throttle(async (mainId?: string) => await router.push(`/pages_system/database/edit/index?mainId=${mainId}`));

const handleDelete = throttle(async (item: QueryDatabasePagedOutput) => {
	const { mainId, rowVersion } = item;
	await useMessageBox.confirm("确定删除数据库？", {
		async beforeConfirm() {
			await databaseApi.deleteDatabase({ mainId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});

const handleInitDatabase = throttle(async (item: QueryDatabasePagedOutput) => {
	const { tenantId, databaseType } = item;
	await useMessageBox.confirm("确定要初始化数据库？", {
		async beforeConfirm() {
			await databaseApi.initDatabase({ tenantId, databaseType });
			useToast.success("初始化成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

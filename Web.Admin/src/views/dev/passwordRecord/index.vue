<template>
	<div>
		<FaTable row-key="recordId" :request-api="passwordRecordApi.queryPasswordRecordPaged" :columns="searchColumns" data-search-range="Past3M">
			<FaTableColumn prop="avatar" label="头像" fixed="left" width="70" small-width="50" sortable type="image" />
			<FaTableColumn prop="nickName" label="昵称" fixed="left" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="accountKey" label="账号Key" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="mobile" label="手机" width="150" small-width="130" copy sortable />
			<FaTableColumn prop="email" label="邮箱" width="200" small-width="180" copy sortable />
			<FaTableColumn prop="operationType" label="操作类型" width="120" small-width="100" sortable tag :enum="passwordOperationTypeEnum" />
			<FaTableColumn prop="type" label="类型" width="100" small-width="80" sortable tag :enum="appStore.getDictionary('PasswordTypeEnum')" />
			<FaTableColumn prop="createdTime" label="创建时间" width="180" small-width="160" sortable date-fix type="dateTime" />
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { passwordRecordApi } from "@/api/services/Center/passwordRecord";
import { useApp } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";

defineOptions({
	name: "DevPasswordRecord",
});

const appStore = useApp();
const passwordOperationTypeEnum = appStore.getDictionary("PasswordOperationTypeEnum");

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "accountId",
		label: "账号",
		search: { el: "AccountSelectPage" },
	},
	{
		pureSearch: true,
		prop: "operationType",
		label: "操作类型",
		enum: passwordOperationTypeEnum,
		search: { el: "el-select" },
	},
];
</script>

<template>
	<div>
		<FaTable row-key="apiId" :request-api="apiApi.queryApiPaged" hide-search-time>
			<template #expand="{ row }: { row: ApiInfoModel }">
				<!-- eslint-disable-next-line vue/no-v-html -- 后端已对描述内容进行 HTML 编码 -->
				<div v-html="row.description" />
			</template>
			<FaTableColumn prop="apiUrl" label="接口地址" fixed="left" width="240" small-width="220" copy sortable />
			<FaTableColumn prop="apiName" label="接口名称" fixed="left" width="180" small-width="160" sortable />
			<FaTableColumn prop="moduleName" label="模块名称" width="120" small-width="100" sortable />
			<FaTableColumn
				prop="method"
				label="请求方式"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('HttpRequestMethodEnum')"
			/>
			<FaTableColumn
				prop="action"
				label="操作方式"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('HttpRequestActionEnum')"
			/>
			<FaTableColumn prop="hasAuth" label="授权检测" width="120" small-width="100" sortable tag :enum="booleanEnum" />
			<FaTableColumn prop="hasPermission" label="权限检测" width="120" small-width="100" sortable tag :enum="booleanEnum" />
			<FaTableColumn prop="tags" label="标签" width="200" small-width="180" sortable>
				<template #default="{ row }: { row: ApiInfoModel }">
					<el-tag v-for="(item, index) in row.tags" :key="index" type="info" effect="dark">{{ item }}</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="serviceName" label="服务名称" width="140" small-width="120" sortable />
			<FaTableColumn prop="groupName" label="分组名称" width="140" small-width="120" sortable />
			<FaTableColumn prop="groupTitle" label="分组标题" width="140" small-width="120" sortable />
			<FaTableColumn prop="version" label="版本" width="100" small-width="80" sortable />
			<FaTableColumn prop="sort" label="排序" width="80" small-width="70" sortable />
			<FaTableColumn prop="description" label="分组描述" width="200" small-width="180" sortable show-overflow-tooltip />
			<FaTableColumn prop="createdTime" label="创建时间" width="180" small-width="160" sortable date-fix type="dateTime" />
			<FaTableColumn prop="updatedTime" label="更新时间" width="180" small-width="160" sortable date-fix type="dateTime" />
		</FaTable>
	</div>
</template>

<script lang="ts" setup>
import { apiApi } from "@/api/services/Center/api";
import { useApp } from "@/stores";
import type { ApiInfoModel } from "@/api/services/Center/api/models/ApiInfoModel";

defineOptions({
	name: "DevApi",
});

const appStore = useApp();
const booleanEnum = appStore.getDictionary("BooleanEnum");
</script>

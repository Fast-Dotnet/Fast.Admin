<template>
	<div>
		<FaTable row-key="recordId" :request-api="requestLogApi.queryRequestLogPaged" :columns="searchColumns">
			<FaTableColumn
				prop="mobile"
				label="昵称/手机"
				fixed="left"
				width="200"
				small-width="180"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: RequestLogModel }">
					{{ row.nickName }}
					<br />
					手机：<span v-iconCopy="row.mobile">{{ row.mobile }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="createdTime"
				label="请求时间"
				width="280"
				small-width="260"
				sortable
				class-name="fa-table__line-height-normal-column"
			>
				<template #default="{ row }: { row: RequestLogModel }">
					<span>地区：{{ row.province }} - {{ row.city }}</span>
					<br />
					<span>Ip：{{ row.ip }}</span>
					<br />
					<span>时间：{{ dayjs(row.createdTime).format("YYYY-MM-DD HH:mm:ss") }}</span>
					<el-tag v-if="row.createdTime" type="info" round effect="light" size="small" style="margin-left: 5px !important">
						{{ formatChineseRelativeTime(row.createdTime) }}
					</el-tag>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="os" label="请求设备" width="280" small-width="260" sortable class-name="fa-table__line-height-normal-column">
				<template #default="{ row }: { row: RequestLogModel }">
					<span>设备：{{ row.device }}</span>
					<br />
					<span>操作系统：{{ row.os }}</span>
					<br />
					<span>浏览器：{{ row.browser }}</span>
				</template>
			</FaTableColumn>
			<FaTableColumn
				prop="isSuccess"
				label="成功"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="booleanEnum"
				:show-overflow-tooltip="false"
			/>
			<FaTableColumn prop="operationName" label="操作名称" width="200" small-width="180" sortable />
			<FaTableColumn prop="operationAction" label="操作方式" width="120" small-width="100" sortable tag :enum="httpRequestActionEnum" />
			<FaTableColumn prop="location" label="请求地址" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="requestMethod" label="请求方式" width="120" small-width="100" sortable tag :enum="httpRequestMethodEnum" />
			<FaTableColumn prop="param" label="请求参数" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: RequestLogModel }">
					<el-tag
						v-if="row.param && (userInfoStore.isSuperAdmin || userInfoStore.isAdmin)"
						type="info"
						style="cursor: pointer"
						@click="
							() => {
								state.title = '请求参数';
								state.content = row.param;
								state.decrypt = false;
								state.visible = true;
							}
						"
					>
						查看
					</el-tag>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="result" label="返回结果" width="120" small-width="100" sortable>
				<template #default="{ row }: { row: RequestLogModel }">
					<el-tag
						v-if="row.result && (userInfoStore.isSuperAdmin || userInfoStore.isAdmin)"
						type="info"
						style="cursor: pointer"
						@click="
							() => {
								state.title = '返回结果';
								state.content = row.result;
								state.decrypt = true;
								state.visible = true;
							}
						"
					>
						查看
					</el-tag>
					<span v-else>--</span>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="elapsedTime" label="耗时" width="100" small-width="80" sortable>
				<template #default="{ row }: { row: RequestLogModel }"> {{ (Number(row.elapsedTime ?? 0) / 1000).toFixed(4) }}秒 </template>
			</FaTableColumn>
			<FaTableColumn prop="className" label="类名" width="300" small-width="280" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="methodName" label="方法名" width="200" small-width="180" sortable :show-overflow-tooltip="false" />
			<FaTableColumn prop="tenantName" label="租户" width="200" small-width="180" sortable />
			<FaTableColumn prop="departmentName" label="部门" width="150" small-width="130" sortable />
		</FaTable>
		<el-dialog v-model="state.visible" :title="state.title" width="1000px" align-center draggable destroy-on-close>
			<el-scrollbar>
				<div style="max-height: 500px; padding-bottom: 20px; padding-right: 10px">
					<VueJsonPretty
						:data="jsonContent"
						:deep="3"
						show-length
						show-line-number
						show-icon
						virtual
						:height="500"
						:theme="configStore.layout.isDark ? 'dark' : 'light'"
					/>
				</div>
			</el-scrollbar>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive } from "vue";
import { dayjs } from "element-plus";
import { AESDecrypt, formatChineseRelativeTime } from "@fast-china/utils";
import VueJsonPretty from "vue-json-pretty";
import { requestLogApi } from "@/api/services/Center/requestLog";
import { useApp, useConfig, useUserInfo } from "@/stores";
import type { FaTableColumnCtx } from "fast-element-plus";
import type { JSONDataType } from "vue-json-pretty/types/utils";
import type { RequestLogModel } from "@/api/services/Center/requestLog/models/RequestLogModel";

defineOptions({
	name: "SystemRequestLog",
});

const appStore = useApp();
const booleanEnum = appStore.getDictionary("BooleanEnum");
const httpRequestActionEnum = appStore.getDictionary("HttpRequestActionEnum");
const httpRequestMethodEnum = appStore.getDictionary("HttpRequestMethodEnum");

const configStore = useConfig();
const userInfoStore = useUserInfo();

const state = reactive({
	visible: false,
	decrypt: false,
	title: "日志",
	content: "",
});

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === "object" && value !== null;

const jsonContent = computed<JSONDataType>(() => {
	try {
		const result = JSON.parse(state.content) as JSONDataType;
		try {
			if (state.decrypt && isRecord(result) && isRecord(result.value)) {
				const timestamp = result.value.timestamp;
				const data = result.value.data;
				if ((typeof timestamp === "string" || typeof timestamp === "number") && typeof data === "string") {
					const timestampText = String(timestamp);
					result.value.data = AESDecrypt(data, timestampText, `FIV${timestampText}`).parseJson();
				}
				// 处理 ""xxx"" 这种数据
				if (typeof result.value.data === "string" && result.value.data.startsWith('"') && result.value.data.endsWith('"')) {
					result.value.data = result.value.data.replace(/"/g, "");
				}
			}
			return result;
		} catch {
			return result;
		}
	} catch {
		return state.content;
	}
});

const searchColumns: FaTableColumnCtx[] = [
	{
		pureSearch: true,
		prop: "accountId",
		label: "账号",
		search: { el: "AccountSelectPage" },
	},
	{
		pureSearch: true,
		prop: "tenantId",
		label: "租户",
		search: { el: "TenantSelectPage" },
	},
	{
		pureSearch: true,
		prop: "isSuccess",
		label: "成功",
		enum: booleanEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "operationAction",
		label: "操作方式",
		enum: httpRequestActionEnum,
		search: { el: "el-select" },
	},
	{
		pureSearch: true,
		prop: "requestMethod",
		label: "请求方式",
		enum: httpRequestMethodEnum,
		search: { el: "el-select" },
	},
];
</script>

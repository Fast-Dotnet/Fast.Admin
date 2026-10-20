<template>
	<div>
		<FaTable ref="faTableRef" row-key="serialRuleId" :request-api="sysSerialApi.querySysSerialRulePaged" hide-search-time>
			<!-- 表格按钮操作区域 -->
			<template #header>
				<el-button v-auth="'SysSerial:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
			</template>

			<FaTableColumn
				prop="ruleType"
				label="规则类型"
				fixed="left"
				width="120"
				small-width="100"
				sortable
				tag
				:enum="appStore.getDictionary('SysSerialRuleTypeEnum')"
			/>
			<FaTableColumn prop="prefix" label="前缀" fixed="left" width="100" small-width="80" sortable />
			<FaTableColumn
				prop="dateType"
				label="时间类型"
				width="180"
				small-width="150"
				sortable
				tag
				:enum="appStore.getDictionary('SerialDateTypeEnum')"
			/>
			<FaTableColumn
				prop="spacer"
				label="分隔符"
				width="100"
				small-width="80"
				sortable
				tag
				:enum="appStore.getDictionary('SerialSpacerEnum')"
			/>
			<FaTableColumn prop="length" label="长度" width="100" small-width="80" sortable />
			<FaTableColumn prop="lastSerial" label="最后序号" width="120" small-width="100" sortable />
			<FaTableColumn prop="lastSerialNo" label="最后编号" width="200" small-width="180" sortable />
			<FaTableColumn prop="lastTime" label="最后生成时间" width="180" small-width="160" sortable date-fix type="dateTime" />
			<FaTableColumn prop="createdTime" label="创建时间" width="240" small-width="220" sortable type="timeInfo" />
			<FaTableColumn
				prop="updatedTime"
				label="更新时间"
				width="240"
				small-width="220"
				sortable
				type="timeInfo"
				:time-info-field="{ userName: 'updatedUserName', time: 'updatedTime' }"
			/>

			<!-- 表格操作 -->
			<template #operation="{ row }: { row: QuerySysSerialRulePagedOutput }">
				<el-button v-auth="'SysSerial:Detail'" size="small" plain @click="editFormRef.detail(row.serialRuleId)">详情</el-button>
				<el-button v-auth="'SysSerial:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.serialRuleId)">编辑</el-button>
			</template>
		</FaTable>
		<SysSerialEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { sysSerialApi } from "@/api/services/Center/sysSerial";
import { useApp } from "@/stores";
import SysSerialEdit from "./edit/index.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { QuerySysSerialRulePagedOutput } from "@/api/services/Center/sysSerial/models/QuerySysSerialRulePagedOutput";

defineOptions({
	name: "DevSysSerial",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof SysSerialEdit>>("editFormRef");
</script>

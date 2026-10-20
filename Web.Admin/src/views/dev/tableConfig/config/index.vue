<template>
	<div class="el-card h100" style="display: flex; flex-direction: column">
		<el-divider content-position="left">
			<el-icon>
				<StarFilled />
			</el-icon>
			{{ state.tableName }}
		</el-divider>
		<FaTable ref="faTableRef" :data="state.tableData" :pagination="false" :tool-btn="false">
			<template #header="{ loading }">
				<el-button :icon="ArrowLeftBold" @click="handleBack">返回</el-button>
				<el-button :disabled="loading" type="primary" :icon="Plus" @click="handleAdd">新增</el-button>
			</template>
			<FaTableColumn prop="prop" label="绑定字段" width="280" fixed>
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-input v-model="row.prop" maxlength="50" placeholder="请输入绑定字段" />
				</template>
			</FaTableColumn>
			<FaTableColumn prop="label" label="名称" width="280">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-input v-model="row.label" maxlength="50" placeholder="请输入绑定名称" />
				</template>
			</FaTableColumn>
			<FaTableColumn prop="fixed" label="固定" width="240">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-radio-group v-model="row.fixed">
						<el-radio value="">无</el-radio>
						<el-radio value="left">左侧</el-radio>
						<el-radio value="right">右侧</el-radio>
					</el-radio-group>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="autoWidth" label="自动宽度" width="160">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-radio-group v-model="row.autoWidth">
						<el-radio :value="true">是</el-radio>
						<el-radio :value="false">否</el-radio>
					</el-radio-group>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="width" label="宽度" width="200">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-input-number v-model="row.width" :min="0" :max="999" step-strictly :controls="false" placeholder="请输入宽度">
						<template #suffix>
							<span>px</span>
						</template>
					</el-input-number>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="smallWidth" label="最小宽度" width="200">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-input-number v-model="row.smallWidth" :min="0" :max="999" step-strictly :controls="false" placeholder="请输入最小宽度">
						<template #suffix>
							<span>px</span>
						</template>
					</el-input-number>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="order" label="顺序" width="200">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-input-number
						v-model="row.order"
						:min="0"
						:max="999"
						step-strictly
						:controls="false"
						placeholder="请输入顺序"
						@change="handleOrderChange"
					/>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="show" label="显示" width="160">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-radio-group v-model="row.show">
						<el-radio :value="true">显示</el-radio>
						<el-radio :value="false">隐藏</el-radio>
					</el-radio-group>
				</template>
			</FaTableColumn>
			<FaTableColumn prop="copy" label="复制" width="160">
				<template #default="{ row }: { row: FaTableColumnCtx }">
					<el-radio-group v-model="row.copy">
						<el-radio :value="true">是</el-radio>
						<el-radio :value="false">否</el-radio>
					</el-radio-group>
				</template>
			</FaTableColumn>
			<!-- 表格操作 -->
			<template #operation="{ $index }">
				<el-button size="small" plain type="primary" @click="advancedConfigRef.edit(state.tableData[$index], $index)">高级配置</el-button>
				<el-button size="small" plain type="danger" @click="handleDelete($index)">删除</el-button>
			</template>
		</FaTable>
		<div style="display: flex; justify-content: center; margin-top: 10px">
			<el-button :icon="Close" @click="handleBack">取消</el-button>
			<el-button type="primary" :icon="Select" @click="handleSave">保存</el-button>
		</div>
		<AdvancedConfigForm
			ref="advancedConfigRef"
			@change="
				(row, rowIndex) => {
					state.tableData[rowIndex] = row;
				}
			"
		/>
	</div>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef, watch } from "vue";
import { ArrowLeftBold, Close, Plus, Select, StarFilled } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { tableApi } from "@/api/services/Center/table";
import AdvancedConfigForm from "./components/advancedConfig.vue";
import type { FaTableInstance } from "fast-element-plus";
import type { WatchHandle } from "vue";
import type { FaTableColumnCtx } from "@/api/services/Center/table/models/FaTableColumnCtx";

defineOptions({
	name: "DevTableColumnConfig",
});

const emit = defineEmits(["back", "ok"]);

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const advancedConfigRef = useTemplateRef<InstanceType<typeof AdvancedConfigForm>>("advancedConfigRef");

const state = reactive({
	/** 是否存在改变 */
	change: false,
	/** 表格Id */
	tableId: withDefineType<string>(),
	/** 表格名称 */
	tableName: "",
	/** 行版本号 */
	rowVersion: withDefineType<string>(),
	/** 表格数据 */
	tableData: withDefineType<FaTableColumnCtx[]>([]),
});

let tableWatch: WatchHandle;

/** 处理返回 */
const handleBack = () => {
	tableWatch();
	if (state.change) {
		ElMessageBox.confirm("确定要取消编辑？", {
			type: "warning",
		}).then(() => {
			emit("back");
		});
	} else {
		emit("back");
	}
};

/** 处理添加 */
const handleAdd = () => {
	state.change = true;
	state.tableData.push({
		prop: "",
		label: "",
		fixed: "",
		autoWidth: false,
		width: 100,
		smallWidth: 100,
		order: state.tableData.length + 1,
		show: true,
		copy: false,
		sortable: true,
		sortableField: "",
		type: "",
		link: false,
		clickEmit: "",
		tag: false,
		enum: "",
		dateFix: false,
		dateFormat: "",
		authTag: [],
		dataDeleteField: "",
		slot: "",
		otherConfig: [],
		pureSearch: false,
		searchEl: "",
		searchKey: "",
		searchLabel: "",
		searchOrder: 0,
		searchSlot: "",
		searchConfig: [],
	});
};

/** 处理删除 */
const handleDelete = (index: number) => {
	state.tableData.splice(index, 1);
};

/** 处理排序改变 */
const handleOrderChange = () => {
	state.tableData = state.tableData.sort((a, b) => {
		if (a.order !== b.order) {
			return a.order - b.order;
		} else {
			return state.tableData.indexOf(b) - state.tableData.indexOf(a);
		}
	});
	state.tableData.forEach((item, index) => {
		item.order = index + 1;
	});
};

/** 处理保存 */
const handleSave = () => {
	ElMessageBox.confirm("确认要保存表格配置？", { type: "warning" }).then(async () => {
		await tableApi.editTableColumnConfig({
			tableId: state.tableId,
			columns: state.tableData,
			rowVersion: state.rowVersion,
		});
		state.change = false;
		ElMessage.success("保存成功！");
		handleBack();
		emit("ok");
	});
};

const edit = (tableId: string, tableName: string, rowVersion: string) => {
	state.tableId = tableId;
	state.tableName = tableName;
	state.rowVersion = rowVersion;
	state.change = false;
	faTableRef.value.doLoading(async () => {
		state.tableData = await tableApi.queryTableColumnConfigDetail(tableId);

		tableWatch = watch(
			() => state.tableData,
			() => {
				state.change = true;
			},
			{
				deep: true,
			}
		);
	});
};

defineExpose({
	edit,
});
</script>

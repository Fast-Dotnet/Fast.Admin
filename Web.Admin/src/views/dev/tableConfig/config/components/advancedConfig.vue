<template>
	<FaDialog ref="faDialogRef" width="80vw" full-height title="高级配置" confirm-button-text="保存" @confirm-click="handleConfirm">
		<el-tabs class="advanced-config-tabs" type="border-card" v-model="state.activeTab">
			<el-tab-pane :name="1" label="基础配置">
				<el-scrollbar>
					<el-form class="fa-form advanced-config-form" label-width="auto" label-suffix="" :model="state.formData" label-position="top">
						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="prop" label="绑定字段">
									<el-input v-model="state.formData.prop" maxlength="50" placeholder="请输入绑定字段" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="label" label="名称">
									<el-input v-model="state.formData.label" maxlength="50" placeholder="请输入名称" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="order" label="顺序">
									<el-input-number
										v-model="state.formData.order"
										:min="0"
										:max="999"
										step-strictly
										:controls="false"
										placeholder="请输入顺序"
									/>
								</el-form-item>
							</el-col>
						</el-row>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="autoWidth" label="自动宽度">
									<el-radio-group v-model="state.formData.autoWidth">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="width" label="宽度">
									<el-input-number
										v-model="state.formData.width"
										:min="0"
										:max="999"
										step-strictly
										:controls="false"
										placeholder="请输入宽度"
									>
										<template #suffix>
											<span>px</span>
										</template>
									</el-input-number>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="smallWidth" label="最小宽度">
									<el-input-number
										v-model="state.formData.smallWidth"
										:min="0"
										:max="999"
										step-strictly
										:controls="false"
										placeholder="请输入最小宽度"
									>
										<template #suffix>
											<span>px</span>
										</template>
									</el-input-number>
								</el-form-item>
							</el-col>
						</el-row>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="fixed" label="固定">
									<el-radio-group v-model="state.formData.fixed">
										<el-radio value="">无</el-radio>
										<el-radio value="left">左侧</el-radio>
										<el-radio value="right">右侧</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="show" label="显示">
									<el-radio-group v-model="state.formData.show">
										<el-radio :value="true">显示</el-radio>
										<el-radio :value="false">隐藏</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="copy" label="复制">
									<el-radio-group v-model="state.formData.copy">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
						</el-row>

						<el-form-item prop="type" label="列类型">
							<el-radio-group v-model="state.formData.type">
								<el-radio value="">无</el-radio>
								<el-radio value="expand">可展开按钮</el-radio>
								<el-radio value="image">图片</el-radio>
								<el-radio value="date">日期</el-radio>
								<el-radio value="time">时间</el-radio>
								<el-radio value="dateTime">日期时间</el-radio>
								<el-radio value="d2">保留2位</el-radio>
								<el-radio value="d4">保留4位</el-radio>
								<el-radio value="d6">保留6位</el-radio>
								<el-radio value="gd2">保留2位(千分位)</el-radio>
								<el-radio value="gd4">保留4位(千分位)</el-radio>
								<el-radio value="gd6">保留6位(千分位)</el-radio>
								<el-radio value="timeInfo">时间信息</el-radio>
							</el-radio-group>
						</el-form-item>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="sortable" label="排序">
									<el-radio-group v-model="state.formData.sortable">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="link" label="链接">
									<el-radio-group v-model="state.formData.link">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="tag" label="标签">
									<el-radio-group v-model="state.formData.tag">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
						</el-row>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="sortableField" label="排序字段">
									<el-input v-model="state.formData.sortableField" maxlength="50" placeholder="请输入排序字段" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="clickEmit" label="点击事件名称">
									<el-input v-model="state.formData.clickEmit" maxlength="50" placeholder="请输入链接按钮事件名称" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="enum" label="字典名称">
									<el-input v-model="state.formData.enum" maxlength="50" placeholder="请输入字典名称" />
								</el-form-item>
							</el-col>
						</el-row>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="dateFix" label="日期格式化">
									<el-radio-group v-model="state.formData.dateFix">
										<el-radio :value="true">是</el-radio>
										<el-radio :value="false">否</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :span="18">
								<el-form-item prop="dateFormat" label="日期格式化">
									<el-radio-group v-model="state.formData.dateFormat">
										<el-radio value="">无</el-radio>
										<el-radio value="YYYY-MM-DD HH:mm:ss">年-月-日 时:分:秒</el-radio>
										<el-radio value="YYYY-MM-DD HH:mm">年-月-日 时:分</el-radio>
										<el-radio value="YYYY-MM-DD">年-月-日</el-radio>
										<el-radio value="YYYY-MM">年-月</el-radio>
										<el-radio value="YYYY">年</el-radio>
										<el-radio value="MM">月</el-radio>
										<el-radio value="DD">日</el-radio>
										<el-radio value="MM-DD">月-日</el-radio>
										<el-radio value="HH:mm:ss">时:分:秒</el-radio>
										<el-radio value="HH:mm">时:分</el-radio>
										<el-radio value="HH">时</el-radio>
										<el-radio value="mm:ss">分:秒</el-radio>
										<el-radio value="mm">分</el-radio>
										<el-radio value="ss">秒</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
						</el-row>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="slot" label="插槽">
									<el-input v-model="state.formData.slot" maxlength="50" placeholder="请输入插槽" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="authTag" label="权限标识">
									<template #label="{ label }">
										{{ label }}
										<el-text size="small" type="info">多个请用 , 分割。</el-text>
									</template>
									<el-input
										v-model="state.formData.authTagStr"
										maxlength="50"
										placeholder="请输入权限标识"
										@change="
											(value) => {
												state.formData.authTag = value
													.split(',')
													.map((v) => v.trim())
													.filter(Boolean);
											}
										"
									/>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item prop="dataDeleteField" label="数据删除">
									<el-radio-group v-model="state.formData.dataDeleteField" style="width: auto; padding-right: 30px">
										<el-radio value="">无</el-radio>
										<el-radio value="isDelete">删除</el-radio>
									</el-radio-group>
									<el-input v-model="state.formData.dataDeleteField" placeholder="请输入数据删除字段" style="width: auto" />
								</el-form-item>
							</el-col>
						</el-row>

						<el-divider content-position="left">
							<el-text type="info">
								<el-icon>
									<StarFilled />
								</el-icon>
								搜索配置
							</el-text>
						</el-divider>
						<el-form-item prop="pureSearch" label="纯搜索项">
							<el-radio-group v-model="state.formData.pureSearch">
								<el-radio :value="true">是</el-radio>
								<el-radio :value="false">否</el-radio>
							</el-radio-group>
						</el-form-item>
						<el-form-item prop="searchEl" label="搜索项">
							<template #label="{ label }">
								{{ label }}
								<el-text class="search-component-tip" size="small" type="info">
									支持任何全局组件，如没有选项，请自行在代码中添加(src\views\dev\tableConfig\config\components\advancedConfig.vue)
								</el-text>
							</template>
							<el-radio-group v-model="state.formData.searchEl" style="width: auto; padding-right: 30px">
								<el-radio value="">无</el-radio>
								<el-radio value="el-input">输入框</el-radio>
								<el-radio value="el-input-number">数字输入框</el-radio>
								<el-radio value="el-select">选择器下拉框</el-radio>
								<el-radio value="el-select-v2">选择器下拉框V2</el-radio>
								<el-radio value="el-tree-select">树形选择器</el-radio>
								<el-radio value="el-cascader">级联选择器</el-radio>
								<el-radio value="el-date-picker">日期选择器</el-radio>
								<el-radio value="el-time-picker">时间选择器</el-radio>
								<el-radio value="el-time-select">时间选择器下拉框</el-radio>
								<el-radio value="el-switch">开关</el-radio>
								<el-radio value="slot">插槽</el-radio>
								<el-radio value="ApplicationSelect">应用</el-radio>
								<el-radio value="TenantSelectPage">租户</el-radio>
								<el-radio value="AccountSelectPage">账号</el-radio>
								<el-radio value="DepartmentTreeSelect">部门</el-radio>
								<el-radio value="EmployeeSelectPage">职员</el-radio>
							</el-radio-group>
							<el-input v-model="state.formData.searchEl" placeholder="请输入搜索项" style="width: auto" />
						</el-form-item>

						<el-row :gutter="24">
							<el-col :span="6">
								<el-form-item prop="searchKey" label="搜索项Key">
									<el-input v-model="state.formData.searchKey" maxlength="50" placeholder="请输入搜索项Key" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="searchLabel" label="搜索项名称">
									<el-input v-model="state.formData.searchLabel" maxlength="50" placeholder="请输入搜索项名称" />
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="searchOrder" label="搜索项排序">
									<el-input-number
										v-model="state.formData.searchOrder"
										:min="0"
										:max="999"
										step-strictly
										:controls="false"
										placeholder="请输入搜索项排序"
									/>
								</el-form-item>
							</el-col>
							<el-col :span="6">
								<el-form-item prop="searchSlot" label="搜索项插槽">
									<el-input v-model="state.formData.searchSlot" maxlength="50" placeholder="请输入搜索项插槽" />
								</el-form-item>
							</el-col>
						</el-row>
					</el-form>
				</el-scrollbar>
			</el-tab-pane>
			<el-tab-pane :name="2" label="搜索高级配置">
				<DevTableConfigObjectTable v-model="state.formData.searchConfig" />
			</el-tab-pane>
			<el-tab-pane :name="3" label="其他高级配置">
				<DevTableConfigObjectTable v-model="state.formData.otherConfig" />
			</el-tab-pane>
		</el-tabs>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { StarFilled } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import { FaDialog } from "fast-element-plus";
import { withDefineType } from "@fast-china/utils";
import DevTableConfigObjectTable from "./objectTable.vue";
import type { FaDialogInstance } from "fast-element-plus";
import type { FaTableColumnCtx } from "@/api/services/Center/table/models/FaTableColumnCtx";

defineOptions({
	name: "DevTableConfigAdvancedConfig",
});

const emit = defineEmits(["change"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<FaTableColumnCtx & { authTagStr?: string }>({}),
	rowIndex: withDefineType<number>(),
	activeTab: 1,
});

const handleConfirm = () => {
	if (state.formData.searchConfig?.length > 0) {
		for (let index = 0; index < state.formData.searchConfig.length; index++) {
			const item = state.formData.searchConfig[index];
			if (!item.prop) {
				ElMessage.error(`搜索高级配置，第【${index + 1}】行，字段名称不能为空！`);
				state.activeTab = 2;
				return;
			}
			if (!item.value) {
				ElMessage.error(`搜索高级配置，第【${index + 1}】行，字段值不能为空！`);
				state.activeTab = 2;
				return;
			}
		}
	} else {
		state.formData.searchConfig = [];
	}
	if (state.formData.otherConfig?.length > 0) {
		for (let index = 0; index < state.formData.otherConfig.length; index++) {
			const item = state.formData.otherConfig[index];
			if (!item.prop) {
				ElMessage.error(`其他高级配置，第【${index + 1}】行，字段名称不能为空！`);
				state.activeTab = 3;
				return;
			}
			if (!item.value) {
				ElMessage.error(`其他高级配置，第【${index + 1}】行，字段值不能为空！`);
				state.activeTab = 3;
				return;
			}
		}
	} else {
		state.formData.otherConfig = [];
	}
	emit("change", state.formData, state.rowIndex);
	faDialogRef.value.close();
};

const edit = (row: FaTableColumnCtx, rowIndex: number) => {
	faDialogRef.value.open(() => {
		state.formData = row;
		state.rowIndex = rowIndex;
	});
};

defineExpose({
	element: faDialogRef,
	edit,
});
</script>

<style lang="scss" scoped>
.advanced-config-tabs {
	height: inherit;

	> :deep(.el-tabs__content) {
		display: flex;
		flex: 1;
		flex-direction: column;
		height: 100%;

		> .el-tab-pane {
			display: flex;
			flex: 1;
			flex-direction: column;
			height: 100%;

			> :first-child {
				height: 100%;
			}
		}
	}
}

.advanced-config-form {
	// 为 24px gutter 的右侧负外边距预留空间，保留原有左侧对齐。
	padding-right: 12px;
}

.search-component-tip {
	overflow-wrap: anywhere;
}

@media (max-width: 520px) {
	.advanced-config-form {
		:deep(.el-col-6) {
			flex-basis: 50%;
			max-width: 50%;
		}

		:deep(.el-col-12),
		:deep(.el-col-18) {
			flex-basis: 100%;
			max-width: 100%;
		}
	}
}
</style>

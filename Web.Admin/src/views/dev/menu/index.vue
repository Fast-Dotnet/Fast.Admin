<template>
	<div>
		<div class="fa__display_lr-r">
			<ApplicationTree @change="handleApplicationChange" />
			<FaTable ref="faTableRef" row-key="menuId" :request-api="menuApi.queryMenuPaged" hide-search-time :pagination="false" default-expand-all>
				<!-- 表格按钮操作区域 -->
				<template #header>
					<el-button v-auth="'Menu:Add'" type="primary" :icon="Plus" @click="editFormRef.add()">新增</el-button>
				</template>

				<FaTableColumn prop="menuName" label="菜单名称" fixed="left" width="200" small-width="180" sortable>
					<template #default="{ row }: { row: QueryMenuPagedOutput }">
						<FaIcon v-if="row.webIcon" style="margin-right: 5px" size="16" :name="row.webIcon" />
						<span>{{ row.menuName }}</span>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="menuCode" label="菜单编码" width="200" small-width="180" sortable tag />
				<FaTableColumn prop="menuTitle" label="菜单标题" width="150" small-width="130" sortable />
				<FaTableColumn
					prop="menuType"
					label="菜单类型"
					width="120"
					small-width="100"
					sortable
					tag
					:enum="appStore.getDictionary('MenuTypeEnum')"
				/>
				<FaTableColumn prop="visible" label="显示" width="100" small-width="80" sortable tag :enum="appStore.getDictionary('BooleanEnum')" />
				<FaTableColumn
					prop="status"
					label="状态"
					width="100"
					small-width="80"
					sortable
					tag
					:enum="appStore.getDictionary('CommonStatusEnum')"
				/>
				<FaTableColumn prop="edition" label="版本" width="100" small-width="80" sortable tag :enum="appStore.getDictionary('EditionEnum')" />
				<FaTableColumn prop="webRouter" label="Web端" width="300" small-width="280" sortable>
					<template #default="{ row }: { row: QueryMenuPagedOutput }">
						<div style="display: flex; align-items: center; gap: 5px">
							<Tag size="small" name="CommonStatusEnum" :value="row.hasWeb ? CommonStatusEnum.Enable : CommonStatusEnum.Disable" />
							<FaIcon v-if="row.webIcon" size="16" :name="row.webIcon" />
							<el-tag v-if="row.webRouter" type="primary" effect="plain">{{ row.webRouter }}</el-tag>
						</div>
						<el-tag v-if="row.webComponent" type="info" effect="plain">{{ row.webComponent }}</el-tag>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="mobileRouter" label="移动端" width="300" small-width="280" sortable>
					<template #default="{ row }: { row: QueryMenuPagedOutput }">
						<div style="display: flex; align-items: center; gap: 5px">
							<Tag size="small" name="CommonStatusEnum" :value="row.hasMobile ? CommonStatusEnum.Enable : CommonStatusEnum.Disable" />
							<FaImage
								v-if="row.mobileIcon"
								src="https://gitee.com/FastDotnet/Fast.Admin/raw/master/Fast.png@!thumb"
								original
								:preview="false"
							/>
						</div>
						<el-tag v-if="row.mobileRouter" type="info" effect="plain">{{ row.mobileRouter }}</el-tag>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="desktopRouter" label="桌面端" width="300" small-width="280" sortable>
					<template #default="{ row }: { row: QueryMenuPagedOutput }">
						<div style="display: flex; align-items: center; gap: 5px">
							<Tag size="small" name="CommonStatusEnum" :value="row.hasDesktop ? CommonStatusEnum.Enable : CommonStatusEnum.Disable" />
							<el-tag v-if="row.desktopIcon" type="primary" effect="plain">{{ row.desktopIcon }}</el-tag>
						</div>
						<el-tag v-if="row.desktopRouter" type="info" effect="plain">{{ row.desktopRouter }}</el-tag>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="link" label="内链/外链地址" width="200" small-width="180" sortable>
					<template #default="{ row }: { row: QueryMenuPagedOutput }">
						<el-link type="info" target="_blank" rel="noopener noreferrer" :href="row.link">{{ row.link }}</el-link>
					</template>
				</FaTableColumn>
				<FaTableColumn prop="sort" label="排序" width="100" small-width="80" sortable />
				<FaTableColumn prop="appName" label="应用名称" width="150" small-width="130" sortable />
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
				<template #operation="{ row }: { row: QueryMenuPagedOutput }">
					<el-button v-auth="'Menu:Edit'" size="small" plain type="primary" @click="editFormRef.edit(row.menuId)">编辑</el-button>
					<el-button v-auth="'Menu:Delete'" size="small" plain type="warning" @click="handleDelete(row)">删除</el-button>
					<el-button
						v-auth="'Menu:Status'"
						v-if="row.status == CommonStatusEnum.Enable"
						size="small"
						plain
						type="danger"
						@click="handleChangeStatus(row)"
					>
						禁用
					</el-button>
					<el-button v-auth="'Menu:Status'" v-else size="small" plain type="warning" @click="handleChangeStatus(row)">启用</el-button>
				</template>
			</FaTable>
		</div>
		<MenuEdit ref="editFormRef" @ok="faTableRef.refresh()" />
	</div>
</template>

<script lang="ts" setup>
import { useTemplateRef } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { menuApi } from "@/api/services/Center/menu";
import { useApp } from "@/stores";
import MenuEdit from "./edit/index.vue";
import type { ElSelectorOutput, FaTableInstance } from "fast-element-plus";
import type { QueryMenuPagedOutput } from "@/api/services/Center/menu/models/QueryMenuPagedOutput";

defineOptions({
	name: "DevMenu",
});

const appStore = useApp();

const faTableRef = useTemplateRef<FaTableInstance>("faTableRef");
const editFormRef = useTemplateRef<InstanceType<typeof MenuEdit>>("editFormRef");

/** 应用更改 */
const handleApplicationChange = (data: ElSelectorOutput) => {
	faTableRef.value.searchParam.appId = data.value;
	faTableRef.value.refresh();
};

/** 处理删除 */
const handleDelete = (row: QueryMenuPagedOutput) => {
	const { menuId, rowVersion } = row;
	ElMessageBox.confirm("确定要删除菜单？", { type: "warning" }).then(async () => {
		await menuApi.deleteMenu({ menuId, rowVersion });
		ElMessage.success("删除成功！");
		await faTableRef.value?.refresh();
	});
};

/** 处理状态变更 */
const handleChangeStatus = (row: QueryMenuPagedOutput) => {
	const { menuId, status, rowVersion } = row;
	ElMessageBox.confirm(`确定${status === CommonStatusEnum.Enable ? "禁用" : "启用"}菜单？`, { type: "warning" }).then(async () => {
		await menuApi.changeStatus({
			menuId,
			rowVersion,
		});
		ElMessage.success("操作成功！");
		await faTableRef.value?.refresh();
	});
};
</script>

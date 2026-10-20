<template>
	<FaDialog
		ref="faDialogRef"
		width="1000"
		full-height
		:title="state.dialogTitle"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="
			() => {
				state.formData = {
					menuIds: [],
					buttonIds: [],
				};
				state.menuList = [];
			}
		"
	>
		<el-collapse :model-value="state.menuList.map((item) => item.value)">
			<el-collapse-item v-for="menu in state.menuList" :key="menu.value" :name="menu.value">
				<template #title>
					{{ menu.label }}
					<el-checkbox border :model-value="isMenuChecked(menu.value)" @click.stop @change="handleMenuChange(menu.value, $event)">
						<template #default>{{ getButtonCheckedCount(menu.value) }} / {{ menu.children.length }}</template>
					</el-checkbox>
					<el-checkbox
						class="checkbox__right"
						border
						:model-value="isButtonAllChecked(menu.value)"
						:indeterminate="isButtonIndeterminate(menu.value)"
						@click.stop
						@change="handleButtonAllChange(menu.value, $event)"
					>
						<template #default>全选</template>
					</el-checkbox>
				</template>
				<div v-if="menu.children.length > 0" class="item__content__div">
					<span>操作</span>
					<el-checkbox-group v-model="state.formData.buttonIds">
						<el-checkbox v-for="button in menu.children" :key="button.value" border :value="button.value">
							{{ button.label }}
							<FaIcon v-if="button.data?.hasDesktop" name="el-icon-Monitor" />
							<FaIcon v-if="button.data?.hasWeb" name="el-icon-ChromeFilled" />
							<FaIcon v-if="button.data?.hasMobile" name="el-icon-Iphone" />
						</el-checkbox>
					</el-checkbox-group>
				</div>
			</el-collapse-item>
		</el-collapse>
	</FaDialog>
</template>

<script lang="ts" setup>
import { reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { roleApi } from "@/api/services/Admin/role";
import type { CheckboxValueType } from "element-plus";
import type { ElSelectorOutput, FaDialogInstance } from "fast-element-plus";
import type { RoleAuthInput } from "@/api/services/Admin/role/models/RoleAuthInput";

defineOptions({
	name: "AdminRoleAuthEdit",
});

const emit = defineEmits(["ok"]);

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");

const state = reactive({
	formData: withDefineType<RoleAuthInput>({
		menuIds: [],
		buttonIds: [],
	}),
	dialogTitle: "角色授权",
	menuList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const isMenuChecked = (menuId: string) => {
	return state.formData.menuIds.includes(menuId) || false;
};

const isButtonAllChecked = (menuId: string) => {
	const menu = state.menuList.find((item) => item.value === menuId);
	if (!menu || menu.children.length === 0) {
		return false;
	}
	const buttonIds = menu.children.map((item) => item.value);
	return buttonIds.every((id) => state.formData.buttonIds?.includes(id));
};

const isButtonIndeterminate = (menuId: string) => {
	const menu = state.menuList.find((item) => item.value === menuId);
	if (!menu || menu.children.length === 0) {
		return false;
	}
	const buttonIds = menu.children.map((item) => item.value);
	const checkedCount = buttonIds.filter((id) => state.formData.buttonIds?.includes(id)).length;
	return checkedCount > 0 && checkedCount < buttonIds.length;
};

const getButtonCheckedCount = (menuId: string) => {
	const menu = state.menuList.find((item) => item.value === menuId);
	if (!menu) {
		return 0;
	}
	const buttonIds = menu.children.map((item) => item.value);
	return buttonIds.filter((id) => state.formData.buttonIds?.includes(id)).length;
};

const handleMenuChange = (menuId: string, val: CheckboxValueType) => {
	if (val) {
		// 选中菜单
		if (!state.formData.menuIds?.includes(menuId)) {
			state.formData.menuIds = [...(state.formData.menuIds || []), menuId];
		}
	} else {
		// 取消选中菜单
		state.formData.menuIds = state.formData.menuIds?.filter((id) => id !== menuId) || [];
	}
};

const handleButtonAllChange = (menuId: string, val: CheckboxValueType) => {
	const menu = state.menuList.find((item) => item.value === menuId);
	if (!menu) {
		return;
	}

	const allButtonIds = menu.children.map((item) => item.value);

	if (val) {
		// 全选：将该菜单下所有按钮 ID 添加到 buttonIds
		const newButtonIds = allButtonIds.filter((id) => !state.formData.buttonIds?.includes(id));
		state.formData.buttonIds = [...(state.formData.buttonIds || []), ...newButtonIds];

		// 同时选中菜单
		if (!state.formData.menuIds?.includes(menuId)) {
			state.formData.menuIds = [...(state.formData.menuIds || []), menuId];
		}
	} else {
		// 取消全选：移除该菜单下所有按钮 ID
		state.formData.buttonIds = state.formData.buttonIds?.filter((id) => !allButtonIds.includes(id)) || [];

		// 同时取消选中菜单
		state.formData.menuIds = state.formData.menuIds?.filter((id) => id !== menuId) || [];
	}
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await roleApi.roleAuth(state.formData);
		ElMessage.success("授权成功！");
		emit("ok");
	});
};

const open = (roleId: string) => {
	faDialogRef.value.open(async () => {
		const apiRes = await roleApi.queryRoleAuthMenu({ roleId });
		state.formData = apiRes;
		state.menuList = await roleApi.queryAuthMenu();
		state.dialogTitle = `角色授权 - ${apiRes.roleName}`;
	});
};

defineExpose({
	element: faDialogRef,
	open,
});
</script>

<style lang="scss" scoped>
.el-collapse {
	border: none;

	.el-collapse-item {
		border: 1px solid var(--el-collapse-border-color);

		&:nth-child(n + 2) {
			margin-top: 20px;
		}

		:deep(.el-collapse-item__header) {
			padding-left: 20px;
			border: none;
			transition: none;

			&.is-active {
				border-bottom: 1px solid var(--el-collapse-border-color);
			}

			.el-checkbox {
				margin-left: 10px;
				transition:
					border-color 0.25s cubic-bezier(0.71, -0.46, 0.29, 1.46),
					background-color 0.25s cubic-bezier(0.71, -0.46, 0.29, 1.46),
					outline 0.25s cubic-bezier(0.71, -0.46, 0.29, 1.46);

				&:hover {
					border-color: var(--el-checkbox-checked-input-border-color);
				}
			}

			.el-collapse-item__title {
				display: flex;
				align-items: center;
			}

			.checkbox__right {
				margin-left: auto;
				margin-right: 12px;
			}
		}

		:deep(.el-collapse-item__wrap) {
			border: none;

			.el-collapse-item__content {
				padding-bottom: 0;
			}
		}
	}
}

.item__content__div {
	display: grid;
	grid-template-columns: repeat(24, minmax(0, 1fr));

	&:nth-child(2) {
		border-top: 1px solid var(--el-collapse-border-color);
	}

	> span {
		display: flex;
		grid-column: span 2;
		justify-content: center;
		align-items: center;
		border-right: 1px solid var(--el-collapse-border-color);
	}

	> .el-checkbox-group {
		display: flex;
		flex-wrap: wrap;
		grid-column: span 22;
		padding: 5px 10px 5px;

		.el-checkbox {
			margin: 5px 30px 5px 0;

			&:hover {
				border-color: var(--el-checkbox-checked-input-border-color);
			}
		}
	}
}

@media (max-width: 767px) {
	.el-collapse {
		.el-collapse-item {
			:deep(.el-collapse-item__header) {
				height: auto;
				min-height: var(--el-collapse-header-height);
				padding-left: 10px;

				.el-collapse-item__title {
					flex-wrap: wrap;
					row-gap: 4px;
					min-width: 0;
					padding-block: 6px;
					line-height: 1.5;
					overflow-wrap: anywhere;
				}
			}
		}
	}

	.item__content__div {
		> span {
			grid-column: 1 / -1;
			min-height: 32px;
			border-right: 0;
			border-bottom: 1px solid var(--el-collapse-border-color);
		}

		> .el-checkbox-group {
			grid-column: 1 / -1;
			gap: 8px;
			padding: 10px;

			.el-checkbox {
				max-width: 100%;
				height: auto;
				min-height: var(--el-checkbox-height);
				margin: 0;
				padding-block: 5px;

				:deep(.el-checkbox__label) {
					white-space: normal;
					overflow-wrap: anywhere;
				}
			}
		}
	}
}
</style>

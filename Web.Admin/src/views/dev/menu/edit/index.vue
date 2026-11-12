<template>
	<FaDialog
		ref="faDialogRef"
		width="1800"
		full-height
		:title="state.dialogTitle"
		confirm-button-text="保存"
		@confirm-click="handleConfirm"
		@close="faFormRef.resetFields()"
	>
		<FaForm ref="faFormRef" :model="state.formData" :rules="state.formRules" cols="4" label-width="120">
			<FaLayoutGridItem span="4">
				<el-divider content-position="left">菜单信息</el-divider>
			</FaLayoutGridItem>

			<FaFormItem prop="appId" label="应用">
				<ApplicationSelect v-model="state.formData.appId" v-model:app-name="state.formData.appName" />
			</FaFormItem>
			<FaFormItem prop="parentId" label="父级">
				<el-cascader
					v-model="state.formData.parentId"
					:options="state.menuList"
					placeholder="请选择父级菜单"
					filterable
					clearable
					:props="{ checkStrictly: true, checkOnClickNode: true, emitPath: false }"
				/>
			</FaFormItem>
			<FaFormItem prop="menuType" label="菜单类型">
				<RadioGroup name="MenuTypeEnum" v-model="state.formData.menuType" />
			</FaFormItem>
			<FaFormItem prop="edition" label="版本" span="4">
				<RadioGroup name="EditionEnum" v-model="state.formData.edition" />
			</FaFormItem>
			<FaFormItem prop="roleType" label="角色" span="4">
				<el-checkbox-group v-model="state.formData.roleTypes">
					<el-checkbox v-for="(item, index) in roleTypeEnum" :key="index" :label="item.label" :value="item.value" />
				</el-checkbox-group>
			</FaFormItem>
			<FaFormItem prop="menuCode" label="菜单编码">
				<el-input v-model="state.formData.menuCode" maxlength="50" placeholder="请输入菜单编码" />
			</FaFormItem>
			<FaFormItem prop="menuName" label="菜单名称">
				<el-input v-model="state.formData.menuName" maxlength="20" placeholder="请输入菜单名称" />
			</FaFormItem>
			<FaFormItem prop="menuTitle" label="菜单标题">
				<el-input v-model="state.formData.menuTitle" maxlength="20" placeholder="请输入菜单标题" />
			</FaFormItem>
			<FaFormItem prop="sort" label="排序" tips="从小到大">
				<el-input-number v-model="state.formData.sort" :min="1" :max="99999" placeholder="请输入排序" />
			</FaFormItem>
			<FaFormItem prop="visible" label="显示">
				<RadioGroup button name="BooleanEnum" v-model="state.formData.visible" />
			</FaFormItem>
			<FaFormItem v-if="state.dialogState !== 'add'" prop="status" label="状态">
				<RadioGroup button name="CommonStatusEnum" v-model="state.formData.status" />
			</FaFormItem>

			<FaLayoutGridItem span="4">
				<el-divider content-position="left">Web端</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="hasWeb" label="Web端">
				<el-checkbox v-model="state.formData.hasWeb">Web端</el-checkbox>
			</FaFormItem>
			<template v-if="state.formData.hasWeb">
				<FaFormItem prop="webIcon" label="图标">
					<IconSelect
						v-model="state.formData.webIcon"
						placeholder="请选择Web端图标"
						:show-all-levels="false"
						:props="{ emitPath: false }"
					/>
				</FaFormItem>
				<template v-if="state.formData.menuType === MenuTypeEnum.Menu">
					<FaFormItem prop="webComponent" label="组件地址">
						<el-cascader
							v-model="state.componentValue"
							:options="state.componentList"
							placeholder="请选择Web端组件地址"
							filterable
							clearable
							@change="handleComponentChange"
						/>
					</FaFormItem>
					<FaFormItem prop="webRouter" label="路由地址">
						<el-input v-model="state.formData.webRouter" maxlength="200" placeholder="请输入Web端路由地址" />
					</FaFormItem>
					<FaFormItem prop="webTab" label="导航栏">
						<RadioGroup button name="BooleanEnum" v-model="state.formData.webTab" />
					</FaFormItem>
					<FaFormItem prop="webKeepAlive" label="KeepAlive">
						<RadioGroup button name="BooleanEnum" v-model="state.formData.webKeepAlive" />
					</FaFormItem>
				</template>
			</template>

			<FaLayoutGridItem span="4">
				<el-divider content-position="left">移动端</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="hasMobile" label="移动端">
				<el-checkbox v-model="state.formData.hasMobile">移动端</el-checkbox>
			</FaFormItem>
			<template v-if="state.formData.hasMobile">
				<FaFormItem prop="mobileIcon" label="图标">
					<el-input type="textarea" v-model="state.formData.mobileIcon" :rows="2" maxlength="200" placeholder="请输入移动端图标" />
				</FaFormItem>
				<FaFormItem v-if="state.formData.menuType === MenuTypeEnum.Menu" prop="mobileRouter" label="路由地址">
					<el-input type="textarea" v-model="state.formData.mobileRouter" :rows="2" maxlength="200" placeholder="请输入移动端路由地址" />
				</FaFormItem>
			</template>

			<FaLayoutGridItem span="4">
				<el-divider content-position="left">桌面端</el-divider>
			</FaLayoutGridItem>
			<FaFormItem prop="hasDesktop" label="桌面端">
				<el-checkbox v-model="state.formData.hasDesktop">桌面端</el-checkbox>
			</FaFormItem>
			<template v-if="state.formData.hasDesktop">
				<FaFormItem prop="desktopIcon" label="图标">
					<el-input type="textarea" v-model="state.formData.desktopIcon" :rows="2" maxlength="200" placeholder="请输入桌面端图标" />
				</FaFormItem>
				<FaFormItem v-if="state.formData.menuType === MenuTypeEnum.Menu" prop="desktopRouter" label="路由地址">
					<el-input type="textarea" v-model="state.formData.desktopRouter" :rows="2" maxlength="200" placeholder="请输入桌面端路由地址" />
				</FaFormItem>
			</template>

			<template v-if="state.formData.menuType === MenuTypeEnum.Internal || state.formData.menuType === MenuTypeEnum.Outside">
				<FaLayoutGridItem span="4">
					<el-divider content-position="left">其他</el-divider>
				</FaLayoutGridItem>
				<FaFormItem prop="link" label="内链/外链地址">
					<el-input type="textarea" v-model="state.formData.link" :rows="2" maxlength="200" placeholder="请输入内链/外链地址" />
				</FaFormItem>
			</template>
		</FaForm>

		<ButtonTable v-model="state.formData.buttonList" />
	</FaDialog>
</template>

<script setup lang="ts">
import { onMounted, reactive, useTemplateRef } from "vue";
import { ElMessage } from "element-plus";
import { withDefineType } from "@fast-china/utils";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";
import { menuApi } from "@/api/services/Center/menu";
import routerPath from "@/router/routes.generated.json";
import { useApp } from "@/stores";
import ButtonTable from "./components/buttonTable.vue";
import type { CascaderValue, FormRules } from "element-plus";
import type { ElSelectorOutput, FaDialogInstance, FaFormInstance } from "fast-element-plus";
import type { AddMenuInput } from "@/api/services/Center/menu/models/AddMenuInput";
import type { EditMenuInput } from "@/api/services/Center/menu/models/EditMenuInput";

defineOptions({
	name: "DevMenuEdit",
});

const emit = defineEmits(["ok"]);

const appStore = useApp();
const roleTypeEnum = appStore.getDictionary("RoleTypeEnum");
const routerPathMap: Readonly<Record<string, string>> = routerPath;

const faDialogRef = useTemplateRef<FaDialogInstance>("faDialogRef");
const faFormRef = useTemplateRef<FaFormInstance>("faFormRef");

type IFormData = EditMenuInput &
	AddMenuInput & {
		appId?: string;
		appName?: string;
		roleTypes?: RoleTypeEnum[];
	};

const state = reactive({
	formData: withDefineType<IFormData>({}),
	formRules: withDefineType<FormRules<IFormData>>({
		appId: [{ required: true, message: "请选择应用", trigger: "change" }],
		menuType: [{ required: true, message: "请选择菜单类型", trigger: "change" }],
		menuCode: [{ required: true, message: "请输入菜单编码", trigger: "blur" }],
		menuName: [{ required: true, message: "请输入菜单名称", trigger: "blur" }],
		menuTitle: [{ required: true, message: "请输入菜单标题", trigger: "blur" }],
		// desktopIcon: [{ required: true, message: "请输入桌面端图标", trigger: "blur" }],
		// desktopRouter: [{ required: true, message: "请输入桌面端路由地址", trigger: "blur" }],
		// webIcon: [{ required: true, message: "请输入Web端图标", trigger: "blur" }],
		webRouter: [{ required: true, message: "请输入Web端路由地址", trigger: "blur" }],
		webComponent: [{ required: true, message: "请选择Web端组件地址", trigger: "change" }],
		// mobileIcon: [{ required: true, message: "请输入移动端图标", trigger: "blur" }],
		// mobileRouter: [{ required: true, message: "请输入移动端路由地址", trigger: "blur" }],
		link: [{ required: true, message: "请输入内链/外链地址", trigger: "blur" }],
		sort: [{ required: true, message: "请输入排序", trigger: "blur" }],
	}),
	dialogState: withDefineType<IPageStateType>("add"),
	dialogTitle: "菜单",
	componentList: withDefineType<ElSelectorOutput<string>[]>([]),
	componentValue: [],
	menuList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const handleComponentChange = (value: CascaderValue) => {
	if (!Array.isArray(value) || value.length === 0) {
		state.formData.webRouter = "";
		state.formData.webComponent = "";
		return;
	}
	const componentParts: string[] = [];
	for (const part of value) {
		if (typeof part !== "string") {
			state.formData.webRouter = "";
			state.formData.webComponent = "";
			return;
		}
		componentParts.push(part);
	}

	// 拼接路径并去掉 .vue
	const componentPath = componentParts.join("/").replace(/\.vue$/, "");

	if (routerPathMap[`/src/views/${componentPath}.vue`]) {
		// 去掉末尾 /index
		state.formData.webRouter = `/${componentPath}`.replace(/\/index$/, "");
		state.formData.webComponent = componentPath;
	} else {
		state.formData.webRouter = "";
		state.formData.webComponent = "";
	}
};

const handleConfirm = () => {
	faDialogRef.value.close(async () => {
		await faFormRef.value.validateScrollToField();
		const { formData, dialogState } = state;
		if (formData.roleTypes?.length > 0) {
			let _roleType = 0;
			formData.roleTypes.forEach((item) => (_roleType |= item));
			formData.roleType = _roleType;
		}

		switch (dialogState) {
			case "add":
				await menuApi.addMenu(formData);
				ElMessage.success("新增成功！");
				break;
			case "edit":
				await menuApi.editMenu(formData);
				ElMessage.success("保存成功！");
				break;
		}
		emit("ok");
	});
};

const handleFlagsEnum = () => {
	state.formData.roleTypes = [];
	if (state.formData.roleType) {
		for (const key in RoleTypeEnum) {
			const item = RoleTypeEnum[key];
			if (typeof item !== "number") {
				continue;
			}
			if (item === 0) {
				continue;
			}
			if ((state.formData.roleType & item) !== 0) {
				state.formData.roleTypes.push(item);
			}
		}
	}
};

const add = () => {
	faDialogRef.value.open(async () => {
		state.dialogState = "add";
		state.dialogTitle = "添加菜单";
		state.formData = {
			parentId: undefined,
			menuType: MenuTypeEnum.Catalog,
			roleType: RoleTypeEnum.Default,
			edition: EditionEnum.None,
			visible: true,
			hasWeb: true,
			webTab: false,
			webKeepAlive: false,
			hasMobile: false,
			hasDesktop: false,
			status: CommonStatusEnum.Enable,
			buttonList: [],
		};
		state.menuList = await menuApi.menuSelector();
	});
};

const edit = (menuId: string) => {
	faDialogRef.value.open(async () => {
		state.dialogState = "edit";
		const apiRes = await menuApi.queryMenuDetail(menuId);
		if (apiRes.parentId === "0") {
			apiRes.parentId = undefined;
		}
		state.formData = apiRes;
		state.menuList = await menuApi.menuSelector();
		state.componentValue = apiRes.webComponent
			? apiRes.webComponent.split("/").map((part, index, arr) => (index === arr.length - 1 ? `${part}.vue` : part))
			: [];
		state.dialogTitle = `编辑菜单 - ${apiRes.menuName}`;
		handleFlagsEnum();
	});
};

onMounted(() => {
	state.componentList = [];

	for (const path in routerPathMap) {
		const label = routerPathMap[path];
		const cleanPath = path.replace(/^\/src\/views\//, "");
		const parts = cleanPath.split("/").filter(Boolean);

		let currentLevel = state.componentList;

		for (let i = 0; i < parts.length; i++) {
			const part = parts[i];
			let node = currentLevel.find((item) => item.value === part);

			if (!node) {
				node = {
					value: part,
					label: part,
					children: [],
				};
				currentLevel.push(node);
			}

			if (i === parts.length - 1) {
				node.label = label;
				delete node.children;
			}

			currentLevel = node.children || [];
		}
	}
});

defineExpose({
	element: faDialogRef,
	add,
	edit,
});
</script>

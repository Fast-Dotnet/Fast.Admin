<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-cell-group title="菜单信息">
				<FaApplicationSelect v-model="state.formData.appId" v-model:app-name="state.formData.appName" prop="appId" />
				<FaCellPicker title="父级" prop="parentId" :value="state.formData.parentName" placeholder="请选择父级菜单">
					<template #default="{ visible }">
						<FaTreePicker
							title="父级菜单"
							v-model="state.formData.parentId"
							v-model:label="state.formData.parentName"
							v-model:visible="visible.value"
							:request-api="menuApi.menuSelector"
							disabled-search
							clearable
							check-strictly
						/>
					</template>
				</FaCellPicker>
				<wd-form-item title="菜单类型" prop="menuType">
					<FaRadioGroup name="MenuTypeEnum" v-model="state.formData.menuType" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="版本" prop="edition">
					<FaRadioGroup name="EditionEnum" v-model="state.formData.edition" type="button" />
				</wd-form-item>
				<wd-form-item title="角色" prop="roleType">
					<FaCheckboxGroup name="RoleTypeEnum" v-model="state.formData.roleType" type="square" />
				</wd-form-item>
				<wd-form-item title="菜单编码" prop="menuCode">
					<wd-input v-model="state.formData.menuCode" clearable :maxlength="50" placeholder="请输入菜单编码" />
				</wd-form-item>
				<wd-form-item title="菜单名称" prop="menuName">
					<wd-input v-model="state.formData.menuName" clearable :maxlength="20" placeholder="请输入菜单名称" />
				</wd-form-item>
				<wd-form-item title="菜单标题" prop="menuTitle">
					<wd-input v-model="state.formData.menuTitle" clearable :maxlength="20" placeholder="请输入菜单标题" />
				</wd-form-item>
				<wd-form-item title="排序" prop="sort">
					<wd-input v-model="state.formData.sort" type="number" placeholder="请输入排序" />
				</wd-form-item>
				<wd-form-item title="显示" prop="visible">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.visible" direction="horizontal" />
				</wd-form-item>
				<wd-form-item v-if="state.pageMode === 'edit'" title="状态" prop="status">
					<FaRadioGroup name="CommonStatusEnum" v-model="state.formData.status" direction="horizontal" />
				</wd-form-item>
			</wd-cell-group>

			<wd-cell-group title="Web端">
				<wd-form-item title="Web端" prop="hasWeb">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.hasWeb" direction="horizontal" />
				</wd-form-item>
				<template v-if="state.formData.hasWeb">
					<wd-form-item title="图标" prop="webIcon">
						<wd-input v-model="state.formData.webIcon" clearable :maxlength="200" placeholder="请输入图标" />
					</wd-form-item>
					<template v-if="state.formData.menuType === MenuTypeEnum.Menu">
						<wd-form-item title="组件地址" prop="webComponent">
							<wd-input v-model="state.formData.webComponent" clearable :maxlength="200" placeholder="请输入组件地址" />
						</wd-form-item>
						<wd-form-item title="路由地址" prop="webRouter">
							<wd-textarea
								v-model="state.formData.webRouter"
								custom-style="--fa-textarea-height: 75px"
								clearable
								:maxlength="200"
								placeholder="请输入路由地址"
							/>
						</wd-form-item>
						<wd-form-item title="导航栏" prop="webTab">
							<FaRadioGroup name="BooleanEnum" v-model="state.formData.webTab" direction="horizontal" />
						</wd-form-item>
						<wd-form-item title="KeepAlive" prop="webKeepAlive">
							<FaRadioGroup name="BooleanEnum" v-model="state.formData.webKeepAlive" direction="horizontal" />
						</wd-form-item>
					</template>
				</template>
			</wd-cell-group>

			<wd-cell-group title="移动端">
				<wd-form-item title="移动端" prop="hasMobile">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.hasMobile" direction="horizontal" />
				</wd-form-item>
				<template v-if="state.formData.hasMobile">
					<wd-form-item title="图标" prop="mobileIcon">
						<wd-textarea
							v-model="state.formData.mobileIcon"
							custom-style="--fa-textarea-height: 75px"
							clearable
							:maxlength="200"
							placeholder="请输入图标"
						/>
					</wd-form-item>
					<wd-form-item v-if="state.formData.menuType === MenuTypeEnum.Menu" title="路由地址" prop="mobileRouter">
						<wd-textarea
							v-model="state.formData.mobileRouter"
							custom-style="--fa-textarea-height: 75px"
							clearable
							:maxlength="200"
							placeholder="请输入移动端路由地址"
						/>
					</wd-form-item>
				</template>
			</wd-cell-group>
			<wd-cell-group title="桌面端">
				<wd-form-item title="桌面端" prop="hasDesktop">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.hasDesktop" direction="horizontal" />
				</wd-form-item>
				<template v-if="state.formData.hasDesktop">
					<wd-form-item title="图标" prop="desktopIcon">
						<wd-textarea
							v-model="state.formData.desktopIcon"
							custom-style="--fa-textarea-height: 75px"
							clearable
							:maxlength="200"
							placeholder="请输入图标"
						/>
					</wd-form-item>
					<wd-form-item v-if="state.formData.menuType === MenuTypeEnum.Menu" title="路由地址" prop="desktopRouter">
						<wd-textarea
							v-model="state.formData.desktopRouter"
							custom-style="--fa-textarea-height: 75px"
							clearable
							:maxlength="200"
							placeholder="请输入桌面端路由地址"
						/>
					</wd-form-item>
				</template>
			</wd-cell-group>
			<wd-cell-group v-if="state.formData.menuType === MenuTypeEnum.Internal || state.formData.menuType === MenuTypeEnum.Outside" title="其他">
				<wd-form-item title="内链/外链" prop="link">
					<wd-textarea
						v-model="state.formData.link"
						custom-style="--fa-textarea-height: 75px"
						clearable
						:maxlength="200"
						placeholder="请输入内链/外链"
					/>
				</wd-form-item>
			</wd-cell-group>
		</wd-form>

		<wd-divider content-position="left" custom-style="--wot-divider-padding: 0">按钮信息</wd-divider>

		<FaCard v-for="(item, index) in state.formData.buttonList" :key="index" custom-style="margin-left: 20rpx; margin-right: 20rpx;">
			<FaField label="按钮名称" :value="item.buttonName" />
			<FaField label="按钮编码" :value="item.buttonCode" selectable />
			<FaField label="版本" center>
				<FaTag name="EditionEnum" :value="item.edition" />
			</FaField>
			<FaField label="角色">
				<FaTagFlag name="RoleTypeEnum" :value="item.roleType" />
			</FaField>
			<FaField label="Web端" center>
				<FaTag name="BooleanEnum" :value="item.hasWeb" />
			</FaField>
			<FaField label="移动端" center>
				<FaTag name="BooleanEnum" :value="item.hasMobile" />
			</FaField>
			<FaField label="桌面端" center>
				<FaTag name="BooleanEnum" :value="item.hasDesktop" />
			</FaField>
			<FaField label="排序" :value="item.sort" />
			<FaField label="状态" center>
				<FaTag name="CommonStatusEnum" :value="item.status" />
			</FaField>

			<template #footer>
				<wd-button size="mini" type="primary" variant="plain" @click="handleButtonEdit(item, index)">编辑</wd-button>
				<wd-button size="mini" type="danger" variant="plain" @click="handleButtonDelete(index)">删除</wd-button>
			</template>
		</FaCard>

		<wd-button custom-class="fa-form-submit" type="primary" variant="plain" icon="plus" @click="handleButtonAdd">新增按钮</wd-button>
		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>

		<FaPopup ref="faPopupRef" :title="state.popupTitle" height="70%" position="bottom" closable>
			<wd-form ref="buttonFormRef" :model="state.buttonFormData" :schema="state.buttonFormSchema" border>
				<wd-form-item title="按钮名称" prop="buttonName">
					<wd-input v-model="state.buttonFormData.buttonName" clearable :maxlength="50" placeholder="请输入按钮名称" />
				</wd-form-item>
				<wd-form-item title="按钮编码" prop="buttonCode">
					<wd-input v-model="state.buttonFormData.buttonCode" clearable :maxlength="50" placeholder="请输入按钮编码" />
				</wd-form-item>
				<wd-form-item title="版本" prop="edition">
					<FaRadioGroup name="EditionEnum" v-model="state.buttonFormData.edition" type="button" />
				</wd-form-item>
				<wd-form-item title="角色" prop="roleType">
					<FaCheckboxGroup name="RoleTypeEnum" v-model="state.buttonFormData.roleType" type="square" />
				</wd-form-item>
				<wd-form-item title="Web端" prop="hasWeb">
					<FaRadioGroup name="BooleanEnum" v-model="state.buttonFormData.hasWeb" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="移动端" prop="hasMobile">
					<FaRadioGroup name="BooleanEnum" v-model="state.buttonFormData.hasMobile" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="桌面端" prop="hasDesktop">
					<FaRadioGroup name="BooleanEnum" v-model="state.buttonFormData.hasDesktop" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="排序" prop="sort">
					<wd-input v-model="state.buttonFormData.sort" type="number" placeholder="请输入排序" />
				</wd-form-item>
				<wd-form-item title="状态" prop="status">
					<FaRadioGroup name="CommonStatusEnum" v-model="state.buttonFormData.status" direction="horizontal" />
				</wd-form-item>
			</wd-form>

			<wd-button custom-class="fa-form-submit" type="primary" @click="handleButtonConfirm">保存</wd-button>
		</FaPopup>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { EditionEnum } from "@/api/enums/EditionEnum";
import { MenuTypeEnum } from "@/api/enums/MenuTypeEnum";
import { RoleTypeEnum } from "@/api/enums/RoleTypeEnum";
import { menuApi } from "@/api/services/Center/menu";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddMenuInput } from "@/api/services/Center/menu/models/AddMenuInput";
import type { EditMenuButtonInput } from "@/api/services/Center/menu/models/EditMenuButtonInput";
import type { EditMenuInput } from "@/api/services/Center/menu/models/EditMenuInput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "DevMenuEdit",
	authCode: ["Menu:Add", "Menu:Edit"],
	style: {
		navigationBarTitleText: "菜单",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();
const faPopupRef = ref<FaPopupInstance>();
const buttonFormRef = ref<FormInstance>();

type IFormData = EditMenuInput & AddMenuInput & { appName?: string; parentName?: string };

const state = reactive({
	loading: false,
	formData: withDefineType<IFormData>({ buttonList: [] }),
	formSchema: useFormSchema<IFormData>({
		appId: { required: true, message: "请选择应用" },
		menuType: { required: true, message: "请选择菜单类型" },
		menuCode: { required: true, message: "请输入菜单编码" },
		menuName: { required: true, message: "请输入菜单名称" },
		menuTitle: { required: true, message: "请输入菜单标题" },
		sort: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 99999 ? undefined : "排序必须是 1 到 99999 之间的整数"),
		},
		webRouter: {
			validator: (value, model) => (model.hasWeb && model.menuType === MenuTypeEnum.Menu && !value?.trim() ? "请输入Web端路由地址" : undefined),
		},
		webComponent: {
			validator: (value, model) => (model.hasWeb && model.menuType === MenuTypeEnum.Menu && !value?.trim() ? "请输入Web端组件地址" : undefined),
		},
		link: {
			validator: (value, model) =>
				[MenuTypeEnum.Internal, MenuTypeEnum.Outside].includes(model.menuType) && !value?.trim() ? "请输入内链/外链地址" : undefined,
		},
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	menuId: withDefineType<string>(),
	popupTitle: "新增按钮",
	buttonIndex: -1,
	buttonFormData: withDefineType<EditMenuButtonInput>({}),
	buttonFormSchema: useFormSchema<EditMenuButtonInput>({
		buttonName: { required: true, message: "请输入按钮名称" },
		buttonCode: { required: true, message: "请输入按钮编码" },
		sort: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 99999 ? undefined : "排序必须是 1 到 99999 之间的整数"),
		},
	}),
});

const handleButtonAdd = throttle(async () => {
	state.popupTitle = "新增按钮";
	state.buttonIndex = -1;
	state.buttonFormData = {
		edition: EditionEnum.None,
		hasWeb: state.formData.hasWeb,
		hasMobile: state.formData.hasMobile,
		hasDesktop: state.formData.hasDesktop,
		sort: state.formData.buttonList.length + 1,
		status: CommonStatusEnum.Enable,
	};
	await faPopupRef.value.open();
});

const handleButtonEdit = throttle(async (item: EditMenuButtonInput, index: number) => {
	state.popupTitle = "编辑按钮";
	state.buttonIndex = index;
	state.buttonFormData = { ...item };
	await faPopupRef.value.open();
});

const handleButtonDelete = throttle(async (index: number) => {
	await useMessageBox.confirm("确定要删除按钮？");
	state.formData.buttonList.splice(index, 1);
});

const handleButtonConfirm = throttle(async () => {
	const { valid } = await buttonFormRef.value.validate();
	if (!valid) return;
	await faPopupRef.value.close(() => {
		if (state.buttonIndex !== -1) {
			state.formData.buttonList[state.buttonIndex] = { ...state.buttonFormData };
		} else {
			state.formData.buttonList.push({ ...state.buttonFormData });
		}
	});
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	const buttonCodes = state.formData.buttonList.map((item) => item.buttonCode);
	if (new Set(buttonCodes).size !== buttonCodes.length) {
		useToast.warning("按钮编码不能重复");
		return;
	}
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await menuApi.addMenu(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await menuApi.editMenu(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Menu");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/menu/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				state.formData = {
					menuType: MenuTypeEnum.Catalog,
					roleType: RoleTypeEnum.Default,
					edition: EditionEnum.None,
					visible: true,
					hasWeb: true,
					webTab: false,
					webKeepAlive: false,
					hasMobile: false,
					hasDesktop: false,
					sort: 1,
					status: CommonStatusEnum.Enable,
					buttonList: [],
				};
				uni.setNavigationBarTitle({ title: "添加菜单" });
				break;
			case "edit":
				{
					const apiRes = await menuApi.queryMenuDetail(state.menuId);
					if (apiRes.parentId === "0") apiRes.parentId = undefined;
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑菜单 - ${apiRes.menuName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; menuId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.menuId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.menuId = options.menuId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

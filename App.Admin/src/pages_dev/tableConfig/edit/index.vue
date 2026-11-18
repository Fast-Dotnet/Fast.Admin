<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="表格名称" prop="tableName">
				<wd-input v-model="state.formData.tableName" clearable :maxlength="50" placeholder="请输入表格名称" />
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					v-model="state.formData.remark"
					custom-style="--fa-textarea-height: 75px"
					clearable
					:maxlength="200"
					placeholder="请输入备注"
				/>
			</wd-form-item>
		</wd-form>
		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { tableApi } from "@/api/services/Center/table";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddTableConfigInput } from "@/api/services/Center/table/models/AddTableConfigInput";
import type { EditTableConfigInput } from "@/api/services/Center/table/models/EditTableConfigInput";

definePage({
	name: "DevTableConfigEdit",
	authCode: ["Table:Add", "Table:Edit"],
	style: {
		navigationBarTitleText: "表格配置",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<EditTableConfigInput & AddTableConfigInput>({}),
	formSchema: useFormSchema<EditTableConfigInput & AddTableConfigInput>({
		tableName: { required: true, message: "请输入表格名称" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	tableId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await tableApi.addTableConfig(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await tableApi.editTableConfig(state.formData);
				useToast.success("保存成功！");
				break;
			case "copy":
				await tableApi.copyTableConfig({ ...state.formData, tableId: state.tableId });
				useToast.success("复制成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("TableConfig");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/tableConfig/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加表格配置" });
				break;
			case "edit":
				{
					const apiRes = await tableApi.queryTableConfigDetail(state.tableId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑表格配置 - ${apiRes.tableName}` });
				}
				break;
			case "copy":
				uni.setNavigationBarTitle({ title: "复制表格配置" });
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; tableId?: string }) => {
	if (!options?.mode || (options.mode !== "add" && !options?.tableId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.tableId = options.tableId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

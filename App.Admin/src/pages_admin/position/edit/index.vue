<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="职位名称" prop="positionName">
				<wd-input v-model="state.formData.positionName" clearable :maxlength="20" placeholder="请输入职位名称" />
			</wd-form-item>
			<wd-form-item title="排序" prop="sort">
				<wd-input v-model="state.formData.sort" type="number" placeholder="请输入排序" />
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.remark"
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
import { positionApi } from "@/api/services/Admin/position";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddPositionInput } from "@/api/services/Admin/position/models/AddPositionInput";
import type { EditPositionInput } from "@/api/services/Admin/position/models/EditPositionInput";

definePage({
	name: "AdminPositionEdit",
	authCode: ["Position:Add", "Position:Edit"],
	style: {
		navigationBarTitleText: "职位",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<EditPositionInput & AddPositionInput>({ sort: 1 }),
	formSchema: useFormSchema<EditPositionInput & AddPositionInput>({
		positionName: { required: true, message: "请输入职位名称" },
		sort: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 9999 ? undefined : "排序必须是 1 到 9999 之间的整数"),
		},
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	positionId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await positionApi.addPosition(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await positionApi.editPosition(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/position/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加职位" });
				break;
			case "edit":
				{
					const apiRes = await positionApi.queryPositionDetail(state.positionId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑职位 - ${apiRes.positionName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; positionId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.positionId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.positionId = options.positionId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="职位名称" prop="positionName">
				<wd-input v-model="state.formData.positionName" clearable :maxlength="20" placeholder="请输入职位名称" />
			</wd-form-item>
			<wd-form-item title="排序" prop="sort">
				<wd-input-number v-model="state.formData.sort" :min="1" :max="9999" />
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
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
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
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
	formData: withDefineType<EditPositionInput & AddPositionInput>({ sort: 1 }),
	formSchema: useFormSchema<EditPositionInput & AddPositionInput>({
		positionName: { required: true, message: "请输入职位名称" },
		sort: { required: true, message: "请输入排序" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	positionId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
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
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/position/page/index");
});

const handleLoad = async () => {
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

<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.formDisabled" border>
			<wd-form-item title="职位名称" prop="positionName">
				<wd-input v-model="state.formData.positionName" clearable :maxlength="20" placeholder="请输入职位名称" />
			</wd-form-item>
			<wd-form-item title="排序" prop="sort">
				<wd-input-number v-model="state.formData.sort" :min="1" :max="9999" />
			</wd-form-item>
			<wd-form-item title="备注" prop="remark">
				<wd-textarea v-model="state.formData.remark" clearable :maxlength="200" placeholder="请输入备注" />
			</wd-form-item>

			<template v-if="state.pageMode === 'detail'">
				<wd-form-item title="创建人" prop="createdUserName">
					{{ state.formData.createdUserName }}
				</wd-form-item>
				<wd-form-item title="创建时间" prop="createdTime">
					{{ state.formData.createdTime }}
				</wd-form-item>
				<wd-form-item title="更新人" prop="updatedUserName">
					{{ state.formData.updatedUserName }}
				</wd-form-item>
				<wd-form-item title="更新时间" prop="updatedTime">
					{{ state.formData.updatedTime }}
				</wd-form-item>
			</template>
		</wd-form>

		<wd-button custom-class="btn__submit" type="primary" :disabled="state.formDisabled" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { positionApi } from "@/api/services/Admin/position";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddPositionInput } from "@/api/services/Admin/position/models/AddPositionInput";
import type { QueryPositionDetailOutput } from "@/api/services/Admin/position/models/QueryPositionDetailOutput";

definePage({
	name: "AdminPositionEdit",
	layout: "layout",
	style: {
		navigationBarTitleText: "职位管理",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<QueryPositionDetailOutput>({ sort: 1 }),
	formSchema: useFormSchema<AddPositionInput>({
		positionName: { required: true, message: "请输入职位名称" },
		sort: { required: true, message: "请输入排序" },
	}),
	formDisabled: false,
	pageMode: withDefineType<IPageStateType>("detail"),
	positionId: withDefineType<string>(),
});

const handleConfirm = async () => {
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
};

const handleLoad = async () => {
	switch (state.pageMode) {
		case "detail":
			{
				state.formDisabled = true;
				const apiRes = await positionApi.queryPositionDetail(state.positionId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `职位详情 - ${apiRes.positionName}` });
			}
			break;
		case "add":
			state.formDisabled = false;
			uni.setNavigationBarTitle({ title: "添加职位" });
			break;
		case "edit":
			{
				state.formDisabled = false;
				const apiRes = await positionApi.queryPositionDetail(state.positionId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑职位 - ${apiRes.positionName}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; positionId?: string }) => {
	if (!options?.mode || ((options.mode === "edit" || options.mode === "detail") && !options?.positionId)) {
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

<style lang="scss" scoped>
:deep(.wd-textarea__inner) {
	height: 150rpx;
}

.btn__submit {
	display: block;
	margin: 30rpx 50rpx;
}
</style>

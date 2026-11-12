<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="职级名称" prop="jobLevelName">
				<wd-input v-model="state.formData.jobLevelName" clearable :maxlength="20" placeholder="请输入职级名称" />
			</wd-form-item>
			<wd-form-item title="职级等级" prop="level">
				<wd-input v-model="state.formData.level" clearable :maxlength="5" placeholder="请输入职级等级" />
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
import { jobLevelApi } from "@/api/services/Admin/jobLevel";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddJobLevelInput } from "@/api/services/Admin/jobLevel/models/AddJobLevelInput";
import type { EditJobLevelInput } from "@/api/services/Admin/jobLevel/models/EditJobLevelInput";

definePage({
	name: "AdminJobLevelEdit",
	authCode: ["JobLevel:Add", "JobLevel:Edit"],
	style: {
		navigationBarTitleText: "职级",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<EditJobLevelInput & AddJobLevelInput>({}),
	formSchema: useFormSchema<EditJobLevelInput & AddJobLevelInput>({
		jobLevelName: { required: true, message: "请输入职级名称" },
		level: { required: true, message: "请输入职级等级" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	jobLevelId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	try {
		switch (state.pageMode) {
			case "add":
				await jobLevelApi.addJobLevel(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await jobLevelApi.editJobLevel(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_admin/jobLevel/page/index");
});

const handleLoad = async () => {
	switch (state.pageMode) {
		case "add":
			uni.setNavigationBarTitle({ title: "添加职级" });
			break;
		case "edit":
			{
				const apiRes = await jobLevelApi.queryJobLevelDetail(state.jobLevelId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑职级 - ${apiRes.jobLevelName}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; jobLevelId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.jobLevelId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.jobLevelId = options.jobLevelId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

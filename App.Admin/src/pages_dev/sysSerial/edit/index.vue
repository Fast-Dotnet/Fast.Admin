<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item v-if="state.pageMode === 'add'" title="规则类型" prop="ruleType">
				<wd-radio-group v-model="state.formData.ruleType" type="button" direction="horizontal">
					<wd-radio v-for="item in sysSerialRuleTypeEnum" :key="String(item.value)" :value="item.value">{{ item.label }}</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="前缀" prop="prefix">
				<wd-input v-model="state.formData.prefix" clearable :maxlength="5" placeholder="请输入前缀" />
			</wd-form-item>
			<wd-form-item title="时间类型" prop="dateType">
				<wd-radio-group v-model="state.formData.dateType" type="button" direction="horizontal">
					<wd-radio v-for="item in serialDateTypeEnum" :key="String(item.value)" :value="item.value">{{ item.label }}</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="分隔符" prop="spacer">
				<wd-radio-group v-model="state.formData.spacer" type="button" direction="horizontal">
					<wd-radio v-for="item in serialSpacerEnum" :key="String(item.value)" :value="item.value">{{ item.label }}</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="长度" prop="length">
				<wd-input-number v-model="state.formData.length" :min="1" :max="6" />
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
import { SerialDateTypeEnum } from "@/api/enums/SerialDateTypeEnum";
import { SerialSpacerEnum } from "@/api/enums/SerialSpacerEnum";
import { SysSerialRuleTypeEnum } from "@/api/enums/SysSerialRuleTypeEnum";
import { sysSerialApi } from "@/api/services/Center/sysSerial";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import { useApp } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddSysSerialRuleInput } from "@/api/services/Center/sysSerial/models/AddSysSerialRuleInput";
import type { EditSysSerialRuleInput } from "@/api/services/Center/sysSerial/models/EditSysSerialRuleInput";

definePage({
	name: "DevSysSerialEdit",
	authCode: ["SysSerial:Add", "SysSerial:Edit"],
	style: {
		navigationBarTitleText: "序号",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const appStore = useApp();

const sysSerialRuleTypeEnum = appStore.getDictionary("SysSerialRuleTypeEnum");
const serialDateTypeEnum = appStore.getDictionary("SerialDateTypeEnum");
const serialSpacerEnum = appStore.getDictionary("SerialSpacerEnum");

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<EditSysSerialRuleInput & AddSysSerialRuleInput>({
		ruleType: SysSerialRuleTypeEnum.AppNo,
		dateType: SerialDateTypeEnum.Year,
		spacer: SerialSpacerEnum.None,
		length: 4,
	}),
	formSchema: useFormSchema<EditSysSerialRuleInput & AddSysSerialRuleInput>({
		ruleType: { required: true, message: "请选择规则类型" },
		dateType: { required: true, message: "请选择时间类型" },
		spacer: { required: true, message: "请选择分隔符" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	serialRuleId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
	try {
		switch (state.pageMode) {
			case "add":
				await sysSerialApi.addSysSerialRule(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await sysSerialApi.editSysSerialRule(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/sysSerial/page/index");
});

const handleLoad = async () => {
	switch (state.pageMode) {
		case "add":
			uni.setNavigationBarTitle({ title: "添加序号" });
			break;
		case "edit":
			{
				const apiRes = await sysSerialApi.querySysSerialRuleDetail(state.serialRuleId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑序号` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; serialRuleId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.serialRuleId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.serialRuleId = options.serialRuleId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="商户类型" prop="merchantType">
				<FaRadioGroup name="PaymentChannelEnum" v-model="state.formData.merchantType" type="button" />
			</wd-form-item>
			<wd-form-item title="商户名称" prop="merchantName">
				<wd-input v-model="state.formData.merchantName" clearable :maxlength="30" placeholder="请输入商户名称" />
			</wd-form-item>
			<wd-form-item title="商户号" prop="merchantNo">
				<wd-input v-model="state.formData.merchantNo" clearable :maxlength="32" placeholder="请输入商户号" />
			</wd-form-item>
			<wd-form-item title="商户密钥" prop="merchantSecret">
				<wd-textarea
					custom-style="--fa-textarea-height: 75px"
					v-model="state.formData.merchantSecret"
					clearable
					:maxlength="200"
					placeholder="请输入商户密钥"
				/>
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

			<wd-cell-group title="公钥信息">
				<wd-form-item title="公钥序号" prop="publicSerialNum">
					<wd-textarea
						custom-style="--fa-textarea-height: 75px"
						v-model="state.formData.publicSerialNum"
						clearable
						:maxlength="200"
						placeholder="请输入公钥序号"
					/>
				</wd-form-item>
				<wd-form-item title="公钥" prop="publicKey">
					<wd-textarea
						custom-style="--fa-textarea-height: 150px"
						v-model="state.formData.publicKey"
						clearable
						:maxlength="4096"
						placeholder="请输入公钥"
					/>
				</wd-form-item>
			</wd-cell-group>

			<wd-cell-group title="证书信息">
				<wd-form-item title="序号" prop="certSerialNum">
					<wd-textarea
						custom-style="--fa-textarea-height: 75px"
						v-model="state.formData.certSerialNum"
						clearable
						:maxlength="200"
						placeholder="请输入证书序号"
					/>
				</wd-form-item>
				<wd-form-item title="证书" prop="cert">
					<wd-textarea
						custom-style="--fa-textarea-height: 150px"
						v-model="state.formData.cert"
						clearable
						:maxlength="4096"
						placeholder="请输入证书"
					/>
				</wd-form-item>
				<wd-form-item title="证书私钥" prop="certPrivateKey">
					<wd-textarea
						custom-style="--fa-textarea-height: 150px"
						v-model="state.formData.certPrivateKey"
						clearable
						:maxlength="4096"
						placeholder="请输入证书私钥"
					/>
				</wd-form-item>
			</wd-cell-group>
		</wd-form>

		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { merchantApi } from "@/api/services/Center/merchant";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddMerchantInput } from "@/api/services/Center/merchant/models/AddMerchantInput";
import type { EditMerchantInput } from "@/api/services/Center/merchant/models/EditMerchantInput";

definePage({
	name: "SystemMerchantEdit",
	authCode: ["Merchant:Add", "Merchant:Edit"],
	style: {
		navigationBarTitleText: "商户号",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();

const state = reactive({
	loading: false,
	formData: withDefineType<AddMerchantInput & EditMerchantInput>({ merchantType: PaymentChannelEnum.WeChat }),
	formSchema: useFormSchema<AddMerchantInput & EditMerchantInput>({
		merchantName: { required: true, message: "请输入商户名称" },
		merchantNo: { required: true, message: "请输入商户号" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	merchantId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await merchantApi.addMerchant(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await merchantApi.editMerchant(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Merchant");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/merchant/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				uni.setNavigationBarTitle({ title: "添加商户号" });
				break;
			case "edit":
				{
					const apiRes = await merchantApi.queryMerchantDetail(state.merchantId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑商户号 - ${apiRes.merchantNo}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; merchantId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.merchantId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.merchantId = options.merchantId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

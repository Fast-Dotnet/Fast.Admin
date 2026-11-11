<template>
	<view class="page">
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" :disabled="state.formDisabled" border>
			<wd-form-item title="商户类型" prop="merchantType">
				<wd-radio-group v-model="state.formData.merchantType" type="button" direction="horizontal">
					<wd-radio v-for="item in paymentChannelEnum" :key="String(item.value)" :value="item.value">{{ item.label }}</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="商户名称" prop="merchantName">
				<wd-input v-model="state.formData.merchantName" clearable :maxlength="30" placeholder="请输入商户名称" />
			</wd-form-item>
			<wd-form-item title="商户号" prop="merchantNo">
				<wd-input v-model="state.formData.merchantNo" clearable :maxlength="32" placeholder="请输入商户号" />
			</wd-form-item>
			<wd-form-item title="商户密钥" prop="merchantSecret">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.merchantSecret"
					clearable
					:maxlength="200"
					placeholder="请输入商户密钥"
				/>
			</wd-form-item>
			<wd-form-item title="公钥序号" prop="publicSerialNum">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.publicSerialNum"
					clearable
					:maxlength="200"
					placeholder="请输入公钥序号"
				/>
			</wd-form-item>
			<wd-form-item title="公钥" prop="publicKey">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 300rpx"
					v-model="state.formData.publicKey"
					clearable
					:maxlength="4096"
					placeholder="请输入公钥"
				/>
			</wd-form-item>
			<wd-form-item title="证书序号" prop="certSerialNum">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 150rpx"
					v-model="state.formData.certSerialNum"
					clearable
					:maxlength="200"
					placeholder="请输入证书序号"
				/>
			</wd-form-item>
			<wd-form-item title="证书" prop="cert">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 300rpx"
					v-model="state.formData.cert"
					clearable
					:maxlength="4096"
					placeholder="请输入证书"
				/>
			</wd-form-item>
			<wd-form-item title="证书私钥" prop="certPrivateKey">
				<wd-textarea
					custom-style="--wot-textarea-inner-min-height: 300rpx"
					v-model="state.formData.certPrivateKey"
					clearable
					:maxlength="4096"
					placeholder="请输入证书私钥"
				/>
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

		<wd-button custom-class="fa-form-submit" type="primary" :disabled="state.formDisabled" @click="handleConfirm">保存</wd-button>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { PaymentChannelEnum } from "@/api/enums/PaymentChannelEnum";
import { merchantApi } from "@/api/services/Center/merchant";
import { useFormSchema, useLoading, useMessageBox, useToast } from "@/hooks";
import { useApp } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddMerchantInput } from "@/api/services/Center/merchant/models/AddMerchantInput";
import type { QueryMerchantDetailOutput } from "@/api/services/Center/merchant/models/QueryMerchantDetailOutput";

definePage({
	name: "SystemMerchantEdit",
	layout: "layout",
	authCode: ["Merchant:Detail", "Merchant:Add", "Merchant:Edit"],
	style: {
		navigationBarTitleText: "商户号",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();

const appStore = useApp();
const paymentChannelEnum = appStore.getDictionary("PaymentChannelEnum");

const formRef = ref<FormInstance>();

const state = reactive({
	formData: withDefineType<QueryMerchantDetailOutput>({ merchantType: PaymentChannelEnum.WeChat }),
	formSchema: useFormSchema<AddMerchantInput>({
		merchantName: { required: true, message: "请输入商户名称" },
		merchantNo: { required: true, message: "请输入商户号" },
	}),
	formDisabled: false,
	pageMode: withDefineType<IPageStateType>("detail"),
	merchantId: withDefineType<string>(),
});

const handleConfirm = async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	useLoading.show();
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
		useLoading.hide();
	}
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_system/merchant/page/index");
};

const handleLoad = async () => {
	switch (state.pageMode) {
		case "detail":
			{
				state.formDisabled = true;
				const apiRes = await merchantApi.queryMerchantDetail(state.merchantId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `商户号详情 - ${apiRes.merchantNo}` });
			}
			break;
		case "add":
			state.formDisabled = false;
			uni.setNavigationBarTitle({ title: "添加商户号" });
			break;
		case "edit":
			{
				state.formDisabled = false;
				const apiRes = await merchantApi.queryMerchantDetail(state.merchantId);
				state.formData = apiRes;
				uni.setNavigationBarTitle({ title: `编辑商户号 - ${apiRes.merchantNo}` });
			}
			break;
	}
};

onLoad((options: { mode?: IPageStateType; merchantId?: string }) => {
	if (!options?.mode || ((options.mode === "edit" || options.mode === "detail") && !options?.merchantId)) {
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

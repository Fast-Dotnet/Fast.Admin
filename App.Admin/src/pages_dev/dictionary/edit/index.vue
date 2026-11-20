<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="字典Key" prop="dictionaryKey">
				<wd-input
					v-if="state.pageMode === 'add'"
					v-model="state.formData.dictionaryKey"
					clearable
					:maxlength="50"
					placeholder="请输入字典Key"
				/>
				<text v-else>{{ state.formData.dictionaryKey }}</text>
			</wd-form-item>
			<wd-form-item title="字典名称" prop="dictionaryName">
				<wd-input v-model="state.formData.dictionaryName" clearable :maxlength="50" placeholder="请输入字典名称" />
			</wd-form-item>
			<wd-form-item title="值类型" prop="valueType">
				<wd-radio-group v-model="state.formData.valueType">
					<wd-radio :value="DictionaryValueTypeEnum.String">字符串</wd-radio>
					<wd-radio :value="DictionaryValueTypeEnum.Int">数字（Int）</wd-radio>
					<wd-radio :value="DictionaryValueTypeEnum.Long">数字（Long）</wd-radio>
					<wd-radio :value="DictionaryValueTypeEnum.Boolean">Boolean</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item v-if="state.pageMode === 'edit'" title="状态" prop="status">
				<FaRadioGroup name="CommonStatusEnum" v-model="state.formData.status" direction="horizontal" />
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

		<template v-if="state.pageMode === 'edit'">
			<wd-divider content-position="left" custom-style="--wot-divider-padding: 0">字典项</wd-divider>

			<FaCard v-for="(item, index) in state.formData.dictionaryItemList" :key="index" custom-style="margin-left: 20rpx; margin-right: 20rpx;">
				<FaField label="项名称" :value="item.label" />
				<FaField label="项值" :value="item.value" selectable />
				<FaField label="标签类型" center>
					<wd-tag v-if="item.type === TagTypeEnum.Primary" type="primary">Primary</wd-tag>
					<wd-tag v-if="item.type === TagTypeEnum.Success" type="success">Success</wd-tag>
					<wd-tag v-if="item.type === TagTypeEnum.Info" type="default">Info</wd-tag>
					<wd-tag v-if="item.type === TagTypeEnum.Warning" type="warning">Warning</wd-tag>
					<wd-tag v-if="item.type === TagTypeEnum.Danger" type="danger">Danger</wd-tag>
				</FaField>
				<FaField label="排序" :value="item.order" />
				<FaField label="显示" center>
					<FaTag name="BooleanEnum" :value="item.visible" />
				</FaField>
				<FaField label="状态" center>
					<FaTag name="CommonStatusEnum" :value="item.status" />
				</FaField>
				<FaField label="提示" :value="item.tips" />

				<template #footer>
					<wd-button size="mini" type="primary" variant="plain" @click="handleItemEdit(item, index)">编辑</wd-button>
					<wd-button size="mini" type="danger" variant="plain" @click="handleItemDelete(index)">删除</wd-button>
				</template>
			</FaCard>
		</template>

		<wd-button v-if="state.pageMode === 'edit'" custom-class="fa-form-submit" type="primary" variant="plain" icon="plus" @click="handleItemAdd">
			新增字典项
		</wd-button>
		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>

		<FaPopup ref="faPopupRef" :title="state.popupTitle" height="70%" position="bottom" closable>
			<wd-form ref="itemFormRef" :model="state.itemFormData" :schema="state.itemFormSchema" border>
				<wd-form-item title="名称" prop="label">
					<wd-input v-model="state.itemFormData.label" clearable :maxlength="50" placeholder="请输入名称" />
				</wd-form-item>
				<wd-form-item title="值" prop="value">
					<wd-input v-model="state.itemFormData.value" clearable :maxlength="50" placeholder="请输入值" />
				</wd-form-item>
				<wd-form-item title="标签类型" prop="type">
					<wd-radio-group v-model="state.itemFormData.type">
						<wd-radio :value="TagTypeEnum.Primary">Primary</wd-radio>
						<wd-radio :value="TagTypeEnum.Success">Success</wd-radio>
						<wd-radio :value="TagTypeEnum.Info">Info</wd-radio>
						<wd-radio :value="TagTypeEnum.Warning">Warning</wd-radio>
						<wd-radio :value="TagTypeEnum.Danger">Danger</wd-radio>
					</wd-radio-group>
				</wd-form-item>
				<wd-form-item title="排序" prop="order">
					<wd-input v-model.number="state.itemFormData.order" type="number" placeholder="请输入排序" />
				</wd-form-item>
				<wd-form-item title="显示" prop="visible">
					<FaRadioGroup name="BooleanEnum" v-model="state.itemFormData.visible" direction="horizontal" />
				</wd-form-item>
				<wd-form-item v-if="state.pageMode === 'edit'" title="状态" prop="status">
					<FaRadioGroup name="CommonStatusEnum" v-model="state.itemFormData.status" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="提示" prop="tips">
					<wd-textarea
						v-model="state.itemFormData.tips"
						custom-style="--fa-textarea-height: 75px"
						clearable
						:maxlength="100"
						placeholder="请输入提示"
					/>
				</wd-form-item>
			</wd-form>

			<wd-button custom-class="fa-form-submit" type="primary" @click="handleItemConfirm">保存</wd-button>
		</FaPopup>
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { CommonStatusEnum } from "@/api/enums/CommonStatusEnum";
import { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";
import { TagTypeEnum } from "@/api/enums/TagTypeEnum";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { AddDictionaryInput } from "@/api/services/Center/dictionary/models/AddDictionaryInput";
import type { EditDictionaryInput } from "@/api/services/Center/dictionary/models/EditDictionaryInput";
import type { EditDictionaryItemInput } from "@/api/services/Center/dictionary/models/EditDictionaryItemInput";
import type { FaPopupInstance } from "@/components";

definePage({
	name: "DevDictionaryEdit",
	authCode: ["Dictionary:Add", "Dictionary:Edit"],
	style: {
		navigationBarTitleText: "字典",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const formRef = ref<FormInstance>();
const faPopupRef = ref<FaPopupInstance>();
const itemFormRef = ref<FormInstance>();

type IFormData = EditDictionaryInput & Omit<AddDictionaryInput, "dictionaryItemList">;

const state = reactive({
	loading: false,
	formData: withDefineType<IFormData>({ dictionaryItemList: [] }),
	formSchema: useFormSchema<IFormData>({
		dictionaryKey: { required: true, message: "请输入字典Key" },
		dictionaryName: { required: true, message: "请输入字典名称" },
		valueType: { required: true, message: "请选择值类型" },
	}),
	pageMode: withDefineType<IPageStateType>("add"),
	dictionaryId: withDefineType<string>(),
	popupTitle: "新增字典项",
	itemIndex: -1,
	itemFormData: withDefineType<EditDictionaryItemInput>({}),
	itemFormSchema: useFormSchema<EditDictionaryItemInput>({
		label: { required: true, message: "请输入名称" },
		value: { required: true, message: "请输入值" },
		type: { required: true, message: "请选择标签类型" },
		order: {
			required: true,
			message: "请输入排序",
			validator: (value) => (Number.isInteger(value) && value >= 1 && value <= 9999 ? undefined : "排序必须是 1 到 9999 之间的整数"),
		},
		visible: { required: true, message: "请选择是否显示" },
	}),
});

const handleItemAdd = throttle(async () => {
	state.popupTitle = "新增字典项";
	state.itemIndex = -1;
	state.itemFormData = {
		type: TagTypeEnum.Primary,
		order: state.formData.dictionaryItemList.length + 1,
		visible: true,
		status: CommonStatusEnum.Enable,
	};
	await faPopupRef.value.open();
});

const handleItemEdit = throttle(async (item: EditDictionaryItemInput, index: number) => {
	state.popupTitle = "编辑字典项";
	state.itemIndex = index;
	state.itemFormData = { ...item };
	await faPopupRef.value.open();
});

const handleItemDelete = throttle(async (index: number) => {
	await useMessageBox.confirm("确定要删除字典项？");
	state.formData.dictionaryItemList.splice(index, 1);
});

const handleItemConfirm = throttle(async () => {
	const { valid } = await itemFormRef.value.validate();
	if (!valid) return;
	await faPopupRef.value.close(() => {
		if (state.itemIndex !== -1) {
			state.formData.dictionaryItemList[state.itemIndex] = { ...state.itemFormData };
		} else {
			state.formData.dictionaryItemList.push({ ...state.itemFormData });
		}
	});
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				await dictionaryApi.addDictionary(state.formData);
				useToast.success("新增成功！");
				break;
			case "edit":
				await dictionaryApi.editDictionary(state.formData);
				useToast.success("保存成功！");
				break;
		}
	} finally {
		state.loading = false;
	}
	pagingStore.incrementPageVersion("Dictionary");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/dictionary/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		switch (state.pageMode) {
			case "add":
				state.formData = { valueType: DictionaryValueTypeEnum.String, dictionaryItemList: [] };
				uni.setNavigationBarTitle({ title: "添加字典" });
				break;
			case "edit":
				{
					const apiRes = await dictionaryApi.queryDictionaryDetail(state.dictionaryId);
					state.formData = apiRes;
					uni.setNavigationBarTitle({ title: `编辑字典 - ${apiRes.dictionaryName}` });
				}
				break;
		}
	} finally {
		state.loading = false;
	}
};

onLoad((options: { mode?: IPageStateType; dictionaryId?: string }) => {
	if (!options?.mode || (options.mode === "edit" && !options?.dictionaryId)) {
		useMessageBox.alert("参数异常");
		return;
	}
	state.pageMode = options.mode;
	state.dictionaryId = options.dictionaryId;
	handleLoad();
});

onPullDownRefresh(() =>
	useMessageBox
		.confirm("确认刷新页面？")
		.then(handleLoad)
		.finally(() => uni.stopPullDownRefresh())
);
</script>

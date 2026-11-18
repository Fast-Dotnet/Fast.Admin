<template>
	<FaPopup
		ref="faPopupRef"
		:title="state.columnIndex < 0 ? '新增列' : '编辑列'"
		height="85%"
		position="bottom"
		closable
		show-confirm-button
		confirm-button-text="确定"
		@confirm="handleConfirm"
	>
		<template v-if="state.columnIndex >= 0" #titlePrefix>
			<wd-button type="danger" variant="plain" size="small" @click="handleDelete">删除</wd-button>
		</template>

		<wd-form ref="formRef" :model="state.formData" :schema="state.formSchema" border>
			<wd-form-item title="绑定字段" prop="prop">
				<wd-input v-model="state.formData.prop" clearable :maxlength="50" placeholder="请输入绑定字段" />
			</wd-form-item>
			<wd-form-item title="字段名称" prop="label">
				<wd-input v-model="state.formData.label" clearable :maxlength="50" placeholder="请输入字段名称" />
			</wd-form-item>
			<wd-form-item title="顺序" prop="order">
				<wd-input v-model="state.formData.order" type="number" placeholder="请输入顺序" />
			</wd-form-item>
			<wd-form-item title="自动宽度" prop="autoWidth">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.autoWidth" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="宽度" prop="width">
				<wd-input v-model="state.formData.width" type="number" placeholder="请输入宽度" />
			</wd-form-item>
			<wd-form-item title="最小宽度" prop="smallWidth">
				<wd-input v-model="state.formData.smallWidth" type="number" placeholder="请输入最小宽度" />
			</wd-form-item>
			<wd-form-item title="固定" prop="fixed">
				<wd-radio-group v-model="state.formData.fixed" direction="horizontal">
					<wd-radio value="">无</wd-radio>
					<wd-radio value="left">左侧</wd-radio>
					<wd-radio value="right">右侧</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="显示" prop="show">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.show" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="复制" prop="copy">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.copy" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="列类型" prop="type">
				<wd-radio-group v-model="state.formData.type">
					<wd-radio value="">无</wd-radio>
					<wd-radio value="expand">可展开按钮</wd-radio>
					<wd-radio value="image">图片</wd-radio>
					<wd-radio value="date">日期</wd-radio>
					<wd-radio value="time">时间</wd-radio>
					<wd-radio value="dateTime">日期时间</wd-radio>
					<wd-radio value="d2">保留2位</wd-radio>
					<wd-radio value="d4">保留4位</wd-radio>
					<wd-radio value="d6">保留6位</wd-radio>
					<wd-radio value="gd2">保留2位(千分位)</wd-radio>
					<wd-radio value="gd4">保留4位(千分位)</wd-radio>
					<wd-radio value="gd6">保留6位(千分位)</wd-radio>
					<wd-radio value="timeInfo">时间信息</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="排序" prop="sortable">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.sortable" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="排序字段" prop="sortableField">
				<wd-input v-model="state.formData.sortableField" clearable :maxlength="50" placeholder="请输入排序字段" />
			</wd-form-item>
			<wd-form-item title="链接" prop="link">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.link" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="点击事件" prop="clickEmit">
				<wd-input v-model="state.formData.clickEmit" clearable :maxlength="50" placeholder="请输入点击事件" />
			</wd-form-item>
			<wd-form-item title="标签" prop="tag">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.tag" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="字典名称" prop="enum">
				<wd-input v-model="state.formData.enum" clearable :maxlength="50" placeholder="请输入字典名称" />
			</wd-form-item>
			<wd-form-item title="日期格式化" prop="dateFix">
				<FaRadioGroup name="BooleanEnum" v-model="state.formData.dateFix" direction="horizontal" />
			</wd-form-item>
			<wd-form-item title="日期格式" prop="dateFormat">
				<wd-radio-group v-model="state.formData.dateFormat">
					<wd-radio value="">无</wd-radio>
					<wd-radio value="YYYY-MM-DD HH:mm:ss">年-月-日 时:分:秒</wd-radio>
					<wd-radio value="YYYY-MM-DD HH:mm">年-月-日 时:分</wd-radio>
					<wd-radio value="YYYY-MM-DD">年-月-日</wd-radio>
					<wd-radio value="YYYY-MM">年-月</wd-radio>
					<wd-radio value="YYYY">年</wd-radio>
					<wd-radio value="MM">月</wd-radio>
					<wd-radio value="DD">日</wd-radio>
					<wd-radio value="MM-DD">月-日</wd-radio>
					<wd-radio value="HH:mm:ss">时:分:秒</wd-radio>
					<wd-radio value="HH:mm">时:分</wd-radio>
					<wd-radio value="HH">时</wd-radio>
					<wd-radio value="mm:ss">分:秒</wd-radio>
					<wd-radio value="mm">分</wd-radio>
					<wd-radio value="ss">秒</wd-radio>
				</wd-radio-group>
			</wd-form-item>
			<wd-form-item title="插槽" prop="slot">
				<wd-input v-model="state.formData.slot" clearable :maxlength="50" placeholder="请输入插槽" />
			</wd-form-item>
			<wd-form-item title="权限标识" prop="authTag">
				<wd-input v-model="state.formData.authTagStr" clearable :maxlength="50" placeholder="多个权限用英文逗号分隔" />
			</wd-form-item>
			<wd-form-item title="数据删除字段" prop="dataDeleteField">
				<wd-input v-model="state.formData.dataDeleteField" clearable :maxlength="50" placeholder="请输入数据删除字段" />
			</wd-form-item>

			<wd-cell-group title="搜索信息">
				<wd-form-item title="纯搜索" prop="pureSearch">
					<FaRadioGroup name="BooleanEnum" v-model="state.formData.pureSearch" direction="horizontal" />
				</wd-form-item>
				<wd-form-item title="搜索项" prop="searchEl">
					<wd-input v-model="state.formData.searchEl" clearable :maxlength="50" placeholder="请输入搜索项" />
				</wd-form-item>
				<wd-form-item title="搜索项Key" prop="searchKey">
					<wd-input v-model="state.formData.searchKey" clearable :maxlength="50" placeholder="请输入搜索项Key" />
				</wd-form-item>
				<wd-form-item title="搜索项名称" prop="searchLabel">
					<wd-input v-model="state.formData.searchLabel" clearable :maxlength="50" placeholder="请输入搜索项名称" />
				</wd-form-item>
				<wd-form-item title="搜索项排序" prop="searchOrder">
					<wd-input v-model="state.formData.searchOrder" type="number" placeholder="请输入搜索项排序" />
				</wd-form-item>
				<wd-form-item title="搜索项插槽" prop="searchSlot">
					<wd-input v-model="state.formData.searchSlot" clearable :maxlength="50" placeholder="请输入搜索项插槽" />
				</wd-form-item>
			</wd-cell-group>
		</wd-form>

		<template v-for="section in advancedSections" :key="section.key">
			<wd-cell-group :title="section.label">
				<FaCard v-for="(item, index) in state.formData[section.key]" :key="index" :title="'字段 ' + (index + 1)">
					<wd-form :model="item" border>
						<wd-form-item title="字段名称" prop="prop">
							<wd-input v-model="item.prop" clearable :maxlength="50" placeholder="请输入字段名称" />
						</wd-form-item>
						<wd-form-item title="字段类型" prop="type">
							<wd-radio-group v-model="item.type">
								<wd-radio :value="ColumnAdvancedTypeEnum.String">字符串</wd-radio>
								<wd-radio :value="ColumnAdvancedTypeEnum.Number">数字</wd-radio>
								<wd-radio :value="ColumnAdvancedTypeEnum.Boolean">Boolean</wd-radio>
								<wd-radio :value="ColumnAdvancedTypeEnum.Function">Function</wd-radio>
							</wd-radio-group>
						</wd-form-item>
						<wd-form-item title="字段值" prop="value">
							<wd-textarea
								v-model="item.value"
								custom-style="--fa-textarea-height: 75px"
								clearable
								:maxlength="500"
								placeholder="请输入字段值"
							/>
						</wd-form-item>
					</wd-form>
					<template #footer>
						<wd-button size="mini" type="danger" variant="plain" @click="state.formData[section.key].splice(index, 1)">
							删除字段
						</wd-button>
					</template>
				</FaCard>
				<wd-button
					custom-class="fa-form-submit"
					size="small"
					icon="plus"
					variant="plain"
					@click="state.formData[section.key].push({ type: ColumnAdvancedTypeEnum.String })"
				>
					新增字段
				</wd-button>
			</wd-cell-group>
		</template>
	</FaPopup>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { ColumnAdvancedTypeEnum } from "@/api/enums/ColumnAdvancedTypeEnum";
import { useFormSchema, useMessageBox, useToast } from "@/hooks";
import type { FormInstance } from "@wot-ui/ui/components/wd-form/types";
import type { FaTableColumnCtx } from "@/api/services/Center/table/models/FaTableColumnCtx";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevTableColumnConfigEdit",
});

const modelValue = defineModel<FaTableColumnCtx[]>({ required: true });

const faPopupRef = ref<FaPopupInstance>();
const formRef = ref<FormInstance>();

const advancedSections = [
	{ key: "searchConfig", label: "搜索高级配置" },
	{ key: "otherConfig", label: "其他高级配置" },
] satisfies { key: "searchConfig" | "otherConfig"; label: string }[];

const state = reactive({
	formData: withDefineType<FaTableColumnCtx & { authTagStr?: string }>({ searchConfig: [], otherConfig: [] }),
	formSchema: useFormSchema<FaTableColumnCtx>({
		order: {
			required: true,
			message: "请输入顺序",
			validator: (value) => (Number.isInteger(value) && value >= 0 && value <= 999 ? undefined : "顺序必须是 0 到 999 之间的整数"),
		},
		width: {
			required: true,
			message: "请输入宽度",
			validator: (value) => (Number.isInteger(value) && value >= 0 && value <= 999 ? undefined : "宽度必须是 0 到 999 之间的整数"),
		},
		smallWidth: {
			required: true,
			message: "请输入最小宽度",
			validator: (value) => (Number.isInteger(value) && value >= 0 && value <= 999 ? undefined : "最小宽度必须是 0 到 999 之间的整数"),
		},
		searchOrder: {
			required: true,
			message: "请输入搜索项排序",
			validator: (value) => (Number.isInteger(value) && value >= 0 && value <= 999 ? undefined : "搜索项排序必须是 0 到 999 之间的整数"),
		},
	}),
	columnIndex: -1,
});

const handleConfirm = throttle(async () => {
	const { valid } = await formRef.value.validate();
	if (!valid) return;
	if (state.formData.searchConfig?.length > 0) {
		for (let index = 0; index < state.formData.searchConfig.length; index++) {
			const item = state.formData.searchConfig[index];
			if (!item.prop) {
				useToast.error(`搜索高级配置，第【${index + 1}】项，字段名称不能为空！`);
				return;
			}
			if (!item.value) {
				useToast.error(`搜索高级配置，第【${index + 1}】项，字段值不能为空！`);
				return;
			}
		}
	} else {
		state.formData.searchConfig = [];
	}
	if (state.formData.otherConfig?.length > 0) {
		for (let index = 0; index < state.formData.otherConfig.length; index++) {
			const item = state.formData.otherConfig[index];
			if (!item.prop) {
				useToast.error(`其他高级配置，第【${index + 1}】项，字段名称不能为空！`);
				return;
			}
			if (!item.value) {
				useToast.error(`其他高级配置，第【${index + 1}】项，字段值不能为空！`);
				return;
			}
		}
	} else {
		state.formData.otherConfig = [];
	}
	await faPopupRef.value.close(() => {
		const column = {
			...state.formData,
			authTag: state.formData.authTagStr
				.split(",")
				.map((v) => v.trim())
				.filter(Boolean),
		};
		if (state.columnIndex < 0) modelValue.value.push(column);
		else modelValue.value[state.columnIndex] = column;
		modelValue.value.sort((a, b) => a.order - b.order);
		modelValue.value.forEach((item, index) => {
			item.order = index + 1;
		});
	});
});

const handleDelete = throttle(async () => {
	await useMessageBox.confirm("确定要删除列？");
	await faPopupRef.value.close(() => {
		modelValue.value.splice(state.columnIndex, 1);
		modelValue.value.forEach((item, index) => {
			item.order = index + 1;
		});
	});
});

const add = throttle(
	async () =>
		await faPopupRef.value.open(() => {
			state.columnIndex = -1;
			state.formData = {
				prop: "",
				label: "",
				fixed: "",
				autoWidth: false,
				width: 100,
				smallWidth: 100,
				order: modelValue.value.length + 1,
				show: true,
				copy: false,
				sortable: true,
				sortableField: "",
				type: "",
				link: false,
				clickEmit: "",
				tag: false,
				enum: "",
				dateFix: false,
				dateFormat: "",
				authTag: [],
				authTagStr: "",
				dataDeleteField: "",
				slot: "",
				otherConfig: [],
				pureSearch: false,
				searchEl: "",
				searchKey: "",
				searchLabel: "",
				searchOrder: 0,
				searchSlot: "",
				searchConfig: [],
			};
		})
);

const edit = throttle(
	async (index: number) =>
		await faPopupRef.value.open(() => {
			const item = modelValue.value[index];
			state.columnIndex = index;
			state.formData = {
				...item,
				authTagStr: item.authTag?.join(",") ?? "",
				searchConfig: item.searchConfig?.map((config) => ({ ...config })) ?? [],
				otherConfig: item.otherConfig?.map((config) => ({ ...config })) ?? [],
			};
		})
);

defineExpose({
	add,
	edit,
});
</script>

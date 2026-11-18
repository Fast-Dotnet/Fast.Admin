<template>
	<view class="page">
		<FaLoading mask :loading="state.loading" />
		<view style="padding: var(--fa-spacing-page)">
			<wd-button size="small" icon="plus" @click="columnFormRef?.add()">新增列</wd-button>
		</view>
		<wd-cell-group border>
			<wd-cell
				v-for="(item, index) in state.formData.columns"
				:key="item.columnId ?? index"
				:title="item.prop"
				:value="item.label"
				center
				is-link
				@click="columnFormRef?.edit(index)"
			/>
		</wd-cell-group>
		<wd-empty v-if="!state.formData.columns.length" icon="no-content" tip="暂无配置列" />
		<wd-button custom-class="fa-form-submit" type="primary" @click="handleConfirm">保存</wd-button>

		<ColumnConfig ref="columnFormRef" v-model="state.formData.columns" />
	</view>
</template>

<script setup lang="ts">
import { onLoad, onPullDownRefresh } from "@dcloudio/uni-app";
import { reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { tableApi } from "@/api/services/Center/table";
import { useMessageBox, useToast } from "@/hooks";
import { usePaging } from "@/stores";
import ColumnConfig from "./components/column.vue";
import type { EditTableColumnConfigInput } from "@/api/services/Center/table/models/EditTableColumnConfigInput";

definePage({
	name: "DevTableColumnConfig",
	authCode: "Table:Edit",
	style: {
		navigationBarTitleText: "配置列",
		enablePullDownRefresh: true,
	},
});

const router = useRouter();
const pagingStore = usePaging();

const columnFormRef = ref<InstanceType<typeof ColumnConfig>>();

const state = reactive({
	loading: false,
	formData: withDefineType<EditTableColumnConfigInput>({ columns: [] }),
	tableId: withDefineType<string>(),
});

const handleConfirm = throttle(async () => {
	await useMessageBox.confirm("确认要保存表格配置？", {
		async beforeConfirm() {
			state.loading = true;
			try {
				await tableApi.editTableColumnConfig(state.formData);
				useToast.success("保存成功！");
			} finally {
				state.loading = false;
			}
		},
	});
	pagingStore.incrementPageVersion("TableConfig");
	if (getCurrentPages().length > 1) router.back();
	else router.replace("/pages_dev/tableConfig/page/index");
});

const handleLoad = async () => {
	state.loading = true;
	try {
		const [detail, columns] = await Promise.all([
			tableApi.queryTableConfigDetail(state.tableId),
			tableApi.queryTableColumnConfigDetail(state.tableId),
		]);
		state.formData = { tableId: state.tableId, rowVersion: detail.rowVersion, columns };
		uni.setNavigationBarTitle({ title: `配置列 - ${detail.tableName}` });
	} finally {
		state.loading = false;
	}
};

onLoad((options: { tableId?: string }) => {
	if (!options?.tableId) {
		useMessageBox.alert("参数异常");
		return;
	}
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

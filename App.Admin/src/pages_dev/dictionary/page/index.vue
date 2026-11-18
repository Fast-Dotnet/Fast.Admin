<template>
	<FaPaging ref="faPagingRef" page-key="Dictionary" :request-api="dictionaryApi.queryDictionaryPaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput :filter="false" v-model:search-param="searchParam.value" @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #footer>
					<FaAuth code="Dictionary:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard
				v-for="item in pagedList"
				:key="item.dictionaryId"
				:title="item.dictionaryKey"
				code="Dictionary:Detail"
				@click="detailFormRef?.detail(item.dictionaryId)"
			>
				<template #titleExtra>
					<FaTag name="CommonStatusEnum" :value="item.status" />
				</template>
				<FaField label="字典名称" :value="item.dictionaryName" />
				<FaField label="值类型">
					<wd-tag v-if="item.valueType === DictionaryValueTypeEnum.String" type="default" variant="plain">字符串</wd-tag>
					<wd-tag v-if="item.valueType === DictionaryValueTypeEnum.Int" type="success" variant="plain">数字（Int）</wd-tag>
					<wd-tag v-if="item.valueType === DictionaryValueTypeEnum.Long" type="primary" variant="plain">数字（Long）</wd-tag>
					<wd-tag v-if="item.valueType === DictionaryValueTypeEnum.Boolean" type="danger" variant="plain">Boolean</wd-tag>
				</FaField>
				<FaField label="Flags枚举" center>
					<FaTag name="BooleanEnum" :value="item.hasFlags" />
				</FaField>
				<FaField label="备注" :value="item.remark" />

				<template #footer>
					<FaAuth code="Dictionary:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.dictionaryId)">编辑</wd-button>
					</FaAuth>
					<FaAuth code="Dictionary:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<DictionaryDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import { useMessageBox, useToast } from "@/hooks";
import DictionaryDetail from "./components/detail.vue";
import type { QueryDictionaryPagedOutput } from "@/api/services/Center/dictionary/models/QueryDictionaryPagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "DevDictionary",
	authCode: "Dictionary:Paged",
	style: {
		navigationBarTitleText: "数据字典",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof DictionaryDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, dictionaryId?: string) =>
		await router.push({
			path: "/pages_dev/dictionary/edit/index",
			query: {
				mode,
				dictionaryId,
			},
		})
);

const handleDelete = throttle(async (item: QueryDictionaryPagedOutput) => {
	const { dictionaryId, rowVersion } = item;
	await useMessageBox.confirm("确定要删除字典？", {
		async beforeConfirm() {
			await dictionaryApi.deleteDictionary({ dictionaryId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

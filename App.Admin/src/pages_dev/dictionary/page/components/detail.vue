<template>
	<FaPopup ref="faPopupRef" title="字典详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="字典Key" :value="formData.dictionaryKey" />
			<FaField label="字典名称" :value="formData.dictionaryName" />
			<FaField label="值类型">
				<wd-tag v-if="formData.valueType === DictionaryValueTypeEnum.String" type="default" variant="plain">字符串</wd-tag>
				<wd-tag v-if="formData.valueType === DictionaryValueTypeEnum.Int" type="success" variant="plain">数字（Int）</wd-tag>
				<wd-tag v-if="formData.valueType === DictionaryValueTypeEnum.Long" type="primary" variant="plain">数字（Long）</wd-tag>
				<wd-tag v-if="formData.valueType === DictionaryValueTypeEnum.Boolean" type="danger" variant="plain">Boolean</wd-tag>
			</FaField>
			<FaField label="Flags枚举" center>
				<FaTag name="BooleanEnum" :value="formData.hasFlags" />
			</FaField>
			<FaField label="状态" center>
				<FaTag name="CommonStatusEnum" :value="formData.status" />
			</FaField>
			<FaField label="备注" :value="formData.remark" />
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>

		<FaCard v-for="item in formData.dictionaryItemList" :key="item.dictionaryItemId">
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
		</FaCard>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { DictionaryValueTypeEnum } from "@/api/enums/DictionaryValueTypeEnum";
import { TagTypeEnum } from "@/api/enums/TagTypeEnum";
import { dictionaryApi } from "@/api/services/Center/dictionary";
import type { QueryDictionaryDetailOutput } from "@/api/services/Center/dictionary/models/QueryDictionaryDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevDictionaryDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryDictionaryDetailOutput>();

const detail = throttle(
	async (dictionaryId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await dictionaryApi.queryDictionaryDetail(dictionaryId);
		})
);

defineExpose({
	detail,
});
</script>

<template>
	<FaPopup ref="faPopupRef" title="部门详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="机构" :value="formData.orgName" />
			<FaField label="父级" :value="formData.parentName" />
			<FaField label="部门名称" :value="formData.departmentName" />
			<FaField label="部门编码" :value="formData.departmentCode" selectable />
			<FaField label="排序" :value="formData.sort" />
			<FaField label="数据公开" center>
				<FaTag name="BooleanEnum" :value="formData.dataPublic" />
			</FaField>
			<FaField label="联系人" :value="formData.contacts" />
			<FaField label="电话" :value="formData.phone" />
			<FaField label="邮箱" :value="formData.email" />
			<FaField label="备注" :value="formData.remark" />
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { departmentApi } from "@/api/services/Admin/department";
import type { QueryDepartmentDetailOutput } from "@/api/services/Admin/department/models/QueryDepartmentDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminDepartmentDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryDepartmentDetailOutput>({});

const detail = throttle(
	async (departmentId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await departmentApi.queryDepartmentDetail(departmentId);
		})
);

defineExpose({
	detail,
});
</script>

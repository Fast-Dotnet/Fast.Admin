<template>
	<FaPopup ref="popupRef" title="职员详情" height="70%" position="bottom" closable>
		<view style="display: flex; flex-direction: column; gap: 10px">
			<FaField label="证件照">
				<FaImage :src="formData.idPhoto" width="80px" height="80px" />
			</FaField>
			<FaField label="姓名" :value="formData.employeeNo" />
			<FaField label="工号" :value="formData.employeeNo" />
			<FaField label="手机" :value="formData.mobile" />
			<FaField label="邮箱" :value="formData.email" />
			<FaField label="性别">
				<FaTag name="GenderEnum" :value="formData.sex" />
			</FaField>
			<FaField label="状态">
				<FaTag name="EmployeeStatusEnum" :value="formData.status" />
			</FaField>
			<FaField label="入职日期" :value="formData.entryDate?.slice(0, 10)" />
			<FaField label="离职日期" :value="formData.resignDate?.slice(0, 10)" />
			<FaField label="离职原因" :value="formData.resignReason" />
			<FaField label="角色" :value="roleNames" />
			<FaField label="备注" :value="formData.remark" />
			<FaField label="创建人" :value="formData.createdUserName" />
			<FaField label="创建时间" :value="formData.createdTime" />
			<FaField label="更新人" :value="formData.updatedUserName" />
			<FaField label="更新时间" :value="formData.updatedTime" />
		</view>

		<FaCard v-for="item in formData.orgList" :key="item.departmentId" :title="item.departmentName">
			<template #titleExtra>
				<wd-tag v-if="item.isPrincipal" type="primary">负责人</wd-tag>
				<FaTag name="BooleanEnum" :value="item.isPrincipal" />
			</template>
			<FaField label="主部门">
				<FaTag name="BooleanEnum" :value="item.isPrimary" />
			</FaField>
			<FaField label="机构" :value="item.orgName" />
			<FaField label="职位" :value="item.positionName" />
			<FaField label="职级" :value="item.jobLevelName" />
		</FaCard>
	</FaPopup>
</template>

<script setup lang="ts">
import { computed, ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import type { QueryEmployeeDetailOutput } from "@/api/services/Admin/employee/models/QueryEmployeeDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminEmployeeDetail",
});

const popupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryEmployeeDetailOutput>();
const roleNames = computed(() => formData.value.roleList?.map((item) => item.roleName).join("，"));

const detail = throttle((employeeId: string) =>
	popupRef.value.open(async () => {
		formData.value = await employeeApi.queryEmployeeDetail(employeeId);
	})
);

defineExpose({
	detail,
});
</script>

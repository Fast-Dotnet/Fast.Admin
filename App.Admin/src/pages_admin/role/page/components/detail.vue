<template>
	<FaPopup ref="faPopupRef" title="角色详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="角色名称" :value="state.formData.roleName" selectable />
			<FaField label="角色编码" :value="state.formData.roleCode" copyable />
			<FaField label="角色类型" center>
				<FaTag name="RoleTypeEnum" :value="state.formData.roleType" />
			</FaField>
			<FaField label="数据范围" center>
				<FaTag name="DataScopeTypeEnum" :value="state.formData.dataScopeType" />
			</FaField>
			<FaField label="系统菜单" center>
				<FaTag name="BooleanEnum" :value="state.formData.isSystemMenu" />
			</FaField>
			<FaField
				v-if="state.formData.dataScopeType === DataScopeTypeEnum.CustomDept"
				label="自定义部门"
				:value="dataScopeDepartmentNames"
				selectable
			/>
			<FaField label="可分配角色" :value="roleNames" selectable />
			<FaField label="排序" :value="state.formData.sort" selectable />
			<FaField label="备注" :value="state.formData.remark" selectable />
			<FaField label="创建人" :value="state.formData.createdUserName" selectable />
			<FaField label="创建时间" :value="state.formData.createdTime" selectable />
			<FaField label="更新人" :value="state.formData.updatedUserName" selectable />
			<FaField label="更新时间" :value="state.formData.updatedTime" selectable />
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue";
import { throttle, withDefineType } from "@fast-china/utils";
import { DataScopeTypeEnum } from "@/api/enums/DataScopeTypeEnum";
import { roleApi } from "@/api/services/Admin/role";
import type { QueryRoleDetailOutput } from "@/api/services/Admin/role/models/QueryRoleDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "AdminRoleDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const state = reactive({
	formData: withDefineType<QueryRoleDetailOutput>({}),
	roleList: withDefineType<ElSelectorOutput<string>[]>([]),
});

const dataScopeDepartmentNames = computed(() => state.formData.dataScopeDepartmentNames?.join("，"));
const roleNames = computed(() =>
	state.formData.assignableRoleIds?.map((roleId) => state.roleList.find((role) => role.value === roleId)?.label).join("，")
);

const detail = throttle(
	async (roleId: string) =>
		await faPopupRef.value.open(async () => {
			const apiRes = await roleApi.queryRoleDetail(roleId);
			state.formData = apiRes;
			state.roleList = await roleApi.roleSelector();
		})
);

defineExpose({
	detail,
});
</script>

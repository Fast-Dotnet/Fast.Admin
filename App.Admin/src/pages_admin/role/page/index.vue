<template>
	<FaPaging ref="faPagingRef" page-key="Role" :request-api="roleApi.queryRolePaged">
		<template #top="{ searchParam, refresh }">
			<FaSearchInput v-model:search-param="searchParam.value" hide-search-time @confirm="refresh()" @clear="refresh()" @search="refresh()">
				<template #filter>
					<FaCellPicker title="角色类型" name="RoleTypeEnum" v-model="searchParam.value.roleType" radio clearable />
					<FaCellPicker title="数据范围" name="DataScopeTypeEnum" v-model="searchParam.value.dataScopeType" radio clearable />
				</template>
				<template #footer>
					<FaAuth code="Role:Add">
						<wd-button size="small" icon="plus" @click="handleEdit('add')">新增</wd-button>
					</FaAuth>
				</template>
			</FaSearchInput>
		</template>

		<template #default="{ pagedList }">
			<FaCard v-for="item in pagedList" :key="item.roleId" code="Role:Detail" @click="detailFormRef.detail(item.roleId)">
				<FaField label="角色名称" :value="item.roleName" />
				<FaField label="角色编码" :value="item.roleCode" copyable />
				<FaField label="角色类型" center>
					<FaTag name="RoleTypeEnum" :value="item.roleType" />
				</FaField>
				<FaField label="数据范围" center>
					<FaTag name="DataScopeTypeEnum" :value="item.dataScopeType" />
				</FaField>

				<template #footer>
					<FaAuth code="Role:Edit">
						<wd-button size="mini" type="primary" variant="plain" @click="handleEdit('edit', item.roleId)">编辑</wd-button>
					</FaAuth>
					<FaAuth v-if="!item.isSystemMenu" code="Role:Edit">
						<wd-button size="mini" type="success" variant="plain" @click="handleAuth(item.roleId)">授权</wd-button>
					</FaAuth>
					<FaAuth code="Role:Delete">
						<wd-button size="mini" type="danger" variant="plain" @click="handleDelete(item)">删除</wd-button>
					</FaAuth>
				</template>
			</FaCard>
		</template>
	</FaPaging>

	<RoleDetail ref="detailFormRef" />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { throttle } from "@fast-china/utils";
import { useRouter } from "@wot-ui/router";
import { roleApi } from "@/api/services/Admin/role";
import { useMessageBox, useToast } from "@/hooks";
import RoleDetail from "./components/detail.vue";
import type { QueryRolePagedOutput } from "@/api/services/Admin/role/models/QueryRolePagedOutput";
import type { FaPagingInstance } from "@/components";

definePage({
	name: "AdminRole",
	authCode: "Role:Paged",
	style: {
		navigationBarTitleText: "角色管理",
	},
});

const router = useRouter();

const faPagingRef = ref<FaPagingInstance>();
const detailFormRef = ref<InstanceType<typeof RoleDetail>>();

const handleEdit = throttle(
	async (mode: IPageStateType, roleId?: string) =>
		await router.push({
			path: "/pages_admin/role/edit/index",
			query: {
				mode,
				roleId,
			},
		})
);

const handleAuth = throttle((roleId: string) => router.push(`/pages_admin/role/auth/index?roleId=${roleId}`));

const handleDelete = throttle(async (item: QueryRolePagedOutput) => {
	const { roleId, rowVersion } = item;
	await useMessageBox.confirm("确定删除角色？", {
		async beforeConfirm() {
			await roleApi.deleteRole({ roleId, rowVersion });
			useToast.success("删除成功！");
			faPagingRef.value.refresh();
		},
	});
});
</script>

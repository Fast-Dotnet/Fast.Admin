<template>
	<FaPopup ref="faPopupRef" title="数据库配置详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="数据库名称" :value="formData.dbName" />
			<FaField label="租户" :value="formData.tenantName" />
			<FaField label="数据库类型" center>
				<FaTag name="DatabaseTypeEnum" :value="formData.databaseType" />
			</FaField>
			<FaField label="公网IP" :value="formData.publicIp" selectable />
			<FaField label="内网IP" :value="formData.intranetIp" />
			<FaField label="端口号" :value="formData.port" />
			<FaField label="Db类型" center>
				<FaTag name="SugarDbType" :value="formData.dbType" />
			</FaField>
			<FaField label="数据库用户" :value="formData.dbUser" />
			<FaField label="超时时间" :value="`${formData.commandTimeOut ?? 0}秒`" />
			<FaField label="警告时间" :value="`${formData.sugarSqlExecMaxSeconds ?? 0}秒`" />
			<FaField label="差异日志" center>
				<FaTag name="BooleanEnum" :value="formData.diffLog" />
			</FaField>
			<FaField label="禁用AOP" center>
				<FaTag name="BooleanEnum" :value="formData.disableAop" />
			</FaField>
			<FaField label="初始化" center>
				<wd-tag v-if="formData.isInitialized" type="success">已初始化</wd-tag>
				<wd-tag v-else type="danger">未初始化</wd-tag>
			</FaField>
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
import type { QueryDatabasePagedOutput } from "@/api/services/Center/database/models/QueryDatabasePagedOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "SystemDatabaseDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryDatabasePagedOutput>({});

const detail = throttle(
	async (item: QueryDatabasePagedOutput) =>
		await faPopupRef.value.open(() => {
			formData.value = item;
		})
);

defineExpose({
	detail,
});
</script>

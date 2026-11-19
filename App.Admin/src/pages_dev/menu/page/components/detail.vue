<template>
	<FaPopup ref="faPopupRef" title="菜单详情" height="70%" position="bottom" closable>
		<view class="page-detail__popup">
			<FaField label="菜单名称" :value="formData.menuName" selectable />
			<FaField label="菜单编码" :value="formData.menuCode" copyable />
			<FaField label="菜单标题" :value="formData.menuTitle" selectable />
			<FaField label="应用" :value="formData.appName" selectable />
			<FaField label="菜单类型" center>
				<FaTag name="MenuTypeEnum" :value="formData.menuType" />
			</FaField>
			<FaField label="版本" center>
				<FaTag name="EditionEnum" :value="formData.edition" />
			</FaField>
			<FaField label="角色">
				<FaTagFlag name="RoleTypeEnum" :value="formData.roleType" />
			</FaField>
			<FaField label="排序" :value="formData.sort" selectable />
			<FaField label="显示" center>
				<FaTag name="BooleanEnum" :value="formData.visible" />
			</FaField>
			<FaField label="状态" center>
				<FaTag name="CommonStatusEnum" :value="formData.status" />
			</FaField>
			<FaField label="Web端" center>
				<FaTag name="BooleanEnum" :value="formData.hasWeb" />
			</FaField>
			<FaField label="Web端图标" :value="formData.webIcon" selectable />
			<FaField label="Web端路由" :value="formData.webRouter" copyable />
			<FaField label="Web端组件" :value="formData.webComponent" copyable />
			<FaField label="导航栏" center>
				<FaTag name="BooleanEnum" :value="formData.webTab" />
			</FaField>
			<FaField label="KeepAlive" center>
				<FaTag name="BooleanEnum" :value="formData.webKeepAlive" />
			</FaField>
			<FaField label="移动端" center>
				<FaTag name="BooleanEnum" :value="formData.hasMobile" />
			</FaField>
			<FaField label="移动端图标" :value="formData.mobileIcon" selectable />
			<FaField label="移动端路由" :value="formData.mobileRouter" copyable />
			<FaField label="桌面端" center>
				<FaTag name="BooleanEnum" :value="formData.hasDesktop" />
			</FaField>
			<FaField label="桌面端图标" :value="formData.desktopIcon" selectable />
			<FaField label="桌面端路由" :value="formData.desktopRouter" copyable />
			<FaField label="内链/外链" :value="formData.link" copyable />
			<FaField label="创建人" :value="formData.createdUserName" selectable />
			<FaField label="创建时间" :value="formData.createdTime" selectable />
			<FaField label="更新人" :value="formData.updatedUserName" selectable />
			<FaField label="更新时间" :value="formData.updatedTime" selectable />
		</view>

		<FaCard v-for="item in formData.buttonList" :key="item.buttonId">
			<FaField label="按钮名称" :value="item.buttonName" selectable />
			<FaField label="按钮编码" :value="item.buttonCode" copyable />
			<FaField label="版本" center>
				<FaTag name="EditionEnum" :value="item.edition" />
			</FaField>
			<FaField label="角色">
				<FaTagFlag name="RoleTypeEnum" :value="item.roleType" />
			</FaField>
			<FaField label="Web端" center>
				<FaTag name="BooleanEnum" :value="item.hasWeb" />
			</FaField>
			<FaField label="移动端" center>
				<FaTag name="BooleanEnum" :value="item.hasMobile" />
			</FaField>
			<FaField label="桌面端" center>
				<FaTag name="BooleanEnum" :value="item.hasDesktop" />
			</FaField>
			<FaField label="排序" :value="item.sort" selectable />
			<FaField label="状态" center>
				<FaTag name="CommonStatusEnum" :value="item.status" />
			</FaField>
		</FaCard>
	</FaPopup>
</template>

<script setup lang="ts">
import { ref, shallowRef } from "vue";
import { throttle } from "@fast-china/utils";
import { menuApi } from "@/api/services/Center/menu";
import type { QueryMenuDetailOutput } from "@/api/services/Center/menu/models/QueryMenuDetailOutput";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "DevMenuDetail",
});

const faPopupRef = ref<FaPopupInstance>();

const formData = shallowRef<QueryMenuDetailOutput>({});

const detail = throttle(
	async (menuId: string) =>
		await faPopupRef.value.open(async () => {
			formData.value = await menuApi.queryMenuDetail(menuId);
		})
);

defineExpose({
	detail,
});
</script>

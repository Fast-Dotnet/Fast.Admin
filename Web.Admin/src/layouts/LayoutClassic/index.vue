<template>
	<el-container
		:class="[
			{
				contentFull: navTabsStore.contentFull,
				contentLarge: navTabsStore.contentLarge,
			},
		]"
	>
		<!-- 桌面端侧边栏 -->
		<el-aside
			v-if="!useDrawerMenu"
			:style="{ '--el-aside-width': addCssUnit(configStore.layout.menuCollapse ? 'auto' : configStore.layout.menuWidth) }"
		>
			<LayoutLogo />
			<LayoutMenu />
		</el-aside>
		<!-- 移动端抽屉菜单 -->
		<el-drawer
			v-else
			body-class="mobile-menu__body"
			v-model="mobileMenuVisible"
			direction="ltr"
			:show-close="false"
			:with-header="false"
			:size="configStore.layout.menuWidth"
		>
			<LayoutLogo />
			<LayoutMenu />
		</el-drawer>
		<el-container>
			<el-header>
				<div class="nav-bar" :style="{ '--height': addCssUnit(configStore.layout.navBarHeight) }">
					<div class="left">
						<el-icon
							v-if="!useDrawerMenu"
							class="menu-collapse fa__hover__twinkle"
							:title="configStore.layout.menuCollapse ? '展开' : '折叠'"
							role="button"
							tabindex="0"
							:aria-label="configStore.layout.menuCollapse ? '展开' : '折叠'"
							@keydown.enter.prevent="handleMenuToggle"
							@keydown.space.prevent="handleMenuToggle"
							@click="handleMenuToggle"
						>
							<Expand v-if="configStore.layout.menuCollapse" />
							<Fold v-else />
						</el-icon>
						<el-icon
							v-else
							class="menu-collapse fa__hover__twinkle"
							title="打开菜单"
							role="button"
							tabindex="0"
							aria-label="打开菜单"
							@keydown.enter.prevent="handleMenuToggle"
							@keydown.space.prevent="handleMenuToggle"
							@click="handleMenuToggle"
						>
							<Expand />
						</el-icon>
						<LayoutBreadcrumb v-if="configStore.layout.breadcrumb && !useDrawerMenu" />
					</div>
					<div class="right">
						<FaSelect
							ref="faTenantSelectRef"
							width="180px"
							size="default"
							value-key="userKey"
							:props="{ label: 'tenantName' }"
							:lazy="false"
							more-detail
							:request-api="loginApi.queryLoginUser"
							@change="handleTenantChange"
							@data-change="() => faTenantSelectRef.setSelection(userInfoStore.userKey)"
						>
							<template #default="data">
								<div style="display: flex; justify-content: space-between; align-items: center; gap: 8px; width: 100%">
									<FaAvatar :src="data.idPhoto" thumb size="small" />
									<div style="flex: 1">
										<span>{{ data.tenantName }}</span>
										<span style="display: flex; justify-content: space-between; width: 100%">
											<span style="font-size: var(--el-font-size-extra-small); padding-right: 8px">{{
												data.employeeName
											}}</span>
											<span style="font-size: var(--el-font-size-extra-small)">{{ data.employeeNo }}</span>
										</span>
									</div>
								</div>
							</template>
						</FaSelect>
						<el-icon
							v-if="configStore.layout.menuSearch"
							class="menu-search fa__hover__twinkle"
							title="搜索菜单"
							role="button"
							tabindex="0"
							aria-label="搜索菜单"
							@keydown.enter.prevent="menuSearchRef?.open()"
							@keydown.space.prevent="menuSearchRef?.open()"
							@click="menuSearchRef?.open()"
						>
							<Search />
						</el-icon>
						<LayoutScreenFull v-if="configStore.layout.screenFull && !useDrawerMenu" />
						<el-dropdown
							class="avatar"
							placement="bottom"
							trigger="click"
							hide-on-click
							:title="userInfoStore.employeeName || userInfoStore.nickName"
						>
							<div class="user-info">
								<FaAvatar original :size="24" :src="userInfoStore.avatar" :icon="UserFilled" />
								<span class="nick-name">{{ userInfoStore.nickName }}（{{ userInfoStore.employeeName }}）</span>
							</div>
							<template #dropdown>
								<el-dropdown-menu>
									<el-dropdown-item :icon="Key" @click="changePasswordRef.open()">修改密码</el-dropdown-item>
									<el-dropdown-item :icon="IdCard" @click="accountEditRef.open()">账号信息</el-dropdown-item>
									<el-dropdown-item
										v-if="!userInfoStore.isSuperAdmin && !userInfoStore.isAdmin"
										:icon="User"
										@click="employeeEditRef.open()"
									>
										职员信息
									</el-dropdown-item>
									<el-dropdown-item divided :icon="Iphone" @click="changeMobileRef.open()">修改手机</el-dropdown-item>
									<el-dropdown-item :icon="Message" @click="changeEmailRef.open()">修改邮箱</el-dropdown-item>
									<el-dropdown-item divided :icon="Refresh" @click="handleRefreshSystem">刷新系统</el-dropdown-item>
									<el-dropdown-item :icon="Lock" @click="handleScreenLock">锁定屏幕</el-dropdown-item>
								</el-dropdown-menu>
							</template>
						</el-dropdown>
						<el-icon
							class="setting fa__hover__twinkle"
							title="高级配置"
							role="button"
							tabindex="0"
							aria-label="高级配置"
							@keydown.enter.prevent="layoutConfigRef.open()"
							@keydown.space.prevent="layoutConfigRef.open()"
							@click="layoutConfigRef.open()"
						>
							<Setting />
						</el-icon>
						<el-icon
							class="logout fa__hover__twinkle"
							title="退出系统"
							role="button"
							tabindex="0"
							aria-label="退出系统"
							@keydown.enter.prevent="handleLogout"
							@keydown.space.prevent="handleLogout"
							@click="handleLogout"
						>
							<SwitchButton />
						</el-icon>
					</div>
				</div>
				<LayoutNavTab />
			</el-header>
			<el-main :style="{ '--el-main-padding': addCssUnit(configStore.layout.mainPadding) }">
				<el-scrollbar>
					<RouterView v-slot="{ Component, route }">
						<transition mode="out-in" :name="configStore.layout.mainAnimation">
							<KeepAlive :include="navTabsStore.keepAliveComponentNameList">
								<component :is="Component" :key="route.path" class="layout-main" />
							</KeepAlive>
						</transition>
					</RouterView>
				</el-scrollbar>
			</el-main>
			<el-footer :style="{ '--el-footer-height': configStore.layout.footer ? addCssUnit(configStore.layout.footerHeight) : 0 }">
				<Footer />
			</el-footer>
		</el-container>
		<teleport to="body">
			<transition name="slide-bottom" mode="out-in">
				<LayoutScreenLock v-if="configStore.screen.screenLock" />
			</transition>
		</teleport>
	</el-container>
</template>

<script setup lang="ts">
import { computed, inject, ref, useTemplateRef, watch } from "vue";
import { RouterView, useRouter } from "vue-router";
import { Expand, Fold, Iphone, Key, Lock, Message, Refresh, Search, Setting, SwitchButton, User, UserFilled } from "@element-plus/icons-vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { IdCard } from "@fast-element-plus/icons-vue";
import { Local, addCssUnit, useWindowSize } from "@fast-china/utils";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { accountEditKey, changeEmailKey, changeMobileKey, changePasswordKey, employeeEditKey, layoutConfigKey, menuSearchKey } from "@/layouts";
import LayoutBreadcrumb from "@/layouts/components/Breadcrumb/index.vue";
import LayoutLogo from "@/layouts/components/Logo/index.vue";
import LayoutNavTab from "@/layouts/components/NavTab/index.vue";
import LayoutScreenFull from "@/layouts/components/ScreenFull/index.vue";
import LayoutScreenLock from "@/layouts/components/ScreenLock/index.vue";
import { useConfig, useNavTabs, useUserInfo } from "@/stores";
import LayoutMenu from "./components/menu.vue";
import type { ElSelectorOutput, FaSelectInstance } from "fast-element-plus";

defineOptions({
	name: "LayoutClassic",
});

const router = useRouter();
const configStore = useConfig();
const navTabsStore = useNavTabs();
const userInfoStore = useUserInfo();
const windowSize = useWindowSize();

const layoutConfigRef = inject(layoutConfigKey);
const menuSearchRef = inject(menuSearchKey);
const changePasswordRef = inject(changePasswordKey);
const accountEditRef = inject(accountEditKey);
const employeeEditRef = inject(employeeEditKey);
const changeMobileRef = inject(changeMobileKey);
const changeEmailRef = inject(changeEmailKey);
const faTenantSelectRef = useTemplateRef<FaSelectInstance>("faTenantSelectRef");

/** 窄屏设备使用抽屉菜单，避免平板竖屏的内容区被侧栏持续占用。 */
const useDrawerMenu = computed(() => windowSize.width.value < 992);
/** 移动端菜单可见性 */
const mobileMenuVisible = ref(false);

/** 菜单切换 */
const handleMenuToggle = () => {
	if (useDrawerMenu.value) {
		mobileMenuVisible.value = !mobileMenuVisible.value;
	} else {
		configStore.layout.menuCollapse = !configStore.layout.menuCollapse;
	}
};

const handleRefreshSystem = () => {
	ElMessageBox.confirm("此操作会强制刷新当前页面，是否继续操作？", { type: "warning" }).then(() => {
		// 删除 HTTP 缓存数据
		Local.removeByPrefix("HTTP_CACHE_");
		// 刷新App
		window.location.reload();
	});
};

const handleTenantChange = async (data: ElSelectorOutput | ElSelectorOutput[]) => {
	if (Array.isArray(data)) return;
	const { userKey } = userInfoStore;
	if (data.userKey !== userKey) {
		const loginRes = await loginApi.tryLogin({ userKey: data.userKey });
		if (loginRes.status === LoginStatusEnum.Success) {
			ElMessage.success(`切换租户【${data.tenantName}】成功`);
			userInfoStore.login();
			// 删除 HTTP 缓存数据
			Local.removeByPrefix("HTTP_CACHE_");
			// 刷新App
			window.location.reload();
		} else {
			ElMessage.error(loginRes.message);
		}
	}
};

const handleScreenLock = () => {
	ElMessageBox.prompt("请输入锁屏密码", {
		showClose: false,
		confirmButtonText: "锁定",
		closeOnPressEscape: true,
		inputType: "password",
		inputPlaceholder: "请输入锁屏密码",
		inputValidator(value) {
			if (!value?.trim()) {
				return "锁屏密码不能为空";
			}
			return true;
		},
	}).then(({ value }) => {
		configStore.screen.password = value;
		configStore.screen.screenLock = true;
	});
};

const handleLogout = () => {
	ElMessageBox.confirm(`确定要退出登录？`, { type: "warning" }).then(async () => {
		await userInfoStore.logout();
		ElMessage.success(`退出登录成功`);
	});
};

watch(
	() => router.currentRoute.value.path,
	() => {
		mobileMenuVisible.value = false;
	}
);
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

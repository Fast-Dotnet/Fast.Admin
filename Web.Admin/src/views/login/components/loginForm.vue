<template>
	<section class="login-form" :class="`login-form--${props.variant}`">
		<header class="login-form__header">
			<div class="login-form__eyebrow">
				<span class="eyebrow-dot"></span>
				{{ variantEyebrow }}
			</div>
			<h2>{{ stepContent.title }}</h2>
			<p>{{ stepContent.description }}</p>
		</header>

		<transition mode="out-in" name="login-step">
			<div v-if="formStep !== 'SelectTenant'" :key="formStep" class="login-form__step">
				<div v-if="formStep === 'NewAccount'" class="login-form__back">
					<el-button link type="primary" :icon="ArrowLeftBold" @click="handleNewAccountBack">返回已保存账号</el-button>
				</div>

				<el-form
					ref="elFormRef"
					size="large"
					label-position="top"
					:model="formData"
					:rules="props.formRules"
					:disabled="faButtonRef?.loading"
					:scroll-to-error="false"
					@keyup.enter.prevent="handleKeyupEnter"
					@submit.prevent
				>
					<el-form-item v-if="formStep === 'TenantAccount' && tenantList.length > 0" label="工作空间" prop="userKey">
						<el-select
							v-model="formData.userKey"
							fit-input-width
							placeholder="选择已保存的租户"
							popper-class="login-tenant-popper"
							@change="handleTenantChange"
						>
							<el-option
								v-for="(item, index) in tenantList"
								:key="item.tenant.userKey"
								:label="item.tenant.tenantName"
								:value="item.tenant.userKey"
							>
								<div class="tenant-option">
									<img class="tenant-option__logo" :src="item.tenant.logoUrl" :alt="item.tenant.tenantName" />
									<div class="tenant-option__content">
										<strong>{{ item.tenant.tenantName }}</strong>
										<span>{{ item.tenant.departmentName || "无部门" }} · {{ item.tenant.employeeName }}</span>
									</div>
									<Tag size="small" name="EditionEnum" :value="item.tenant.edition" />
									<el-button
										class="tenant-option__remove"
										size="small"
										text
										circle
										:icon="Close"
										aria-label="移除已保存账号"
										@click.stop="handleTenantRemove(index, item)"
									/>
								</div>
							</el-option>
							<template #footer>
								<el-button type="primary" text :icon="Plus" @click="handleNewAccount">绑定新的租户账号</el-button>
							</template>
						</el-select>
					</el-form-item>

					<el-form-item prop="account">
						<template #label>
							<FaFormItemTip
								v-if="formStep === 'TenantAccount'"
								label="账号"
								tips="已授权租户不能修改账号，如需使用其他账号，请绑定新的租户账号"
							/>
							<span v-else>账号</span>
						</template>
						<el-input
							v-model.trim="formData.account"
							placeholder="请输入登录账号"
							:prefix-icon="User"
							:show-word-limit="false"
							:disabled="formStep === 'TenantAccount'"
							autocapitalize="off"
							autocomplete="username"
							spellcheck="false"
							tabindex="1"
							@change="handleAccountChange"
						/>
					</el-form-item>

					<el-form-item label="密码" prop="password">
						<el-input
							type="password"
							v-model.trim="formData.password"
							placeholder="请输入登录密码"
							:prefix-icon="Lock"
							show-password
							:show-word-limit="false"
							autocomplete="current-password"
							tabindex="2"
						/>
					</el-form-item>

					<ImageCaptcha
						ref="captchaRef"
						prop="captchaCode"
						v-model="formData.captchaCode"
						v-model:captcha-key="formData.captchaKey"
						:disabled="faButtonRef?.loading"
					/>

					<div class="login-form__meta">
						<el-checkbox v-model="formData.rememberMe" size="default">记住登录信息</el-checkbox>
						<el-button size="default" type="warning" link @click="passwordResetRef.open()">忘记密码</el-button>
					</div>

					<FaButton ref="faButtonRef" class="login-submit" size="large" type="primary" @click="handleFormLogin">
						<span>进入工作台</span>
						<el-icon><Right /></el-icon>
					</FaButton>
				</el-form>
			</div>

			<div v-else key="SelectTenant" class="login-form__step login-form__step--tenant">
				<div class="login-form__back">
					<el-button type="primary" link :disabled="faButtonRef?.loading" :icon="ArrowLeftBold" @click="handleTenantSelectionBack">
						<span>返回账号登录</span>
					</el-button>
				</div>

				<el-scrollbar class="tenant-list">
					<button
						v-for="item in tenantSelector"
						:key="item.userKey"
						class="tenant-card"
						type="button"
						:disabled="faButtonRef?.loading"
						@click="handleTenantLogin(item)"
					>
						<img :src="item.logoUrl" :alt="item.tenantName" />
						<span class="tenant-card__body">
							<strong>{{ item.tenantName }}</strong>
							<small>{{ item.departmentName || "无部门" }} · {{ item.employeeName }}</small>
						</span>
						<Tag size="small" name="EditionEnum" :value="item.edition" />
						<el-icon class="tenant-card__arrow"><Right /></el-icon>
					</button>
				</el-scrollbar>
			</div>
		</transition>

		<footer class="login-form__footer">
			<span></span>
			Powered by FastDotnet
		</footer>

		<PasswordReset ref="passwordResetRef" />
	</section>
</template>

<script lang="ts" setup>
import { computed, useTemplateRef } from "vue";
import { ArrowLeftBold, Close, Lock, Plus, Right, User } from "@element-plus/icons-vue";
import { type FaButtonInstance, useOverlay } from "fast-element-plus";
import ImageCaptcha from "@/components/ImageCaptcha/index.vue";
import { useApp } from "@/stores";
import { useLogin } from "../useLogin.ts";
import PasswordReset from "./passwordReset.vue";
import type { FormInstance, FormRules } from "element-plus";
import type { LoginTenantOutput } from "@/api/services/Auth/login/models/LoginTenantOutput";

defineOptions({
	name: "LoginForm",
});

type LoginVariant = "classic" | "modern" | "simple" | "split";

const props = defineProps<{
	/** 登录页视觉类型。 */
	variant: LoginVariant;
	/** 登录表单校验规则。 */
	formRules?: FormRules;
}>();

const appStore = useApp();
const elFormRef = useTemplateRef<FormInstance>("elFormRef");
const faButtonRef = useTemplateRef<FaButtonInstance>("faButtonRef");
const captchaRef = useTemplateRef<InstanceType<typeof ImageCaptcha>>("captchaRef");
const passwordResetRef = useTemplateRef<InstanceType<typeof PasswordReset>>("passwordResetRef");

const {
	formData,
	tenantList,
	formStep,
	tenantSelector,
	currentTenant,
	handleTenantChange,
	handleTenantRemove,
	handleNewAccount,
	handleNewAccountBack,
	handleAccountChange,
	handleLogin,
	handleFormLogin,
	handleKeyupEnter,
} = useLogin(elFormRef, faButtonRef, captchaRef);

const variantEyebrow = computed(() => {
	switch (props.variant) {
		case "classic":
			return "ENTERPRISE MANAGEMENT";
		case "modern":
			return "SECURE DIGITAL WORKSPACE";
		case "split":
			return "UNIFIED MANAGEMENT ACCESS";
		case "simple":
			return "FOCUS · CREATE · DELIVER";
	}
});

const stepContent = computed(() => {
	switch (formStep.value) {
		case "Account":
			return { title: "欢迎登录", description: `进入 ${appStore.appName}，开启高效工作` };
		case "TenantAccount":
			return {
				title: "欢迎回来",
				description: currentTenant.value
					? `${currentTenant.value.tenantName} · ${currentTenant.value.employeeName}`
					: "选择工作空间后继续登录",
			};
		case "NewAccount":
			return { title: "绑定新账号", description: "使用新的租户账号进入工作空间" };
		case "SelectTenant":
			return { title: "选择工作空间", description: "此账号关联了多个租户，请选择本次登录入口" };
	}
});

const handleTenantSelectionBack = () => {
	formData.value.loginTicket = undefined;
	formData.value.userKey = undefined;
	formStep.value = tenantList.value.length > 0 ? "NewAccount" : "Account";
};

const handleTenantLogin = async (tenant: LoginTenantOutput) => {
	formData.value.userKey = tenant.userKey;
	useOverlay.show();
	await handleLogin(null, () => useOverlay.hide());
};
</script>

<style scoped lang="scss">
.login-form {
	width: 100%;
	color: var(--login-text, #162033);

	.login-form__header {
		margin-bottom: 28px;

		h2 {
			margin: 10px 0 8px;
			font-size: clamp(28px, 3vw, 36px);
			font-weight: 600;
			line-height: 1.16;
			letter-spacing: -1.2px;
			color: var(--login-heading, #101828);
		}

		p {
			margin: 0;
			font-size: 14px;
			line-height: 1.65;
			color: var(--login-muted, #667085);
		}
	}

	.login-form__eyebrow {
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 1.8px;
		color: var(--el-color-primary);
	}
	.login-form {
		.eyebrow-dot {
			width: 7px;
			height: 7px;
			border-radius: 50%;
			background: #22c55e;
			box-shadow: 0 0 0 5px rgb(34 197 94 / 12%);
			animation: loginStatusPulse 2.4s ease-in-out infinite;
		}
	}

	.login-form__step {
		min-height: 334px;
	}

	.login-form__step--tenant {
		display: flex;
		min-height: 360px;
		flex-direction: column;
	}

	.login-form__back {
		margin: -8px 0 14px;

		:deep(.el-button > span) {
			margin-left: 0;
		}
	}

	.login-form__meta {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin: -2px 0 20px;
	}

	.login-form__footer {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 9px;
		margin-top: 18px;
		font-size: 11px;
		letter-spacing: 0.6px;
		color: var(--login-faint, #98a2b3);

		span {
			width: 18px;
			height: 1px;
			background: currentcolor;
		}
	}

	:deep(.el-form-item) {
		margin-bottom: 20px;
	}

	:deep(.el-form-item__label) {
		padding-bottom: 7px;
		font-size: 13px;
		font-weight: 600;
		color: var(--login-label, #344054);
	}

	:deep(.el-input__wrapper),
	:deep(.el-select__wrapper) {
		min-height: 48px;
		padding-inline: 15px;
		border: 1px solid var(--login-input-border, rgb(16 24 40 / 10%));
		border-radius: 13px;
		background: var(--login-input-bg, rgb(255 255 255 / 78%));
		box-shadow: 0 1px 2px rgb(16 24 40 / 3%);
		transition:
			border-color 180ms ease,
			box-shadow 180ms ease,
			background-color 180ms ease,
			transform 180ms ease;

		&:hover {
			border-color: color-mix(in srgb, var(--el-color-primary) 45%, transparent);
		}
	}

	:deep(.el-input__wrapper.is-focus),
	:deep(.el-select__wrapper.is-focused) {
		border-color: color-mix(in srgb, var(--el-color-primary) 70%, white);
		background: var(--login-input-focus-bg, #fff);
		box-shadow: 0 0 0 4px color-mix(in srgb, var(--el-color-primary) 12%, transparent);
		transform: translateY(-1px);
	}

	:deep(.el-input__inner) {
		color: var(--login-heading, #101828);

		&::placeholder {
			color: var(--login-faint, #98a2b3);
		}
	}

	:deep(.el-input__prefix),
	:deep(.el-select__prefix) {
		color: var(--el-color-primary);
	}

	:deep(.el-checkbox__label) {
		font-size: 13px;
		color: var(--login-muted, #667085);
	}

	.login-submit {
		width: 100%;
		height: 50px;
		border: 0;
		border-radius: 13px;
		font-size: 15px;
		font-weight: 600;
		letter-spacing: 0.8px;
		background: linear-gradient(110deg, var(--el-color-primary), color-mix(in srgb, var(--el-color-primary) 68%, #7259ff));
		box-shadow: 0 12px 24px color-mix(in srgb, var(--el-color-primary) 25%, transparent);
		transition:
			transform 180ms ease,
			box-shadow 180ms ease,
			filter 180ms ease;

		:deep(> span) {
			display: flex;
			align-items: center;
			justify-content: center;
			gap: 10px;
		}

		&:hover {
			filter: saturate(1.08) brightness(1.04);
			box-shadow: 0 16px 30px color-mix(in srgb, var(--el-color-primary) 32%, transparent);
			transform: translateY(-2px);
		}

		&:active {
			transform: translateY(0);
		}
	}
}

.tenant-list {
	flex: 0 0 400px;
	height: 400px;
	margin-right: -8px;
	padding-right: 8px;
}

.tenant-card {
	display: grid;
	width: 100%;
	grid-template-columns: 42px minmax(0, 1fr) auto 20px;
	align-items: center;
	gap: 12px;
	margin-top: 5px;
	margin-bottom: 5px;
	padding: 12px;
	color: var(--login-text, #162033);
	text-align: left;
	cursor: pointer;
	border: 1px solid var(--login-input-border, rgb(16 24 40 / 10%));
	border-radius: 14px;
	background: var(--login-input-bg, rgb(255 255 255 / 78%));
	transition:
		border-color 180ms ease,
		background-color 180ms ease,
		transform 180ms ease,
		box-shadow 180ms ease;

	> img {
		width: 42px;
		height: 42px;
		object-fit: cover;
		border-radius: 12px;
		box-shadow: 0 5px 12px rgb(16 24 40 / 10%);
	}

	.tenant-card__body {
		display: flex;
		flex-direction: column;
		gap: 4px;

		strong,
		small {
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
		}

		strong {
			font-size: 14px;
			font-weight: 600;
		}

		small {
			color: var(--login-muted, #667085);
		}
	}

	.tenant-card__arrow {
		color: var(--login-faint, #98a2b3);
		transition: transform 180ms ease;
	}

	&:hover,
	&:focus-visible {
		outline: none;
		border-color: color-mix(in srgb, var(--el-color-primary) 50%, transparent);
		background: var(--login-input-focus-bg, #fff);
		box-shadow: 0 10px 24px color-mix(in srgb, var(--el-color-primary) 10%, transparent);
		transform: translateY(-2px);

		.tenant-card__arrow {
			color: var(--el-color-primary);
			transform: translateX(3px);
		}
	}
}

:global(.login-tenant-popper) {
	max-width: min(430px, calc(100vw - 24px));
	border: 1px solid var(--el-border-color-lighter);
	border-radius: 14px;
	box-shadow: 0 18px 50px rgb(15 23 42 / 18%);
	overflow: hidden;
}

:global(.login-tenant-popper .el-select-dropdown__list) {
	padding: 0;
}

:global(.login-tenant-popper .el-select-dropdown__item) {
	height: auto;
	min-height: 72px;
	margin: 0;
	padding: 12px 14px;
	line-height: 1.4;
	border-bottom: 1px solid var(--el-border-color-lighter);
	border-radius: 0;
	background: transparent;
	transition:
		color 180ms ease,
		background-color 180ms ease;
}

:global(.login-tenant-popper .el-select-dropdown__item:last-child) {
	border-bottom: 0;
}

:global(.login-tenant-popper .el-select-dropdown__item.is-hovering) {
	background: var(--el-color-primary-light-9);
}

:global(.login-tenant-popper .el-select-dropdown__item.is-selected) {
	background: color-mix(in srgb, var(--el-color-primary) 12%, var(--el-bg-color));
}

:global(.login-tenant-popper .el-select-dropdown__item.is-selected .tenant-option__content strong) {
	color: var(--el-color-primary);
}

:global(.login-tenant-popper .el-select-dropdown__footer) {
	padding: 0;
	border-top: 1px solid var(--el-border-color-lighter);
}

:global(.login-tenant-popper .el-select-dropdown__footer .el-button) {
	width: 100%;
	height: 52px;
	margin: 0;
	border-radius: 0;
}

:global(.login-tenant-popper .tenant-option) {
	display: grid;
	width: 100%;
	min-width: 0;
	grid-template-columns: 42px minmax(0, 1fr) auto 32px;
	align-items: center;
	gap: 12px;
}

:global(.login-tenant-popper .tenant-option__logo) {
	width: 42px;
	height: 42px;
	object-fit: cover;
	border: 1px solid var(--el-border-color-lighter);
	border-radius: 11px;
	box-shadow: 0 3px 8px rgb(15 23 42 / 10%);
}

:global(.login-tenant-popper .tenant-option__content) {
	display: flex;
	min-width: 0;
	flex-direction: column;
	gap: 4px;
}

:global(.login-tenant-popper .tenant-option__content strong),
:global(.login-tenant-popper .tenant-option__content span) {
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

:global(.login-tenant-popper .tenant-option__content strong) {
	color: var(--el-text-color-primary);
	font-size: 14px;
	font-weight: 600;
}

:global(.login-tenant-popper .tenant-option__content span) {
	font-size: 12px;
	color: var(--el-text-color-secondary);
}

:global(.login-tenant-popper .tenant-option__remove) {
	width: 28px;
	height: 28px;
	padding: 0;
	color: var(--el-text-color-placeholder);
	transition:
		color 180ms ease,
		background-color 180ms ease,
		transform 180ms ease;
}

:global(.login-tenant-popper .tenant-option__remove:hover) {
	color: var(--el-color-danger);
	background: var(--el-color-danger-light-9);
	transform: rotate(90deg) scale(1.06);
}

:global(.login-tenant-popper .tenant-option__remove:active) {
	transform: rotate(90deg) scale(0.9);
}

.login-step-enter-active,
.login-step-leave-active {
	transition:
		opacity 180ms ease,
		transform 240ms cubic-bezier(0.22, 1, 0.36, 1),
		filter 180ms ease;
}

.login-step-enter-from {
	opacity: 0;
	filter: blur(4px);
	transform: translateX(18px);
}

.login-step-leave-to {
	opacity: 0;
	filter: blur(4px);
	transform: translateX(-14px);
}

@keyframes loginStatusPulse {
	0%,
	100% {
		box-shadow: 0 0 0 4px rgb(34 197 94 / 10%);
	}

	50% {
		box-shadow: 0 0 0 7px rgb(34 197 94 / 4%);
	}
}

@media (max-width: 520px) {
	.login-form {
		.login-form__header {
			margin-bottom: 18px;

			h2 {
				margin-block: 8px 6px;
				font-size: 27px;
			}
		}

		.login-form__step {
			min-height: 300px;
		}

		.login-form__footer {
			margin-top: 14px;
		}

		.login-form__meta {
			margin-bottom: 16px;
		}

		:deep(.el-form-item) {
			margin-bottom: 16px;
		}
	}
}

@media (prefers-reduced-motion: reduce) {
	.login-form *,
	.login-form *::before,
	.login-form *::after,
	:global(.login-tenant-popper .tenant-option__remove) {
		scroll-behavior: auto !important;
		animation-duration: 0.01ms !important;
		animation-iteration-count: 1 !important;
		transition-duration: 0.01ms !important;
	}
}
</style>

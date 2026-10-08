import { computed, inject, ref } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { type FaButtonInstance, formUtil, useOverlay } from "fast-element-plus";
import { Local } from "@fast-china/utils";
import { LoginStatusEnum } from "@/api/enums/LoginStatusEnum";
import { loginApi } from "@/api/services/Auth/login";
import { useUserInfo } from "@/stores";
import type { FormInstance } from "element-plus";
import type { Ref } from "vue";
import type { LoginInput } from "@/api/services/Auth/login/models/LoginInput";
import type { LoginOutput } from "@/api/services/Auth/login/models/LoginOutput";
import type { LoginTenantOutput } from "@/api/services/Auth/login/models/LoginTenantOutput";
import type { TenantLoginInput } from "@/api/services/Auth/login/models/TenantLoginInput";
import type { ImageCaptcha } from "@/components";

/** 账号登录与租户登录共用的表单数据 */
export type IFormData = {
	/** 记住登录信息 */
	rememberMe?: boolean;
} & LoginInput &
	TenantLoginInput;

/** 本地保存的租户登录条目 */
export type ITenantData = {
	/** 租户 */
	tenant: LoginTenantOutput & { userKey: string };
	/** 表单数据 */
	formData: IFormData;
};

/** 登录表单步骤 */
export type IFormStep = "Account" | "TenantAccount" | "SelectTenant" | "NewAccount";

/** 从上层注入登录状态，使不同登录页布局复用同一套交互。 */
const createLogin = (elFormRef: Ref<FormInstance>, faButtonRef: Ref<FaButtonInstance>, captchaRef: Ref<InstanceType<typeof ImageCaptcha>>) => {
	const userInfoStore = useUserInfo();

	/** 表单数据 */
	const formData = inject<Ref<IFormData>>("formData");
	/** 租户集合 */
	const tenantList = inject<Ref<ITenantData[]>>("tenantList");
	/** 表单步骤 */
	const formStep = inject<Ref<IFormStep>>("formStep");
	/** 缓存Key */
	const cFormKey = inject<string>("cFormKey");

	/** 租户选择器 */
	const tenantSelector = ref<LoginTenantOutput[]>([]);
	/** 当前选择租户 */
	const currentTenant = computed<LoginTenantOutput>(() => tenantList.value.find((item) => item.tenant.userKey === formData.value.userKey)?.tenant);

	/** 租户改变 */
	const handleTenantChange = (value: string) => {
		const fInfo = tenantList.value.find((f) => f.tenant.userKey === value);
		if (!fInfo) {
			ElMessage.error("租户信息不存在");
			return;
		}
		formData.value = { ...fInfo.formData, userKey: fInfo.tenant.userKey, captchaKey: formData.value.captchaKey, captchaCode: undefined };
	};

	/** 租户刷新 */
	const handleRefreshTenant = () => {
		if (tenantList.value.length === 0) {
			formData.value = { captchaKey: formData.value.captchaKey, rememberMe: false };
			Local.remove(cFormKey);
			formStep.value = "Account";
		} else {
			const tenant = tenantList.value[0];
			formData.value = { ...tenant.formData, userKey: tenant.tenant.userKey, captchaKey: formData.value.captchaKey, captchaCode: undefined };
			Local.set(cFormKey, tenantList.value);
		}
	};

	/** 租户删除 */
	const handleTenantRemove = (index: number, value: ITenantData) => {
		ElMessageBox.confirm("您确定要移除此登录信息吗？", {
			dangerouslyUseHTMLString: true,
		}).then(() => {
			if (value.tenant.userKey === formData.value.userKey) {
				formData.value.userKey = undefined;
			}
			tenantList.value.splice(index, 1);
			handleRefreshTenant();
		});
	};

	/** 新账号 */
	const handleNewAccount = () => {
		formStep.value = "NewAccount";
		formData.value = { captchaKey: formData.value.captchaKey, rememberMe: false };
	};

	/** 新账号返回 */
	const handleNewAccountBack = () => {
		handleRefreshTenant();
		formStep.value = tenantList.value.length > 0 ? "TenantAccount" : "Account";
	};

	/** 账号改变 */
	const handleAccountChange = () => {
		formData.value.password = undefined;
		formData.value.loginTicket = undefined;
	};

	/** 登录 */
	const handleLogin = async (_event: MouseEvent, done: () => void) => {
		const { account, password, loginTicket, userKey, rememberMe, captchaKey, captchaCode } = formData.value;
		// 这里判断登录凭据和密码是否都为空，如果是，则提示用户输入密码
		if (!loginTicket && !password) {
			ElMessage.warning("请输入密码");
			return;
		}
		try {
			let apiRes: LoginOutput;
			// 判断是否存在用户Key，如果存在直接租户登录
			if (userKey) {
				apiRes = await loginApi.tenantLogin({ userKey, password: loginTicket ? undefined : password, loginTicket, captchaKey, captchaCode });
			} else {
				apiRes = await loginApi.login({ account, password, captchaKey, captchaCode });
			}
			switch (apiRes.status) {
				// 登录成功
				case LoginStatusEnum.Success:
					{
						const tenantInfo = apiRes.tenantList?.[0];
						const fIdx = tenantList.value.findIndex((f) => f.tenant.userKey === tenantInfo.userKey);
						if (fIdx >= 0) {
							tenantList.value.splice(fIdx, 1);
						}
						const cacheFormData: IFormData = { ...formData.value, captchaKey: undefined, captchaCode: undefined, loginTicket: undefined };
						tenantList.value.unshift({
							tenant: { ...tenantInfo, userKey: tenantInfo.userKey },
							formData: rememberMe
								? cacheFormData
								: {
										...cacheFormData,
										rememberMe: false,
										password: undefined,
									},
						});
						Local.set(cFormKey, tenantList.value);
						userInfoStore.login();
					}
					break;
				// 选择租户登录
				case LoginStatusEnum.SelectTenant:
					ElMessage.success(apiRes.message);
					tenantSelector.value = apiRes.tenantList;
					formData.value.loginTicket = apiRes.loginTicket;
					formStep.value = "SelectTenant";
					break;
				case LoginStatusEnum.AuthExpired:
				case LoginStatusEnum.NotAccount:
				default:
					ElMessage.error(apiRes.message || "登录失败，请稍后重试");
			}
		} catch {
			captchaRef.value?.refresh();
		} finally {
			if (loginTicket) formData.value.loginTicket = undefined;
			done?.();
		}
	};

	/** 表单登录 */
	const handleFormLogin = (event: MouseEvent, done?: () => void) => {
		formUtil
			.validate(elFormRef)
			.then(() => handleLogin(event, done))
			.catch(() => done?.());
	};

	/** 回车提交时同时控制按钮与遮罩的加载状态。 */
	const handleKeyupEnter = () => {
		useOverlay.show();
		faButtonRef.value.loading = true;
		handleFormLogin(null, () => {
			useOverlay.hide();
			faButtonRef.value.loading = false;
		});
	};

	return {
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
	};
};

/**
 * 创建登录页共享的表单与租户切换操作。
 *
 * @remarks
 * 必须在 setup 中调用，并由上层提供 formData、tenantList、formStep 和 cFormKey。
 *
 * @param elFormRef - 登录表单实例
 * @param faButtonRef - 登录按钮实例，用于管理回车提交时的加载状态
 * @param captchaRef - 图形验证码实例，用于登录失败后的刷新
 * @returns 登录状态与表单操作方法
 */
export const useLogin = (
	elFormRef: Ref<FormInstance>,
	faButtonRef: Ref<FaButtonInstance>,
	captchaRef: Ref<InstanceType<typeof ImageCaptcha>>
): ReturnType<typeof createLogin> => createLogin(elFormRef, faButtonRef, captchaRef);

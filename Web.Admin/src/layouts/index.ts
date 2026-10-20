import type { InjectionKey, Ref } from "vue";
import type AccountEdit from "@/layouts/components/AccountEdit/index.vue";
import type ChangeEmail from "@/layouts/components/ChangeEmail/index.vue";
import type ChangeMobile from "@/layouts/components/ChangeMobile/index.vue";
import type ChangePassword from "@/layouts/components/ChangePassword/index.vue";
import type layoutConfig from "@/layouts/components/Config/index.vue";
import type EmployeeEdit from "@/layouts/components/EmployeeEdit/index.vue";
import type menuSearch from "@/layouts/components/MenuSearch/index.vue";

/** 布局配置组件引用的注入键 */
export const layoutConfigKey: InjectionKey<Ref<InstanceType<typeof layoutConfig>>> = Symbol("layoutConfigKey");
/** 菜单搜索组件引用的注入键 */
export const menuSearchKey: InjectionKey<Ref<InstanceType<typeof menuSearch>>> = Symbol("menuSearchKey");
/** 修改密码弹窗引用的注入键 */
export const changePasswordKey: InjectionKey<Ref<InstanceType<typeof ChangePassword>>> = Symbol("changePasswordKey");
/** 账号编辑弹窗引用的注入键 */
export const accountEditKey: InjectionKey<Ref<InstanceType<typeof AccountEdit>>> = Symbol("accountEditKey");
/** 职员编辑弹窗引用的注入键 */
export const employeeEditKey: InjectionKey<Ref<InstanceType<typeof EmployeeEdit>>> = Symbol("employeeEditKey");
/** 修改手机号弹窗引用的注入键 */
export const changeMobileKey: InjectionKey<Ref<InstanceType<typeof ChangeMobile>>> = Symbol("changeMobileKey");
/** 修改邮箱弹窗引用的注入键 */
export const changeEmailKey: InjectionKey<Ref<InstanceType<typeof ChangeEmail>>> = Symbol("changeEmailKey");

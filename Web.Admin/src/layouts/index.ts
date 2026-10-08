import type { InjectionKey, Ref } from "vue";
import type ChangePassword from "@/layouts/components/ChangePassword/index.vue";
import type layoutConfig from "@/layouts/components/Config/index.vue";
import type menuSearch from "@/layouts/components/MenuSearch/index.vue";

/** 布局配置组件引用的注入键 */
export const layoutConfigKey: InjectionKey<Ref<InstanceType<typeof layoutConfig>>> = Symbol("layoutConfigKey");
/** 菜单搜索组件引用的注入键 */
export const menuSearchKey: InjectionKey<Ref<InstanceType<typeof menuSearch>>> = Symbol("menuSearchKey");
/** 修改密码弹窗引用的注入键 */
export const changePasswordKey: InjectionKey<Ref<InstanceType<typeof ChangePassword>>> = Symbol("changePasswordKey");

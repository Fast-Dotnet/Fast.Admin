import AccountSelect from "./accountSelect/index.vue";
import ApplicationClientSelect from "./applicationClientSelect/index.vue";
import ApplicationSelect from "./applicationSelect/index.vue";
import Auth from "./auth/index.vue";
import Avatar from "./avatar/index.vue";
import Card from "./card/index.vue";
import CellPicker from "./cellPicker/index.vue";
import CheckboxGroup from "./checkboxGroup/index.vue";
import CitySelect from "./citySelect/index.vue";
import DepartmentTreeSelect from "./departmentTreeSelect/index.vue";
import EmployeeSelect from "./employeeSelect/index.vue";
import Field from "./field/index.vue";
import Footer from "./footer/index.vue";
import Icon from "./icon/index.vue";
import Image from "./image/index.vue";
import ImageCaptcha from "./imageCaptcha/index.vue";
import Loading from "./loading/index.vue";
import LoadingPage from "./loadingPage/index.vue";
import MerchantSelect from "./merchantSelect/index.vue";
import NavbarCapsule from "./navbarCapsule/index.vue";
import Overlay from "./overlay/index.vue";
import Paging from "./paging/index.vue";
import Popup from "./popup/index.vue";
import ProvinceSelect from "./provinceSelect/index.vue";
import RadioGroup from "./radioGroup/index.vue";
import RegionSelect from "./regionSelect/index.vue";
import SearchInput from "./searchInput/index.vue";
import SelectPage from "./selectPage/index.vue";
import SwitchTenantPopup from "./switchTenantPopup/index.vue";
import Tag from "./tag/index.vue";
import TagFlag from "./tagFlag/index.vue";
import TenantSelect from "./tenantSelect/index.vue";
import Text from "./text/index.vue";
import TreePicker from "./treePicker/index.vue";
import UploadImage from "./uploadImage/index.vue";
import UploadImages from "./uploadImages/index.vue";
import Watermark from "./watermark/index.vue";
import type { ComponentExposed } from "vue-component-type-helpers";

export { AccountSelect };
export type FaAccountSelectInstance = InstanceType<typeof AccountSelect>;
export { ApplicationClientSelect };
export type FaApplicationClientSelectInstance = InstanceType<typeof ApplicationClientSelect>;
export { ApplicationSelect };
export type FaApplicationSelectInstance = InstanceType<typeof ApplicationSelect>;
export { Auth };
export type FaAuthInstance = InstanceType<typeof Auth>;
export { Avatar };
export type FaAvatarInstance = InstanceType<typeof Avatar>;
export { Card };
export type FaCardInstance = InstanceType<typeof Card>;
export { CellPicker };
export type FaCellPickerInstance<TValue extends string | number | boolean> = ComponentExposed<typeof CellPicker<TValue>>;
export { CheckboxGroup };
export type FaCheckboxGroupInstance = InstanceType<typeof CheckboxGroup>;
export { CitySelect };
export type FaCitySelectInstance = InstanceType<typeof CitySelect>;
export { DepartmentTreeSelect };
export type FaDepartmentTreeSelectInstance = InstanceType<typeof DepartmentTreeSelect>;
export { EmployeeSelect };
export type FaEmployeeSelectInstance = InstanceType<typeof EmployeeSelect>;
export { Field };
export type FaFieldInstance = InstanceType<typeof Field>;
export { Footer };
/** Footer 组件实例 */
export type FaFooterInstance = InstanceType<typeof Footer>;
export { Icon };
/** Icon 组件实例 */
export type FaIconInstance = InstanceType<typeof Icon>;
export { Image };
/** Image 组件实例 */
export type FaImageInstance = InstanceType<typeof Image>;
export { ImageCaptcha };
/** ImageCaptcha 组件实例 */
export type FaImageCaptchaInstance = InstanceType<typeof ImageCaptcha>;
export { Loading };
/** Loading 组件实例 */
export type FaLoadingInstance = InstanceType<typeof Loading>;
export { LoadingPage };
/** LoadingPage 组件实例 */
export type FaLoadingPageInstance = InstanceType<typeof LoadingPage>;
export { MerchantSelect };
/** MerchantSelect 组件实例 */
export type FaMerchantSelectInstance = InstanceType<typeof MerchantSelect>;
export { NavbarCapsule };
/** NavbarCapsule 组件实例 */
export type FaNavbarCapsuleInstance = InstanceType<typeof NavbarCapsule>;
export { Overlay };
/** Overlay 组件实例 */
export type FaOverlayInstance = InstanceType<typeof Overlay>;
export { Paging };
/** Paging 组件实例，保留组件泛型参数 */
export type FaPagingInstance<TResult = unknown, TInput extends string | number | boolean | PagedInput = PagedInput> = ComponentExposed<
	typeof Paging<TResult, TInput>
>;
export { Popup };
/** Popup 组件实例 */
export type FaPopupInstance = InstanceType<typeof Popup>;
export { ProvinceSelect };
/** ProvinceSelect 组件实例 */
export type FaProvinceSelectInstance = InstanceType<typeof ProvinceSelect>;
export { RadioGroup };
/** RadioGroup 组件实例 */
export type FaRadioGroupInstance = InstanceType<typeof RadioGroup>;
export { RegionSelect };
/** RegionSelect 组件实例 */
export type FaRegionSelectInstance = InstanceType<typeof RegionSelect>;
export { SearchInput };
/** SearchInput 组件实例，保留组件泛型参数 */
export type FaSearchInputInstance<TInput extends PagedInput = PagedInput> = ComponentExposed<typeof SearchInput<TInput>>;
export { SelectPage };
/** SelectPage 组件实例，保留组件泛型参数 */
export type FaSelectPageInstance<
	TValue extends string | number | boolean,
	TResult extends ElSelectorOutput = ElSelectorOutput<TValue>,
	TInput extends string | number | boolean | PagedInput = PagedInput,
> = ComponentExposed<typeof SelectPage<TValue, TResult, TInput>>;
export { SwitchTenantPopup };
/** SwitchTenantPopup 组件实例 */
export type FaSwitchTenantPopupInstance = InstanceType<typeof SwitchTenantPopup>;
export { Tag };
/** Tag 组件实例 */
export type FaTagInstance = InstanceType<typeof Tag>;
export { TagFlag };
/** TagFlag 组件实例 */
export type FaTagFlagInstance = InstanceType<typeof TagFlag>;
export { TenantSelect };
/** TenantSelect 组件实例 */
export type FaTenantSelectInstance = InstanceType<typeof TenantSelect>;
export { Text };
/** Text 组件实例 */
export type FaTextInstance = InstanceType<typeof Text>;
export { TreePicker };
/** TreePicker 组件实例，保留组件泛型参数 */
export type FaTreePickerInstance<
	TValue extends string | number | boolean,
	TResult extends ElTreeOutput = ElTreeOutput<TValue>,
	TInput extends string | number | boolean | PagedInput = PagedInput,
> = ComponentExposed<typeof TreePicker<TValue, TResult, TInput>>;
export { UploadImage };
/** UploadImage 组件实例 */
export type FaUploadImageInstance = InstanceType<typeof UploadImage>;
export { UploadImages };
/** UploadImages 组件实例 */
export type FaUploadImagesInstance = InstanceType<typeof UploadImages>;
export { Watermark };
/** Watermark 组件实例 */
export type FaWatermarkInstance = InstanceType<typeof Watermark>;

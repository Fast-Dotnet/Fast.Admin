import { actionSheetProps } from "@wot-ui/ui/components/wd-action-sheet/types";
import { calendarProps } from "@wot-ui/ui/components/wd-calendar/types";
import { cascaderProps } from "@wot-ui/ui/components/wd-cascader/types";
import { curtainProps } from "@wot-ui/ui/components/wd-curtain/types";
import { datetimePickerProps } from "@wot-ui/ui/components/wd-datetime-picker/types";
import { defaultOptions as dialogDefaultOptions } from "@wot-ui/ui/components/wd-dialog";
import { dialogProps } from "@wot-ui/ui/components/wd-dialog/types";
import { defaultOptions as imagePreviewDefaultOptions } from "@wot-ui/ui/components/wd-image-preview";
import { imagePreviewProps } from "@wot-ui/ui/components/wd-image-preview/types";
import { inputProps } from "@wot-ui/ui/components/wd-input/types";
import { keyboardProps } from "@wot-ui/ui/components/wd-keyboard/types";
import { navbarProps } from "@wot-ui/ui/components/wd-navbar/types";
import { setNotifyDefaultOptions } from "@wot-ui/ui/components/wd-notify";
import { notifyProps } from "@wot-ui/ui/components/wd-notify/types";
import { overlayProps } from "@wot-ui/ui/components/wd-overlay/types";
import { pickerProps } from "@wot-ui/ui/components/wd-picker/types";
import { popupProps } from "@wot-ui/ui/components/wd-popup/types";
import { selectPickerProps } from "@wot-ui/ui/components/wd-select-picker/types";
import { tabbarProps } from "@wot-ui/ui/components/wd-tabbar/types";
import { textareaProps } from "@wot-ui/ui/components/wd-textarea/types";
import { toastProps } from "@wot-ui/ui/components/wd-toast/types";
import { defaultOptions as videoPreviewDefaultOptions } from "@wot-ui/ui/components/wd-video-preview";
import { videoPreviewProps } from "@wot-ui/ui/components/wd-video-preview/types";

/** 设置 Wot UI 的应用默认值。 */
export function loadWotUi(): void {
	// 默认显示字数统计
	inputProps.showWordLimit.default = true;
	// 默认显示字数统计
	textareaProps.showWordLimit.default = true;
	// 对话框默认使用文字按钮
	dialogDefaultOptions.theme = "text";
	// 对话框层级高于业务弹窗和选择器
	dialogDefaultOptions.zIndex = 1001;
	// 轻提示默认显示在屏幕中部偏上
	toastProps.position.default = "middle-top";
	// 轻提示层级高于全局加载遮罩
	toastProps.zIndex.default = 2099;
	// 直接使用 wd-notify 时的默认层级
	notifyProps.zIndex.default = 1100;
	// 通过 showNotify 调用时的默认层级
	setNotifyDefaultOptions({ zIndex: 1100 });
	// 导航栏低于业务弹窗
	tabbarProps.zIndex.default = 90;
	navbarProps.zIndex.default = 101;

	// 业务弹窗、幕帘和独立遮罩
	overlayProps.zIndex.default = 200;
	popupProps.zIndex.default = 200;
	curtainProps.zIndex.default = 200;

	// 操作面板和选择器可以从业务弹窗中打开
	actionSheetProps.zIndex.default = 210;
	calendarProps.zIndex.default = 210;
	cascaderProps.zIndex.default = 210;
	datetimePickerProps.zIndex.default = 210;
	pickerProps.zIndex.default = 210;
	selectPickerProps.zIndex.default = 210;

	// 默认按月、年切换，带时间的变体从面板高度中扣除 120px 给滚轮。
	calendarProps.switchMode.default = "year-month";
	calendarProps.panelHeight.default = 394;

	// 自定义键盘可以从选择器中打开
	keyboardProps.zIndex.default = 220;

	// 预览同时支持组件和组合式 API，层级低于确认对话框
	imagePreviewProps.zIndex.default = 1000;
	imagePreviewDefaultOptions.zIndex = 1000;
	videoPreviewProps.zIndex.default = 1000;
	videoPreviewDefaultOptions.zIndex = 1000;

	// 弹层默认传送到 body，避开页面内的层叠上下文和裁剪
	actionSheetProps.rootPortal.default = true;
	calendarProps.rootPortal.default = true;
	cascaderProps.rootPortal.default = true;
	curtainProps.rootPortal.default = true;
	datetimePickerProps.rootPortal.default = true;
	dialogProps.rootPortal.default = true;
	keyboardProps.rootPortal.default = true;
	notifyProps.rootPortal.default = true;
	pickerProps.rootPortal.default = true;
	popupProps.rootPortal.default = true;
	selectPickerProps.rootPortal.default = true;
	setNotifyDefaultOptions({ rootPortal: true });
}

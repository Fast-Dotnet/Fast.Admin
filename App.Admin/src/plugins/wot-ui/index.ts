import { actionSheetProps } from "@wot-ui/ui/components/wd-action-sheet/types";
import { calendarProps } from "@wot-ui/ui/components/wd-calendar/types";
import { cascaderProps } from "@wot-ui/ui/components/wd-cascader/types";
import { curtainProps } from "@wot-ui/ui/components/wd-curtain/types";
import { datetimePickerProps } from "@wot-ui/ui/components/wd-datetime-picker/types";
import { defaultOptions as dialogDefaultOptions } from "@wot-ui/ui/components/wd-dialog";
import { dialogProps } from "@wot-ui/ui/components/wd-dialog/types";
import { dorpMenuItemProps } from "@wot-ui/ui/components/wd-drop-menu-item/types";
import { inputProps } from "@wot-ui/ui/components/wd-input/types";
import { keyboardProps } from "@wot-ui/ui/components/wd-keyboard/types";
import { setNotifyDefaultOptions } from "@wot-ui/ui/components/wd-notify";
import { notifyProps } from "@wot-ui/ui/components/wd-notify/types";
import { pickerProps } from "@wot-ui/ui/components/wd-picker/types";
import { popupProps } from "@wot-ui/ui/components/wd-popup/types";
import { selectPickerProps } from "@wot-ui/ui/components/wd-select-picker/types";
import { tabbarProps } from "@wot-ui/ui/components/wd-tabbar/types";
import { textareaProps } from "@wot-ui/ui/components/wd-textarea/types";
import { toastProps } from "@wot-ui/ui/components/wd-toast/types";

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
	// 底部导航层级低于业务弹窗
	tabbarProps.zIndex.default = 90;
	// 普通弹窗面板和遮罩的默认层级
	popupProps.zIndex.default = 200;
	// 选择器层级高于普通弹窗
	pickerProps.zIndex.default = 210;
	// 日期时间选择器层级高于筛选弹窗
	datetimePickerProps.zIndex.default = 210;

	// #ifdef H5
	/** 弹层默认传送到 body，避开页面内的层叠上下文和裁剪 */
	actionSheetProps.rootPortal.default = true;
	calendarProps.rootPortal.default = true;
	cascaderProps.rootPortal.default = true;
	curtainProps.rootPortal.default = true;
	datetimePickerProps.rootPortal.default = true;
	dialogProps.rootPortal.default = true;
	dorpMenuItemProps.rootPortal.default = true;
	keyboardProps.rootPortal.default = true;
	notifyProps.rootPortal.default = true;
	pickerProps.rootPortal.default = true;
	popupProps.rootPortal.default = true;
	selectPickerProps.rootPortal.default = true;
	setNotifyDefaultOptions({ rootPortal: true });
	// #endif
}

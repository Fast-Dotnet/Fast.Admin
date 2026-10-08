import { inputProps } from "@wot-ui/ui/components/wd-input/types";
import { textareaProps } from "@wot-ui/ui/components/wd-textarea/types";

/** 设置 Wot UI 输入组件的默认字数统计选项。 */
export function loadWotUi(): void {
	// 默认显示字数统计
	inputProps.showWordLimit.default = true;
	// 默认显示字数统计
	textareaProps.showWordLimit.default = true;
}

import type { CheckAppUpdatePackageOutput } from "./CheckAppUpdatePackageOutput";

/**
 * 检查应用更新输出
 */
export interface CheckAppUpdateOutput {
	/**
	 * 是否存在可用更新
	 */
	hasUpdate?: boolean;
	/**
	 * 
	 */
	package?: CheckAppUpdatePackageOutput;
}


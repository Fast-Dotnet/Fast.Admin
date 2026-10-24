import { axiosUtil } from "@fast-china/axios";
import type { CheckAppUpdateInput } from "./models/CheckAppUpdateInput";
import type { CheckAppUpdateOutput } from "./models/CheckAppUpdateOutput";
import type { LaunchOutput } from "./models/LaunchOutput";

/**
 * AppApi
 */
export const appApi = {
	/**
	 * Launch
	 */
	launch(): Promise<LaunchOutput> {
		return axiosUtil.request<LaunchOutput>({
			url: "/launch",
			method: "post",
			requestType: "auth",
		});
	},
	/**
	 * 检查应用更新
	 */
	checkAppUpdate(data: CheckAppUpdateInput): Promise<CheckAppUpdateOutput> {
		return axiosUtil.request<CheckAppUpdateOutput>({
			url: "/app/checkAppUpdate",
			method: "post",
			data,
			requestType: "query",
		});
	},
};

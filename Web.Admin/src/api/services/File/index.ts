import { axiosUtil } from "@fast-china/axios";
import type { AxiosProgressEvent, AxiosResponse } from "axios";
import type { PagedResult } from "fast-element-plus";
import type { DownloadFileInput } from "./models/DownloadFileInput";
import type { QueryFilePagedInput } from "./models/QueryFilePagedInput";
import type { QueryFilePagedOutput } from "./models/QueryFilePagedOutput";

/**
 * 文件服务Api
 */
export const fileApi = {
	/**
	 * 获取文件分页列表
	 */
	queryFilePaged(data: QueryFilePagedInput): Promise<PagedResult<QueryFilePagedOutput>> {
		return axiosUtil.request<PagedResult<QueryFilePagedOutput>>({
			url: "/file/queryFilePaged",
			method: "post",
			data,
			requestType: "query",
		});
	},
	/**
	 * 下载文件
	 */
	download(data: DownloadFileInput, autoDownloadFile = true): Promise<AxiosResponse<Blob>> {
		return axiosUtil.request<AxiosResponse<Blob>>({
			url: "/file/download",
			method: "post",
			data,
			responseType: "blob",
			autoDownloadFile,
			requestType: "download",
		});
	},
	/**
	 * 上传Logo
	 */
	uploadLogo(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadLogo",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传头像
	 */
	uploadAvatar(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadAvatar",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传证件照
	 */
	uploadIdPhoto(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadIdPhoto",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传安装包
	 */
	uploadPackage(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadPackage",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传富文本
	 */
	uploadEditor(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadEditor",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传文件
	 */
	uploadFile(data: FormData, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadFile",
			method: "post",
			data,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
};

import { axiosUtil } from "@fast-china/axios";
import type { AxiosProgressEvent, AxiosResponse } from "axios";
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
	download(data: DownloadFileInput, autoDownloadFile = true): Promise<AxiosResponse<Blob | ArrayBuffer | string>> {
		return axiosUtil.request<AxiosResponse<Blob | ArrayBuffer | string>>({
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
	uploadLogo(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadLogo",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传头像
	 */
	uploadAvatar(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadAvatar",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传证件照
	 */
	uploadIdPhoto(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadIdPhoto",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传安装包
	 */
	uploadPackage(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadPackage",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传富文本
	 */
	uploadEditor(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadEditor",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
	/**
	 * 上传文件
	 */
	uploadFile(filePath: string, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void): Promise<string> {
		return axiosUtil.request<string>({
			url: "/file/uploadFile",
			method: "upload",
			name: "file",
			filePath,
			onUploadProgress,
			cancelDuplicateRequest: false,
			requestType: "upload",
		});
	},
};

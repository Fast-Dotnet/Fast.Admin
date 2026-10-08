import { createPinia } from "pinia";
import { decodeSecureBase64, encodeSecureBase64 } from "@fast-china/utils";
import { createPersistedState } from "pinia-plugin-persistedstate";
import type { App } from "vue";

const storageCrypto = import.meta.env.VITE_STORAGE_CRYPTO === "true";
const storagePrefix = "pinia__";

/** 应用共用的 Pinia 实例 */
export const pinia = createPinia();

/** 注册存储适配和持久化插件，并将 Pinia 安装到应用。 */
export const loadPinia = (app: App): void => {
	pinia.use(
		createPersistedState({
			storage: {
				getItem: (key: string) => {
					const result = uni.getStorageSync<string>(`${storagePrefix}${key}`);
					if (!result) return null;
					return storageCrypto ? decodeSecureBase64(result) : result;
				},
				setItem: (key: string, value: string) => {
					uni.setStorageSync(`${storagePrefix}${key}`, storageCrypto ? encodeSecureBase64(value) : value);
				},
			},
			// 当设置为 true 时，持久化/恢复 Store 时可能发生的任何错误都将使用 console.error 输出。
			debug: true,
		})
	);
	app.use(pinia);
};

export * from "./app";
export * from "./config";
export * from "./userInfo";

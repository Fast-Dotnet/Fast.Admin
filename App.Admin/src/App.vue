<script setup lang="ts">
import { onLaunch } from "@dcloudio/uni-app";
import { logger } from "@fast-china/utils";
import { CommonRoute } from "@/common";
import { useApp, useConfig } from "@/stores";

onLaunch((options) => {
	const appStore = useApp();
	const configStore = useConfig();

	logger.debug("App", `成功加载【${appStore.appBaseInfo.appName}】`, new Date());

	// #ifdef APP-PLUS
	// App端需要调用setUIStyle，否则无法使用深色模式
	plus.nativeUI.setUIStyle("auto");
	// #endif

	// 设置应用名称
	appStore.setAppName(appStore.appBaseInfo.appName);

	if (`/${options.path}` !== CommonRoute.Launcher) {
		// 处理未经过 Launcher 页面导致 axios 配置不存在的问题
		appStore.setFastAxios();
		// 处理未经过 Launcher 页面导致字典不存在的问题
		appStore.setDictionary();
	}

	// 初始化主题
	configStore.initTheme();
});
</script>

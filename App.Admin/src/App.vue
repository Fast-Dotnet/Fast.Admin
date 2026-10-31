<script setup lang="ts">
import { onLaunch } from "@dcloudio/uni-app";
import { CommonRoute } from "@/common";
import { useUpdate } from "@/hooks";
import { usePrivacy } from "@/hooks/use-privacy";
import { loadAppRuntime } from "@/plugins";
import { useApp } from "@/stores";

onLaunch((options) => {
	// #ifdef MP
	// 冷启动时尽早监听更新，后台下载完成后会自动提示重启。
	useUpdate.checkUpdate();
	// #endif

	// 首次启动由启动页取得同意，再继续初始化。
	if (!usePrivacy.hasAgreed()) return;
	loadAppRuntime();

	if (`/${options.path}` !== CommonRoute.Launcher) {
		const appStore = useApp();
		// 处理未经过 Launcher 页面导致 axios 配置不存在的问题
		appStore.setFastAxios();
		// 处理未经过 Launcher 页面导致字典不存在的问题
		appStore.setDictionary();
	}
});
</script>

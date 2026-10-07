import { URL, fileURLToPath } from "node:url";
import vue from "@vitejs/plugin-vue";
import vueJsx from "@vitejs/plugin-vue-jsx";
import { defineConfig, loadEnv } from "vite";
import { buildInfo, cdnImport, componentRegistry, envGuard, routerMeta, svgIcons } from "fast-vite-plugins";
import visualizer from "rollup-plugin-visualizer";
import viteCompression from "vite-plugin-compression";
import vueDevTools from "vite-plugin-vue-devtools";
import type { UserConfig } from "vite";

const viteRoot = fileURLToPath(new URL(".", import.meta.url));

export default defineConfig(({ command, mode }): UserConfig => {
	const viteEnv = loadEnv(mode, viteRoot, "") as ImportMetaEnv;
	const isBuild = command === "build";
	const isDevelopment = mode === "development";
	const serverPort = Number(viteEnv.VITE_PORT);

	return {
		/* 固定以配置文件所在目录作为项目根目录，避免因启动位置不同而影响路径解析。 */
		root: viteRoot,
		/* 应用部署基础路径，同时作为构建产物中资源引用的公共前缀。 */
		base: viteEnv.STATIC_ASSET_BASE_URL,
		resolve: {
			/* 配置源码目录别名。 */
			alias: {
				"@": fileURLToPath(new URL("./src", import.meta.url)),
			},
		},
		server: {
			/* 监听所有网络地址，允许通过局域网 IP 访问开发服务器。 */
			host: true,
			/* 开发服务器端口。 */
			port: Number.isInteger(serverPort) && serverPort > 0 ? serverPort : 2001,
			/* 启动开发服务器后不自动打开浏览器。 */
			open: false,
			/* 允许开发服务器响应跨域请求。 */
			cors: true,
			/* 端口被占用时自动尝试下一个可用端口。 */
			strictPort: false,
			/* 将本地接口请求代理到后端服务。 */
			proxy: {
				"/api": {
					/* 后端接口代理地址。 */
					target: viteEnv.API_PROXY_URL,
					/* 将请求 Host 修改为目标服务器地址。 */
					changeOrigin: true,
					/* 支持 WebSocket 请求代理。 */
					ws: true,
					/* 转发请求前移除 /api 路径前缀。 */
					rewrite: (path) => path.replace(/^\/api/, ""),
				},
			},
		},
		build: {
			/* 使用 Vite 8 内置的 Oxc 压缩器，无需额外安装 Terser。 */
			minify: "oxc",
			/* gzip 文件由压缩插件生成，无需额外计算构建产物的压缩体积。 */
			reportCompressedSize: false,
			/* 将所有 CSS 合并为单个文件，不按异步模块拆分。 */
			cssCodeSplit: false,
			/* 不生成 Source Map，减少构建体积并避免暴露源码。 */
			sourcemap: false,
			/* 构建前清空输出目录。 */
			emptyOutDir: true,
			/* 构建产物输出目录。 */
			outDir: viteEnv.BUILD_OUT_DIR,
			/* 禁止将小型静态资源内联为 Base64，所有资源均单独输出。 */
			assetsInlineLimit: 0,
			/* 配置 Vite 8 使用的 Rolldown 构建行为。 */
			rolldownOptions: {
				output: {
					minify: {
						compress: {
							/* 非开发环境移除 debugger 语句。 */
							dropDebugger: true,
						},
						/* 控制是否缩短变量和属性相关标识符。 */
						mangle: true,
					},
					/* 异步代码块输出到 assets/js，并携带内容哈希。 */
					chunkFileNames: "assets/js/[name]-[hash].js",
					/* 入口文件输出到 assets/js，并携带内容哈希。 */
					entryFileNames: "assets/js/[name]-[hash].js",
					/* 样式、字体和图片等资源按扩展名分类输出。 */
					assetFileNames: "assets/[ext]/[name]-[hash][extname]",
					codeSplitting: {
						groups: [
							/*
							 * 按第三方包名称拆分 node_modules 依赖。
							 * 小于 20 KiB 的模块由 Rolldown 自动合并，避免生成过多零碎文件。
							 */
							{
								/* 仅匹配 node_modules 中的第三方模块。 */
								test: /[/\\]node_modules[/\\]/,
								/* 使用较低优先级，为后续自定义业务分组预留覆盖空间。 */
								priority: -1,
								/* 仅将达到 20 KiB 的依赖拆分为独立代码块。 */
								minSize: 20 * 1024,
								/* 不递归合并当前模块的依赖，避免不同包被强制归入同一代码块。 */
								includeDependenciesRecursively: false,
								name: (id): string => {
									/* 统一 Windows 和 Unix 路径分隔符，简化后续路径解析。 */
									const normalizedId = id.replace(/\\/g, "/");
									const nodeModulesPath = "/node_modules/";

									/*
									 * pnpm 路径可能包含多层 node_modules。
									 * 使用最后一次出现的位置，获取最终依赖的真实包名。
									 */
									const nodeModulesIndex = normalizedId.lastIndexOf(nodeModulesPath);
									if (nodeModulesIndex === -1) return null;

									/* 截取包路径，例如 vue/dist 或 @vue/runtime-core/dist。 */
									const packagePath = normalizedId.slice(nodeModulesIndex + nodeModulesPath.length);
									const [packageName, scopedPackageName] = packagePath.split("/");
									if (!packageName) return null;

									/* 将 @vue/runtime-core 等作用域包转换为 _vendor_vue_runtime-core。 */
									if (packageName.startsWith("@")) {
										if (!scopedPackageName) return null;
										return `_vendor_${packageName.slice(1)}_${scopedPackageName}`;
									}

									/* 将 vue、axios、pinia 等普通包转换为对应的依赖分块名称。 */
									return `_vendor_${packageName}`;
								},
							},
						],
					},
				},
			},
		},
		plugins: [
			/* 启动和构建前校验关键环境变量，避免配置拼写错误延迟到运行期才暴露。 */
			envGuard({
				schema: {
					VITE_PORT: { pattern: /^\d+$/u, description: "开发服务器端口" },
					VITE_PUBLIC_PATH: { pattern: /^(?:\/|https?:\/\/)/u, description: "前端路由基础路径" },
					STATIC_ASSET_BASE_URL: { pattern: /^(?:\/|https?:\/\/)/u, description: "静态资源基础地址" },
					BUILD_OUT_DIR: { pattern: /^[^<>:"|?*]+$/u, description: "构建输出目录" },
					VITE_API_BASE_URL: { pattern: /^(?:\/|https?:\/\/)/u, description: "接口基础地址" },
					VITE_APP_VERSION: true,
					VITE_STORAGE_CRYPTO: { values: ["true", "false"] },
					VITE_ENABLE_MOBILE: { values: ["true", "false"] },
					API_PROXY_URL: { required: isDevelopment, pattern: /^https?:\/\//u, description: "开发接口代理地址" },
				},
			}),
			/* 编译 Vue 3 单文件组件。 */
			vue(),
			/* 编译 Vue JSX 和 TSX，并应用 Vue 专用语法转换。 */
			vueJsx(),
			/* 集成 Vue DevTools 开发调试工具。 */
			isDevelopment && vueDevTools(),
			/* 扫描本地 SVG 文件并生成图标组件及相关类型声明。 */
			svgIcons({
				dir: "src/assets/icons",
				output: "src/icons/index.ts",
			}),
			/* 自动导入并注册项目组件。 */
			componentRegistry({
				dirs: "src/components",
				output: "src/components/index.ts",
				dts: "types/components.d.ts",
				/* 富文本编辑器体积较大且当前未全局使用，需要时由页面按需导入。 */
				include: ({ relativePath }) => relativePath !== "Editor/index.vue",
			}),
			/* 根据项目约定生成或处理路由路径。 */
			routerMeta({
				dir: "src/views",
				output: "src/router/routes.generated.json",
			}),
			/* 生成构建信息，供客户端检测线上版本更新。 */
			buildInfo({
				version: viteEnv.VITE_APP_VERSION,
			}),
			cdnImport({
				/* 开发环境不使用 CDN，避免影响本地调试。 */
				dev: false,
				urlTemplate: viteEnv.CDN_URL,
				modules: [
					{
						name: "vue",
						global: "Vue",
						js: isBuild ? "dist/vue.runtime.global.prod.js" : "dist/vue.runtime.global.js",
					},
					{
						name: "vue-router",
						global: "VueRouter",
						js: isBuild ? "dist/vue-router.global.prod.js" : "dist/vue-router.global.js",
					},
					{
						name: "vue-json-pretty",
						global: "VueJsonPretty.default",
						js: "lib/vue-json-pretty.js",
						css: "lib/styles.css",
					},
					{
						name: "dayjs",
						global: "dayjs",
						js: ["dayjs.min.js", "locale/zh-cn.js"],
					},
					{
						name: "@element-plus/icons-vue",
						global: "ElementPlusIconsVue",
						js: "dist/index.iife.min.js",
					},
					{
						name: "element-plus",
						global: "ElementPlus",
						js: ["dist/index.full.min.js", "dist/locale/zh-cn.min.js"],
						css: ["dist/index.css", "theme-chalk/dark/css-vars.css"],
					},
					{
						name: "@fast-china/utils",
						global: "FastUtils",
						js: "dist/index.global.min.js",
					},
					{
						name: "axios",
						global: "axios",
						js: "dist/axios.min.js",
					},
					{
						name: "@fast-china/axios",
						global: "FastAxios",
						js: "dist/index.global.min.js",
					},
					{
						name: "pinia",
						global: "Pinia",
						js: "dist/pinia.iife.prod.js",
					},
					{
						name: "pinia-plugin-persistedstate",
						global: "piniaPluginPersistedstate",
						js: "dist/index.iife.js",
					},
					{
						name: "@microsoft/signalr",
						global: "signalR",
						js: "dist/browser/signalr.min.js",
					},
					{
						name: "nprogress",
						global: "NProgress",
						js: "nprogress.js",
						css: "nprogress.css",
					},
					{
						name: "echarts",
						global: "echarts",
						js: "dist/echarts.min.js",
					},
					{
						name: "pinyin-pro",
						global: "pinyinPro",
						js: "dist/index.js",
					},
					{
						name: "@fast-element-plus/icons-vue",
						global: "FastElementPlusIconsVue",
						js: "dist/index.global.min.js",
					},
					{
						name: "fast-element-plus",
						global: "FastElementPlus",
						js: "dist/index.global.min.js",
						css: "dist/index.css",
					},
				],
			}),
			/* 仅在构建阶段为超过 10 KiB 的静态资源生成 gzip 文件。 */
			isBuild &&
				viteCompression({
					/* 在构建日志中输出压缩结果及压缩率。 */
					verbose: true,
					/* 启用静态资源压缩。 */
					disable: false,
					/* 保留未经压缩的原始文件。 */
					deleteOriginFile: false,
					/* 仅压缩大于 10 KiB 的文件。 */
					threshold: 10 * 1024,
					/* 使用 gzip 压缩算法。 */
					algorithm: "gzip",
					/* 压缩文件扩展名。 */
					ext: ".gz",
					/* 仅压缩 CSS、JavaScript、JSON、SVG 等文本静态资源，不处理 HTML 文件。 */
					filter: /\.(?:css|js|json|mjs|svg|webmanifest|xml)$/i,
				}),
			/* 仅在构建阶段生成依赖体积分析报告，并自动打开报告页面。 */
			isBuild &&
				visualizer({
					filename: `${viteEnv.BUILD_OUT_DIR}/analysis.html`,
					open: false,
				}),
		],
	};
});

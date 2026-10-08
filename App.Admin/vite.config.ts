import { readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import UniPlugin from "@dcloudio/vite-plugin-uni";
import { defineConfig } from "vite";
import UniHelperComponents from "@uni-helper/vite-plugin-uni-components";
import UniHelperLayouts from "@uni-helper/vite-plugin-uni-layouts";
import UniHelperPages from "@uni-helper/vite-plugin-uni-pages";
import { FastResolver } from "./resolver.fast";
import { WotResolver } from "./resolver.wot-ui";
import { ZPagingResolver } from "./resolver.z-paging";

// 兼容插件的模块导出形式，优先使用 default 导出，否则使用模块本身。
const Uni = (UniPlugin as typeof UniPlugin & { default?: typeof UniPlugin }).default ?? UniPlugin;
const viteRoot = fileURLToPath(new URL(".", import.meta.url));
const isDevelopment = process.env.NODE_ENV === "development";

export default defineConfig({
	/* 固定以配置文件所在目录作为项目根目录，避免因启动位置不同而影响路径解析。 */
	root: viteRoot,
	resolve: {
		/* 配置源码目录别名。 */
		alias: {
			"@": fileURLToPath(new URL("./src", import.meta.url)),
			static: fileURLToPath(new URL("./src/static", import.meta.url)),
		},
	},
	server: {
		/* 监听所有网络地址，允许通过局域网 IP 访问开发服务器。 */
		host: true,
		/* 开发服务器端口。 */
		port: 2010,
		/* 启动开发服务器后不自动打开浏览器。 */
		open: false,
		/* 允许开发服务器响应跨域请求。 */
		cors: true,
		/* 端口被占用时自动尝试下一个可用端口。 */
		strictPort: false,
	},
	css: {
		preprocessorOptions: {
			scss: {
				/* 使用 Sass Modern Compiler API。 */
				api: "modern-compiler",
				/* 消除旧版 JavaScript API 的弃用警告。 */
				silenceDeprecations: ["legacy-js-api"],
			},
		},
	},
	optimizeDeps: {
		/* Wot UI 国际化需要排除 @wot-ui/ui 的依赖预构建。 */
		exclude: ["@wot-ui/ui"],
	},
	build: {
		/* 无需额外计算构建产物的 gzip 压缩体积。 */
		reportCompressedSize: false,
		/* 不生成 Source Map，减少构建体积并避免暴露源码。 */
		sourcemap: false,
		/* 开发环境不压缩，发行环境使用 Terser。 */
		minify: isDevelopment ? false : "terser",
		terserOptions: {
			compress: {
				/* 移除 console.*。 */
				drop_console: true,
				/* 移除 debugger。 */
				drop_debugger: true,
			},
			format: {
				/* 移除注释。 */
				comments: false,
			},
			/** 混淆变量名。 */
			mangle: true,
		},
	},
	/** Vite 插件 */
	plugins: [
		/* 基于文件系统自动生成页面路由。 */
		UniHelperPages({
			outDir: "src",
			dts: "types/pages.d.ts",
			homePage: "pages/launcher/index",
			dir: "src/pages",
			exclude: ["**/components/**/*.*"],
			/* 自动扫描分包目录。 */
			subPackages: readdirSync(fileURLToPath(new URL("./src", import.meta.url)), {
				withFileTypes: true,
			})
				.filter((entry) => entry.isDirectory() && entry.name.startsWith("pages_"))
				.map((entry) => `src/${entry.name}`)
				.sort(),
		}),
		/* 页面 Layout。 */
		UniHelperLayouts(),
		/* 组件自动导入。 */
		UniHelperComponents({
			dirs: [],
			resolvers: [WotResolver(), ZPagingResolver(), FastResolver()],
			types: [
				{ names: ["ZPaging"], from: "z-paging/types/comps/z-paging" },
				{ names: ["ZPagingSwiper"], from: "z-paging/types/comps/z-paging-swiper" },
				{ names: ["ZPagingSwiperItem"], from: "z-paging/types/comps/z-paging-swiper-item" },
				{ names: ["ZPagingEmptyView"], from: "z-paging/types/comps/z-paging-empty-view" },
				{ names: ["ZPagingCell"], from: "z-paging/types/comps/z-paging-cell" },
			],
			dts: "types/components.d.ts",
		}),
		/* uni-app Vite 插件必须放在相关转换插件之后。 */
		Uni(),
	],
});

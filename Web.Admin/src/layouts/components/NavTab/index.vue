<template>
	<div
		:class="['nav-tab', 'nav-tab--' + configStore.layout.navTabStyle.toLowerCase()]"
		:style="{ '--height': configStore.layout.navTab ? addCssUnit(configStore.layout.navTabHeight) : 0 }"
	>
		<el-icon v-show="state.isShowArrow" class="icon-arrow left fa__hover__twinkle" title="向左滚动" @click="handleScrollTo('left')">
			<ArrowLeft />
		</el-icon>
		<el-scrollbar ref="scrollbarRef" @wheel.passive="handleWheelScroll" @scroll="handleScroll">
			<transition-group name="el-fade-in" tag="div" class="nav-tab__warp">
				<div
					v-for="(tag, idx) in navTabsStore.navTabs"
					:key="tag.path"
					ref="tagRefs"
					:data-path="tag.path"
					:class="['nav-tab__item', { 'is-active': route.path === tag.path }]"
					:title="tag.meta?.title"
					@click.prevent="(event) => handleTabClick(event, tag)"
					@mousedown.middle="(event) => handleCloseClick(event, tag)"
					@contextmenu.prevent="(event) => handleContextmenuClick(event, tag, idx)"
				>
					{{ tag.meta?.title }}
					<el-icon
						v-if="!tag.meta?.affix && route.path === tag.path"
						class="icon-close"
						title="关闭"
						@click.prevent.stop="(event: MouseEvent) => handleCloseClick(event, tag)"
					>
						<Close />
					</el-icon>
				</div>
			</transition-group>
		</el-scrollbar>
		<el-icon v-show="state.isShowArrow" class="icon-arrow right fa__hover__twinkle" title="向右滚动" @click="handleScrollTo('right')">
			<ArrowRight />
		</el-icon>
		<el-icon class="icon-arrow right fa__hover__twinkle" title="刷新" @click="navTabsStore.refreshTab(navTabsStore.activeTab)">
			<Refresh />
		</el-icon>
		<el-dropdown
			size="small"
			trigger="click"
			title="操作"
			@command="handleCommand"
			@visible-change="(val: boolean) => val && handleContextMenuList(navTabsStore.activeTab, navTabsStore.activeIndex)"
		>
			<el-icon class="icon-arrow right fa__hover__twinkle">
				<ArrowDown />
			</el-icon>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item
						v-for="(item, idx) in contextMenuList"
						:key="idx"
						:command="item.command"
						:icon="item.icon"
						:disabled="item.disabled"
						:divided="item.divided"
					>
						{{ item.label }}
					</el-dropdown-item>
					<el-dropdown-item
						command=""
						divided
						:icon="navTabsStore.contentLarge ? FullScreenExit : FullScreen"
						@click="navTabsStore.setContentLarge(!navTabsStore.contentLarge)"
					>
						{{ navTabsStore.contentLarge ? "内容区复原" : "内容区放大" }}
					</el-dropdown-item>
					<el-dropdown-item command="" :icon="Monitor" @click="navTabsStore.setContentFull(!navTabsStore.contentFull)">
						内容区全屏
					</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>
		<el-dropdown
			ref="dropdownRef"
			:virtual-ref="dropdownTriggerRef"
			size="small"
			:show-arrow="false"
			virtual-triggering
			trigger="contextmenu"
			placement="bottom-start"
			:popper-options="{
				modifiers: [{ name: 'offset', options: { offset: [0, 0] } }],
			}"
			@command="handleCommand"
		>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item
						v-for="(item, idx) in contextMenuList"
						:key="idx"
						:command="item.command"
						:icon="item.icon"
						:disabled="item.disabled"
						:divided="item.divided"
					>
						{{ item.label }}
					</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>
		<div
			v-if="navTabsStore.contentFull"
			class="screen-full__close fa__hover__twinkle"
			title="关闭内容区全屏"
			@click="navTabsStore.setContentFull(!navTabsStore.contentFull)"
		>
			<el-icon>
				<Exit />
			</el-icon>
		</div>
	</div>
</template>

<script setup lang="ts">
import { markRaw, nextTick, onMounted, reactive, ref, useTemplateRef } from "vue";
import { onBeforeRouteUpdate, useRoute, useRouter } from "vue-router";
import { ArrowDown, ArrowLeft, ArrowRight, Close, DArrowLeft, DArrowRight, Minus, Monitor, PriceTag, Refresh } from "@element-plus/icons-vue";
import { Exit, FullScreen, FullScreenExit } from "@fast-element-plus/icons-vue";
import { addCssUnit, useResizeObserver, withDefineType } from "@fast-china/utils";
import { routerUtil } from "@/router";
import { useConfig, useNavTabs } from "@/stores";
import type { DropdownInstance, ElScrollbar } from "element-plus";
import type { RouteLocationNormalized } from "vue-router";
import type { INavTab } from "@/stores";

defineOptions({
	name: "NavTab",
});

const route = useRoute();
const router = useRouter();
const configStore = useConfig();
const navTabsStore = useNavTabs();

const tagRefs = useTemplateRef<HTMLDivElement[]>("tagRefs");
const scrollbarRef = useTemplateRef<InstanceType<typeof ElScrollbar>>("scrollbarRef");
const dropdownRef = useTemplateRef<DropdownInstance>("dropdownRef");

const contextMenuList = reactive([
	{
		command: "Refresh",
		label: "重新加载",
		disabled: false,
		divided: false,
		icon: markRaw(Refresh),
		data: withDefineType<INavTab>(),
	},
	{
		command: "Close",
		label: "关闭标签页",
		disabled: false,
		divided: false,
		icon: markRaw(Close),
		data: withDefineType<INavTab>(),
	},
	{
		command: "CloseLeft",
		label: "关闭左侧标签页",
		disabled: false,
		divided: true,
		icon: markRaw(DArrowLeft),
		data: withDefineType<INavTab>(),
	},
	{
		command: "CloseRight",
		label: "关闭右侧标签页",
		disabled: false,
		divided: false,
		icon: markRaw(DArrowRight),
		data: withDefineType<INavTab>(),
	},
	{
		command: "CloseOther",
		label: "关闭其他标签页",
		disabled: false,
		divided: true,
		icon: markRaw(PriceTag),
		data: withDefineType<INavTab>(),
	},
	{
		command: "CloseAll",
		label: "关闭全部标签页",
		disabled: false,
		divided: false,
		icon: markRaw(Minus),
		data: withDefineType<INavTab>(),
	},
]);

const state = reactive({
	/** 显示箭头 */
	isShowArrow: false,
	/** 当前滚动条距离左边的距离 */
	currentScrollLeft: 0,
	/** 每次滚动的距离 */
	translateDistance: 100,
	dropdownPosition: {
		top: 0,
		left: 0,
		bottom: 0,
		right: 0,
	} as DOMRect,
});

const dropdownTriggerRef = ref({
	getBoundingClientRect: () => state.dropdownPosition,
});

const handleCommand = (command: string) => {
	if (command) {
		const { data } = contextMenuList.find((f) => f.command === command);
		switch (command) {
			case "Refresh":
				navTabsStore.refreshTab(data);
				break;
			case "Close":
				navTabsStore.closeTab(data);
				break;
			case "CloseLeft":
				navTabsStore.closeTabs(data, "left");
				break;
			case "CloseRight":
				navTabsStore.closeTabs(data, "right");
				break;
			case "CloseOther":
				navTabsStore.closeTabs(data);
				break;
			case "CloseAll":
				navTabsStore.closeTabs();
				break;
		}
	}
};

/**
 * 获取可能需要的宽度
 */
const getWidth = () => {
	const wrapRef = scrollbarRef.value?.wrapRef;
	if (!wrapRef) {
		return { scrollWidth: 0, clientWidth: 0, lastDistance: 0 };
	}

	/** 可滚动内容的长度 */
	const scrollWidth = wrapRef.scrollWidth;
	/** 滚动可视区宽度 */
	const clientWidth = wrapRef.clientWidth;
	/** 最后剩余可滚动的宽度 */
	const lastDistance = scrollWidth - clientWidth - state.currentScrollLeft;

	return { scrollWidth, clientWidth, lastDistance };
};

/**
 * 左右滚动
 */
const handleScrollTo = (direction: "left" | "right", distance: number = state.translateDistance) => {
	const { scrollWidth, clientWidth } = getWidth();
	// 没有横向滚动条，直接结束
	if (clientWidth >= scrollWidth || !scrollbarRef.value) return;
	const currentScrollLeft = state.currentScrollLeft;
	let scrollLeft: number;
	if (direction === "left") {
		scrollLeft = Math.max(0, currentScrollLeft - distance);
	} else {
		scrollLeft = Math.min(currentScrollLeft + distance);
	}
	scrollbarRef.value.setScrollLeft(scrollLeft);
};

/**
 * 鼠标滚轮滚动时触发
 */
const handleWheelScroll = (event: WheelEvent) => {
	if (/^-/.test(event.deltaY.toString())) {
		handleScrollTo("left");
	} else {
		handleScrollTo("right");
	}
};

/**
 * 滚动时触发
 */
const handleScroll = ({ scrollLeft }: { scrollLeft: number }) => {
	state.currentScrollLeft = scrollLeft;
};

/**
 * 移动到目标位置
 */
const handleMoveTo = (to?: RouteLocationNormalized) => {
	nextTick(() => {
		const curTo = to ?? route;
		const findTagRef = tagRefs.value?.find((element) => element?.dataset.path === curTo.path);
		if (findTagRef) {
			const { offsetWidth, offsetLeft } = findTagRef;
			const { clientWidth } = getWidth();
			// 当前 tag 在可视区域左边时
			if (offsetLeft < state.currentScrollLeft) {
				const distance = state.currentScrollLeft - offsetLeft;
				handleScrollTo("left", distance);
				return;
			}

			// 当前 tag 在可视区域右边时
			const width = clientWidth + state.currentScrollLeft - offsetWidth;
			if (offsetLeft > width) {
				const distance = offsetLeft - width;
				handleScrollTo("right", distance);
				return;
			}
		}
	});
};

const handleTabClick = (_event: MouseEvent, tag: INavTab) => {
	if (tag.path === route.path) return;
	// 左键
	routerUtil.routePushSafe(router, { path: tag.path, query: tag.query });
};

const handleContextMenuList = (tag: INavTab, index: number) => {
	// 禁用重新加载
	contextMenuList[0].disabled = tag.path !== route.path;
	// 禁用关闭
	contextMenuList[1].disabled = tag?.meta?.affix === true;
	// 禁用关闭左侧
	contextMenuList[2].disabled = index <= 0;
	// 禁用关闭右侧
	contextMenuList[3].disabled = index >= navTabsStore.navTabs.length - 1;
	// 禁用关闭其他和关闭全部
	contextMenuList[4].disabled = contextMenuList[5].disabled = navTabsStore.navTabs.length === 1 ? true : false;

	contextMenuList.forEach((item) => {
		item.data = tag;
	});
};

const handleContextmenuClick = (event: MouseEvent, tag: INavTab, index: number) => {
	handleContextMenuList(tag, index);

	const { clientX, clientY } = event;
	state.dropdownPosition = DOMRect.fromRect({
		x: clientX,
		y: clientY,
	});
	dropdownRef.value.handleOpen();
};

const handleCloseClick = (_event: MouseEvent, tag: INavTab) => {
	if (tag.meta?.affix === true) {
		return;
	}
	navTabsStore.closeTab(tag);
};

onMounted(() => {
	navTabsStore.initNavTabs(router);
	navTabsStore.addTab(router.currentRoute.value);
	navTabsStore.setActiveRoute(router.currentRoute.value);
	handleMoveTo(router.currentRoute.value);
	nextTick(() => {
		const wrapRef = scrollbarRef.value?.wrapRef;
		if (!wrapRef) return;

		useResizeObserver(wrapRef, () => {
			const { scrollWidth, clientWidth } = getWidth();
			state.isShowArrow = scrollWidth > clientWidth;
			handleMoveTo(router.currentRoute.value);
		});
	});
});

onBeforeRouteUpdate((to: RouteLocationNormalized) => {
	navTabsStore.addTab(routerUtil.pickByRoute(to));
	navTabsStore.setActiveRoute(to);
	handleMoveTo(to);
});
</script>

<style lang="scss" scoped>
@use "./index.scss";
</style>

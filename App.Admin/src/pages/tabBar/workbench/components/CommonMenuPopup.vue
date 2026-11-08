<template>
	<FaPopup
		ref="popupRef"
		custom-class="fa-popup__card"
		height="76%"
		position="bottom"
		title="编辑常用应用"
		closable
		show-confirm-button
		show-close-button
		confirm-button-text="保存"
		:disabled-confirm-button="!!state.dragState"
		:close-on-click-modal="false"
		:z-index="2001"
		@after-enter="measureLayout"
		@close="handleDragCancel"
		@confirm="handleConfirm"
	>
		<scroll-view class="menu-warp" scroll-y :scroll-top="state.scrollPosition" @scroll="handleScroll">
			<view class="menu-content">
				<view v-if="state.selectedMenus.length" class="selected-list">
					<view class="selected-list__header">
						<text class="section-title">常用应用</text>
						<text class="selected-list__tip">按住图片拖动排序</text>
					</view>
					<movable-area class="selected-grid" :style="{ height: `${Math.ceil(state.selectedMenus.length / 4) * state.rowHeight}px` }">
						<movable-view
							v-for="item in draggableMenus"
							:key="item.menu.menuCode"
							class="selected-menu"
							:class="{ 'is-dragging': state.dragState?.menuCode === item.menu.menuCode }"
							:style="{
								width: `${state.gridWidth / 4}px`,
								height: `${state.rowHeight}px`,
								visibility: state.gridWidth ? 'visible' : 'hidden',
							}"
							:x="item.x"
							:y="item.y"
							direction="all"
							disabled
							:animation="state.dragState?.menuCode !== item.menu.menuCode"
						>
							<!-- #ifdef H5 -->
							<!-- 原生节点保留 PointerEvent 字段，避免 uni-app 包装后丢失指针类型和坐标。 -->
							<div @pointerdown.stop="handleDragPointerStart($event, item.menu)">
								<!-- #endif -->
								<view
									class="selected-menu__icon"
									@touchstart.stop="handleDragStart($event, item.menu)"
									@touchmove.stop.prevent="handleDragTouchMove"
									@touchend.stop="handleDragEnd"
									@touchcancel.stop="handleDragCancel"
								>
									<FaImage
										v-if="item.menu.icon?.includes('/')"
										custom-class="selected-menu__picture"
										width="76rpx"
										height="76rpx"
										:src="item.menu.icon"
										mode="aspectFit"
										original
										:hide-image="false"
										:enable-preview="false"
									/>
									<FaIcon v-else custom-class="selected-menu__picture" :name="item.menu.icon || 'menu'" size="44rpx" />
									<button
										class="selected-menu__remove"
										hover-class="none"
										@pointerdown.stop
										@touchstart.stop
										@touchmove.stop
										@touchend.stop
										@click.stop="handleToggleMenu(item.menu)"
									>
										<view class="selected-menu__remove-icon"><wd-icon name="close" size="12px" /></view>
									</button>
								</view>
								<!-- #ifdef H5 -->
							</div>
							<!-- #endif -->
							<text class="selected-menu__title">{{ item.menu.menuName }}</text>
						</movable-view>
					</movable-area>
				</view>

				<view v-for="item in props.menus" :key="item.menuCode" class="menu-list">
					<view class="section-header">
						<view class="section-title">
							<view class="section-icon">
								<view class="section-marker" :style="{ backgroundColor: getMarkerColor(item) }" />
							</view>
							<text>{{ item.menuName }}</text>
						</view>
					</view>
					<MenuGrid :menus="item.children" editable :selected-menu-codes="selectedMenuCodes" @select="handleToggleMenu" />
				</view>
			</view>
		</scroll-view>
	</FaPopup>
</template>

<script setup lang="ts">
import { computed, getCurrentInstance, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from "vue";
import { definePropType, logger, withDefineType } from "@fast-china/utils";
import { employeeApi } from "@/api/services/Admin/employee";
import { useToast } from "@/hooks";
import MenuGrid from "@/pages/tabBar/workbench/components/MenuGrid.vue";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";
import type { FaPopupInstance } from "@/components";

defineOptions({
	name: "CommonMenuPopup",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 菜单列表 */
	menus: {
		type: definePropType<AuthMenuInfoDto[]>(Array),
		required: true,
	},
});

const emit = defineEmits<{
	ok: [];
}>();

/** 单次拖拽的排序快照与坐标，距离单位均为 px */
interface MenuDragState {
	menuCode: string;
	/** 拖拽前的菜单顺序，用于稳定渲染顺序和取消回滚 */
	menus: AuthMenuInfoDto[];
	// 起点在网格内的坐标，整个拖拽过程保持不变。
	originX: number;
	originY: number;
	// 起始触点与当前触点均使用视口坐标，两者差值用于计算位移。
	originTouchX: number;
	originTouchY: number;
	/** 起始滚动位置，用于补偿拖拽期间的内容滚动 */
	originScrollTop: number;
	pointerX: number;
	pointerY: number;
	// 当前网格坐标，直接传给 movable-view 的 x、y。
	x: number;
	y: number;
}

const { proxy } = getCurrentInstance();

const popupRef = ref<FaPopupInstance>();

const state = reactive({
	/** 选中的菜单集合 */
	selectedMenus: withDefineType<AuthMenuInfoDto[]>([]),
	gridWidth: 0,
	// 104rpx = 图标 84rpx + 间距 12rpx + 下内边距 8rpx；40px = 上内边距 8px + 两行 16px 标题。
	rowHeight: uni.upx2px(104) + 40,
	// scrollPosition 用于下发滚动指令，scrollTop 以 scroll 事件回传的实际位置为准。
	scrollPosition: 0,
	dragState: withDefineType<MenuDragState>(),
	scrollTop: 0,
	// top、bottom 是视口边界；gridBottom 是常用菜单网格底部在滚动内容中的位置。
	scrollBounds: { top: 0, bottom: 0, height: 0, contentHeight: 0, gridBottom: 0 },
});

/** 选中的菜单 Code 集合 */
const selectedMenuCodes = computed(() => state.selectedMenus.map((menu) => menu.menuCode));

let autoScrollTimer: ReturnType<typeof setInterval> | undefined;
// #ifdef H5
let mousePointerId: number | undefined;
// #endif

// 与四列网格保持一致，使用 px 坐标供 movable-view 定位。
const getMenuPosition = (index: number) => ({ x: (index % 4) * (state.gridWidth / 4), y: Math.floor(index / 4) * state.rowHeight });

// 拖拽期间固定节点遍历顺序，仅按草稿顺序更新目标坐标，避免列表重排打断当前触摸。
const draggableMenus = computed(() =>
	(state.dragState?.menus ?? state.selectedMenus).map((menu) => {
		const position = getMenuPosition(state.selectedMenus.findIndex((item) => item.menuCode === menu.menuCode));
		const drag = state.dragState;
		return { menu, ...(drag?.menuCode === menu.menuCode ? { x: drag.x, y: drag.y } : position) };
	})
);

// 使用 uni-app 节点查询测量网格与滚动区域，保持 H5 和小程序的坐标来源一致。
const measureLayout = () =>
	new Promise<void>((resolve) => {
		if (!proxy || !popupRef.value?.visible) {
			resolve();
			return;
		}
		const query = uni.createSelectorQuery().in(proxy);
		let gridBottom = 0;
		query.select(".selected-grid").boundingClientRect((rect) => {
			if (rect && !Array.isArray(rect)) {
				state.gridWidth = rect.width;
				gridBottom = rect.bottom;
			}
		});
		query.select(".menu-warp").boundingClientRect((rect) => {
			if (rect && !Array.isArray(rect)) state.scrollBounds = { ...state.scrollBounds, top: rect.top, bottom: rect.bottom, height: rect.height };
		});
		query.select(".menu-content").boundingClientRect((rect) => {
			if (rect && !Array.isArray(rect)) state.scrollBounds.contentHeight = rect.height;
		});
		state.rowHeight = uni.upx2px(104) + 40;
		query.exec(() => {
			// 将网格底部的视口坐标转为内容坐标，避免滚动后自动滚动上限随之偏移。
			state.scrollBounds.gridBottom = gridBottom - state.scrollBounds.top + state.scrollTop;
			resolve();
		});
	});

const updateDragPosition = () => {
	const drag = state.dragState;
	if (!drag || !state.gridWidth) return;
	const columnWidth = state.gridWidth / 4;
	// movable-view 仅负责定位与换位动画，禁用原生拖动，避免与图片触摸事件同时修改坐标。
	const maxY = (Math.ceil(state.selectedMenus.length / 4) - 1) * state.rowHeight;
	drag.x = Math.max(0, Math.min(state.gridWidth - columnWidth, drag.originX + drag.pointerX - drag.originTouchX));
	// 内容滚动时补上滚动差值，保证手指停在原处时，拖动项仍跟随触点。
	drag.y = Math.max(0, Math.min(maxY, drag.originY + drag.pointerY - drag.originTouchY + state.scrollTop - drag.originScrollTop));
	// 按最近的网格单元确定落点，未填满的末行最多落到最后一个菜单。
	const target = Math.min(state.selectedMenus.length - 1, Math.round(drag.y / state.rowHeight) * 4 + Math.round(drag.x / columnWidth));
	const index = state.selectedMenus.findIndex((menu) => menu.menuCode === drag.menuCode);
	if (index === target) return;
	// 移走当前项再插入目标位，其余菜单保持相对顺序并依次让位。
	const menus = [...state.selectedMenus];
	const [menu] = menus.splice(index, 1);
	menus.splice(target, 0, menu);
	state.selectedMenus = menus;
};

// 松手后保留草稿顺序，清空拖拽坐标使当前项回到网格位置，并停止边缘滚动。
const handleDragEnd = () => {
	if (autoScrollTimer !== undefined) clearInterval(autoScrollTimer);
	autoScrollTimer = undefined;
	state.dragState = undefined;
	// #ifdef H5
	window.removeEventListener("pointermove", handleDragPointerMove);
	window.removeEventListener("pointerup", handleDragPointerEnd);
	window.removeEventListener("pointercancel", handleDragPointerCancel);
	window.removeEventListener("blur", handleDragCancel);
	mousePointerId = undefined;
	// #endif
};

// 中断只回滚本次拖拽，之前已完成的排序仍保留在弹窗草稿中。
function handleDragCancel() {
	if (state.dragState) state.selectedMenus = state.dragState.menus;
	handleDragEnd();
}

const startDrag = (menu: AuthMenuInfoDto, clientX: number, clientY: number) => {
	if (state.dragState || !menu.menuCode) return;
	if (state.selectedMenus.length < 2 || !state.gridWidth) return;
	const position = getMenuPosition(state.selectedMenus.findIndex((item) => item.menuCode === menu.menuCode));
	// 保存起始位置和排序快照，移动过程中只修改弹窗草稿，保存时再提交接口。
	state.dragState = {
		menuCode: menu.menuCode,
		menus: [...state.selectedMenus],
		originX: position.x,
		originY: position.y,
		originTouchX: clientX,
		originTouchY: clientY,
		originScrollTop: state.scrollTop,
		pointerX: clientX,
		pointerY: clientY,
		...position,
	};
	state.scrollPosition = state.scrollTop;
	// 图片拦截触摸滚动，scroll-y 保持开启，确保边缘自动滚动仍能通过 scroll-top 生效。
	autoScrollTimer = setInterval(() => {
		const drag = state.dragState;
		const { scrollBounds, scrollTop } = state;
		if (!drag || !scrollBounds.height) return;
		// 触点进入上下边缘 36px 后，每 32ms 滚动 8px；手指静止时也能继续滚动。
		const step = drag.pointerY < scrollBounds.top + 36 ? -8 : drag.pointerY > scrollBounds.bottom - 36 ? 8 : 0;
		if (!step) return;
		// 最多滚到常用菜单网格底部，不继续滚向下方的候选菜单目录。
		const maxScroll = Math.max(0, Math.min(scrollBounds.contentHeight, scrollBounds.gridBottom) - scrollBounds.height);
		if ((step < 0 && scrollTop <= 0) || (step > 0 && scrollTop >= maxScroll)) return;
		state.scrollPosition = Math.max(0, Math.min(maxScroll, scrollTop + step));
	}, 32);
};

const handleDragStart = (event: UniHelper.TouchEvent, menu: AuthMenuInfoDto) => {
	const touch = event.touches[0];
	if (event.touches.length !== 1 || touch?.clientX === undefined || touch.clientY === undefined) return;
	startDrag(menu, touch.clientX, touch.clientY);
};

// #ifdef H5
function handleDragPointerMove(event: PointerEvent) {
	if (event.pointerId !== mousePointerId || !state.dragState) return;
	// 鼠标在窗口外松开后返回时，结束拖动，避免菜单继续跟随鼠标。
	if ((event.buttons & 1) === 0) {
		handleDragEnd();
		return;
	}
	event.preventDefault();
	state.dragState.pointerX = event.clientX;
	state.dragState.pointerY = event.clientY;
	updateDragPosition();
}

function handleDragPointerEnd(event: PointerEvent) {
	if (event.pointerId === mousePointerId) handleDragEnd();
}

function handleDragPointerCancel(event: PointerEvent) {
	if (event.pointerId === mousePointerId) handleDragCancel();
}
// #endif

const handleDragPointerStart = (event: PointerEvent, menu: AuthMenuInfoDto) => {
	// #ifdef H5
	// 触屏继续使用原有 touch 事件，避免同一次操作触发两套拖动。
	if (event.pointerType !== "mouse" || event.button !== 0 || state.dragState) return;
	startDrag(menu, event.clientX, event.clientY);
	if (!state.dragState) return;
	event.preventDefault();
	mousePointerId = event.pointerId;
	// 监听窗口以覆盖移出图标后的移动和松手，拖动结束时统一解除。
	window.addEventListener("pointermove", handleDragPointerMove);
	window.addEventListener("pointerup", handleDragPointerEnd);
	window.addEventListener("pointercancel", handleDragPointerCancel);
	window.addEventListener("blur", handleDragCancel);
	// #endif
};

const handleDragTouchMove = (event: UniHelper.TouchEvent) => {
	const touch = event.touches[0];
	if (event.touches.length !== 1 || touch?.clientX === undefined || touch.clientY === undefined) {
		// 多指或触点丢失时取消，避免切换跟踪手指导致菜单突然跳位。
		handleDragCancel();
		return;
	}
	if (!state.dragState) return;
	state.dragState.pointerX = touch.clientX;
	state.dragState.pointerY = touch.clientY;
	updateDragPosition();
};

const handleScroll = ({ detail }: UniHelper.ScrollViewOnScrollEvent) => {
	state.scrollTop = detail.scrollTop;
	state.scrollBounds.contentHeight = detail.scrollHeight;
	// 自动滚动可能没有新的 touchmove，仍需按实际滚动位置更新拖动项。
	updateDragPosition();
};

const handleResize = () => {
	// 窗口变化后旧列宽和坐标不再可靠，先回滚当前拖拽，再测量新布局。
	handleDragCancel();
	nextTick(measureLayout).catch((error: unknown) => logger.error("CommonMenuPopup", error));
};

// 常见菜单分组对应的颜色错开色系，避免相邻标题连续出现近似颜色。
const markerColors = [
	"#14b8a6",
	"#f43f5e",
	"#3b82f6",
	"#84cc16",
	"#f97316",
	"#a855f7",
	"#22c55e",
	"#64748b",
	"#d946ef",
	"#ef4444",
	"#0ea5e9",
	"#8b5cf6",
	"#b45309",
	"#10b981",
	"#eab308",
	"#ec4899",
	"#6366f1",
	"#0891b2",
	"#f59e0b",
	"#06b6d4",
];

const getMarkerColor = (menu: AuthMenuInfoDto) => {
	let colorIndex = 0;
	for (let index = 0; index < menu.menuCode.length; index++) colorIndex += menu.menuCode.charCodeAt(index);
	return markerColors[colorIndex % markerColors.length];
};

const handleToggleMenu = (menu: AuthMenuInfoDto) => {
	// 拖拽期间冻结增删，避免菜单集合与当前拖拽快照不一致。
	if (state.dragState || !menu.menuCode) return;
	state.selectedMenus = selectedMenuCodes.value.includes(menu.menuCode)
		? state.selectedMenus.filter((item) => item.menuCode !== menu.menuCode)
		: [...state.selectedMenus, menu];
};

const handleConfirm = () => {
	popupRef.value.close(async () => {
		await employeeApi.editCommonMenus({ commonMenuCodes: selectedMenuCodes.value });
		useToast.success("常用应用已保存");
		emit("ok");
	});
};

const open = (menus: AuthMenuInfoDto[]) => {
	popupRef.value?.open(async () => {
		handleDragCancel();
		state.selectedMenus = [...menus];
		// 先同步实际位置再归零，确保上次绑定值已为 0、但用户手动滚动过时，仍能触发回顶。
		state.scrollPosition = state.scrollTop;
		await nextTick();
		state.scrollPosition = 0;
		state.scrollTop = 0;
		await measureLayout();
	});
};

// 数量变化会改变网格高度和滚动上限，等待节点更新后重新测量。
watch(
	() => state.selectedMenus.length,
	async () => {
		await nextTick();
		await measureLayout();
	}
);

onMounted(() => uni.onWindowResize(handleResize));

onBeforeUnmount(() => {
	// 页面卸载后停止计时器并解除窗口监听，避免继续操作已销毁的节点。
	handleDragEnd();
	uni.offWindowResize(handleResize);
});

defineExpose({
	open,
});
</script>

<style lang="scss" scoped>
.menu-warp {
	flex: 1;
	min-height: 0;
	height: 0;
}

.menu-content {
	display: flex;
	flex-direction: column;
	gap: 16rpx;
	padding: 4rpx;
}

.selected-list,
.menu-list {
	padding: 20rpx 24rpx;
	background-color: var(--wot-card-bg, var(--wot-filled-oppo));
	border: var(--fa-border-panel);
	border-radius: var(--wot-radius-extra-large);
}

.selected-list {
	box-shadow: var(--fa-shadow-panel);
}

.selected-list__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12rpx;
	margin-bottom: 12rpx;
}

.selected-list__tip {
	font-size: var(--wot-typography-label-size-main);
	color: var(--wot-text-auxiliary);
}

.selected-grid {
	width: 100%;
	// 为拖动项的放大效果和阴影保留绘制空间。
	overflow: visible;
}

.selected-menu {
	display: flex;
	box-sizing: border-box;
	flex-direction: column;
	align-items: center;
	gap: 12rpx;
	padding: 8px 0 8rpx;
	// 避免移动浏览器长按时选中文字或弹出原生菜单，打断排序手势。
	user-select: none;
	-webkit-touch-callout: none;
	&.is-dragging {
		z-index: 2;
		.selected-menu__icon {
			transform: scale(1.08);
			box-shadow: var(--fa-shadow-ambient);
		}
	}
}

.selected-menu__icon {
	position: relative;
	display: flex;
	flex-shrink: 0;
	align-items: center;
	justify-content: center;
	width: 84rpx;
	height: 84rpx;
	border-radius: 24rpx;
	color: var(--wot-primary-6);
	background: var(--wot-primary-1);
	transition: transform var(--fa-motion-duration-fast) var(--fa-motion-easing-standard);
	/* #ifdef H5 */
	cursor: grab;
	/* #endif */
	:deep(.selected-menu__picture) {
		// 由外层图标容器统一接收触摸，避免图片内部节点拦截拖拽事件。
		pointer-events: none;
	}
}

/* #ifdef H5 */
.selected-menu.is-dragging .selected-menu__icon {
	cursor: grabbing;
}
/* #endif */

.selected-menu__title {
	min-width: 0;
	max-width: 100%;
	font-size: var(--wot-typography-label-size-main);
	line-height: 16px;
	display: -webkit-box;
	-webkit-line-clamp: 2;
	-webkit-box-orient: vertical;
	overflow: hidden;
	text-align: center;
}

.selected-menu__remove {
	position: absolute;
	top: -8px;
	right: -8px;
	display: flex;
	align-items: center;
	justify-content: center;
	// 图标保持 18px，按钮扩大至 32px，便于触摸点击。
	width: 32px;
	height: 32px;
	margin: 0;
	padding: 0;
	border: 0;
	background: transparent;
	/* #ifdef H5 */
	cursor: pointer;
	/* #endif */
	&::after {
		border: 0;
	}
}

.selected-menu__remove-icon {
	display: flex;
	align-items: center;
	justify-content: center;
	width: 18px;
	height: 18px;
	border-radius: 50%;
	color: var(--wot-text-white);
	background: var(--wot-danger-main);
}

/* #ifdef H5 */
@media (pointer: fine) {
	.selected-menu__remove {
		// 鼠标无需放大点击区，避免遮住图片中心；按钮图标保持原位置。
		top: -1px;
		right: -1px;
		width: 18px;
		height: 18px;
	}
}
/* #endif */

.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	gap: 16rpx;
	min-height: 28px;
	margin-bottom: 12rpx;
}

.section-title {
	display: flex;
	align-items: center;
	gap: 12rpx;
	font-size: var(--wot-typography-body-size-large);
	font-weight: var(--wot-font-weight-semibold);
}

.section-icon {
	display: flex;
	flex-shrink: 0;
	align-items: center;
	justify-content: center;
	width: 12px;
	height: 20px;
}

.section-marker {
	flex-shrink: 0;
	width: 8rpx;
	height: 28rpx;
	border-radius: 4rpx;
}
</style>

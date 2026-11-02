<template>
	<FaPopup ref="popupRef" height="80%" position="bottom" :close-on-click-modal="false" :z-index="2001">
		<view class="commonMenu-popup">
			<view class="popup__header">
				<wd-button type="primary" variant="text" @click="popupRef?.close()">取消</wd-button>
				<text class="section-title">编辑常用应用</text>
				<wd-button type="primary" variant="text" @click="handleConfirm">保存</wd-button>
			</view>
			<scroll-view class="menu-warp" scroll-y>
				<view v-if="selectedMenus.length" class="page-card selected-list">
					<view v-for="(item, idx) in selectedMenus" :key="item.menuCode" class="selected-row">
						<text class="selected-row__title">{{ item.menuName }}</text>
						<wd-button type="primary" variant="text" size="small" :disabled="idx === 0" @click="handleMoveMenu(idx, -1)">
							<wd-icon name="arrow-up" />
						</wd-button>
						<wd-button
							type="primary"
							variant="text"
							size="small"
							:disabled="idx === selectedMenus.length - 1"
							@click="handleMoveMenu(idx, 1)"
						>
							<wd-icon name="arrow-down" />
						</wd-button>
						<wd-button type="primary" variant="text" size="small" @click="handleToggleMenu(item)">
							<wd-icon name="close" />
						</wd-button>
					</view>
				</view>

				<view v-for="item in props.menus" :key="item.menuCode" class="page-card menu-list">
					<view class="section-header">
						<view class="section-title">
							<view class="section-marker" :style="{ backgroundColor: getMarkerColor(item) }" />
							<text>{{ item.menuName }}</text>
						</view>
					</view>
					<MenuGrid :menus="item.children" editable :selected-menu-codes="selectedMenuCodes" @select="handleToggleMenu" />
				</view>
			</scroll-view>
		</view>
	</FaPopup>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { definePropType } from "@fast-china/utils";
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

const popupRef = ref<FaPopupInstance>();

/** 选中的菜单集合 */
const selectedMenus = ref<AuthMenuInfoDto[]>([]);
/** 选中的菜单 Code 集合 */
const selectedMenuCodes = computed(() => selectedMenus.value.map((menu) => menu.menuCode));

const markerColors = [
	"#3b82f6",
	"#22c55e",
	"#f59e0b",
	"#ef4444",
	"#8b5cf6",
	"#06b6d4",
	"#ec4899",
	"#14b8a6",
	"#f97316",
	"#6366f1",
	"#84cc16",
	"#d946ef",
	"#0ea5e9",
	"#f43f5e",
	"#10b981",
	"#a855f7",
	"#eab308",
	"#0891b2",
	"#db2777",
	"#7c3aed",
];

const getMarkerColor = (menu: AuthMenuInfoDto) => {
	let colorIndex = 0;
	for (let index = 0; index < menu.menuCode.length; index++) colorIndex += menu.menuCode.charCodeAt(index);
	return markerColors[colorIndex % markerColors.length];
};

const handleMoveMenu = (index: number, offset: number) => {
	const menus = [...selectedMenus.value];
	const target = index + offset;
	if (target < 0 || target >= menus.length) return;
	[menus[index], menus[target]] = [menus[target], menus[index]];
	selectedMenus.value = menus;
};

const handleToggleMenu = (menu: AuthMenuInfoDto) => {
	if (!menu.menuCode) return;
	selectedMenus.value = selectedMenuCodes.value.includes(menu.menuCode)
		? selectedMenus.value.filter((item) => item.menuCode !== menu.menuCode)
		: [...selectedMenus.value, menu];
};

const handleConfirm = () => {
	popupRef.value.close(async () => {
		await employeeApi.editCommonMenus({ commonMenuCodes: selectedMenuCodes.value });
		useToast.success("常用应用已保存");
		emit("ok");
	});
};

const open = (menus: AuthMenuInfoDto[]) => {
	popupRef.value?.open(() => {
		selectedMenus.value = [...menus];
	});
};

defineExpose({
	open,
});
</script>

<style lang="scss" scoped>
.commonMenu-popup {
	display: flex;
	flex-direction: column;
	height: 100%;
	background: var(--wot-filled-bottom);
}

.popup__header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 16rpx 24rpx;
	background: var(--wot-filled-oppo);
	border-bottom: 1rpx solid var(--wot-divider-light);
}

.menu-warp {
	flex: 1;
	min-height: 0;
	height: 0;
}

.selected-list,
.menu-list {
	margin: 12rpx 24rpx 24rpx;
}

.selected-row {
	display: flex;
	align-items: center;
	gap: 8rpx;
	min-height: 76rpx;
	border-bottom: 1rpx solid var(--wot-divider-light);
}

.selected-row:last-child {
	border-bottom: 0;
}

.selected-row__title {
	flex: 1;
	min-width: 0;
	overflow-wrap: anywhere;
	font-size: 26rpx;
}

.section-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	gap: 16rpx;
	min-height: 44rpx;
	margin-bottom: 30rpx;
}

.section-title {
	display: flex;
	align-items: center;
	gap: 12rpx;
	font-size: 30rpx;
	font-weight: var(--wot-font-weight-semibold);
}

.section-marker {
	flex-shrink: 0;
	width: 8rpx;
	height: 28rpx;
	border-radius: 4rpx;
}
</style>

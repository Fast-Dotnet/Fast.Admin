<template>
	<view class="menu-grid" :class="{ 'is-editable': props.editable }">
		<button v-for="menu in props.menus" :key="menu.menuCode" class="menu-item" @click="emit('select', menu)">
			<view class="menu-item__icon">
				<FaImage
					v-if="menu.icon && menu.icon.includes('/')"
					width="76rpx"
					height="76rpx"
					:src="menu.icon"
					mode="aspectFit"
					original
					:hide-image="false"
					:enable-preview="false"
				/>
				<FaIcon v-else :name="menu.icon || 'menu'" size="44rpx" />
				<view v-if="props.editable" class="menu-item__check" :class="{ 'is-selected': props.selectedMenuCodes.includes(menu.menuCode) }">
					<wd-icon :name="props.selectedMenuCodes.includes(menu.menuCode) ? 'check' : 'plus'" size="12px" />
				</view>
			</view>
			<text class="menu-item__title">{{ menu.menuName }}</text>
		</button>
	</view>
</template>

<script setup lang="ts">
import { definePropType } from "@fast-china/utils";
import type { AuthMenuInfoDto } from "@/api/services/Auth/auth/models/AuthMenuInfoDto";

defineOptions({
	name: "MenuGrid",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 展示的菜单列表 */
	menus: {
		type: definePropType<AuthMenuInfoDto[]>(Array),
		required: true,
	},
	/** 是否显示菜单选择标记 */
	editable: Boolean,
	/** 已选菜单 Code 集合 */
	selectedMenuCodes: {
		type: definePropType<string[]>(Array),
		default: (): string[] => [],
	},
});

const emit = defineEmits<{
	select: [menu: AuthMenuInfoDto];
}>();
</script>

<style lang="scss" scoped>
.menu-grid {
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 32rpx 12rpx;

	&.is-editable {
		row-gap: 20rpx;

		.menu-item {
			gap: 10rpx;
		}
	}
}

.menu-item {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 16rpx;
	min-width: 0;
	width: 100%;
	margin: 0;
	padding: 0;
	border: 0;
	background: transparent;
	color: var(--wot-text-main);
	line-height: 1.5;
	border-radius: 12rpx;
	overflow: visible;
}

.menu-item::after {
	border: 0;
}

.menu-item__icon {
	display: flex;
	position: relative;
	align-items: center;
	justify-content: center;
	width: 84rpx;
	height: 84rpx;
	border-radius: 24rpx;
	color: var(--wot-primary-6);
	background: var(--wot-primary-1);
}

.menu-item__title {
	max-width: 100%;
	font-size: var(--wot-typography-label-size-main);
	overflow-wrap: anywhere;
}

.menu-item__check {
	position: absolute;
	top: -4px;
	right: -4px;
	display: flex;
	align-items: center;
	justify-content: center;
	width: 18px;
	height: 18px;
	border-radius: 50%;
	color: var(--wot-text-secondary);
	background: var(--wot-filled-strong);
	border: 1px solid var(--wot-card-bg, var(--wot-filled-oppo));
}

.menu-item__check.is-selected {
	background: var(--wot-primary-6);
	color: var(--wot-text-white);
}
</style>

<template>
	<view class="menu-grid">
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
					@click="emit('select', menu)"
				/>
				<FaIcon v-else :name="menu.icon || 'menu'" size="44rpx" />
				<view v-if="props.editable" class="menu-item__check" :class="{ 'is-selected': props.selectedCodes.includes(menu.menuCode) }">
					<wd-icon :name="props.selectedCodes.includes(menu.menuCode) ? 'check' : 'add'" size="20rpx" />
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
	selectedCodes: {
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
	font-size: 24rpx;
	overflow-wrap: anywhere;
}

.menu-item__check {
	position: absolute;
	top: -8rpx;
	right: -8rpx;
	display: flex;
	align-items: center;
	justify-content: center;
	width: 32rpx;
	height: 32rpx;
	border-radius: 50%;
	color: var(--wot-text-secondary);
	background: var(--wot-filled-strong);
	border: 2rpx solid var(--wot-filled-oppo);
}

.menu-item__check.is-selected {
	background: var(--wot-primary-6);
	color: var(--wot-text-white);
}
</style>

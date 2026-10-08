<template>
	<view class="fa-navbar-capsule">
		<view class="fa-navbar-capsule__icon">
			<wd-icon v-if="props.back" name="chevron-left" @click="() => emit('backClick')" />
			<wd-icon v-if="props.home" name="home" @click="() => emit('homeClick')" />
			<slot name="icon" />
		</view>
		<view class="fa-navbar-capsule__content">
			<slot />
		</view>
	</view>
</template>

<script setup lang="ts">
defineOptions({
	name: "NavbarCapsule",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 显示返回按钮 */
	back: {
		type: Boolean,
		default: true,
	},
	/** 显示首页按钮 */
	home: {
		type: Boolean,
		default: false,
	},
});

const emit = defineEmits<{
	/** 返回按钮点击事件，实际导航由调用方处理 */
	backClick: [];
	/** 首页按钮点击事件，实际导航由调用方处理 */
	homeClick: [];
}>();
</script>

<style scoped lang="scss">
.fa-navbar-capsule {
	position: relative;
	top: -2px;
	height: var(--wot-navbar-capsule-height, 30px);
	display: flex;
	align-items: center;
	justify-content: center;
	gap: 5px;
	color: var(--wot-navbar-color, var(--wot-text-main));

	.fa-navbar-capsule__icon {
		height: 100%;
		border-radius: 27px;
		border: 1px solid hsla(0, 0%, 100%, 0.25);
		background: rgba(0, 0, 0, 0.25);
		display: flex;
		align-items: center;
		justify-content: center;
		.wd-icon,
		.fa-icon {
			width: 42px;
			font-size: 20px;
			color: var(--wot-navbar-color, var(--wot-text-main));
			cursor: pointer;
			&:nth-of-type(n + 2)::after {
				content: "";
				display: block;
				position: absolute;
				top: 50%;
				transform: translateY(-50%);
				width: 1px;
				height: 58%;
				background: var(--wot-border-main);
			}
		}
	}

	.fa-navbar-capsule__content {
		height: 100%;
		display: flex;
		align-items: center;
		justify-content: center;
	}
}
</style>

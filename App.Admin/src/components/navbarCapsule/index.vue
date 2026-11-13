<template>
	<view class="fa-navbar-capsule">
		<view class="fa-navbar-capsule__icon">
			<wd-icon v-if="props.back" name="left" @click="() => emit('backClick')" />
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

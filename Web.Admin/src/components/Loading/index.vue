<template>
	<div class="loading">
		<div class="loading__box">
			<svg>
				<circle cx="25" cy="25" r="20" fill="none" />
			</svg>
			<p class="loading-text">{{ props.loadingText }}</p>
		</div>
	</div>
</template>

<script setup lang="ts">
defineOptions({
	name: "Loading",
});

const props = defineProps({
	/** 加载提示文本 */
	loadingText: {
		type: String,
		default: "加载中...",
	},
});
</script>

<style scoped lang="scss">
.loading {
	@keyframes loading-rotate {
		100% {
			transform: rotate(360deg);
		}
	}

	@keyframes loading-dash {
		0% {
			stroke-dasharray: 1, 200;
			stroke-dashoffset: 0;
		}
		50% {
			stroke-dasharray: 90, 150;
			stroke-dashoffset: -40px;
		}
		100% {
			stroke-dasharray: 90, 150;
			stroke-dashoffset: -120px;
		}
	}

	& {
		background-color: var(--el-overlay-color-light);
		bottom: 0;
		left: 0;
		margin: 0;
		position: fixed;
		right: 0;
		top: 0;
		transition: opacity 0.3s;
		z-index: 2001;
		.loading__box {
			margin-top: -25px;
			position: absolute;
			text-align: center;
			top: 50%;
			width: 100%;
			svg {
				animation: loading-rotate 2s linear infinite;
				display: inline;
				height: 50px;
				width: 50px;
				circle {
					animation: loading-dash 1.5s ease-in-out infinite;
					stroke-dasharray: 90, 150;
					stroke-dashoffset: 0;
					stroke-width: 2;
					stroke: var(--el-color-primary);
					stroke-linecap: round;
				}
			}
			p {
				color: var(--el-color-primary);
				font-size: var(--el-font-size-base);
				margin: 3px 0;
			}
		}
	}
}
</style>

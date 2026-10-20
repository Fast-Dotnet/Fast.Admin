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

<style lang="scss" scoped>
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

	position: fixed;
	bottom: 0;
	left: 0;
	right: 0;
	top: 0;
	z-index: 2001;
	margin: 0;
	background-color: var(--el-overlay-color-light);
	transition: opacity 0.3s;
}

.loading__box {
	position: absolute;
	top: 50%;
	width: 100%;
	margin-top: -25px;
	text-align: center;

	svg {
		display: inline;
		width: 50px;
		height: 50px;
		animation: loading-rotate 2s linear infinite;

		circle {
			stroke-dasharray: 90, 150;
			stroke-dashoffset: 0;
			stroke-width: 2;
			stroke: var(--el-color-primary);
			stroke-linecap: round;
			animation: loading-dash 1.5s ease-in-out infinite;
		}
	}

	p {
		margin: 3px 0;
		font-size: var(--el-font-size-base);
		color: var(--el-color-primary);
	}
}

@media (prefers-reduced-motion: reduce) {
	.loading__box {
		svg,
		svg circle {
			animation: none;
		}
	}
}
</style>

<template>
	<template v-if="globalSize === 'small'">
		<div class="footer-small">
			<div class="footer-row">
				<a href="https://fastdotnet.com" target="_blank" rel="noopener noreferrer">
					Copyright © 2018~{{ new Date().getFullYear() }} Fast All rights reserved.
				</a>
				<el-text>v{{ appVersion }}</el-text>
			</div>
			<div v-if="appStore.publicSecurityCode" class="footer-row">
				<a
					:href="`https://beian.mps.gov.cn/#/query/webSearch?code=${appStore.publicSecurityCode?.replace(/\D+/g, '')}`"
					target="_blank"
					rel="noopener noreferrer"
				>
					<img :src="gonganImage" alt="" width="16" height="16" />
					{{ appStore.publicSecurityCode }}
				</a>
				<a v-if="appStore.icpSecurityCode" href="https://beian.miit.gov.cn/" target="_blank" rel="noopener noreferrer">
					{{ appStore.icpSecurityCode }}
				</a>
			</div>
		</div>
	</template>
	<template v-else>
		<a href="https://fastdotnet.com" target="_blank" rel="noopener noreferrer">
			Copyright © 2018~{{ new Date().getFullYear() }} Fast All rights reserved.
		</a>
		<a
			v-if="appStore.publicSecurityCode"
			:href="`https://beian.mps.gov.cn/#/query/webSearch?code=${appStore.publicSecurityCode?.replace(/\D+/g, '')}`"
			target="_blank"
			rel="noopener noreferrer"
		>
			<img :src="gonganImage" alt="" width="16" height="16" />
			{{ appStore.publicSecurityCode }}
		</a>
		<a v-if="appStore.icpSecurityCode" href="https://beian.miit.gov.cn/" target="_blank" rel="noopener noreferrer">
			{{ appStore.icpSecurityCode }}
		</a>
		<el-text>v{{ appVersion }}</el-text>
	</template>
</template>

<script setup lang="ts">
import { useGlobalSize } from "element-plus";
import gonganImage from "@/assets/images/gongan.png";
import { useApp } from "@/stores";

defineOptions({
	// eslint-disable-next-line vue/no-reserved-component-names -- 保持现有组件名称与项目引用兼容
	name: "Footer",
});

const globalSize = useGlobalSize();
const appStore = useApp();

const appVersion = import.meta.env.VITE_APP_VERSION;
</script>

<style lang="scss" scoped>
.footer-small {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	gap: 6px;
	width: 100%;
}

.footer-row {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: center;
	row-gap: 2px;
	min-width: 0;
	max-width: 100%;
}

a,
.el-text {
	display: flex;
	align-items: center;
	justify-content: center;
	gap: 3px;
	min-width: 0;
	max-width: 100%;
	padding-inline: 5px;
	text-align: center;
	text-decoration: none;
	letter-spacing: 0.5px;
	overflow-wrap: anywhere;
	color: var(--el-text-color-secondary);

	img {
		flex-shrink: 0;
	}
}

a:hover {
	color: var(--el-text-color-primary);
}

@media (max-width: 480px) {
	.footer-small {
		gap: 8px;
	}

	a,
	.el-text {
		letter-spacing: 0;
	}
}
</style>

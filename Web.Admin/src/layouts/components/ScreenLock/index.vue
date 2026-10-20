<template>
	<div class="screen-lock">
		<button type="button" class="screen-lock__top" @click="state.showDate = false">
			<el-icon>
				<Lock />
			</el-icon>
			<span>点击解锁</span>
		</button>
		<div class="screen-lock__center">
			<div>
				<transition name="slide-bottom" mode="out-in">
					<span :key="state.hour">{{ state.hour }}</span>
				</transition>
			</div>
			<div>
				<transition name="slide-bottom" mode="out-in">
					<span :key="state.minute">{{ state.minute }}</span>
				</transition>
			</div>
			<div>
				<transition name="slide-bottom" mode="out-in">
					<span :key="state.second">{{ state.second }}</span>
				</transition>
			</div>
		</div>
		<div class="screen-lock__bottom">
			<transition name="slide-bottom" mode="out-in">
				<span :key="state.date">{{ state.date }}</span>
			</transition>
		</div>
		<transition name="slide-bottom" mode="out-in">
			<div v-show="!state.showDate" class="overlay">
				<div class="overlay__top">
					<FaAvatar original :size="64" :src="userInfoStore.avatar" :icon="UserFilled" />
					<span>{{ userInfoStore.employeeName || userInfoStore.nickName }}</span>
					<el-input v-model.trim="state.password" placeholder="请输入锁屏密码" type="password" :prefix-icon="Lock" autocomplete="off" />
					<span v-show="state.passwordError" class="error">锁屏密码错误</span>
					<div class="btn__warp">
						<el-button type="primary" size="default" link @click="state.showDate = true">返回</el-button>
						<el-button type="primary" size="default" link @click="handleBackLogin">返回登录</el-button>
						<el-button type="primary" size="default" link @click="handleUnlock">进入系统</el-button>
					</div>
				</div>
				<div class="overlay__bottom">
					<span>{{ state.hour }}:{{ state.minute }}:{{ state.second }}</span>
					<span>{{ state.date }}</span>
				</div>
			</div>
		</transition>
	</div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, reactive } from "vue";
import { Lock, UserFilled } from "@element-plus/icons-vue";
import { dayjs } from "element-plus";
import { useConfig, useUserInfo } from "@/stores";

defineOptions({
	name: "ScreenLock",
});

const configStore = useConfig();
const userInfoStore = useUserInfo();

/** 定时器 */
let timer: NodeJS.Timeout = null;

const state = reactive({
	/** 显示时间 */
	showDate: true,
	/** 日期 */
	date: "",
	/** 时 */
	hour: "",
	/** 分 */
	minute: "",
	/** 秒 */
	second: "",
	/** 密码 */
	password: "",
	/** 密码错误 */
	passwordError: false,
});

const handleBackLogin = async () => {
	configStore.screen.password = "";
	configStore.screen.screenLock = false;
	await userInfoStore.logout();
};

const handleUnlock = () => {
	if (!state.password) return;
	if (state.password === configStore.screen.password) {
		state.passwordError = false;
		configStore.screen.password = "";
		configStore.screen.screenLock = false;
	} else {
		state.passwordError = true;
	}
};

const handleUpdate = () => {
	const now = dayjs();
	state.date = now.format("YYYY-MM-DD dddd");
	state.hour = now.format("HH");
	state.minute = now.format("mm");
	state.second = now.format("ss");
};

const handleKeyDown = (event: KeyboardEvent) => {
	if (!state.showDate) {
		if (event.key === "Escape") {
			state.showDate = true;
		} else if (event.key === "Enter") {
			handleUnlock();
		}
	}
};

onMounted(() => {
	handleUpdate();
	timer && clearInterval(timer);
	timer = setInterval(handleUpdate, 1000);
	window.addEventListener("keydown", handleKeyDown);
});

onUnmounted(() => {
	timer && clearInterval(timer);
	timer = null;
	window.removeEventListener("keydown", handleKeyDown);
});
</script>

<style lang="scss" scoped>
.screen-lock {
	position: fixed;
	inset: 0;
	z-index: 2001;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: max(20px, env(safe-area-inset-top)) clamp(12px, 7vw, 100px) max(20px, env(safe-area-inset-bottom));
	color: #f2f3f5;
	background: #0a0a0a;
	-webkit-user-select: none;
	user-select: none;
}

.screen-lock__top {
	display: flex;
	flex-shrink: 0;
	flex-direction: column;
	align-items: center;
	padding: 10px;
	font-size: clamp(18px, 2vw, 24px);
	color: inherit;
	border: 0;
	background: transparent;
	cursor: pointer;
}

.screen-lock__center {
	display: flex;
	flex: 1;
	flex-direction: row;
	gap: clamp(8px, 3vw, 50px);
	width: 100%;
	min-height: 0;
	padding: clamp(16px, 8vh, 100px) clamp(0px, 3vw, 50px);

	div {
		display: flex;
		flex: 1;
		align-items: center;
		justify-content: center;
		min-width: 0;
		border-radius: clamp(12px, 3vw, 40px);
		background-color: #141414;

		span {
			font-size: min(20vw, 48dvh);
			font-weight: 600;
			font-variant-numeric: tabular-nums;
		}
	}
}

.screen-lock__bottom {
	flex-shrink: 0;
	font-size: clamp(18px, 3vw, 32px);
	text-align: center;
}

.overlay {
	position: fixed;
	inset: 0;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	overflow: auto;
	padding: max(20px, env(safe-area-inset-top)) max(16px, env(safe-area-inset-right)) max(20px, env(safe-area-inset-bottom))
		max(16px, env(safe-area-inset-left));
	font-size: clamp(20px, 4vw, 28px);
	background-color: rgba(0, 0, 0, 0.7);
	backdrop-filter: blur(32px);
}

.overlay__top {
	display: flex;
	flex: 1;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	width: min(320px, 100%);
	padding-block: 16px;
	overflow-wrap: anywhere;
	text-align: center;

	.el-avatar {
		margin-bottom: 10px;
		font-size: 36px;
	}

	.el-input {
		width: 100%;
		margin-top: 10px;
		margin-bottom: 5px;
	}
}

.error {
	width: 100%;
	font-size: 14px;
	color: #f56c6c;
}

.btn__warp {
	display: flex;
	justify-content: space-between;
	width: 100%;
	line-height: 0;
}

.overlay__bottom {
	display: flex;
	flex-shrink: 0;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	font-size: clamp(16px, 3vw, 24px);
}

@media (max-width: 767px) and (orientation: portrait) {
	.screen-lock__center {
		flex: 0 0 auto;
		height: clamp(140px, 35dvh, 280px);
		margin-block: auto;
		padding: 0;
	}
}
</style>

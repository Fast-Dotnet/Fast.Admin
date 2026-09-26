<template>
	<div class="screen-lock">
		<div class="top" @click="state.showDate = false">
			<el-icon>
				<Lock />
			</el-icon>
			<span>点击解锁</span>
		</div>
		<div class="center">
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
		<div class="bottom">
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

<style scoped lang="scss">
.screen-lock {
	position: fixed;
	top: 0;
	left: 0;
	width: 100vw;
	height: 100vh;
	background: #0a0a0a;
	z-index: 2001;
	padding: 20px 100px;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	-webkit-user-select: none;
	user-select: none;
	.top {
		font-size: 24px;
		padding: 10px;
		display: flex;
		flex-direction: column;
		align-items: center;
		cursor: pointer;
	}
	.center {
		width: 100%;
		padding: 100px 50px;
		flex: 1;
		display: flex;
		flex-direction: row;
		gap: 50px;
		div {
			flex: 1;
			background-color: #141414;
			border-radius: 40px;
			display: flex;
			align-items: center;
			justify-content: center;
			span {
				font-size: 20vw;
				font-weight: 600;
			}
		}
	}
	.bottom {
		font-size: 32px;
	}
	.overlay {
		position: fixed;
		top: 0;
		left: 0;
		width: 100vw;
		height: 100vh;
		background-color: rgba(0, 0, 0, 0.7);
		backdrop-filter: blur(32px);
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		font-size: 28px;
		padding: 20px 0;
		.overlay__top {
			flex: 1;
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
			.el-avatar {
				font-size: 36px;
				margin-bottom: 10px;
			}
			.el-input {
				margin-top: 10px;
				width: 320px;
				margin-bottom: 5px;
			}
			.error {
				width: 320px;
				font-size: 14px;
				color: #f56c6c;
			}
			.btn__warp {
				width: 320px;
				line-height: 0;
				display: flex;
				justify-content: space-between;
			}
		}
		.overlay__bottom {
			font-size: 24px;
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
		}
	}
}
</style>

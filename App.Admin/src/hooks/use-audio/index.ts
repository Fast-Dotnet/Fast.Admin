import { reactive, readonly } from "vue";

/** 当前平台内部音频上下文实例 */
interface InnerAudioContextInstance {
	/** 音频资源地址 */
	src: string;
	/** 当前播放位置，单位为秒 */
	currentTime: number;
	/** 播放倍率 */
	playbackRate?: number;
	/** 音量，范围为 0 到 1 */
	volume?: number;
	/** 开始或继续播放。 */
	play(): void;
	/** 暂停播放。 */
	pause(): void;
	/** 跳转到指定秒数。 */
	seek(seconds: number): void;
	/** 注册音频可播放事件 */
	onCanplay(callback: () => void): void;
	/** 注册开始播放事件 */
	onPlay(callback: () => void): void;
	/** 注册暂停事件 */
	onPause(callback: () => void): void;
	/** 注册播放结束事件 */
	onEnded(callback: () => void): void;
	/** 注册播放进度更新事件 */
	onTimeUpdate(callback: () => void): void;
	/** 注册播放错误事件 */
	onError(callback: () => void): void;
	/** 注册等待数据事件 */
	onWaiting?(callback: () => void): void;
	/** 注册开始跳转事件 */
	onSeeking?(callback: () => void): void;
	/** 注册跳转完成事件 */
	onSeeked?(callback: () => void): void;
}

/** 音频播放事件 */
export interface AudioPlaybackEvents {
	/** 音频可以播放 */
	onCanplay?(session: AudioPlaybackSession): void;
	/** 音频开始或继续播放 */
	onPlay?(session: AudioPlaybackSession): void;
	/** 音频暂停 */
	onPause?(session: AudioPlaybackSession): void;
	/** 音频播放结束 */
	onEnded?(session: AudioPlaybackSession): void;
	/** 播放进度更新 */
	onTimeUpdate?(session: AudioPlaybackSession): void;
	/** 音频等待数据 */
	onWaiting?(session: AudioPlaybackSession): void;
	/** 音频开始跳转 */
	onSeeking?(session: AudioPlaybackSession): void;
	/** 音频跳转完成 */
	onSeeked?(session: AudioPlaybackSession): void;
	/** 音频播放失败 */
	onError?(session: AudioPlaybackSession): void;
}

/** 音频播放选项 */
export interface AudioPlaybackOptions extends AudioPlaybackEvents {
	/**
	 * 是否在音频可播放后自动开始
	 * @defaultValue true
	 */
	autoplay?: boolean;
	/**
	 * 起始位置，单位为秒，必须为非负有限数
	 * @defaultValue 0
	 */
	startTime?: number;
	/** 播放倍率，范围为 0.5 到 2；未指定时沿用管理器当前倍率，初始为 1 */
	playbackRate?: number;
	/** 音量，范围为 0 到 1；未指定时沿用管理器当前音量，初始为 1 */
	volume?: number;
}

/**
 * 单次音频播放会话。
 *
 * @remarks
 * 新播放会话会使旧会话失效；失效后的控制方法不再操作底层音频，currentTime 返回 0。
 * 页面离开时由调用方 release，底层实例仍保留供后续会话复用。
 */
export interface AudioPlaybackSession {
	/** 当前播放位置，单位为秒 */
	readonly currentTime: number;
	/** 当前会话是否仍持有全局音频实例 */
	readonly isActive: boolean;
	/** 开始或继续播放当前会话，并返回当前会话。 */
	play(): AudioPlaybackSession;
	/** 暂停当前会话，并返回当前会话。 */
	pause(): AudioPlaybackSession;
	/** 跳转到指定秒数，并返回当前会话。 */
	seek(seconds: number): AudioPlaybackSession;
	/** 设置播放倍率，并返回当前会话。 */
	setPlaybackRate(value: number): AudioPlaybackSession;
	/** 设置音量，并返回当前会话。 */
	setVolume(value: number): AudioPlaybackSession;
	/** 释放当前会话对全局音频实例的占用。 */
	release(): void;
}

/** 应用级音频状态 */
export interface AudioState {
	/** 当前音频资源地址；没有活动会话时为空字符串 */
	src: string;
	/** 当前播放位置，单位为秒 */
	currentTime: number;
	/** 是否存在正在占用全局音频实例的会话 */
	hasActiveSession: boolean;
	/** 音频是否正在播放 */
	isPlaying: boolean;
	/** 音频是否正在等待媒体数据 */
	isWaiting: boolean;
	/** 当前播放倍率 */
	playbackRate: number;
	/** 当前音量，范围为 0 到 1 */
	volume: number;
}

/** 应用级单例音频管理器 */
export interface AudioManager {
	/** 只读音频状态 */
	readonly state: Readonly<AudioState>;
	/**
	 * 使用唯一的音频实例播放指定地址，并使上一会话失效。
	 *
	 * @param src - 非空音频资源地址
	 * @param options - 播放配置及当前会话的事件回调
	 * @returns 当前播放会话
	 * @throws 地址为空或播放参数超出有效范围时抛出异常。
	 */
	play(src: string, options?: AudioPlaybackOptions): AudioPlaybackSession;
	/** 暂停当前音频。 */
	pause(): AudioManager;
	/** 继续播放当前音频。 */
	resume(): AudioManager;
	/** 停止当前音频并释放播放会话。 */
	stop(): AudioManager;
	/** 跳转当前音频的播放位置。 */
	seek(seconds: number): AudioManager;
	/** 设置当前及后续音频的播放倍率。 */
	setPlaybackRate(value: number): AudioManager;
	/** 设置当前及后续音频的音量。 */
	setVolume(value: number): AudioManager;
}

/** 应用级音频状态 */
const state = reactive<AudioState>({
	src: "",
	currentTime: 0,
	hasActiveSession: false,
	isPlaying: false,
	isWaiting: false,
	playbackRate: 1,
	volume: 1,
});

/** 全应用唯一的底层音频实例 */
let audio: InnerAudioContextInstance | undefined;
/** 当前占用底层音频实例的播放会话 */
let activeSession:
	| {
			id: number;
			isReady: boolean;
			autoplay: boolean;
			startTime: number;
			events: AudioPlaybackEvents;
			handle: AudioPlaybackSession;
	  }
	| undefined;
/** 单调递增的会话编号，用于隔离已经失效的页面操作 */
let sessionId = 0;

/**
 * 创建当前平台的内部音频上下文适配器。
 *
 * @remarks
 * H5 直接包装 `HTMLAudioElement`，以保留真实的 `playing` 事件和 `playbackRate`；其他平台使用 uni-app 原生内部音频上下文。
 * 实例只允许由本模块懒创建一次，页面不能直接持有或销毁平台句柄。
 *
 * @returns 当前平台的内部音频上下文实例
 */
function createInnerAudioContext(): InnerAudioContextInstance {
	// #ifdef H5
	const context = new Audio();
	// HTMLMediaElement.play() 通过 Promise 报告失败，统一转换为内部音频上下文的错误事件。
	const errors: (() => void)[] = [];
	context.preload = "auto";
	return {
		get src() {
			return context.src;
		},
		set src(value) {
			context.src = value;
		},
		get currentTime() {
			return context.currentTime;
		},
		get playbackRate() {
			return context.playbackRate;
		},
		set playbackRate(value) {
			if (value !== undefined) context.playbackRate = value;
		},
		get volume() {
			return context.volume;
		},
		set volume(value) {
			if (value !== undefined) context.volume = value;
		},
		play() {
			context.play().catch(() => {
				for (const callback of errors) callback();
			});
		},
		pause() {
			context.pause();
		},
		seek(seconds) {
			context.currentTime = seconds;
		},
		onCanplay(callback) {
			context.addEventListener("canplay", callback);
		},
		onPlay(callback) {
			context.addEventListener("playing", callback);
		},
		onPause(callback) {
			context.addEventListener("pause", callback);
		},
		onEnded(callback) {
			context.addEventListener("ended", callback);
		},
		onTimeUpdate(callback) {
			context.addEventListener("timeupdate", callback);
		},
		onWaiting(callback) {
			context.addEventListener("waiting", callback);
		},
		onSeeking(callback) {
			context.addEventListener("seeking", callback);
		},
		onSeeked(callback) {
			context.addEventListener("seeked", callback);
		},
		onError(callback) {
			errors.push(callback);
			context.addEventListener("error", callback);
		},
	};
	// #endif
	// #ifndef H5
	return uni.createInnerAudioContext();
	// #endif
}

/** 将底层音频事件发送给当前有效会话。 */
const emit = (event: keyof AudioPlaybackEvents): void => {
	const session = activeSession;
	if (session) session.events[event]?.(session.handle);
};

/** 获取全局音频实例，并在首次创建时安装唯一一组事件监听。 */
const getAudio = (): InnerAudioContextInstance => {
	if (audio) return audio;
	audio = createInnerAudioContext();
	audio.onCanplay(() => {
		const session = activeSession;
		if (!session || session.isReady) return;
		session.isReady = true;
		if (session.startTime > 0) audio?.seek(session.startTime);
		emit("onCanplay");
		if (session.autoplay && session === activeSession) audio?.play();
	});
	audio.onPlay(() => {
		state.isPlaying = true;
		state.isWaiting = false;
		emit("onPlay");
	});
	audio.onPause(() => {
		state.isPlaying = false;
		if (!activeSession?.isReady) return;
		emit("onPause");
	});
	audio.onEnded(() => {
		state.isPlaying = false;
		state.isWaiting = false;
		emit("onEnded");
	});
	audio.onTimeUpdate(() => {
		state.currentTime = audio?.currentTime ?? 0;
		emit("onTimeUpdate");
	});
	audio.onWaiting?.(() => {
		state.isWaiting = true;
		emit("onWaiting");
	});
	audio.onSeeking?.(() => emit("onSeeking"));
	audio.onSeeked?.(() => {
		state.currentTime = audio?.currentTime ?? 0;
		state.isWaiting = false;
		emit("onSeeked");
	});
	audio.onError(() => {
		state.isPlaying = false;
		state.isWaiting = false;
		emit("onError");
	});
	return audio;
};

/** 判断会话是否仍持有全局音频实例。 */
const isCurrentSession = (id: number): boolean => activeSession?.id === id;

/** 校验并返回跨端支持的播放倍率。 */
const normalizePlaybackRate = (value: number): number => {
	if (!Number.isFinite(value) || value < 0.5 || value > 2) throw new Error("播放倍率必须在 0.5 到 2 之间。");
	return value;
};

/** 校验并返回有效音量。 */
const normalizeVolume = (value: number): number => {
	if (!Number.isFinite(value) || value < 0 || value > 1) throw new Error("音量必须在 0 到 1 之间。");
	return value;
};

/** 校验并返回以秒为单位的播放位置。 */
const normalizeSeconds = (value: number): number => {
	if (!Number.isFinite(value) || value < 0) throw new Error("播放位置必须是大于或等于 0 的有限数值。");
	return value;
};

/** 应用级单例音频管理 */
export const useAudio: AudioManager = {
	/** 只读音频状态 */
	state: readonly(state),

	/**
	 * 使用应用唯一的音频实例播放指定地址。
	 *
	 * @remarks
	 * 新调用会使上一播放会话立即失效；旧页面后续的暂停或释放操作不会影响新会话。
	 *
	 * @param src - 音频资源地址
	 * @param options - 播放选项
	 * @returns 当前播放会话
	 */
	play(src: string, options: AudioPlaybackOptions = {}): AudioPlaybackSession {
		const url = src.trim();
		if (!url) throw new Error("音频资源地址不能为空。");
		const playbackRate = normalizePlaybackRate(options.playbackRate ?? state.playbackRate);
		const volume = normalizeVolume(options.volume ?? state.volume);
		const startTime = normalizeSeconds(options.startTime ?? 0);

		const context = getAudio();
		const id = ++sessionId;
		const previousSession = activeSession;
		if (previousSession && state.isPlaying) previousSession.events.onPause?.(previousSession.handle);
		activeSession = undefined;
		context.pause();
		const handle: AudioPlaybackSession = {
			get currentTime() {
				return isCurrentSession(id) ? context.currentTime : 0;
			},
			get isActive() {
				return isCurrentSession(id);
			},
			play() {
				if (isCurrentSession(id)) context.play();
				return handle;
			},
			pause() {
				if (isCurrentSession(id)) context.pause();
				return handle;
			},
			seek(seconds) {
				if (isCurrentSession(id)) context.seek(normalizeSeconds(seconds));
				return handle;
			},
			setPlaybackRate(value) {
				if (!isCurrentSession(id)) return handle;
				const normalizedValue = normalizePlaybackRate(value);
				state.playbackRate = normalizedValue;
				context.playbackRate = normalizedValue;
				return handle;
			},
			setVolume(value) {
				if (!isCurrentSession(id)) return handle;
				const normalizedValue = normalizeVolume(value);
				state.volume = normalizedValue;
				context.volume = normalizedValue;
				return handle;
			},
			release() {
				if (!isCurrentSession(id)) return;
				activeSession = undefined;
				context.pause();
				state.src = "";
				state.currentTime = 0;
				state.hasActiveSession = false;
				state.isPlaying = false;
				state.isWaiting = false;
			},
		};
		activeSession = {
			id,
			isReady: false,
			autoplay: options.autoplay !== false,
			startTime,
			events: options,
			handle,
		};

		state.src = url;
		state.currentTime = 0;
		state.hasActiveSession = true;
		state.isPlaying = false;
		state.isWaiting = true;
		state.playbackRate = playbackRate;
		state.volume = volume;
		context.playbackRate = state.playbackRate;
		context.volume = state.volume;
		context.src = url;

		return handle;
	},
	/**
	 * 暂停当前音频；没有活动会话时不执行操作。
	 *
	 * @returns 音频管理器，可继续链式调用
	 */
	pause(): AudioManager {
		activeSession?.handle.pause();
		return useAudio;
	},
	/**
	 * 继续播放当前音频；没有活动会话时不执行操作。
	 *
	 * @returns 音频管理器，可继续链式调用
	 */
	resume(): AudioManager {
		activeSession?.handle.play();
		return useAudio;
	},
	/**
	 * 停止当前音频并释放播放会话。
	 *
	 * @remarks
	 * 底层音频实例仍然保留，下一次调用 `play()` 时会继续复用。
	 *
	 * @returns 音频管理器，可继续链式调用
	 */
	stop(): AudioManager {
		activeSession?.handle.release();
		return useAudio;
	},
	/**
	 * 跳转当前音频的播放位置。
	 *
	 * @param seconds - 播放位置，单位为秒
	 * @returns 音频管理器，可继续链式调用
	 */
	seek(seconds: number): AudioManager {
		activeSession?.handle.seek(normalizeSeconds(seconds));
		return useAudio;
	},
	/**
	 * 设置当前及后续音频的播放倍率。
	 *
	 * @param value - 播放倍率，范围为 0.5 到 2
	 * @returns 音频管理器，可继续链式调用
	 */
	setPlaybackRate(value: number): AudioManager {
		const normalizedValue = normalizePlaybackRate(value);
		state.playbackRate = normalizedValue;
		if (activeSession) activeSession.handle.setPlaybackRate(normalizedValue);
		return useAudio;
	},
	/**
	 * 设置当前及后续音频的音量。
	 *
	 * @param value - 音量，范围为 0 到 1
	 * @returns 音频管理器，可继续链式调用
	 */
	setVolume(value: number): AudioManager {
		const normalizedValue = normalizeVolume(value);
		state.volume = normalizedValue;
		if (activeSession && audio) audio.volume = normalizedValue;
		return useAudio;
	},
};

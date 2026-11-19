/** 连接状态，可用于运行时判断 */
export enum SignalRSocketState {
	Disconnected = "Disconnected",
	Connecting = "Connecting",
	Connected = "Connected",
	Reconnecting = "Reconnecting",
	Disconnecting = "Disconnecting",
}

/** 服务端事件处理器；参数类型仅用于静态提示，业务层仍需校验外部数据。 */
export type SignalRMessageHandler<TArgs extends unknown[] = unknown[]> = (...args: TArgs) => unknown;

/** 通用事件处理器；与按名称注册的处理器同时触发，不承担客户端调用的返回值。 */
export type SignalREventHandler = (target: string, args: unknown[]) => void | Promise<void>;

/** 开始重连或最终关闭时的处理器 */
export type SignalRLifecycleHandler = (error?: Error) => void | Promise<void>;

/** 重连成功时的处理器；直连 WebSocket 不提供协商得到的 connectionId。 */
export type SignalRReconnectedHandler = () => void | Promise<void>;

/** 当前重连轮次的信息 */
export type SignalRRetryContext = {
	/** 本轮已安排的重试次数；第一次计算延迟时为 0 */
	previousRetryCount: number;
	/** 本轮重连已经过的时间，单位为毫秒 */
	elapsedMilliseconds: number;
	/** 最近一次连接失败的原因 */
	retryReason: Error;
};

/** 与 SignalR IRetryPolicy 相同的重连策略入口 */
export type SignalRRetryPolicy = {
	/** 返回下次重连的延迟毫秒数；返回 null 停止重连。 */
	nextRetryDelayInMilliseconds(context: SignalRRetryContext): number | null;
};

/** 重连配置；true 使用默认延迟，false 禁用自动重连 */
export type SignalRAutomaticReconnect = boolean | readonly number[] | SignalRRetryPolicy | ((context: SignalRRetryContext) => number | null);

/** 单次调用配置，独立于连接级默认值 */
export type SignalRInvocationOptions = {
	/** 调用超时，单位为毫秒；超时只结束本地等待，不取消服务端方法 */
	timeoutInMilliseconds?: number;
};

/** 流式结果订阅器；异步回调的异常交给 onError，不形成流量背压。 */
export type SignalRStreamSubscriber<T> = {
	/** 处理一个流式结果 */
	next?: (item: T) => unknown;
	/** 处理服务端错误、断线或主动停止；未提供时交给 onError */
	error?: (error: Error) => unknown;
	/** 服务端正常结束结果流时触发；主动取消不触发 */
	complete?: () => unknown;
};

/** 订阅句柄 */
export type SignalRSubscription = {
	/** 移除当前订阅；最后一个订阅被移除时，发送 CancelInvocation 取消服务端结果流。 */
	dispose(): void;
};

/** 服务端结果流；不缓存已发送的数据，也不会在断线重连后自动恢复。 */
export type SignalRStreamResult<T> = {
	/** 注册订阅器；已结束的流只通知最终状态，已取消的流不再通知。 */
	subscribe(subscriber: SignalRStreamSubscriber<T>): SignalRSubscription;
};

/** 原生 WebSocket 连接与 SignalR JSON 协议配置 */
export type SignalRSocketOptions = {
	/** 每次连接、重连前重新读取 Token；本封装不主动请求刷新接口。 */
	accessTokenFactory?: () => string | Promise<string>;
	/** 原生平台请求头；H5 WebSocket 不支持自定义请求头 */
	headers?: Record<string, string>;
	/** 包括地址、Token 获取和底层连接建立的超时，默认 10000 毫秒 */
	connectTimeoutInMilliseconds?: number;
	/** 底层连接打开后等待 SignalR 握手响应的超时，默认 15000 毫秒 */
	handshakeTimeoutInMilliseconds?: number;
	/** 客户端空闲发送间隔，默认 15000 毫秒 */
	keepAliveIntervalInMilliseconds?: number;
	/** 等待服务端有效消息的超时，默认 60000 毫秒 */
	serverTimeoutInMilliseconds?: number;
	/** 单次 Hub 调用超时，默认 10000 毫秒；不取消服务端方法 */
	invocationTimeoutInMilliseconds?: number;
	/** 等待 SocketTask.send 回调的超时，默认 10000 毫秒 */
	sendTimeoutInMilliseconds?: number;
	/** 等待底层关闭事件的上限，默认 3000 毫秒 */
	closeTimeoutInMilliseconds?: number;
	/** 默认按 0、2000、10000、30000 毫秒重连；false 禁用，策略返回 null 停止。 */
	automaticReconnect?: SignalRAutomaticReconnect;
	/** 首次连接失败是否应用重连策略，默认 false，与 SignalR start 的默认语义一致 */
	retryOnInitialFailure?: boolean;
	/** 接收业务回调、未处理的流错误和关闭错误；业务侧记录错误时应脱敏。 */
	onError?: (error: Error) => void | Promise<void>;
};

/** 基于 SocketTask 的 SignalR JSON 客户端，事件与方法参数按 SDK 的可变参数方式传递 */
export type SignalRSocket = {
	/** 当前连接状态 */
	readonly state: SignalRSocketState;
	/** 客户端空闲发送间隔，单位为毫秒；修改后重新计时 */
	keepAliveIntervalInMilliseconds: number;
	/** 等待服务端有效消息的超时，单位为毫秒；修改后重新计时 */
	serverTimeoutInMilliseconds: number;
	/** 可并发调用；连接中或重连中重复 start 会等待同一轮连接。 */
	start(): Promise<void>;
	/** 停止重连、拒绝未完成调用并等待底层关闭；随后可重新 start。 */
	stop(): Promise<void>;
	/**
	 * 调用 Hub 方法并等待 Completion；断线不重放，超时不取消后端执行。
	 * @typeParam T - 预期结果类型；只提供静态提示，不执行运行时校验
	 * @param target - Hub 方法名
	 * @param args - 按后端参数顺序传递的实参；数组作为一个实参传递
	 * @returns 服务端返回值；无返回值的方法解析为 undefined
	 */
	invoke<T = unknown>(target: string, ...args: unknown[]): Promise<T>;
	/** 使用独立超时调用 Hub 方法；args 为完整参数数组。 */
	invokeWithOptions<T = unknown>(target: string, args: unknown[], options: SignalRInvocationOptions): Promise<T>;
	/** 仅等待消息写入连接，不等待服务端方法的执行结果。 */
	send(target: string, ...args: unknown[]): Promise<void>;
	/**
	 * 立即调用服务端流式方法，使用 subscribe 接收结果、dispose 取消订阅。
	 * @typeParam T - 预期流元素类型；不执行运行时校验
	 * @remarks
	 * 应在返回后立即订阅；单次调用超时不适用于结果流，连接失活仍由服务端消息超时检测。
	 */
	stream<T = unknown>(target: string, ...args: unknown[]): SignalRStreamResult<T>;
	/** 注册事件处理器；名称不区分大小写，同一函数不会重复注册。 */
	on<TArgs extends unknown[]>(target: string, handler: SignalRMessageHandler<TArgs>): void;
	/** 注册只执行一次的事件处理器；off 可通过原函数取消。 */
	once<TArgs extends unknown[]>(target: string, handler: SignalRMessageHandler<TArgs>): void;
	/** 移除指定处理器；省略 handler 时移除该事件的全部处理器。 */
	off<TArgs extends unknown[]>(target: string, handler?: SignalRMessageHandler<TArgs>): void;
	/** 注册开始重连的处理器，一轮重连只触发一次。 */
	onreconnecting(handler: SignalRLifecycleHandler): void;
	/** 注册重连成功的处理器；不会等待异步回调再开放业务调用。 */
	onreconnected(handler: SignalRReconnectedHandler): void;
	/** 注册最终关闭的处理器；单次自动重试失败不会触发。 */
	onclose(handler: SignalRLifecycleHandler): void;
	/** 移除指定重连处理器；省略参数时移除全部。 */
	offreconnecting(handler?: SignalRLifecycleHandler): void;
	/** 移除指定重连成功处理器；省略参数时移除全部。 */
	offreconnected(handler?: SignalRReconnectedHandler): void;
	/** 移除指定关闭处理器；省略参数时移除全部。 */
	offclose(handler?: SignalRLifecycleHandler): void;
	/** 兼容原有同步入口；需要等待关闭完成时使用 stop。 */
	close(): void;
};

/** 当前支持的入站 JSON 协议消息 */
type SignalRHubMessage =
	| { type: 1; target: string; arguments: unknown[]; invocationId?: string }
	| { type: 2; invocationId: string; item: unknown }
	| { type: 3; invocationId: string; result?: unknown; error?: string }
	| { type: 6 }
	| { type: 7; error?: string; allowReconnect?: boolean };

/** 等待服务端 Completion 的普通调用 */
type PendingCall = {
	resolve: (value: unknown) => void;
	reject: (error: Error) => void;
	timer: ReturnType<typeof setTimeout>;
};

/** 仍在接收元素的服务端结果流 */
type PendingStream = {
	next: (item: unknown) => void;
	complete: (error?: Error) => void;
};

/** 保存原函数引用，供去重和 off 解绑使用 */
type RegisteredHandler = {
	handler: SignalRMessageHandler;
	once: boolean;
};

/** 一次底层连接尝试；退休后所有迟到回调均不得影响新连接 */
type SocketAttempt = {
	socket?: UniApp.SocketTask;
	/** 尚未收到完整分隔符的入站文本 */
	buffer: string;
	handshaken: boolean;
	retired: boolean;
	nativeClosed: boolean;
	connectTimer?: ReturnType<typeof setTimeout>;
	handshakeTimer?: ReturnType<typeof setTimeout>;
	heartbeatTimer?: ReturnType<typeof setTimeout>;
	serverTimer?: ReturnType<typeof setTimeout>;
	/** 尚未完成的原生发送回调；连接结束时统一拒绝 */
	writes: Set<(error?: Error) => void>;
	/** 底层关闭等待器；onClose、关闭失败和超时只能完成一次 */
	finishClose?: () => void;
};

// SignalR JSON 帧使用 ASCII 0x1E 分隔，与 WebSocket 消息边界无关。
const recordSeparator = "\u001e";
const defaultRetryDelays = [0, 2000, 10_000, 30_000];

/** 将外部 JSON 值收窄为可读取字段的非数组对象，排除 null。 */
const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === "object" && value !== null && !Array.isArray(value);

/** 保留 Error；其他拒绝值转换为不含原始数据的通用错误。 */
const toError = (error: unknown) => (error instanceof Error ? error : new Error("SignalR 操作失败"));

/** 校验毫秒数；普通超时必须为正数，重连延迟可显式允许 0。 */
function checkDuration(value: number, name: string, allowZero = false): number {
	if (!Number.isFinite(value) || (allowZero ? value < 0 : value <= 0)) {
		throw new RangeError(`${name} 必须是${allowZero ? "非负" : "正"}有限数值`);
	}
	return value;
}

/** 校验入站协议字段；不对业务参数和返回值的 DTO 结构作推断。 */
function parseHubMessage(value: unknown): SignalRHubMessage {
	if (!isRecord(value)) throw new Error("SignalR 消息必须是对象");
	switch (value.type) {
		case 1: {
			if (
				typeof value.target !== "string" ||
				!value.target.trim() ||
				!Array.isArray(value.arguments) ||
				("invocationId" in value && typeof value.invocationId !== "string")
			) {
				throw new Error("SignalR Invocation 消息无效");
			}
			const message: Extract<SignalRHubMessage, { type: 1 }> = { type: 1, target: value.target, arguments: value.arguments };
			// 正向收窄可选字段，同时兼容 strictNullChecks 开启和关闭的工程。
			if (typeof value.invocationId === "string") message.invocationId = value.invocationId;
			return message;
		}
		case 2:
			if (typeof value.invocationId !== "string" || !("item" in value)) throw new Error("SignalR StreamItem 消息无效");
			return { type: 2, invocationId: value.invocationId, item: value.item };
		case 3: {
			if (
				typeof value.invocationId !== "string" ||
				("error" in value && typeof value.error !== "string") ||
				("error" in value && "result" in value)
			) {
				throw new Error("SignalR Completion 消息无效");
			}
			const message: Extract<SignalRHubMessage, { type: 3 }> = { type: 3, invocationId: value.invocationId };
			if (typeof value.error === "string") message.error = value.error;
			// 保留 result 是否存在，区分普通调用返回值和流式调用的结束标记。
			if ("result" in value) message.result = value.result;
			return message;
		}
		case 6:
			return { type: 6 };
		case 7: {
			if (("error" in value && typeof value.error !== "string") || ("allowReconnect" in value && typeof value.allowReconnect !== "boolean")) {
				throw new Error("SignalR Close 消息无效");
			}
			const message: Extract<SignalRHubMessage, { type: 7 }> = { type: 7 };
			if (typeof value.error === "string") message.error = value.error;
			if (typeof value.allowReconnect === "boolean") message.allowReconnect = value.allowReconnect;
			return message;
		}
		default:
			throw new Error("不支持的 SignalR 消息类型");
	}
}

/** 转换 WebSocket 协议并替换认证参数，保留调用方已有的业务查询参数。 */
function createSocketUrl(value: string, accessToken?: string): string {
	const url = value.replace(/^http/i, "ws");
	if (!/^wss?:\/\//i.test(url) || url.includes("#")) throw new Error("SignalR 连接地址无效");
	if (accessToken === undefined) return url;
	const separator = url.indexOf("?");
	const path = separator === -1 ? url : url.slice(0, separator);
	const query =
		separator === -1
			? []
			: url
					.slice(separator + 1)
					.split("&")
					.filter(Boolean);
	// 避免重连时沿用地址中旧的 access_token，且不依赖 URL/URLSearchParams。
	const parameters = query.filter((item) => decodeURIComponent(item.split("=")[0] ?? "").toLowerCase() !== "access_token");
	if (accessToken) parameters.push(`access_token=${encodeURIComponent(accessToken)}`);
	return parameters.length ? `${path}?${parameters.join("&")}` : path;
}

/**
 * 创建仅依赖 UniApp.SocketTask 的 SignalR JSON 客户端。
 *
 * @param url - Hub 绝对地址或地址工厂，支持 http、https、ws、wss
 * @param options - 连接、心跳、超时和重连配置
 * @param onEvent - 可选的通用事件观察入口，不承担客户端返回值
 * @returns 尚未启动的客户端，应先注册事件再调用 start
 * @remarks
 * 使用 SignalR JSON v1 协议直连 WebSocket，每次连接均重新调用地址和 Token 工厂。
 * 支持普通调用、客户端返回值和服务端结果流；不支持 HTTP 协商、上传流、MessagePack 和有状态重连。
 * 不依赖浏览器 WebSocket、URL 或 AbortController；进入后台后的定时器调度仍由各平台决定。
 * @throws 配置中的时间不是有效正数，或重连延迟不是非负数时抛出 RangeError。
 */
export function createSignalRSocket(
	url: string | (() => string | Promise<string>),
	options: SignalRSocketOptions = {},
	onEvent?: SignalREventHandler
): SignalRSocket {
	const connectTimeout = checkDuration(options.connectTimeoutInMilliseconds ?? 10_000, "连接超时");
	const handshakeTimeout = checkDuration(options.handshakeTimeoutInMilliseconds ?? 15_000, "握手超时");
	const invocationTimeout = checkDuration(options.invocationTimeoutInMilliseconds ?? 10_000, "调用超时");
	const sendTimeout = checkDuration(options.sendTimeoutInMilliseconds ?? 10_000, "发送超时");
	const closeTimeout = checkDuration(options.closeTimeoutInMilliseconds ?? 3000, "关闭超时");
	let keepAlive = checkDuration(options.keepAliveIntervalInMilliseconds ?? 15_000, "心跳间隔");
	let serverTimeout = checkDuration(options.serverTimeoutInMilliseconds ?? 60_000, "服务端超时");
	const configuredRetry = options.automaticReconnect ?? true;
	const retryDelays =
		typeof configuredRetry === "object" && !("nextRetryDelayInMilliseconds" in configuredRetry) ? [...configuredRetry] : defaultRetryDelays;
	retryDelays.forEach((delay) => checkDuration(delay, "重连间隔", true));

	const pending = new Map<string, PendingCall>();
	const streams = new Map<string, PendingStream>();
	const startWaiters = new Set<{ resolve: () => void; reject: (error: Error) => void }>();
	const handlers = new Map<string, Set<RegisteredHandler>>();
	const reconnectingHandlers = new Set<SignalRLifecycleHandler>();
	const reconnectedHandlers = new Set<SignalRReconnectedHandler>();
	const closeHandlers = new Set<SignalRLifecycleHandler>();
	const closingSockets = new Set<Promise<void>>();

	let state = SignalRSocketState.Disconnected;
	let active: SocketAttempt | undefined;
	let desired = false;
	let hasConnected = false;
	let closeNotified = true;
	// cycle 隔离 stop/start 周期；attempt 身份隔离同一周期中的物理重连。
	let cycle = 0;
	let sequence = 0;
	let retryCount = 0;
	let retryStartedAt = 0;
	let retryTimer: ReturnType<typeof setTimeout> | undefined;
	let stopPromise: Promise<void> | undefined;

	/** 上报未由调用方 Promise 接收的错误，并隔离错误处理器自身的异常。 */
	function reportError(error: unknown): void {
		if (!options.onError) {
			// 不输出可能带认证参数的 URL、底层异常对象或协议内容。
			console.error("[SignalR] 回调或连接清理失败，可通过 onError 获取错误");
			return;
		}
		try {
			Promise.resolve(options.onError(toError(error))).catch(() => console.error("[SignalR] onError 回调执行失败"));
		} catch {
			console.error("[SignalR] onError 回调执行失败");
		}
	}

	/** 隔离同步和异步回调异常；不等待业务回调完成后再处理后续消息。 */
	function notify(callback: () => unknown): void {
		try {
			Promise.resolve(callback()).catch(reportError);
		} catch (error) {
			reportError(error);
		}
	}

	/** 判断回调是否仍属于当前有效连接，屏蔽旧 Socket 的迟到事件。 */
	function isCurrent(attempt: SocketAttempt): boolean {
		return desired && active === attempt && !attempt.retired;
	}

	/** 取消尚未执行的重连计时器，已开始的连接尝试由 retire 清理。 */
	function clearRetry(): void {
		if (retryTimer !== undefined) clearTimeout(retryTimer);
		retryTimer = undefined;
	}

	/** 先移除等待项并清除超时，再拒绝单个普通调用；迟到 Completion 将被忽略。 */
	function rejectCall(id: string, error: Error): void {
		const call = pending.get(id);
		if (!call) return;
		clearTimeout(call.timer);
		pending.delete(id);
		call.reject(error);
	}

	/** 结束当前普通调用和结果流，连接恢复后不自动重放业务请求。 */
	function rejectPending(error: Error): void {
		for (const id of pending.keys()) rejectCall(id, error);
		// 先摘除旧流，再通知订阅器，避免回调中创建的新调用被本轮清理误伤。
		const interrupted = [...streams.values()];
		streams.clear();
		for (const stream of interrupted) stream.complete(error);
	}

	/** 统一拒绝本轮所有并发 start 等待者。 */
	function rejectStarting(error: Error): void {
		for (const waiter of startWaiters) waiter.reject(error);
		startWaiters.clear();
	}

	/** 跟踪原生关闭过程；优先等待 onClose，失败或超时后释放等待者。 */
	function closeNative(attempt: SocketAttempt): void {
		const socket = attempt.socket;
		if (!socket || attempt.nativeClosed || attempt.finishClose) return;
		const closing = new Promise<void>((resolve) => {
			let finished = false;
			let timer: ReturnType<typeof setTimeout> | undefined;
			const finish = () => {
				if (finished) return;
				finished = true;
				clearTimeout(timer);
				attempt.finishClose = undefined;
				resolve();
			};
			timer = setTimeout(() => {
				reportError(new Error("等待 WebSocket 关闭超时"));
				finish();
			}, closeTimeout);
			attempt.finishClose = finish;
			try {
				socket.close({
					code: 1000,
					fail: () => {
						reportError(new Error("WebSocket 关闭请求失败"));
						finish();
					},
				});
			} catch (error) {
				reportError(error);
				finish();
			}
		});
		const tracked = closing.finally(() => closingSockets.delete(tracked));
		closingSockets.add(tracked);
	}

	/** 使连接尝试失效，清除缓冲、计时器及未完成发送，再发起底层关闭。 */
	function retire(attempt: SocketAttempt, error: Error): void {
		if (!attempt.retired) {
			attempt.retired = true;
			attempt.buffer = "";
			if (active === attempt) active = undefined;
			clearTimeout(attempt.connectTimer);
			clearTimeout(attempt.handshakeTimer);
			clearTimeout(attempt.heartbeatTimer);
			clearTimeout(attempt.serverTimer);
			for (const finish of attempt.writes) finish(error);
		}
		closeNative(attempt);
	}

	/** 结束整个启动或重连周期；每轮只通知一次 onclose，允许回调中重新 start。 */
	function finishDisconnected(error?: Error): void {
		const shouldNotify = !closeNotified;
		closeNotified = true;
		desired = false;
		cycle++;
		clearRetry();
		state = SignalRSocketState.Disconnected;
		rejectStarting(error ?? new Error("连接已停止"));
		rejectPending(error ?? new Error("连接已停止"));
		if (shouldNotify) {
			for (const handler of [...closeHandlers]) notify(() => handler(error));
		}
	}

	/** 计算本轮下次重试延迟；策略耗尽、返回 null 或抛错时结束重连。 */
	function scheduleRetry(error: Error): void {
		const currentCycle = cycle;
		if (!desired || state !== SignalRSocketState.Reconnecting) return;
		let delay: number | null;
		try {
			if (configuredRetry === false) {
				delay = null;
			} else {
				const context: SignalRRetryContext = {
					previousRetryCount: retryCount,
					elapsedMilliseconds: Date.now() - retryStartedAt,
					retryReason: error,
				};
				if (typeof configuredRetry === "function") {
					delay = configuredRetry(context);
				} else if (typeof configuredRetry === "object" && "nextRetryDelayInMilliseconds" in configuredRetry) {
					delay = configuredRetry.nextRetryDelayInMilliseconds(context);
				} else {
					delay = retryDelays[retryCount] ?? null;
				}
			}
			if (delay !== null) {
				checkDuration(delay, "重连间隔", true);
			}
		} catch (policyError) {
			if (desired && cycle === currentCycle) finishDisconnected(toError(policyError));
			else reportError(policyError);
			return;
		}
		if (!desired || cycle !== currentCycle) return;
		if (delay === null) {
			finishDisconnected(error);
			return;
		}
		retryCount++;
		retryTimer = setTimeout(() => {
			retryTimer = undefined;
			if (desired && cycle === currentCycle) openAttempt();
		}, delay);
	}

	/** 结束单次物理连接，并根据首次失败配置、服务端指令和重连策略决定下一步。 */
	function failAttempt(attempt: SocketAttempt, error: Error, allowReconnect = true): void {
		if (!isCurrent(attempt)) return;
		const currentCycle = cycle;
		retire(attempt, error);
		rejectPending(error);
		// 流订阅器的 error 回调可能主动 stop，不得再继续安排旧周期的重连。
		if (!desired || cycle !== currentCycle) return;
		if (!allowReconnect || (!hasConnected && !options.retryOnInitialFailure) || configuredRetry === false) {
			finishDisconnected(error);
			return;
		}
		if (state !== SignalRSocketState.Reconnecting) {
			state = SignalRSocketState.Reconnecting;
			retryCount = 0;
			retryStartedAt = Date.now();
			for (const handler of [...reconnectingHandlers]) notify(() => handler(error));
		}
		scheduleRetry(error);
	}

	/** 从最近一条有效入站消息重新计时，超时后视为连接失活。 */
	function armServerTimeout(attempt: SocketAttempt): void {
		if (!isCurrent(attempt) || !attempt.handshaken) return;
		clearTimeout(attempt.serverTimer);
		// 仅入站有效消息续期；本机持续发送心跳不能证明服务端仍然在线。
		attempt.serverTimer = setTimeout(() => failAttempt(attempt, new Error("等待服务端消息超时")), serverTimeout);
	}

	/** 空闲发送达到间隔后发送 SignalR Ping，业务发送成功时重新计算间隔。 */
	function armHeartbeat(attempt: SocketAttempt): void {
		if (!isCurrent(attempt) || !attempt.handshaken) return;
		clearTimeout(attempt.heartbeatTimer);
		attempt.heartbeatTimer = setTimeout(() => {
			write(attempt, JSON.stringify({ type: 6 }) + recordSeparator).catch((error: unknown) => failAttempt(attempt, toError(error)));
		}, keepAlive);
	}

	/** 将完整文本帧写入当前 Socket；等待原生回调，连接结束或发送超时则拒绝。 */
	function write(attempt: SocketAttempt, data: string): Promise<void> {
		const socket = attempt.socket;
		if (!isCurrent(attempt) || !socket) return Promise.reject(new Error("连接不可用"));
		return new Promise((resolve, reject) => {
			let timer: ReturnType<typeof setTimeout> | undefined;
			const finish = (error?: Error) => {
				if (!attempt.writes.delete(finish)) return;
				clearTimeout(timer);
				if (error) {
					reject(error);
				} else {
					armHeartbeat(attempt);
					resolve();
				}
			};
			timer = setTimeout(() => finish(new Error("WebSocket 发送超时")), sendTimeout);
			attempt.writes.add(finish);
			try {
				socket.send({ data, success: () => finish(), fail: () => finish(new Error("WebSocket 发送失败")) });
			} catch (error) {
				finish(toError(error));
			}
		});
	}

	/** 分发服务端事件；带 invocationId 的调用由唯一处理器返回客户端执行结果。 */
	function dispatchInvocation(attempt: SocketAttempt, message: Extract<SignalRHubMessage, { type: 1 }>): void {
		if (onEvent) notify(() => (isCurrent(attempt) ? onEvent(message.target, message.arguments) : undefined));
		const key = message.target.toLowerCase();
		const registered = [...(handlers.get(key) ?? [])];
		// 普通事件和客户端返回值调用共用 once 的移除逻辑。
		const invokeHandler = (registration: RegisteredHandler) => {
			if (registration.once) {
				// 执行前移除，防止同步回调触发下一条消息时再次进入一次性处理器。
				const current = handlers.get(key);
				if (!current?.has(registration)) return undefined;
				current?.delete(registration);
				if (!current?.size) handlers.delete(key);
			}
			return registration.handler(...message.arguments);
		};
		if (message.invocationId === undefined) {
			for (const handler of registered) notify(() => (isCurrent(attempt) ? invokeHandler(handler) : undefined));
			return;
		}

		// 服务端要求客户端返回值时，只允许一个处理器承担结果。
		const reply = async () => {
			if (!isCurrent(attempt)) return;
			let response: { type: 3; invocationId: string; result?: unknown; error?: string };
			try {
				if (registered.length !== 1) throw new Error("客户端返回值调用需要且只能注册一个处理器");
				const handler = registered[0];
				if (!handler) return;
				const result = await invokeHandler(handler);
				response = { type: 3, invocationId: message.invocationId ?? "", result };
				// 在结果不可序列化时，仍向服务端返回明确的执行错误。
				JSON.stringify(response);
			} catch (error) {
				response = { type: 3, invocationId: message.invocationId ?? "", error: toError(error).message };
			}
			if (isCurrent(attempt)) await write(attempt, JSON.stringify(response) + recordSeparator);
		};
		reply().catch((error: unknown) => failAttempt(attempt, toError(error)));
	}

	/** 按记录分隔符拆包；先完成协议握手，再路由事件、调用结果和流元素。 */
	function receive(attempt: SocketAttempt, data: unknown): void {
		if (!isCurrent(attempt)) return;
		if (typeof data !== "string") {
			failAttempt(attempt, new Error("SignalR JSON 协议要求文本消息"), false);
			return;
		}
		attempt.buffer += data;
		try {
			let boundary: number;
			while (isCurrent(attempt) && (boundary = attempt.buffer.indexOf(recordSeparator)) !== -1) {
				const record = attempt.buffer.slice(0, boundary);
				attempt.buffer = attempt.buffer.slice(boundary + 1);
				if (!record) continue;
				const value: unknown = JSON.parse(record);

				if (!attempt.handshaken) {
					if (!isRecord(value) || "type" in value || (value.error !== undefined && typeof value.error !== "string")) {
						throw new Error("SignalR 握手响应无效");
					}
					if (typeof value.error === "string") throw new Error(value.error);
					clearTimeout(attempt.connectTimer);
					clearTimeout(attempt.handshakeTimer);
					attempt.handshaken = true;
					const reconnected = state === SignalRSocketState.Reconnecting;
					hasConnected = true;
					state = SignalRSocketState.Connected;
					retryCount = 0;
					armServerTimeout(attempt);
					armHeartbeat(attempt);
					for (const waiter of startWaiters) waiter.resolve();
					startWaiters.clear();
					if (reconnected) {
						for (const handler of [...reconnectedHandlers]) notify(() => (isCurrent(attempt) ? handler() : undefined));
					}
					continue;
				}

				const message = parseHubMessage(value);
				armServerTimeout(attempt);
				switch (message.type) {
					case 1:
						dispatchInvocation(attempt, message);
						break;
					case 2: {
						const stream = streams.get(message.invocationId);
						if (stream) stream.next(message.item);
						else rejectCall(message.invocationId, new Error("普通调用不能接收流式结果，请使用 stream"));
						break;
					}
					case 3: {
						const stream = streams.get(message.invocationId);
						if (stream) {
							const error =
								message.error !== undefined
									? new Error(message.error)
									: "result" in message
										? new Error("流式调用不能返回单个结果，请使用 invoke")
										: undefined;
							stream.complete(error);
							break;
						}
						const call = pending.get(message.invocationId);
						if (!call) break;
						clearTimeout(call.timer);
						pending.delete(message.invocationId);
						if (message.error !== undefined) call.reject(new Error(message.error));
						else call.resolve(message.result);
						break;
					}
					case 6:
						break;
					case 7:
						failAttempt(attempt, new Error(message.error ?? "服务端关闭连接"), message.allowReconnect === true);
						break;
				}
			}
		} catch (error) {
			failAttempt(attempt, toError(error), false);
		}
	}

	/** 创建新的物理连接尝试，重新求值地址和 Token，并为握手注册独立回调。 */
	function openAttempt(): void {
		const currentCycle = cycle;
		const attempt: SocketAttempt = {
			buffer: "",
			handshaken: false,
			retired: false,
			nativeClosed: false,
			writes: new Set(),
		};
		active = attempt;
		attempt.connectTimer = setTimeout(() => failAttempt(attempt, new Error("WebSocket 连接超时")), connectTimeout);
		const prepare = async () => {
			// 先等旧连接关闭，避免重连期间同时存在两个有效 Socket。
			await Promise.all([...closingSockets]);
			if (!isCurrent(attempt) || cycle !== currentCycle) return;
			const address = typeof url === "function" ? await url() : url;
			if (!isCurrent(attempt) || cycle !== currentCycle) return;
			const token = await options.accessTokenFactory?.();
			if (!isCurrent(attempt) || cycle !== currentCycle) return;
			const socketUrl = createSocketUrl(address, token);
			const socket = uni.connectSocket({
				url: socketUrl,
				header: options.headers,
				fail: () => failAttempt(attempt, new Error("WebSocket 连接失败")),
			});
			attempt.socket = socket;
			socket.onClose(() => {
				attempt.nativeClosed = true;
				attempt.finishClose?.();
				if (isCurrent(attempt)) failAttempt(attempt, new Error("WebSocket 连接已断开"));
			});
			if (!isCurrent(attempt)) {
				closeNative(attempt);
				return;
			}
			socket.onOpen(() => {
				if (!isCurrent(attempt)) return;
				clearTimeout(attempt.connectTimer);
				attempt.handshakeTimer = setTimeout(() => failAttempt(attempt, new Error("SignalR 握手超时")), handshakeTimeout);
				write(attempt, JSON.stringify({ protocol: "json", version: 1 }) + recordSeparator).catch((error: unknown) =>
					failAttempt(attempt, toError(error))
				);
			});
			socket.onMessage(({ data }) => receive(attempt, data));
			socket.onError(() => failAttempt(attempt, new Error("WebSocket 连接错误")));
		};
		prepare().catch((error: unknown) => failAttempt(attempt, toError(error)));
	}

	/** 获取已完成握手的有效连接；业务调用在连接中、重连中和关闭中直接失败。 */
	function getConnectedAttempt(): SocketAttempt {
		if (state !== SignalRSocketState.Connected || !active?.handshaken || active.retired) throw new Error("SignalR 尚未连接");
		return active;
	}

	/** 编码普通调用；不带 invocationId 时表示无需等待执行结果的 send。 */
	function encodeInvocation(target: string, args: unknown[], invocationId?: string): string {
		if (!target.trim() || !Array.isArray(args)) throw new Error("Hub 方法名或参数无效");
		return JSON.stringify({ type: 1, target, arguments: args, invocationId }) + recordSeparator;
	}

	/** 将参数数组、独立超时与待完成调用登记在同一条发送路径中。 */
	function invokeWithOptions<T = unknown>(target: string, args: unknown[], callOptions: SignalRInvocationOptions): Promise<T>;
	async function invokeWithOptions(target: string, args: unknown[], callOptions: SignalRInvocationOptions): Promise<unknown> {
		const attempt = getConnectedAttempt();
		const timeout = checkDuration(callOptions.timeoutInMilliseconds ?? invocationTimeout, "调用超时");
		const invocationId = String(++sequence);
		const data = encodeInvocation(target, args, invocationId);
		return new Promise<unknown>((resolve, reject) => {
			const timer = setTimeout(() => rejectCall(invocationId, new Error(`调用 ${target} 超时`)), timeout);
			// 必须先登记再发送，底层发送回调之前也可能收到服务端 Completion。
			pending.set(invocationId, { resolve, reject, timer });
			write(attempt, data).catch((error: unknown) => {
				rejectCall(invocationId, toError(error));
				failAttempt(attempt, toError(error));
			});
		});
	}

	/** 将 SDK 形式的可变参数转为协议 arguments，并使用连接级默认调用超时。 */
	function invoke<T = unknown>(target: string, ...args: unknown[]): Promise<T>;
	function invoke(target: string, ...args: unknown[]): Promise<unknown> {
		return invokeWithOptions(target, args, {});
	}

	/** 创建立即发送的结果流；订阅器共享一次服务端调用，最后一个取消时通知服务端。 */
	function stream<T = unknown>(target: string, ...args: unknown[]): SignalRStreamResult<T>;
	function stream(target: string, ...args: unknown[]): SignalRStreamResult<unknown> {
		const attempt = getConnectedAttempt();
		if (!target.trim()) throw new Error("Hub 方法名无效");
		const invocationId = String(++sequence);
		const data = JSON.stringify({ type: 4, target, arguments: args, invocationId }) + recordSeparator;
		const subscribers = new Set<{ subscriber: SignalRStreamSubscriber<unknown>; isActive: boolean }>();
		let isEnded = false;
		let isCanceled = false;
		let terminalError: Error | undefined;
		let hasReportedError = false;

		// 终态保留给迟到订阅者；缺少 error 处理器时，每条流只上报一次错误。
		const notifyEnd = (subscriber: SignalRStreamSubscriber<unknown>) => {
			const error = terminalError;
			if (error) {
				const handleError = subscriber.error;
				if (handleError) {
					notify(() => handleError(error));
				} else if (!hasReportedError) {
					hasReportedError = true;
					reportError(error);
				}
			} else {
				const handleComplete = subscriber.complete;
				if (handleComplete) notify(() => handleComplete());
			}
		};
		// 先标记终结并摘除等待项，再通知订阅者，避免完成回调重入造成重复通知。
		const complete = (error?: Error) => {
			if (isEnded) return;
			isEnded = true;
			terminalError = error;
			streams.delete(invocationId);
			const current = [...subscribers];
			subscribers.clear();
			if (error && !current.length) {
				hasReportedError = true;
				reportError(error);
			}
			for (const entry of current) {
				if (!entry.isActive) continue;
				entry.isActive = false;
				notifyEnd(entry.subscriber);
			}
		};
		streams.set(invocationId, {
			next: (item) => {
				for (const entry of [...subscribers]) {
					const handleNext = entry.subscriber.next;
					if (entry.isActive && handleNext) notify(() => handleNext(item));
				}
			},
			complete,
		});
		write(attempt, data).catch((error: unknown) => failAttempt(attempt, toError(error)));

		return {
			subscribe(subscriber) {
				const entry = { subscriber, isActive: !isEnded };
				if (isEnded) {
					if (!isCanceled) notifyEnd(subscriber);
				} else {
					subscribers.add(entry);
				}
				return {
					dispose() {
						if (!entry.isActive) return;
						entry.isActive = false;
						subscribers.delete(entry);
						if (isEnded || subscribers.size) return;
						// 先终结本地状态，再取消服务端流；迟到元素和重复 dispose 均不再生效。
						isEnded = true;
						isCanceled = true;
						streams.delete(invocationId);
						if (isCurrent(attempt)) {
							write(attempt, JSON.stringify({ type: 5, invocationId }) + recordSeparator).catch((error: unknown) =>
								failAttempt(attempt, toError(error))
							);
						}
					},
				};
			},
		};
	}

	/** 按不区分大小写的名称保存原函数引用，同一事件内按函数身份去重。 */
	function registerHandler(target: string, handler: SignalRMessageHandler, once: boolean): void {
		if (!target.trim() || typeof handler !== "function") throw new Error("事件名称或处理器无效");
		const key = target.toLowerCase();
		const registered = handlers.get(key) ?? new Set<RegisteredHandler>();
		if (![...registered].some((entry) => entry.handler === handler)) registered.add({ handler, once });
		handlers.set(key, registered);
	}

	/** 注册持续监听；泛型只描述业务合同，不在此处转换或校验 DTO。 */
	function on<TArgs extends unknown[]>(target: string, handler: SignalRMessageHandler<TArgs>): void;
	function on(target: string, handler: SignalRMessageHandler): void {
		registerHandler(target, handler, false);
	}

	/** 注册一次性监听，执行前移除以保证同步重入时最多触发一次。 */
	function once<TArgs extends unknown[]>(target: string, handler: SignalRMessageHandler<TArgs>): void;
	function once(target: string, handler: SignalRMessageHandler): void {
		registerHandler(target, handler, true);
	}

	/** 通过原函数引用移除普通或一次性监听；省略处理器时清空该事件。 */
	function off<TArgs extends unknown[]>(target: string, handler?: SignalRMessageHandler<TArgs>): void;
	function off(target: string, handler?: SignalRMessageHandler): void {
		const key = target.toLowerCase();
		if (!handler) {
			handlers.delete(key);
			return;
		}
		const registered = handlers.get(key);
		for (const entry of registered ?? []) {
			if (entry.handler === handler) registered?.delete(entry);
		}
		if (!registered?.size) handlers.delete(key);
	}

	const client: SignalRSocket = {
		get state() {
			return state;
		},
		get keepAliveIntervalInMilliseconds() {
			return keepAlive;
		},
		set keepAliveIntervalInMilliseconds(value) {
			keepAlive = checkDuration(value, "心跳间隔");
			if (active) armHeartbeat(active);
		},
		get serverTimeoutInMilliseconds() {
			return serverTimeout;
		},
		set serverTimeoutInMilliseconds(value) {
			serverTimeout = checkDuration(value, "服务端超时");
			if (active) armServerTimeout(active);
		},
		start() {
			if (state === SignalRSocketState.Connected) return Promise.resolve();
			if (state === SignalRSocketState.Disconnecting) return Promise.reject(new Error("请等待 stop 完成后再 start"));
			return new Promise((resolve, reject) => {
				startWaiters.add({ resolve, reject });
				if (state === SignalRSocketState.Connecting || state === SignalRSocketState.Reconnecting) return;
				desired = true;
				hasConnected = false;
				closeNotified = false;
				cycle++;
				state = SignalRSocketState.Connecting;
				openAttempt();
			});
		},
		stop() {
			if (stopPromise !== undefined) return stopPromise;
			if (state === SignalRSocketState.Disconnected && !closingSockets.size) return Promise.resolve();
			const error = new Error("连接由客户端停止");
			desired = false;
			cycle++;
			clearRetry();
			state = SignalRSocketState.Disconnecting;
			const task = Promise.resolve()
				.then(() => Promise.all([...closingSockets]))
				.then(() => {
					if (stopPromise === task) stopPromise = undefined;
					finishDisconnected();
				})
				.finally(() => {
					if (stopPromise === task) stopPromise = undefined;
				});
			stopPromise = task;
			// 先建立关闭锁再触发清理回调，避免订阅器或 onError 中再次 stop 时重复关闭。
			if (active) retire(active, error);
			rejectStarting(error);
			rejectPending(error);
			return task;
		},
		invoke,
		invokeWithOptions,
		stream,
		async send(target, ...args) {
			const attempt = getConnectedAttempt();
			const data = encodeInvocation(target, args);
			try {
				await write(attempt, data);
			} catch (error) {
				failAttempt(attempt, toError(error));
				throw error;
			}
		},
		on,
		once,
		off,
		onreconnecting(handler) {
			reconnectingHandlers.add(handler);
		},
		onreconnected(handler) {
			reconnectedHandlers.add(handler);
		},
		onclose(handler) {
			closeHandlers.add(handler);
		},
		offreconnecting(handler) {
			if (handler) reconnectingHandlers.delete(handler);
			else reconnectingHandlers.clear();
		},
		offreconnected(handler) {
			if (handler) reconnectedHandlers.delete(handler);
			else reconnectedHandlers.clear();
		},
		offclose(handler) {
			if (handler) closeHandlers.delete(handler);
			else closeHandlers.clear();
		},
		close() {
			client.stop().catch(reportError);
		},
	};
	return client;
}

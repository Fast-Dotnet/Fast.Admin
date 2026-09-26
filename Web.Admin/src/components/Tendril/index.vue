<template>
	<canvas ref="canvasRef" id="canvas" />
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, reactive, useTemplateRef } from "vue";
import { parseHexColor, withDefineType } from "@fast-china/utils";

defineOptions({
	name: "Tendril",
});

const props = defineProps({
	/** @description 节点速度的摩擦衰减系数 */
	friction: {
		type: Number,
		default: 0.5,
	},
	/** @description 丝带数量 */
	trails: {
		type: Number,
		default: 20,
	},
	/** @description 每条丝带的节点数量 */
	size: {
		type: Number,
		default: 50,
	},
	/** @description 前一节点速度对后一节点的影响系数 */
	dampening: {
		type: Number,
		default: 0.25,
	},
	/** @description 弹力沿节点链逐步衰减的系数 */
	tension: {
		type: Number,
		default: 0.98,
	},
});

/** 动画画布 */
const canvasRef = useTemplateRef<HTMLCanvasElement>("canvasRef");
/** 画布二维渲染上下文 */
let ctx: CanvasRenderingContext2D = null;
/** 当前动画帧请求标识 */
let animationFrameId: number = null;
let textFont: string;

const state = reactive({
	/** 鼠标当前位置作为丝带目标点 */
	target: {
		x: window.innerWidth / 2,
		y: window.innerHeight / 2,
	},
	/** 所有丝带 */
	tendrils: withDefineType<Tendril[]>([]),
	/** 控制丝带色相变化的振荡器 */
	hue: withDefineType<Oscillator>(),
	/** 动画是否允许继续运行 */
	running: true,
	/** 点击画布时循环展示的文字 */
	socialistCoreValues: ["富强", "民主", "文明", "和谐", "自由", "平等", "公正", "法治", "爱国", "敬业", "诚信", "友善"],
	/** 下一条点击文字的索引 */
	socialistCoreValueIndex: 0,
	/** 正在上浮并淡出的点击文字 */
	floatingTexts: withDefineType<
		{
			text: string;
			x: number;
			y: number;
			opacity: number;
		}[]
	>([]),
});

/** 颜色振荡器，用于动态改变颜色 */
class Oscillator {
	/** 当前相位 */
	phase = 0;
	/** 基础偏移 */
	offset = 0;
	/** 振荡频率 */
	frequency = 0.001;
	/** 振幅 */
	amplitude = 1;
	/** 当前值 */
	value = 0;

	constructor(options: Partial<Oscillator>) {
		this.phase = options.phase ?? 0;
		this.offset = options.offset ?? 0;
		this.frequency = options.frequency ?? 0.001;
		this.amplitude = options.amplitude ?? 1;
	}

	/** 更新当前值 */
	update() {
		this.phase += this.frequency;
		this.value = this.offset + Math.sin(this.phase) * this.amplitude;
		return this.value;
	}
}

/** 节点 */
class TendrilNode {
	x = 0;
	y = 0;
	/** 水平速度 */
	vx = 0;
	/** 垂直速度 */
	vy = 0;
}

/** 一条丝带，由多个节点组成 */
class Tendril {
	/** 弹力系数 */
	spring: number;
	/** 摩擦力 */
	friction: number;
	/** 节点 */
	nodes: TendrilNode[];

	constructor(options: { spring: number }) {
		/* 轻微随机化弹力和摩擦力，使多条丝带的运动轨迹产生差异。 */
		this.spring = options.spring + Math.random() * 0.1 - 0.05;
		this.friction = props.friction + Math.random() * 0.01 - 0.005;
		this.nodes = [];
		for (let i = 0; i < props.size; i++) {
			this.nodes.push(new TendrilNode());
		}
	}

	/** 更新节点位置，根据目标点和上一节点计算运动 */
	update() {
		let spring = this.spring;
		let node = this.nodes[0];
		node.vx += (state.target.x - node.x) * spring;
		node.vy += (state.target.y - node.y) * spring;

		for (let i = 0, n = this.nodes.length; i < n; i++) {
			node = this.nodes[i];
			if (i > 0) {
				const prev = this.nodes[i - 1];
				node.vx += (prev.x - node.x) * spring;
				node.vy += (prev.y - node.y) * spring;
				node.vx += prev.vx * props.dampening;
				node.vy += prev.vy * props.dampening;
			}
			node.vx *= this.friction;
			node.vy *= this.friction;
			node.x += node.vx;
			node.y += node.vy;

			spring *= props.tension;
		}
	}

	/** 绘制丝带曲线 */
	draw() {
		if (!ctx) return;
		let x = this.nodes[0].x;
		let y = this.nodes[0].y;

		ctx.beginPath();
		ctx.moveTo(x, y);

		for (let i = 1, n = this.nodes.length - 2; i < n; i++) {
			const a = this.nodes[i];
			const b = this.nodes[i + 1];
			x = (a.x + b.x) * 0.5;
			y = (a.y + b.y) * 0.5;
			ctx.quadraticCurveTo(a.x, a.y, x, y);
		}

		const a = this.nodes[this.nodes.length - 2];
		const b = this.nodes[this.nodes.length - 1];
		ctx.quadraticCurveTo(a.x, a.y, b.x, b.y);
		ctx.stroke();
		ctx.closePath();
	}
}

/** 初始化所有丝带 */
const reset = () => {
	state.tendrils = [];
	for (let i = 0; i < props.trails; i++) {
		const tendril = new Tendril({ spring: 0.45 + 0.025 * (i / props.trails) });
		/* 新丝带从当前目标点开始，避免首次绘制跨越整个画布。 */
		tendril.nodes.forEach((node) => {
			node.x = state.target.x;
			node.y = state.target.y;
		});
		state.tendrils.push(tendril);
	}
};

/** 更新并绘制丝带与上浮文字，然后安排下一动画帧 */
const loop = () => {
	animationFrameId = null;
	if (!state.running || !ctx) return;

	/* 清除上一帧并使用 lighter 混合模式叠加丝带颜色。 */
	ctx.globalCompositeOperation = "source-over";
	ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

	ctx.globalCompositeOperation = "lighter";
	ctx.strokeStyle = `hsla(${Math.round(state.hue.update())}, 90%, 50%, 0.25)`;
	ctx.lineWidth = 1;

	for (const tendril of state.tendrils) {
		tendril.update();
		tendril.draw();
	}

	/* 更新并绘制点击产生的上浮文字。 */
	ctx.globalCompositeOperation = "source-over";
	ctx.font = textFont;
	ctx.textBaseline = "top";

	for (let i = state.floatingTexts.length - 1; i >= 0; i--) {
		const t = state.floatingTexts[i];
		t.y -= 2.4;
		t.opacity -= 0.005;

		const textColor = getComputedStyle(document.documentElement).getPropertyValue("--el-text-color-regular").trim();
		const { red, green, blue } = parseHexColor(textColor);
		ctx.fillStyle = `rgba(${red},${green},${blue},${t.opacity.toFixed(3)})`;
		ctx.fillText(t.text, t.x, t.y);

		if (t.opacity <= 0) {
			state.floatingTexts.splice(i, 1);
		}
	}

	animationFrameId = requestAnimationFrame(loop);
};

/** 鼠标或触摸移动时更新目标点 */
const mousemove = (event: MouseEvent | TouchEvent) => {
	if ("touches" in event && event.touches.length > 0) {
		state.target.x = event.touches[0].pageX;
		state.target.y = event.touches[0].pageY;
	} else if ("clientX" in event) {
		state.target.x = event.clientX;
		state.target.y = event.clientY;
	}
	event.preventDefault();
};

/** 触摸开始时初始化目标点 */
const touchstart = (event: TouchEvent) => {
	if (event.touches.length === 1) {
		state.target.x = event.touches[0].pageX;
		state.target.y = event.touches[0].pageY;
	}
};

/** 画布尺寸自适应窗口大小 */
const resize = () => {
	ctx.canvas.width = window.innerWidth;
	ctx.canvas.height = window.innerHeight;
};

/** 在首次指针交互时绑定持续事件并启动动画 */
const init = (event: MouseEvent | TouchEvent) => {
	document.removeEventListener("mousemove", init);
	document.removeEventListener("touchstart", init);

	document.addEventListener("mousemove", mousemove);
	document.addEventListener("touchmove", mousemove);
	document.addEventListener("touchstart", touchstart);

	mousemove(event);
	reset();
	if (state.running && animationFrameId === null) loop();
};

/** 页面重新获得焦点时恢复动画 */
const start = () => {
	if (state.running && animationFrameId !== null) return;
	state.running = true;
	loop();
};

/** 页面失去焦点或组件卸载时停止动画帧 */
const stop = () => {
	state.running = false;
	if (animationFrameId !== null) {
		cancelAnimationFrame(animationFrameId);
		animationFrameId = null;
	}
};

/** 在点击位置添加下一条上浮文字 */
const handleClick = (event: PointerEvent) => {
	const rect = canvasRef.value.getBoundingClientRect();

	const x = event.clientX - rect.left;
	const y = event.clientY - rect.top;
	const text = state.socialistCoreValues[state.socialistCoreValueIndex];
	state.socialistCoreValueIndex = (state.socialistCoreValueIndex + 1) % state.socialistCoreValues.length;

	state.floatingTexts.push({ text, x, y, opacity: 1 });

	event.preventDefault();
};

onMounted(() => {
	if (!canvasRef.value) return;

	/* 获取 2D 绘图上下文；不支持时不再初始化动画。 */
	ctx = canvasRef.value.getContext("2d");
	if (!ctx) return;
	textFont = `600 18px ${getComputedStyle(canvasRef.value).fontFamily}`;

	state.running = true;
	state.hue = new Oscillator({
		phase: Math.random() * (Math.PI * 2),
		amplitude: 85,
		frequency: 0.0015,
		offset: 285,
	});

	/* 首次鼠标或触摸交互时再启动动画，减少未交互页面的资源消耗。 */
	document.addEventListener("mousemove", init);
	document.addEventListener("touchstart", init);
	document.body.addEventListener("orientationchange", resize);
	window.addEventListener("resize", resize);
	window.addEventListener("focus", start);
	window.addEventListener("blur", stop);

	/* 初始化画布像素尺寸。 */
	resize();
});

onUnmounted(() => {
	stop();
	document.removeEventListener("mousemove", init);
	document.removeEventListener("touchstart", init);
	document.removeEventListener("mousemove", mousemove);
	document.removeEventListener("touchmove", mousemove);
	document.removeEventListener("touchstart", touchstart);
	document.body.removeEventListener("orientationchange", resize);
	window.removeEventListener("resize", resize);
	window.removeEventListener("focus", start);
	window.removeEventListener("blur", stop);
	ctx = null;
});

defineExpose({
	/** 在指定指针位置添加上浮文字 */
	click: handleClick,
});
</script>

<style scoped lang="scss">
#canvas {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	cursor: none;
}
</style>

/**
 * @qiun/ucharts v2.5.0-20230101 类型声明（社区补充，非官方提供）。
 *
 * 基于 npm 发布包 u-charts.js 的实际导出、实例方法及 v2 配置整理。
 * 适用于 TypeScript、Vue 3、uni-app（含微信小程序）。
 *
 * 注意：本声明仅提供类型检查与智能提示，不会改变 uCharts 的运行行为。
 * @see https://www.ucharts.cn/v2/#/guide/index
 * @see https://www.npmjs.com/package/@qiun/ucharts
 */
declare module "@qiun/ucharts" {
	/** uCharts v2 内置图表类型。 */
	export type UChartsType =
		| "line"
		| "area"
		| "column"
		| "bar"
		| "mount"
		| "mix"
		| "pie"
		| "ring"
		| "rose"
		| "radar"
		| "arcbar"
		| "gauge"
		| "candle"
		| "funnel"
		| "word"
		| "map"
		| "scatter"
		| "bubble";

	/** CSS 颜色字符串（如 #1890ff、rgba(0,0,0,.5)）。 */
	export type UChartsColor = string;

	/** 实线或虚线。 */
	export type UChartsLineType = "solid" | "dash";

	/** 直线或平滑曲线。 */
	export type UChartsCurveType = "straight" | "curve";

	/** 点与图例可用标记形状。 */
	export type UChartsShape = "circle" | "rect" | "triangle" | "diamond" | "line" | "none";

	/** 图表内的二维坐标。 */
	export interface UChartsPosition {
		/** 横坐标（像素）。 */
		x: number;
		/** 纵坐标（像素）。 */
		y: number;
	}

	/** 折线、柱状图的单个可自定义数据点。 */
	export interface UChartsDataPoint {
		/** 实际数值。 */
		value: number;
		/** 当前数据点颜色。 */
		color?: UChartsColor;
		/** 当前数据点文字标签颜色。 */
		textColor?: UChartsColor;
		/** 当前数据点文字字号。 */
		textSize?: number;
		/** 数据点标签内容；具体图表是否支持取决于渲染实现。 */
		name?: string;
	}

	/** 散点图的 X/Y 数值坐标。 */
	export type UChartsScatterPoint = [x: number, y: number];

	/** 气泡图的 X/Y 坐标和气泡大小。 */
	export type UChartsBubblePoint = [x: number, y: number, radius: number];

	/** K 线数据：开盘价、收盘价、最低价、最高价。 */
	export type UChartsCandlePoint = [open: number, close: number, low: number, high: number];

	/** uCharts 接受的数据序列结构。 */
	export type UChartsSeriesData =
		number | null | Array<number | null | UChartsDataPoint | UChartsScatterPoint | UChartsBubblePoint | UChartsCandlePoint>;

	/** 提示框格式化函数；参数按运行时调用顺序保留。 */
	export type UChartsFormatter = (
		item: UChartsSeries | UChartsDataPoint,
		category: string | number | undefined,
		index: number,
		options: UChartsOptions
	) => string;

	/** 图表数据序列；普通图表通常由多个 series 组成。 */
	export interface UChartsSeries<TData extends UChartsSeriesData = UChartsSeriesData> {
		/** 数据系列名称，对应图例名称。 */
		name: string;
		/** 数值或数值数组；饼图通常使用单个数值。 */
		data: TData;
		/** 自定义颜色；未指定时使用全局 color 调色板。 */
		color?: UChartsColor;
		/** 是否显示当前序列；点击图例后会改变此值。 */
		show?: boolean;
		/** 仅在混合图中指定当前序列的类型。 */
		type?: UChartsType | "point";
		/** 图表数据标签颜色。 */
		textColor?: UChartsColor;
		/** 图表数据标签字号（px）。 */
		textSize?: number;
		/** 自定义数据标签格式化。 */
		formatter?: (item: UChartsDataPoint | number, index: number, series: UChartsSeries) => string;
		/** 当前系列绑定的 Y 轴索引，从 0 开始。 */
		index?: number;
		/** 线型：实线或虚线。 */
		lineType?: UChartsLineType;
		/** 虚线的单段长度。 */
		dashLength?: number;
		/** 折线或区域的曲率类型。 */
		style?: UChartsCurveType;
		/** 数据点图形。 */
		pointShape?: UChartsShape;
		/** 图例图形。 */
		legendShape?: UChartsShape;
		/** 混合图中是否额外绘制数据点。 */
		addPoint?: boolean;
		/** 在 Tooltip 中隐藏本系列的图例项。 */
		disableLegend?: boolean;
		/** 区域、图形本身的不透明度。 */
		opacity?: number;
		/** 当前系列的线宽。 */
		width?: number;
		/** 渐变颜色序列索引。 */
		linearIndex?: number;
	}

	/** GeoJSON 地图要素的几何数据。 */
	export interface UChartsMapGeometry {
		/** GeoJSON 几何类型。 */
		type: "Polygon" | "MultiPolygon";
		/** GeoJSON 坐标数组；保留多边形嵌套。 */
		coordinates: number[][][] | number[][][][];
	}

	/** 地图图表使用的 GeoJSON Feature（series 项）。 */
	export interface UChartsMapFeature {
		/** GeoJSON Feature 标识。 */
		type?: "Feature";
		/** 要素属性，如地区名。 */
		properties: { name: string; [key: string]: unknown };
		/** 地区多边形几何信息。 */
		geometry: UChartsMapGeometry;
		/** 地区颜色。 */
		color?: UChartsColor;
		/** 可选数值，用于地区着色或自定义 tooltip。 */
		data?: number;
		/** 是否显示。 */
		show?: boolean;
	}

	/** 圆环图、仪表盘等图表的中心标题。 */
	export interface UChartsTitle {
		/** 标题内容。 */
		name?: string;
		/** 字体大小（px）。 */
		fontSize?: number;
		/** 标题颜色。 */
		color?: UChartsColor;
		/** 水平偏移量（px）。 */
		offsetX?: number;
		/** 垂直偏移量（px）。 */
		offsetY?: number;
		/** 标题位置或对齐方式。 */
		position?: "top" | "center" | "bottom";
	}

	/** X 轴相关配置。 */
	export interface UChartsXAxis {
		/** 类目轴最多显示的标签数量 */
		labelCount?: number;
		/** 是否隐藏 X 轴，默认 false。 */
		disabled?: boolean;
		/** 不绘制 X 轴网格，默认 false。 */
		disableGrid?: boolean;
		/** 是否旋转 X 轴标签，默认 false。 */
		rotateLabel?: boolean;
		/** X 轴标签旋转角度，默认 45。 */
		rotateAngle?: number;
		/** 坐标轴标签颜色，默认 #666666。 */
		fontColor?: UChartsColor;
		/** 坐标轴标签字体大小，默认 13px。 */
		fontSize?: number;
		/** 标签行高，默认 20px。 */
		lineHeight?: number;
		/** 标签上外边距，默认 0。 */
		marginTop?: number;
		/** X 轴划分的刻度数量，默认 5。 */
		splitNumber?: number;
		/** 是否显示辅助刻度线，默认 false。 */
		calibration?: boolean;
		/** 网格线型，默认 solid。 */
		gridType?: UChartsLineType;
		/** 虚线段长度，默认 4px。 */
		dashLength?: number;
		/** 网格线颜色。 */
		gridColor?: UChartsColor;
		/** 是否绘制 X 轴轴线，默认 true。 */
		axisLine?: boolean;
		/** X 轴轴线颜色，默认 #cccccc。 */
		axisLineColor?: UChartsColor;
		/** 数据点在单元格中的对齐方式，默认 center。 */
		boundaryGap?: "center" | "justify";
		/** 开启滚动后屏幕内显示的数据点数量。 */
		itemCount?: number;
		/** 初始滚动对齐方式，默认 left。 */
		scrollAlign?: "left" | "right";
		/** 当前滚动位置；由实例更新维护。 */
		scrollPosition?: "left" | "right" | "current" | number;
		/** X 轴起点值，由渲染器根据数据自动计算。 */
		min?: number;
		/** X 轴终点值，由渲染器根据数据自动计算。 */
		max?: number;
		/** X 轴文字格式化。 */
		format?: (value: string | number, index?: number) => string;
		/** 坐标轴标题。 */
		title?: string;
		/** 坐标轴标题字号，默认 13px。 */
		titleFontSize?: number;
		/** 坐标轴标题颜色，默认 #666666。 */
		titleFontColor?: UChartsColor;
		/** 坐标轴标题水平偏移量。 */
		titleOffsetX?: number;
		/** 坐标轴标题垂直偏移量。 */
		titleOffsetY?: number;
		/** X 轴字体是否加粗。 */
		fontWeight?: string;
	}

	/** 单个 Y 轴；在多 Y 轴场景中用于 yAxis.data。 */
	export interface UChartsYAxisItem {
		/** 左右位置。 */
		position?: "left" | "right";
		/** Y 轴标签最小值；默认根据数据计算。 */
		min?: number;
		/** Y 轴标签最大值；默认根据数据计算。 */
		max?: number;
		/** Y 轴数值格式化。 */
		format?: (value: number) => string | number;
		/** Y 轴是否隐藏。 */
		disabled?: boolean;
		/** 显示宽度（px），也可使用自动计算。 */
		width?: number;
		/** 字号（px）。 */
		fontSize?: number;
		/** 字体颜色。 */
		fontColor?: UChartsColor;
		/** Y 轴轴线颜色。 */
		axisLineColor?: UChartsColor;
		/** Y 轴轴线是否绘制。 */
		axisLine?: boolean;
		/** Y 轴标题。 */
		title?: string;
		/** Y 轴标题字号。 */
		titleFontSize?: number;
		/** Y 轴标题颜色。 */
		titleFontColor?: UChartsColor;
		/** 标题水平偏移。 */
		titleOffsetX?: number;
		/** 标题垂直偏移。 */
		titleOffsetY?: number;
		/** 单个轴的刻度数量。 */
		splitNumber?: number;
		/** Y 轴刻度标签在轴线的哪侧。 */
		textAlign?: "left" | "right" | "center";
		/** 单个轴对应的数据单位。 */
		unit?: string;
		/** Y 轴对齐方式。 */
		calibration?: boolean;
	}

	/** Y 轴全局配置。 */
	export interface UChartsYAxis {
		/** 多 Y 轴配置列表；如需指定 min/max，优先放入此数组中。 */
		data?: UChartsYAxisItem[];
		/** 是否显示 Y 轴标题，默认 false。 */
		showTitle?: boolean;
		/** 是否隐藏 Y 轴，默认 false。 */
		disabled?: boolean;
		/** 是否关闭 Y 轴网格，默认 false。 */
		disableGrid?: boolean;
		/** 网格划分方式，默认 number。 */
		gridSet?: "number" | "max";
		/** Y 轴网格分段数量，默认 5。 */
		splitNumber?: number;
		/** 网格线型，默认 solid。 */
		gridType?: UChartsLineType;
		/** 网格线颜色，默认 #cccccc。 */
		gridColor?: UChartsColor;
		/** 虚线段长度。 */
		dashLength?: number;
		/** 多个 Y 轴之间的距离，默认 10px。 */
		padding?: number;
		/** 轴文字颜色，默认 #666666。 */
		fontColor?: UChartsColor;
		/** 单个 Y 轴时使用的最大值。 */
		max?: number;
		/** 单个 Y 轴时使用的最小值。 */
		min?: number;
		/** 单个 Y 轴时的数值格式化。 */
		format?: (value: number) => string | number;
	}

	/** 图例配置。 */
	export interface UChartsLegend {
		/** 是否显示图例，默认 true。 */
		show?: boolean;
		/** 图例位置，默认 bottom。 */
		position?: "top" | "right" | "bottom" | "left";
		/** 图例项对齐方式，默认 center。 */
		float?: "left" | "right" | "center" | "top" | "bottom";
		/** 图例背景颜色，默认透明。 */
		backgroundColor?: UChartsColor;
		/** 图例边框颜色。 */
		borderColor?: UChartsColor;
		/** 图例边框宽度（px）。 */
		borderWidth?: number;
		/** 内边距，默认 5px。 */
		padding?: number;
		/** 图例区域外边距，默认 5px。 */
		margin?: number;
		/** 图例项之间的间距，默认 10px。 */
		itemGap?: number;
		/** 字体大小。 */
		fontSize?: number;
		/** 行高。 */
		lineHeight?: number;
		/** 字体颜色。 */
		fontColor?: UChartsColor;
		/** 隐藏图例项的颜色，默认 #CECECE。 */
		hiddenColor?: UChartsColor;
		/** 图例项格式化映射；可按名称或索引提供字符串或回调。 */
		formatter?: Record<string, string | ((name: string) => string)>;
	}

	/** Tooltip 中文字项。 */
	export interface UChartsTooltipText {
		/** 显示的文本。 */
		text: string;
		/** 文本或图例标识的颜色。 */
		color?: UChartsColor;
		/** 前置标识类型。 */
		legendShape?: UChartsShape | "auto";
		/** 是否隐藏这一项的图例标识。 */
		disableLegend?: boolean;
	}

	/** Tooltip 全局样式。 */
	export interface UChartsTooltip {
		/** 是否显示 Tooltip 内容框，默认 true。 */
		showBox?: boolean;
		/** 背景色，默认 #000000。 */
		bgColor?: UChartsColor;
		/** 背景透明度，默认 0.7。 */
		bgOpacity?: number;
		/** 边框颜色。 */
		borderColor?: UChartsColor;
		/** 边框宽度。 */
		borderWidth?: number;
		/** 文本颜色，默认 #FFFFFF。 */
		fontColor?: UChartsColor;
		/** 字体大小。 */
		fontSize?: number;
		/** 提示框内边距。 */
		boxPadding?: number;
		/** 图例是否可见。 */
		legendShow?: boolean;
		/** 图例图形样式；auto 表示自动继承 series。 */
		legendShape?: UChartsShape | "auto";
		/** 图例标识大小。 */
		legendWidth?: number;
		/** 行高。 */
		lineHeight?: number;
		/** 定位指示线颜色。 */
		gridColor?: UChartsColor;
		/** 定位指示线类型。 */
		gridType?: UChartsLineType;
		/** 虚线每段长度。 */
		dashLength?: number;
		/** 是否显示横向辅助线（运行时代码拼写为 horizentalLine）。 */
		horizentalLine?: boolean;
		/** 是否显示 X 轴当前数据标签。 */
		xAxisLabel?: boolean;
		/** 是否显示 Y 轴当前数据标签。 */
		yAxisLabel?: boolean;
		/** 标签背景色。 */
		labelBgColor?: UChartsColor;
		/** 标签背景透明度。 */
		labelBgOpacity?: number;
		/** 标签文字颜色。 */
		labelFontColor?: UChartsColor;
		/** 高亮柱子的背景色。 */
		activeBgColor?: UChartsColor;
		/** 高亮柱子的背景透明度。 */
		activeBgOpacity?: number;
		/** Tooltip 左右偏移量。 */
		offsetX?: number;
		/** Tooltip 上下偏移量。 */
		offsetY?: number;
		/** Tooltip 指示线宽度。 */
		gridWidth?: number;
		/** Tooltip 默认提示框的定位位置。 */
		position?: "left" | "right" | "top" | "bottom";
	}

	/** 折线图配置。 */
	export interface UChartsLineExtra {
		/** 线型，默认 straight。 */
		type?: UChartsCurveType;
		/** 折线宽度（px），默认 2。 */
		width?: number;
		/** 是否绘制数据点。 */
		addPoint?: boolean;
		/** 单点的大小（px）。 */
		pointSize?: number;
		/** 图形是否渐变。 */
		linearType?: "none" | "custom" | "opacity";
		/** 渐变色列表。 */
		customColor?: UChartsColor[];
		/** 是否在数据点之间显示辅助区域。 */
		activeType?: "hollow" | "solid";
		/** 折线端点标记类型。 */
		pointShape?: UChartsShape;
	}

	/** 区域图配置。 */
	export interface UChartsAreaExtra extends UChartsLineExtra {
		/** 区域填充透明度，默认约 0.2。 */
		opacity?: number;
		/** 是否同时绘制折线。 */
		addLine?: boolean;
		/** 是否使用渐变填充。 */
		gradient?: boolean;
	}

	/** 柱状图温度计模式。 */
	export interface UChartsColumnMeter {
		/** 温度计外框线宽。 */
		border?: number;
		/** 温度计空白部分的填充颜色。 */
		fillColor?: UChartsColor;
	}

	/** 柱状图配置。 */
	export interface UChartsColumnExtra {
		/** 分组、堆叠或温度计样式。 */
		type?: "group" | "stack" | "meter";
		/** 指定柱子最大宽度（px）。 */
		width?: number;
		/** 同一类别下各柱的间距（px）。 */
		seriesGap?: number;
		/** 不同类别的左右留白（px）。 */
		categoryGap?: number;
		/** 柱子圆角（px）。 */
		barBorderRadius?: number[] | number;
		/** 柱状图渐变模式。 */
		linearType?: "none" | "custom" | "opacity";
		/** 自定义渐变色列表。 */
		customColor?: UChartsColor[];
		/** 渐变方向。 */
		linearOpacity?: number;
		/** 温度计式柱图额外配置。 */
		meter?: UChartsColumnMeter;
		/** 自定义柱状图顶部封口颜色。 */
		activeBgColor?: UChartsColor;
	}

	/** 横向条形图配置。 */
	export interface UChartsBarExtra {
		/** 分组或堆叠。 */
		type?: "group" | "stack";
		/** 条形最大宽度（px）。 */
		width?: number;
		/** 同组数据间距（px）。 */
		seriesGap?: number;
		/** 不同类别之间的留白（px）。 */
		categoryGap?: number;
		/** 条形圆角。 */
		barBorderRadius?: number | number[];
		/** 条形渐变模式。 */
		linearType?: "none" | "custom" | "opacity";
		/** 自定义渐变色。 */
		customColor?: UChartsColor[];
	}

	/** 山峰图配置。 */
	export interface UChartsMountExtra {
		/** 山峰图绘制形状。 */
		type?: "mount" | "triangle" | "bar" | "sharp";
		/** 山峰宽度与占位宽度的比例。 */
		widthRatio?: number;
		/** 图形背景透明度。 */
		opacity?: number;
		/** 山峰顶部文字颜色。 */
		textColor?: UChartsColor;
		/** 山峰顶部文字字号。 */
		textSize?: number;
		/** 是否绘制山峰顶部数据标签。 */
		showLabel?: boolean;
		/** 渐变模式。 */
		linearType?: "none" | "custom" | "opacity";
		/** 自定义渐变色列表。 */
		customColor?: UChartsColor[];
	}

	/** 混合图配置。 */
	export interface UChartsMixExtra {
		/** 混合图中柱图样式。 */
		column?: UChartsColumnExtra;
		/** 混合图中折线样式。 */
		line?: UChartsLineExtra;
		/** 混合图中区域样式。 */
		area?: UChartsAreaExtra;
	}

	/** 饼图共通配置。 */
	export interface UChartsPieExtra {
		/** 高亮扇区透明度。 */
		activeOpacity?: number;
		/** 高亮扇区向外扩张半径（px）。 */
		activeRadius?: number;
		/** 以 3 点钟方向为基准的起始角度偏移（度）。 */
		offsetAngle?: number;
		/** 图形外侧引导线长度（px）。 */
		labelWidth?: number;
		/** 扇区之间是否绘制分割线。 */
		border?: boolean;
		/** 扇区分割线宽度。 */
		borderWidth?: number;
		/** 扇区分割线颜色。 */
		borderColor?: UChartsColor;
		/** 标签格式化。 */
		labelFormat?: (item: UChartsSeries, index: number) => string;
		/** 内圈半径与外圈半径的比例。 */
		innerRadius?: number;
		/** 饼图外圆半径。 */
		radius?: number;
		/** 标签文字最小间隔。 */
		labelDistance?: number;
		/** 绘图中心。 */
		center?: UChartsPosition;
	}

	/** 圆环图额外配置。 */
	export interface UChartsRingExtra extends UChartsPieExtra {
		/** 圆环宽度（px）。 */
		ringWidth?: number;
		/** 圆环中心半径。 */
		innerRadius?: number;
		/** 圆环中心是否显示自定义文本。 */
		centerColor?: UChartsColor;
	}

	/** 南丁格尔玫瑰图配置。 */
	export interface UChartsRoseExtra extends UChartsPieExtra {
		/** area 为面积模式，radius 为半径模式。 */
		type?: "area" | "radius";
		/** 最小绘制半径（px）。 */
		minRadius?: number;
	}

	/** 圆弧进度图配置。 */
	export interface UChartsArcbarExtra {
		/** 默认圆弧或整圆。 */
		type?: "default" | "circle";
		/** 进度线条宽度（px）。 */
		width?: number;
		/** 底层轨道颜色。 */
		backgroundColor?: UChartsColor;
		/** 起始角度，0~2 表示完整一圈。 */
		startAngle?: number;
		/** 结束角度，0~2 表示完整一圈。 */
		endAngle?: number;
		/** 绘制半径（px）。 */
		radius?: number;
		/** 多个圆弧之间的间距（px）。 */
		gap?: number;
		/** 自定义圆心位置。 */
		center?: UChartsPosition;
		/** 线条末端形状。 */
		lineCap?: "butt" | "round" | "square";
		/** 圆弧进度方向是否反转。 */
		direction?: "clockwise" | "counterclockwise";
	}

	/** 仪表盘刻度线配置。 */
	export interface UChartsGaugeSplitLine {
		/** 刻度线径向偏移。 */
		fixRadius?: number;
		/** 大刻度数量。 */
		splitNumber?: number;
		/** 大刻度线长度。 */
		width?: number;
		/** 刻度线颜色。 */
		color?: UChartsColor;
		/** 子刻度数量。 */
		childNumber?: number;
		/** 子刻度线长度。 */
		childWidth?: number;
	}

	/** 仪表盘指针配置。 */
	export interface UChartsGaugePointer {
		/** 指针宽度（px）。 */
		width?: number;
		/** 指针颜色；auto 表示自动获取当前进度颜色。 */
		// eslint-disable-next-line @typescript-eslint/no-redundant-type-constituents -- 显式保留 "auto" 字面量，明确标识其特殊语义。
		color?: UChartsColor | "auto";
	}

	/** 仪表盘配置。 */
	export interface UChartsGaugeExtra {
		/** default 普通仪表盘，progress 进度样式。 */
		type?: "default" | "progress";
		/** 仪表盘轨道宽度（px）。 */
		width?: number;
		/** 标签颜色。 */
		labelColor?: UChartsColor;
		/** 起始角度，0~2 表示一圈。 */
		startAngle?: number;
		/** 结束角度，0~2 表示一圈。 */
		endAngle?: number;
		/** 数值最小值。 */
		startNumber?: number;
		/** 数值最大值。 */
		endNumber?: number;
		/** 刻度值文本格式化。 */
		labelFormat?: (value: number) => string | number;
		/** 仪表盘刻度线。 */
		splitLine?: UChartsGaugeSplitLine;
		/** 仪表盘指针。 */
		pointer?: UChartsGaugePointer;
		/** 默认轨道背景颜色。 */
		backgroundColor?: UChartsColor;
		/** 仪表盘进度的圆角设置。 */
		lineCap?: "round" | "butt" | "square";
	}

	/** 雷达图配置。 */
	export interface UChartsRadarExtra {
		/** 雷达轴最大值；默认依据数据最大值计算。 */
		max?: number;
		/** 各维度标签字体颜色。 */
		labelColor?: UChartsColor;
		/** 坐标网格颜色。 */
		gridColor?: UChartsColor;
		/** 雷达图背景区域透明度。 */
		opacity?: number;
		/** 网格圈数。 */
		gridCount?: number;
		/** 是否显示雷达图区域背景。 */
		fillArea?: boolean;
		/** 雷达图线条宽度。 */
		lineWidth?: number;
		/** 雷达图标签字体大小。 */
		labelFontSize?: number;
	}

	/** K 线图涨跌颜色。 */
	export interface UChartsCandleColor {
		/** 上涨线颜色。 */
		upLine?: UChartsColor;
		/** 上涨实体填充色。 */
		upFill?: UChartsColor;
		/** 下跌线颜色。 */
		downLine?: UChartsColor;
		/** 下跌实体填充色。 */
		downFill?: UChartsColor;
	}

	/** K 线图均线配置。 */
	export interface UChartsCandleAverage {
		/** 是否启用自动均线计算。 */
		show?: boolean;
		/** 均线名称，例如 MA5、MA10。 */
		name?: string[];
		/** 均线统计窗口，例如 5、10、20。 */
		day?: number[];
		/** 均线颜色列表。 */
		color?: UChartsColor[];
	}

	/** K 线图配置。 */
	export interface UChartsCandleExtra {
		/** 蜡烛颜色配置。 */
		color?: UChartsCandleColor;
		/** 均线配置。 */
		average?: UChartsCandleAverage;
		/** K 线蜡烛宽度。 */
		width?: number;
		/** K 线图高亮标签背景色。 */
		activeBgColor?: UChartsColor;
	}

	/** 漏斗图配置。 */
	export interface UChartsFunnelExtra {
		/** 高亮时的透明度。 */
		activeOpacity?: number;
		/** 高亮时向外扩张的宽度（px）。 */
		activeWidth?: number;
		/** 是否绘制各层分割线。 */
		border?: boolean;
		/** 分割线宽度。 */
		borderWidth?: number;
		/** 分割线颜色。 */
		borderColor?: UChartsColor;
		/** 主体不透明度。 */
		fillOpacity?: number;
		/** 标签相对漏斗位置。 */
		labelAlign?: "left" | "right";
		/** 漏斗图绘制位置。 */
		align?: "left" | "center" | "right";
	}

	/** 词云图配置。 */
	export interface UChartsWordExtra {
		/** 词云布局；不同类型的效果依赖 uCharts 内部实现。 */
		type?: "normal" | "vertical" | "tilt" | "random";
		/** 是否自动为每个词随机选色。 */
		autoColors?: boolean;
		/** 字号的最小值。 */
		minFontSize?: number;
		/** 字号的最大值。 */
		maxFontSize?: number;
		/** 词语之间的距离。 */
		padding?: number;
	}

	/** GeoJSON 地图配置。 */
	export interface UChartsMapExtra {
		/** 是否显示行政边界。 */
		border?: boolean;
		/** 边界线宽。 */
		borderWidth?: number;
		/** 边界颜色。 */
		borderColor?: UChartsColor;
		/** 地区填充透明度。 */
		fillOpacity?: number;
		/** 选中地区边界颜色。 */
		activeBorderColor?: UChartsColor;
		/** 选中地区填充颜色。 */
		activeFillColor?: UChartsColor;
		/** 选中地区填充透明度。 */
		activeFillOpacity?: number;
		/** 是否显示地图地区标签。 */
		showLabel?: boolean;
		/** 地区标签颜色。 */
		labelColor?: UChartsColor;
	}

	/** 散点图配置。 */
	export interface UChartsScatterExtra {
		/** 散点大小（px）。 */
		pointSize?: number;
		/** 散点形状。 */
		pointShape?: UChartsShape;
		/** 散点是否描边。 */
		border?: boolean;
		/** 散点描边宽度。 */
		borderWidth?: number;
		/** 散点透明度。 */
		opacity?: number;
	}

	/** 气泡图配置。 */
	export interface UChartsBubbleExtra {
		/** 气泡最小直径（px）。 */
		minSize?: number;
		/** 气泡最大直径（px）。 */
		maxSize?: number;
		/** 气泡透明度。 */
		opacity?: number;
		/** 气泡边框宽度。 */
		borderWidth?: number;
	}

	/** 多图表可用的标记线配置。 */
	export interface UChartsMarkLineData {
		/** 标记线所在的 Y 轴数值。 */
		value: number;
		/** 标记线颜色。 */
		lineColor?: UChartsColor;
		/** 是否展示标记线标签。 */
		showLabel?: boolean;
		/** 标签字体颜色。 */
		labelFontColor?: UChartsColor;
		/** 标签背景色。 */
		labelBgColor?: UChartsColor;
		/** 标签背景透明度。 */
		labelBgOpacity?: number;
		/** 关联 Y 轴索引。 */
		yAxisIndex?: number;
		/** 自定义标签内容。 */
		labelText?: string;
	}

	/** 水平辅助标记线配置。 */
	export interface UChartsMarkLine {
		/** 线型，默认 solid。 */
		type?: UChartsLineType;
		/** 虚线段长度（px）。 */
		dashLength?: number;
		/** 标记线列表。 */
		data?: UChartsMarkLineData[];
		/** 标记线宽度。 */
		width?: number;
	}

	/** 每种图形类型的扩展配置。 */
	export interface UChartsExtra {
		/** 折线图。 */
		line?: UChartsLineExtra;
		/** 区域图。 */
		area?: UChartsAreaExtra;
		/** 柱状图。 */
		column?: UChartsColumnExtra;
		/** 横向条形图。 */
		bar?: UChartsBarExtra;
		/** 山峰图。 */
		mount?: UChartsMountExtra;
		/** 混合图。 */
		mix?: UChartsMixExtra;
		/** 饼图。 */
		pie?: UChartsPieExtra;
		/** 圆环图。 */
		ring?: UChartsRingExtra;
		/** 玫瑰图。 */
		rose?: UChartsRoseExtra;
		/** 圆弧进度图。 */
		arcbar?: UChartsArcbarExtra;
		/** 仪表盘。 */
		gauge?: UChartsGaugeExtra;
		/** 雷达图。 */
		radar?: UChartsRadarExtra;
		/** K 线图。 */
		candle?: UChartsCandleExtra;
		/** 漏斗图。 */
		funnel?: UChartsFunnelExtra;
		/** 词云图。 */
		word?: UChartsWordExtra;
		/** 地图。 */
		map?: UChartsMapExtra;
		/** 散点图。 */
		scatter?: UChartsScatterExtra;
		/** 气泡图。 */
		bubble?: UChartsBubbleExtra;
		/** Tooltip。 */
		tooltip?: UChartsTooltip;
		/** 标记线。 */
		markLine?: UChartsMarkLine;
	}

	/** 触摸或鼠标位置对象（兼容 uni-app、H5 和小程序）。 */
	export interface UChartsTouch {
		/** Canvas 相对坐标 X。 */
		x?: number;
		/** Canvas 相对坐标 Y。 */
		y?: number;
		/** 客户端坐标 X。 */
		clientX?: number;
		/** 客户端坐标 Y。 */
		clientY?: number;
		/** 页面坐标 X。 */
		pageX?: number;
		/** 页面坐标 Y。 */
		pageY?: number;
	}

	/** 图表交互事件；兼容小程序经由 mp 包装的触摸数据。 */
	export interface UChartsTouchEvent {
		/** 变化的触点列表。 */
		changedTouches?: ArrayLike<UChartsTouch>;
		/** 小程序框架包装的触点数据。 */
		mp?: { changedTouches: ArrayLike<UChartsTouch> };
		/** H5 鼠标事件的 X 偏移。 */
		offsetX?: number;
		/** H5 鼠标事件的 Y 偏移。 */
		offsetY?: number;
		/** 设备或 Canvas 标识。 */
		type?: string;
	}

	/** getCurrentDataIndex 在直角坐标系图表中的结果。 */
	export interface UChartsCurrentDataIndex {
		/** 数据下标；部分图表会返回多个数据点索引。 */
		index: number | number[];
		/** 分组数据系列索引。 */
		group?: number;
	}

	/** 用于 showToolTip 的运行时配置。 */
	export interface UChartsShowToolTipOptions {
		/** 指定要高亮的数据下标；不指定时从触点定位。 */
		index?: number | number[];
		/** 覆盖自动生成的 Tooltip 列表。 */
		textList?: UChartsTooltipText[];
		/** 指定 Tooltip 的绘制位置。 */
		offset?: UChartsPosition;
		/** 自定义 Tooltip 值格式化。 */
		formatter?: UChartsFormatter;
		/** Tooltip 文本分组间隔。 */
		itemGap?: number;
		/** 自定义扩展文本。 */
		content?: string;
	}

	/** 在 updateData 中控制滚动位置。 */
	export interface UChartsUpdateDataOptions extends Partial<UChartsOptions> {
		/** 更新时的滚动位置，默认 current。 */
		scrollPosition?: "current" | "left" | "right";
	}

	/** 绘图过程中生成的辅助数据；用于访问图表坐标信息。 */
	export interface UChartsRuntimeChartData {
		/** 横轴已计算坐标。 */
		xAxisPoints?: number[];
		/** 各数据点的计算坐标；图形间结构有区别。 */
		calPoints?: unknown[];
		/** 每个分类占用的宽度（px）。 */
		eachSpacing?: number;
		/** X 轴计算结果。 */
		xAxisData?: { xAxisPoints: number[]; eachSpacing?: number; startX?: number; endX?: number };
		/** Y 轴计算结果，结构由具体图形决定。 */
		yAxisData?: unknown;
		/** 图例计算结果。 */
		legendData?: unknown;
		/** 饼图、圆环图、玫瑰图辅助信息。 */
		pieData?: unknown;
		/** 雷达图辅助信息。 */
		radarData?: unknown;
		/** 漏斗图辅助信息。 */
		funnelData?: unknown;
		/** 词云图辅助信息。 */
		wordCloudData?: unknown;
	}

	/**
	 * 初始化图表的完整配置。
	 *
	 * @remarks
	 * 原生 uCharts v2 要求显式传入 `context`，并且使用画布真实尺寸作为
	 * `width`、`height`。uni-app 端使用 `uni.createCanvasContext(...)` 或
	 * Canvas 2D 节点上下文；这里将 context 声明为 `object`，避免强绑定
	 * DOM 的 CanvasRenderingContext2D，与微信小程序 CanvasContext 不兼容。
	 */
	export interface UChartsOptions {
		/** 图表类型。 */
		type: UChartsType;
		/** Canvas 绘制上下文。 */
		context: object;
		/** 画布宽度（px）。 */
		width: number;
		/** 画布高度（px）。 */
		height: number;
		/** 图表数据序列列表。 */
		series: Array<UChartsSeries | UChartsMapFeature>;
		/** X 轴类别标签；饼图、圆环图等可省略。 */
		categories?: Array<string | number>;
		/** 整体图表配色方案。 */
		color?: UChartsColor[];
		/** 画布背景色。 */
		background?: UChartsColor;
		/** 设备像素比；高分辨率 Canvas2D 通常需要指定。 */
		pixelRatio?: number;
		/** Canvas 2D 节点模式；微信小程序新版 canvas 等适用。 */
		canvas2d?: boolean;
		/** 图表内边距：[上、右、下、左]，默认 [10,10,10,10]。 */
		padding?: [top: number, right: number, bottom: number, left: number];
		/** 整体字体大小，默认 13px。 */
		fontSize?: number;
		/** 整体字体颜色，默认 #666666。 */
		fontColor?: UChartsColor;
		/** 是否启动入场动画；实际源码缺省值为 false。 */
		animation?: boolean;
		/** 动画时长（ms），默认 1000。 */
		duration?: number;
		/** 动画缓动方式，默认 easeInOut。 */
		timing?: "easeIn" | "easeOut" | "easeInOut" | "linear";
		/** 是否旋转图表 90°。 */
		rotate?: boolean;
		/** 是否显示数据值标签，默认由图表实现决定。 */
		dataLabel?: boolean;
		/** 是否显示数据点标记。 */
		dataPointShape?: boolean;
		/** 是否启用图表横向拖动。 */
		enableScroll?: boolean;
		/** 是否绘制标记线，需配合 extra.markLine.data。 */
		enableMarkLine?: boolean;
		/** 拖动时的最高重绘帧率，源码默认约为 60。 */
		touchMoveLimit?: number;
		/** 图表中心标题。 */
		title?: UChartsTitle;
		/** 图表中心副标题。 */
		subtitle?: UChartsTitle;
		/** X 轴配置。 */
		xAxis?: UChartsXAxis;
		/** Y 轴配置。 */
		yAxis?: UChartsYAxis;
		/** 图例配置。 */
		legend?: UChartsLegend;
		/** 具体图表及 Tooltip 的扩展配置。 */
		extra?: UChartsExtra;
		/** 手工提供的 K 线均线；优先使用自动生成或显式传入。 */
		seriesMA?: UChartsSeries[];
		/** 初始化时指定拖动的起始位置（内部兼容属性）。 */
		_scrollDistance_?: number;
		/** 根据 pixelRatio 计算的内部像素比例。 */
		pix?: number;
		/** 图表绘制区域，运行时填充。 */
		area?: number[];
		/** 运行时计算的坐标数据，初始化后由图表填充。 */
		chartData?: UChartsRuntimeChartData;
		/** Tooltip 当前内容（运行时生成）。 */
		tooltip?: {
			/** 提示文本列表。 */
			textList: UChartsTooltipText[];
			/** 绘制位置。 */
			offset: UChartsPosition;
			/** 调用参数。 */
			option?: UChartsShowToolTipOptions;
			/** 数据索引。 */
			index?: number | number[];
			/** 分组索引。 */
			group?: number;
		};
	}

	/** 滑动过程中实例维护的状态。 */
	export interface UChartsScrollOption {
		/** 当前横向偏移量。 */
		currentOffset: number;
		/** 开始触摸时的坐标。 */
		startTouchX: number;
		/** 本次移动的偏移距离。 */
		distance: number;
		/** 最近一次移动时间。 */
		lastMoveTime: number;
		/** 双指缩放开始时的分类点数。 */
		moveCount?: number;
		/** 双指缩放开始时的左端序列索引。 */
		moveCurrent1?: number;
		/** 双指缩放开始时的右端序列索引。 */
		moveCurrent2?: number;
		/** 触边滚动状态。 */
		position?: "left" | "right" | number;
	}

	/** uCharts 内置事件名称。 */
	export type UChartsEventType = "renderComplete" | "scrollLeft" | "scrollRight" | (string & {});

	/** 图表事件处理函数。 */
	export type UChartsEventListener = (...args: unknown[]) => void;

	/** 图表内置事件分发对象。 */
	export interface UChartsEventEmitter {
		/** 注册事件。 */
		addEventListener(type: UChartsEventType, listener: UChartsEventListener): void;
		/** 删除指定类型的全部监听器。 */
		delEventListener(type: UChartsEventType): void;
		/** 手工触发事件。 */
		trigger(type: UChartsEventType, ...args: unknown[]): void;
	}

	/** 运行时动画实例。 */
	export interface UChartsAnimationInstance {
		/** 终止当前动画。 */
		stop(): void;
	}

	/**
	 * uCharts v2 原生图表实例。
	 *
	 * @example
	 * ```ts
	 * import uCharts from '@qiun/ucharts';
	 * import type { UChartsOptions } from '@qiun/ucharts';
	 *
	 * const options: UChartsOptions = {
	 *   type: 'line', context, width: 320, height: 220,
	 *   categories: ['周一', '周二', '周三'],
	 *   series: [{ name: '预约人数', data: [12, 18, 25] }],
	 *   extra: { line: { type: 'curve', width: 2 } },
	 * };
	 * const chart = new uCharts(options);
	 * chart.updateData({ series: [{ name: '预约人数', data: [13, 19, 26] }] });
	 * ```
	 */
	export default class uCharts {
		/** 创建图表并立即绘制。 */
		constructor(options: UChartsOptions);

		/** 当前图表配置（部分字段会由 uCharts 在运行时修改）。 */
		opts: UChartsOptions;
		/** 使用的 Canvas 绘图上下文。 */
		context: object;
		/** 当前图表滚动状态。 */
		scrollOption: UChartsScrollOption;
		/** 底层事件管理器。 */
		uevent: UChartsEventEmitter;
		/** 当前动画对象，动画初始化后可用。 */
		animationInstance?: UChartsAnimationInstance;
		/** uCharts 内部计算配置；避免对未公开结构做错误强类型假设。 */
		config: Record<string, unknown>;

		/**
		 * 合并新配置并重绘图表；支持更新 categories、series、extra 等选项。
		 * @param data 需要更新的部分配置及滚动位置。
		 */
		updateData(data?: UChartsUpdateDataOptions): void;

		/**
		 * 设置每屏显示的数据点个数；要求初始化时启用 enableScroll。
		 * @param options 目标数据点个数。
		 */
		zoom(options: { itemCount: number }): void;

		/**
		 * 双指缩放图表。
		 * @remarks 该方法在原始源码中拼写为 `dobuleZoom`，此处忠实保留。
		 * @param event 包含至少两个 changedTouches 的触摸事件。
		 */
		dobuleZoom(event: UChartsTouchEvent): void;

		/** 终止进行中的绘图动画。 */
		stopAnimation(): void;

		/**
		 * 注册事件；常用事件：renderComplete、scrollLeft、scrollRight。
		 * @param type 事件名称。
		 * @param listener 监听器。
		 */
		addEventListener(type: UChartsEventType, listener: UChartsEventListener): void;

		/** 删除某一事件类型的所有监听器。 */
		delEventListener(type: UChartsEventType): void;

		/**
		 * 根据触摸坐标查找所命中的数据点。
		 * @returns 饼图等返回数字索引；直角坐标系通常返回包含 index 的对象；未命中返回 -1。
		 */
		getCurrentDataIndex(event: UChartsTouchEvent): number | UChartsCurrentDataIndex;

		/**
		 * 根据触摸位置判断命中的图例项。
		 * @returns 图例项的索引，未命中时返回 -1。
		 */
		getLegendDataIndex(event: UChartsTouchEvent): number;

		/**
		 * 切换点击的图例项的显示状态并重绘。
		 * @param event 点击图例区域的触摸事件。
		 * @param options 是否为此次重绘启用动画。
		 */
		touchLegend(event: UChartsTouchEvent, options?: { animation?: boolean }): void;

		/**
		 * 根据触摸位置显示当前数据点 Tooltip。
		 * @param event 交互事件，需包含变化的触摸坐标。
		 * @param options 指定索引、覆盖提示项或自定义提示文案。
		 */
		showToolTip(event: UChartsTouchEvent, options?: UChartsShowToolTipOptions): void;

		/**
		 * 将图表直接滚动到指定偏移量。
		 * @param distance 横向偏移量（像素）。
		 */
		translate(distance: number): void;

		/** 拖动开始，记录初始触点位置。 */
		scrollStart(event: UChartsTouchEvent): void;

		/**
		 * 根据触摸位置滚动图表。
		 * @returns 实际滚动位置；滚动未生效时返回 undefined。
		 */
		scroll(event: UChartsTouchEvent): number | undefined;

		/** 拖动结束，结算当前滚动距离。 */
		scrollEnd(event: UChartsTouchEvent): void;
	}
}

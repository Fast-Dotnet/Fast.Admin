import { Fragment, computed, defineComponent, onMounted, shallowReactive, shallowRef } from "vue";
import { ElDropdownItem, ElMessage, ElMessageBox, dayjs } from "element-plus";
import { FaTable, faTableEmits, faTableProps } from "fast-element-plus";
import { debounce, makeSlots, useEmits, useExpose, useProps, useRender, useWindowSize, withDefineType } from "@fast-china/utils";
import { tableApi } from "@/api/services/Center/table";
import { useApp, useConfig } from "@/stores";
import type { FaTableColumnCtx, FaTableInstance, FaTableSlots } from "fast-element-plus";

export default defineComponent({
	name: "FastTable",
	props: {
		...faTableProps,
		/** 是否显示列配置按钮 */
		columnSettingBtn: {
			type: Boolean,
			default: true,
		},
	},
	emits: {
		...faTableEmits,
	},
	slots: makeSlots<FaTableSlots>(),
	setup(props, { slots, emit, expose }) {
		// FastTable 基础组件实例
		const faTableRef = shallowRef<FaTableInstance | null>(null);

		const appStore = useApp();
		const configStore = useConfig();
		const { width: windowWidth } = useWindowSize();

		const state = shallowReactive({
			/** 是否正在执行列配置请求 */
			loading: false,
			/** 当前列配置请求的加载提示 */
			loadingText: "加载中...",
			/** 表格列 */
			columns: withDefineType<FaTableColumnCtx[]>([]),
			/** 是否存在缓存列 */
			existCacheColumns: false,
		});
		// 是否需要取消业务列固定以适配小屏幕
		const isSmallScreen = computed(() => windowWidth.value < 1200);

		/** 后端列配置附带的动态属性元数据；type 取值为 1 字符串、2 数字、4 Boolean、8 方法 */
		type FaTableColumnLocalCtx = {
			/** 列展示属性的动态配置 */
			otherAdvancedConfig?: { prop: string; type: number }[];
			/** 列搜索属性的动态配置 */
			searchAdvancedConfig?: { prop: string; type: number }[];
		};

		// 将后端列配置中的函数字符串恢复为运行时回调
		const handleColumnType = (localColumns: (FaTableColumnCtx & FaTableColumnLocalCtx)[]) => {
			// 提取受支持箭头函数字符串中的参数和函数体
			const handleFunctionArgs = (
				functionStr: string
			): {
				args: string[];
				body: string;
			} => {
				// 去掉字符串开头和结尾的多余空白字符，包括换行符
				const trimmedStr = functionStr.trim();

				// 正则表达式用于匹配箭头函数的参数和函数体，包括支持解构参数
				const arrowFunctionMatch =
					trimmedStr.match(/^\(([^)]*)\)\s*=>\s*\{([\s\S]*)\};?$/) ?? trimmedStr.match(/^([A-Za-z_$][\w$]*)\s*=>\s*\{([\s\S]*)\};?$/);

				if (arrowFunctionMatch) {
					const args = arrowFunctionMatch[1]
						.split(",")
						.map((arg) => arg.trim())
						.filter((arg) => arg);
					const body = arrowFunctionMatch[2].trim();
					return { args, body };
				}

				return { args: [], body: "" };
			};

			for (const column of localColumns) {
				if (column.otherAdvancedConfig?.length) {
					column.otherAdvancedConfig
						.filter((f) => f.type === 8)
						.forEach((advKey: { prop: string; type: number }) => {
							const { args, body } = handleFunctionArgs((column as Record<string, string>)[advKey.prop]);
							// eslint-disable-next-line no-new-func, @typescript-eslint/no-implied-eval, @typescript-eslint/no-unsafe-function-type -- 后端表格配置允许保存函数体，此处必须恢复为回调函数。
							(column as Record<string, Function>)[advKey.prop] = new Function(...args, body);
						});
					delete column.otherAdvancedConfig;
				}
				if (column.searchAdvancedConfig?.length) {
					column.searchAdvancedConfig
						.filter((f) => f.type === 8)
						.forEach((advKey: { prop: string; type: number }) => {
							const { args, body } = handleFunctionArgs(column.search.props[advKey.prop]);
							// eslint-disable-next-line no-new-func, @typescript-eslint/no-implied-eval -- 后端表格配置允许保存函数体，此处必须恢复为回调函数。
							column.search.props[advKey.prop] = new Function(...args, body);
						});
					delete column.searchAdvancedConfig;
				}
			}

			return localColumns;
		};

		// 将系统最新列配置同步到当前用户缓存
		const syncColumnsCache = (showConfirm = true) => {
			if (props.columns) return;

			function localRequest() {
				faTableRef.value.doLoading(async () => {
					try {
						await tableApi.syncUserTableConfig({
							tableKey: props.tableKey,
						});
						appStore.deleteTableColumns(props.tableKey);
						ElMessage.success("同步成功");
						// eslint-disable-next-line no-use-before-define -- 回调在初始化完成后执行，届时列加载方法已定义
						await loadTableColumns();
					} catch (error) {
						ElMessage.error("同步失败");
						throw error;
					}
				}, "同步列配置中...");
			}

			if (showConfirm) {
				ElMessageBox.confirm("确认同步列缓存配置？此操作无法撤销。", {
					type: "warning",
					beforeClose() {
						localRequest();
					},
				});
			} else {
				localRequest();
			}
		};

		// 清除当前用户的表格列缓存并重新加载配置
		const clearColumnsCache = () => {
			if (props.columns) return;
			ElMessageBox.confirm("确认重置列缓存配置？此操作无法撤销。", {
				type: "warning",
			}).then(async () => {
				await faTableRef.value.doLoading(async () => {
					try {
						await tableApi.clearUserTableConfig({ tableKey: props.tableKey });
						appStore.deleteTableColumns(props.tableKey);
						ElMessage.success("重置成功");
						// eslint-disable-next-line no-use-before-define -- 回调在初始化完成后执行，届时列加载方法已定义
						await loadTableColumns();
					} catch (error) {
						ElMessage.error("重置列配置失败");
						throw error;
					}
				}, "重置列配置中...");
			});
		};

		// 保存当前用户调整后的表格列配置
		const saveColumnsCache = async (columns: FaTableColumnCtx[]) => {
			if (props.columns) return;
			const findSourceColumn = (columnId?: number | string, sourceColumns = state.columns): FaTableColumnCtx | undefined => {
				for (const column of sourceColumns) {
					if (column.columnId === columnId) return column;
					const childColumn = findSourceColumn(columnId, column._children);
					if (childColumn) return childColumn;
				}
				return undefined;
			};
			await faTableRef.value.doLoading(async () => {
				try {
					await tableApi.saveUserTableConfig({
						tableKey: props.tableKey,
						columns: columns.map((m) => {
							const fixed = isSmallScreen.value ? (findSourceColumn(m.columnId)?.fixed ?? m.fixed) : m.fixed;
							return {
								columnId: m.columnId?.toString(),
								label: m.label,
								fixed: typeof fixed === "string" ? fixed : "",
								autoWidth: m.autoWidth,
								width: Number(m.width),
								smallWidth: Number(m.smallWidth),
								order: m.order,
								show: m.show,
								copy: m.copy,
								sortable: m.sortable,
								searchLabel: m.search?.label,
								searchOrder: m.search?.order,
							};
						}),
					});
					/* 服务端缓存保存成功后，使应用级列缓存失效，后续重新获取最新配置。 */
					state.existCacheColumns = true;
					appStore.setTableColumns(props.tableKey, columns);
					appStore.deleteTableColumns(props.tableKey);
					ElMessage.success("保存列配置成功");
				} catch (error) {
					ElMessage.error("保存列配置失败");
					throw error;
				}
			}, "保存列配置中...");
		};

		// 递归复制列配置，并将字典名称解析为字典项
		const handleColumns = (columns: FaTableColumnCtx[]) => {
			return columns.map((col) => {
				const result = { ...col };

				if (result._children?.length) {
					result._children = handleColumns(result._children);
				}

				if (result.enum && typeof result.enum === "string") {
					const enumDict = appStore.getDictionary(result.enum);
					if (enumDict) {
						result.enum = enumDict;
					}
				}

				return result;
			});
		};

		// 小屏取消业务列固定，组件库内置列保持原有固定行为
		const responsiveColumns = computed(() => {
			if (!isSmallScreen.value) return state.columns;

			const componentColumnTypes = new Set(["selection", "index", "expand"]);
			const cancelBusinessColumnFixed = (columns: FaTableColumnCtx[]): FaTableColumnCtx[] =>
				columns.map((column) => ({
					...column,
					...(!componentColumnTypes.has(column.type ?? "") && { fixed: false }),
					...(column._children?.length && { _children: cancelBusinessColumnFixed(column._children) }),
				}));

			return cancelBusinessColumnFixed(state.columns);
		});

		// 优先加载传入列或本地缓存，否则请求服务端列配置
		const loadTableColumns = async () => {
			let columns: FaTableColumnCtx[];
			if (props.columns) {
				columns = props.columns;
			} else {
				const cacheColumns = appStore.getTableColumns(props.tableKey, false);
				if (cacheColumns) {
					columns = cacheColumns;
				} else {
					state.loading = true;
					state.loadingText = "加载列配置中...";
					try {
						const apiRes = await tableApi.queryTableColumnConfig(props.tableKey);
						/* 服务端列定义变化时，先同步用户缓存，避免继续使用过期列配置。 */
						if (apiRes.change) {
							const lastUpdateTime = apiRes.updatedTime ?? new Date();
							ElMessage.info(
								`当前列配置于 '${dayjs(lastUpdateTime).format("YYYY-MM-DD")}' 已发生改变，为确保数据准确性，正在同步缓存配置，请稍后...`
							);
							syncColumnsCache(false);
							return;
						} else {
							state.existCacheColumns = apiRes.cache;
							columns = handleColumnType(apiRes.cache ? apiRes.cacheColumns : apiRes.columns);
							appStore.setTableColumns(props.tableKey, columns);
						}
					} catch (error) {
						ElMessage.error("加载列配置失败");
						throw error;
					} finally {
						state.loading = false;
					}
				}
			}

			state.columns = handleColumns(columns);
		};

		// 清空列状态并重新加载、渲染表格
		const doRender = () => {
			state.columns = [];
			debounce(async () => {
				await loadTableColumns();
				await faTableRef.value?.doRender();
			}, 300);
		};

		onMounted(async () => {
			await loadTableColumns();
		});

		// 透传给基础 FaTable 的属性
		const tableProps = useProps(props, faTableProps, [
			"columns",
			"hideImage",
			"collapsedSearch",
			"advancedSearchDrawer",
			"dataSearchRange",
			"columnSettingBtn",
			"columnsChange",
		]);

		// 透传基础 FaTable 事件
		const tableEmits = useEmits(faTableEmits, emit);

		// 在基础列设置菜单中追加同步和重置操作
		const tableSlot = {
			...slots,
			columnSetting: () => (
				<Fragment>
					<ElDropdownItem disabled={!state.existCacheColumns} onClick={syncColumnsCache}>
						同步列配置
					</ElDropdownItem>
					<ElDropdownItem disabled={!state.existCacheColumns} onClick={clearColumnsCache}>
						重置列配置
					</ElDropdownItem>
				</Fragment>
			),
		};

		useRender(() => (
			<FaTable
				{...tableProps.value}
				{...tableEmits.value}
				v-slots={tableSlot}
				ref={faTableRef}
				v-loading={state.loading}
				element-loading-text={state.loadingText}
				columns={responsiveColumns.value}
				searchForm={props.searchForm && configStore.tableLayout.showSearch}
				hideImage={configStore.tableLayout.hideImage}
				collapsedSearch={configStore.tableLayout.defaultCollapsedSearch}
				advancedSearchDrawer={configStore.tableLayout.advancedSearchDrawer}
				dataSearchRange={props.dataSearchRange || configStore.tableLayout.dataSearchRange}
				columnSettingBtn={props.columnSettingBtn && !props.columns}
				columnsChange={saveColumnsCache}
			/>
		));

		return useExpose(expose, {
			/** 清空多选表格的用户选择 */
			clearSelection: computed(() => faTableRef.value?.clearSelection),
			/** 返回当前选中的行 */
			getSelectionRows: computed(() => faTableRef.value?.getSelectionRows),
			/** 返回当前半选中的行 */
			getHalfSelectionRows: computed(() => faTableRef.value?.getHalfSelectionRows),
			/** 切换多选表格中某一行的选中状态；第二个参数可直接指定是否选中 */
			toggleRowSelection: computed(() => faTableRef.value?.toggleRowSelection),
			/** 切换多选表格的全选状态 */
			toggleAllSelection: computed(() => faTableRef.value?.toggleAllSelection),
			/** 切换可展开表格或树表格中某一行的展开状态；第二个参数可直接指定是否展开 */
			toggleRowExpansion: computed(() => faTableRef.value?.toggleRowExpansion),
			/** 设置单选表格的当前行；不传参数时取消当前高亮行 */
			setCurrentRow: computed(() => faTableRef.value?.setCurrentRow),
			/** 清空排序条件，使数据恢复为未排序状态 */
			clearSort: computed(() => faTableRef.value?.clearSort),
			/** 清除指定 columnKey 列表的过滤条件；不传参数时清除全部过滤器 */
			clearFilter: computed(() => faTableRef.value?.clearFilter),
			/** 重新计算表格布局，适用于表格可见性发生变化的场景 */
			doLayout: computed(() => faTableRef.value?.doLayout),
			/** 手动排序表格；prop 指定排序列，order 指定排序顺序 */
			sort: computed(() => faTableRef.value?.sort),
			/** 将表格滚动到指定坐标 */
			scrollTo: computed(() => faTableRef.value?.scrollTo),
			/** 设置垂直滚动位置 */
			setScrollTop: computed(() => faTableRef.value?.setScrollTop),
			/** 设置水平滚动位置 */
			setScrollLeft: computed(() => faTableRef.value?.setScrollLeft),
			/** 获取当前表格列上下文 */
			columns: computed(() => faTableRef.value?.columns),
			/** 更新懒加载表格指定 rowKey 对应的子节点 */
			updateKeyChildren: computed(() => faTableRef.value?.updateKeyChildren),
			/** 当前加载状态 */
			loading: computed(() => faTableRef.value?.loading),
			/** 表格数据 */
			tableData: computed(() => faTableRef.value?.tableData),
			/** 分页数据 */
			tablePagination: computed(() => faTableRef.value?.tablePagination),
			/** 搜索参数 */
			searchParam: computed(() => faTableRef.value?.searchParam),
			/** 表格选中状态 */
			selected: computed(() => faTableRef.value?.selected),
			/** 选中数据列表 */
			selectedList: computed(() => faTableRef.value?.selectedList),
			/** 选中数据的 rowKey 列表 */
			selectedListIds: computed(() => faTableRef.value?.selectedListIds),
			/** 部分选中数据的 rowKey 列表 */
			indeterminateSelectedListIds: computed(() => faTableRef.value?.indeterminateSelectedListIds),
			/** 表格宽度 */
			tableWidth: computed(() => faTableRef.value?.tableWidth),
			/** 表格高度 */
			tableHeight: computed(() => faTableRef.value?.tableHeight),
			/** 切换多选表格中某一行的半选状态；第二个参数可直接指定是否半选 */
			toggleRowIndeterminateSelection: computed(() => faTableRef.value?.toggleRowIndeterminateSelection),
			/** 刷新表格数据 */
			refresh: computed(() => faTableRef.value?.refresh),
			/** 重置表格状态和数据 */
			reset: computed(() => faTableRef.value?.reset),
			/** 重新加载并渲染表格，适用于 tableKey 发生变化的场景 */
			doRender,
			/** 在异步操作期间显示 Table 加载状态 */
			doLoading: computed(() => faTableRef.value?.doLoading),
		});
	},
});

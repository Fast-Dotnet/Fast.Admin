import type { FormSchema, FormSchemaIssue } from "@wot-ui/ui";

/** 单条字段校验规则，按必填、正则、自定义校验的顺序执行 */
type FormRule<T extends object, K extends keyof T> = {
	/** 是否必填，同时控制必填星号 */
	required?: boolean;
	/** 必填或正则校验失败提示；自定义校验使用其返回的提示 */
	message?: string;
	/** 正则校验，支持字符串、数字、布尔值和 bigint；复杂值使用自定义校验 */
	pattern?: RegExp;
	/**
	 * 执行同步或异步的自定义字段校验。
	 *
	 * @remarks
	 * 前置校验通过后，即使字段为空也会执行。
	 * 抛出的异常或 Promise 拒绝会继续向调用方传播，不会转换成字段错误。
	 *
	 * @param value - 当前字段值
	 * @param model - 完整表单模型，只读访问
	 * @returns 校验结果或其 Promise；非空字符串表示错误提示，`undefined` 或空字符串表示通过
	 */
	validator?: (value: T[K], model: Readonly<T>) => string | undefined | Promise<string | undefined>;
};

/** 以表单模型的顶层字段名为键；规则数组按声明顺序执行 */
type UseFormSchemaInput<T extends object> = {
	[K in Extract<keyof T, string>]?: FormRule<T, K> | FormRule<T, K>[];
};

// 0 和 false 都是有效值；空白字符串仅视为空，不修改原始模型。
const isEmpty = (value: unknown): boolean => {
	return (
		value === undefined || value === null || (typeof value === "string" && value.trim() === "") || (Array.isArray(value) && value.length === 0)
	);
};

/**
 * 将字段规则转换为 Wot 表单校验配置，同时提供必填星号的判断依据。
 *
 * @remarks
 * 每个字段只返回首条错误，不同字段依次校验并收集错误。
 *
 * 每次调用时读取当前规则，支持修改规则对象；不解析点路径或数组下标路径。
 *
 * 自定义校验抛出的异常不会转换成字段错误，也不会被吞掉。
 *
 * 泛型仅提供类型提示，传给 `wd-form` 的模型应与该类型一致。
 *
 * @typeParam T - 表单模型类型；省略时不限制字段名，字段值为 `unknown`
 * @param schema - 字段校验规则
 * @returns 可传给 `wd-form` 的 `schema` 配置
 */
export const useFormSchema = <T extends object = Record<string, unknown>>(schema: UseFormSchemaInput<T>): FormSchema => {
	return {
		isRequired(path) {
			const fieldSchema = schema[path as Extract<keyof T, string>];
			if (!fieldSchema) return false;

			return Array.isArray(fieldSchema) ? fieldSchema.some((rule) => rule.required === true) : fieldSchema.required === true;
		},

		async validate(model: Record<string, unknown>) {
			const issues: FormSchemaIssue[] = [];
			// Wot 的校验接口不保留模型泛型，由调用方保证模型与规则类型一致。
			const formModel = model as T;

			for (const [field, fieldRules] of Object.entries(schema) as [
				Extract<keyof T, string>,
				UseFormSchemaInput<T>[Extract<keyof T, string>],
			][]) {
				if (!fieldRules) continue;
				const value = formModel[field];
				const empty = isEmpty(value);
				const rules = Array.isArray(fieldRules) ? fieldRules : [fieldRules];

				for (const rule of rules) {
					let message: string | undefined;

					if (rule.required && empty) {
						message = rule.message || "该项为必填项";
					}

					// 空值由 required 负责，避免选填字段被正则拦截。
					if (!message && !empty && rule.pattern) {
						// 创建独立实例，避免带 g/y 标志的正则受 lastIndex 影响。
						const pattern = new RegExp(rule.pattern.source, rule.pattern.flags);

						const isScalar =
							typeof value === "string" || typeof value === "number" || typeof value === "boolean" || typeof value === "bigint";
						if (!isScalar || !pattern.test(String(value))) {
							message = rule.message || "格式不正确";
						}
					}

					if (!message && rule.validator) {
						message = await rule.validator(value, formModel);
					}

					if (message) {
						issues.push({ path: [field], message });
						break;
					}
				}
			}

			return issues;
		},
	};
};

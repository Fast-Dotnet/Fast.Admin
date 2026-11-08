import { ElementTypes, NodeTypes, createSimpleExpression, findDir, findProp, hasDynamicKeyVBind, isSlotOutlet } from "@vue/compiler-core";
import type { DirectiveNode, ElementNode, NodeTransform, TemplateChildNode } from "@vue/compiler-core";

/** 用于判断插槽结构的有效节点 */
const contentNodes = (children: TemplateChildNode[]) =>
	children.filter((child) => child.type !== NodeTypes.COMMENT && (child.type !== NodeTypes.TEXT || child.content.trim().length > 0));

/** 仅对单个静态名称的 `<slot>` 生成转发条件。本地兜底内容和结构指令沿用编译器的登记逻辑。 */
const getForwardingCondition = (children: TemplateChildNode[]): string | undefined => {
	const content = contentNodes(children);
	const outlet = content[0];
	if (content.length !== 1 || !outlet || !isSlotOutlet(outlet)) return undefined;
	if (contentNodes(outlet.children).length || hasDynamicKeyVBind(outlet) || findDir(outlet, /^(if|else-if|else|for)$/, true)) return undefined;

	const name = findProp(outlet, "name", false, true);
	if (!name) return "$slots.default";
	if (name.type !== NodeTypes.ATTRIBUTE || !name.value) return undefined;

	// 转发时可以改名，按 `<slot name>` 检查调用方提供的插槽。
	return `$slots[${JSON.stringify(name.value.content)}]`;
};

/** 按子组件的登记名收集插槽条件。值为 `undefined` 时保留该名称的原有登记。 */
const getSlotConditions = (component: ElementNode): Map<string, string | undefined> | undefined => {
	if (findDir(component, "slot", true)) return undefined;

	const conditions = new Map<string, string | undefined>();
	const defaultChildren: TemplateChildNode[] = [];

	for (const child of contentNodes(component.children)) {
		if (child.type === NodeTypes.ELEMENT && child.tag === "template" && !contentNodes(child.children).length) continue;

		const slot = child.type === NodeTypes.ELEMENT && child.tag === "template" ? findDir(child, "slot", true) : undefined;
		if (!slot || child.type !== NodeTypes.ELEMENT) {
			defaultChildren.push(child);
			continue;
		}

		// uni-app 在 `u-s` 中用 `d` 表示默认插槽。
		let name = "d";
		const { arg } = slot;
		if (arg) {
			// 动态插槽名可能与静态插槽名重合，此时沿用编译器生成的名单。
			if (arg.type !== NodeTypes.SIMPLE_EXPRESSION || !arg.isStatic) return undefined;
			if (arg.content !== "default") name = arg.content;
		}

		const forwarding = getForwardingCondition(child.children);

		// 同名分支可能提供本地内容，保留该插槽的登记。
		if (conditions.has(name) || !forwarding || findDir(child, /^(else-if|else|for)$/, true)) {
			conditions.set(name, undefined);
			continue;
		}

		const guard = findDir(child, "if")?.exp;
		conditions.set(name, guard?.type === NodeTypes.SIMPLE_EXPRESSION ? `(${guard.content}) && ${forwarding}` : forwarding);
	}

	if (defaultChildren.length) {
		conditions.set("d", conditions.has("d") ? undefined : getForwardingCondition(defaultChildren));
	}

	return conditions;
};

/**
 * 修正 uni-app 小程序编译器生成的 `u-s` 插槽名单。
 * 调用方未提供转发插槽时，从名单中省略该插槽，让子组件使用自己的默认视图。
 * 作用域参数和插槽内容仍由 uni-app 编译，H5 由 Vue 处理条件插槽。
 */
export const transformForwardedSlots: NodeTransform = (node, context) => {
	if (node.type !== NodeTypes.ROOT || !("miniProgram" in context)) return undefined;

	const replacements: { component: ElementNode; binding: DirectiveNode; originalNames: string }[] = [];

	const scan = (child: TemplateChildNode): void => {
		if (child.type !== NodeTypes.ELEMENT) return;

		if (child.tagType === ElementTypes.COMPONENT && !findProp(child, "u-s", false, true)) {
			const conditions = getSlotConditions(child);
			if (conditions && [...conditions.values()].some(Boolean)) {
				const names = [...conditions].map(([name, condition]) => {
					const literal = JSON.stringify(name);
					return condition ? `(${condition}) ? ${literal} : ""` : literal;
				});

				// 先按普通绑定编译，让 uni-app 解析 $slots 和当前模板作用域。
				const binding: DirectiveNode = {
					type: NodeTypes.DIRECTIVE,
					name: "bind",
					arg: createSimpleExpression("data-uni-slot-registration", true),
					exp: createSimpleExpression(`[${names.join(", ")}].filter(Boolean)`),
					modifiers: [],
					loc: child.loc,
				};

				child.props.push(binding);
				replacements.push({
					component: child,
					binding,
					originalNames: `[${[...conditions.keys()].map((name) => `'${name}'`).join(",")}]`,
				});
			}
		}

		child.children.forEach(scan);
	};

	// 在 uni-app 展开作用域插槽前识别原始模板中的转发结构。
	node.children.forEach(scan);

	return () => {
		for (const { component, binding, originalNames } of replacements) {
			const registered = findProp(component, "u-s", true);
			const arg = binding.arg;

			// 名单一致时才替换，避免覆盖编译器生成的其他插槽结构。
			if (
				registered?.type === NodeTypes.DIRECTIVE &&
				registered.exp?.type === NodeTypes.SIMPLE_EXPRESSION &&
				registered.exp.content === originalNames &&
				arg?.type === NodeTypes.SIMPLE_EXPRESSION
			) {
				component.props.splice(component.props.indexOf(registered), 1);
				arg.content = "u-s";
			} else {
				const bindingIndex = component.props.indexOf(binding);
				if (bindingIndex !== -1) component.props.splice(bindingIndex, 1);
			}
		}
	};
};

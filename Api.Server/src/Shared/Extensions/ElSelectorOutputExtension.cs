// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Shared;

/// <summary>
/// <see cref="ElSelectorOutput{T}"/> 扩展方法
/// </summary>
public static class ElSelectorOutputExtension
{
    /// <summary>
    /// 构造树形
    /// </summary>
    /// <param name="list">待构建的选择器节点列表</param>
    /// <returns>构建后的根节点列表</returns>
    public static List<ElSelectorOutput<T>> Build<T>(this List<ElSelectorOutput<T>> list)
    {
        // 构建所有节点 Value 的集合
        var valueSet = new HashSet<object>(list.Select(sl => (object)sl.Value));

        // 根节点条件：ParentId == 0 或者父节点不在当前列表中（被权限过滤掉了）
        var result = list.Where(wh => wh.ParentId.Equals(0) || !valueSet.Contains(wh.ParentId)).ToList();

        result.ForEach(e => BuildChildNodes(list, e));
        return result;
    }

    /// <summary>
    /// 构造子节点集合
    /// </summary>
    /// <param name="totalNodes">节点总数</param>
    /// <param name="node">当前节点序号</param>
    private static void BuildChildNodes<T>(List<ElSelectorOutput<T>> totalNodes, ElSelectorOutput<T> node)
    {
        var nodeSubList = totalNodes.Where(wh => wh.ParentId.Equals(node.Value)).ToList();
        nodeSubList.ForEach(e => BuildChildNodes(totalNodes, e));
        node.Children = nodeSubList;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace GameExtension.Editor
{
    /// <summary>
    /// 组件收集器接口。
    /// </summary>
    public interface IComponentCollector
    {
        /// <summary>
        /// 收集 Transform 的所有子组件。 
        /// </summary>
        /// <param name="target">目标 Transform。</param>
        /// <param name="fieldNamePrefix">字段前缀。</param>
        /// <param name="fieldNameByType">是否使用组件类型作为字段名。</param>
        /// <param name="componentMapDict">组件类型映射字典。</param>
        /// <returns>字段组件字典。</returns>
        Dictionary<string, Component> CollectComponentFields(Transform target, string fieldNamePrefix, bool fieldNameByType, Dictionary<string, string> componentMapDict);
    }
}

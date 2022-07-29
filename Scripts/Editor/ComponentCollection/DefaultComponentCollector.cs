using System.Collections.Generic;
using UnityEngine;

namespace GameExtension.Editor
{
    public class DefaultComponentCollector : IComponentCollector
    {
        private const char ComponentSeparator = '_';

        /// <summary>
        /// 收集 Transform 的所有子组件。 
        /// </summary>
        /// <param name="target">目标 Transform。</param>
        /// <param name="fieldNamePrefix">字段前缀。</param>
        /// <param name="fieldNameByType">是否使用组件类型作为字段名。</param>
        /// <param name="componentMapDict">组件类型映射字典。</param>
        /// <returns>字段组件字典。</returns>
        public Dictionary<string, Component> CollectComponentFields(Transform target, string fieldNamePrefix, bool fieldNameByType, Dictionary<string, string> componentMapDict)
        {
            Dictionary<string, Component> fieldComponentDict = new Dictionary<string, Component>();
            var children = target.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                CollectComponentField(child, fieldComponentDict, fieldNamePrefix, fieldNameByType, componentMapDict);
            }
            return fieldComponentDict;
        }

        /// <summary>
        /// 收集 Transform 的组件。 
        /// </summary>
        /// <param name="target">目标 Transform。</param>
        /// <param name="fieldComponentDict">收集的结果字典。</param>
        /// <param name="fieldNamePrefix">字段前缀。</param>
        /// <param name="fieldNameByType">是否使用组件类型作为字段名。</param>
        /// <param name="componentMapDict">组件类型映射字典。</param>
        private void CollectComponentField(Transform target, Dictionary<string, Component> fieldComponentDict, string fieldNamePrefix, bool fieldNameByType, Dictionary<string, string> componentMapDict)
        {
            string[] splits = target.name.Split(ComponentSeparator);
            if (splits.Length <= 1)
            {
                return;
            }

            int nameIndex = splits.Length - 1;
            for (int i = 0; i < nameIndex; i++)
            {
                string typeKey = splits[i];
                string typeName;
                if (!componentMapDict.TryGetValue(typeKey, out typeName))
                {
                    Debug.LogErrorFormat("Component type key '{0}' has no mapping component type.", typeKey);
                    continue;
                }

                Component component = target.GetComponent(typeName);
                if (component == null)
                {
                    Debug.LogErrorFormat("Transform '{0}' has no component '{1}'.", target.name, typeName);
                    continue;
                }

                string name = splits[nameIndex];
                name = string.IsNullOrEmpty(fieldNamePrefix) ? LowerFirst(name) : UpperFirst(name);
                string fieldName = string.Format("{0}{1}{2}", fieldNamePrefix, name, fieldNameByType ? typeName : typeKey);
                if (fieldComponentDict.ContainsKey(fieldName))
                {
                    Debug.LogErrorFormat("Already exits the same field '{0}'. Please modify the name of transform '{1}'.", fieldName, target.name);
                    continue;
                }

                fieldComponentDict.Add(fieldName, component);
            }
        }

        /// <summary>
        /// 首字符大写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string UpperFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if (str.Length <= 1)
            {
                return str.ToUpper();
            }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// 首字符小写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string LowerFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if (str.Length <= 1)
            {
                return str.ToLower();
            }
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameExtension.Editor
{
    public class DefaultCollectionGenerator : ICollectionGenerator
    {
        /// <summary>
        /// 生成 Components 代码。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="codeTemplate">代码模板。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="className">类名。</param>
        /// <param name="fieldTypeDict">字段类型字典。</param>
        public void GenerateComponentsCode(string filePath, string codeTemplate, string nameSpace, string className, Dictionary<string, string> fieldTypeDict)
        {
            if (string.IsNullOrEmpty(codeTemplate))
            {
                Debug.LogError("Component collection code template file is invalid.");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder(codeTemplate);
            stringBuilder.Replace("__CREATE_TIME__", DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ff"));
            stringBuilder.Replace("__NAME_SPACE__", nameSpace);
            stringBuilder.Replace("__CLASS_NAME__", className);
            string field, getField;
            GenerateComponentField(fieldTypeDict, out field, out getField);
            stringBuilder.Replace("__FIELD__", field);
            stringBuilder.Replace("__GET_FIELD__", getField);

            SaveFile(filePath, stringBuilder.ToString());
        }

        /// <summary>
        /// 生成 MonoBehaviour 代码。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="codeTemplate">代码模板。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="className">类名。</param>
        public void GenerateBehaviourCode(string filePath, string codeTemplate, string nameSpace, string className)
        {
            if (string.IsNullOrEmpty(codeTemplate))
            {
                Debug.LogError("Component behaviour code template file is invalid.");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder(codeTemplate);

            stringBuilder.Replace("__NAME_SPACE__", nameSpace);
            stringBuilder.Replace("__CLASS_NAME__", className);

            SaveFile(filePath, stringBuilder.ToString());
        }

        /// <summary>
        /// 生成组件字段。
        /// </summary>
        /// <param name="fieldTypeDict">字段类型字典。</param>
        /// <param name="field">字段代码区域 的字符。</param>
        /// <param name="getField">获取组件代码区域 的字符。</param>
        private void GenerateComponentField(Dictionary<string, string> fieldTypeDict, out string field, out string getField)
        {
            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder getFieldBuilder = new StringBuilder();
            int index = 0;
            foreach (var pair in fieldTypeDict)
            {
                fieldBuilder.AppendLine(string.Format("        private {0} {1};", pair.Value, pair.Key));
                getFieldBuilder.AppendLine(string.Format("            {0} = collection.GetComponent<{1}>({2});", pair.Key, pair.Value, index++));
            }

            // 去除最后的换行符
            field = fieldBuilder.ToString().TrimEnd();
            getField = getFieldBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// 保存文件。
        /// </summary>
        /// <param name="filePath">文件地址。</param>
        /// <param name="content">内容字符串。</param>
        private void SaveFile(string filePath, string content)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
        }
    }
}

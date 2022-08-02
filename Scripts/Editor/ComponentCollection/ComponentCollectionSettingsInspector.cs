using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameExtension.Editor
{
    [CustomEditor(typeof(ComponentCollectionSettings))]
    public class ComponentCollectionSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Extension Code"))
            {
                GenerateExtensionCode();
            }
        }

        /// <summary>
        /// 生成 Extension 代码。
        /// </summary>
        private void GenerateExtensionCode()
        {
            ComponentCollectionSettings settings = (ComponentCollectionSettings) target;

            if (string.IsNullOrEmpty(settings.m_DefaultCodeSavePath))
            {
                Debug.LogError("Code save path is invalid.");
                return;
            }

            if (!Directory.Exists(settings.m_DefaultCodeSavePath))
            {
                Directory.CreateDirectory(settings.m_DefaultCodeSavePath);
            }

            string nameSpace = settings.m_DefaultNameSpace;
            if (string.IsNullOrEmpty(nameSpace) || !settings.m_DefaultNameRegex.IsMatch(nameSpace))
            {
                Debug.LogErrorFormat("NameSpace '{0}' is invalid.", nameSpace);
                return;
            }

            if (settings.m_CollectionExtensionCodeTemplate == null)
            {
                Debug.LogError("CollectionExtensionCodeTemplate is null, Please check 'ComponentCollectionSettings' asset.");
                return;
            }

            string codeFileName = string.Format("{0}/ComponentCollectionExtension.cs", settings.m_DefaultCodeSavePath);
            if (!CheckGenerateFile(codeFileName))
            {
                return;
            }

            GenerateExtensionCode(codeFileName, settings.m_CollectionExtensionCodeTemplate.text, settings.m_DefaultNameSpace, settings.ComponentMapDict.Values.ToList());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 检查文件是否可以生成。
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        private bool CheckGenerateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return EditorUtility.DisplayDialog("File Exits", " File already exits, continue regenerate ?", "Continue", "Cancel");
            }
            return true;
        }

        /// <summary>
        /// 生成 Extension 代码。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="codeTemplate">代码模板。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="componentTypes">组件类型。</param>
        public void GenerateExtensionCode(string filePath, string codeTemplate, string nameSpace, List<string> componentTypes)
        {
            if (string.IsNullOrEmpty(codeTemplate))
            {
                Debug.LogError("Component behaviour code template file is invalid.");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder(codeTemplate);

            stringBuilder.Replace("__CREATE_TIME__", DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ff"));
            stringBuilder.Replace("__NAME_SPACE__", nameSpace);
            string function;
            GenerateGetComponentFunction(componentTypes, out function);
            stringBuilder.Replace("__GET_COMPONENT__", function);

            SaveFile(filePath, stringBuilder.ToString());
        }

        /// <summary>
        /// 生成获取组件扩展方法。
        /// </summary>
        /// <param name="componentTypes">组件类型。</param>
        /// <param name="function">获取组件扩展方法代码区域 的字符。</param>
        private void GenerateGetComponentFunction(List<string> componentTypes, out string function)
        {
            StringBuilder functionBuilder = new StringBuilder();
            foreach (string component in componentTypes)
            {
                functionBuilder.AppendLine(string.Format("        public static {0} Get{0}(this ComponentCollection componentCollection, int index)", component));
                functionBuilder.AppendLine("        {");
                functionBuilder.AppendLine(string.Format("            return componentCollection.GetComponent<{0}>(index);", component));
                functionBuilder.AppendLine("        }");
                functionBuilder.AppendLine();
            }
            function = functionBuilder.ToString().TrimEnd();
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

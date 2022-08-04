using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameExtension.Editor
{
    [CustomEditor(typeof(ComponentCollection))]
    public class ComponentCollectionInspector : UnityEditor.Editor
    {
        private const string NoneOptionName = "<None>";
        private readonly string[] m_FieldNameRule = {"TypeKey", "TypeName"};

        private int m_ExitsSettingsCount;
        private ComponentCollectionSettings m_Settings;

        private SerializedProperty m_CollectorTypeName;
        private SerializedProperty m_FieldNamePrefix;
        private SerializedProperty m_FieldNameByType;
        private SerializedProperty m_FieldNames;
        private SerializedProperty m_FieldComponents;
        private string[] m_CollectorTypeNames;
        private int m_CollectorTypeNameIndex;
        private IComponentCollector m_Collector;
        private int m_FieldNameRuleIndex;
        private bool m_ShowComponentField;

        private SerializedProperty m_GeneratorTypeName;
        private SerializedProperty m_NameSpace;
        private SerializedProperty m_ClassName;
        private SerializedProperty m_CodeSavePath;
        private string[] m_GeneratorTypeNames;
        private int m_GeneratorTypeNameIndex;
        private ICollectionGenerator m_Generator;

        private SerializedProperty m_Setup;

        private SerializedProperty m_Components;

        private IComponentCollector Collector
        {
            get
            {
                if (m_Collector == null)
                {
                    string collectorTypeName = m_CollectorTypeName.stringValue;
                    if (string.IsNullOrEmpty(collectorTypeName))
                    {
                        Debug.LogError("Collector is invalid.");
                        return null;
                    }

                    Type collectorType = TypeUtility.GetEditorType(collectorTypeName);
                    if (collectorType == null)
                    {
                        Debug.LogErrorFormat("Can not get collector type '{0}'.", collectorTypeName);
                        return null;
                    }

                    m_Collector = (IComponentCollector) Activator.CreateInstance(collectorType);
                    if (m_Collector == null)
                    {
                        Debug.LogErrorFormat("Can not create collector instance '{0}'.", collectorTypeName);
                        return null;
                    }
                }

                return m_Collector;
            }
        }

        private ICollectionGenerator Generator
        {
            get
            {
                if (m_Generator == null)
                {
                    string generatorTypeName = m_GeneratorTypeName.stringValue;
                    if (string.IsNullOrEmpty(generatorTypeName))
                    {
                        Debug.LogError("Generator is invalid.");
                        return null;
                    }

                    Type generatorType = TypeUtility.GetEditorType(generatorTypeName);
                    if (generatorType == null)
                    {
                        Debug.LogErrorFormat("Can not get generator type '{0}'.", generatorTypeName);
                        return null;
                    }

                    m_Generator = (ICollectionGenerator) Activator.CreateInstance(generatorType);
                    if (m_Generator == null)
                    {
                        Debug.LogErrorFormat("Can not create generator instance '{0}'.", generatorTypeName);
                        return null;
                    }
                }

                return m_Generator;
            }
        }

        private void OnEnable()
        {
            string[] paths = AssetDatabase.FindAssets("t:ComponentCollectionSettings");
            m_ExitsSettingsCount = paths.Length;
            if (m_ExitsSettingsCount != 1)
            {
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            m_Settings = AssetDatabase.LoadAssetAtPath<ComponentCollectionSettings>(path);

            InitSerializeData();
            m_ShowComponentField = true;
        }

        public override void OnInspectorGUI()
        {
            if (m_Settings == null)
            {
                DrawSettingsGUI();
                return;
            }

            serializedObject.Update();

            DrawCollectGUI();
            DrawGenerateGUI();

            if (m_Setup.boolValue == false)
            {
                SetupDefaultValue();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 初始化序列化数据
        /// </summary>
        private void InitSerializeData()
        {
            // Setup
            m_Setup = serializedObject.FindProperty("m_Setup");

            // Collect settings
            m_CollectorTypeName = serializedObject.FindProperty("m_CollectorTypeName");
            m_FieldNamePrefix = serializedObject.FindProperty("m_FieldNamePrefix");
            m_FieldNameByType = serializedObject.FindProperty("m_FieldNameByType");
            m_FieldNames = serializedObject.FindProperty("m_FieldNames");
            m_FieldComponents = serializedObject.FindProperty("m_FieldComponents");

            // CollectorTypeNames
            List<string> collectorTypeNames = new List<string> {NoneOptionName};
            collectorTypeNames.AddRange(TypeUtility.GetEditorTypeNames(typeof(IComponentCollector)));
            m_CollectorTypeNames = collectorTypeNames.ToArray();

            // CollectorCollectorTypeNameIndex
            m_CollectorTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_CollectorTypeName.stringValue))
            {
                m_CollectorTypeNameIndex = collectorTypeNames.IndexOf(m_CollectorTypeName.stringValue);
                if (m_CollectorTypeNameIndex <= 0)
                {
                    m_CollectorTypeNameIndex = 0;
                    m_CollectorTypeName.stringValue = null;
                    m_Collector = null;
                }
            }
            collectorTypeNames.Clear();

            // Generate settings
            m_GeneratorTypeName = serializedObject.FindProperty("m_GeneratorTypeName");
            m_NameSpace = serializedObject.FindProperty("m_NameSpace");
            m_ClassName = serializedObject.FindProperty("m_ClassName");
            m_CodeSavePath = serializedObject.FindProperty("m_CodeSavePath");

            // GeneratorTypeNames
            List<string> generatorTypeNames = new List<string> {NoneOptionName};
            generatorTypeNames.AddRange(TypeUtility.GetEditorTypeNames(typeof(ICollectionGenerator)));
            m_GeneratorTypeNames = generatorTypeNames.ToArray();

            // GeneratorTypeNameIndex
            m_GeneratorTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_GeneratorTypeName.stringValue))
            {
                m_GeneratorTypeNameIndex = generatorTypeNames.IndexOf(m_GeneratorTypeName.stringValue);
                if (m_GeneratorTypeNameIndex <= 0)
                {
                    m_GeneratorTypeNameIndex = 0;
                    m_GeneratorTypeName.stringValue = null;
                    m_Generator = null;
                }
            }
            generatorTypeNames.Clear();

            // m_FieldNameRuleIndex
            m_FieldNameRuleIndex = m_FieldNameByType.boolValue ? 1 : 0;

            // Runtime Components
            m_Components = serializedObject.FindProperty("m_Components");
        }

        /// <summary>
        /// 设置默认值。
        /// </summary>
        private void SetupDefaultValue()
        {
            m_Setup.boolValue = true;

            m_CollectorTypeName.stringValue = m_Settings.m_CollectorTypeName;
            m_FieldNamePrefix.stringValue = m_Settings.m_DefaultFieldNamePrefix;
            m_FieldNameByType.boolValue = m_Settings.m_DefaultFieldNameByType;

            m_GeneratorTypeName.stringValue = m_Settings.m_GeneratorTypeName;
            m_NameSpace.stringValue = m_Settings.m_DefaultNameSpace;
            m_ClassName.stringValue = serializedObject.targetObject.name;
            m_CodeSavePath.stringValue = m_Settings.m_DefaultCodeSavePath;

            List<string> temp = new List<string>(m_CollectorTypeNames);
            int collectorIndex = temp.IndexOf(m_Settings.m_CollectorTypeName);
            m_CollectorTypeNameIndex = Mathf.Max(0, collectorIndex);
            temp = new List<string>(m_GeneratorTypeNames);
            int generatorIndex = temp.IndexOf(m_Settings.m_GeneratorTypeName);
            m_GeneratorTypeNameIndex = Mathf.Max(0, generatorIndex);
            temp.Clear();

            m_FieldNameRuleIndex = m_FieldNameByType.boolValue ? 1 : 0;
        }

        /// <summary>
        /// 绘制 Settings GUI。
        /// </summary>
        private void DrawSettingsGUI()
        {
            if (m_ExitsSettingsCount > 1)
            {
                EditorGUILayout.HelpBox("Multiple 'ComponentCollectionSettings' asset exist. Please delete the redundant assets.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Need a 'ComponentCollectionSettings' asset, Please create one.", MessageType.Error);
            if (GUILayout.Button("Create"))
            {
                m_Settings = CreateInstance<ComponentCollectionSettings>();
                AssetDatabase.CreateAsset(m_Settings, "Assets/ComponentCollectionSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                InitSerializeData();
            }
        }

        /// <summary>
        /// 绘制收集设置 GUI。
        /// </summary>
        private void DrawCollectGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("Collect Settings", "BoldLabel");

                int collectorTypeNameIndex = EditorGUILayout.Popup("Collector", m_CollectorTypeNameIndex, m_CollectorTypeNames);
                if (collectorTypeNameIndex != m_CollectorTypeNameIndex)
                {
                    m_CollectorTypeNameIndex = collectorTypeNameIndex;
                    m_CollectorTypeName.stringValue = m_CollectorTypeNameIndex <= 0 ? null : m_CollectorTypeNames[m_CollectorTypeNameIndex];
                    m_Collector = null;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    m_FieldNamePrefix.stringValue = EditorGUILayout.TextField("Field Name Rule", m_FieldNamePrefix.stringValue);
                    int index = EditorGUILayout.Popup(m_FieldNameRuleIndex, m_FieldNameRule);
                    if (m_FieldNameRuleIndex != index)
                    {
                        m_FieldNameRuleIndex = index;
                        m_FieldNameByType.boolValue = m_FieldNameRuleIndex == 1;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                m_ShowComponentField = EditorGUILayout.Foldout(m_ShowComponentField, "Components", true);
                EditorGUI.indentLevel--;
                if (m_ShowComponentField)
                {
                    int deleteIndex = -1;
                    for (int i = 0; i < m_FieldNames.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(string.Format("[{0}]", i), GUILayout.Width(30));
                            SerializedProperty fieldName = m_FieldNames.GetArrayElementAtIndex(i);
                            SerializedProperty component = m_FieldComponents.GetArrayElementAtIndex(i);
                            fieldName.stringValue = EditorGUILayout.TextField(fieldName.stringValue);
                            component.objectReferenceValue = EditorGUILayout.ObjectField(component.objectReferenceValue, typeof(Component), true);
                            if (GUILayout.Button("", "OL Minus"))
                            {
                                deleteIndex = i;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(4);
                    }
                    if (deleteIndex != -1)
                    {
                        SerializedArrayDeleteIndex(m_FieldNames, deleteIndex);
                        SerializedArrayDeleteIndex(m_FieldComponents, deleteIndex);
                    }
                }

                if (GUILayout.Button("Collect Components To Update"))
                {
                    CollectComponentFieldsToUpdate();
                    SyncSerializeComponents();
                }

                if (GUILayout.Button("Collect Components To Add"))
                {
                    CollectComponentFieldsToAdd();
                    SyncSerializeComponents();
                }

                if (GUILayout.Button("Remove Null Component"))
                {
                    RemoveNullComponent();
                    SyncSerializeComponents();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制生成设置 GUI。
        /// </summary>
        private void DrawGenerateGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("Generate Settings", "BoldLabel");

                int generatorTypeNameIndex = EditorGUILayout.Popup("Generator", m_GeneratorTypeNameIndex, m_GeneratorTypeNames);
                if (generatorTypeNameIndex != m_GeneratorTypeNameIndex)
                {
                    m_GeneratorTypeNameIndex = generatorTypeNameIndex;
                    m_GeneratorTypeName.stringValue = m_GeneratorTypeNameIndex <= 0 ? null : m_GeneratorTypeNames[m_GeneratorTypeNameIndex];
                    m_Generator = null;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    m_NameSpace.stringValue = EditorGUILayout.TextField("Name Space", m_NameSpace.stringValue);
                    if (GUILayout.Button("Default", GUILayout.Width(60)))
                    {
                        m_NameSpace.stringValue = m_Settings.m_DefaultNameSpace;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    m_ClassName.stringValue = EditorGUILayout.TextField("Class Name", m_ClassName.stringValue);
                    if (GUILayout.Button("Default", GUILayout.Width(60)))
                    {
                        m_ClassName.stringValue = serializedObject.targetObject.name;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Code Save Path", m_CodeSavePath.stringValue);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        string path = EditorUtility.SaveFolderPanel("Select Save Path", m_CodeSavePath.stringValue, string.Empty);
                        if (string.IsNullOrEmpty(path))
                        {
                            Debug.LogError("Select folder path is invalid.");
                        }
                        else
                        {
                            int index = path.IndexOf("Assets", StringComparison.Ordinal);
                            if (index < 0)
                            {
                                Debug.LogWarning("Suggest to save to any folder of 'Assets'.");
                            }
                            m_CodeSavePath.stringValue = path.Substring(index);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Generate Components Code"))
                {
                    GenerateComponentsCode();
                }

                if (GUILayout.Button("Generate Behaviour Code"))
                {
                    GenerateBehaviourCode();
                }

                if (GUILayout.Button("Add Behaviour Component"))
                {
                    AddBehaviourComponent();
                }
            }
            EditorGUILayout.EndVertical();
        }

        #region Collect

        /// <summary>
        /// 以更新组件字段列表的方式收集。
        /// </summary>
        private void CollectComponentFieldsToUpdate()
        {
            if (Collector == null)
            {
                return;
            }

            m_FieldNames.ClearArray();
            m_FieldComponents.ClearArray();
            Transform transform = ((Component) target).transform;
            Dictionary<string, Component> fieldComponentDict = Collector.CollectComponentFields(transform, m_FieldNamePrefix.stringValue, m_FieldNameByType.boolValue, m_Settings.ComponentMapDict);
            foreach (var pair in fieldComponentDict)
            {
                CollectComponentField(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 以增加组件字段到列表的方式收集。
        /// </summary>
        private void CollectComponentFieldsToAdd()
        {
            if (Collector == null)
            {
                return;
            }

            Transform transform = ((Component) target).transform;
            Dictionary<string, Component> fieldComponentDict = Collector.CollectComponentFields(transform, m_FieldNamePrefix.stringValue, m_FieldNameByType.boolValue, m_Settings.ComponentMapDict);
            foreach (var pair in fieldComponentDict)
            {
                CollectComponentField(pair.Key, pair.Value);
            }

            // 正序检查后面重复的字段索引。
            List<int> adjectiveFieldIndex = new List<int>();
            List<string> cacheFieldNames = new List<string>();
            for (int i = 0; i < m_FieldNames.arraySize; i++)
            {
                string fieldName = m_FieldNames.GetArrayElementAtIndex(i).stringValue;
                if (cacheFieldNames.Contains(fieldName))
                {
                    adjectiveFieldIndex.Add(i);
                    continue;
                }
                cacheFieldNames.Add(fieldName);
            }
            cacheFieldNames.Clear();

            // 倒序删除重复的字段索引。删除必须使用倒序，否则索引会错误
            for (int i = adjectiveFieldIndex.Count - 1; i >= 0; i--)
            {
                int adjectiveIndex = adjectiveFieldIndex[i];
                SerializedArrayDeleteIndex(m_FieldNames, adjectiveIndex);
                SerializedArrayDeleteIndex(m_FieldComponents, adjectiveIndex);
            }
            adjectiveFieldIndex.Clear();
        }

        /// <summary>
        /// 收集组件字段。
        /// </summary>
        /// <param name="fieldName">字段名。</param>
        /// <param name="component">组件引用。</param>
        private void CollectComponentField(string fieldName, Component component)
        {
            int index = m_FieldNames.arraySize;
            m_FieldNames.InsertArrayElementAtIndex(index);
            m_FieldNames.GetArrayElementAtIndex(index).stringValue = fieldName;
            m_FieldComponents.InsertArrayElementAtIndex(index);
            m_FieldComponents.GetArrayElementAtIndex(index).objectReferenceValue = component;
        }

        /// <summary>
        /// 移除空组件。
        /// </summary>
        private void RemoveNullComponent()
        {
            for (int i = m_FieldNames.arraySize - 1; i >= 0; i--)
            {
                Component component = (Component) m_FieldComponents.GetArrayElementAtIndex(i).objectReferenceValue;
                if (component == null)
                {
                    SerializedArrayDeleteIndex(m_FieldNames, i);
                    SerializedArrayDeleteIndex(m_FieldComponents, i);
                }
            }
        }

        /// <summary>
        /// 序列化数组移除索引元素。
        /// </summary>
        /// <param name="serializedProperty">数组的序列化属性。</param>
        /// <param name="index">要移除的索引。</param>
        private void SerializedArrayDeleteIndex(SerializedProperty serializedProperty, int index)
        {
            int originLength = serializedProperty.arraySize;
            if (originLength <= index)
            {
                return;
            }

            // 序列化的引用类型，第一次删除时会将该引用置空，第二次才会将索引移除。所以在这里检查一下。
            // 序列化 string 类型会被认为是值类型，可以直接移除。
            serializedProperty.DeleteArrayElementAtIndex(index);
            if (originLength == serializedProperty.arraySize)
            {
                serializedProperty.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>
        /// 同步序列化的 Components。
        /// </summary>
        private void SyncSerializeComponents()
        {
            m_Components.ClearArray();
            for (int i = 0; i < m_FieldComponents.arraySize; i++)
            {
                m_Components.InsertArrayElementAtIndex(i);
                m_Components.GetArrayElementAtIndex(i).objectReferenceValue = m_FieldComponents.GetArrayElementAtIndex(i).objectReferenceValue;
            }
        }

        #endregion

        #region Generate

        /// <summary>
        /// 检查生成文件的条件是否满足。
        /// </summary>
        /// <returns></returns>
        private bool CheckGenerateCondition()
        {
            string savePath = m_CodeSavePath.stringValue;
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Code save path is invalid.");
                return false;
            }

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string nameSpace = m_NameSpace.stringValue;
            if (string.IsNullOrEmpty(nameSpace) || !m_Settings.m_DefaultNameRegex.IsMatch(nameSpace))
            {
                Debug.LogErrorFormat("NameSpace '{0}' is invalid.", nameSpace);
                return false;
            }

            string className = m_ClassName.stringValue;
            if (string.IsNullOrEmpty(className) || !m_Settings.m_DefaultNameRegex.IsMatch(className))
            {
                Debug.LogErrorFormat("Class name '{0}' is invalid.", className);
                return false;
            }

            return true;
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
        /// 生成 Components 代码。
        /// </summary>
        private void GenerateComponentsCode()
        {
            if (Generator == null)
            {
                Debug.LogError("Generator is null.");
                return;
            }

            if (!CheckGenerateCondition())
            {
                return;
            }

            if (m_Settings.m_ComponentsCodeTemplate == null)
            {
                Debug.LogError("ComponentsCodeTemplate is null, Please check 'ComponentCollectionSettings' asset.");
                return;
            }

            string codeFileName = string.Format("{0}/{1}.Components.cs", m_CodeSavePath.stringValue, m_ClassName.stringValue);
            if (!CheckGenerateFile(codeFileName))
            {
                return;
            }

            RemoveNullComponent();
            SyncSerializeComponents();

            Dictionary<string, string> fieldTypeDict = new Dictionary<string, string>();
            for (int i = 0; i < m_FieldNames.arraySize; i++)
            {
                string fieldName = m_FieldNames.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(fieldName) || !m_Settings.m_FieldNameRegex.IsMatch(fieldName))
                {
                    Debug.LogErrorFormat("Field name '{0}' is invalid.", fieldName);
                    continue;
                }

                string componentTypeName = m_FieldComponents.GetArrayElementAtIndex(i).objectReferenceValue.GetType().Name;
                fieldTypeDict.Add(fieldName, componentTypeName);
            }

            Generator.GenerateComponentsCode(codeFileName, m_Settings.m_ComponentsCodeTemplate.text, m_NameSpace.stringValue, m_ClassName.stringValue, fieldTypeDict);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成 MonoBehaviour 代码。
        /// </summary>
        private void GenerateBehaviourCode()
        {
            if (Generator == null)
            {
                Debug.LogError("Generator is null.");
                return;
            }

            if (!CheckGenerateCondition())
            {
                return;
            }

            if (m_Settings.m_BehaviourCodeTemplate == null)
            {
                Debug.LogError("BehaviourCodeTemplate is null, Please check 'ComponentCollectionSettings' asset.");
                return;
            }

            string codeFileName = string.Format("{0}/{1}.cs", m_CodeSavePath.stringValue, m_ClassName.stringValue);
            if (!CheckGenerateFile(codeFileName))
            {
                return;
            }

            Generator.GenerateBehaviourCode(codeFileName, m_Settings.m_BehaviourCodeTemplate.text, m_NameSpace.stringValue, m_ClassName.stringValue);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 自动添加组件。
        /// </summary>
        private void AddBehaviourComponent()
        {
            string typeName = string.Format("{0}.{1}", m_NameSpace.stringValue, m_ClassName.stringValue);
            Type componentType = TypeUtility.GetRuntimeType(typeName);
            if (componentType == null)
            {
                Debug.LogWarningFormat("Can't load type '{0}'. Please check  whether 'TypeUtility.RuntimeAssemblyNames' contains the assembly name of this type? ", typeName);
                return;
            }
            ((Component) target).gameObject.AddComponent(componentType);
        }

        #endregion
    }
}

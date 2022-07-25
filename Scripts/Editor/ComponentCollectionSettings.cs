﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Editor
{
    [CreateAssetMenu(fileName = "ComponentCollectionSettings", menuName = "Game/Component Collection Settings", order = 1)]
    public class ComponentCollectionSettings : ScriptableObject
    {
        /// <summary>
        /// 组件类型映射。
        /// </summary>
        [Serializable]
        public class ComponentTypeMap
        {
            public string m_TypeKey;
            public string m_TypeName;

            public ComponentTypeMap(string typeKey, string typeName)
            {
                m_TypeKey = typeKey;
                m_TypeName = typeName;
            }
        }

        [Header("Assembly")]
        [Tooltip("编辑器程序集名称集合。")]
        public string[] m_EditorAssemblyNames = {"Assembly-CSharp-Editor", "ComponentCollection.Editor"};
        [Tooltip("运行时程序集名称集合。")]
        public string[] m_RuntimeAssemblyNames = {"Assembly-CSharp", "ComponentCollection"};

        [FormerlySerializedAs("m_Collector")]
        [Space(4)]
        [Header("Collect")]
        [Tooltip("ComponentCollector。")]
        public string m_CollectorTypeName = "Game.Editor.DefaultComponentCollector";
        [Tooltip("组件类型映射列表。")]
        public List<ComponentTypeMap> m_ComponentMaps = new List<ComponentTypeMap>
        {
            // Transform
            new ComponentTypeMap("Tran", "Transform"),
            new ComponentTypeMap("Rect", "RectTransform"),

            // Animation
            new ComponentTypeMap("Anim", "Animation"),
            new ComponentTypeMap("Animator", "Animator"),

            // Graphic
            new ComponentTypeMap("Text", "Text"),
            new ComponentTypeMap("Image", "Image"),
            new ComponentTypeMap("RawImage", "RawImage"),

            // Controls
            new ComponentTypeMap("Button", "Button"),
            new ComponentTypeMap("Toggle", "Toggle"),
            new ComponentTypeMap("TGroup", "ToggleGroup"),
            new ComponentTypeMap("Slider", "Slider"),
            new ComponentTypeMap("Scrollbar", "Scrollbar"),
            new ComponentTypeMap("Dropdown", "Dropdown"),
            new ComponentTypeMap("InputField", "InputField"),

            // Container
            new ComponentTypeMap("Canvas", "Canvas"),
            new ComponentTypeMap("ScrollView", "ScrollRect"),
            new ComponentTypeMap("CGroup", "CanvasGroup"),
            new ComponentTypeMap("GLGroup", "GridLayoutGroup"),
            new ComponentTypeMap("VLGroup", "VerticalLayoutGroup"),
            new ComponentTypeMap("HLGroup", "HorizontalLayoutGroup"),

            // Mask
            new ComponentTypeMap("Mask", "Mask"),
            new ComponentTypeMap("RectMask", "RectMask2D"),
        };
        [Tooltip("默认字段前缀。")]
        public string m_DefaultFieldNamePrefix = "m_";
        [Tooltip("字段名是否使用组件类型 默认值。")]
        public bool m_DefaultFieldNameByType = false;

        [FormerlySerializedAs("m_Generator")]
        [Space(4)]
        [Header("Generate")]
        [Tooltip("CollectionGenerator。")]
        public string m_GeneratorTypeName = "Game.Editor.DefaultCollectionGenerator";
        [Tooltip("默认命名空间。")]
        public string m_DefaultNameSpace = "Game";
        [Tooltip("默认代码保存地址。")]
        public string m_DefaultCodeSavePath = "Assets";
        [Tooltip("代码模板。")]
        public TextAsset m_ComponentsCodeTemplate;
        [Tooltip("代码模板。")]
        public TextAsset m_BehaviourCodeTemplate;

        private Dictionary<string, string> m_ComponentMapDict;

        /// <summary>
        /// 组件类型映射字典。
        /// </summary>
        public Dictionary<string, string> ComponentMapDict
        {
            get
            {
                if (m_ComponentMapDict == null)
                {
                    m_ComponentMapDict = new Dictionary<string, string>();
                    for (int i = m_ComponentMaps.Count - 1; i >= 0; i--)
                    {
                        ComponentTypeMap map = m_ComponentMaps[i];
                        if (m_ComponentMapDict.ContainsKey(map.m_TypeKey))
                        {
                            m_ComponentMaps.RemoveAt(i);
                            continue;
                        }
                        m_ComponentMapDict.Add(map.m_TypeKey, map.m_TypeName);
                    }
                }

                return m_ComponentMapDict;
            }
        }
    }
}

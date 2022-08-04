using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    [AddComponentMenu("Game/Component Collection")]
    [DisallowMultipleComponent]
    public sealed class ComponentCollection : MonoBehaviour
    {
#if UNITY_EDITOR
        /*
         * 编辑器使用的序列化数据。不会影响运行时逻辑。
         * 这些字段只能在编辑器宏中调用，否则打包会提示错误。一般也不会在外部调用。
         * 此脚本一般不会修改，能在外部访问的只有 GetComponent<T>(int) 函数。
         */
                
        // 是否已设置过默认值
        #pragma warning disable 0414
        [SerializeField] private bool m_Setup;
        
        // Collect settings
        [SerializeField] private string m_CollectorTypeName;
        [SerializeField] private string m_FieldNamePrefix;
        [SerializeField] private bool m_FieldNameByType;
        [SerializeField] private List<string> m_FieldNames;
        [SerializeField] private List<Component> m_FieldComponents;
        
        // Generate settings
        [SerializeField] private string m_GeneratorTypeName;
        [SerializeField] private string m_NameSpace;
        [SerializeField] private string m_ClassName;
        [SerializeField] private string m_CodeSavePath;

        private void Reset()
        {
            m_Setup = false;
        }
#endif
        
        [SerializeField] private List<Component> m_Components;

        public T GetComponent<T>(int index) where T : Component
        {
            if (index < 0 || index >= m_Components.Count)
            {
                Debug.LogError("Get component failed with invalid index.");
                return null;
            }

            T component = m_Components[index] as T;
            if (component == null)
            {
                Debug.LogErrorFormat("Get component failed with invalid type, index = {0}.", index);
                return null;
            }

            return component;
        }
    }
}

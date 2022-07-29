using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameExtension.Editor
{
    public class BatchEditor : EditorWindow
    {
        private const string DefaultTitleName = "Batch Editor";
        private const string NoneOptionName = "<None>";

        [SerializeField]
        private List<GameObject> m_TargetObjects = new List<GameObject>();
        private SerializedObject m_SerializedThis;
        private SerializedProperty m_SerializedObjects;
        private string[] m_HelperTypeNames;
        private int m_HelperTypeNameIndex;
        private BatchEditHelperBase m_Helper;

        [MenuItem("Game Extension/Batch Editor", false)]
        private static void ShowWindow()
        {
            var window = CreateWindow<BatchEditor>();
            window.titleContent = new GUIContent(DefaultTitleName);
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        private void OnEnable()
        {
            m_SerializedThis = new SerializedObject(this);
            m_SerializedObjects = m_SerializedThis.FindProperty("m_TargetObjects");

            List<string> helperTypeNames = new List<string> {NoneOptionName};
            helperTypeNames.AddRange(TypeUtility.GetEditorTypeNames(typeof(BatchEditHelperBase)));
            m_HelperTypeNames = helperTypeNames.ToArray();
            helperTypeNames.Clear();
        }

        private void OnGUI()
        {
            GUISelectHelper();
            GUISelectTarget();
            if (m_Helper != null)
            {
                m_Helper.CustomEditGUI();
            }
        }

        private void GUISelectTarget()
        {
            m_SerializedThis.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SerializedObjects, true);
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedThis.ApplyModifiedProperties();
            }
        }

        private void GUISelectHelper()
        {
            int helperTypeNameIndex = EditorGUILayout.Popup("Batch Helper", m_HelperTypeNameIndex, m_HelperTypeNames);
            if (helperTypeNameIndex != m_HelperTypeNameIndex)
            {
                m_HelperTypeNameIndex = helperTypeNameIndex;
                Type helperType = TypeUtility.GetEditorType(m_HelperTypeNames[m_HelperTypeNameIndex]);
                if (helperType != null)
                {
                    m_Helper = (BatchEditHelperBase) Activator.CreateInstance(helperType, m_TargetObjects);
                    titleContent.text = m_Helper.HelperName;
                }
                else
                {
                    m_Helper = null;
                    titleContent.text = DefaultTitleName;
                }
            }
        }
    }
}

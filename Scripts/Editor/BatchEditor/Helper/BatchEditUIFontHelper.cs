using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension.Editor
{
    public class BatchEditUIFontHelper : BatchEditHelperBase
    {
        private Font m_NewFont;
        private bool m_ModifyAllFont;
        private Font m_OldFont;
        private bool m_ModifyNullFont;

        public override string HelperName
        {
            get
            {
                return "Batch Font Editor";
            }
        }

        public BatchEditUIFontHelper(List<GameObject> targetObjects) : base(targetObjects)
        {
        }

        public override void CustomEditGUI()
        {
            m_NewFont = (Font) EditorGUILayout.ObjectField("New Font", m_NewFont, typeof(Font), false);
            m_ModifyAllFont = EditorGUILayout.Toggle("Modify All Font", m_ModifyAllFont);
            EditorGUI.BeginDisabledGroup(m_ModifyAllFont);
            {
                // EditorGUI.indentLevel++;
                m_OldFont = (Font) EditorGUILayout.ObjectField("Old Font", m_OldFont, typeof(Font), false);
                m_ModifyNullFont = EditorGUILayout.Toggle("Modify Null Font", m_ModifyNullFont);
                // EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Batch Modify"))
            {
                ModifyFonts();
            }
        }

        private void ModifyFonts()
        {
            if (m_NewFont == m_OldFont)
            {
                return;
            }

            if (m_NewFont == null)
            {
                Debug.LogError("New font is null.");
                return;
            }

            if (m_OldFont == null && !m_ModifyAllFont)
            {
                Debug.LogError("Old font is null. Please set or choose the modify all font option.");
                return;
            }

            for (int i = 0; i < m_TargetObjectObjects.Count; i++)
            {
                var item = m_TargetObjectObjects[i];
                if (item != null)
                {
                    EditorUtility.DisplayProgressBar("Modify Progress", item.name, (i + 1f) / m_TargetObjectObjects.Count);
                    ModifyFont(item);
                    if (PrefabUtility.SavePrefab(item))
                    {
                        Debug.LogFormat("Prefab save succeed: {0}.", item.name);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Prefab save failed: {0}.", item.name);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private void ModifyFont(GameObject target)
        {
            var texts = target.GetComponentsInChildren<Text>(true);
            foreach (var item in texts)
            {
                if (m_ModifyAllFont || item.font == m_OldFont || m_ModifyNullFont && item.font == null)
                {
                    item.font = m_NewFont;
                }
            }
            Debug.LogFormat("Prefab modify succeed: {0}.", target.name);
        }
    }
}

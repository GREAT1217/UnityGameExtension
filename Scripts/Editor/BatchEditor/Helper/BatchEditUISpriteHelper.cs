using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension.Editor
{
    public class BatchEditUISpriteHelper : BatchEditHelperBase
    {
        private Sprite m_OldSprite;
        private Sprite m_NewSprite;
        private bool m_ModifyNullSprite;
        private readonly GUILayoutOption m_SpriteFieldHeight;

        public override string HelperName
        {
            get
            {
                return "Batch Sprite Editor";
            }
        }

        public BatchEditUISpriteHelper(List<GameObject> targetObjects) : base(targetObjects)
        {
            m_SpriteFieldHeight = GUILayout.Height(18);
        }

        public override void CustomEditGUI()
        {
            m_OldSprite = (Sprite) EditorGUILayout.ObjectField("Old Sprite", m_OldSprite, typeof(Sprite), false, m_SpriteFieldHeight);
            m_NewSprite = (Sprite) EditorGUILayout.ObjectField("New Sprite", m_NewSprite, typeof(Sprite), false, m_SpriteFieldHeight);
            m_ModifyNullSprite = EditorGUILayout.Toggle("Modify Null Sprite", m_ModifyNullSprite);

            if (GUILayout.Button("Batch Modify"))
            {
                ModifySprites();
            }
        }

        private void ModifySprites()
        {
            if (m_NewSprite == m_OldSprite)
            {
                return;
            }

            if (m_NewSprite == null)
            {
                Debug.LogError("New sprite is null.");
                return;
            }

            if (m_OldSprite == null)
            {
                Debug.LogError("Old sprite is null.");
                return;
            }

            for (int i = 0; i < m_TargetObjectObjects.Count; i++)
            {
                var item = m_TargetObjectObjects[i];
                if (item != null)
                {
                    EditorUtility.DisplayProgressBar("Modify Progress", item.name, (i + 1f) / m_TargetObjectObjects.Count);
                    ModifySprite(item);
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

        private void ModifySprite(GameObject target)
        {
            var texts = target.GetComponentsInChildren<Image>(true);
            foreach (var item in texts)
            {
                if (item.sprite == m_OldSprite || m_ModifyNullSprite && item.sprite == null)
                {
                    item.sprite = m_NewSprite;
                }
            }
            Debug.LogFormat("Prefab modify succeed: {0}.", target.name);
        }
    }
}

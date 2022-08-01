using UnityEditor;
using UnityEngine;

namespace GameExtension.Editor
{
    public static class PrefabUtility
    {
        public static bool SavePrefab(GameObject prefab)
        {
            if (UnityEditor.PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                return false;
            }

            string prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                return false;
            }

            bool saveResult;
            string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(prefabAssetPath))
            {
                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabPath, InteractionMode.AutomatedAction, out saveResult);
                return saveResult;
            }

            UnityEditor.PrefabUtility.SavePrefabAsset(prefab, out saveResult);
            return saveResult;
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension.Editor
{
    public class UGUIRaycastTargetDebug : UnityEditor.Editor
    {
        private const string MenuItemName = "Game Extension/Debug Raycast Target";
        private const string PrefsKey = "DebugRaycastTarget";

        private static bool s_DebugRaycastTarget;
        private static readonly Color s_DebugFaceColor = new Color(0f, 1f, 0f, 0.2f);
        private static readonly Color s_DebugOutlineColor = new Color(0f, 1f, 0f, 1f);

        [InitializeOnLoadMethod]
        private static void Init()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += DrawRaycastTargetGUI;
#else
        SceneView.onSceneGUIDelegate += DrawRaycastTargetGUI;
#endif
            s_DebugRaycastTarget = EditorPrefs.GetBool(PrefsKey, false);
            Menu.SetChecked(MenuItemName, s_DebugRaycastTarget);
        }

        [MenuItem(MenuItemName, false, 0)]
        public static void SwitchDebugRaycastTarget()
        {
            s_DebugRaycastTarget = !s_DebugRaycastTarget;
            EditorPrefs.SetBool(PrefsKey, s_DebugRaycastTarget);
            Menu.SetChecked(MenuItemName, s_DebugRaycastTarget);
        }

        private static void DrawRaycastTargetGUI(SceneView sceneView)
        {
            if (!s_DebugRaycastTarget)
            {
                return;
            }

            Vector3[] fourCornerArray = new Vector3[4];
            foreach (var graphic in Object.FindObjectsOfType<MaskableGraphic>())
            {
                if (graphic.raycastTarget)
                {
                    graphic.rectTransform.GetWorldCorners(fourCornerArray);
                    Handles.DrawSolidRectangleWithOutline(fourCornerArray, s_DebugFaceColor, s_DebugOutlineColor);
                }
            }
        }
    }
}

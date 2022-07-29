using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// copy from MenuOptions of https://github.com/Unity-Technologies/uGUI.git
namespace GameExtension.Editor
{
    /// <summary>
    /// This script adds the UI menu options to the Unity Editor.
    /// </summary>
    internal static class UGUIMenuExtension
    {
        private const string kUILayerName = "UI";
        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";

        private static UGUICreator.Resources s_StandardResources;

        private static UGUICreator.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }
            return s_StandardResources;
        }

        private static void SetPositionVisibleInSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
#if UNITY_2018_3_OR_NEWER
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;

                // If in Prefab Mode, Canvas has to be part of Prefab contents,
                // otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                // Create canvas under context GameObject,
                // and make that be the parent which UI element is added under.
                GameObject canvas = CreateNewUI();
                canvas.transform.SetParent(parent.transform, false);
                parent = canvas;
            }

            // Setting the element to be a child of an element already in the scene should
            // be sufficient to also move the element to that scene.
            // However, it seems the element needs to be already in its destination scene when the
            // RegisterCreatedObjectUndo is performed; otherwise the scene it was created in is dirtied.
            SceneManager.MoveGameObjectToScene(element, parent.scene);

            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);

            if (element.transform.parent == null)
            {
                Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);

            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + element.name);

            GameObjectUtility.SetParentAndAlign(element, parent);
            if (!explicitParentChoice) // not a context click, so center in sceneview
                SetPositionVisibleInSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
#else
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleInSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
#endif
        }

        // Graphic elements

        [MenuItem("GameObject/Custom UI/Text", false, 0)]
        public static void AddText(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateText(false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Text - Raycast", false, 1)]
        public static void AddTextRaycast(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateText(true);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Image", false, 6)]
        public static void AddImage(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateImage(false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Image - Raycast", false, 7)]
        public static void AddImageRaycast(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateImage(true);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/RawImage", false, 8)]
        public static void AddRawImage(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateRawImage(false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/RawImage - Raycast", false, 9)]
        public static void AddRawImageRaycast(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateRawImage(true);
            PlaceUIElementRoot(go, menuCommand);
        }

        // Button and toggle are controls you just click on.

        [MenuItem("GameObject/Custom UI/Button", false, 30)]
        public static void AddButton(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateButton(GetStandardResources(), false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Button - Text", false, 31)]
        public static void AddButtonWithText(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateButton(GetStandardResources(), true);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Toggle", false, 36)]
        public static void AddToggle(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateToggle(GetStandardResources(), false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Toggle - Text", false, 38)]
        public static void AddToggleWithText(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateToggle(GetStandardResources(), true);
            PlaceUIElementRoot(go, menuCommand);
        }

        // Slider and Scrollbar modify a number

        [MenuItem("GameObject/Custom UI/Slider", false, 38)]
        public static void AddSlider(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateSlider(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Scrollbar", false, 39)]
        public static void AddScrollbar(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateScrollbar(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // More advanced controls below

        [MenuItem("GameObject/Custom UI/InputField", false, 40)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Containers

        [MenuItem("GameObject/Custom UI/Canvas", false, 60)]
        public static void AddCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }
            Selection.activeGameObject = go;
        }

        // [MenuItem("GameObject/Custom UI/Panel", false, 62)]
        // public static void AddPanel(MenuCommand menuCommand)
        // {
        //     GameObject go = UGUICreator.CreatePanel(GetStandardResources());
        //     PlaceUIElementRoot(go, menuCommand);
        //
        //     // Panel is special, we need to ensure there's no padding after repositioning.
        //     RectTransform rect = go.GetComponent<RectTransform>();
        //     rect.anchoredPosition = Vector2.zero;
        //     rect.sizeDelta = Vector2.zero;
        // }

        [MenuItem("GameObject/Custom UI/ScrollView - Mask", false, 66)]
        public static void AddScrollViewUseMask(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateScrollView(GetStandardResources(), true);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/ScrollView - RectMask", false, 67)]
        public static void AddScrollViewUseRectMask(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateScrollView(GetStandardResources(), false);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Dropdown - Mask", false, 68)]
        public static void AddDropdownUseMask(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateDropdown(GetStandardResources(), true);
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/Dropdown - RectMask", false, 69)]
        public static void AddDropdownUseRectMask(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateDropdown(GetStandardResources(), false);
            PlaceUIElementRoot(go, menuCommand);
        }

        // Helper methods

        private static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

#if UNITY_2018_3_OR_NEWER
            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                root.transform.SetParent(prefabStage.prefabContentsRoot.transform, false);
                customScene = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // If there is no event system add one...
            // No need to place event system in custom scene as these are temporary anyway.
            // It can be argued for or against placing it in the user scenes,
            // but let's not modify scene user is not currently looking at.
            if (!customScene)
                CreateEventSystem(false);
#else
            // if there is no event system add one...
            CreateEventSystem(false);
#endif

            return root;
        }

        [MenuItem("GameObject/Custom UI/EventSystem", false, 100)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
#if UNITY_2018_3_OR_NEWER
            StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
            var esys = stage.FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                if (parent == null)
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                else
                    GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }
#else
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }
#endif

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

#if UNITY_2018_3_OR_NEWER
            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas))
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
                if (IsValidCanvas(canvasArray[i]))
                    return canvasArray[i].gameObject;
#else
            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;
#endif

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

#if UNITY_2018_3_OR_NEWER
        private static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            // It's important that the non-editable canvas from a prefab scene won't be rejected,
            // but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
                return false;

            return true;
        }
#endif

        // Extension

        [MenuItem("GameObject/Custom UI/RectRaycast", false, 10)]
        public static void AddRectRaycast(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateRectRaycast();
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/Custom UI/RectButton", false, 11)]
        public static void AddRectButton(MenuCommand menuCommand)
        {
            GameObject go = UGUICreator.CreateRectButton();
            PlaceUIElementRoot(go, menuCommand);
        }
    }
}

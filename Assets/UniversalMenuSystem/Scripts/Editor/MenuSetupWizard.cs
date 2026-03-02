// ============================================================================
// Universal Menu System - Editor Setup Wizard
// Compatible with Unity 6000.0+
// ============================================================================
// Editor window that scaffolds the entire menu hierarchy in scene.
// Creates Canvas, panels, text buttons, and wires everything up.
// Access via menu: Tools > Universal Menu System > Setup Wizard
// ============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;
using UniversalMenuSystem.Menus;
using UniversalMenuSystem.Settings;
using UniversalMenuSystem.SaveLoad;
using UniversalMenuSystem.UI;

namespace UniversalMenuSystem.Editor
{
    public class MenuSetupWizard : EditorWindow
    {
        private string gameTitle = "YOUR GAME TITLE";
        private string gameplaySceneName = "Gameplay";
        private bool createSaveLoadManager = true;
        private bool addHorrorEffects = true;
        private bool addAudioController = true;
        private Color backgroundColor = new Color(0f, 0f, 0f, 0.85f);
        private Color textColor = new Color(0.65f, 0.62f, 0.58f, 1f);
        private Color hoverColor = new Color(0.92f, 0.87f, 0.78f, 1f);
        private int fontSize = 36;
        private int titleFontSize = 72;

        [MenuItem("Tools/Universal Menu System/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<MenuSetupWizard>("Menu System Setup");
            window.minSize = new Vector2(400, 550);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Universal Menu System", EditorStyles.boldLabel);
            GUILayout.Label("Horror / Walking Simulator Style", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This wizard creates the full menu UI hierarchy in your scene.\n" +
                "It scaffolds Canvas, panels, text buttons, and wires all components.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Configuration
            GUILayout.Label("Game Configuration", EditorStyles.boldLabel);
            gameTitle = EditorGUILayout.TextField("Game Title", gameTitle);
            gameplaySceneName = EditorGUILayout.TextField("Gameplay Scene Name", gameplaySceneName);

            EditorGUILayout.Space(5);
            GUILayout.Label("Visual Style", EditorStyles.boldLabel);
            backgroundColor = EditorGUILayout.ColorField("Background Overlay", backgroundColor);
            textColor = EditorGUILayout.ColorField("Text Color (Normal)", textColor);
            hoverColor = EditorGUILayout.ColorField("Text Color (Hover)", hoverColor);
            fontSize = EditorGUILayout.IntSlider("Button Font Size", fontSize, 18, 72);
            titleFontSize = EditorGUILayout.IntSlider("Title Font Size", titleFontSize, 36, 120);

            EditorGUILayout.Space(5);
            GUILayout.Label("Features", EditorStyles.boldLabel);
            createSaveLoadManager = EditorGUILayout.Toggle("Save/Load System", createSaveLoadManager);
            addHorrorEffects = EditorGUILayout.Toggle("Horror UI Effects", addHorrorEffects);
            addAudioController = EditorGUILayout.Toggle("Audio Controller", addAudioController);

            EditorGUILayout.Space(15);

            // Build button
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("BUILD MENU SYSTEM", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Build Menu System",
                    "This will create the full menu hierarchy in the current scene.\nContinue?",
                    "Build", "Cancel"))
                {
                    BuildMenuSystem();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Create Save Slot Prefab"))
            {
                CreateSaveSlotPrefab();
            }
        }

        private void BuildMenuSystem()
        {
            Undo.SetCurrentGroupName("Build Universal Menu System");
            int undoGroup = Undo.GetCurrentGroup();

            // ── 1. System Root ───────────────────────────────────────────────
            GameObject systemRoot = new GameObject("[MenuSystem]");
            Undo.RegisterCreatedObjectUndo(systemRoot, "Create MenuSystem Root");

            // GameManager
            var gm = systemRoot.AddComponent<GameManager>();
            SerializedObject gmSo = new SerializedObject(gm);
            gmSo.FindProperty("newGameScene") ?.stringValue = gameplaySceneName;
            gmSo.ApplyModifiedPropertiesWithoutUndo();

            // GameSettings
            systemRoot.AddComponent<GameSettings>();

            // SaveLoadManager
            if (createSaveLoadManager)
                systemRoot.AddComponent<SaveLoadManager>();

            // AudioController
            if (addAudioController)
                systemRoot.AddComponent<MenuAudioController>();

            // ── 2. Canvas ──────────────────────────────────────────────────
            GameObject canvasObj = new GameObject("MenuCanvas");
            canvasObj.transform.SetParent(systemRoot.transform);
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // ResolutionManager
            canvasObj.AddComponent<ResolutionManager>();

            // Safe area panel
            GameObject safeArea = CreatePanel(canvasObj, "SafeArea", true);
            StretchRectTransform(safeArea.GetComponent<RectTransform>());

            // ── 3. MenuManager ─────────────────────────────────────────────
            var menuManager = canvasObj.AddComponent<MenuManager>();
            SerializedObject mmSo = new SerializedObject(menuManager);

            // ── 4. Background Overlay ──────────────────────────────────────
            GameObject bgOverlay = CreatePanel(safeArea, "BackgroundOverlay", false);
            StretchRectTransform(bgOverlay.GetComponent<RectTransform>());
            var bgImage = bgOverlay.AddComponent<Image>();
            bgImage.color = backgroundColor;
            bgImage.raycastTarget = true;
            var bgCG = bgOverlay.GetComponent<CanvasGroup>() ?? bgOverlay.AddComponent<CanvasGroup>();

            mmSo.FindProperty("backgroundOverlay").objectReferenceValue = bgCG;

            // ── 5. Create All Panels ───────────────────────────────────────
            GameObject mainMenuPanel = CreateMenuPanel(safeArea, "MainMenuPanel");
            GameObject pauseMenuPanel = CreateMenuPanel(safeArea, "PauseMenuPanel");
            GameObject settingsPanel = CreateMenuPanel(safeArea, "SettingsPanel");
            GameObject saveMenuPanel = CreateMenuPanel(safeArea, "SaveMenuPanel");
            GameObject loadMenuPanel = CreateMenuPanel(safeArea, "LoadMenuPanel");
            GameObject confirmQuitPanel = CreateMenuPanel(safeArea, "ConfirmQuitPanel");
            GameObject creditsPanel = CreateMenuPanel(safeArea, "CreditsPanel");

            // Wire panels to MenuManager
            mmSo.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("pauseMenuPanel").objectReferenceValue = pauseMenuPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("saveMenuPanel").objectReferenceValue = saveMenuPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("loadMenuPanel").objectReferenceValue = loadMenuPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("confirmQuitPanel").objectReferenceValue = confirmQuitPanel.GetComponent<CanvasGroup>();
            mmSo.FindProperty("creditsPanel").objectReferenceValue = creditsPanel.GetComponent<CanvasGroup>();
            mmSo.ApplyModifiedPropertiesWithoutUndo();

            // ── 6. Populate Main Menu ──────────────────────────────────────
            PopulateMainMenu(mainMenuPanel);

            // ── 7. Populate Pause Menu ─────────────────────────────────────
            PopulatePauseMenu(pauseMenuPanel);

            // ── 8. Populate Settings Panel ─────────────────────────────────
            PopulateSettingsPanel(settingsPanel);

            // ── 9. Populate Save & Load Panels ─────────────────────────────
            PopulateSaveLoadPanel(saveMenuPanel, "SAVE GAME", true);
            PopulateSaveLoadPanel(loadMenuPanel, "LOAD GAME", false);

            // ── 10. Populate Confirm Quit ──────────────────────────────────
            PopulateConfirmQuit(confirmQuitPanel);

            // ── 11. Populate Credits ───────────────────────────────────────
            PopulateCreditsPanel(creditsPanel);

            // ── 12. EventSystem ────────────────────────────────────────────
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("[MenuSetupWizard] ✓ Menu system built successfully! Assign your fonts and audio clips in the Inspector.");
            Selection.activeGameObject = systemRoot;
        }

        // ── Panel Builders ───────────────────────────────────────────────────

        private void PopulateMainMenu(GameObject panel)
        {
            var controller = panel.AddComponent<MainMenuController>();
            SerializedObject so = new SerializedObject(controller);

            // Title (top-right area, RE style)
            var titleObj = CreateTextElement(panel, "TitleText", gameTitle, titleFontSize,
                TextAlignmentOptions.Right);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.8f);
            titleRT.anchorMax = new Vector2(0.95f, 0.95f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            so.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("gameTitle").stringValue = gameTitle;

            if (addHorrorEffects)
            {
                var typewriter = titleObj.AddComponent<TypewriterEffect>();
                titleObj.AddComponent<AmbientFlicker>();
            }

            // Button container (right-aligned, vertical)
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panel.transform, false);
            var containerRT = buttonContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.6f, 0.15f);
            containerRT.anchorMax = new Vector2(0.92f, 0.7f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            var vlg = buttonContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.UpperRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            if (addHorrorEffects)
                buttonContainer.AddComponent<StaggeredReveal>();

            // Create menu buttons
            var newGameBtn = CreateMenuButton(buttonContainer, "New Game");
            var continueBtn = CreateMenuButton(buttonContainer, "Continue");
            var loadBtn = CreateMenuButton(buttonContainer, "Load Game");
            var settingsBtn = CreateMenuButton(buttonContainer, "Settings");
            var quitBtn = CreateMenuButton(buttonContainer, "Quit");

            so.FindProperty("newGameButton").objectReferenceValue = newGameBtn.GetComponent<Button>();
            so.FindProperty("continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            so.FindProperty("loadGameButton").objectReferenceValue = loadBtn.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            so.FindProperty("newGameScene").stringValue = gameplaySceneName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PopulatePauseMenu(GameObject panel)
        {
            var controller = panel.AddComponent<PauseMenuController>();
            SerializedObject so = new SerializedObject(controller);

            // Header
            CreateTextElement(panel, "PauseHeaderText", "PAUSED", titleFontSize / 2,
                TextAlignmentOptions.Center, new Vector2(0.3f, 0.8f), new Vector2(0.7f, 0.92f));

            // Button container (centered)
            GameObject container = new GameObject("ButtonContainer");
            container.transform.SetParent(panel.transform, false);
            var cRT = container.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.3f, 0.15f);
            cRT.anchorMax = new Vector2(0.7f, 0.75f);
            cRT.offsetMin = Vector2.zero;
            cRT.offsetMax = Vector2.zero;

            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            if (addHorrorEffects)
                container.AddComponent<StaggeredReveal>();

            var resumeBtn = CreateMenuButton(container, "Resume");
            var saveBtn = CreateMenuButton(container, "Save Game");
            var loadBtn = CreateMenuButton(container, "Load Game");
            var settingsBtn = CreateMenuButton(container, "Settings");
            var mainMenuBtn = CreateMenuButton(container, "Main Menu");
            var quitBtn = CreateMenuButton(container, "Quit");

            so.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
            so.FindProperty("saveGameButton").objectReferenceValue = saveBtn.GetComponent<Button>();
            so.FindProperty("loadGameButton").objectReferenceValue = loadBtn.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuBtn.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PopulateSettingsPanel(GameObject panel)
        {
            var controller = panel.AddComponent<SettingsMenuController>();

            // Header
            CreateTextElement(panel, "SettingsHeaderText", "SETTINGS", titleFontSize / 2,
                TextAlignmentOptions.Center, new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.96f));

            // Back button (bottom)
            var backBtnObj = CreateMenuButton(panel, "Back", 28);
            var backRT = backBtnObj.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0.05f, 0.02f);
            backRT.anchorMax = new Vector2(0.2f, 0.08f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;

            // Note: Full settings wiring requires Inspector assignment
            // The wizard creates the structure; user assigns SliderS, Toggles, Dropdowns.
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("backButton").objectReferenceValue = backBtnObj.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Placeholder content text
            CreateTextElement(panel, "SettingsPlaceholder",
                "Tab contents go here.\n\nAssign Display / Audio / Gameplay / Accessibility\npanels and controls in the Inspector.",
                22, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.85f));
        }

        private void PopulateSaveLoadPanel(GameObject panel, string header, bool isSave)
        {
            if (isSave)
                panel.AddComponent<SaveMenuController>();
            else
                panel.AddComponent<LoadMenuController>();

            CreateTextElement(panel, "HeaderText", header, titleFontSize / 2,
                TextAlignmentOptions.Center, new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.96f));

            // Slot container (scrollable area)
            GameObject slotArea = new GameObject("SlotContainer");
            slotArea.transform.SetParent(panel.transform, false);
            var saRT = slotArea.AddComponent<RectTransform>();
            saRT.anchorMin = new Vector2(0.1f, 0.12f);
            saRT.anchorMax = new Vector2(0.9f, 0.85f);
            saRT.offsetMin = Vector2.zero;
            saRT.offsetMax = Vector2.zero;

            var vlg = slotArea.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Placeholder text
            CreateTextElement(slotArea, "PlaceholderText",
                "Save slots will be spawned here.\nAssign a SaveSlotUI prefab in the Inspector.",
                20, TextAlignmentOptions.Center);

            // Back button
            var backBtnObj = CreateMenuButton(panel, "Back", 28);
            var backRT = backBtnObj.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0.05f, 0.02f);
            backRT.anchorMax = new Vector2(0.2f, 0.08f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
        }

        private void PopulateConfirmQuit(GameObject panel)
        {
            var controller = panel.AddComponent<ConfirmQuitController>();
            SerializedObject so = new SerializedObject(controller);

            // Prompt text
            var promptObj = CreateTextElement(panel, "PromptText", "Are you sure you want to quit?",
                fontSize, TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.7f));
            so.FindProperty("promptText").objectReferenceValue = promptObj.GetComponent<TextMeshProUGUI>();

            // Yes / No buttons (horizontal)
            GameObject btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(panel.transform, false);
            var rowRT = btnRow.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.25f, 0.3f);
            rowRT.anchorMax = new Vector2(0.75f, 0.45f);
            rowRT.offsetMin = Vector2.zero;
            rowRT.offsetMax = Vector2.zero;

            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 80;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            var yesBtn = CreateMenuButton(btnRow, "Yes", fontSize);
            var noBtn = CreateMenuButton(btnRow, "No", fontSize);

            so.FindProperty("yesButton").objectReferenceValue = yesBtn.GetComponent<Button>();
            so.FindProperty("noButton").objectReferenceValue = noBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void PopulateCreditsPanel(GameObject panel)
        {
            panel.AddComponent<CreditsController>();

            // Back button
            var backBtnObj = CreateMenuButton(panel, "Back", 28);
            var backRT = backBtnObj.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0.05f, 0.02f);
            backRT.anchorMax = new Vector2(0.2f, 0.08f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;

            // Credits text area
            CreateTextElement(panel, "CreditsText",
                "Credits content is configured on the CreditsController component.",
                22, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
        }

        // ── Prefab Builders ─────────────────────────────────────────────────

        private void CreateSaveSlotPrefab()
        {
            // Build a save slot UI as a prefab
            GameObject slotObj = new GameObject("SaveSlotUI");
            var rt = slotObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 80);

            var slotUI = slotObj.AddComponent<SaveSlotUI>();
            var cg = slotObj.AddComponent<CanvasGroup>();

            // Data container
            GameObject dataContainer = new GameObject("DataContainer");
            dataContainer.transform.SetParent(slotObj.transform, false);
            StretchRectTransform(dataContainer.AddComponent<RectTransform>());

            var slotNameObj = CreateTextElement(dataContainer, "SlotName", "SLOT 0", 28,
                TextAlignmentOptions.Left, new Vector2(0.02f, 0.5f), new Vector2(0.3f, 1f));
            var timestampObj = CreateTextElement(dataContainer, "Timestamp", "2026-01-01", 20,
                TextAlignmentOptions.Left, new Vector2(0.32f, 0.5f), new Vector2(0.65f, 1f));
            var locationObj = CreateTextElement(dataContainer, "Location", "Chapter 1", 20,
                TextAlignmentOptions.Left, new Vector2(0.02f, 0f), new Vector2(0.5f, 0.5f));
            var playTimeObj = CreateTextElement(dataContainer, "PlayTime", "00:45:12", 20,
                TextAlignmentOptions.Right, new Vector2(0.7f, 0.5f), new Vector2(0.98f, 1f));

            // Delete button
            var deleteBtn = CreateMenuButton(dataContainer, "Delete", 18);
            var delRT = deleteBtn.GetComponent<RectTransform>();
            delRT.anchorMin = new Vector2(0.85f, 0f);
            delRT.anchorMax = new Vector2(0.98f, 0.5f);
            delRT.offsetMin = Vector2.zero;
            delRT.offsetMax = Vector2.zero;

            // Empty container
            GameObject emptyContainer = new GameObject("EmptyContainer");
            emptyContainer.transform.SetParent(slotObj.transform, false);
            StretchRectTransform(emptyContainer.AddComponent<RectTransform>());
            var emptyLabel = CreateTextElement(emptyContainer, "EmptyLabel", "— Empty Slot —", 24,
                TextAlignmentOptions.Center);
            emptyContainer.SetActive(false);

            // Main button
            var mainBtn = slotObj.AddComponent<Button>();
            mainBtn.transition = Selectable.Transition.None; // we handle visuals via TextHoverEffect

            // Wire SaveSlotUI references
            SerializedObject so = new SerializedObject(slotUI);
            so.FindProperty("slotNameText").objectReferenceValue = slotNameObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("timestampText").objectReferenceValue = timestampObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("locationText").objectReferenceValue = locationObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("playTimeText").objectReferenceValue = playTimeObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("emptyLabel").objectReferenceValue = emptyLabel.GetComponent<TextMeshProUGUI>();
            so.FindProperty("slotButton").objectReferenceValue = mainBtn;
            so.FindProperty("deleteButton").objectReferenceValue = deleteBtn.GetComponent<Button>();
            so.FindProperty("dataContainer").objectReferenceValue = dataContainer;
            so.FindProperty("emptyContainer").objectReferenceValue = emptyContainer;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string prefabDir = "Assets/UniversalMenuSystem/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/UniversalMenuSystem", "Prefabs");
            }

            string prefabPath = $"{prefabDir}/SaveSlotUI.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(slotObj, prefabPath, InteractionMode.UserAction);
            DestroyImmediate(slotObj);

            Debug.Log($"[MenuSetupWizard] ✓ Save slot prefab created at: {prefabPath}");
        }

        // ── Utility Methods ─────────────────────────────────────────────────

        private GameObject CreatePanel(GameObject parent, string name, bool addCanvasGroup)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            StretchRectTransform(rt);

            if (addCanvasGroup)
                panel.AddComponent<CanvasGroup>();

            return panel;
        }

        private GameObject CreateMenuPanel(GameObject parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            StretchRectTransform(rt);

            var cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            panel.SetActive(false);

            return panel;
        }

        private GameObject CreateMenuButton(GameObject parent, string label, int size = -1)
        {
            if (size < 0) size = fontSize;

            GameObject btnObj = new GameObject(label.Replace(" ", "") + "Button");
            btnObj.transform.SetParent(parent.transform, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, size + 20);

            // TMP text
            var tmp = btnObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = size;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.fontStyle = FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = true;

            // Button component (no visual transition — text-only style)
            var btn = btnObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Horror hover effect
            if (addHorrorEffects)
            {
                var hover = btnObj.AddComponent<TextHoverEffect>();
                SerializedObject hSo = new SerializedObject(hover);
                hSo.FindProperty("normalColor").colorValue = textColor;
                hSo.FindProperty("hoverColor").colorValue = hoverColor;
                hSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Layout element for proper sizing in layout groups
            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = size + 20;

            return btnObj;
        }

        private GameObject CreateTextElement(GameObject parent, string name, string text,
            int size, TextAlignmentOptions alignment,
            Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            var rt = obj.AddComponent<RectTransform>();
            if (anchorMin.HasValue)
            {
                rt.anchorMin = anchorMin.Value;
                rt.anchorMax = anchorMax ?? Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                StretchRectTransform(rt);
            }

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = textColor;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;

            return obj;
        }

        private void StretchRectTransform(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
#endif

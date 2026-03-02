# AGENT.md — AI Agent Instructions for Universal Menu System

This file contains instructions for AI coding agents (GitHub Copilot, Cursor, Cline, etc.) to set up, modify, and extend the Universal Menu System in a fresh Unity project.

---

## Project Overview

- **What**: A complete, game-agnostic menu system for Unity with horror/walking simulator aesthetics (Resident Evil-style)
- **Engine**: Unity 6000.0+ (Unity 6)
- **Language**: C#
- **Only dependency**: TextMeshPro (bundled with Unity)
- **Architecture**: Singleton managers, navigation stack, payload-agnostic save system

---

## Setting Up From Scratch

### Step 1: Create a Unity Project

1. Open Unity Hub
2. Create a new project using **Unity 6000.0+** (any template: 3D, URP, or HDRP)
3. Ensure TextMeshPro is installed (it ships with Unity by default; if prompted to import TMP Essentials, accept)

### Step 2: Import This Package

1. Clone this repository or download the ZIP
2. Copy the entire `Assets/UniversalMenuSystem/` folder into your Unity project's `Assets/` directory
3. Unity will auto-compile the scripts

### Step 3: Run the Setup Wizard

1. In Unity Editor, go to **Tools → Universal Menu System → Setup Wizard**
2. Configure:
   - **Game Title**: Your game's name (displays on main menu)
   - **Gameplay Scene Name**: The scene to load when "New Game" is clicked
   - **Visual Style**: Adjust colors and font sizes if desired
   - **Features**: Toggle Save/Load, Horror Effects, Audio Controller
3. Click **BUILD MENU SYSTEM**
4. Click **Create Save Slot Prefab** (for save/load slot UI)

### Step 4: Post-Setup Essentials

#### Assign Fonts
The wizard creates all `TextMeshProUGUI` elements with Unity's default TMP font. For the intended horror aesthetic, assign a TMP font asset to all text elements. Recommended fonts:
- Bebas Neue (clean tall caps)
- Cinzel (elegant serif)
- Special Elite (typewriter feel)
- IM Fell English (old-world serif)

To create a TMP font asset: **Window → TextMeshPro → Font Asset Creator**, import a `.ttf`/`.otf` file.

#### Assign Audio Clips
Select the `[MenuSystem]` GameObject → `MenuAudioController` component and assign:
- `Hover Sound` — subtle click or whisper
- `Select Sound` — deeper click or mechanical latch
- `Back Sound` — soft reverse click
- `Menu Ambience` — low drone, wind, eerie hum (set to loop)

#### Register Scenes in Build Settings
Go to **File → Build Settings** and add:
1. Your main menu scene (containing the menu system)
2. Your gameplay scene(s)

---

## Key Architecture Decisions

### Singletons
These managers use `DontDestroyOnLoad` and the singleton pattern:
- `MenuManager.Instance` — central hub for all menu transitions
- `GameManager.Instance` — scene tracking, pause toggle (Escape key)
- `GameSettings.Instance` — settings persistence via PlayerPrefs
- `SaveLoadManager.Instance` — slot-based save/load with injectable backend
- `MenuAudioController.Instance` — UI sounds and ambient loop

### Navigation Stack
`MenuManager` uses a `Stack<MenuState>` for browser-like back-navigation:
```csharp
MenuManager.Instance.NavigateTo(MenuState.Settings); // pushes current to stack
MenuManager.Instance.GoBack();                       // pops and transitions back
MenuManager.Instance.CloseAllMenus();                // clears stack, hides all
```

### MenuState Enum
```csharp
public enum MenuState { None, MainMenu, PauseMenu, Settings, SaveMenu, LoadMenu, ConfirmQuit, Credits }
```
Extend this enum and add a corresponding panel in `MenuManager` when adding new menus.

### Save System
The save system is **payload-agnostic**. Games provide a `string` payload (typically JSON) and receive it back on load:
```csharp
// Saving: provide callbacks on SaveMenuController
saveMenu.GetSavePayload = () => JsonUtility.ToJson(myGameState);
saveMenu.GetLocationName = () => SceneManager.GetActiveScene().name;
saveMenu.GetPlayTime = () => Time.timeSinceLevelLoad;

// Loading: handle the loaded payload on LoadMenuController
loadMenu.OnPayloadLoaded = (payload, slotData) => {
    var state = JsonUtility.FromJson<MyGameState>(payload);
    SceneManager.LoadScene(state.sceneName);
};
```

Replace the default file-based backend by implementing `ISaveProvider`:
```csharp
SaveLoadManager.Instance.Provider = new MyCloudSaveProvider();
```

---

## Namespaces

| Namespace | Purpose |
|---|---|
| `UniversalMenuSystem.Core` | MenuManager, GameManager, MenuState, MenuAudioController, ResolutionManager |
| `UniversalMenuSystem.Menus` | MainMenuController, PauseMenuController, SettingsMenuController, SaveMenuController, LoadMenuController, ConfirmQuitController, CreditsController |
| `UniversalMenuSystem.Settings` | GameSettings, SettingsData |
| `UniversalMenuSystem.SaveLoad` | SaveLoadManager, SaveSlotData, SaveSlotUI, ISaveProvider |
| `UniversalMenuSystem.UI` | TextHoverEffect, CanvasGroupFader, TypewriterEffect, StaggeredReveal, AmbientFlicker |
| `UniversalMenuSystem.Editor` | MenuSetupWizard (editor-only, `#if UNITY_EDITOR`) |

---

## Common Agent Tasks

### Adding a New Menu Panel

1. Add a new value to the `MenuState` enum in `Scripts/Core/MenuState.cs`
2. In `MenuManager.cs`:
   - Add a `[SerializeField] private CanvasGroup newPanel;` field
   - Add a case in `GetPanelForState()` to return it
3. Create a new controller script in `Scripts/Menus/`
4. In the Setup Wizard or manually: create the panel GameObject under SafeArea with a CanvasGroup (alpha=0, interactable=false, inactive)
5. Wire the reference in MenuManager's Inspector

### Modifying Settings

1. Add new fields to `SettingsData.cs` (the `[Serializable]` struct)
2. Update `GameSettings.cs` to save/load the new fields to PlayerPrefs and apply them
3. Update `SettingsMenuController.cs` to add UI controls for the new fields

### Changing Visual Style

All horror effects are configurable via Inspector:
- `TextHoverEffect`: normalColor, hoverColor, flickerChance, breathingSpeed, selectionPrefix
- `AmbientFlicker`: mode (AlphaPulse/RandomFlicker/PositionDrift/ScalePulse), speeds, ranges
- `TypewriterEffect`: charactersPerSecond, glitchOnReveal, speedJitter
- `StaggeredReveal`: staggerDelay, slideFromOffset, itemFadeDuration
- `CanvasGroupFader`: slideOnFadeIn, scaleOnFadeIn, fadeCurve

### Integrating With Your Game

1. **New Game**: `MainMenuController` calls `SceneManager.LoadScene(newGameScene)`. Set the scene name in Inspector.
2. **Pause**: `GameManager` listens for Escape key and toggles `MenuManager.Instance.NavigateTo(MenuState.PauseMenu)`. Works automatically.
3. **Save/Load**: Wire the `GetSavePayload`, `GetLocationName`, `GetPlayTime` callbacks on `SaveMenuController`, and `OnPayloadLoaded` on `LoadMenuController`.
4. **Settings**: `GameSettings.Instance.Current` returns the active `SettingsData`. Subscribe to changes or read values directly.

---

## File Map

```
Assets/UniversalMenuSystem/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs              # Singleton. Scene tracking, Escape for pause, cursor lock.
│   │   ├── MenuManager.cs              # Singleton. Central hub. Fade transitions. Navigation stack.
│   │   ├── MenuState.cs                # Enum: None, MainMenu, PauseMenu, Settings, SaveMenu, LoadMenu, ConfirmQuit, Credits
│   │   ├── MenuAudioController.cs      # Singleton. PlayHover(), PlaySelect(), PlayBack(), ambient loop.
│   │   └── ResolutionManager.cs        # CanvasScaler config (1920x1080, match 0.5) + safe area.
│   ├── Menus/
│   │   ├── MainMenuController.cs       # Right-aligned buttons: New Game, Continue, Load, Settings, Quit.
│   │   ├── PauseMenuController.cs      # In-game overlay: Resume, Save, Load, Settings, Main Menu, Quit.
│   │   ├── SettingsMenuController.cs   # 4 tabs: Display, Audio, Gameplay, Accessibility. Apply/Revert/Defaults.
│   │   ├── SaveMenuController.cs       # Spawns SaveSlotUI for each slot. Overwrite confirmation.
│   │   ├── LoadMenuController.cs       # Spawns SaveSlotUI for each slot. Load confirmation.
│   │   ├── ConfirmQuitController.cs    # Yes/No dialog. Application.Quit().
│   │   └── CreditsController.cs        # Auto-scrolling credits with skip.
│   ├── Settings/
│   │   ├── GameSettings.cs             # Singleton. Loads/saves SettingsData to PlayerPrefs. Applies to Unity.
│   │   └── SettingsData.cs             # [Serializable] struct. Display, Audio, Gameplay, Accessibility fields.
│   ├── SaveLoad/
│   │   ├── SaveLoadManager.cs          # Singleton. ISaveProvider interface. DefaultFileSaveProvider (JSON files).
│   │   ├── SaveSlotData.cs             # Per-slot: slotId, timestamp, playTime, locationName, payload, hasData.
│   │   └── SaveSlotUI.cs               # MonoBehaviour for one slot entry. Data/empty containers, delete button.
│   ├── UI/
│   │   ├── TextHoverEffect.cs          # Hover: slide right, color shift, flicker, breathing, selection prefix.
│   │   ├── CanvasGroupFader.cs         # FadeIn/FadeOut with optional slide and scale.
│   │   ├── TypewriterEffect.cs         # Character-by-character with speed jitter and glitch chars.
│   │   ├── StaggeredReveal.cs          # Sequential child reveal with stagger delay.
│   │   └── AmbientFlicker.cs           # Modes: AlphaPulse, RandomFlicker, PositionDrift, ScalePulse.
│   └── Editor/
│       └── MenuSetupWizard.cs          # Editor window: Tools > Universal Menu System > Setup Wizard.
└── Prefabs/
    └── SaveSlotUI.prefab               # Generated by wizard's "Create Save Slot Prefab" button.
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `MenuManager.Instance` is null | Ensure the Setup Wizard was run, or the `[MenuSystem]` root exists in a loaded scene with `DontDestroyOnLoad` |
| Panels don't show | Check that each panel CanvasGroup is assigned on the MenuManager component in the Inspector |
| No hover effects | Ensure `TextHoverEffect` is attached to menu button GameObjects |
| TMP text shows boxes | Import TMP Essentials via Window → TextMeshPro → Import TMP Essential Resources |
| Save files not appearing | Check `Application.persistentDataPath + "/Saves/"` exists; ensure `SaveLoadManager` is in the scene |
| Setup Wizard missing | The menu item requires scripts in an `Editor/` folder; ensure `Scripts/Editor/MenuSetupWizard.cs` compiled without errors |
| Pause doesn't work in-game | `GameManager` must be in the scene and `IsInMainMenu` must be false (set automatically by scene name) |

---

## Requirements

- **Unity 6000.0+** (Unity 6)
- **TextMeshPro** (included with Unity)
- No other packages, plugins, or dependencies

# Universal Menu System

**Horror / Walking Simulator Style Menu Framework for Unity 6000.0+**

A complete, game-agnostic menu system with animated text-based UI — inspired by Resident Evil-style layouts where atmospheric backgrounds meet minimal, elegant text navigation.

---

## Features

| Feature | Description |
|---|---|
| **Main Menu** | Title + right-aligned text buttons (New Game, Continue, Load, Settings, Quit) |
| **Pause Menu** | In-game overlay (Resume, Save, Load, Settings, Main Menu, Quit) |
| **Settings Menu** | Tabbed: Display, Audio, Gameplay, Accessibility with apply/revert/defaults |
| **Save System** | Slot-based with injectable payloads, file-backed persistence |
| **Load System** | Shows all slots with metadata (timestamp, location, play time) |
| **Credits** | Auto-scrolling text with skip |
| **Confirm Quit** | Yes/No dialog |
| **Resolution Scaling** | `CanvasScaler` + safe area support for all resolutions |
| **Horror UI Effects** | Text hover glow, flicker, breathing, typewriter, staggered reveal |
| **Audio System** | Hover/click/back sounds, ambient menu atmosphere loop |

---

## Quick Start

### 1. Import
Drop the `UniversalMenuSystem` folder into your Unity project's `Assets/` directory.

### 2. Run the Setup Wizard
**Tools → Universal Menu System → Setup Wizard**

This creates the entire UI hierarchy in your scene:
- `[MenuSystem]` root with GameManager, GameSettings, SaveLoadManager
- `MenuCanvas` with all panels pre-wired
- Text buttons with hover effects
- EventSystem (if missing)

### 3. Assign Your Font
Select each `TextMeshProUGUI` component and assign your preferred TMP font asset. Horror recommendations:
- **Bebas Neue** — clean, tall caps
- **Cinzel** — elegant serif
- **Special Elite** — typewriter feel
- **IM Fell English** — old-world serif

### 4. Assign Audio Clips
Select `[MenuSystem]` → `MenuAudioController` and assign:
- `Hover Sound` — subtle click or whisper
- `Select Sound` — deeper click or mechanical latch
- `Back Sound` — soft reverse click
- `Menu Ambience` — low drone, distant wind, eerie hum

### 5. Set Your Scene Names
On `GameManager`:
- `Main Menu Scene Name` — your main menu scene (default: `"MainMenu"`)

On `MainMenuController`:
- `New Game Scene` — your first gameplay scene (default: `"Gameplay"`)

---

## Architecture

```
[MenuSystem]                        (DontDestroyOnLoad root)
├── GameManager                     (scene tracking, pause key, cursor)
├── GameSettings                    (settings persistence via PlayerPrefs)
├── SaveLoadManager                 (slot-based save system)
└── MenuAudioController             (UI sounds + ambient loop)

MenuCanvas
├── ResolutionManager               (CanvasScaler configuration)
├── SafeArea                        (device safe area clipping)
│   ├── BackgroundOverlay           (semi-transparent dark overlay)
│   ├── MainMenuPanel               (title + buttons)
│   ├── PauseMenuPanel              (in-game overlay)
│   ├── SettingsPanel               (tabbed settings)
│   ├── SaveMenuPanel               (slot list for saving)
│   ├── LoadMenuPanel               (slot list for loading)
│   ├── ConfirmQuitPanel            (yes/no)
│   └── CreditsPanel                (auto-scroll)
```

---

## How the Navigation Stack Works

`MenuManager` maintains a **back-stack**. When you navigate from Main Menu → Settings, "Main Menu" is pushed onto the stack. Pressing "Back" pops and transitions back. All transitions are animated fades using `AnimationCurve`.

```csharp
// Navigate forward (pushes current state to stack)
MenuManager.Instance.NavigateTo(MenuState.Settings);

// Go back (pops from stack)
MenuManager.Instance.GoBack();

// Close everything
MenuManager.Instance.CloseAllMenus();
```

---

## Save/Load Integration

The system is **payload-agnostic**. Your game serializes its state into a string; this system handles slot management, timestamps, and UI.

### Saving

```csharp
// On your SaveMenuController (or anywhere):
saveMenu.GetSavePayload = () => JsonUtility.ToJson(myGameState);
saveMenu.GetLocationName = () => SceneManager.GetActiveScene().name;
saveMenu.GetPlayTime = () => myPlayTimer.ElapsedSeconds;
```

### Loading

```csharp
// On your LoadMenuController:
loadMenu.OnPayloadLoaded = (payload, slotData) =>
{
    var state = JsonUtility.FromJson<MyGameState>(payload);
    SceneManager.LoadScene(state.sceneName);
    // Apply your deserialized state...
};
```

### Custom Save Backend

Implement `ISaveProvider` and inject it:

```csharp
SaveLoadManager.Instance.Provider = new MySteamCloudSaveProvider();
```

---

## Settings Data Fields

| Category | Field | Type | Default |
|---|---|---|---|
| Display | `resolutionIndex` | int | -1 (auto) |
| Display | `qualityLevel` | int | -1 (auto) |
| Display | `fullscreen` | bool | true |
| Display | `vsyncCount` | int | 1 |
| Display | `brightness` | float | 1.0 |
| Audio | `masterVolume` | float | 1.0 |
| Audio | `musicVolume` | float | 0.8 |
| Audio | `sfxVolume` | float | 1.0 |
| Audio | `voiceVolume` | float | 1.0 |
| Audio | `ambientVolume` | float | 0.7 |
| Audio | `muteAll` | bool | false |
| Gameplay | `mouseSensitivity` | float | 1.0 |
| Gameplay | `invertYAxis` | bool | false |
| Gameplay | `fieldOfView` | float | 70 |
| Gameplay | `showSubtitles` | bool | true |
| Gameplay | `headBob` | bool | true |
| Gameplay | `gamma` | float | 1.0 |
| Accessibility | `reducedMotion` | bool | false |
| Accessibility | `highContrastUI` | bool | false |
| Accessibility | `screenShake` | bool | true |

---

## UI Effects (Horror Aesthetic)

### TextHoverEffect
Attach to any `TextMeshProUGUI` button. On hover:
- Text slides right with selection indicator (`► `)
- Color shifts from muted parchment to warm glow
- Random alpha flicker (dying lightbulb effect)
- Subtle breathing pulse when idle

### StaggeredReveal
Attach to a parent of menu items. Children fade in one-by-one with a stagger delay and slide animation — classic horror menu entrance.

### TypewriterEffect
Types text character-by-character. Optional speed jitter and glitch characters for unease.

### AmbientFlicker
Modes: `AlphaPulse`, `RandomFlicker`, `PositionDrift`, `ScalePulse`. Adds subtle life to static elements.

### CanvasGroupFader
Component-level fade in/out with optional slide and scale animations.

---

## Resolution & Scaling

`ResolutionManager` auto-configures `CanvasScaler`:
- **Scale Mode**: Scale With Screen Size
- **Reference**: 1920×1080
- **Match**: 0.5 (balanced width/height)
- **Safe Area**: Automatically adjusts for notches and rounded corners

All UI uses anchored `RectTransform` layout — stretches naturally across any aspect ratio.

---

## File Structure

```
Assets/UniversalMenuSystem/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs              # Scene tracking, pause input
│   │   ├── MenuManager.cs             # Central hub, transitions, back-stack
│   │   ├── MenuState.cs               # Enum of all menu states
│   │   ├── MenuAudioController.cs     # UI sounds & ambient loop
│   │   └── ResolutionManager.cs       # Canvas scaling & safe area
│   ├── Menus/
│   │   ├── MainMenuController.cs      # Title screen buttons
│   │   ├── PauseMenuController.cs     # In-game pause overlay
│   │   ├── SettingsMenuController.cs  # Tabbed settings (4 categories)
│   │   ├── SaveMenuController.cs      # Slot-based save UI
│   │   ├── LoadMenuController.cs      # Slot-based load UI
│   │   ├── ConfirmQuitController.cs   # Yes/No quit dialog
│   │   └── CreditsController.cs       # Auto-scrolling credits
│   ├── Settings/
│   │   ├── GameSettings.cs            # Settings persistence (PlayerPrefs)
│   │   └── SettingsData.cs            # Serializable settings struct
│   ├── SaveLoad/
│   │   ├── SaveLoadManager.cs         # Slot manager + ISaveProvider
│   │   ├── SaveSlotData.cs            # Per-slot metadata + payload
│   │   └── SaveSlotUI.cs              # UI entry for one slot
│   ├── UI/
│   │   ├── TextHoverEffect.cs         # Horror-style text hover
│   │   ├── CanvasGroupFader.cs        # Animated fade utility
│   │   ├── TypewriterEffect.cs        # Character-by-character reveal
│   │   ├── StaggeredReveal.cs         # Sequential child reveal
│   │   └── AmbientFlicker.cs          # Ambient UI animation
│   └── Editor/
│       └── MenuSetupWizard.cs         # One-click scene setup
└── Prefabs/
    └── SaveSlotUI.prefab              # (generated by wizard)
```

---

## Requirements

- **Unity 6000.0+** (Unity 6)
- **TextMeshPro** (included with Unity)
- No other dependencies

---

## License

MIT — use freely in commercial and personal projects.

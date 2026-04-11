# *Lost in the Sky Realms*

A **2D tilemap platformer** built in Unity, set across floating **pink-toned sky islands**. Jump between clouds, collect sky treasures, and reach the exit portal before time runs out.

This repo is a **playable template**: movement, combat-adjacent health, UI, timers, and level flow are wired up so you can focus on art, level design, and your own game feel.

---

## What’s inside


| Area             | Details                                                                                                                          |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **World**        | Unity **Tilemap** levels with a sky-realm aesthetic (tiles and props under `Assets/png/`)                                        |
| **Player**       | `**SkyExplorerController*`* — run, jump, **double jump**, grounded checks, animator hooks, optional **footstep and landing VFX** |
| **Input**        | `**SkyTravelMode`** — switch between **mobile** (touch UI) and **PC** controls                                                   |
| **Camera**       | **Cinemachine** + **URP** for a clean 2D look; **Pixel Perfect** friendly setup                                                  |
| **Progression**  | `**SkyRealmGameManager`** — **coins** & **sky crystals**, **countdown timer**, **level complete** and **game over** flows        |
| **Health**       | `**SkyHeartManager`** — heart UI with full / half / empty states, damage feedback                                                |
| **Finish line**  | `**SkyPortalTrigger`** — triggers fade-to-black and **level complete**                                                           |
| **UI**           | `**SkyRealmUIManager`** & `**SkyRealmPauseMenu**` — menus and mobile control visibility                                          |
| **Collectibles** | `**SkyCollectible`** — pickups wired into the score and economy                                                                  |


Character animation and a **Wizard**-style 2D rig live under `Assets/Wizard - 2D Character/` (with demo scenes you can reference).

---

## Requirements

- **[Unity 6](https://unity.com/releases)** — this project targets editor `**6000.3.10f1`** (see `ProjectSettings/ProjectVersion.txt`).
- Open the project folder in Unity Hub and allow the editor to import assets on first load.

Using the matching editor version avoids subtle serialization and package drift when collaborating or opening on another machine.

---

## Running the game

1. Clone or download this repository.
2. Open the project in Unity **6000.3.10f1** (or the same **6000.3.x** line if you accept minor upgrade prompts).
3. From **File → Build Settings**, confirm scenes if you change the flow. Main gameplay scenes live under `Assets/Scenes/` (`**Menu`**, `**Level**`).
4. Press **Play** from the scene you want to test (typically `**Menu`** for a full run, or `**Level**` for iteration).

---

## Project layout (quick map)

```
Assets/
├── Scenes/           # Menu, Level, and gameplay flow
├── Scripts/          # SkyExplorerController, managers, UI, collectibles, portals
├── png/              # Tiles, backgrounds, objects
└── Wizard - 2D Character/   # Player rig, animations, demo scenes
```

Custom game logic you’ll extend most often lives in `**Assets/Scripts/**`.

---

## Tech stack (packages)

Notable packages from `Packages/manifest.json`:

- **Universal RP** — 2D rendering pipeline  
- **2D Tilemap**, **2D Animation**, **Pixel Perfect**, **Sprite Shape**  
- **Cinemachine** — follow / confine cameras  
- **TextMesh Pro** — UI text  
- **Unity UI (uGUI)** — canvases and controls

---

*Float high, collect bright, and don’t miss the last platform.*
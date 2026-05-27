# Cave Bosses: Dragon Trappers

Cave Bosses: Dragon Trappers is a Unity prototype about running a small dragon-trapping company. Buy supplies, enter caves, set traps, catch dragons, sell your haul at the market, and decide when to risk staying deeper underground.

The current build focuses on the first playable management loop: market preparation, cave actions, time pressure, dragon capture, selling, safe rooms, and day-end company reporting.

## Current Gameplay Loop

1. Start at the company hub.
2. Visit the market to buy traps and rabbits.
3. Enter the cave and choose actions:
   - Set a normal trap.
   - Set a rabbit-baited trap.
   - Build a safe room.
   - Travel deeper.
   - End the day.
4. Return to the market and sell captured dragons by species and condition.
5. Review the day-end company report.

## Prototype Features

- Market buying with changing trap and rabbit prices.
- Cave time budget where every action spends time.
- Normal traps and rabbit-baited traps.
- Rabbit bait improves dragon encounter and capture chances.
- Live and dead dragon capture outcomes.
- Species-based dragon selling.
- Safe rooms for surviving the night in a cave.
- Run reset when ending the day underground without a safe room.
- Cave depth and capture progress display.
- Day-end company report with income, expense, net profit, score, catches, sales, and reset status.
- Local third-party placeholder assets credited under `Assets/ThirdParty/CREDITS.md`.

## Unity Version

This project is currently set up with:

```text
Unity 6000.3.16f1
```

Open the repository folder with Unity Hub using this version or a compatible Unity 6 editor.

## Running the Prototype

1. Open the project in Unity.
2. Open or play from the prototype scenes:
   - `Assets/Scenes/MainHub.unity`
   - `Assets/Scenes/Market.unity`
   - `Assets/Scenes/Cave.unity`
3. If scene loading needs setup, use:

```text
DragonTrappers > Add Prototype Scenes to Build Settings
```

4. Press Play.

## Useful Editor Tools

The project includes local prototype tools under the `DragonTrappers` Unity menu:

- `Add Prototype Scenes to Build Settings`
- `Verify UI Buttons (Play Mode)`

Run the verifier while Play Mode is active to check the main UI flow.

## Project Structure

```text
Assets/
  Scenes/                  Prototype scenes
  scripts/                 Core gameplay and UI scripts
    Core/                  Session/bootstrap helpers
    Flow/                  Scene flow
    Services/              UI-to-gameplay action adapter
    UI/                    Runtime UI presenters and factories
    Editor/                Unity editor utilities
  ThirdParty/              Credited third-party placeholder assets
Packages/                  Unity package manifest
ProjectSettings/           Unity project settings
```

## Third-Party Assets

This repository includes placeholder assets from third-party sources. See:

```text
Assets/ThirdParty/CREDITS.md
```

Third-party assets remain under their original licenses. The project license does not override those licenses.

## License

All original project code, design, and game-specific content are proprietary and all rights are reserved by Semih Guler.

No permission is granted to copy, redistribute, sell, sublicense, or reuse this project or its original content without prior written permission.

See `LICENSE.md` for details.

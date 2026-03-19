# 🐉 Cave Bosses: Dragon Trappers

> **A strategy / management / trading game built in Unity (C#)**
> *Currently in active development*

## 📖 About

Cave Bosses: Dragon Trappers is an isometric 2D strategy game where you manage a dragon-catching company. Explore caves, set traps, capture dragons (alive or dead), trade them on the market, and compete against rival firms to dominate the industry.

**Core Gameplay Loop:**
- 🌅 **Morning:** Visit the market — buy traps, bait, equipment
- ☀️ **Daytime:** Enter caves — set traps, explore, hunt dragons
- 🌙 **Night:** No safe room = no survival
- 📊 **Scoring:** Highest score claims the cave as habitat
- 🔄 **New Day:** Trade, upgrade, prepare for the next cave

## 🎮 Key Features (Design)

- **Dynamic Economy** — Vendor prices fluctuate based on supply/demand ratios and daily inflation
- **Trap System** — Multiple trap types with different catch rates, durability, and bait requirements
- **Habitat Management** — Cleared caves become habitats for breeding and storing dragons
- **Rival Firms (AI)** — Compete for cave ownership through scoring
- **Time Pressure** — Every action costs time; plan carefully or get stuck in the dark
- **Safe Room Mechanic** — Build shelters to survive the night, but sacrifice market access

## 🏗️ Architecture

The project follows a **modular, component-based architecture** with loosely coupled systems:

```
📁 Scripts/
├── PrototypeGame.cs      # Main game controller & state machine
├── VendorSystem.cs       # Dynamic pricing, stock management, trade logic
├── TimeSystem.cs         # Day/night cycle, time spending with callbacks
├── InventorySystem.cs    # Player inventory, dragon tracking (alive/dead by type)
├── DragonType.cs         # Dragon data class (rarity, catch modifier, prices)
└── SellOption.cs         # Sell screen data model (enum + dragon name)
```

### Technical Highlights

- **Dynamic Pricing Engine:** `VendorSystem` calculates buy/sell prices using stock-to-target ratios with `Mathf.Lerp` interpolation and cumulative daily inflation (1%/day)
- **Callback Pattern:** `TimeSystem.SpendTime()` uses `Action<string>` and `Action` delegates for logging and time-over events — keeping systems decoupled
- **Serializable Components:** All systems are `[System.Serializable]` for Unity Inspector editing while maintaining clean separation of concerns
- **Type-Safe Inventory:** `InventorySystem` uses `Dictionary<string, int>` for per-species dragon tracking with initialization from `DragonType[]` arrays

## 🛠️ Tech Stack

| Technology | Usage |
|-----------|-------|
| **Unity** | Game engine |
| **C#** | Core programming language |
| **Unity Inspector** | Runtime debugging & tuning via Serializable fields |

## 📋 Development Status

- [x] Game Design Document (GDD)
- [x] Vendor system with dynamic pricing
- [x] Time system with day/night cycle
- [x] Inventory system with dragon tracking
- [x] Dragon type definitions and data model
- [x] Sell option system
- [ ] Cave generation & exploration
- [ ] Trap placement mechanics
- [ ] AI rival firms
- [ ] Scoring & habitat system
- [ ] Art assets & visual design
- [ ] UI/UX implementation
- [ ] Sound design

## 🎯 Design Philosophy

This game is designed from the ground up with a focus on **emergent strategy through interlocking systems**. Every decision has trade-offs:

- Buy more traps → less gold for bait → lower catch rates
- Build a safe room → survive the night → miss the morning market
- Catch dragons alive → higher score → but they need habitat space
- Sell early → quick profit → lose scoring potential

## 👤 Author

**Semih Güler** — Game Developer & Software Engineer
- 📧 smh.guler@gmail.com
- 🎓 42 Istanbul | Kocaeli University (Computer Engineering)
- 🎮 Passionate about game mechanics design and systems programming

## 📄 License

This project is proprietary. All rights reserved.

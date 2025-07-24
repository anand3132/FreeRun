# RedGaint.Games.ParkourSystem

This folder contains the new Parkour System for the FreeRun project, re-implemented using a Finite State Machine (FSM) architecture. The logic is inspired by the Fantacode Studios Parkour & Climbing System, but is being rewritten for maintainability, extensibility, and integration with the rest of the RedGaint game framework.

## Goals
- FSM-based parkour and climbing logic
- Modular, testable, and extensible code
- Clean separation of runtime, editor, and resources
- All code under the `RedGaint.Games.ParkourSystem` namespace

## Migration Plan
1. Implement FSM core for parkour states
2. Gradually port and adapt logic from Fantacode plugin
3. Copy and adapt required animations and resources
4. Integrate with existing character and input systems
5. Test and iterate step by step

---

**Do not place Fantacode code directly here. Only adapted, rewritten, or referenced logic.** 
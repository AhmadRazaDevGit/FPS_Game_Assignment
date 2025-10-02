# FPS Game Assignment - Code Overview

This document provides a quick, high-signal overview of the codebase to help readers, reviewers, and contributors understand the responsibilities of each folder and script. It focuses on runtime code first and then editor/utility code.

## Table of Contents
- Runtime Systems
  - Player
  - Camera
  - Weapons
  - Enemy AI
  - UI & Scenes
- ScriptableObjects
- Editor Tools
- Project Structure

---

## Runtime Systems

### Player (`Assets/FPS_Game_Assignment/Player/Scripts`)
- `PlayerMovement.cs`: First-person movement using `CharacterController`. Reads input via `IInputProvider`, handles ground checks, jumping, gravity, crouch (including controller height/center changes) and smooth camera height. Configured by `MovementConfig` SO.
- `Interfaces/IInputProvider.cs`: Abstraction for reading movement and action input.
- `Inputs/Mobile/MobileInputProvider.cs`: Mobile implementation of `IInputProvider` (virtual joystick & screen buttons).
- `Inputs/Mobile/VirtualJoystick.cs`: UI joystick logic for mobile movement input.
- `Inputs/PC(Optional)/KeyboardMouseInput.cs`: Optional desktop input implementation for testing in editor/PC.

Health System (`Player/Scripts/Health System`):
- `HealthBase.cs`: Base health logic (receive damage, death events, etc.).
- `PlayerHealth.cs`: Player-specific health, extends/uses `HealthBase` and raises game events on death.
- `DamageOverlayUI.cs`: Displays a damage overlay effect on the HUD when the player is hurt.
- `HealthUI.cs`: Updates health bar UI to reflect current health.

ScriptableObjects:
- `ScriptableObjects/MovementConfig.cs`: Tunables for player movement (speed, gravity, crouch, layers, etc.).

### Camera (`Assets/FPS_Game_Assignment/Camera/Scripts`)
- `RotationController.cs`: Applies look input to rotate the camera/player. Reads from `ILookInputProvide` and uses `LookConfig` SO.
- `Interfaces/ILookInputProvide.cs`: Interface for look input providers.
- `Inputs/Mobile/MobileSwipeLook.cs`: Mobile touch/swipe look provider.
- `Inputs/PC(optional)/KeyboardMouseLook.cs`: Optional desktop mouse look provider for testing.
- `ScriptableObjects/LookConfig.cs`: Tunables for look sensitivity, smoothing, and limits.

### Weapons (`Assets/FPS_Game_Assignment/Weapons/Scripts`)
- `WeaponManager.cs`: Selects active weapon, routes fire/reload commands, connects to UI.
- `WeaponBase.cs`: Base class for weapons (common firing pipeline, cooldowns, ammo handling hooks).
- `Guns/Pistol.cs`, `Guns/Rifle.cs`: Concrete weapon implementations extending `WeaponBase`.
- `Projectile.cs`: Lightweight projectile moved manually each frame with raycast-based hit detection; returns to `ObjectPool` when expired or on hit.
- `Pool/ObjectPool.cs`: Simple object pooling to reduce allocations (used by projectiles and effects).
- `Interfaces/IDamageable.cs`: Contract for applying damage to targets.
- `UiDisplay/AmmoDisplayUI.cs`: Updates ammo count UI based on current weapon state.
- `Buttons/FireButton.cs`, `Buttons/ReloadButton.cs`: Mobile UI buttons wired to weapon actions.

ScriptableObjects:
- `ScriptableObjects/WeaponData.cs`: Weapon parameters (damage, fire rate, projectile settings, etc.).
- `ScriptableObjects/Weapons.cs`: Collection of weapon entries (e.g., available weapons for a level).

### Enemy AI (`Assets/FPS_Game_Assignment/Enemy/Scripts`)
Core AI:
- `BaseEnemy.cs`: Core enemy context. Holds references to `Animator`, `NavMeshAgent`, `EnemySensor`, `EnemyHealth`, and waypoints. Initializes and drives the `StateMachine` each frame. Waypoints can be injected via `SetWayPointProvider(WayPoint)` or serialized.
- `StateMachine.cs`: Minimal finite state machine. Manages `CurrentState`, `PreviousState` and calls `Enter/Exit/Tick`.
- `Interfaces/IState.cs`: Interface for AI states (`Enter`, `Tick`, `Exit`).
- `Interfaces/IEnemyContext.cs`: Interface exposing context operations/state to AI states.

States (`Enemy/Scripts/States`):
- `IdleState.cs`: Waits/idles; transitions to patrol.
- `PatrolState.cs`: Moves between waypoints.
- `ChaseState.cs`: Pursues detected targets.
- `AttackState.cs`: Triggers attack behavior/animation and damage hooks.
- `HitState.cs`: Reaction to being hit; temporary state.
- `DieState.cs`: Handles death; disables agent and schedules cleanup.

Sensing, Health, Spawning:
- `EnemySensor.cs`: Trigger-based sensor that raises `OnDetected(Transform)` for valid layers. Configured by `EnemyData` (radius and mask).
- `HealthSystem/EnemyHealth.cs`: Enemy health handling and death events.
- `EnemySpawner.cs`: Spawns enemies from `Enimies` SO, supports delayed spawns. Injects `WayPoint` provider into spawned `BaseEnemy` instances.
- `WayPoint.cs`: Holds a `Transform[] wayPoints`. Context menu populates from children for convenient autho-ring.
- `Zombie.cs`: Example concrete enemy implementation (uses `BaseEnemy` plus specific setup/animation hooks).

ScriptableObjects:
- `ScriptableObjects/EnemyData.cs`: Tunables for an enemy (speeds, animations, detection radius, etc.).
- `ScriptableObjects/Enimies.cs`: Collection of enemy prefabs to spawn.

### UI & Scenes (`Assets/FPS_Game_Assignment/Scripts`)
- `UIManager.cs`: Subscribes to a `GameEventWithBool` (player died) and shows the fail panel.
- `SceneRestarter.cs`: Utility to reload the gameplay scene on command/event.

ScriptableObjects:
- `ScriptableObjects/GameEventWithBool.cs`: Simple event channel (bool payload) for decoupled communication (e.g., player death).

---

## ScriptableObjects (Summary)
- Player: `MovementConfig`
- Camera: `LookConfig`
- Weapons: `WeaponData`, `Weapons`
- Enemies: `EnemyData`, `Enimies`
- Events: `GameEventWithBool`

These SOs centralize tunables and data assets for designers and facilitate decoupling and runtime swapping.

---

## Editor Tools (`Assets/FPS_Game_Assignment/*/Editor`)
- `Weapons/Scripts/Editor/IntegrateWeaponWindow.cs`: Editor utility to integrate a weapon setup into a scene/prefab (adds required components, wires object pools, etc.).
- `Enemy/Scripts/Editor/IntegrateEnemyWindow.cs`: Editor utility to integrate/enforce required enemy setup.

These are not included in runtime builds.

---

## Project Structure (High level)
- `ART/`: Visual assets (UI, characters, weapons, third-party content)
- `Camera/`: Camera control and look input
- `Enemy/`: AI logic, states, health, sensors, and data
- `Player/`: Movement, input, player health and HUD
- `Weapons/`: Weapons, projectiles, pooling, and UI controls
- `Prefabs/` and `Scenes/`: Game prefabs and scene assets
- `Scripts/`: Cross-cutting utilities (UI manager, events, scene restart)

---

## Notes for Contributors
- Follow existing patterns: SO-driven config, interface-based inputs, event channels for decoupling.
- Avoid allocations in hot paths (Updates). Prefer pooling for frequently spawned objects.
- Mobile: keep physics layers tight and UI updates batched.


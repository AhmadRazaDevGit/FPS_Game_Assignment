# FPS Game Assignment - Code Overview

This document provides a comprehensive overview of the FPS game codebase, detailing the responsibilities of each system and component. The project is a mobile-first-person shooter with AI enemies, weapon systems, and player movement controls optimized for both mobile and desktop platforms.

## Table of Contents
- [Game Overview](#game-overview)
- [Runtime Systems](#runtime-systems)
  - [Player Systems](#player-systems)
  - [Camera Systems](#camera-systems) 
  - [Weapons Systems](#weapons-systems)
  - [Enemy AI Systems](#enemy-ai-systems)
  - [UI & Game Management](#ui--game-management)
- [ScriptableObjects](#scriptableobjects)
- [Editor Tools](#editor-tools)
- [Project Structure](#project-structure)
- [Asset Credits](#asset-credits)

## Game Overview

- **Player Movement**: First-person movement with crouch, jump, and mobile/desktop input support
- **Weapons System**: Projectile-based weapons (Pistol, Rifle) with pooling and customizable stats
- **Enemy AI**: State machine-based zombie enemies with patrol, chase, attack behaviors
- **Mobile Controls**: Virtual joystick, touch look controls, and mobile UI buttons
- **Health System**: Player and enemy health with UI feedback and damage effects

---

## Runtime Systems

## Player Systems (`Assets/FPS_Game_Assignment/Player/`)

### Movement System (`Player/Scripts/`)
- **`PlayerMovement.cs`**: Core first-person movement controller using Unity's `CharacterController`. Features include:
  - Multi-platform input support via `IInputProvider` interface
  - Physics-based movement with ground detection, jumping, and gravity
  - Smooth crouching with character controller height/center adjustment
  - Camera position synchronization during crouch transitions
  - Configurable via `MovementConfig` ScriptableObject

### Input Systems (`Player/Scripts/Inputs/`)
- **`Interfaces/IInputProvider.cs`**: Platform-agnostic input interface defining movement, jump, and crouch inputs
- **`Mobile/MobileInputProvider.cs`**: Mobile touch input implementation supporting virtual joystick and UI buttons
- **`Mobile/VirtualJoystick.cs`**: Custom virtual joystick UI component with pointer event handling and normalized output (-1 to 1)
- **`PC(Optional)/KeyboardMouseInput.cs`**: Desktop keyboard/mouse input for editor testing and PC builds

### Health System (`Player/Scripts/Health System/`)
- **`HealthBase.cs`**: Abstract base class providing core health functionality (damage, death events, max health)
- **`PlayerHealth.cs`**: Player-specific health implementation that triggers game events on death
- **`DamageOverlayUI.cs`**: Visual feedback system showing damage overlay effects on the HUD
- **`HealthUI.cs`**: Health bar UI controller with smooth fill transitions and health text display

### Configuration
- **`ScriptableObjects/MovementConfig.cs`**: Designer-friendly movement parameters including speeds, physics settings, crouch behavior, and layer masks

## Camera Systems (`Assets/FPS_Game_Assignment/Camera/`)

### Look Control (`Camera/Scripts/`)
- **`RotationController.cs`**: First-person camera rotation controller that applies look input to camera and player transforms
  - Reads input via `ILookInputProvider` interface for platform flexibility
  - Supports sensitivity scaling, smoothing, and vertical look limits
  - Configured through `LookConfig` ScriptableObject

### Input Providers (`Camera/Scripts/Inputs/`)
- **`Interfaces/ILookInputProvider.cs`**: Platform-agnostic interface for camera look input
- **`Mobile/MobileSwipeLook.cs`**: Touch-based look input for mobile devices with swipe gesture recognition
- **`PC(Optional)/KeyboardMouseLook.cs`**: Mouse look input for desktop testing and PC builds

### Configuration
- **`ScriptableObjects/LookConfig.cs`**: Camera control parameters including sensitivity, smoothing factors, and vertical rotation limits

## Weapons Systems (`Assets/FPS_Game_Assignment/Weapons/`)

### Core Weapon Logic (`Weapons/Scripts/`)
- **`WeaponManager.cs`**: Central weapon controller that manages weapon switching and instantiation
  - Instantiates all weapon prefabs from `Weapons` ScriptableObject at start
  - Handles weapon equipping/unequipping and position management
  - Routes fire/reload commands to active weapon
  - Broadcasts weapon change events to UI systems

- **`WeaponBase.cs`**: Abstract base class for all weapons providing:
  - Standardized firing pipeline with semi-auto and full-auto support
  - Ammo management (current magazine, spare ammo) with events
  - Reload system with progress callbacks and state management
  - Audio integration for fire and empty sounds
  - Object pooling integration for projectiles
  - Spread calculation for accuracy simulation

### Weapon Implementations (`Weapons/Scripts/Guns/`)
- **`Pistol.cs`**: Semi-automatic pistol implementation extending `WeaponBase`
- **`Rifle.cs`**: Automatic rifle implementation with sustained fire capability

### Projectile System (`Weapons/Scripts/`)
- **`Projectile.cs`**: Projectile behavior with:
  - Physics-based movement with configurable speed and lifetime
  - Raycast-based hit detection for precise collision
  - Automatic return to object pool on hit or expiration
  - Damage application to `IDamageable` targets
  - Layer mask filtering for hit targets

### Object Pooling (`Weapons/Scripts/Pool/`)
- **`ObjectPool.cs`**: Memory-efficient pooling system for projectiles and effects to reduce garbage collection

### UI Integration (`Weapons/Scripts/`)
- **`UiDisplay/AmmoDisplayUI.cs`**: Real-time ammo counter with format customization
- **`Buttons/FireButton.cs`**: Mobile fire button with pointer event handling
- **`Buttons/ReloadButton.cs`**: Mobile reload button integration

### Interfaces (`Weapons/Scripts/Interfaces/`)
- **`IDamageable.cs`**: Contract for entities that can receive damage from projectiles

### Configuration
- **`ScriptableObjects/WeaponData.cs`**: Per-weapon configuration including:
  - Identity (name, description)
  - Ammo settings (magazine size, spare ammo, reload time)
  - Fire behavior (semi/full-auto, fire rate, spread)
  - Projectile properties (speed, lifetime, damage, prefab)
  - Audio clips and visual effects
- **`ScriptableObjects/Weapons.cs`**: Collection of available weapon prefabs for level/game configuration

## Enemy AI Systems (`Assets/FPS_Game_Assignment/Enemy/`)

### Core AI Framework (`Enemy/Scripts/`)
- **`BaseEnemy.cs`**: Central enemy controller implementing `IEnemyContext` interface
  - Manages references to `Animator`, `NavMeshAgent`, `EnemySensor`, and `EnemyHealth`
  - Initializes and drives the state machine each frame
  - Supports waypoint injection for patrol routes
  - Handles target detection events and state transitions
  - Configurable via `EnemyData` ScriptableObject

- **`StateMachine.cs`**: Lightweight finite state machine implementation
  - Manages current and previous states with transition tracking
  - Provides `Enter/Exit/Tick` lifecycle for states
  - Thread-safe state changes with validation

### State System (`Enemy/Scripts/States/`)
- **`IdleState.cs`**: Waiting state with configurable delay before transitioning to patrol
- **`PatrolState.cs`**: Navigation between waypoints with random target selection and arrival detection
- **`ChaseState.cs`**: Target pursuit using NavMeshAgent with distance-based transitions to attack/patrol
- **`AttackState.cs`**: Combat state with damage application, attack cooldowns, and animation triggers
- **`HitState.cs`**: Stagger reaction state providing brief invulnerability and recovery time
- **`DieState.cs`**: Death state handling agent disabling, cleanup scheduling, and destruction

### Interfaces (`Enemy/Scripts/Interfaces/`)
- **`IState.cs`**: State contract defining `Enter`, `Tick`, and `Exit` methods
- **`IEnemyContext.cs`**: Context interface exposing enemy components and operations to states
- **`IWayPointAssignable.cs`**: Interface for dynamic waypoint assignment

### Sensing & Detection (`Enemy/Scripts/`)
- **`EnemySensor.cs`**: Trigger-based detection system
  - Configurable detection radius and layer filtering
  - Raises `OnDetected` events for valid targets
  - Integrates with `EnemyData` configuration

### Health System (`Enemy/Scripts/HealthSystem/`)
- **`EnemyHealth.cs`**: Enemy-specific health implementation extending `HealthBase`
  - Triggers hit reactions on damage received
  - Raises death events for state machine integration
  - UI-compatible health display support

### Spawning & Management (`Enemy/Scripts/`)
- **`EnemySpawner.cs`**: Enemy instantiation system
  - Spawns enemies from `Enemies` ScriptableObject collection
  - Supports delayed spawning for pacing control
  - Automatically injects waypoint providers into spawned enemies
- **`WayPoint.cs`**: Waypoint collection component with editor utilities for easy setup
- **`EnemyFactory.cs`**: Factory pattern implementation for enemy creation

### Concrete Implementations (`Enemy/Scripts/`)
- **`Zombie.cs`**: Basic zombie enemy extending `BaseEnemy` with specific behaviors and animations

### Configuration
- **`ScriptableObjects/EnemyData.cs`**: Comprehensive enemy configuration including:
  - Movement parameters (patrol speed, chase speed, stopping distances)
  - Detection settings (radius, layer masks)
  - Combat properties (attack damage, cooldowns, recovery times)
  - Death and cleanup timings
- **`ScriptableObjects/Enemies.cs`**: Collection of enemy prefabs available for spawning

## UI & Game Management (`Assets/FPS_Game_Assignment/Scripts/`)

### Core UI Systems
- **`UIManager.cs`**: Central UI controller managing game state displays
  - Subscribes to player death events via `GameEventWithBool`
  - Controls fail panel visibility based on game state
  - Provides centralized UI event handling

- **`SceneRestarter.cs`**: Scene management utility for game flow control
  - Handles gameplay scene reloading on player death or restart requests
  - Provides clean restart functionality

### Event System
- **`ScriptableObjects/GameEventWithBool.cs`**: Decoupled event communication system
  - Enables loose coupling between game systems
  - Used for critical game state changes (player death, level completion)
  - Publisher-subscriber pattern implementation

---

## ScriptableObjects Configuration System

The project uses ScriptableObjects extensively for data-driven design and runtime configuration:

### System Configuration Assets
- **Player**: `MovementConfig` - Movement physics, speeds, jump/crouch parameters, layer masks
- **Camera**: `LookConfig` - Look sensitivity, smoothing, rotation limits
- **Weapons**: 
  - `WeaponData` - Individual weapon stats (damage, fire rate, ammo, projectile properties)
  - `Weapons` - Collection of available weapon prefabs for game/level configuration
- **Enemies**:
  - `EnemyData` - AI behavior parameters (speeds, detection radius, attack damage, timings)
  - `Enemies` - Collection of enemy prefabs for spawning systems
- **Events**: `GameEventWithBool` - Decoupled event communication for game state changes

### Benefits
- **Designer-Friendly**: Non-programmers can adjust game balance and behavior
- **Runtime Flexibility**: Configuration changes without code recompilation
- **Decoupling**: Systems reference data assets rather than hardcoded values
- **Asset Management**: Centralized configuration with Unity's asset system

---

## Editor Tools & Utilities

### Weapon Integration (`Weapons/Scripts/Editor/`)
- **`IntegrateWeaponWindow.cs`**: Custom editor window for weapon setup automation
  - Automatically adds required components to weapon prefabs
  - Configures object pools for projectiles
  - Wires weapon data connections
  - Validates weapon setup for runtime compatibility

### Enemy Integration (`Enemy/Scripts/Editor/`)
- **`IntegrateEnemyWindow.cs`**: Enemy setup validation and integration tool
  - Ensures required components are present on enemy prefabs
  - Validates state machine configuration
  - Checks NavMeshAgent and Animator setup
  - Provides automated enemy prefab preparation

### Utility Features
- **Context Menu Helpers**: Various scripts include context menu items for designer workflow improvement
- **Validation Systems**: Editor-time validation prevents common setup errors
- **Automated Setup**: Reduces manual configuration work for complex prefabs


---

## Project Structure

### Core Systems
- **`Player/`**: Player movement, input systems, health, and related UI components
- **`Camera/`**: First-person camera control with platform-specific input providers
- **`Weapons/`**: Weapon systems, projectiles, object pooling, and weapon UI
- **`Enemy/`**: AI state machines, enemy behaviors, health, and spawning systems
- **`Scripts/`**: Cross-cutting utilities (UI management, scene control, event systems)

### Assets & Content
- **`ART/`**: Visual assets organized by category
  - `UI/`: User interface elements (buttons, health bars, crosshairs, damage overlays)
  - `Unity Asset Store/`: Third-party assets (environments, characters, weapons, zombies)
- **`Prefabs/`**: Reusable game objects
  - `UI/`: UI canvas prefabs for game interface
- **`Scenes/`**: Game scenes with lighting and navigation data
  - `Gameplay.unity`: Main game scene with NavMesh baking
- **`SO/` (various)**: ScriptableObject asset instances for runtime configuration

### Audio Assets
- **`Weapons/Sounds/`**: Weapon sound effects organized by weapon type
  - `Pistol/`: Pistol fire and reload sounds
  - `Rifle/`: Rifle fire and reload sounds

### Material Assets
- **`Weapons/Materials/`**: Projectile materials (PistolBullet, RifleBullet)

---

## Asset Credits

This project uses several high-quality assets from the Unity Asset Store:

### Environment Assets
- **Environment Pack**: Low-poly modular environment pieces including boxes, walls, ramps, stairs, and structures for level building

### Character Assets
- **Free Low Poly Robot**: Modular robot parts with materials and animations
- **Zombie Character Pack**: Low-poly zombie models with textures and animations for enemy implementation

### Weapon Assets
- **Low Poly Weapons VOL.1**: Comprehensive collection of low-poly weapons including:
  - Assault rifles (AK74, M4)
  - Pistols (M1911)
  - Sniper rifles (M107)
  - Machine guns (M249, M2 .50cal)
  - Shotguns (Benelli M4)
  - SMGs (Uzi)
  - RPGs and grenades
  - Various scopes and attachments

All Unity Asset Store assets are located in `ART/Unity Asset Store/` and are used under their respective licenses.

---

## Development Guidelines

### Code Patterns
- **ScriptableObject-Driven Configuration**: Use SOs for all tunable parameters and data assets
- **Interface-Based Design**: Implement platform abstractions (IInputProvider, ILookInputProvider) for multi-platform support
- **Event-Driven Architecture**: Use event channels (GameEventWithBool) for decoupled system communication
- **Object Pooling**: Pool frequently instantiated objects (projectiles, effects) to reduce garbage collection

### Performance Considerations
- **Mobile Optimization**: Keep physics layers optimized and batch UI updates
- **Hot Path Optimization**: Avoid allocations in Update() methods
- **Efficient NavMesh Usage**: Use NavMeshAgent stopping distances and layer masks effectively

### Platform Support
- **Primary Target**: Mobile devices with touch controls
- **Secondary Support**: Desktop for testing and development with keyboard/mouse input


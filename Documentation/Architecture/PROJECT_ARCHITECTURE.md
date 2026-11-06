# MECH-XR Project Architecture

MECH-XR is built using a decoupled, modular software architecture in Unity 6 C#. It strictly separates procedural mesh construction, kinematic calculations, input interactions, AR surface tracking, and user interface management.

```
Assets/
├── Art/
│   ├── Materials/               # Shaders and engineering materials
│   └── Prefabs/                 # Mechanism prefabs and UI templates
├── Editor/
│   ├── AndroidBuildTool.cs      # Build pipeline, platform switching, PlayerSettings & APK output
│   └── SceneSetupTool.cs        # Automated MainScene generator & automated verification tests
├── Scenes/
│   └── MainScene.unity          # Primary AR production scene (Build Index 0)
└── Scripts/
    ├── AR/
    │   ├── ARPlacementManager.cs    # ARCore surface raycasting, reticle lifecycle, model anchoring
    │   └── PlacementReticle.cs      # Procedural cyan target ring with plane alignment
    ├── Interaction/
    │   ├── InspectableComponent.cs  # Component metadata, highlight feedback, tap raycast target
    │   └── ModelInteractionController.cs # One-finger rotation (360°), pinch scale, click detection
    ├── Mechanisms/
    │   ├── MechanismBase.cs         # Abstract kinematics base class (RPM, ratio, pause, reverse)
    │   ├── SpurGearPair.cs          # Parallel shaft transmission (20T driver, 40T driven, 2:1 ratio)
    │   ├── BevelGearPair.cs         # 90° intersecting shaft bevel gears (16T pinion, 32T crown)
    │   └── WormGearPair.cs          # Skew non-intersecting worm drive (1 start, 30T wheel, 30:1 ratio)
    ├── Procedural/
    │   └── GearMeshBuilder.cs       # Procedural 3D mesh generator (spur, bevel, worm helix, shafts, bed)
    └── UI/
        └── UIManager.cs             # Dark charcoal / electric blue HUD, menu cards, telemetry dashboards
```

---

## 🏗️ Layered Subsystem Responsibilities

### 1. Procedural Geometry Layer (`MechXR.Procedural`)
- **`GearMeshBuilder`**:
  - Pure static procedural geometric generator.
  - Constructs manifold 3D meshes at runtime with zero external file or asset dependencies.
  - Generates:
    - Involute/trapezoidal spur gears with parametric tooth counts, addendum, dedendum, bore holes, and rim thickness.
    - Conical bevel gears with slant taper converging toward the pitch cone apex.
    - Single/multi-start helical worms with continuous 3D trapezoidal threads and capped core cylinders.
    - Precision steel drive shafts, collar bearings, and laboratory bedplates.

### 2. Kinematics & Mechanics Layer (`MechXR.Mechanisms`)
- **`MechanismBase`**:
  - Abstract foundation defining the mechanical contract.
  - Exposes properties: `InputRPM`, `OutputRPM`, `GearRatio`, `IsPaused`, `IsReversed`.
  - Integrates time and angular velocity deg/sec ($\omega = n \cdot 360 / 60$) frame-by-frame.
  - Fires `OnKinematicsUpdated` events whenever speeds or drive modes change.
- **`SpurGearPair`**:
  - Simulates external conjugate meshing across parallel shafts.
  - Enforces opposite rotational direction ($\omega_2 = -\omega_1 / i$).
  - Offsets driven gear phase by $\pi / Z_2$ to ensure interlocking teeth without intersection.
- **`BevelGearPair`**:
  - Translates rotation across a 90° intersecting angle.
  - Calculates pitch cone angles $\delta_1 = \arctan(Z_1 / Z_2)$ and $\delta_2 = \arctan(Z_2 / Z_1)$.
- **`WormGearPair`**:
  - Coordinates non-intersecting perpendicular shafts.
  - Translates 1 full rotation of the worm into an advance of $Z_1$ teeth on the worm wheel ($i = 30:1$).

### 3. AR Surface & Tracking Layer (`MechXR.AR`)
- **`ARPlacementManager`**:
  - Interfaces with Unity AR Foundation's `ARRaycastManager` and `ARPlaneManager`.
  - Tracks horizontal surfaces (`PlaneWithinPolygon` and `PlaneEstimated`).
  - Projects the `PlacementReticle` onto the detected desk surface.
  - Manages placement state: `SearchingForSurfaces` ➔ `SurfaceDetected` ➔ `ObjectPlaced`.
  - Supports model repositioning, removal, and live mechanism swapping.
  - Includes a desktop fallback raycasting against an artificial tabletop ($Y = -0.3\text{ m}$) for Editor testing.
- **`PlacementReticle`**:
  - Generates a procedural flat circular reticle with electric-blue emission that locks to the tracked plane normal.

### 4. Interactive Inspection Layer (`MechXR.Interaction`)
- **`InspectableComponent`**:
  - Attached to each mechanical piece (pinion, crown gear, worm screw, wheel, drive shaft, baseplate).
  - Contains descriptive engineering metadata (`ComponentName`, `RoleDescription`, `TechnicalSpecs`).
  - Swaps material tints to electric blue when selected and resets upon deselection.
- **`ModelInteractionController`**:
  - Handles single-finger swipe gestures to smoothly rotate the assembly around its vertical axis for 360° inspection.
  - Handles two-finger pinch gestures to scale the tabletop model within safe ergonomic bounds ($0.4\times$ to $2.5\times$).
  - Performs screen-space raycasting from camera to select inspectable components while filtering out UI touches (`EventSystem.current.IsPointerOverGameObject()`).

### 5. Presentation & UI Layer (`MechXR.UI`)
- **`UIManager`**:
  - Follows modern laboratory styling (dark charcoal `#12171F` background, white/slate text `#9EADC0`, and electric blue `#00E0FF` accents).
  - Dynamically builds and controls:
    - **Menu Screen**: High-contrast cards for Spur, Bevel, and Worm mechanisms.
    - **AR HUD**: Header with mechanism title, Return-to-Menu button, and Replace button.
    - **Control Dock**: Play/Pause toggle, Reverse toggle, Reset button, live Input RPM slider (10–300 RPM), and Scale slider.
    - **Component Inspection Card**: Modal card displaying clicked component's role and engineering specs.
    - **Calculations & Telemetry Panel**: Live math breakdown showing velocity ratios, output speed, and torque multipliers.

---

## 🔄 Interaction & Data Flow Diagram

```
[ User Touch / Mouse ]
         │
         ├───► [ UI Element? ] ──Yes──► [ UIManager (Sliders, Buttons, Menu) ]
         │                                       │
         └───No                                  ▼
                 │                      [ Updates Mechanism Properties ]
                 ▼                      (InputRPM, Pause, Reverse, Reset)
         [ ModelInteractionController ]          │
                 │                               ▼
                 ├─── Single Finger Drag ──► Rotate Assembly (Yaw)
                 ├─── Pinch ───────────────► Scale Assembly (0.4x - 2.5x)
                 └─── Tap Component ───────► Raycast InspectableComponent
                                                 │
                                                 ▼
                                     [ Triggers Inspection Card in UI ]
```


# MECH-XR — COMPLETE DEVELOPMENT & PHASE HISTORY

This document records the chronological development trajectory of the MECH-XR project from inception to the final Phase 6.1 release.

---

## Phase 1 & 2: Architectural Foundation & AR Tracking
- **Objective:** Establish the AR foundation on Unity 6.3 LTS for Android.
- **Milestones:**
  - Configured AR Foundation 6.0.8 with Google ARCore XR Plugin 6.0.8.
  - Implemented `ARPlacementManager.cs` for planar surface detection, visual placement reticle, and 6-DoF touch placement.
  - Set up Android ARM64 compilation pipeline targeting Android 16 (API 36).

---

## Phase 3: Core Mechanical Catalog & Procedural CAD Pipeline
- **Objective:** Eliminate external binary model bloat by building pure procedural mesh generators in C#.
- **Milestones:**
  - Implemented `GearMeshBuilder.cs` generating mathematically exact involute teeth, root fillets, pitch circles, and bore keyways.
  - Built initial 6 CAD mechanisms: Spur Gear Pair, Bevel Gear Pair, Worm Gear Drive, Planetary Epicyclic Gearbox, Rack and Pinion, and Belt & Pulley Drive.
  - Verified kinematic velocity ratios and interactive component inspection.
  - Report: [`MECH_XR_PHASE3_REPORT.md`](MECH_XR_PHASE3_REPORT.md).

---

## Phase 4: UI Finite State Machine & Exploded Views
- **Objective:** Refactor mobile UI into an exclusive 5-state lifecycle and implement component isolation.
- **Milestones:**
  - Built `UIManager.cs` finite state machine: `Library`, `ARScanning`, `ARPlaced`, `ComponentInspection`, `ExplodedView`.
  - Implemented axial detachment and 360° part inspection for individual gears, bearings, and shafts.
  - Created simultaneous multi-component exploded views with reversible assembly animations.
  - Automated tests: 20 baseline kinematic tests (`TEST-01` to `TEST-20`) passing.
  - Reports: [`MECH_XR_PHASE4_REPORT.md`](MECH_XR_PHASE4_REPORT.md), [`MECH_XR_PHASE4_1_REPORT.md`](MECH_XR_PHASE4_1_REPORT.md).

---

## Phase 5: Scientific Experiment Lab & Gear Train Expansion
- **Objective:** Transform MECH-XR from an interactive viewer into a rigorous analytical laboratory.
- **Milestones:**
  - Added the collapsible Scientific Experiment Lab drawer.
  - Implemented real-time SI calculation engine: $\omega = \frac{2\pi N}{60}$, $P = T \cdot \omega$, $T_{\text{out}} = \eta \cdot T_{\text{in}} \cdot i$, $P_{\text{loss}} = (1-\eta)P_{\text{in}}$.
  - Built 4 real-time graph drawers: RPM, Torque, Speed/Time, and Power/Loss (Sky Blue, Emerald Green, Crimson Red).
  - Implemented 4 standard engineering presets (High Speed, High Torque, Industrial, Low Efficiency) and A/B live snapshot comparison.
  - Expanded catalog from 6 to 14 mechanisms with advanced gear trains: Idler Gear Train, Compound Gear Train, Reverted Gear Train, Internal Gear Pair, Helical Gear Pair, Herringbone Gear Pair, Automotive Differential (Willis law), and Harmonic Strain Wave Drive.
  - Automated tests: Expanded to 47 passing tests (`TEST-01` to `TEST-P5-16`).
  - Report: [`MECH_XR_PHASE5_REPORT.md`](MECH_XR_PHASE5_REPORT.md).

---

## Phase 6: Mechanism Library Expansion to 26 Mechanisms
- **Objective:** Expand catalog across 4 distinct mechanical engineering domains.
- **Milestones:**
  - Implemented 12 brand-new procedural mechanisms:
    - **Engines (4)**: Four-Stroke IC Engine, Slider-Crank Mechanism, Cam & Roller Follower, Reciprocating Pump.
    - **Linkages (4)**: Planar Four-Bar Linkage, Crank-Rocker Quick-Return, Geneva 4-Slot Maltese Cross, Scotch Yoke Mechanism.
    - **Drives (4)**: Universal (Cardan) Joint, Chain & Sprocket Drive, Ratchet & Pawl Mechanism, Lead Screw and Nut.
  - Automated tests: Expanded to 61 passing tests (`TEST-01` to `TEST-P6-14`).
  - Report: [`MECH_XR_PHASE6_REPORT.md`](MECH_XR_PHASE6_REPORT.md).

---

## Phase 6.1: Android Physical Touch Scrolling Repair
- **Objective:** Resolve physical touchscreen scrolling lock on Android mobile devices (POCO X6 5G).
- **Root Cause Identified:**
  - Card background images lacked `raycastTarget = true`, causing Unity's `GraphicRaycaster` to return 0 hits on drag gestures.
  - Viewport bounds conflicted with dynamically generated cards, requiring explicit `LayoutRebuilder.ForceRebuildLayoutImmediate()`.
- **Engineering Fixes Applied:**
  - Enforced `raycastTarget = true` across all 26 card backgrounds and container margins.
  - Fixed top bar and category filter bar while allowing vertical list underneath ($y=[0.02, 0.805]$).
  - Dynamically recalculated content height to $4398\text{ px}$.
  - Added automated mobile drag simulation tests in `SceneSetupTool.cs`.
  - Bumped Android bundle version code to `2` (`versionName = "1.1"`).
  - Automated tests: **All 71 tests passing** (`TEST-01` to `TEST-P6.1-10`).
  - Production build: `Builds/MECH-XR-PHASE6-1-v2.apk` verified and deployed to physical hardware.
  - Report: [`MECH_XR_PHASE6_1_SCROLL_FIX_REPORT.md`](MECH_XR_PHASE6_1_SCROLL_FIX_REPORT.md).

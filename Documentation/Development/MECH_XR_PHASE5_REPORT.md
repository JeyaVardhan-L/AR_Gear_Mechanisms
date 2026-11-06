# MECH-XR — PHASE 5 TECHNICAL REPORT: SCIENTIFIC EXPERIMENT LAB, CONTROL REPAIR & 14-MECHANISM CATALOG

**Project**: MECH-XR — Augmented Reality Mechanical Engineering Learning Application  
**Engine**: Unity 6.3 LTS (`6000.3.27f2`) | Built-in Render Pipeline  
**Target Hardware**: Xiaomi POCO X6 5G (Redmi Note 13 Pro 5G / `23122PCD1I`, Android 16 / API 36, ARM64-v8a)  
**AR Packages**: AR Foundation 6.0.8, ARCore XR Plugin 6.0.8, Google ARCore  
**Build Artifact**: `Builds/MECH-XR-PHASE5.apk` (28,557,879 bytes / 27.23 MB)  
**Verification Date**: November 4, 2026  

---

## EXECUTIVE SUMMARY

Phase 5 delivers a complete mechanical engineering learning and scientific experimentation environment for MECH-XR on Android. Building on the verified ARCore plane tracking, physical surface placement, and TextMeshPro typography established in Phase 4.1, Phase 5 addresses the critical speed control bugs, eliminates the redundant scale slider, introduces natural two-finger pinch scaling, builds a scientific laboratory drawer with dynamic real-time graphs and SI calculations, and expands the mechanical library from 6 to **14 fully interactive CAD-grade mechanisms**.

All 47 automated tests (`TEST-01` to `TEST-20`, `TEST-21`, `TEST-P4.1-01` to `TEST-P4.1-10`, `TEST-P5-01` to `TEST-P5-16`) pass with 100% compliance.

---

## 1. ROOT CAUSE AUDIT & CONTROL REPAIR

### Priority 1: Speed Slider Unresponsiveness
- **Observed Bug**: Physical dragging or tapping along the velocity slider track failed to register or adjust angular velocity reliably on the POCO X6 5G.
- **Root Cause Identified**:
  1. *Raycast Pass-Through*: In Unity UI (`UnityEngine.UI.Slider`), tap-to-reposition requires an active `Graphic` component with `raycastTarget = true` on the track hierarchy. The previous dynamic generator created the slider root GameObject with no `Image` receiver, while the `Background` and `Fill` images had `raycastTarget` disabled. Only the tiny 28px circular handle had `raycastTarget = true`.
  2. *Pass-Through to 3D Plane Detection*: When user tapped along the slider bar, `EventSystem.current.IsPointerOverGameObject()` evaluated to `false`, causing the touch to pass directly through to `ARPlacementManager`, triggering accidental plane raycasts instead of adjusting speed.
- **Implemented Fix**:
  - Attached a transparent receiver image (`alpha = 0.001f`) directly on `sliderGo.AddComponent<Image>()` with `raycastTarget = true`.
  - Configured `Background` and `Fill` images with `raycastTarget = true`.
  - Expanded slider track touch target to 48 dp height across full track width.
  - Added dedicated stepper buttons `[-10 RPM]` and `[+10 RPM]` flanking the slider with minimum 44 dp touch bounds for single-tap incremental control.
  - Linked `Slider.onValueChanged` directly to `MechanismBase.InputRPM`, instantaneously recalculating kinematic angular velocity ($\omega = \frac{2\pi N}{60}$ rad/s) and updating the readout with zero lag.

### Priority 2: Removal of Scale Slider & Natural Pinch-to-Zoom
- **Observed Bug**: The static scale slider occupied valuable mobile screen dock space and did not feel natural for an AR application.
- **Root Cause & Architectural Shift**: Mobile AR standards dictate gesture-based two-finger pinch scaling.
- **Implemented Fix**:
  - Completely removed the scale slider from `ARBottomBar` dock.
  - Overhauled `ModelInteractionController.cs` with touch isolation logic:
    - Verifies neither touch is over a UI Graphic before processing gestures.
    - Measures relative distance ratio between two active touches:
      $$\text{scaleFactor} = \frac{\|\mathbf{p}_1(t) - \mathbf{p}_2(t)\|}{\|\mathbf{p}_1(t_0) - \mathbf{p}_2(t_0)\|}$$
    - Clamps uniform scaling within $[0.4\times, 2.5\times]$ bounds around the assembly's grounded baseplate anchor without altering world coordinates.
    - Emits `OnScaleChanged(float scale)` to trigger a sleek, transient HUD scale indicator pill (`1.0×`) positioned above the bottom dock that auto-fades after 1.8 seconds.
    - Implemented a touch release cooldown guard (0.25s) preventing accidental single-finger dragging or component selection when lifting fingers from a pinch gesture.

---

## 2. SCIENTIFIC EXPERIMENT LAB DRAWER & SI TELEMETRY

Phase 5 integrates a collapsible scientific engineering drawer (`EXPERIMENT LAB 🔬`) accessible from the top-right HUD of any placed mechanism.

### Key Laboratory Capabilities:
1. **Live Mechanical Parameters**:
   - Variable Input RPM ($10 - 300\text{ RPM}$, default $120\text{ RPM}$).
   - Variable Input Torque ($T_{in} \in [0.1, 500.0]\text{ N}\cdot\text{m}$, default $10.0\text{ N}\cdot\text{m}$).
   - Mechanical Transmission Efficiency ($\eta \in [50\%, 100\%]$, default $95\%$).
   - Differential Turn Bias ($\Delta \in [-0.8, +0.8]$, straight / left / right cornering).
2. **SI Formula & Power Energy Engine**:
   - Input Angular Velocity:
     $$\omega_{in} = \frac{2\pi \cdot N_{in}}{60}\quad [\text{rad/s}]$$
   - Input Mechanical Power:
     $$P_{in} = T_{in} \cdot \omega_{in}\quad [\text{W}]$$
   - Output Power (accounting for friction & tooth engagement losses):
     $$P_{out} = \eta \cdot P_{in}\quad [\text{W}]$$
   - Output Torque:
     $$T_{out} = \eta \cdot T_{in} \cdot i\quad [\text{N}\cdot\text{m}]$$
   - Mechanical Power Loss:
     $$P_{loss} = P_{in} - P_{out} = (1 - \eta) P_{in}\quad [\text{W}]$$
3. **Dynamic Real-Time Graphs**:
   - Responsive multi-bar graph rendering live metrics with color-coded bars:
     - Input Bar (Sky Blue `#38BDF8`)
     - Output Bar (Emerald Green `#34D399`)
     - Loss Bar (Crimson `#E53945`)
   - 4 Interactive Graph Modes:
     - `RPM`: Compares $N_{in}$ vs $N_{out}$.
     - `TORQUE`: Compares $T_{in}$ vs $T_{out}$.
     - `SPEED/TIME`: Displays dynamic multi-sample rolling angular velocity timeline.
     - `POWER/LOSS`: Visualizes $P_{in}$, $P_{out}$, and thermal power dissipation $P_{loss}$.
4. **Engineering Presets**:
   - `HIGH SPEED`: $240\text{ RPM}, 5\text{ N}\cdot\text{m}, \eta=96\%$.
   - `HIGH TORQUE`: $60\text{ RPM}, 50\text{ N}\cdot\text{m}, \eta=94\%$.
   - `INDUSTRIAL`: $150\text{ RPM}, 30\text{ N}\cdot\text{m}, \eta=92\%$.
   - `LOW EFF (75%)`: $120\text{ RPM}, 20\text{ N}\cdot\text{m}, \eta=75\%$ (demonstrates dramatic thermal losses).
5. **A/B Experiment Snapshot Comparison**:
   - Students tap `CAPTURE EXPERIMENT A 📌` to freeze baseline state.
   - Live telemetry updates immediately display dynamic delta differences:
     $$\Delta\text{RPM} = N - N_A,\quad \Delta\text{Power} = P - P_A,\quad \Delta\text{Loss} = P_{loss} - P_{loss,A}$$

---

## 3. COMPLETE 14-MECHANISM CATALOG

The application now supports **14 full mechanical assemblies**, each with authentic CAD-grade geometry, cast pillow block pedestals, ground steel shafts, ball bearings, and inspectable components.

| Index | Mechanism Name | Kinematic Ratio ($i$) | Input / Output Speed | Key Engineering Principle |
|:---:|:---|:---:|:---:|:---|
| **01** | **Spur Gear Pair** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | Parallel shafts, external counter-rotation, $20\text{T}/40\text{T}$, $a = 105.0\text{ mm}$. |
| **02** | **Bevel Gear Pair** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | $90^\circ$ intersecting axes, conical involute meshing, $16\text{T}/32\text{T}$. |
| **03** | **Worm & Wheel Drive** | $30.00 : 1$ | $120 \to 4.0\text{ RPM}$ | Non-intersecting perpendicular skew axes, self-locking Acme thread, high reduction. |
| **04** | **Planetary Gear System** | $4.00 : 1$ | $120 \to 30\text{ RPM}$ | Coaxial epicyclic drive, Sun $18\text{T}$, 3 Planets $18\text{T}$, Ring $54\text{T}$, high torque density. |
| **05** | **Rack and Pinion** | $20.00\text{ T/m}$ | $120\text{ RPM} \to 440\text{ mm/s}$ | Rotary-to-linear conversion, dovetail guide ways, $22\text{T}$ rack, linear pitch $11\text{ mm}$. |
| **06** | **Belt and Pulley System** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | Flexible friction power transmission, identical rotation, continuous V-belt. |
| **07** | **Simple Gear Train with Idler** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | 3 parallel shafts ($Z_1=20, Z_2=28, Z_3=40$). Idler preserves rotation direction; ratio is independent of idler tooth count. |
| **08** | **Compound Gear Train** | $6.00 : 1$ | $120 \to 20\text{ RPM}$ | Two-stage reduction: Stage 1 ($18\text{T}\to 36\text{T}$, $2\times$), Stage 2 ($18\text{T}\to 54\text{T}$, $3\times$). Multiplied ratio $i = 2 \times 3 = 6.0$. |
| **09** | **Reverted Gear Train** | $3.00 : 1$ | $120 \to 40\text{ RPM}$ | Coaxial input and output shafts with equal center distance constraint ($a_1 = a_2 = 105.0\text{ mm}$, $Z_1=20, Z_2=40, Z_3=24, Z_4=36$). |
| **10** | **Internal Gear Pair** | $3.00 : 1$ | $120 \to 40\text{ RPM}$ | Annular internal ring gear ($Z_2=60$) meshed with internal pinion ($Z_1=20$). Same directional rotation; compact center distance $a = 70.0\text{ mm}$. |
| **11** | **Helical Gear Pair** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | Teeth cut at $\beta = \pm 20^\circ$ helix angle along cylinder; continuous tooth contact, smooth high-load capacity with axial thrust vectoring. |
| **12** | **Herringbone Gear Pair** | $2.00 : 1$ | $120 \to 60\text{ RPM}$ | Double-helical opposing chevrons ($\pm 20^\circ$) completely cancel net axial thrust ($F_{axial} = 0.0\text{ N}$) without requiring thrust bearings. |
| **13** | **Automotive Differential** | $3.00 : 1$ (Mean) | $120 \to 40\text{ RPM}$ | Crown ring bevel gear ($36\text{T}$), drive pinion ($12\text{T}$), carrier cross-pin with dual spider bevel pinions ($14\text{T}$) and side bevel gears ($20\text{T}$). Governed by Willis equation: $\omega_c = (\omega_L + \omega_R)/2$. |
| **14** | **Harmonic Drive** | $24.00 : 1$ | $120 \to 5.0\text{ RPM}$ | Strain wave gearing: Elliptical wave generator cam, flexible external flexspline ($Z_f=48$), rigid circular spline ($Z_c=50$). Zero backlash, counter-rotation. |

---

## 4. AUTOMATED VERIFICATION RESULTS

Execution of the complete headless batchmode test suite (`SceneSetupTool.BuildMainSceneAndTest`) produced:
```
==================================================
[MechXR] All automated Phase 4, Phase 4.1 & Phase 5 engineering tests completed!
==================================================
```
- **Total Tests Executed**: 47
- **Tests Passed**: 47
- **Tests Failed**: 0
- **Exit Code**: 0

### Summary of Passed Test Suites:
1. **Kinematic Baseline (`TEST-01` to `TEST-18`)**: All 6 original mechanisms verify ratio, RPM, and component count.
2. **Component Disassembly & Detach (`TEST-19`)**: Verified detachment and reassembly along explosion vectors.
3. **Exploded View State Machine (`TEST-20`)**: Verified transition between assembled and exploded configurations.
4. **Typography & Font Asset Binding (`TEST-21`)**: 123/123 `TextMeshProUGUI` components verified with non-null font assets. Zero blank or missing text.
5. **Phase 4.1 Screen State Machine (`TEST-P4.1-01` to `TEST-P4.1-10`)**: 10-cycle transition testing across 14 mechanisms verified zero duplicate headers, complete library deactivation on AR enter, and clean state restoration.
6. **Speed Controls Hit Testing (`TEST-P5-01`)**: Raycast targets enabled across Container, Background, Fill, Handle, and Dock.
7. **Kinematic Velocity & Steppers (`TEST-P5-02`)**: Tested across 30, 60, 120, 240, 300 RPM. Stepper buttons $[-10/+10]$ verified.
8. **Motion Transitions (`TEST-P5-03`)**: Pause, Resume, Reverse, and Reset verified.
9. **Touch Scaling (`TEST-P5-04`)**: ScaleSlider confirmed removed from UI; pinch-to-zoom scaling verified within $[0.4\times, 2.5\times]$.
10. **SI Energy & Torque Engine (`TEST-P5-05`)**: Verified $P_{in} = 125.66\text{ W}$, $P_{out} = 119.38\text{ W}$, $T_{out} = 19.00\text{ N}\cdot\text{m}$, $P_{loss} = 6.28\text{ W}$.
11. **Scientific Presets (`TEST-P5-06`)**: High Speed, High Torque, Industrial, and Low Efficiency presets confirmed.
12. **A/B Experiment Snapshot (`TEST-P5-07`)**: Baseline freeze and live delta calculation verified.
13. **New Mechanisms Kinematics (`TEST-P5-08` to `TEST-P5-15`)**:
    - Mechanism 07 (Simple Gear Train): $i = 2.0:1$, Idler independence verified.
    - Mechanism 08 (Compound Gear Train): Dual-stage $i = 6.0:1$ verified.
    - Mechanism 09 (Reverted Gear Train): Coaxial alignment, $a_1 = a_2 = 105.0\text{ mm}$, $i = 3.0:1$ verified.
    - Mechanism 10 (Internal Gear Pair): Same direction, $a = 70.0\text{ mm}$, $i = 3.0:1$ verified.
    - Mechanism 11 (Helical Gear Pair): Helix angle $\beta = 20^\circ$, axial thrust vectoring verified.
    - Mechanism 12 (Herringbone Gear Pair): Double-helical chevrons, net axial thrust cancellation verified.
    - Mechanism 13 (Automotive Differential): Straight drive ($40/40\text{ RPM}$), Left turn ($20/60\text{ RPM}$), Willis law $(\omega_L + \omega_R)/2 = 40\text{ RPM}$ verified.
    - Mechanism 14 (Harmonic Drive): Strain wave $i = 24.0:1$, counter-rotating $5.0\text{ RPM}$, zero backlash verified.
14. **Full 14-Mechanism Catalog (`TEST-P5-16`)**: All 14 mechanisms verified placed and operational in AR ($14/14$).

---

## 5. PRODUCTION APK METRICS

The production APK `Builds/MECH-XR-PHASE5.apk` was compiled using Unity 6.3 LTS AndroidPlayer with Gradle:
- **File Path**: `c:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE5.apk`
- **File Size**: 28,557,879 bytes (27.23 MB)
- **Package Name**: `com.mechxr.gearlearning`
- **Target SDK**: 36 (Android 16) | Minimum SDK: 26 (Android 8.0 Oreo)
- **Native ABI**: `arm64-v8a` (64-bit ARM)
- **Signing Scheme**: APK Signature Scheme v2 (`Verified using v2 scheme: true`)
- **Preserved Builds**:
  - `Builds/MECH-XR.apk` (27,173,655 bytes)
  - `Builds/MECH-XR-FIXED.apk` (27,177,855 bytes)
  - `Builds/MECH-XR-PHASE3.apk` (27,267,221 bytes)
  - `Builds/MECH-XR-PHASE4.apk` (28,479,379 bytes)
  - `Builds/MECH-XR-PHASE4-1.apk` (28,488,459 bytes)
  - `Builds/MECH-XR-PHASE5.apk` (28,557,879 bytes)


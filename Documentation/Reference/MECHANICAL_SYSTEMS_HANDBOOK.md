# MECH-XR — Technical Architecture & Mechanical Systems Manual
## Comprehensive Engineering Reference & System Specification

**Project:** MECH-XR — Interactive Mechanical Engineering Laboratory  
**Platform:** Android Augmented Reality Application  
**Engine:** Unity 6.3 LTS  
**Unity Version:** `6000.3.27f2`  
**Target Hardware:** Xiaomi POCO X6 5G (Redmi Note 13 Pro 5G / `23122PCD1I`)  
**Programming Language:** C# (.NET Standard 2.1 / C# 9.0)  
**AR Framework:** Unity AR Foundation 6.0.8 + Google ARCore XR Plugin 6.0.8  
**Package Identifier:** `com.mechxr.gearlearning`  
**Target Architecture:** ARM64 (`arm64-v8a`)  
**Target OS:** Android 16 (API Level 36) | Minimum OS: Android 8.0 (API Level 26)  
**Latest Production Build:** `Builds/MECH-XR-PHASE6-1-v2.apk` (Version Code 2, Version 1.1, 28.68 MB)  
**Latest Documented Milestone:** Phase 6.1 — Android Library Touch Scrolling Repair & Complete 26-Mechanism Catalog  

---

# PART 1 — UNDERSTANDING THE PROJECT

## 1.1 What is MECH-XR?

MECH-XR is an Augmented Reality-based mechanical engineering laboratory and learning application developed for Android smartphones.

It allows engineering students and educators to explore physical mechanisms as interactive 3D virtual assemblies anchored directly onto physical surfaces in their real-world surroundings.

Instead of studying only static textbook diagrams or pre-rendered videos, students can:
- View 26 CAD-grade mechanical mechanisms as fully interactive 3D assemblies.
- Anchor virtual mechanisms onto physical tables, desks, or workshop floors using AR plane detection.
- Observe real-time mechanical motion transmission and kinematic conversions.
- Adjust input rotational speed (RPM) dynamically via ergonomic touch controls.
- Monitor real-time scientific telemetry including input/output RPM, angular velocity (\(\omega\)), torque, power, and efficiency.
- Study linear displacement (\(x\)), velocity (\(v\)), and acceleration (\(a\)) graphs.
- Inspect individual mechanical components in isolation with role descriptions, engineering specs, and exploded view animations.
- Conduct A/B comparative experiments under distinct mechanical operating regimes (High Speed, High Torque, Industrial, Low Efficiency).

The application contains a comprehensive catalog of 26 mechanical mechanisms categorized into four distinct engineering disciplines:
1. **Gear Systems** (14 mechanisms)
2. **Engines & Reciprocating Mechanisms** (4 mechanisms)
3. **Linkages & Motion Conversion** (4 mechanisms)
4. **Mechanical Drives** (4 mechanisms)

## 1.2 Why did we build this project?

Traditional mechanical engineering education relies on:
1. Two-dimensional textbook schematics.
2. Abstract mathematical kinematic equations.
3. Physical laboratory test rigs.
4. Static 3D CAD drawings.
5. Static 2D slides and diagrams.

These conventional pedagogical methods present severe real-world limitations:
- **Spatial Cognitive Load:** Visualizing complex 3D kinematic interactions (such as planetary gear carrier movement, hypoid bevel tooth engagement, or automotive differential carrier motion during turns) from 2D orthogonal drawings is difficult for students.
- **Physical Equipment Bottlenecks:** Real mechanical gearboxes, four-stroke engine cutaways, and strain-wave harmonic reducers are expensive, bulky, require workshop maintenance, and can rarely be operated safely by individual students outside supervised hours.
- **Disconnected Theory vs. Practice:** Textbook equations (such as Willis' formula for planetary gears or the slider-crank displacement expansion) are often memorized symbolically without direct visual intuition of how angle changes translate to displacement spikes.

MECH-XR bridges this educational gap by combining:
- **Augmented Reality (AR):** Provides 1:1 true-scale spatial immersion in the student's personal study space.
- **Procedural 3D Geometry:** Generates exact tooth profiles, helical angles, and housing apertures on demand.
- **Kinematic Simulation:** Dynamically calculates rotational speeds, velocity ratios, and displacement curves at runtime.
- **Tactile UI Controls:** Enables real-time parameter tweaking with immediate visual feedback.
- **Engineering Telemetry:** Directly correlates applied torque, angular velocity, and mechanical power.

**Central Principle:** Turn abstract mechanical engineering equations into an interactive, spatial laboratory experience.

## 1.3 What makes MECH-XR different from a normal 3D animation?

A conventional 3D animation is a pre-baked, non-interactive visual loop:
- Objects follow hardcoded keyframes regardless of user input.
- Motion cannot be dynamically coupled to mathematical formulas.
- No physical correlation exists between speed, torque, and power loss.

In MECH-XR:
- Motion is **kinematically driven by real-time mathematical loop closure**.
- When input RPM is altered via the slider or steppers, every gear tooth, rocker link, camshaft, and plunger instantaneously adjusts its rotational and linear velocity according to governing mechanical formulas.
- In a slider-crank mechanism, piston displacement is computed at runtime from crankshaft angle (\(\theta\)), crank radius (\(r\)), and connecting rod length (\(l\)) using exact trigonometry.
- In an automotive differential, adjusting the turn bias slider redistributes wheel speeds according to Willis' axle law while maintaining carrier speed.

This makes MECH-XR a **computational kinematic laboratory**, not merely a graphical animation.

---

# PART 2 — COMPLETE TECHNOLOGY STACK

## 2.1 Technology Stack Architecture

| Layer | Technology | Version | Purpose in MECH-XR |
|---|---|---|---|
| **Engine Runtime** | Unity 6.3 LTS | `6000.3.27f2` | Core 3D engine, lifecycle management, rendering loop |
| **Language** | C# | .NET Standard 2.1 | Application programming, kinematic solvers, UI state controllers |
| **AR Framework** | Unity AR Foundation | `6.0.8` | Cross-platform XR abstraction layer for plane detection and raycasting |
| **AR Backend** | Google ARCore XR Plugin | `6.0.8` | Android native AR tracking, plane estimation, camera background |
| **XR Subsystem** | Unity XR Management | `4.5.1` | Native XR loader orchestration and lifecycle control |
| **UI Framework** | Unity UI (uGUI) + TextMeshPro | `2.0.0` | High-performance mobile UI hierarchy, responsive CanvasScaler |
| **Rendering** | Built-in Forward Pipeline | Forward | Ultra-low draw-call mobile rendering, 60 FPS thermal efficiency |
| **Mobile Target** | Android SDK / NDK | API 26–36 | Native Android deployment targeting ARM64 (`arm64-v8a`) |
| **Compilers** | IL2CPP + Gradle 9.3.1 | OpenJDK 17 | High-performance AOT compilation for 64-bit Android |
| **Testing** | Unity Test Framework | Batchmode | 71 automated regression and kinematic validation tests |
| **Target Device** | Xiaomi POCO X6 5G | HyperOS / Android 16 | Physical validation hardware (`23122PCD1I` / `garnet`) |

## 2.2 Unity 6.3 LTS Engine Concepts

- **GameObject:** The fundamental entity in Unity. In MECH-XR, GameObjects represent physical mechanical parts (`Pinion_Gear`, `Crown_Ring`, `Piston`, `Connecting_Rod`), UI panels (`LibraryScreen`, `ARHudScreen`), and system managers (`UIManager`, `ARPlacementManager`).
- **Component:** A reusable modular script or functional unit attached to a GameObject. For instance, a gear GameObject contains a `Transform`, `MeshFilter`, `MeshRenderer`, `MeshCollider`, `InspectableComponent`, and a kinematic controller subclassed from `MechanismBase`.
- **Prefab:** A serialized GameObject template. MECH-XR utilizes procedural assembly generation while serializing standard UI card layouts and inspection badges.
- **Hierarchy:** The logical tree structure organizing parent-child relationships. Child components inherit parent position and rotation transforms (e.g., planet gears are children of the planet carrier).
- **Time.deltaTime:** The time in seconds taken to complete the last frame. Multiplying rotational increments by `Time.deltaTime` ensures kinematic motion is independent of device frame rate (a mechanism rotates at identical physical speed at 30 FPS, 60 FPS, or 120 FPS).

---

# PART 3 — AUGMENTED REALITY ARCHITECTURE

## 3.1 AR Foundation & ARCore Architecture

MECH-XR utilizes Unity's **AR Foundation 6.0.8** backed by **Google ARCore 6.0.8** on Android:

```
+-------------------------------------------------------------+
|                     MECH-XR Application                     |
|           (UIManager, MechanismBase, Kinematic Solvers)      |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                    Unity AR Foundation 6.0                  |
|    (ARSession, ARPlaneManager, ARRaycastManager, XROrigin)  |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                  Unity XR Management 4.5                    |
|                      (ARCoreLoader)                         |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                     Google ARCore SDK                       |
|           (Visual-Inertial Odometry, Plane Detection)       |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|             Xiaomi POCO X6 5G Android Hardware              |
|        (Camera Sensor, 6-DoF IMU Gyroscope/Accelerometer)   |
+-------------------------------------------------------------+
```

## 3.2 Visual-Inertial Odometry (VIO)
ARCore estimates the smartphone's 6-Degree-of-Freedom (6-DoF) pose in physical space by fusing two distinct sensory inputs:
1. **Visual Stream (Camera):** Tracks high-contrast visual feature points across consecutive camera frames.
2. **Inertial Stream (IMU):** Samples the phone's gyroscope and accelerometer at high frequencies (>100 Hz).

Combining both streams compensates for the weaknesses of each: visual tracking prevents IMU drift, while the IMU provides immediate tracking during fast camera panning when visual motion blur occurs.

## 3.3 Plane Detection & Raycasting Workflow
1. **Feature Clustering:** ARCore identifies coplanar clusters of feature points located on horizontal planes (tables, desks).
2. **Convex Polygon Fitting:** Plane boundaries are estimated and surfaced to `ARPlaneManager`.
3. **Screen Raycast:** When the user taps the screen, `ARRaycastManager.Raycast()` projects a ray through the camera view into physical space.
4. **Pose Acquisition:** If an intersection occurs with a detected `TrackableType.PlaneWithinPolygon`, a 3D `Pose` (position vector and rotation quaternion) is returned.
5. **Mechanism Placement:** The active mechanism root GameObject is instantiated at that exact pose and oriented toward the user.

---

# PART 4 — PROCEDURAL 3D MESH GENERATION

Rather than importing massive static CAD files (.STEP/.OBJ/.FBX) that inflate APK size and prevent dynamic geometric adjustments, MECH-XR generates CAD-grade mechanical meshes **procedurally at runtime** via `GearMeshBuilder.cs` and its extension modules.

## 4.1 Anatomical Structure of a Unity Mesh
A 3D surface is defined by:
- **Vertices (`Vector3[]`):** Exact spatial coordinates in local space.
- **Triangles (`int[]`):** Winding indices (ordered clockwise) defining outward-facing surface normals.
- **Normals (`Vector3[]`):** Unit vectors perpendicular to vertices, used for real-time specular lighting.
- **UVs (`Vector2[]`):** Normalized texture coordinates mapping metallic/roughness shaders.

## 4.2 Mathematical Generation of an External Spur Gear
To procedurally generate a spur gear with tooth count \(Z\), module \(m\), and face width \(b\):
1. **Pitch Radius:** \(R_p = \frac{m \cdot Z}{2}\)
2. **Addendum (Tip) Radius:** \(R_a = R_p + m\)
3. **Dedendum (Root) Radius:** \(R_d = R_p - 1.25m\)
4. **Angular Pitch:** \(\Delta\theta = \frac{2\pi}{Z}\)
5. **Involute Approximation:** For each tooth \(i \in [0, Z-1]\), the profile transitions across:
   - Root flank: \(\theta_0 = i \cdot \Delta\theta\)
   - Pitch intersection: \(\theta_1 = \theta_0 + 0.25 \cdot \Delta\theta\)
   - Tooth crest: \(\theta_2 = \theta_0 + 0.50 \cdot \Delta\theta\)
   - Trailing flank: \(\theta_3 = \theta_0 + 0.75 \cdot \Delta\theta\)
6. **Extrusion:** Vertices are mirrored at \(z = -b/2\) and \(z = +b/2\) to construct the front face, rear face, tooth crests, root troughs, and bore cylinder.

---

# PART 5 — KINEMATIC SIMULATION & MECHANICAL MATHEMATICS

## 5.1 SI Unit Kinematic Foundations

### Angular Velocity Conversion
Given an input rotational speed \(N\) in revolutions per minute (RPM):
\[
\omega = \frac{2\pi N}{60} \quad [\text{rad/s}]
\]

### Mechanical Power & Torque
Mechanical power transmitted by a rotating shaft:
\[
P_{\text{in}} = T_{\text{in}} \cdot \omega_{\text{in}} \quad [\text{Watts}]
\]
Accounting for mechanical gear mesh efficiency \(\eta\) (typically 90%–98%):
\[
P_{\text{out}} = P_{\text{in}} \cdot \eta
\]
\[
P_{\text{loss}} = P_{\text{in}} - P_{\text{out}} = P_{\text{in}} \cdot (1 - \eta)
\]
Output torque under reduction ratio \(i = \frac{N_{\text{in}}}{N_{\text{out}}}\):
\[
T_{\text{out}} = T_{\text{in}} \cdot i \cdot \eta \quad [\text{N}\cdot\text{m}]
\]

---

# PART 6 — EXPERIMENT LAB & INTERACTIVE TELEMETRY

The **Experiment Lab** (`UIManager.cs`) is MECH-XR's analytical laboratory interface.

## 6.1 Real-Time Experiment Controls
- **Precision RPM Slider:** Adjusts simulated shaft speed from \(10\text{ RPM}\) to \(300\text{ RPM}\) (default \(120\text{ RPM}\)).
- **Tactile Step Buttons:** \(-10\text{ RPM}\) and \(+10\text{ RPM}\) fine-tuning steppers.
- **Two-Finger Pinch Scaling:** Dynamic assembly resizing clamped within \([0.4\times, 2.5\times]\) bounding envelopes.
- **Direction Reversal:** Toggles shaft rotation vector between clockwise and counter-clockwise.
- **Kinematic Pause / Run:** Freezes kinematic integration for static component inspection.

## 6.2 Pre-Configured Operating Presets
1. **High Speed (Racing / Spindle):** \(300\text{ RPM}\), \(5.0\text{ N}\cdot\text{m}\), \(\eta = 0.95\)
2. **High Torque (Heavy Industrial):** \(45\text{ RPM}\), \(25.0\text{ N}\cdot\text{m}\), \(\eta = 0.88\)
3. **Balanced Industrial:** \(120\text{ RPM}\), \(10.0\text{ N}\cdot\text{m}\), \(\eta = 0.92\)
4. **Low Efficiency (Worn / Misaligned):** \(120\text{ RPM}\), \(10.0\text{ N}\cdot\text{m}\), \(\eta = 0.70\)

## 6.3 A/B Comparative Analysis System
Students can snapshot current operating telemetry into **Slot A**, adjust input variables (e.g. increase RPM or lower efficiency), and view live calculated deltas (\(\Delta\text{RPM}, \Delta P, \Delta\text{Loss}\)) in **Slot B** side-by-side.

---

# PART 7 — COMPLETE 26-MECHANISM CATALOGUE

MECH-XR features 26 fully realized mechanical systems categorized into four engineering groups.

```
+----------------------------------------------------------------------------------------------------+
|                                    MECH-XR 26-MECHANISM LIBRARY                                    |
+----------------------------------+----------------------------------+------------------------------+
| 1. GEAR SYSTEMS (14)             | 2. ENGINES & RECIPROCATING (4)   | 3. LINKAGES & CONVERSION (4) |
|  01. Spur Gear Pair              |  15. Four-Stroke IC Engine       |  19. Four-Bar Linkage        |
|  02. Bevel Gear Pair             |  16. Slider-Crank Mechanism      |  20. Crank-Rocker Mechanism  |
|  03. Worm & Wheel                |  17. Cam and Follower            |  21. Geneva Mechanism        |
|  04. Planetary Gear System       |  18. Reciprocating Pump          |  22. Scotch Yoke             |
|  05. Rack and Pinion             +----------------------------------+------------------------------+
|  06. Belt and Pulley System      | 4. MECHANICAL DRIVES (4)                                        |
|  07. Simple Gear Train           |  23. Universal / Cardan Joint                                   |
|  08. Compound Gear Train         |  24. Chain and Sprocket Drive                                   |
|  09. Reverted Gear Train         |  25. Ratchet and Pawl                                           |
|  10. Internal Gear Pair          |  26. Lead Screw and Nut                                         |
|  11. Helical Gear Pair           +-----------------------------------------------------------------+
|  12. Herringbone Gear Pair       |
|  13. Automotive Differential     |
|  14. Harmonic Drive              |
+----------------------------------+
```

### 1. Spur Gear Pair
- **Kinematics:** Parallel shaft transmission. \(i = \frac{Z_2}{Z_1} = \frac{40}{20} = 2.00 : 1\). Opposite rotation.
- **Governing Equations:** \(\omega_2 = -\frac{\omega_1}{i}\), \(a = R_1 + R_2\).

### 2. Bevel Gear Pair
- **Kinematics:** Right-angle \(90^\circ\) intersecting shaft transmission. \(i = \frac{30}{15} = 2.00 : 1\).
- **Pitch Cones:** \(\gamma_1 = \arctan(1/i) = 26.56^\circ\), \(\gamma_2 = 90^\circ - \gamma_1 = 63.44^\circ\).

### 3. Worm & Wheel
- **Kinematics:** Non-intersecting perpendicular shafts. Single-start worm (\(Z_1 = 1\)) meshing with 30-tooth wheel (\(Z_2 = 30\)).
- **Ratio:** \(i = 30.00 : 1\). Extremely high single-stage reduction; demonstrates self-locking principles.

### 4. Planetary Gear System
- **Kinematics:** Epicyclic gear train. Sun gear (\(Z_s = 18\)), 3 Planet gears (\(Z_p = 18\)), fixed Annulus ring gear (\(Z_r = 54\)).
- **Willis Formula:** \(i = 1 + \frac{Z_r}{Z_s} = 1 + \frac{54}{18} = 4.00 : 1\). High torque coaxial reduction.

### 5. Rack and Pinion
- **Kinematics:** Rotary-to-linear conversion. 20-tooth pinion meshing with linear pitch rack.
- **Linear Velocity:** \(v_{\text{rack}} = \omega_{\text{pinion}} \cdot R_{\text{pitch}}\).

### 6. Belt and Pulley System
- **Kinematics:** Parallel friction transmission. Driver pulley (\(D_1 = 70\text{ mm}\)), Driven pulley (\(D_2 = 140\text{ mm}\)).
- **Ratio:** \(i = \frac{D_2}{D_1} = 2.00 : 1\). Identical rotational direction via continuous closed belt.

### 7. Simple Gear Train (with Idler)
- **Kinematics:** Driver (\(Z_1 = 20\)), Idler (\(Z_2 = 28\)), Driven (\(Z_3 = 40\)).
- **Idler Principle:** Overall ratio \(i = \frac{Z_3}{Z_1} = 2.00 : 1\). The idler reverses rotation so driver and driven rotate in the **same** direction; idler tooth count cancels algebraically out of the velocity ratio.

### 8. Compound Gear Train
- **Kinematics:** Dual-stage reduction. Driver (\(Z_1 = 20\)) drives Stage 1 (\(Z_2 = 40\)); shared countershaft carries Stage 2 pinion (\(Z_3 = 15\)) driving output (\(Z_4 = 45\)).
- **Total Ratio:** \(i_{\text{total}} = i_1 \cdot i_2 = \left(\frac{40}{20}\right) \cdot \left(\frac{45}{15}\right) = 2.0 \cdot 3.0 = 6.00 : 1\). Output rotates at \(20\text{ RPM}\) from \(120\text{ RPM}\) input.

### 9. Reverted Gear Train
- **Kinematics:** Coaxial compound reducer where input and output shafts share the same physical centerline.
- **Center Distance Constraint:** \(a_1 = R_1 + R_2 = R_3 + R_4 = a_2 = 105.0\text{ mm}\). Compact clockwork/transmission layout.

### 10. Internal Gear Pair
- **Kinematics:** External pinion (\(Z_1 = 18\)) meshing inside annular internal ring gear (\(Z_2 = 54\)).
- **Direction:** Unlike external gears, driver and driven rotate in the **same direction**. Center distance \(a = R_2 - R_1\).

### 11. Helical Gear Pair
- **Kinematics:** Parallel shafts with teeth cut at a \(\beta = 20.0^\circ\) helix angle.
- **Engagement:** Gradual progressive contact line creates smooth, quiet meshing at the expense of creating axial thrust forces.

### 12. Herringbone Gear Pair (Double-Helical)
- **Kinematics:** Opposed mirrored helices (\(+20^\circ\) and \(-20^\circ\)) forming continuous chevron teeth.
- **Thrust Balance:** Axial thrust vectors cancel out completely (\(F_{a1} + F_{a2} = 0.0\text{ N}\)), eliminating thrust bearing requirements in heavy machinery.

### 13. Automotive Differential
- **Kinematics:** Willis axle differential with bevel spider gears and dual output side gears.
- **Willis Axle Law:** \(\omega_{\text{carrier}} = \frac{\omega_{\text{left}} + \omega_{\text{right}}}{2}\).
- **Turn Simulation:** Adjusting turn bias redistributes wheel speeds dynamically (e.g. Left Turn: Left Wheel drops to \(20\text{ RPM}\), Right Wheel accelerates to \(60\text{ RPM}\), Carrier remains constant at \(40\text{ RPM}\)).

### 14. Harmonic Drive (Strain Wave Robotics)
- **Kinematics:** Elliptical wave generator, flexible external-toothed cup (Flexspline, \(Z_f = 48\)), and rigid circular spline (\(Z_c = 50\)).
- **Reduction Ratio:** \(i = \frac{Z_f}{Z_c - Z_f} = \frac{48}{50 - 48} = 24.00 : 1\). Counter-rotating output with zero backlash for precision robotics.

### 15. Four-Stroke Internal Combustion Engine
- **Kinematics:** Full \(720^\circ\) thermodynamic educational cycle spanning two complete crankshaft revolutions:
  1. **Intake (\(0^\circ\)–\(180^\circ\)):** Piston descends, intake valve opens.
  2. **Compression (\(180^\circ\)–\(360^\circ\)):** Piston ascends, both valves sealed.
  3. **Power (\(360^\circ\)–\(540^\circ\)):** Spark plug representation fires, combustion pushes piston down.
  4. **Exhaust (\(540^\circ\)–\(720^\circ\)):** Piston ascends, exhaust valve opens to purge gas.
- **Camshaft Timing:** Camshaft rotates at exactly half crankshaft speed: \(N_{\text{cam}} = \frac{N_{\text{crank}}}{2}\).

### 16. Slider-Crank Mechanism
- **Kinematics:** Crank of radius \(r = 40\text{ mm}\) and connecting rod of length \(l = 120\text{ mm}\) (\(\lambda = r/l = 0.333\)).
- **Piston Position Function:**
  \[
  x(\theta) = r\cos\theta + \sqrt{l^2 - r^2\sin^2\theta}
  \]
- **Velocity & Acceleration:** Demonstrates secondary harmonic acceleration distortion due to connecting rod angularity.

### 17. Cam and Follower
- **Kinematics:** Eccentric plate cam with roller follower. Base radius \(R_b = 35\text{ mm}\), lift \(h = 18\text{ mm}\).
- **Motion Law:** Converts uniform rotary cam velocity into smooth harmonic rise, dwell, and return profiles.

### 18. Reciprocating Pump
- **Kinematics:** Positive displacement plunger pump driven by slider-crank geometry.
- **Valve Operation:** One-way suction valve opens during stroke \(0^\circ\)–\(180^\circ\); delivery check valve opens during discharge stroke \(180^\circ\)–\(360^\circ\).
- **Theoretical Discharge:** \(Q = \frac{A \cdot S \cdot N}{60} \quad [\text{L/min}]\).

### 19. Four-Bar Linkage
- **Kinematics:** Planar 1-DOF closed kinematic chain: Ground \(d = 120\text{ mm}\), Crank \(a = 40\text{ mm}\), Coupler \(b = 100\text{ mm}\), Rocker \(c = 85\text{ mm}\).
- **Grashof Criterion:** \(s + l \le p + q \implies 40 + 120 \le 85 + 100 \implies 160 \le 185\) (Satisfied — Grashof Class I). Crank rotates continuously through \(360^\circ\).

### 20. Crank-Rocker Mechanism
- **Kinematics:** Converts continuous \(360^\circ\) crank input into oscillating rocker movement.
- **Quick-Return Ratio:** Demonstrates asymmetric forward and return stroke time ratio (\(Q = 1.16 : 1\)) used in shaping machines.

### 21. Geneva Mechanism (Maltese Cross)
- **Kinematics:** 4-slot Geneva wheel with rotating driving pin and locking disc.
- **Intermittent Indexing:** Each crank revolution indexes the output wheel by \(\Delta\theta = \frac{360^\circ}{4} = 90^\circ\) during \(90^\circ\) of crank pin engagement, followed by a stationary \(270^\circ\) dwell (75% dwell duty cycle).

### 22. Scotch Yoke Mechanism
- **Kinematics:** Rotating pin engaging directly into a slotted sliding yoke.
- **Pure Simple Harmonic Motion (SHM):**
  \[
  x(t) = r\cos(\omega t), \quad v(t) = -r\omega\sin(\omega t), \quad a(t) = -r\omega^2\cos(\omega t)
  \]
  Zero connecting rod angularity distortion (pure fundamental frequency).

### 23. Universal Joint (Cardan Joint)
- **Kinematics:** Cross-pin spider connecting non-collinear shafts at operating angle \(\beta = 20.0^\circ\).
- **Non-Uniform Velocity:**
  \[
  \tan\theta_2 = \tan\theta_1 \cdot \cos\beta
  \]
  Output velocity fluctuates sinusoidally at twice the shaft rotation frequency (\(2\omega\)), generating \(\pm 6.2\%\) speed ripple.

### 24. Chain and Sprocket Drive
- **Kinematics:** Positive mechanical power transmission via roller chain. Driver sprocket (\(Z_1 = 15\)), Driven (\(Z_2 = 30\)), \(i = 2.00 : 1\).
- **Chordal Action:** Linear chain speed fluctuates slightly (\(2.19\%\)) as chain rollers seat into polygonal sprocket chords.

### 25. Ratchet and Pawl
- **Kinematics:** 24-tooth asymmetric sawtooth wheel with oscillating drive pawl and stationary reverse-arrest detent pawl.
- **Unidirectional Indexing:** Increments output wheel by \(\Delta\theta = \frac{360^\circ}{24} = 15.0^\circ\) per stroke, preventing reverse back-driving.

### 26. Lead Screw and Nut
- **Kinematics:** Single-start trapezoidal Acme lead screw (\(d = 24\text{ mm}\), lead \(L = 6\text{ mm/rev}\)).
- **Linear Carriage Speed:** \(v = \frac{L \cdot N}{60} = 12.0\text{ mm/s}\) at \(120\text{ RPM}\).
- **Self-Locking & Force Multiplication:** Generates massive axial clamping thrust (\(\approx 9,948\text{ N}\) at \(10\text{ N}\cdot\text{m}\) torque) with self-locking helix angle.

---

# PART 8 — UI ARCHITECTURE & FINITE STATE MACHINE

## 8.1 Finite State Machine (`AppScreenState`)
To guarantee that UI panels never overlap, `UIManager.cs` implements a strict screen state machine:

```
                  +-----------------------+
                  |        Library        |
                  +-----------------------+
                              | (Select Mechanism)
                              v
                  +-----------------------+
                  |       ARScanning      |
                  +-----------------------+
                              | (Surface Detected & Placed)
                              v
                  +-----------------------+ <---------------+
                  |        ARPlaced       |                 |
                  +-----------------------+                 | (Reassemble /
                    | (Inspect)      | (Explode)            |  Deselect)
                    v                v                      |
         +--------------------+   +-------------------+     |
         | ComponentInspect   |   |   ExplodedView    | ----+
         +--------------------+   +-------------------+
```

1. **`Library`:** 26 mechanism cards visible; all AR panels, scanning instructions, and 3D models destroyed.
2. **`ARScanning`:** Library completely deactivated; AR HUD active; "Point camera at horizontal surface" guidance banner displayed.
3. **`ARPlaced`:** Guidance banner hidden; 3D mechanism actively tracked and animated; bottom experiment controls and RPM dock visible.
4. **`ComponentInspection`:** Normal controls docked; component technical specifications, role description, and isolation detach buttons active.
5. **`ExplodedView`:** Parts separated along outward radial trajectories with visual exploded-view guide labels.

---

# PART 9 — MAJOR ENGINEERING BUGS ENCOUNTERED & REPAIRED

### Bug 1: Mechanism Library Touch Scrolling Inactivity on Android (Phase 6.1)
- **Symptom:** On the physical Xiaomi POCO X6 5G, only the first 6 mechanism cards could be seen; vertical swipe gestures were completely ignored.
- **Root Cause:** In `UIManager.CreateMenuCard`, the background card image had `raycastTarget = false`. Because card backgrounds covered 80% of screen space, touch down events produced zero hits in `GraphicRaycaster`. The `EventSystem` never generated drag pointer data, starving `ScrollRect.OnBeginDrag`.
- **Surgical Solution:**
  1. Enabled `cardImg.raycastTarget = true` on all 26 cards.
  2. Added transparent raycast receiver images to `Viewport` and `Content` margins.
  3. Enforced `LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect)` and `Canvas.ForceUpdateCanvases()`, expanding content height to **$4398\text{ px}$** ($3419\text{ px}$ scrollable travel).
  4. Added `StopMovement()` and `verticalNormalizedPosition = 1f` on category filters.

### Bug 2: Missing Collider on Differential Carrier Housing
- **Symptom:** Unity Editor batchmode build threw a `NullReferenceException` inside `AutomotiveDifferential.BuildMechanismAssembly()`.
- **Root Cause:** `carrierHousing.AddComponent<InspectableComponent>()` requires a `Collider` component (`[RequireComponent(typeof(Collider))]`). Because no collider existed on `Carrier_Housing`, component attachment returned `null`.
- **Surgical Solution:** Inserted `carrierHousing.AddComponent<MeshCollider>().sharedMesh = mfHousing.mesh;` immediately prior to component attachment.

### Bug 3: EditMode Object Destruction Warnings
- **Symptom:** Automated headless test runs flooded logs with `Destroy may not be called from edit mode! Use DestroyImmediate instead.`
- **Root Cause:** Calling `Object.Destroy()` during editor-time test executions is prohibited by Unity.
- **Surgical Solution:** Implemented `SafeDestroy(GameObject go)` in `ARPlacementManager.cs`:
  ```csharp
  private void SafeDestroy(GameObject go)
  {
      if (go == null) return;
      if (Application.isPlaying) Destroy(go);
      else DestroyImmediate(go);
  }
  ```

---

# PART 10 — TESTING & QUALITY ASSURANCE

## 10.1 Automated Test Suite Results
Every mechanism, state transition, and touch configuration is verified via automated headless batchmode executions (`MechXR.Editor.SceneSetupTool.BuildMainSceneAndTest`).

**Final Test Summary: 71 / 71 Tests Passed (100% Success, Exit Code 0)**

- **Baseline Kinematic Verifications (`TEST-01` to `TEST-20`):** 20 tests validating gear ratios, velocity output, component detach, and exploded view toggles.
- **Typography & Font Integrity (`TEST-21`):** 1 test confirming 100% non-null SDF material binding across all 123 UI text elements.
- **Phase 4.1 State Machine Tests (`TEST-P4.1-01` to `TEST-P4.1-10`):** 10 tests validating clean startup, AR transitions, inspection states, and zero duplicate header instantiations across 10 repeated cycles.
- **Phase 5 Controls & Laboratory Tests (`TEST-P5-01` to `TEST-P5-16`):** 16 tests verifying speed controls raycasting, velocity mapping, pinch-to-zoom bounds, SI energy calculations, and Mechanisms 07–14.
- **Phase 6 Kinematic Expansion Tests (`TEST-P6-01` to `TEST-P6-14`):** 14 tests confirming kinematic calculations, stroke lengths, valve timings, and catalog verification for Mechanisms 15–26.
- **Phase 6.1 Mobile Touch Scrolling Tests (`TEST-P6.1-01` to `TEST-P6.1-10`):** 10 tests confirming `ScrollRect` configuration, content expansion ($4398\text{ px} \ge 4000\text{ px}$), card raycast targets (26/26), simulated swipe dragging down/up, and category reset.

---

# PART 11 — PRODUCTION BUILD SPECIFICATIONS

```text
================================================================================
MECH-XR PRODUCTION RELEASE ARTIFACT (PHASE 6.1 v2)
================================================================================
APK File Path:       Builds/MECH-XR-PHASE6-1-v2.apk
Secondary Target:    Builds/MECH-XR-PHASE6-1.apk
Binary File Size:    28,678,771 bytes (27.35 MB)
Application ID:      com.mechxr.gearlearning
Version Code:        2
Version Name:        1.1
Target CPU Arch:     ARM64 (arm64-v8a)
Minimum SDK:         API 26 (Android 8.0 Oreo)
Target / Build SDK:  API 36 (Android 16)
Signing Scheme:      APK Signature Scheme v2 (Verified: true)
AR Capabilities:     android.hardware.camera.ar (Required)
                     com.google.ar.core.depth (Supported)
================================================================================
```

---

# PART 12 — ARCHITECTURAL & ENGINEERING DESIGN FAQ

### Q1: What is the core objective of MECH-XR?
**Answer:** MECH-XR is an interactive Android Augmented Reality application designed to transform abstract mechanical engineering theory into spatial 3D visualizations and quantitative laboratory simulations. It enables students to anchor 26 mechanical systems in their real environment, interact with operational parameters (RPM, torque, efficiency), and observe real-time kinematics and telemetry.

### Q2: What is the difference between Kinematics and Kinetics? Which does MECH-XR simulate?
**Answer:** Kinematics is the study of motion (position, displacement, velocity, acceleration) without regard to the forces causing it. Kinetics is the study of motion considering the underlying forces, torques, combustion pressures, and mass moments of inertia. MECH-XR is primarily a **kinematic educational simulator**; while it calculates power and torque relationships from basic SI formulas (\(P = T\omega\)), it does not model internal dynamic gas pressures, thermal expansion, or finite element stress distributions.

### Q3: How does ARCore anchor a virtual gearbox onto a physical desk?
**Answer:** ARCore uses Visual-Inertial Odometry (fusing camera visual feature tracking with high-frequency 6-DoF IMU sensor data) to detect coplanar feature clusters representing horizontal planes. When the user taps a plane, AR Foundation projects an AR raycast. The resulting intersection yields a 3D Pose (Vector3 translation and Quaternion rotation) where Unity instantiates the root GameObject.

### Q4: Why did we build 3D models procedurally in C# rather than importing CAD models from SolidWorks or Blender?
**Answer:** Procedural mesh generation provides three major advantages:
1. **Zero Asset Overhead:** Generating vertices and triangles mathematically in memory keeps the APK footprint under 28 MB.
2. **Parametric Control:** Geometry parameters (tooth count, face width, pitch diameter, helix angle) are live C# variables that can be modified programmatically.
3. **Optimized Draw Calls:** Meshes are generated with minimal vertex budgets, enabling smooth 60 FPS performance on mobile chipsets without thermal throttling.

### Q5: What was the root cause of the touch scrolling failure in Phase 6.1, and how was it solved?
**Answer:** In Unity UI, `ScrollRect` relies on `GraphicRaycaster` to capture drag gestures. The mechanism cards had `raycastTarget = false` on their background images, so touches fell through into empty space without triggering `OnBeginDrag`. Furthermore, `ContentSizeFitter` had not calculated layout dimensions before serialization. We enabled `raycastTarget = true` across all cards, added transparent raycast receivers in viewport margins, and called `LayoutRebuilder.ForceRebuildLayoutImmediate()` to dynamically expand the content container to $4398\text{ px}$.

### Q6: Why did the four-stroke engine require a 720-degree cycle?
**Answer:** In a four-stroke internal combustion engine, each stroke (Intake, Compression, Power, Exhaust) spans $180^\circ$ of crankshaft rotation. Completing all four strokes requires two complete revolutions of the crankshaft ($2 \times 360^\circ = 720^\circ$). Because each valve opens once per cycle, the camshaft must rotate at exactly half crankshaft speed: \(N_{\text{cam}} = \frac{N_{\text{crank}}}{2}\).

### Q7: What is Grashof's Criterion in the Four-Bar Linkage?
**Answer:** Grashof's theorem states that for a planar four-bar linkage, if the sum of the shortest link (\(s\)) and longest link (\(l\)) is less than or equal to the sum of the remaining two intermediate links (\(p + q\)):
\[
s + l \le p + q
\]
Then at least one link can rotate continuously through a full $360^\circ$ relative to the other links. In MECH-XR, \(s = 40\text{ mm}, l = 120\text{ mm}, p = 85\text{ mm}, q = 100\text{ mm}\). Because \(160 \le 185\), the mechanism satisfies Grashof Class I, enabling continuous crank rotation.

### Q8: How does an automotive differential work during a vehicle turn?
**Answer:** When a car turns, the outer wheel travels along an arc of larger radius than the inner wheel, requiring it to rotate faster. A differential uses bevel spider gears mounted inside a rotating carrier to decouple the two half-shafts while transmitting torque. According to Willis' law:
\[
\omega_{\text{carrier}} = \frac{\omega_{\text{left}} + \omega_{\text{right}}}{2}
\]
If the left wheel slows down by \(\Delta\omega\), the spider gears rotate on their cross-pin, accelerating the right wheel by an equal \(\Delta\omega\), keeping carrier speed constant.

### Q9: Does a single Cardan (Universal) joint maintain constant angular velocity?
**Answer:** No. While a universal joint accommodates angular misalignment (\(\beta\)) between shafts, the output shaft experiences sinusoidal velocity fluctuations that cycle twice per revolution ($2\omega$). The relationship is governed by \(\tan\theta_2 = \tan\theta_1\cos\beta\). To achieve constant velocity in automotive drivetrains, two Cardan joints must be used in series with matching angles, or a specialized Constant Velocity (CV) Rzeppa joint must be used.

### Q10: How does MECH-XR achieve frame-rate independent rotation?
**Answer:** By scaling angular displacement increments by `Time.deltaTime` in the `Update()` loop:
```csharp
float deltaAngle = currentInputRPM * 6f * Time.deltaTime; // 360 deg / 60 sec = 6 deg/sec per RPM
currentCrankAngleDeg = (currentCrankAngleDeg + deltaAngle) % 360f;
```
Because `Time.deltaTime` measures the exact elapsed time between frames in seconds, the physical rotational velocity remains identical whether the device is rendering at 30 FPS, 60 FPS, or 120 FPS.

---

# PART 13 — SOURCE CODE MAP & REPOSITORY ARCHITECTURE

```text
MECH-XR/
├── Assets/
│   ├── Editor/
│   │   ├── AndroidBuildTool.cs         # Batchmode Android build orchestration & APK signing
│   │   └── SceneSetupTool.cs           # Automated 71-test validation suite & scene serialization
│   ├── Scenes/
│   │   └── MainScene.unity             # Complete serialized production AR scene
│   └── Scripts/
│       ├── AR/
│       │   ├── ARPlacementManager.cs   # ARCore plane detection, raycasting & safe lifecycle
│       │   └── PlacementReticle.cs     # Real-time planar surface target visualization
│       ├── Interaction/
│       │   ├── InspectableComponent.cs # Individual part selection, isolation & metadata
│       │   └── ModelInteractionController.cs # Touch rotation & two-finger pinch-to-zoom
│       ├── Mechanisms/                 # 26 Procedural Kinematic Mechanism Controllers
│       │   ├── MechanismBase.cs        # Abstract base class for kinematics & telemetry
│       │   ├── AutomotiveDifferential.cs
│       │   ├── FourStrokeEngine.cs
│       │   ├── HarmonicDrive.cs
│       │   ├── HerringboneGearPair.cs
│       │   ├── PlanetaryGearSystem.cs
│       │   └── [21 other mechanisms...]
│       ├── Procedural/                 # Procedural Mesh Geometry Builders
│       │   ├── GearMeshBuilder.cs      # Core procedural gear, bore & tooth extruders
│       │   ├── GearMeshBuilder_Batch1.cs
│       │   ├── GearMeshBuilder_Batch2.cs
│       │   ├── GearMeshBuilder_Batch3.cs
│       │   └── GearMeshBuilder_Extensions.cs
│       └── UI/
│           └── UIManager.cs            # FSM Screen manager, Experiment Lab & ScrollRect
├── Builds/
│   ├── MECH-XR-PHASE6-1-v2.apk         # Latest Production Release APK (v1.1, Code 2)
│   └── MECH-XR-PHASE6-1.apk            # Primary Production Release APK
├── Packages/
│   └── manifest.json                   # AR Foundation 6.0.8, ARCore 6.0.8, uGUI 2.0.0
└── MECH_XR_VIVA_HANDBOOK.md            # This Master Handbook Document
```

---

# CONCLUSION

MECH-XR demonstrates how modern real-time graphics engines and mobile Augmented Reality can bridge the divide between theoretical mechanical engineering equations and tangible physical intuition. By combining procedural CAD geometry, real-time kinematic solvers, rigorous software state management, and ergonomic mobile UI design, the application provides an accessible, high-fidelity spatial laboratory experience directly on everyday Android smartphones.


# MECH-XR — Comprehensive Mechanism Catalog (26 Systems)

This catalog details the complete set of **26 CAD-grade mechanical engineering systems** implemented in MECH-XR Phase 6. Every mechanism features procedurally generated 3D geometry, interactive component inspection with physical metadata, animated exploded views, closed-loop kinematics, and deep integration with the Scientific Experiment Lab.

---

## Category 1: Gear Systems (14 Mechanisms)

### 01. Spur Gear Pair
- **Index**: `0`
- **Category**: Parallel Transmission
- **Kinematics**: Pitch radii $r_1 = 35\text{ mm}, r_2 = 70\text{ mm}$, Module $m = 3.5\text{ mm}$, Pressure angle $\alpha = 20^\circ$.
- **Ratio**: $i = Z_2 / Z_1 = 40 / 20 = 2.0:1$ speed reduction.
- **Direction**: Counter-rotating.
- **Components**: Involute Driver Pinion, Spoked Driven Gear, Drive Shafts, Pillow Blocks, Ground Baseplate.

### 02. Bevel Gear Pair
- **Index**: `1`
- **Category**: Right-Angle Drive
- **Kinematics**: Pitch cone angles $\delta_1 = 26.57^\circ, \delta_2 = 63.43^\circ$, Shaft angle $\Sigma = 90.0^\circ$.
- **Ratio**: $i = Z_2 / Z_1 = 32 / 16 = 2.0:1$.
- **Direction**: 90° intersecting orthogonal transmission.
- **Components**: Conical Driver Pinion, Conical Driven Crown Gear, Dual Orthogonal Shafts, Angle Bracket, Pillow Blocks.

### 03. Worm and Wheel Drive
- **Index**: `2`
- **Category**: High-Reduction Drive
- **Kinematics**: Single-start Acme cylindrical worm ($z_1 = 1$, lead angle $\gamma = 7.39^\circ$), 30-tooth throated worm wheel ($z_2 = 30$, friction angle $\rho' = 8.53^\circ$).
- **Ratio**: $i = z_2 / z_1 = 30 / 1 = 30.0:1$.
- **Special Feature**: Inherent self-locking ($\gamma < \rho'$); payload cannot backdrive the worm.
- **Components**: Cylindrical Acme Worm, Throated Bronze Wheel, Perpendicular Non-Intersecting Shafts, Thrust Bearing Mounts.

### 04. Planetary Gear System (Epicyclic)
- **Index**: `3`
- **Category**: Epicyclic Drive
- **Kinematics**: Sun gear $Z_s = 18$, 3 Planet gears $Z_p = 18$, Fixed Annular Ring $Z_r = 54$.
- **Ratio**: $i = (Z_s + Z_r) / Z_s = (18 + 54) / 18 = 4.0:1$.
- **Output**: Tri-lobed planet carrier rotating coaxially at $N_c = N_s / 4.0$.
- **Components**: Sun Gear, 3x Planet Gears, Fixed Ring Gear, Planet Carrier Cage, Coaxial Shafts.

### 05. Rack and Pinion Pair
- **Index**: `4`
- **Category**: Linear Converter
- **Kinematics**: 20-tooth driver pinion ($r_p = 35\text{ mm}$), linear rack ($m = 3.5\text{ mm}$, pitch $p = 11.0\text{ mm}$).
- **Velocity**: Tangential linear velocity $v = \omega \cdot r_p = \frac{\pi \cdot m \cdot Z \cdot N}{60}$ mm/s.
- **Components**: Involute Pinion, Linear Slotted Rack, Dovetail Guide Rail, End Stops.

### 06. Belt and Pulley System
- **Index**: `5`
- **Category**: Friction Power Drive
- **Kinematics**: Driver pulley $D_1 = 60\text{ mm}$, Driven pulley $D_2 = 120\text{ mm}$, Center distance $C = 150\text{ mm}$.
- **Ratio**: $i = D_2 / D_1 = 120 / 60 = 2.0:1$.
- **Direction**: Same direction; continuous flexible belt loop.
- **Components**: Driver Flanged Pulley, Driven Spoked Pulley, Looping V-Belt, Dual Parallel Shafts.

### 07. Simple Gear Train with Idler
- **Index**: `6`
- **Category**: Multi-Shaft Idler Drive
- **Kinematics**: Driver $Z_1 = 20$, Intermediate Idler $Z_2 = 28$, Driven $Z_3 = 40$.
- **Ratio**: $i = Z_3 / Z_1 = 40 / 20 = 2.0:1$. Idler changes direction without altering overall ratio.
- **Components**: Driver Gear, Center Idler Gear, Driven Output Gear, 3x Shafts, Triple-Bearing Baseplate.

### 08. Compound Gear Train
- **Index**: `7`
- **Category**: Dual-Stage Reducer
- **Kinematics**: Stage 1 ($Z_1 = 18 \to Z_2 = 36$, $2.0:1$), Stage 2 ($Z_3 = 18 \to Z_4 = 54$, $3.0:1$).
- **Ratio**: $i = i_1 \times i_2 = 2.0 \times 3.0 = 6.0:1$ massive reduction.
- **Components**: Input Pinion, Compound Countershaft Cluster, Output Bull Gear, Heavy-Duty Cast Bed.

### 09. Reverted Gear Train
- **Index**: `8`
- **Category**: Coaxial Reducer
- **Kinematics**: Equal center distances $a_1 = a_2 = 105.0\text{ mm}$. Coaxial input and output shafts.
- **Ratio**: Stage 1 ($20T \to 40T$, $2.0:1$), Stage 2 ($24T \to 36T$, $1.5:1$). Overall $i = 3.0:1$.
- **Components**: Input Shaft & Gear, Countershaft Pair, Coaxial Output Gear & Sleeve.

### 10. Internal Gear Pair
- **Index**: `9`
- **Category**: Annular Transmission
- **Kinematics**: External Pinion $Z_1 = 20$ meshing internally with Annular Ring $Z_2 = 60$. Center distance $a = 70.0\text{ mm}$.
- **Ratio**: $i = Z_2 / Z_1 = 60 / 20 = 3.0:1$.
- **Direction**: Identical rotational direction (CW $\to$ CW).
- **Components**: Internal Ring Gear Housing, Pinion Shaft, Off-Center Bearing Carrier.

### 11. Helical Gear Pair
- **Index**: `10`
- **Category**: Smooth Parallel Drive
- **Kinematics**: Helix angles $\beta_1 = +20^\circ$ (RH), $\beta_2 = -20^\circ$ (LH). Normal module $m_n = 3.5\text{ mm}$.
- **Ratio**: $i = Z_2 / Z_1 = 40 / 20 = 2.0:1$.
- **Axial Thrust**: Generates lateral thrust $F_a = F_t \tan\beta$.
- **Components**: Right-Hand Helical Pinion, Left-Hand Helical Gear, Thrust-Bushed Bearings.

### 12. Herringbone Gear Pair (Double-Helical)
- **Index**: `11`
- **Category**: Double-Helical Drive
- **Kinematics**: Opposed chevron helices ($+20^\circ$ and $-20^\circ$).
- **Ratio**: $i = Z_2 / Z_1 = 40 / 20 = 2.0:1$.
- **Special Feature**: Complete cancellation of axial thrust forces ($F_{a1} + F_{a2} = 0\text{ N}$).
- **Components**: Double-Helical Driver Pinion, Spoked Double-Helical Gear, Machine Base.

### 13. Automotive Differential
- **Index**: `12`
- **Category**: Torque-Vectoring Axle
- **Kinematics**: Hypoid Final Drive ($12T \to 36T$, $3:1$), Differential Carrier, Dual Spider Gears ($14T$), Left & Right Sun Gears ($18T$).
- **Willis Law**: $\omega_{carrier} = \frac{\omega_{Left} + \omega_{Right}}{2}$.
- **Interactive Control**: Real-time cornering turn bias slider $[-1.0, +1.0]$.
- **Components**: Bevel Drive Pinion, Hypoid Ring Gear, Carrier Housing, 2x Spider Gears, Left/Right Axle Shafts.

### 14. Harmonic Drive (Strain Wave)
- **Index**: `13`
- **Category**: Strain Wave Robotics
- **Kinematics**: Elliptical Wave Generator, Thin-walled Flexible Flexspline ($Z_f = 48$), Rigid Circular Spline ($Z_c = 50$).
- **Ratio**: $i = \frac{Z_f}{Z_c - Z_f} = \frac{48}{50 - 48} = 24.0:1$.
- **Special Feature**: Zero backlash, high positional stiffness, counter-rotating output.
- **Components**: Elliptical Wave Generator Cam, Flexible Cup Flexspline, Internal Ring Circular Spline.

---

## Category 2: Engines and Reciprocating Mechanisms (4 Mechanisms)

### 15. Four-Stroke Internal Combustion Engine
- **Index**: `14`
- **Category**: Engines & Reciprocating
- **Kinematics**: Complete 720° kinematic Otto cycle (2 full crankshaft revolutions per cycle).
- **Subsystems**:
  - Piston ($r = 30\text{ mm}$, dual compression rings, skirt relief)
  - Forged I-beam connecting rod ($l = 100\text{ mm}$)
  - Crankshaft ($r = 30\text{ mm}$, stroke $S = 60\text{ mm}$)
  - 1/2-speed Overhead Camshaft ($N_{cam} = N_{crank} / 2$)
  - Poppet Intake & Exhaust Valves with harmonic valve lifts
  - Transparent cylinder block revealing real-time piston strokes
  - Spark plug representation flashing during Power stroke ($360^\circ$)
- **Cycle Stages**:
  1. Intake ($0^\circ - 180^\circ$): Piston descends from TDC to BDC; intake valve opens ($10^\circ - 170^\circ$).
  2. Compression ($180^\circ - 360^\circ$): Piston ascends; both valves closed; air-fuel charge compressed.
  3. Power ($360^\circ - 540^\circ$): Spark fires at TDC ($360^\circ$); combustion expansion drives piston downward.
  4. Exhaust ($540^\circ - 720^\circ$): Piston ascends; exhaust valve opens ($550^\circ - 710^\circ$), purging spent gas.
- **Components**: Cylinder Block, Piston & Rings, Connecting Rod, Counterweighted Crankshaft, Flywheel, Camshaft, Intake Valve, Exhaust Valve, Spark Plug.

### 16. Slider-Crank Mechanism
- **Index**: `15`
- **Category**: Engines & Reciprocating
- **Kinematics**: Exact slider displacement:
  $$x(\theta) = r\cos\theta + \sqrt{l^2 - r^2\sin^2\theta}$$
  Velocity:
  $$v(\theta) = -r\omega \left(\sin\theta + \frac{r\sin 2\theta}{2\sqrt{l^2 - r^2\sin^2\theta}}\right)$$
- **Geometry**: Crank radius $r = 40\text{ mm}$, rod length $l = 120\text{ mm}$ ($\lambda = r/l = 0.333$). Total stroke $S = 80\text{ mm}$.
- **Components**: Crank Disk & Pin, Forged I-Beam Connecting Rod, Crosshead Slider, Dual Ground Guide Rails, Machine Bedplate.

### 17. Cam and Follower
- **Index**: `16`
- **Category**: Engines & Reciprocating
- **Kinematics**: Base radius $R_b = 35\text{ mm}$, Maximum lift $h = 18\text{ mm}$.
  - Simple Harmonic Motion profile: Dwell ($0^\circ - 60^\circ$), Harmonic Rise ($60^\circ - 180^\circ$), Peak Dwell ($180^\circ - 220^\circ$), Harmonic Return ($220^\circ - 320^\circ$), Base Dwell ($320^\circ - 360^\circ$).
- **Components**: Machined Plate Cam, Hardened Roller Follower, Vertical Guide Sleeve, Helical Return Spring, Camshaft.

### 18. Reciprocating Pump
- **Index**: `17`
- **Category**: Engines & Reciprocating
- **Kinematics**: Positive-displacement fluid plunger. Bore diameter $d = 60\text{ mm}$, Crank radius $r = 30\text{ mm}$, Stroke $S = 60\text{ mm}$.
- **Discharge**:
  $$Q = \frac{\pi d^2}{4} \cdot S \cdot \frac{N}{60} \text{ m}^3\text{/s} \quad (\approx 20.36\text{ L/min at 120 RPM})$$
- **Valves**: Automatic spring-loaded suction check valve and delivery check valve.
- **Components**: Cast Pump Manifold, Plunger Piston, Crank-Crosshead Drive, Suction Non-Return Valve, Delivery Non-Return Valve, Baseplate.

---

## Category 3: Linkages and Motion Conversion (4 Mechanisms)

### 19. Four-Bar Linkage
- **Index**: `18`
- **Category**: Linkages & Motion Conversion
- **Kinematics**: Planar closed-loop 1-DOF kinematic chain.
  - Fixed ground link $d = 120\text{ mm}$
  - Input crank link $a = 35\text{ mm}$ (Shortest link $s$)
  - Coupler link $b = 110\text{ mm}$
  - Output rocker link $c = 80\text{ mm}$ (Longest link $l = d = 120\text{ mm}$)
- **Grashof Theorem**: $s + l \le p + q \implies 35 + 120 = 155 \le 80 + 110 = 190$ (Grashof Class I satisfied; continuous crank rotation guaranteed).
- **Components**: Ground Base Bar, Driving Crank Bar, Floating Coupler Bar, Rocker Bar, 4x Precision Pivot Pins.

### 20. Crank-Rocker Mechanism
- **Index**: `19`
- **Category**: Linkages & Motion Conversion
- **Kinematics**: Converts continuous 360° crank rotation into bounded rocker angular oscillation ($48.2^\circ$ sweep).
- **Quick-Return Action**: Return stroke occurs faster than advance stroke ($Q = 1.16$).
- **Components**: Input Crank Disk, Connecting Coupler Rod, Oscillating Rocker Arm, Dual Pivot Bearing Blocks.

### 21. Geneva Mechanism (Maltese Cross)
- **Index**: `20`
- **Category**: Linkages & Motion Conversion
- **Kinematics**: 4-slot Geneva wheel with raised locking crescent.
- **Intermittent Motion**:
  - Dwell Angle: $270^\circ$ ($75\%$ of cycle stationary lock).
  - Index Angle: $90^\circ$ ($25\%$ active advance).
  - Average Ratio: $4:1$ reduction.
- **Components**: Geneva Wheel (4 radial slots), Continuous Driver Plate, Hardened Drive Pin, Concave Locking Crescent, Dual Shafts.

### 22. Scotch Yoke Mechanism
- **Index**: `21`
- **Category**: Linkages & Motion Conversion
- **Kinematics**: Translates rotary motion into mathematically exact Simple Harmonic Motion (SHM):
  $$x(t) = r\cos(\omega t), \quad v(t) = -r\omega\sin(\omega t), \quad a(t) = -r\omega^2\cos(\omega t)$$
- **Special Feature**: Zero connecting rod angularity distortion (pure fundamental frequency without higher harmonics). Crank $r = 40\text{ mm}$, Stroke $S = 80\text{ mm}$.
- **Components**: Crank Disk & Drive Pin, Rectangular Slotted Yoke Frame, Floating Bronze Slide Block, Dual Linear Guide Rods.

---

## Category 4: Power Transmission and Mechanical Systems (4 Mechanisms)

### 23. Universal Joint (Cardan / Hooke's Joint)
- **Index**: `22`
- **Category**: Power Transmission & Drives
- **Kinematics**: Intersecting shafts at operating angle $\beta = 20^\circ$.
  $$\tan\theta_2 = \tan\theta_1 \cdot \cos\beta$$
  $$\frac{\omega_2}{\omega_1} = \frac{\cos\beta}{1 - \sin^2\beta\cos^2\theta_1}$$
- **Velocity Ripple**: Instantaneous speed oscillates twice per revolution ($\pm 6.2\%$ at $\beta = 20^\circ$).
- **Components**: Driving Fork Yoke, Driven Fork Yoke, 4-Armed Cross Spider Trunnion, Input & Angled Output Shafts, Pillow Blocks.

### 24. Roller Chain and Sprocket Drive
- **Index**: `23`
- **Category**: Power Transmission & Drives
- **Kinematics**: ANSI #40 / ISO 08B chain ($p = 12.7\text{ mm}$). Driver sprocket $Z_1 = 15$, Driven sprocket $Z_2 = 30$ ($2.0:1$ reduction). Center distance $C = 160\text{ mm}$.
- **Special Feature**: Positive synchronous power transfer without slip; analysis of chordal action (polygonal effect $\Delta v / v = 2.19\%$).
- **Components**: 15T Driver Sprocket, 30T Driven Sprocket, Continuous Linked Roller Chain Loop, Pillow Blocks, Bedplate.

### 25. Ratchet and Pawl Indexing Mechanism
- **Index**: `24`
- **Category**: Power Transmission & Drives
- **Kinematics**: Converts oscillating rocker arm input into strict unidirectional step-wise rotation.
  - Asymmetric sawtooth wheel ($Z = 24$ teeth, pitch step $\Delta\theta = 15.0^\circ$).
  - Overall reduction: $24.0:1$.
- **Special Feature**: Spring-loaded holding detent pawl locks against tooth shoulder, preventing backward rotation during return stroke.
- **Components**: 24-Tooth Sawtooth Wheel, Driving Rocker Arm, Spring-Loaded Driving Pawl, Stationary Holding Detent Pawl, Main Shaft.

### 26. Lead Screw and Traveling Nut
- **Index**: `25`
- **Category**: Power Transmission & Drives
- **Kinematics**: Rotary-to-linear actuator with massive force multiplication.
  - Nominal diameter $d = 24\text{ mm}$, single-start Acme thread lead $L = 6.0\text{ mm/rev}$.
  - Axial thrust per unit torque:
    $$F_{thrust} = \frac{2\pi \eta T_{in}}{L} \approx 995 \text{ N / (N}\cdot\text{m)}$$
    At $10\text{ N}\cdot\text{m}$ input torque, generates $9,948\text{ N}$ (~$1,014\text{ kgf}$) of axial force.
- **Special Feature**: Inherent self-locking (lead angle $\lambda = 4.8^\circ <$ friction angle $\phi = 8.5^\circ$); cannot be backdriven by axial load.
- **Components**: Precision Acme Lead Screw, Phosphor Bronze Flanged Nut Carriage, Dual Hardened Linear Guide Shafts, Fixed Thrust Bearing Mount, Floating Radial Mount.

---

## Technical Summary Matrix

| Index | Mechanism Name | Category | Ratio | Direction | Key Physical Dimension |
|:---:|:---|:---|:---:|:---:|:---|
| 00 | Spur Gear Pair | Gears | 2.0:1 | Counter | $m = 3.5\text{ mm}, a = 105\text{ mm}$ |
| 01 | Bevel Gear Pair | Gears | 2.0:1 | 90° Orthogonal | $\Sigma = 90^\circ, \delta_1 = 26.57^\circ$ |
| 02 | Worm & Wheel | Gears | 30.0:1 | 90° Skew | $\gamma = 7.39^\circ < \rho' = 8.53^\circ$ |
| 03 | Planetary System | Gears | 4.0:1 | Coaxial | $Z_s=18, Z_p=18, Z_r=54$ |
| 04 | Rack and Pinion | Gears | 20T | Linear Conv. | Pitch $p = 11.0\text{ mm}$ |
| 05 | Belt & Pulley | Gears | 2.0:1 | Same | $D_1=60, D_2=120, C=150\text{ mm}$ |
| 06 | Simple Gear Train | Gears | 2.0:1 | Same | $Z_1=20, Z_{id}=28, Z_3=40$ |
| 07 | Compound Gear Train | Gears | 6.0:1 | Counter | $i = 2.0 \times 3.0 = 6.0$ |
| 08 | Reverted Gear Train | Gears | 3.0:1 | Same | $a_1 = a_2 = 105\text{ mm}$, Coaxial |
| 09 | Internal Gear Pair | Gears | 3.0:1 | Same | $Z_1=20, Z_2=60, a=70\text{ mm}$ |
| 10 | Helical Gear Pair | Gears | 2.0:1 | Counter | $\beta = \pm 20^\circ, F_a \ne 0$ |
| 11 | Herringbone Gear Pair | Gears | 2.0:1 | Counter | $\beta = \pm 20^\circ, \Sigma F_a = 0$ |
| 12 | Automotive Differential | Gears | 3.0:1 | Vectoring | Willis: $\omega_c = (\omega_L+\omega_R)/2$ |
| 13 | Harmonic Drive | Gears | 24.0:1 | Counter | $\Delta Z = 2, i = 48/2 = 24.0$ |
| 14 | Four-Stroke IC Engine | Engines | 2.0:1 | Reciprocating | 720° Cycle, $S = 60\text{ mm}$ |
| 15 | Slider-Crank | Engines | 1.0:1 | Reciprocating | $r=40\text{ mm}, l=120\text{ mm}, S=80\text{ mm}$ |
| 16 | Cam and Follower | Engines | 1.0:1 | Reciprocating | $R_b=35\text{ mm}, h=18\text{ mm}$ Lift |
| 17 | Reciprocating Pump | Engines | 1.0:1 | Flow Rate | $d=60\text{ mm}, S=60\text{ mm}, Q=20.4\text{ L/m}$ |
| 18 | Four-Bar Linkage | Linkages | 1.0:1 | Oscillation | Grashof I: $155 \le 190\text{ mm}$ |
| 19 | Crank-Rocker | Linkages | 1.0:1 | Oscillation | Sweep $= 48.2^\circ, Q = 1.16$ |
| 20 | Geneva Mechanism | Linkages | 4.0:1 | Intermittent | 4 Slots, 75% Dwell, 25% Index |
| 21 | Scotch Yoke | Linkages | 1.0:1 | Pure SHM | $r=40\text{ mm}, S=80\text{ mm}$ |
| 22 | Universal Joint | Drives | 1.0:1 | Fluctuating | $\beta = 20^\circ$, Ripple $\pm 6.2\%$ |
| 23 | Chain & Sprocket | Drives | 2.0:1 | Same | $15T \to 30T$, Chordal $= 2.19\%$ |
| 24 | Ratchet & Pawl | Drives | 24.0:1 | One-Way Step | $Z=24$, Step $= 15^\circ$ |
| 25 | Lead Screw & Nut | Drives | 1.0:1 | Rotary-Linear | $d=24, L=6\text{ mm}$, Thrust $9.9\text{ kN}$ |

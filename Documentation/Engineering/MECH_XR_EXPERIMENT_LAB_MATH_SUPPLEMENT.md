# MECH-XR — EXPERIMENT LAB & ENGINEERING MATHEMATICS
## Complete Technical Supplement to the Viva Handbook

**Project:** MECH-XR  
**Module:** Scientific Experiment Lab  
**Engine:** Unity 6.3 LTS (`6000.3.27f2`)  
**Language:** C# (.NET Standard 2.1)  
**Target Hardware:** Xiaomi POCO X6 5G (`23122PCD1I`)  
**Purpose:** Understand every experiment control, mathematical equation, calculated output, graph, preset, and mechanism-specific telemetry feature.

---

# 1. UNDERSTANDING THE EXPERIMENT LAB

## 1.1 What exactly is the Experiment Lab?

The Experiment Lab is the analytical section of MECH-XR.

The mechanism library lets students select and visualize mechanical systems. The Experiment Lab lets them change operating conditions and observe how those changes affect the calculated mechanical outputs.

For example, imagine a 20-tooth gear driving a 40-tooth gear.

If we supply 120 RPM at the input:
- The input gear rotates at 120 RPM.
- The output gear rotates at 60 RPM.
- The output gear rotates in the opposite direction.
- The output torque increases because the system reduces speed.
- Some input power is lost according to the selected efficiency.

The Experiment Lab makes these relationships visible through controls, numerical telemetry, and graphs.

It is not just a settings panel. It is an interactive mathematical model of mechanical transmission.

## 1.2 How do we access it?

The workflow is:
1. Open MECH-XR.
2. Select a mechanism from the 26-mechanism library.
3. Scan a suitable flat surface.
4. Wait for the placement reticle.
5. Tap the surface to place the mechanism.
6. Open the `EXPERIMENT LAB` control from the AR HUD.
7. Adjust the operating parameters.
8. Observe the calculated values and graphs.

The Experiment Lab appears as an ergonomic collapsible drawer.

## 1.3 What parameters can we control?

| Parameter | Documented range | Meaning |
|---|---:|---|
| **Input RPM** | 10–300 RPM | Speed of the driving shaft |
| **Input torque** | 0.1–500 N·m | Applied input rotational torque |
| **Efficiency** | 50–100% | Assumed transmission efficiency |
| **Direction** | Clockwise / Counter-clockwise | Reverses the input rotation |
| **Simulation state** | Run / Pause | Starts or freezes kinematic motion |
| **Model scale** | 0.4×–2.5× | Changes the displayed assembly size |

The RPM control supports:
- Finger dragging across the slider track.
- Tapping anywhere along the slider track.
- Decreasing speed by 10 RPM (`-10` button).
- Increasing speed by 10 RPM (`+10` button).

The interface displays RPM and angular velocity simultaneously.

---

# 2. THE MOST IMPORTANT ENGINEERING CONCEPT: TORQUE

## 2.1 What is torque?

Torque is the turning effect of a force.

Imagine using a spanner to tighten a nut:
- If you apply force near the nut, it is difficult to turn.
- If you apply the same force at the end of a longer spanner, it becomes easier.

This is because the distance from the rotation axis is greater.

The formula is:
\[
T = F \times r
\]

Where:
- \(T\) = torque in newton-metres (\(\text{N}\cdot\text{m}\)).
- \(F\) = applied force in newtons (\(\text{N}\)).
- \(r\) = perpendicular distance from the axis in metres (\(\text{m}\)).

Example:
You apply \(10\text{ N}\) of force at a distance of \(0.5\text{ m}\).
\[
T = 10 \times 0.5 = \mathbf{5\ N\cdot m}
\]
This means the applied force produces \(5\text{ N}\cdot\text{m}\) of torque.

## 2.2 Why does torque increase in a gearbox?

Consider two gears:
- Input gear: 20 teeth.
- Output gear: 40 teeth.

The output gear has twice as many teeth. Therefore, it rotates at half the input speed.

However, in an ideal transmission, it produces approximately twice the input torque. This is the fundamental speed-torque trade-off:
\[
\text{Lower speed} \implies \text{Higher torque}
\]
\[
\text{Higher speed} \implies \text{Lower torque}
\]

A gearbox does not create additional energy. It changes the relationship between rotational speed and torque.

## 2.3 What is gear ratio?

We define the reduction ratio as:
\[
i = \frac{N_{\text{in}}}{N_{\text{out}}} = \frac{Z_{\text{out}}}{Z_{\text{in}}}
\]

Where:
- \(N_{\text{in}}\) = input RPM.
- \(N_{\text{out}}\) = output RPM.
- \(Z_{\text{in}}\) = input gear tooth count.
- \(Z_{\text{out}}\) = output gear tooth count.

Example:
\[
Z_{\text{in}} = 20, \quad Z_{\text{out}} = 40 \implies i = \frac{40}{20} = \mathbf{2 : 1}
\]
The input must rotate twice for the output to rotate once.

## 2.4 Ideal output torque

For an ideal, lossless gearbox:
\[
T_{\text{out}} = T_{\text{in}} \times i
\]

Suppose \(T_{\text{in}} = 10\text{ N}\cdot\text{m}\) and \(i = 2\):
\[
T_{\text{out}} = 10 \times 2 = \mathbf{20\ N\cdot m}
\]
The gearbox doubles torque while halving rotational speed.

## 2.5 Real output torque

Real gearboxes are not perfectly efficient. Some energy is lost through friction, deformation, vibration, and other effects.

We introduce efficiency:
\[
\eta = \frac{P_{\text{out}}}{P_{\text{in}}}
\]

The output torque model used in MECH-XR is:
\[
\mathbf{T_{\text{out}} = \eta \cdot T_{\text{in}} \cdot i}
\]

Suppose:
- Input torque = \(10\text{ N}\cdot\text{m}\).
- Gear ratio = \(2\).
- Efficiency = \(90\%\) (\(\eta = 0.90\)).

Calculate:
\[
T_{\text{out}} = 0.90 \times 10 \times 2 = \mathbf{18\ N\cdot m}
\]
The ideal output was \(20\text{ N}\cdot\text{m}\). The modeled real output is \(18\text{ N}\cdot\text{m}\). The difference represents the effect of the assumed transmission loss.

**Viva answer:** We multiply by efficiency because the real transmission cannot deliver all the input power as useful output power.

---

# 3. ANGULAR VELOCITY

## 3.1 What is angular velocity?

Angular velocity tells us how quickly something rotates.

It is represented by:
\[
\omega
\]
Its SI unit is:
\[
\text{rad/s}
\]

RPM is revolutions per minute, whereas angular velocity is measured in radians per second. MECH-XR converts RPM into angular velocity before using it in the power equation.

## 3.2 Conversion formula

One complete revolution equals \(360^\circ = 2\pi\text{ radians}\). One minute contains 60 seconds.

Therefore:
\[
\mathbf{\omega = \frac{2\pi N}{60}}
\]

Where:
- \(N\) = rotational speed in RPM.
- \(\omega\) = angular velocity in rad/s.

## 3.3 Example: 120 RPM
\[
\omega = \frac{2\pi \times 120}{60} = 4\pi \approx \mathbf{12.566\ rad/s}
\]

## 3.4 Useful conversion reference table

| RPM | Exact expression | Angular velocity |
|---:|:---:|---:|
| 10 | \(\frac{\pi}{3}\) | 1.047 rad/s |
| 45 | \(\frac{3\pi}{2}\) | 4.712 rad/s |
| 60 | \(2\pi\) | 6.283 rad/s |
| 120 | \(4\pi\) | 12.566 rad/s |
| 150 | \(5\pi\) | 15.708 rad/s |
| 240 | \(8\pi\) | 25.133 rad/s |
| 300 | \(10\pi\) | 31.416 rad/s |

## 3.5 Output angular velocity

For a reduction ratio \(i\):
\[
\omega_{\text{out}} = \frac{\omega_{\text{in}}}{i}
\]

Example:
Input \(\omega_{\text{in}} = 12.566\text{ rad/s}\), Ratio \(i = 2\):
\[
\omega_{\text{out}} = \frac{12.566}{2} = \mathbf{6.283\ rad/s} \quad (\text{equivalent to } 60\text{ RPM})
\]

---

# 4. MECHANICAL POWER

## 4.1 What is power?

Power is the rate at which work is performed or energy is transferred.

In a rotating mechanical shaft:
\[
\mathbf{P = T \cdot \omega}
\]

Where:
- \(P\) = mechanical power in watts (\(\text{W}\)).
- \(T\) = torque in \(\text{N}\cdot\text{m}\).
- \(\omega\) = angular velocity in \(\text{rad/s}\).

## 4.2 Why do we multiply torque and angular velocity?

Torque tells us how much turning effect is applied. Angular velocity tells us how quickly the object rotates. Together, their product represents the rate at which rotational mechanical work is done per second.

## 4.3 Example
Suppose \(T = 10\text{ N}\cdot\text{m}\) and \(N = 120\text{ RPM}\):
1. Calculate angular velocity:
   \[
   \omega = \frac{2\pi \times 120}{60} = 12.566\text{ rad/s}
   \]
2. Calculate power:
   \[
   P = 10 \times 12.566 = \mathbf{125.66\ W} \quad (0.12566\text{ kW})
   \]

## 4.4 Important shortcut formula

Substitute the RPM conversion directly into the power equation:
\[
\mathbf{P = \frac{2\pi N T}{60}}
\]

---

# 5. EFFICIENCY AND POWER LOSS

## 5.1 What is efficiency?

Efficiency measures how much of the input power becomes useful output power:
\[
\mathbf{\eta = \frac{P_{\text{out}}}{P_{\text{in}}}} \implies \mathbf{\eta(\%) = \frac{P_{\text{out}}}{P_{\text{in}}} \times 100}
\]

## 5.2 Output power
\[
\mathbf{P_{\text{out}} = \eta \cdot P_{\text{in}}}
\]

Example:
\[
P_{\text{in}} = 125.66\text{ W}, \quad \eta = 0.90 \implies P_{\text{out}} = 0.90 \times 125.66 = \mathbf{113.10\ W}
\]

## 5.3 Power loss

The difference between input and useful output power is the modeled power loss:
\[
\mathbf{P_{\text{loss}} = P_{\text{in}} - P_{\text{out}} = (1 - \eta) \cdot P_{\text{in}}}
\]

Example:
\[
P_{\text{loss}} = 125.66 - 113.10 = \mathbf{12.57\ W}
\]

This loss is displayed in real-time inside the Experiment Lab's **Power/Loss graph**. In a physical machine, this power is dissipated primarily as heat via gear tooth friction and bearing drag.

## 5.4 Why does lower efficiency increase power loss?

| Efficiency (\(\eta\)) | Input power (\(P_{\text{in}}\)) | Output power (\(P_{\text{out}}\)) | Power loss (\(P_{\text{loss}}\)) |
|---:|---:|---:|---:|
| 98% | 100 W | 98 W | 2 W |
| 90% | 100 W | 90 W | 10 W |
| 75% | 100 W | 75 W | 25 W |

Lowering efficiency directly magnifies parasitic thermal dissipation.

---

# 6. ALL FOUR EXPERIMENT LAB GRAPHS

The Experiment Lab displays four real-time graphs rendered via procedural UI curves:

### 6.1 RPM Graph
- **Function:** Compares input shaft speed against output shaft speed.
- **Governing Law:** \(N_{\text{out}} = \frac{N_{\text{in}}}{i}\).
- **Physical Meaning:** Demonstrates velocity step-down or step-up across the gear ratio.

### 6.2 Torque Graph
- **Function:** Compares input torque with efficiency-corrected output torque.
- **Governing Law:** \(T_{\text{out}} = \eta \cdot T_{\text{in}} \cdot i\).
- **Physical Meaning:** Visualizes torque multiplication as a trade-off for reduced output speed.

### 6.3 Speed/Time Graph
- **Function:** Tracks angular velocity changes across continuous time during acceleration, steady state, and braking.
- **Governing Law:** Angular acceleration \(\alpha = \frac{d\omega}{dt}\).
- **Physical Meaning:** Visualizes rotational inertia and dynamic acceleration profiles.

### 6.4 Power/Loss Graph
- **Function:** Traces three distinct energy channels using high-contrast color coding:
  - **Sky Blue:** Input mechanical power (\(P_{\text{in}}\)).
  - **Emerald Green:** Useful output power (\(P_{\text{out}}\)).
  - **Crimson Red:** Dissipated power loss (\(P_{\text{loss}}\)).
- **Governing Law:** \(P_{\text{in}} = P_{\text{out}} + P_{\text{loss}}\).
- **Physical Meaning:** Direct visual representation of the First Law of Thermodynamics (conservation of energy) in rotational systems.

---

# 7. EXPERIMENT PRESETS — ACTUAL NUMERICAL RESULTS

In `UIManager.cs` (lines 1143–1146), four presets configure input RPM, input torque, and efficiency with one tap:

## 7.1 High Speed Preset
- **Inputs:** \(N = 240\text{ RPM}\), \(T_{\text{in}} = 5.0\text{ N}\cdot\text{m}\), \(\eta = 0.96\) (96%)
- **Angular Velocity:** \(\omega = \frac{2\pi \times 240}{60} = 25.133\text{ rad/s}\)
- **Input Power:** \(P_{\text{in}} = 5.0 \times 25.133 = \mathbf{125.66\ W}\)
- **Output Power:** \(P_{\text{out}} = 0.96 \times 125.66 = \mathbf{120.64\ W}\)
- **Power Loss:** \(P_{\text{loss}} = 125.66 - 120.64 = \mathbf{5.03\ W}\)
- **Output Torque (for 2:1 reduction):** \(T_{\text{out}} = 0.96 \times 5.0 \times 2 = \mathbf{9.60\ N\cdot m}\)
- **Output Speed:** \(N_{\text{out}} = \frac{240}{2} = \mathbf{120\ RPM}\)

## 7.2 High Torque Preset
- **Inputs:** \(N = 60\text{ RPM}\), \(T_{\text{in}} = 50.0\text{ N}\cdot\text{m}\), \(\eta = 0.94\) (94%)
- **Angular Velocity:** \(\omega = \frac{2\pi \times 60}{60} = 6.283\text{ rad/s}\)
- **Input Power:** \(P_{\text{in}} = 50.0 \times 6.283 = \mathbf{314.16\ W}\)
- **Output Power:** \(P_{\text{out}} = 0.94 \times 314.16 = \mathbf{295.31\ W}\)
- **Power Loss:** \(P_{\text{loss}} = 314.16 - 295.31 = \mathbf{18.85\ W}\)
- **Output Torque (for 2:1 reduction):** \(T_{\text{out}} = 0.94 \times 50.0 \times 2 = \mathbf{94.00\ N\cdot m}\)
- **Output Speed:** \(N_{\text{out}} = \frac{60}{2} = \mathbf{30\ RPM}\)

## 7.3 Industrial Preset
- **Inputs:** \(N = 150\text{ RPM}\), \(T_{\text{in}} = 30.0\text{ N}\cdot\text{m}\), \(\eta = 0.92\) (92%)
- **Angular Velocity:** \(\omega = \frac{2\pi \times 150}{60} = 15.708\text{ rad/s}\)
- **Input Power:** \(P_{\text{in}} = 30.0 \times 15.708 = \mathbf{471.24\ W}\)
- **Output Power:** \(P_{\text{out}} = 0.92 \times 471.24 = \mathbf{433.54\ W}\)
- **Power Loss:** \(P_{\text{loss}} = 471.24 - 433.54 = \mathbf{37.70\ W}\)
- **Output Torque (for 2:1 reduction):** \(T_{\text{out}} = 0.92 \times 30.0 \times 2 = \mathbf{55.20\ N\cdot m}\)
- **Output Speed:** \(N_{\text{out}} = \frac{150}{2} = \mathbf{75\ RPM}\)

## 7.4 Low Efficiency Preset (Worn / Misaligned)
- **Inputs:** \(N = 120\text{ RPM}\), \(T_{\text{in}} = 20.0\text{ N}\cdot\text{m}\), \(\eta = 0.75\) (75%)
- **Angular Velocity:** \(\omega = \frac{2\pi \times 120}{60} = 12.566\text{ rad/s}\)
- **Input Power:** \(P_{\text{in}} = 20.0 \times 12.566 = \mathbf{251.33\ W}\)
- **Output Power:** \(P_{\text{out}} = 0.75 \times 251.33 = \mathbf{188.50\ W}\)
- **Power Loss:** \(P_{\text{loss}} = 251.33 - 188.50 = \mathbf{62.83\ W}\)
- **Output Torque (for 2:1 reduction):** \(T_{\text{out}} = 0.75 \times 20.0 \times 2 = \mathbf{30.00\ N\cdot m}\)
- **Output Speed:** \(N_{\text{out}} = \frac{120}{2} = \mathbf{60\ RPM}\)

### Preset Summary Table

| Preset | RPM | Input torque | Efficiency | Input power | Output power | Power loss |
|---|---:|---:|---:|---:|---:|---:|
| **High Speed** | 240 | 5.0 N·m | 96% | 125.66 W | 120.64 W | 5.03 W |
| **High Torque** | 60 | 50.0 N·m | 94% | 314.16 W | 295.31 W | 18.85 W |
| **Industrial** | 150 | 30.0 N·m | 92% | 471.24 W | 433.54 W | 37.70 W |
| **Low Efficiency** | 120 | 20.0 N·m | 75% | 251.33 W | 188.50 W | 62.83 W |

---

# 8. A/B EXPERIMENT COMPARISON WORKFLOW

The A/B comparison system in MECH-XR enables direct comparative study:
1. **Capture Baseline:** Tap `CAPTURE EXPERIMENT A` to lock in current operating parameters and telemetry.
2. **Modulate Variables:** Tweak RPM, torque, or efficiency slider controls (e.g. simulate gear degradation by lowering efficiency from 95% to 80%).
3. **Live Delta Calculation:** View real-time difference indicators:
   \[
   \Delta X = X_B - X_A
   \]

### Detailed Comparative Example (2:1 Reduction Gearbox)

| Telemetry Parameter | Experiment A (Baseline) | Experiment B (Modulated) | Absolute Delta (\(\Delta\)) | Physical Meaning |
|---|---:|---:|---:|---|
| **Input RPM** | \(120\text{ RPM}\) | \(240\text{ RPM}\) | \(+120\text{ RPM}\) | Shaft rotational speed doubled |
| **Input Torque** | \(10.0\text{ N}\cdot\text{m}\) | \(10.0\text{ N}\cdot\text{m}\) | \(0.0\text{ N}\cdot\text{m}\) | Input torque kept constant |
| **Efficiency (\(\eta\))** | \(95\%\) (\(0.95\)) | \(80\%\) (\(0.80\)) | \(-15\%\) | Severe simulated mesh wear |
| **Input Power (\(P_{\text{in}}\))** | \(125.66\text{ W}\) | \(251.33\text{ W}\) | \(+125.67\text{ W}\) | Doubled due to speed increase |
| **Output Power (\(P_{\text{out}}\))** | \(119.38\text{ W}\) | \(201.06\text{ W}\) | \(+81.68\text{ W}\) | Useful power delivered increased |
| **Power Loss (\(P_{\text{loss}}\))** | \(6.28\text{ W}\) | \(50.27\text{ W}\) | \(+43.99\text{ W}\) | **700% increase in thermal dissipation** |
| **Output Torque (\(T_{\text{out}}\))** | \(19.00\text{ N}\cdot\text{m}\) | \(16.00\text{ N}\cdot\text{m}\) | \(-3.00\text{ N}\cdot\text{m}\) | Torque dropped due to lower efficiency |
| **Output RPM** | \(60\text{ RPM}\) | \(120\text{ RPM}\) | \(+60\text{ RPM}\) | Output speed doubled proportionally |

**Viva Insight:** This demonstrates that even if a machine runs faster and transmits more gross power, lower transmission efficiency significantly degrades output torque and creates massive thermal losses.

---

# 9. SPECIALIZED MECHANISM TELEMETRY & KINEMATIC FORMULAS

Beyond standard gear ratios, MECH-XR provides specialized mathematical telemetry for complex mechanisms:

### 9.1 Automotive Differential (`AutomotiveDifferential.cs`)
- **Turn Bias Slider:** Modulates left vs. right wheel speed distribution dynamically from \(-1.0\) (full left turn) to \(+1.0\) (full right turn).
- **Willis Axle Law:**
  \[
  \mathbf{\omega_{\text{carrier}} = \frac{\omega_{\text{left}} + \omega_{\text{right}}}{2}}
  \]
- **Verification Example:** At \(N_{\text{carrier}} = 40\text{ RPM}\):
  - Straight line: \(N_L = 40\text{ RPM}, N_R = 40\text{ RPM} \implies \frac{40+40}{2} = 40\text{ RPM}\)
  - Left turn: \(N_L = 20\text{ RPM}, N_R = 60\text{ RPM} \implies \frac{20+60}{2} = 40\text{ RPM}\)
  - Right turn: \(N_L = 60\text{ RPM}, N_R = 20\text{ RPM} \implies \frac{60+20}{2} = 40\text{ RPM}\)

### 9.2 Four-Stroke IC Engine (`FourStrokeEngine.cs`)
- **\(720^\circ\) Crankshaft Cycle:** Two complete revolutions (\(720^\circ\)) per power stroke.
- **Camshaft Timing Law:** Camshaft rotates at exactly half crankshaft speed:
  \[
  \mathbf{N_{\text{cam}} = \frac{N_{\text{crank}}}{2}} \quad (\text{e.g. } 120\text{ RPM crank} \implies 60\text{ RPM cam})
  \]
- **Telemetry Displayed:** Current stroke (Intake, Compression, Power, Exhaust), Crank angle \(\theta \in [0^\circ, 720^\circ]\), Intake/Exhaust valve state (Open/Closed), Piston displacement from TDC (\(s\)), and Instantaneous linear velocity (\(v\)).

### 9.3 Slider-Crank Mechanism (`SliderCrankMechanism.cs`)
- **Dimensions:** Crank radius \(r = 40\text{ mm}\), Connecting rod length \(l = 120\text{ mm}\), Stroke \(S = 2r = 80\text{ mm}\).
- **Rod-to-Crank Obliquity Ratio:** \(\lambda = \frac{r}{l} = \frac{40}{120} = 0.333\).
- **Exact Piston Displacement:**
  \[
  \mathbf{x(\theta) = r\cos\theta + \sqrt{l^2 - r^2\sin^2\theta}}
  \]
- **Velocity & Secondary Harmonic Acceleration:** Demonstrates how rod angularity causes peak acceleration at Top Dead Center (TDC) to exceed acceleration at Bottom Dead Center (BDC).

### 9.4 Cam and Roller Follower (`CamAndFollower.cs`)
- **Dimensions:** Base circle radius \(R_b = 35\text{ mm}\), Maximum stroke lift \(h = 18\text{ mm}\).
- **Kinematic Profile:** Real-time harmonic rise, top dwell, return, and base dwell curves.
- **Telemetry:** Live follower lift \(s(\theta)\) and linear velocity \(v(\theta)\).

### 9.5 Reciprocating Pump (`ReciprocatingPump.cs`)
- **Dimensions:** Plunger diameter \(D = 45\text{ mm}\), Stroke \(S = 60\text{ mm}\) (Crank \(r = 30\text{ mm}\)).
- **Exact Mathematical Discharge Formula:**
  \[
  \mathbf{Q = A \cdot S \cdot \frac{N}{60} \cdot 1000 \cdot 60 = A \cdot S \cdot N \cdot 1000 \quad [\text{L/min}]}
  \]
  Where \(A = \pi \cdot R_{\text{bore}}^2\) and \(S\) are in metres.
- **Valve Check Timing:** Suction valve open during intake (\(15^\circ\)–\(165^\circ\)); Delivery check valve open during discharge stroke (\(195^\circ\)–\(345^\circ\)).

### 9.6 Planar Four-Bar Linkage (`FourBarLinkage.cs`)
- **Dimensions:** Ground \(d = 120\text{ mm}\), Crank \(a = 40\text{ mm}\), Coupler \(b = 100\text{ mm}\), Rocker \(c = 85\text{ mm}\).
- **Grashof Class I Law:** Shortest (\(s = 40\)) + Longest (\(l = 120\)) \(= 160\text{ mm} \le p + q = 85 + 100 = 185\text{ mm}\).
- **Continuity:** The input crank rotates continuously through \(360^\circ\) without jamming.
- **Transmission Angle:** Rocker transmission angle \(\mu\) remains safely bounded between \(40^\circ \le \mu \le 140^\circ\).

### 9.7 Crank-Rocker Mechanism (`CrankRockerMechanism.cs`)
- **Quick-Return Ratio:**
  \[
  \mathbf{Q = \frac{180^\circ + \theta_{\text{advance}}}{180^\circ - \theta_{\text{return}}} = 1.16 : 1}
  \]
  Visualizes asymmetric cycle time utilized in industrial shaping machine tool heads.

### 9.8 Geneva Mechanism (`GenevaMechanism.cs`)
- **4-Slot Maltese Cross:** Indexing step \(\Delta\theta = \frac{360^\circ}{4} = 90^\circ\).
- **Dwell vs. Advance Duty Cycle:**
  - **Active Advance:** \(90^\circ\) of crank pin engagement (\(25\%\) cycle time).
  - **Stationary Dwell:** \(270^\circ\) of locking ring contact (**\(75\%\) cycle time**).
- **Average Velocity Ratio:** \(i_{\text{avg}} = 4.00 : 1\) (\(120\text{ RPM input} \implies 30\text{ RPM average output}\)).

### 9.9 Scotch Yoke Mechanism (`ScotchYokeMechanism.cs`)
- **Pure Simple Harmonic Motion (SHM):**
  \[
  \mathbf{x(t) = r\cos(\omega t), \quad v(t) = -r\omega\sin(\omega t), \quad a(t) = -r\omega^2\cos(\omega t)}
  \]
- **Zero Angularity Distortion:** Contrast with slider-crank; because the crosshead yoke travels in a perpendicular slot, secondary harmonic distortion is mathematically zero.

### 9.10 Universal / Cardan Joint (`UniversalJoint.cs`)
- **Operating Angle:** Configured at \(\beta = 20.0^\circ\).
- **Cardan Kinematic Relationship:**
  \[
  \mathbf{\tan\theta_2 = \tan\theta_1 \cdot \cos\beta}
  \]
- **2nd Harmonic Velocity Ripple:** Output angular velocity oscillates sinusoidally at twice the shaft speed (\(2\omega\)):
  - Peak speed ratio: \(\frac{\omega_2}{\omega_1} = \frac{1}{\cos\beta} = 1.064\times\)
  - Minimum speed ratio: \(\frac{\omega_2}{\omega_1} = \cos\beta = 0.940\times\)
  - Net speed ripple: \(\pm 6.2\%\)

### 9.11 Chain & Sprocket Drive (`ChainAndSprocketDrive.cs`)
- **Sprocket Specifications:** Driver \(Z_1 = 15\), Driven \(Z_2 = 30\), Ratio \(i = 2.00 : 1\).
- **Linear Pitch Velocity:** \(v = \frac{Z_1 \cdot p \cdot N_1}{60} = 0.38\text{ m/s}\) (\(1.37\text{ km/h}\)).
- **Chordal Action (Polygonal Effect):**
  \[
  \frac{\Delta v}{v} = 1 - \cos\left(\frac{180^\circ}{Z_1}\right) = 1 - \cos(12^\circ) = \mathbf{2.19\%}
  \]
  Visualizes slight speed pulsations as chain links seat on sprocket chords.

### 9.12 Ratchet & Pawl Mechanism (`RatchetAndPawl.cs`)
- **Sawtooth Wheel:** \(Z = 24\) asymmetric teeth. Pitch angle \(\Delta\theta = \frac{360^\circ}{24} = 15.0^\circ\).
- **Step-Down Ratio:** \(i = 24 : 1\) (\(120\text{ RPM input} \implies 5.0\text{ RPM indexed output}\)).
- **State Feedback:** Live indication of driving pawl advance (\(0^\circ\)–\(180^\circ\)) vs. holding pawl anti-backlash arrest.

### 9.13 Lead Screw and Nut (`LeadScrewAndNut.cs`)
- **Screw Geometry:** Single-start Acme thread, diameter \(d = 24\text{ mm}\), lead \(L = 6\text{ mm/rev}\).
- **Linear Carriage Velocity:** \(v = \frac{L \cdot N}{60} = 12.0\text{ mm/s}\).
- **Self-Locking Principle:** Because lead angle \(\lambda = 4.55^\circ\) is less than friction angle \(\rho' \approx 8.5^\circ\), the screw cannot be back-driven by axial loads.
- **Axial Clamping Thrust:**
  \[
  \mathbf{F_{\text{axial}} = \frac{2\pi \cdot \eta \cdot T_{\text{in}}}{L}}
  \]
  At \(10\text{ N}\cdot\text{m}\) input torque and \(\eta = 0.95\), generates \(\mathbf{9,948\text{ N}}\) (\(\approx 1,014\text{ kgf}\)) of linear clamping thrust.

---

# 10. RECONCILIATION OF PARAMETERS ACROSS DOCUMENTATION

| Feature / Parameter | Actual C# Code Value | Older Documentation Note | Technical Explanation / Resolution |
|---|---|---|---|
| **High Speed Preset** | \(240\text{ RPM}, 5\text{ N}\cdot\text{m}, 96\%\) | \(300\text{ RPM}, 5\text{ N}\cdot\text{m}, 95\%\) | `UIManager.cs` line 1143 binds `ApplyPreset(240f, 5f, 0.96f)`. |
| **High Torque Preset** | \(60\text{ RPM}, 50\text{ N}\cdot\text{m}, 94\%\) | \(45\text{ RPM}, 25\text{ N}\cdot\text{m}, 88\%\) | `UIManager.cs` line 1144 binds `ApplyPreset(60f, 50f, 0.94f)`. |
| **Industrial Preset** | \(150\text{ RPM}, 30\text{ N}\cdot\text{m}, 92\%\) | \(120\text{ RPM}, 10\text{ N}\cdot\text{m}, 92\%\) | `UIManager.cs` line 1145 binds `ApplyPreset(150f, 30f, 0.92f)`. |
| **Low Efficiency Preset** | \(120\text{ RPM}, 20\text{ N}\cdot\text{m}, 75\%\) | \(120\text{ RPM}, 10\text{ N}\cdot\text{m}, 70\%\) | `UIManager.cs` line 1146 binds `ApplyPreset(120f, 20f, 0.75f)`. |
| **Cam Dimensions** | \(R_b = 35.0\text{ mm}, h = 18.0\text{ mm}\) | \(30\text{ mm} / 16\text{ mm}\) | `CamAndFollower.cs` defines base radius \(35\text{ mm}\), max lift \(18\text{ mm}\). |
| **Pump Dimensions & Flow** | \(D = 60\text{ mm}, S = 60\text{ mm}, Q = 20.36\text{ L/min}\) | \(D = 45\text{ mm}, S = 50\text{ mm}, 9.54\text{ L/min}\) | `ReciprocatingPump.cs` implements bore radius \(30\text{ mm}\) and crank radius \(30\text{ mm}\), integrating \(Q = A \cdot S \cdot N \cdot 1000\). |
| **Universal Joint Angle** | \(\beta = 20.0^\circ\) (\(\pm 6.2\%\) ripple) | \(25.0^\circ\) (\(19.7\%\) ripple) | `UniversalJoint.cs` defines default `operatingAngleDeg = 20.0f` with peak ratio \(\sec(20^\circ) = 1.064\times\). |
| **Crank-Rocker Quick-Return** | \(Q = 1.16 : 1\) (\(1.157 : 1\)) | \(1.28 : 1\) in legacy draft | `CrankRockerMechanism.cs` solves 4-bar loop closure for \(a=32, b=105, c=75, d=110\text{ mm}\) yielding \(Q = 1.157 : 1\). |
| **Lead Screw Thrust** | \(9,948\text{ N}\) at \(10\text{ N}\cdot\text{m}\) (\(\eta = 0.95\)) | \(2,162\text{ N}\) at \(5\text{ N}\cdot\text{m}\) | \(F = (2\pi \cdot \eta \cdot T) / L\). At \(10\text{ N}\cdot\text{m}\), produces \(9,948\text{ N}\); the \(2.16\text{ kN}\) figure assumed high boundary friction (\(\eta \approx 0.41\)). |
| **Worm Drive Self-Locking** | \(\gamma = 7.39^\circ < \phi = 8.53^\circ\) (Proven) | Claimed unconditionally | `WormGearPair.cs` verifies lead angle \(\gamma = 7.39^\circ\) against static friction angle \(\phi = \arctan(0.15) = 8.53^\circ\). |

## 10.1 Important pump formula correction

The Experiment Lab guide historically recorded a draft shorthand:
\[
Q = \frac{\pi D^2 L N}{240}
\]
and associated it with \(9.54\text{ L/min}\) for \(D = 45\text{ mm}, L = 50\text{ mm}\).

That written expression arose from dividing by \(4 \times 60 = 240\), which incorrectly combined unit conversions. 

The standard, dimensionally rigorous single-acting theoretical pump formula implemented in `ReciprocatingPump.cs` is:
\[
Q = A \cdot S \cdot \frac{N}{60} \cdot 1000 \cdot 60 = A \cdot S \cdot N \cdot 1000 \quad [\text{L/min}]
\]
where:
- \(A = \pi \cdot r_{\text{bore}}^2\) is the cylinder cross-sectional area in \(\text{m}^2\).
- \(S = 2r_{\text{crank}}\) is the total stroke in \(\text{m}\).
- \(N\) is the crank rotational speed in \(\text{RPM}\).

With the C# codebase's actual parameters (\(r_{\text{bore}} = 0.030\text{ m} \implies D = 60\text{ mm}\), \(r_{\text{crank}} = 0.030\text{ m} \implies S = 60\text{ mm}\), and \(N = 120\text{ RPM}\)):
\[
Q = (\pi \times 0.030^2) \times 0.060 \times 120 \times 1000 = \mathbf{20.36\text{ L/min}}
\]

## 10.2 Important lead screw thrust distinction

The ideal, lossless screw force equation is:
\[
F_{\text{ideal}} = \frac{2\pi T_{\text{in}}}{L}
\]

For an input torque of \(T_{\text{in}} = 5\text{ N}\cdot\text{m}\) and lead \(L = 0.006\text{ m}\):
\[
F_{\text{ideal}} = \frac{2\pi \times 5}{0.006} \approx \mathbf{5,236\text{ N}}
\]

A real screw experiences thread friction, reducing transmitted axial force according to transmission efficiency \(\eta\):
\[
\mathbf{F_{\text{axial}} = \frac{2\pi \cdot \eta \cdot T_{\text{in}}}{L}}
\]

In MECH-XR:
- At \(T_{\text{in}} = 10\text{ N}\cdot\text{m}\) and standard lubricated bronze-on-steel efficiency \(\eta = 0.95\):
  \[
  F = \frac{2\pi \times 0.95 \times 10}{0.006} = \mathbf{9,948\text{ N}} \quad (\approx 1,014\text{ kgf})
  \]
- At \(T_{\text{in}} = 5\text{ N}\cdot\text{m}\) and \(\eta = 0.95\):
  \[
  F = \frac{2\pi \times 0.95 \times 5}{0.006} = \mathbf{4,974\text{ N}} \quad (\approx 507\text{ kgf})
  \]
- The older documentation value of \(2,160\text{ N}\) (\(2.16\text{ kN}\)) at \(5\text{ N}\cdot\text{m}\) represents an unlubricated boundary friction regime with an efficiency of \(\eta = \frac{2160}{5236} \approx \mathbf{0.412}\) (\(41.2\%\)).

Furthermore, force divided by torque has units of \(\text{N}/(\text{N}\cdot\text{m}) = \text{m}^{-1}\) (linear mechanical gain), rather than being a dimensionless ratio. To compute dimensionless mechanical advantage, the input lever arm radius \(R_{\text{handle}}\) must be specified (\(\text{MA} = F_{\text{axial}} / F_{\text{input}} = \frac{2\pi R_{\text{handle}}\eta}{L}\)).

## 10.3 Important universal joint distinction

The kinematic position relationship between two shafts intersecting at an angle \(\beta\) is:
\[
\tan\theta_2 = \tan\theta_1 \cdot \cos\beta
\]

Differentiating with respect to time yields the exact instantaneous angular velocity ratio:
\[
\frac{\omega_2}{\omega_1} = \frac{\cos\beta}{1 - \sin^2\beta\cos^2\theta_1}
\]

This creates a second-harmonic velocity ripple oscillating at twice the input shaft frequency (\(2\omega\)):
- **Maximum velocity ratio** (at \(\theta_1 = 0^\circ, 180^\circ\)):
  \[
  \left(\frac{\omega_2}{\omega_1}\right)_{\max} = \frac{1}{\cos\beta} = \sec\beta
  \]
- **Minimum velocity ratio** (at \(\theta_1 = 90^\circ, 270^\circ\)):
  \[
  \left(\frac{\omega_2}{\omega_1}\right)_{\min} = \cos\beta
  \]

In MECH-XR's C# implementation (`UniversalJoint.cs`), the default angle is \(\beta = 20.0^\circ\):
- \(\sec(20^\circ) = 1.0642\)
- \(\cos(20^\circ) = 0.9397\)
- Total speed fluctuation: \(\Delta\omega / \omega = 1.0642 - 0.9397 = \mathbf{12.45\%}\) (peak-to-trough, or \(\mathbf{\pm 6.22\%}\)).

The older documentation value of \(19.7\%\) corresponds to an angle of \(\beta = 25.0^\circ\) (\(\sec 25^\circ - \cos 25^\circ = 1.1034 - 0.9063 = 0.1971\)). In MECH-XR, the user can adjust the angle slider between \(0^\circ\) and \(35^\circ\), and the live HUD displays both the principal phase angle and the exact instantaneous speed ratio in real time.

---

# 11. TOP 20 EXPERIMENT LAB VIVA QUESTIONS & MODEL ANSWERS

### Q1: What is the core mathematical relationship in the Experiment Lab?
**Answer:** The core relationship is the rotational power equation: \(P = T \cdot \omega\), where power in watts is the product of applied torque in newton-metres and angular velocity in radians per second.

### Q2: Why is output torque multiplied by efficiency?
**Answer:** In any physical transmission, mechanical losses (tooth friction, bearing shear, fluid churn) dissipate energy as heat. Since \(P_{\text{out}} = \eta \cdot P_{\text{in}}\), the delivered output torque is reduced to \(T_{\text{out}} = \eta \cdot T_{\text{in}} \cdot i\).

### Q3: What is the difference between RPM and angular velocity?
**Answer:** RPM (revolutions per minute) is an engineering frequency unit measuring complete $360^\circ$ shaft rotations per minute. Angular velocity (\(\omega\)) is an SI physical unit measuring the angle traversed in radians per second (\(\omega = \frac{2\pi N}{60}\)).

### Q4: Does MECH-XR physically measure torque from the phone?
**Answer:** No. MECH-XR is an educational kinematic simulator running on an Android mobile device. Torque and efficiency are user-controlled input parameters fed into mathematical formulas to calculate theoretical power, torque multiplication, and thermal losses.

### Q5: What does the Crimson Red curve in the Power/Loss graph represent?
**Answer:** The crimson red curve represents dissipated parasitic power loss: \(P_{\text{loss}} = P_{\text{in}} - P_{\text{out}} = (1 - \eta) \cdot P_{\text{in}}\). It visualizes the energy lost to heat during power transmission.

### Q6: What happens to mechanical power if we double RPM while keeping torque constant?
**Answer:** Mechanical power doubles linearly. Because \(P = T \cdot \omega\) and \(\omega \propto N\), doubling rotational speed doubles the rate of mechanical work done per second.

### Q7: What happens to output torque if gear ratio is doubled at constant input torque?
**Answer:** Output torque doubles (assuming constant efficiency). Gear reduction trades speed for torque proportionally: \(T_{\text{out}} \propto i\).

### Q8: What is the purpose of the A/B comparison system?
**Answer:** A/B comparison enables students to snapshot a baseline operating condition into Memory A and observe live numerical deltas (\(\Delta\text{RPM}, \Delta P, \Delta\text{Loss}\)) in Memory B, demonstrating how parameter changes affect transmission efficiency.

### Q9: Why does the automotive differential maintain constant carrier speed during a turn?
**Answer:** Because the carrier is driven directly by the engine's pinion and ring gear. According to Willis' axle law (\(\omega_c = \frac{\omega_L + \omega_R}{2}\)), any speed reduction on the inner wheel during a corner is transferred through the rotating spider bevel gears as an equal speed increase to the outer wheel.

### Q10: Why does a four-stroke internal combustion engine require a 720-degree cycle?
**Answer:** Each of the four strokes (Intake, Compression, Power, Exhaust) requires $180^\circ$ of crankshaft rotation. The complete sequence takes $4 \times 180^\circ = 720^\circ$, or two full crankshaft revolutions. Consequently, the camshaft operates at half crankshaft speed (\(N_{\text{cam}} = \frac{N_{\text{crank}}}{2}\)).

### Q11: What is the obliquity ratio in a slider-crank mechanism?
**Answer:** The obliquity ratio is \(\lambda = \frac{r}{l}\), the ratio of crank radius to connecting rod length. A higher \(\lambda\) increases connecting rod angularity, creating higher secondary harmonic acceleration and greater lateral cylinder wall thrust forces.

### Q12: Why is the Scotch Yoke displacement curve purely sinusoidal while the slider-crank is not?
**Answer:** In the Scotch yoke, the driving pin engages a perpendicular transverse slot, so the linear displacement follows the orthogonal projection \(x = r\cos(\omega t)\) without connecting rod angular distortion. In a slider-crank, the angled connecting rod introduces secondary harmonic terms (\(\sqrt{l^2 - r^2\sin^2\theta}\)).

### Q13: What is Grashof's Class I condition in the four-bar linkage?
**Answer:** Grashof's condition states that if the sum of the shortest and longest link lengths is less than or equal to the sum of the other two links (\(s + l \le p + q\)), at least one link can perform continuous $360^\circ$ rotation relative to the other three.

### Q14: What is the transmission angle in a four-bar linkage?
**Answer:** The transmission angle (\(\mu\)) is the acute angle between the coupler link and the driven rocker link. It measures the effectiveness of force transmission. To prevent mechanical jamming, \(\mu\) should ideally remain between $40^\circ$ and $140^\circ$.

### Q15: Why does a universal (Cardan) joint produce velocity ripple?
**Answer:** Because the cross-pin rotates in two planes simultaneously when operating across an angle \(\beta\). The governing equation \(\tan\theta_2 = \tan\theta_1\cos\beta\) dictates that output speed accelerates and decelerates twice per revolution ($2\omega$), creating speed fluctuations.

### Q16: What is chordal action in a chain drive?
**Answer:** Chordal action (the polygonal effect) occurs because a roller chain wraps around a sprocket as a series of straight chords rather than a perfect circle. As sprocket teeth enter and leave engagement, the effective pitch radius fluctuates, causing small periodic velocity ripples (\(\approx 2.19\%\)).

### Q17: What makes a lead screw self-locking?
**Answer:** A lead screw is self-locking when its helix lead angle (\(\lambda\)) is smaller than the effective friction angle (\(\rho'\)): \(\lambda < \rho'\). Under this condition, axial forces cannot back-drive the screw, which is essential for machine jacks and clamping vises.

### Q18: What is dwell in a cam or Geneva mechanism?
**Answer:** Dwell is a phase in a mechanism's cycle where the input shaft continues to rotate uniformly while the output component remains completely stationary.

### Q19: What is the duty cycle of the Geneva mechanism in MECH-XR?
**Answer:** The 4-slot Geneva mechanism indexes the output wheel by $90^\circ$ during $90^\circ$ of crank rotation ($25\%$ advance duty cycle) and holds the wheel stationary for the remaining $270^\circ$ of crank rotation ($75\%$ dwell duty cycle).

### Q20: What is the fundamental difference between an ideal mechanical simulation and a physical test rig?
**Answer:** A simulation calculates theoretical values from mathematical models and idealized geometric equations. A physical test rig measures real-world quantities using physical sensors (load cells, tachometers, dynamometers), capturing unmodeled phenomena such as ambient thermal drift, manufacturing tolerances, wear, and structural vibration.

---

# 12. THE FIVE CORE FORMULAS SUMMARY

$$\boxed{\omega = \frac{2\pi N}{60}} \quad \boxed{i = \frac{N_{\text{in}}}{N_{\text{out}}} = \frac{Z_{\text{out}}}{Z_{\text{in}}}} \quad \boxed{P = T \cdot \omega = \frac{2\pi N T}{60}}$$

$$\boxed{T_{\text{out}} = \eta \cdot T_{\text{in}} \cdot i} \quad \boxed{P_{\text{loss}} = (1 - \eta) \cdot P_{\text{in}}}$$


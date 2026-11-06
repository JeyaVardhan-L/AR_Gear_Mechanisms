# MECH-XR — SCIENTIFIC EXPERIMENT LAB USER GUIDE

Welcome to the **MECH-XR Scientific Experiment Lab**. This guide is designed for mechanical engineering students, instructors, and researchers to make full use of the interactive simulation, mathematical models, and real-time physical experimentation tools in MECH-XR.

---

## 1. ACCESSING THE EXPERIMENT LAB

1. **Launch MECH-XR** on your Android device (e.g., Xiaomi POCO X6 5G).
2. From the **Mechanism Catalog**, select any of the **26 mechanisms** (filtered across ALL, GEARS, ENGINES, LINKAGES, and DRIVES) by tapping `EXPLORE ▶`.
3. Scan a flat tabletop or floor surface until the reticle appears, then tap the screen to anchor the mechanical assembly in AR.
4. On the top-right corner of the AR HUD, tap the **`EXPERIMENT LAB 🔬`** button.
5. The collapsible laboratory drawer glides into view, displaying real-time telemetry, dynamic graphs, engineering presets, and comparison controls.

---

## 2. GOVERNING PHYSICAL EQUATIONS

MECH-XR simulates mechanical systems using strict SI engineering standards:

### 2.1 Angular Velocity
$$\omega_{in} = \frac{2\pi \cdot N_{in}}{60}\quad [\text{rad/s}],\qquad \omega_{out} = \frac{\omega_{in}}{i}\quad [\text{rad/s}]$$
where $N_{in}$ is input rotational speed in Revolutions Per Minute (RPM), and $i$ is the kinematic gear ratio.

### 2.2 Ideal vs Real Torque
For an ideal lossless gear transmission:
$$T_{out,\text{ideal}} = T_{in} \cdot i\quad [\text{N}\cdot\text{m}]$$
Under real mechanical conditions with efficiency $\eta \in [0.50, 1.00]$:
$$T_{out,\text{actual}} = \eta \cdot T_{in} \cdot i\quad [\text{N}\cdot\text{m}]$$

### 2.3 Mechanical Power & Dissipation
- **Input Mechanical Power**:
  $$P_{in} = T_{in} \cdot \omega_{in}\quad [\text{W}]$$
- **Output Useful Mechanical Power**:
  $$P_{out} = \eta \cdot P_{in}\quad [\text{W}]$$
- **Power Loss (Thermal Dissipation)**:
  $$P_{loss} = P_{in} - P_{out} = (1 - \eta) P_{in}\quad [\text{W}]$$

---

## 3. INTERACTIVE EXPERIMENT CONTROLS

### 3.1 Input Velocity ($10 - 300\text{ RPM}$)
- **Finger Drag**: Drag along the slider track to smoothly accelerate or decelerate the drive shaft.
- **Track Tap**: Tap anywhere on the slider bar to jump directly to that velocity.
- **Fine-Tune Steppers**: Tap `[-10]` or `[+10]` for precise incremental speed stepping.
- **Angular Velocity Readout**: The live HUD displays both RPM and angular velocity ($\text{rad/s}$).

### 3.2 Input Torque ($0.1 - 500\text{ N}\cdot\text{m}$)
- Adjust input motor torque to observe how transmitted power scales linearly with torque at constant speed.

### 3.3 Transmission Efficiency ($50\% - 100\%$)
- Simulate different lubrication and surface finish conditions:
  - $\eta = 98\%$: Ground AGMA 12 gears with synthetic oil lubrication.
  - $\eta = 90\%$: Standard commercial industrial gearbox.
  - $\eta = 75\%$: Dry sliding friction or worn gear teeth. Notice the dramatic increase in $P_{loss}$ (red bar).

### 3.4 Automotive Differential Turn Bias (Mechanism 13 Only)
- Adjust the **Turn Bias Slider** $(\Delta \in [-0.8, +0.8])$:
  - $\Delta = 0.0$: Straight-line driving ($N_L = N_R = N_{carrier} = 40\text{ RPM}$). Spiders do not spin on cross-pin.
  - $\Delta = -0.5$: Sharp left turn ($N_L = 20\text{ RPM}, N_R = 60\text{ RPM}$).
  - $\Delta = +0.5$: Sharp right turn ($N_L = 60\text{ RPM}, N_R = 20\text{ RPM}$).
  - Verifies **Willis' Epicyclic Law**: $\frac{N_L + N_R}{2} = N_{carrier}$ at all times.

### 3.5 Specialized Mechanism Controls & Dynamic Telemetry
In addition to general power and speed metrics, the 12 new mechanisms display dedicated engineering telemetry:

#### A. Engines & Reciprocating Mechanisms
- **Four-Stroke IC Engine (Mechanism 15)**:
  - **Live Stroke Badge**: Dynamically indicates `INTAKE`, `COMPRESSION`, `POWER`, or `EXHAUST`.
  - **Crankshaft Angle**: Tracks continuous rotation across $0^\circ \to 720^\circ$.
  - **Valve State**: Live readout of intake/exhaust valve lift ($h = 6.0\text{ mm}$) and sealing status.
  - **Camshaft Speed**: Real-time readout demonstrating the exact $1:2$ speed reduction ($N_{\text{cam}} = 0.5 \cdot N_{\text{crank}}$).
- **Slider-Crank Mechanism (Mechanism 16)**:
  - **Piston Stroke**: $S = 2r = 80.0\text{ mm}$ (crank radius $r = 40.0\text{ mm}$, rod $l = 120.0\text{ mm}$).
  - **Obliquity Ratio**: $\lambda = r/l = 0.333$.
  - **Asymmetric Acceleration**: Inspect live peak acceleration at TDC ($1.333 \cdot r\omega^2$) vs BDC ($-0.667 \cdot r\omega^2$).
- **Cam and Follower (Mechanism 17)**:
  - **Base Circle Radius**: $R_b = 35.0\text{ mm}$, Roller radius $r_{\text{roller}} = 9.0\text{ mm}$.
  - **Follower Lift**: Live displacement $s(\theta) \in [0, 18.0\text{ mm}]$ with peak lift $h = 18.0\text{ mm}$.
  - **Harmonic Profile**: Simple harmonic rise ($60^\circ - 180^\circ$), top dwell ($180^\circ - 220^\circ$), harmonic fall ($220^\circ - 320^\circ$), and base dwell ($320^\circ - 60^\circ$).
  - **Follower Velocity**: Live linear velocity $v(\theta)$ in $\text{m/s}$.
- **Reciprocating Pump (Mechanism 18)**:
  - **Bore & Stroke**: Cylinder bore diameter $D = 60.0\text{ mm}$ ($r_{\text{bore}} = 30.0\text{ mm}$), stroke $S = 60.0\text{ mm}$ (crank $r = 30.0\text{ mm}$).
  - **Theoretical Flow Rate ($Q$)**: Evaluated via standard volumetric displacement:
    $$Q = A \cdot S \cdot N \cdot 1000\quad [\text{L/min}]$$
    where $A = \pi \cdot r_{\text{bore}}^2 = 0.002827\text{ m}^2$, $S = 0.060\text{ m}$. At $120\text{ RPM}$, $Q = \mathbf{20.36\text{ L/min}}$.
    *(Note: Older draft documents included a typographical formula $Q = \frac{\pi D^2 L N}{240}$ with $D=45\text{ mm}$ giving $9.54\text{ L/min}$; the actual C# codebase strictly implements the standard dimensional formula yielding $20.36\text{ L/min}$).*
  - **Non-Return Valve Indicators**: Suction check valve (green/open during intake stroke $15^\circ - 165^\circ$) and delivery check valve (cyan/open during discharge stroke $195^\circ - 345^\circ$).

#### B. Linkages & Motion Conversion
- **Four-Bar Linkage (Mechanism 19)**:
  - **Link Dimensions**: Ground $d = 120.0\text{ mm}$, Crank $a = 40.0\text{ mm}$, Coupler $b = 100.0\text{ mm}$, Rocker $c = 85.0\text{ mm}$.
  - **Grashof Condition**: Displays `Grashof Class I (s+l <= p+q)` where $s+l = 40+120 = 160\text{ mm} \le p+q = 85+100 = 185\text{ mm}$, confirming continuous $360^\circ$ crank rotation.
  - **Transmission Angle ($\mu$)**: Real-time angular tracking ensuring $\mu \in [42^\circ, 138^\circ]$ to prevent toggle lock.
- **Crank-Rocker Mechanism (Mechanism 20)**:
  - **Link Dimensions**: Ground $d = 110.0\text{ mm}$, Crank $a = 32.0\text{ mm}$, Coupler $b = 105.0\text{ mm}$, Rocker $c = 75.0\text{ mm}$.
  - **Quick-Return Ratio ($Q$)**: Live calculation displaying $Q = \frac{180^\circ + \alpha}{180^\circ - \alpha} \approx \mathbf{1.16 : 1}$ ($1.157 : 1$ with rocker sweep $\Delta\theta = 52.3^\circ$).
  - **Rocker Swing Angle**: Maximum oscillatory amplitude ($\psi = 52.3^\circ$).
- **Geneva Mechanism (Mechanism 21)**:
  - **Drive State**: Displays `INDEXING (Motion 90°)` vs `LOCKED (Dwell 270°)`.
  - **Duty Cycle**: Highlights the exact $25\%$ indexing / $75\%$ dwell split for the 4-slot cross.
  - **Velocity Ratio**: Average reduction ratio $i_{\text{avg}} = 4.0 : 1$ ($120\text{ RPM input} \implies 30\text{ RPM indexed output}$).
- **Scotch Yoke Mechanism (Mechanism 22)**:
  - **Harmonic Purity**: Displays `Pure SHM (Zero 2nd-Harmonic Unbalance)`.
  - **Kinematic Equations**: $x(t) = r\cos(\omega t)$, $v(t) = -r\omega\sin(\omega t)$, $a(t) = -r\omega^2\cos(\omega t)$.
  - **Linear Stroke**: $S = 2r = 80.0\text{ mm}$, providing an ideal comparison with the Slider-Crank.

#### C. Power Transmission & Drives
- **Universal Joint (Mechanism 23)**:
  - **Operating Shaft Angle ($\beta$)**: Configured default at $\beta = \mathbf{20.0^\circ}$ (adjustable up to $35.0^\circ$).
  - **Cardan Governing Equation**: $\tan\theta_2 = \tan\theta_1 \cdot \cos\beta$.
  - **Velocity Ripple (2nd Harmonic)**: Output angular velocity fluctuates twice per revolution ($2\omega$):
    - Peak speed ratio: $\omega_2 / \omega_1 = \sec\beta = \mathbf{1.064\times}$
    - Trough speed ratio: $\omega_2 / \omega_1 = \cos\beta = \mathbf{0.940\times}$
    - Net speed fluctuation: $\Delta\omega / \omega = \sec\beta - \cos\beta = \mathbf{12.45\%}$ ($\pm 6.22\%$).
    *(Note: Older draft documentation referenced $\beta = 25.0^\circ$ producing $19.7\%$ ripple; the C# codebase implements $\beta = 20.0^\circ$ producing $12.45\%$ ripple).*
- **Chain and Sprocket Drive (Mechanism 24)**:
  - **Sprocket Specifications**: Driver $Z_1 = 15$, Driven $Z_2 = 30$, Ratio $i = 2.00 : 1$ ($120\text{ RPM} \to 60\text{ RPM}$).
  - **Linear Pitch Speed**: $v = 0.38\text{ m/s}$ ($1.37\text{ km/h}$) at $120\text{ RPM}$.
  - **Chordal Action Variation**: Real-time display of polygonal speed fluctuation ($2.19\%$).
- **Ratchet and Pawl (Mechanism 25)**:
  - **One-Way Indexing Step**: $15.0^\circ$ per ratchet tooth ($Z = 24$, ratio $24:1$, $120\text{ RPM} \to 5\text{ RPM}$).
  - **Pawl State**: Displays driving push vs backstop holding engagement.
- **Lead Screw and Nut (Mechanism 26)**:
  - **Lead Screw Geometry**: Nominal diameter $d = 24.0\text{ mm}$, Single-start Acme lead $L = 6.0\text{ mm/rev}$.
  - **Linear Advance Speed**: $v = \frac{L \cdot N}{60} = \mathbf{12.0\text{ mm/s}}$ at $120\text{ RPM}$.
  - **Pitch Radius & Lead Angle**: Mean diameter $d_m = 21.0\text{ mm}$, Lead angle $\lambda = \arctan\left(\frac{L}{\pi d_m}\right) = \mathbf{5.20^\circ}$.
  - **Friction Angle & Self-Locking**: Bronze-on-steel friction coefficient $\mu = 0.15$ gives friction angle $\phi = \arctan(0.15) = \mathbf{8.53^\circ}$. Because $\lambda < \phi$ ($5.2^\circ < 8.53^\circ$), the mechanism is **strictly self-locking**.
  - **Axial Clamping Thrust**:
    $$F_{\text{axial}} = \frac{2\pi \cdot \eta \cdot T_{\text{in}}}{L}$$
    - At $10\text{ N}\cdot\text{m}$ input torque and standard $\eta = 0.95$: $F_{\text{thrust}} = \mathbf{9,948\text{ N}}$ ($\approx 1,014\text{ kgf}$).
    - At $5\text{ N}\cdot\text{m}$ input torque and $\eta = 0.95$: $F_{\text{thrust}} = \mathbf{4,974\text{ N}}$.
    *(Note: Older draft documents reported $2.16\text{ kN}$ at $5\text{ N}\cdot\text{m}$; that value assumed an unlubricated boundary friction efficiency of $\eta \approx 0.41$).*

---

## 4. DYNAMIC REAL-TIME GRAPHS

The Experiment Lab includes an embedded real-time graphing display with four modes:

1. **`RPM`**: Compares input motor speed ($N_{in}$) against output shaft speed ($N_{out}$).
2. **`TORQUE`**: Compares input motor torque ($T_{in}$) against mechanical output torque ($T_{out}$).
3. **`SPEED/TIME`**: Renders a rolling timeline showing dynamic velocity response during acceleration and braking.
4. **`POWER/LOSS`**: Displays input power (Sky Blue), output mechanical power (Emerald Green), and friction heat dissipation (Crimson Red).

---

## 5. EXPERIMENT PRESETS

Use the preset buttons to quickly load standard engineering operating regimes:

- **`HIGH SPEED`**: $240\text{ RPM}$, $5\text{ N}\cdot\text{m}$, $\eta = 96\%$. High kinetic energy transmission.
- **`HIGH TORQUE`**: $60\text{ RPM}$, $50\text{ N}\cdot\text{m}$, $\eta = 94\%$. Heavy equipment planetary / compound drive.
- **`INDUSTRIAL`**: $150\text{ RPM}$, $30\text{ N}\cdot\text{m}$, $\eta = 92\%$. Factory production automation standard.
- **`LOW EFF (75%)`**: $120\text{ RPM}$, $20\text{ N}\cdot\text{m}$, $\eta = 75\%$. Demonstrates thermal failure hazards and lubrication breakdown.

---

## 6. A/B EXPERIMENT COMPARISON

Students can conduct rigorous comparative analyses:
1. Configure your initial baseline parameters (e.g. $120\text{ RPM}$, $10\text{ N}\cdot\text{m}$, $\eta = 95\%$).
2. Tap **`CAPTURE EXPERIMENT A 📌`**. The state is snapshotted.
3. Modify any parameter (e.g. drop efficiency to $80\%$ or increase RPM to $240$).
4. The live comparison text automatically updates with exact delta differences:
   - Speed Difference ($\Delta N$)
   - Output Torque Delta ($\Delta T_{out}$)
   - Power Output Delta ($\Delta P_{out}$)
   - Additional Heat Loss ($\Delta P_{loss}$)

---

## 7. GESTURE & INSPECTION SUMMARY

| Action | Mobile Gesture | Effect |
|:---|:---|:---|
| **Pinch-to-Zoom** | Two-finger pinch in / out | Scales model uniformly between $0.4\times$ and $2.5\times$ with HUD scale pill. |
| **Model Rotation** | Two-finger drag or twist | Rotates entire assembly on tabletop plane. |
| **Inspect Component** | Tap any 3D gear or shaft | Opens component engineering card with specs, material, and role. |
| **Component Detach** | Tap `DETACH ⤢` | Pulls selected component away along its mechanical axis. |
| **360° Part Spin** | One-finger drag while detached | Rotates detached component around its own center for full inspection. |
| **Exploded View** | Tap `EXPLODE ⤢` | Simultaneously explodes all components outward into clear spatial view. |


# MECH-XR — KINEMATICS, MATHEMATICAL MODELS & SI FORMULAS

This document outlines the governing mechanical engineering equations, tooth parameters, kinematic velocity vectors, and SI energy relationships implemented across MECH-XR's **26 mechanical systems**.

---

## 1. GENERAL SI ENERGY & POWER EQUATIONS

For any rotating shaft operating at rotational frequency $N$ [RPM] and subject to torque $T$ [$\text{N}\cdot\text{m}$]:

### 1.1 Angular Velocity
$$\omega = \frac{2\pi \cdot N}{60}\quad [\text{rad/s}]$$

### 1.2 Mechanical Power
$$P_{in} = T_{in} \cdot \omega_{in}\quad [\text{W}]$$

### 1.3 Transmission Efficiency & Torque Transformation
Under mechanical efficiency $\eta \in [0.50, 1.00]$:
$$P_{out} = \eta \cdot P_{in}\quad [\text{W}]$$
$$\omega_{out} = \frac{\omega_{in}}{i}\quad [\text{rad/s}]$$
$$T_{out} = \frac{P_{out}}{\omega_{out}} = \frac{\eta \cdot P_{in}}{\omega_{in} / i} = \eta \cdot T_{in} \cdot i\quad [\text{N}\cdot\text{m}]$$

### 1.4 Power Dissipation & Thermal Losses
$$P_{loss} = P_{in} - P_{out} = (1 - \eta) P_{in}\quad [\text{W}]$$

---

## 2. KINEMATIC FORMULATIONS FOR THE 26 MECHANISMS

### 01. Spur Gear Pair
- **Tooth Counts**: Driver $Z_1 = 20$, Driven $Z_2 = 40$. Standard Module $m = 3.5\text{ mm} = 0.0035\text{ m}$.
- **Pitch Diameters**: $d_1 = m \cdot Z_1 = 70.0\text{ mm}$, $d_2 = m \cdot Z_2 = 140.0\text{ mm}$.
- **Center Distance**: $a = \frac{d_1 + d_2}{2} = \frac{70 + 140}{2} = 105.0\text{ mm}$.
- **Velocity Ratio**: $i = \frac{Z_2}{Z_1} = \frac{40}{20} = 2.0 : 1$.
- **Direction**: Counter-rotating ($\omega_2 = -\frac{\omega_1}{i}$).

### 02. Bevel Gear Pair ($90^\circ$ Intersecting Axes)
- **Tooth Counts**: Pinion $Z_1 = 16$, Crown $Z_2 = 32$. Module $m = 3.5\text{ mm}$.
- **Pitch Cone Angles**: $\delta_1 = \arctan(Z_1 / Z_2) = \arctan(16/32) = 26.565^\circ$, $\delta_2 = 90^\circ - \delta_1 = 63.435^\circ$.
- **Velocity Ratio**: $i = \frac{Z_2}{Z_1} = \frac{32}{16} = 2.0 : 1$.

### 03. Worm and Wheel Drive
- **Parameters**: 1-start Acme worm ($Z_1 = 1$, lead $p_z = \pi m = 10.996\text{ mm}$), globoid wheel ($Z_2 = 30$).
- **Velocity Ratio**: $i = \frac{Z_2}{Z_1} = \frac{30}{1} = 30.0 : 1$.
- **Lead Angle**: $\gamma = \arctan\left(\frac{p_z}{\pi d_1}\right) = \arctan\left(\frac{10.996}{\pi \cdot 27.0}\right) = 7.394^\circ$.
- **Self-Locking Condition**: Friction angle $\rho' \approx \arctan(0.15) = 8.53^\circ > \gamma$. The drive is strictly non-backdrivable.

### 04. Planetary Gear System (Epicyclic)
- **Tooth Counts**: Sun $Z_s = 18$, 3 Planets $Z_p = 18$, Ring $Z_r = 54$.
- **Epicyclic Constraint**: $Z_r = Z_s + 2 Z_p = 18 + 2(18) = 54$.
- **Planetary Spacing Rule**: $(Z_s + Z_r) \pmod 3 = (18 + 54) \pmod 3 = 72 \pmod 3 = 0$ (Equal $120^\circ$ planet spacing).
- **Velocity Ratio (Fixed Ring, Carrier Output)**:
  $$i = 1 + \frac{Z_r}{Z_s} = 1 + \frac{54}{18} = 4.0 : 1$$

### 05. Rack and Pinion Motion Converter
- **Parameters**: Pinion $Z_1 = 20$, Module $m = 3.5\text{ mm}$. Linear pitch $p = \pi m = 10.996\text{ mm}$.
- **Linear Pitch Velocity**:
  $$v = \omega_{pinion} \cdot r_{pitch} = \left(\frac{2\pi \cdot 120}{60}\right) \cdot 0.035 = 0.4398\text{ m/s} = 439.8\text{ mm/s}$$

### 06. Belt and Pulley System
- **Diameters**: Driver $D_1 = 60.0\text{ mm}$, Driven $D_2 = 120.0\text{ mm}$, Center distance $C = 160.0\text{ mm}$.
- **Velocity Ratio**: $i = \frac{D_2}{D_1} = \frac{120}{60} = 2.0 : 1$.
- **Total Belt Pitch Length**:
  $$L = 2C + \frac{\pi(D_1 + D_2)}{2} + \frac{(D_2 - D_1)^2}{4C} = 320 + 282.74 + 5.625 = 608.37\text{ mm}$$

### 07. Simple Gear Train with Idler
- **Tooth Counts**: Driver $Z_1 = 20$, Idler $Z_2 = 28$, Driven $Z_3 = 40$.
- **Center Distances**: $a_{12} = \frac{m(Z_1+Z_2)}{2} = 84.0\text{ mm}$, $a_{23} = \frac{m(Z_2+Z_3)}{2} = 119.0\text{ mm}$.
- **Kinematic Invariance**:
  $$\frac{\omega_3}{\omega_1} = \left(-\frac{Z_1}{Z_2}\right) \cdot \left(-\frac{Z_2}{Z_3}\right) = +\frac{Z_1}{Z_3} = \frac{20}{40} = 0.5$$
  Overall ratio $i = \frac{Z_3}{Z_1} = 2.0 : 1$. The idler tooth count $Z_2$ cancels identically; output turns in the SAME direction as input.

### 08. Compound Gear Train
- **Tooth Counts**: Stage 1 ($Z_1 = 18, Z_2 = 36$), Stage 2 ($Z_3 = 18, Z_4 = 54$).
- **Overall Velocity Ratio**:
  $$i = \left(\frac{Z_2}{Z_1}\right) \cdot \left(\frac{Z_4}{Z_3}\right) = \left(\frac{36}{18}\right) \cdot \left(\frac{54}{18}\right) = 2.0 \cdot 3.0 = 6.0 : 1$$
- **Output Speed at $120\text{ RPM}$**: $N_{out} = \frac{120}{6.0} = 20.0\text{ RPM}$.
- **Direction**: Both meshes reverse direction: $\omega_4 = (-1) \cdot (-1) \cdot \omega_1 = +\omega_1$ (Same direction).

### 09. Reverted Gear Train
- **Tooth Counts**: First pair ($Z_1 = 20, Z_2 = 40$), Second pair ($Z_3 = 24, Z_4 = 36$). Module $m = 3.5\text{ mm}$.
- **Equal Center Distance Condition**:
  $$a_1 = \frac{m(Z_1 + Z_2)}{2} = \frac{3.5(20 + 40)}{2} = 105.0\text{ mm}$$
  $$a_2 = \frac{m(Z_3 + Z_4)}{2} = \frac{3.5(24 + 36)}{2} = 105.0\text{ mm}$$
- **Coaxial Shaft Alignment**: Input shaft and output shaft share the identical geometric centerline ($X=0, Y=0$).
- **Overall Velocity Ratio**: $i = \left(\frac{40}{20}\right) \cdot \left(\frac{36}{24}\right) = 2.0 \cdot 1.5 = 3.0 : 1$.

### 10. Internal Gear Pair
- **Tooth Counts**: Pinion $Z_1 = 20$, Internal Ring $Z_2 = 60$. Module $m = 3.5\text{ mm}$.
- **Center Distance**:
  $$a = \frac{m(Z_2 - Z_1)}{2} = \frac{3.5(60 - 20)}{2} = 70.0\text{ mm}$$
- **Velocity Ratio**: $i = \frac{Z_2}{Z_1} = \frac{60}{20} = 3.0 : 1$.
- **Direction**: Because the teeth engage internally, input and output rotate in the **SAME direction**.

### 11. Helical Gear Pair
- **Tooth Counts**: Pinion $Z_1 = 20$, Gear $Z_2 = 40$. Normal module $m_n = 3.5\text{ mm}$.
- **Helix Angle**: $\beta = 20.0^\circ$.
- **Transverse Module**: $m_t = \frac{m_n}{\cos\beta} = \frac{3.5}{\cos(20^\circ)} = 3.7246\text{ mm}$.
- **Axial Thrust Force**:
  $$F_a = F_t \cdot \tan\beta$$
  where $F_t = \frac{2 T_1}{d_1}$ is tangential tooth force. Produces significant axial thrust requiring thrust bearings.

### 12. Herringbone Gear Pair (Double-Helical)
- **Geometry**: Left helix ($+\beta = +20^\circ$) and right helix ($-\beta = -20^\circ$) meeting at central apex groove.
- **Velocity Ratio**: $i = \frac{Z_2}{Z_1} = \frac{40}{20} = 2.0 : 1$.
- **Net Axial Thrust Cancellation**:
  $$F_{a,\text{net}} = F_{t,\text{left}} \cdot \tan(+20^\circ) + F_{t,\text{right}} \cdot \tan(-20^\circ) = 0.0\text{ N}$$
  Eliminates the requirement for heavy axial thrust bearings.

### 13. Automotive Differential (Open Differential)
- **Components**: Drive Pinion $Z_p = 12$, Crown Ring $Z_r = 36$, 2 Spider Bevels $Z_s = 14$, 2 Side Bevels $Z_w = 20$.
- **Final Drive Ratio**: $i_0 = \frac{Z_r}{Z_p} = \frac{36}{12} = 3.0 : 1$.
- **Carrier Speed**: $N_{carrier} = \frac{N_{in}}{i_0} = \frac{120}{3.0} = 40.0\text{ RPM}$.
- **Willis Epicyclic Kinematic Law**:
  $$\omega_{carrier} = \frac{\omega_{left} + \omega_{right}}{2}$$
- **Cornering / Turn Bias** $(\Delta \in [-0.8, +0.8])$:
  $$\omega_{left} = \omega_{carrier} \cdot (1 + \Delta),\qquad \omega_{right} = \omega_{carrier} \cdot (1 - \Delta)$$
  - Straight-Line ($\Delta = 0$): $\omega_L = \omega_R = \omega_c = 40\text{ RPM}$.
  - Sharp Left ($\Delta = -0.5$): $\omega_L = 20\text{ RPM}$, $\omega_R = 60\text{ RPM}$. Spider pinions rotate around cross-pin.

### 14. Harmonic Drive (Strain Wave Gearing)
- **Components**: Elliptical Wave Generator Cam, Flexible Flexspline ($Z_f = 48$), Rigid Circular Spline ($Z_c = 50$).
- **Teeth Difference**: $\Delta Z = Z_c - Z_f = 50 - 48 = 2\text{ teeth}$.
- **Kinematic Reduction Ratio**:
  $$i = \frac{Z_f}{Z_c - Z_f} = \frac{48}{2} = 24.0 : 1$$
- **Output Velocity**:
  $$N_{out} = -\frac{N_{in}}{i} = -\frac{120}{24} = -5.0\text{ RPM}$$
  Output turns in the **opposite direction** to input with mathematically zero backlash.

### 15. Four-Stroke Internal Combustion Engine (Single-Cylinder Cross-Section)
- **Kinematic Stroke Cycle**: 4 distinct thermodynamic strokes distributed over two complete crankshaft revolutions ($720^\circ$ of crank rotation $\theta$):
  1. **Intake Stroke** ($0^\circ \le \theta < 180^\circ$): Piston translates downward from Top Dead Center (TDC) to Bottom Dead Center (BDC). Intake poppet valve opens via intake cam lobe (lift $h = 6.0\text{ mm}$); exhaust valve remains closed. Air-fuel charge induction.
  2. **Compression Stroke** ($180^\circ \le \theta < 360^\circ$): Piston translates upward from BDC to TDC. Both intake and exhaust valves remain tightly closed against their ground valve seats. Trapped cylinder volume compresses.
  3. **Power / Expansion Stroke** ($360^\circ \le \theta < 540^\circ$): Ignition event occurs at TDC ($\theta = 360^\circ$). Both valves remain sealed. High-pressure expansion drives piston downward from TDC to BDC, delivering mechanical torque to the crankshaft.
  4. **Exhaust Stroke** ($540^\circ \le \theta < 720^\circ$): Piston translates upward from BDC to TDC. Exhaust poppet valve opens via exhaust cam lobe (lift $h = 6.0\text{ mm}$); intake valve remains closed. Spent exhaust gases are expelled through the exhaust port.
- **Camshaft Timing Transmission**: Driven at exactly half crankshaft speed via 2:1 timing drive:
  $$\omega_{\text{cam}} = 0.5 \cdot \omega_{\text{crank}}$$
- **Slider-Crank Kinematics**:
  - Crank throw radius $r = 30.0\text{ mm} = 0.030\text{ m}$.
  - Connecting rod length $l = 100.0\text{ mm} = 0.100\text{ m}$.
  - Obliquity ratio $\lambda = \frac{r}{l} = \frac{30}{100} = 0.30$.
  - Cylinder Bore $B = 65.0\text{ mm} = 0.065\text{ m}$, Stroke $S = 2r = 60.0\text{ mm} = 0.060\text{ m}$.
  - Displacement from TDC:
    $$x(\theta) = r\left(1 - \cos\theta\right) + l\left(1 - \sqrt{1 - \lambda^2 \sin^2\theta}\right)$$
  - Instantaneous Piston Velocity:
    $$v(\theta) = r\omega \left[ \sin\theta + \frac{\lambda \sin(2\theta)}{2\sqrt{1 - \lambda^2\sin^2\theta}} \right] \approx r\omega \left( \sin\theta + \frac{\lambda}{2} \sin(2\theta) \right)$$
  - Instantaneous Piston Acceleration:
    $$a(\theta) = \frac{dv}{dt} \approx r\omega^2 \left( \cos\theta + \lambda \cos(2\theta) \right)$$
  - Primary unbalance acts at engine speed $\omega$; secondary unbalance acts at $2\omega$ scaled by $\lambda$.

### 16. Slider-Crank Mechanism (Kinematic Inversion)
- **Parameters**: Crank radius $r = 40.0\text{ mm}$, Connecting rod length $l = 120.0\text{ mm}$.
- **Obliquity Ratio**: $\lambda = \frac{r}{l} = \frac{40}{120} = \frac{1}{3} \approx 0.333$.
- **Piston Stroke**: $S = 2r = 80.0\text{ mm}$.
- **Connecting Rod Angular Tilt**:
  $$\sin\phi = \lambda \sin\theta \implies \phi(\theta) = \arcsin(\lambda \sin\theta)$$
  Maximum tilt occurs at $\theta = 90^\circ, 270^\circ$: $\phi_{\max} = \arcsin(0.333) = 19.47^\circ$.
- **Crankshaft Angular Velocity**: $\omega = \frac{2\pi N}{60}\text{ rad/s}$.
- **Slider Acceleration Peak Values**:
  - At TDC ($\theta = 0^\circ$): $a_{\text{TDC}} = r\omega^2 (1 + \lambda) = 1.333 \cdot r\omega^2$.
  - At BDC ($\theta = 180^\circ$): $a_{\text{BDC}} = -r\omega^2 (1 - \lambda) = -0.667 \cdot r\omega^2$.
  - Asymmetry between TDC and BDC acceleration is the defining characteristic of finite-rod slider-crank linkages compared to harmonic scotch yokes.

### 17. Cam and Follower (Simple Harmonic Motion Profile)
- **Base Circle Radius**: $R_b = 30.0\text{ mm} = 0.030\text{ m}$.
- **Maximum Follower Lift**: $h = 16.0\text{ mm} = 0.016\text{ m}$.
- **Rise & Return Angles**: Outstroke / Rise angle $\beta_1 = 120^\circ = \frac{2\pi}{3}\text{ rad}$; Dwell at max lift $\beta_d = 60^\circ$; Return stroke $\beta_2 = 120^\circ$; Dwell at base $\beta_0 = 60^\circ$.
- **Simple Harmonic Motion (SHM) Equations (Rise Phase $0 \le \theta < \beta_1$)**:
  - Lift:
    $$s(\theta) = \frac{h}{2} \left[ 1 - \cos\left(\frac{\pi\theta}{\beta_1}\right) \right]$$
  - Follower Linear Velocity:
    $$v(\theta) = \frac{ds}{dt} = \frac{\pi h \omega}{2\beta_1} \sin\left(\frac{\pi\theta}{\beta_1}\right)$$
  - Follower Linear Acceleration:
    $$a(\theta) = \frac{dv}{dt} = \frac{\pi^2 h \omega^2}{2\beta_1^2} \cos\left(\frac{\pi\theta}{\beta_1}\right)$$
- **Cam Pressure Angle ($\alpha$)**:
  $$\tan\alpha = \frac{1}{r(\theta)} \frac{ds}{d\theta} = \frac{v(\theta)/\omega}{R_b + s(\theta)}$$
  Maximum pressure angle is held below $30^\circ$ to prevent lateral binding in the follower sleeve guide.
- **Spring Restoring Force**: Return spring constant $k = 1200\text{ N/m}$, pre-compression $\delta_0 = 5.0\text{ mm}$:
  $$F_{\text{spring}}(\theta) = k (\delta_0 + s(\theta))$$
  Guarantees positive follower-to-cam contact without floating or dynamic liftoff at speeds up to $300\text{ RPM}$.

### 18. Reciprocating Positive Displacement Pump
- **Cylinder Bore**: $D = 45.0\text{ mm} = 0.045\text{ m}$.
- **Cylinder Cross-Sectional Area**:
  $$A_p = \frac{\pi D^2}{4} = \frac{\pi (0.045)^2}{4} = 1.5904 \times 10^{-3}\text{ m}^2 = 15.90\text{ cm}^2$$
- **Crank Radius**: $r = 25.0\text{ mm} = 0.025\text{ m}$.
- **Piston Stroke**: $L = 2r = 50.0\text{ mm} = 0.050\text{ m}$.
- **Displacement Volume per Stroke**:
  $$V_{\text{stroke}} = A_p \cdot L = 1.5904 \times 10^{-3} \cdot 0.050 = 7.952 \times 10^{-5}\text{ m}^3 = 79.52\text{ mL}$$
- **Theoretical Volumetric Discharge Rate**:
  $$Q_{\text{th}} = V_{\text{stroke}} \cdot \frac{N}{60} = \frac{\pi D^2 L N}{240}\quad [\text{m}^3/\text{s}]$$
  In Liters per Minute ($\text{L/min}$):
  $$Q_{\text{th}} = \left(7.952 \times 10^{-5}\right) \cdot N \cdot 1000 = 0.07952 \cdot N\quad [\text{L/min}]$$
  At $N = 120\text{ RPM}$:
  $$Q_{\text{th}} = 0.07952 \cdot 120 = 9.54\text{ L/min}$$
- **Self-Acting Non-Return Check Valves**:
  - **Suction Valve**: Opens during backward intake stroke ($0^\circ < \theta < 180^\circ$); fluid drawn from reservoir.
  - **Delivery / Discharge Valve**: Opens during forward compression stroke ($180^\circ < \theta < 360^\circ$); pressurized fluid delivered to outlet manifold.

### 19. Four-Bar Planar Linkage (Grashof Class I)
- **Link Lengths**:
  - Ground link: $d = L_1 = 140.0\text{ mm} = 0.140\text{ m}$.
  - Input Crank: $a = L_2 = 45.0\text{ mm} = 0.045\text{ m}$.
  - Coupler Link: $b = L_3 = 135.0\text{ mm} = 0.135\text{ m}$.
  - Output Rocker: $c = L_4 = 95.0\text{ mm} = 0.095\text{ m}$.
- **Grashof Condition Verification**:
  - Shortest link: $s = a = 45.0\text{ mm}$.
  - Longest link: $l = d = 140.0\text{ mm}$.
  - Intermediate links: $p = c = 95.0\text{ mm}$, $q = b = 135.0\text{ mm}$.
  - Inequality check:
    $$s + l = 45 + 140 = 185.0\text{ mm}$$
    $$p + q = 95 + 135 = 230.0\text{ mm}$$
    Since $s + l \le p + q$ ($185 \le 230$), the linkage satisfies the **Grashof Criterion** (Class I).
    Because the shortest link $s$ is adjacent to the ground link, the mechanism is a fully rotatable **Crank-Rocker**.
- **Freudenstein Closed-Loop Vector Equation**:
  $$K_1 \cos\theta_4 - K_2 \cos\theta_2 + K_3 = \cos(\theta_2 - \theta_4)$$
  where the Freudenstein coefficients are:
  $$K_1 = \frac{d}{a} = \frac{140}{45} \approx 3.111$$
  $$K_2 = \frac{d}{c} = \frac{140}{95} \approx 1.474$$
  $$K_3 = \frac{a^2 - b^2 + c^2 + d^2}{2ac} = \frac{45^2 - 135^2 + 95^2 + 140^2}{2(45)(95)} = \frac{2025 - 18225 + 9025 + 19600}{8550} = \frac{12425}{8550} \approx 1.453$$
- **Transmission Angle ($\mu$)**:
  Angle between coupler $b$ and rocker $c$:
  $$\cos\mu = \frac{b^2 + c^2 - (d^2 + a^2 - 2ad\cos\theta_2)}{2bc}$$
  Transmission angle is maintained within $42^\circ \le \mu \le 138^\circ$ throughout the entire $360^\circ$ crank cycle, guaranteeing smooth torque transmission without mechanical toggle lock.

### 20. Crank-Rocker Mechanism (Quick-Return Motion)
- **Geometry**: Shortest link is driving crank ($a = 35.0\text{ mm}$), Ground link $d = 120.0\text{ mm}$, Coupler $b = 110.0\text{ mm}$, Rocker $c = 75.0\text{ mm}$.
- **Dead Centers (Toggle Positions)**:
  - Extreme Forward Toggle: Crank and coupler in tension ($a + b = 35 + 110 = 145.0\text{ mm}$).
  - Extreme Return Toggle: Crank and coupler in folding overlap ($b - a = 110 - 35 = 75.0\text{ mm}$).
- **Rocker Angular Oscillation Amplitude ($\psi$)**:
  $$\psi = \arccos\left(\frac{c^2 + d^2 - (b+a)^2}{2cd}\right) - \arccos\left(\frac{c^2 + d^2 - (b-a)^2}{2cd}\right) \approx 48.6^\circ$$
- **Quick-Return Ratio ($Q$)**:
  The crank angle traversed during the forward working stroke is $\theta_{\text{fwd}} = 180^\circ + \alpha$, while the return stroke angle is $\theta_{\text{ret}} = 180^\circ - \alpha$ where $\alpha \approx 22.4^\circ$:
  $$Q = \frac{\text{Time of Forward Stroke}}{\text{Time of Return Stroke}} = \frac{180^\circ + \alpha}{180^\circ - \alpha} = \frac{202.4^\circ}{157.6^\circ} \approx 1.284 : 1$$
  The cutting tool returns $28.4\%$ faster than it cuts, maximizing industrial shaper productivity.

### 21. Geneva Mechanism (Maltese Cross Intermittent Indexer)
- **Geometry**: $n = 4$ radial slots, driver crank radius $r_1 = 45.0\text{ mm}$.
- **Center-to-Center Distance**:
  $$C = \frac{r_1}{\sin(\pi / n)} = \frac{45}{\sin(45^\circ)} = \frac{45}{0.7071} = 63.64\text{ mm}$$
- **Geneva Wheel Outer Radius**:
  $$r_2 = \sqrt{C^2 - r_1^2} = C \cos(\pi / n) = 63.64 \cdot 0.7071 = 45.0\text{ mm}$$
- **Kinematic Angles per Driver Revolution ($360^\circ$)**:
  - Engagement / Motion Angle:
    $$\beta = 2\left(\frac{\pi}{2} - \frac{\pi}{n}\right) = 180^\circ - \frac{360^\circ}{4} = 90.0^\circ$$
  - Locking Dwell Angle:
    $$\gamma = 360^\circ - \beta = 270.0^\circ$$
- **Duty Cycle & Indexing Step**:
  - Wheel indexing advance per cycle: $\Delta\theta_2 = \frac{360^\circ}{n} = \frac{360^\circ}{4} = 90.0^\circ$.
  - Duty cycle: $\frac{\text{Motion Time}}{\text{Total Cycle}} = \frac{90^\circ}{360^\circ} = 0.25$ ($25\%$ indexing motion, $75\%$ stationary locked dwell).
  - Average Output Speed: $N_{\text{out, avg}} = \frac{N_{\text{in}}}{n} = \frac{120}{4} = 30.0\text{ RPM}$.
- **Locking Disk Geometry**:
  The circular locking cam on the driver engages the concave circular cutout on the Geneva cross during the $270^\circ$ dwell, preventing spurious angular backlash or drift while disengaged.

### 22. Scotch Yoke Mechanism (Pure Simple Harmonic Motion)
- **Parameters**: Crank pin radius $r = 40.0\text{ mm} = 0.040\text{ m}$.
- **Reciprocating Stroke**:
  $$S = 2r = 80.0\text{ mm} = 0.080\text{ m}$$
- **Exact Kinematic Equations**:
  For crank angle $\theta = \omega t$:
  - Linear Displacement:
    $$x(t) = r \cos(\omega t)$$
  - Linear Velocity:
    $$v(t) = \frac{dx}{dt} = -r\omega \sin(\omega t)$$
  - Linear Acceleration:
    $$a(t) = \frac{dv}{dt} = -r\omega^2 \cos(\omega t) = -\omega^2 x(t)$$
- **Mathematical Comparison with Slider-Crank**:
  - The Scotch Yoke produces **zero higher-order harmonics** ($a_2 = a_4 = \dots = 0$).
  - Perfect sinusoidal motion eliminates all secondary reciprocating inertia forces ($m r \lambda \omega^2$), resulting in drastically lower high-speed vibration compared to connecting rod engines.

### 23. Universal Joint (Cardan / Hooke's Joint)
- **Shaft Misalignment Angle**: $\alpha = 25.0^\circ = 0.4363\text{ rad}$.
- **Kinematic Angular Relationship**:
  $$\tan\theta_2 = \tan\theta_1 \cdot \cos\alpha$$
- **Instantaneous Angular Velocity Ratio**:
  $$\frac{\omega_2}{\omega_1} = \frac{\cos\alpha}{1 - \sin^2\alpha \cos^2\theta_1}$$
- **Velocity Ratio Bounds per Revolution**:
  - Maximum Velocity Ratio (at $\theta_1 = 0^\circ, 180^\circ$):
    $$\left(\frac{\omega_2}{\omega_1}\right)_{\max} = \frac{1}{\cos\alpha} = \sec(25^\circ) = 1.1034$$
    At $N_{\text{in}} = 120\text{ RPM} \implies N_{\text{out, max}} = 132.4\text{ RPM}$.
  - Minimum Velocity Ratio (at $\theta_1 = 90^\circ, 270^\circ$):
    $$\left(\frac{\omega_2}{\omega_1}\right)_{\min} = \cos\alpha = \cos(25^\circ) = 0.9063$$
    At $N_{\text{in}} = 120\text{ RPM} \implies N_{\text{out, min}} = 108.8\text{ RPM}$.
- **Velocity Ripple (Fluctuation Coefficient)**:
  $$k_v = \frac{\omega_{\max} - \omega_{\min}}{\omega_{\text{avg}}} = \sec\alpha - \cos\alpha = \sin\alpha \tan\alpha = \sin(25^\circ)\tan(25^\circ) \approx 0.197\quad (19.7\%)$$
  Produces dynamic torque ripple and torsional vibration at twice shaft speed ($2\omega_1$).

### 24. Chain and Sprocket Drive (ANSI #40 / ISO 08B Roller Chain)
- **Tooth Counts**: Drive Sprocket $Z_1 = 15$, Driven Sprocket $Z_2 = 30$.
- **Chain Pitch**: $p = 12.7\text{ mm} = 0.50\text{ inches} = 0.0127\text{ m}$.
- **Pitch Diameters**:
  $$D_{p1} = \frac{p}{\sin(180^\circ / Z_1)} = \frac{12.7}{\sin(12.0^\circ)} = \frac{12.7}{0.2079} = 61.08\text{ mm} = 0.06108\text{ m}$$
  $$D_{p2} = \frac{p}{\sin(180^\circ / Z_2)} = \frac{12.7}{\sin(6.0^\circ)} = \frac{12.7}{0.1045} = 121.50\text{ mm} = 0.12150\text{ m}$$
- **Velocity Ratio**:
  $$i = \frac{Z_2}{Z_1} = \frac{30}{15} = 2.0 : 1$$
- **Linear Pitch Line Velocity**:
  $$v = \frac{\pi D_{p1} N_{\text{in}}}{60} = \frac{\pi (0.06108) (120)}{60} = 0.3838\text{ m/s} = 383.8\text{ mm/s}$$
- **Chordal Action (Polygonal Effect)**:
  Because roller chain links wrap as chords across a polygon rather than a smooth circle, pitch radius oscillates between $r_{\max} = \frac{D_p}{2}$ and $r_{\min} = \frac{D_p}{2}\cos\left(\frac{180^\circ}{Z_1}\right)$:
  $$\frac{\Delta v}{v_{\max}} = 1 - \cos\left(\frac{180^\circ}{Z_1}\right) = 1 - \cos(12.0^\circ) = 1 - 0.9781 = 0.0219\quad (2.19\%)$$
  Chordal action creates periodic linear velocity ripples responsible for roller impact noise and drive tension spikes.

### 25. Ratchet and Pawl Mechanism (One-Way Stepping Indexer)
- **Ratchet Wheel Tooth Count**: $Z = 24$ asymmetric saw-teeth.
- **Index Step Angle**:
  $$\Delta\theta = \frac{360^\circ}{Z} = \frac{360^\circ}{24} = 15.0^\circ$$
- **Drive Kinematics**:
  - Driving pawl rocker oscillates with amplitude $\ge 15.0^\circ$ at $N_{\text{in}} = 120\text{ RPM}$.
  - Forward stroke advances ratchet wheel by exactly 1 tooth pitch ($15^\circ$).
  - Return stroke slips over tooth flank back-angle while sprung holding pawl prevents retrograde motion.
  - Overall step-down velocity ratio: $i = Z = 24 : 1$.
  - Mean indexed speed: $N_{\text{out}} = \frac{N_{\text{in}}}{24} = \frac{120}{24} = 5.0\text{ RPM}$.
- **Self-Locking Engagement Condition**:
  The contact flank angle $\beta$ and pawl pivot radius are designed such that reverse driving torque creates a normal reaction force whose vector passes inside the pawl pivot center:
  $$\tan\beta \le \mu_{\text{static}}$$
  guaranteeing positive geometric lock against reverse rotation without slipping.

### 26. Lead Screw and Nut (Acme Power Screw)
- **Geometry**: Nominal screw diameter $d = 20.0\text{ mm} = 0.020\text{ m}$, Pitch $p = 6.0\text{ mm} = 0.006\text{ m}$, Single-start Acme thread (Lead $L = p = 6.0\text{ mm}$, flank half-angle $\alpha_n = 14.5^\circ$).
- **Mean Pitch Diameter**:
  $$d_2 = d - 0.5 p = 20.0 - 3.0 = 17.0\text{ mm} = 0.017\text{ m}$$
- **Thread Lead Angle ($\lambda$)**:
  $$\lambda = \arctan\left(\frac{L}{\pi d_2}\right) = \arctan\left(\frac{6.0}{\pi \cdot 17.0}\right) = \arctan(0.1123) = 6.411^\circ$$
- **Linear Carriage Velocity**:
  $$v = \frac{N_{\text{in}} \cdot L}{60} = \frac{120 \cdot 0.006}{60} = 0.012\text{ m/s} = 12.0\text{ mm/s}$$
- **Mechanical Advantage & Axial Thrust Multiplication**:
  Under input motor torque $T_{\text{in}} = 5.0\text{ N}\cdot\text{m}$:
  - Ideal lossless thrust force:
    $$F_{\text{ideal}} = \frac{2\pi \cdot T_{\text{in}}}{L} = \frac{2\pi \cdot 5.0}{0.006} = 5236.0\text{ N} \approx 5.24\text{ kN}$$
  - Screw Thread Efficiency (with steel-on-bronze friction $\mu = 0.15$):
    Virtual friction angle $\rho'$:
    $$\tan\rho' = \frac{\mu}{\cos\alpha_n} = \frac{0.15}{\cos(14.5^\circ)} = \frac{0.15}{0.9681} = 0.1549 \implies \rho' = 8.807^\circ$$
    Efficiency:
    $$\eta = \frac{\tan\lambda}{\tan(\lambda + \rho')} = \frac{\tan(6.411^\circ)}{\tan(6.411^\circ + 8.807^\circ)} = \frac{0.1123}{\tan(15.218^\circ)} = \frac{0.1123}{0.2720} \approx 41.3\%$$
  - Actual Delivered Axial Thrust:
    $$F_{\text{actual}} = \eta \cdot F_{\text{ideal}} = 0.413 \cdot 5236\text{ N} \approx 2162\text{ N} \approx 2.16\text{ kN}$$
  - Force Multiplication Factor:
    $$\frac{F_{\text{actual}}}{T_{\text{in}}} = \frac{2162\text{ N}}{5\text{ N}\cdot\text{m}} = 432.4\text{ N / (N}\cdot\text{m)}$$
- **Self-Locking Verification**:
  Because the lead angle $\lambda = 6.411^\circ$ is strictly less than the virtual friction angle $\rho' = 8.807^\circ$ ($\lambda < \rho'$):
  The drive is **inherently self-locking**; axial loads cannot back-drive the screw.

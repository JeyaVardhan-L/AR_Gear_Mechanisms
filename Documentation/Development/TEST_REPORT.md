# MECH-XR — AUTOMATED TEST SUITE EXECUTION REPORT

**Project**: MECH-XR Augmented Reality Mechanical Engineering Learning Application  
**Engine**: Unity 6.3 LTS (`6000.3.27f2`)  
**Test Harness**: `MechXR.Editor.SceneSetupTool.BuildMainSceneAndTest`  
**Target Device**: Xiaomi POCO X6 5G (Android 16, ARM64)  
**Verification Date**: November 5, 2026  
**Result**: 71 / 71 TESTS PASSED (100% SUCCESS, EXIT CODE 0)  

---

## 1. COMPREHENSIVE TEST RESULTS SUMMARY

```
==================================================
[MechXR] All automated Phase 4, Phase 4.1, Phase 5 & Phase 6.1 engineering tests completed!
==================================================
```

### 1.1 Original Kinematic Baseline Tests (TEST-01 to TEST-20)
| Test ID | Test Description | Expected | Measured | Status |
|:---:|:---|:---:|:---:|:---:|
| `TEST-01` | Spur Gear Velocity Ratio | $2.00 : 1$ | $2.00 : 1$ | **PASS** |
| `TEST-02` | Spur Gear Output Speed | $60.0\text{ RPM}$ | $60.0\text{ RPM}$ | **PASS** |
| `TEST-03` | Spur Gear Assembly Component Count | $\ge 7$ | 9 | **PASS** |
| `TEST-04` | Bevel Gear Velocity Ratio | $2.00 : 1$ | $2.00 : 1$ | **PASS** |
| `TEST-05` | Bevel Gear Output Speed | $60.0\text{ RPM}$ | $60.0\text{ RPM}$ | **PASS** |
| `TEST-06` | Bevel Gear Assembly Component Count | $\ge 7$ | 9 | **PASS** |
| `TEST-07` | Worm & Wheel Drive Ratio | $30.00 : 1$ | $30.00 : 1$ | **PASS** |
| `TEST-08` | Worm & Wheel Output Speed | $4.0\text{ RPM}$ | $4.0\text{ RPM}$ | **PASS** |
| `TEST-09` | Worm Gear Component Count | $\ge 8$ | 13 | **PASS** |
| `TEST-10` | Planetary Gear Velocity Ratio | $4.00 : 1$ | $4.00 : 1$ | **PASS** |
| `TEST-11` | Planetary Gear Output Speed | $30.0\text{ RPM}$ | $30.0\text{ RPM}$ | **PASS** |
| `TEST-12` | Planetary Gear Component Count | $\ge 7$ | 10 | **PASS** |
| `TEST-13` | Rack and Pinion Pitch Ratio | $20.0\text{ T/m}$ | $20.0\text{ T/m}$ | **PASS** |
| `TEST-14` | Rack Linear Pitch Velocity | $\approx 439.8\text{ mm/s}$ | $439.8\text{ mm/s}$ | **PASS** |
| `TEST-15` | Rack and Pinion Component Count | $\ge 6$ | 8 | **PASS** |
| `TEST-16` | Belt and Pulley Velocity Ratio | $2.00 : 1$ | $2.00 : 1$ | **PASS** |
| `TEST-17` | Belt and Pulley Output Speed | $60.0\text{ RPM}$ | $60.0\text{ RPM}$ | **PASS** |
| `TEST-18` | Belt and Pulley Component Count | $\ge 6$ | 10 | **PASS** |
| `TEST-19` | Inspectable Component Detach & Reassemble | Detached & Reattached | True / True | **PASS** |
| `TEST-20` | Animated Exploded View State Toggle | Exploded & Collapsed | True / True | **PASS** |

### 1.2 TextMeshPro Typography Verification (TEST-21)
| Test ID | Test Description | Measured Value | Requirement | Status |
|:---:|:---|:---:|:---:|:---:|
| `TEST-21` | Canvas TMP Hierarchy Font Asset Binding | 123 / 123 | 100% Non-Null Font Asset | **PASS** |

### 1.3 Phase 4.1 Screen State Machine & Transition Tests
| Test ID | Test Description | Condition Verified | Status |
|:---:|:---|:---|:---:|
| `TEST-P4.1-01` | Clean Startup State | Library active; AR HUD, scanning, bottom bar inactive | **PASS** |
| `TEST-P4.1-02` | Mechanism Library Catalog | 14 mechanism cards present in ScrollView with valid EXPLORE buttons | **PASS** |
| `TEST-P4.1-03` | Library-to-AR Transition | Selecting any of the 14 mechanisms completely hides Library and enters ARScanning | **PASS** |
| `TEST-P4.1-04` | ARPlaced State Transition | Hides scanning instruction, enables mechanism controls, verifies active anchor | **PASS** |
| `TEST-P4.1-05` | Component Inspection State | Displays dedicated inspection card, hides bottom dock controls | **PASS** |
| `TEST-P4.1-06` | Close Inspection Return | Restores ARPlaced state and normal mechanism controls | **PASS** |
| `TEST-P4.1-07` | Return to Library Reset | Destroys placed assembly, deactivates AR panels, resets state | **PASS** |
| `TEST-P4.1-08` | Replace Mechanism Cycle | Removes existing model and re-enters scanning without going to Library | **PASS** |
| `TEST-P4.1-09` | 10-Cycle Duplicate Guard | 10 continuous transitions produce zero duplicate headers or topbars | **PASS** |
| `TEST-P4.1-10` | Exploded View State Machine | Clean toggle between EXPLODE and REASSEMBLE states | **PASS** |

### 1.4 Phase 5 Controls, Scientific Lab & New Mechanism Tests
| Test ID | Test Description | Measured Value | Requirement | Status |
|:---:|:---|:---:|:---:|:---:|
| `TEST-P5-01` | Speed Controls Hit Target Test | All 5 elements active raycast targets | Container=T, Bg=T, Fill=T, Handle=T, Dock=T | **PASS** |
| `TEST-P5-02` | Velocity Mapping & Steppers | Exact matching across 30, 60, 120, 240, 300 RPM | Stepper $\pm 10\text{ RPM}$, $\omega = 12.566\text{ rad/s}$ | **PASS** |
| `TEST-P5-03` | Motion State Transitions | Pause, Resume, Reverse, Reset | Fully responsive | **PASS** |
| `TEST-P5-04` | Touch Pinch-to-Zoom Scaling | Clamped in $[0.4\times, 2.5\times]$ | Scale slider removed from dock | **PASS** |
| `TEST-P5-05` | SI Energy & Torque Equations | $P_{in}=125.66\text{ W}$, $P_{out}=119.38\text{ W}$, $T_{out}=19.00\text{ N}\cdot\text{m}$, $P_{loss}=6.28\text{ W}$ | Error $< 0.1\text{ W}$ | **PASS** |
| `TEST-P5-06` | Scientific Experiment Presets | High Speed, High Torque, Industrial, Low Eff | Parameter updates verified | **PASS** |
| `TEST-P5-07` | A/B Snapshot Comparison | Baseline capture & live delta tracking | $\Delta\text{RPM}, \Delta P, \Delta\text{Loss}$ verified | **PASS** |
| `TEST-P5-08` | Mechanism 07 (Simple Gear Train) | $i = 2.0:1$, Idler independence ($Z_2=28$) | $N_{out} = 60.0\text{ RPM}$ | **PASS** |
| `TEST-P5-09` | Mechanism 08 (Compound Gear Train) | $i = 6.0:1$ dual-stage ($2.0 \times 3.0$) | $N_{out} = 20.0\text{ RPM}$ | **PASS** |
| `TEST-P5-10` | Mechanism 09 (Reverted Gear Train) | $i = 3.0:1$ coaxial reducer, $a_1=a_2=105.0\text{ mm}$ | $N_{out} = 40.0\text{ RPM}$ | **PASS** |
| `TEST-P5-11` | Mechanism 10 (Internal Gear Pair) | $i = 3.0:1$ annular ring, identical rotation | $N_{out} = 40.0\text{ RPM}$ | **PASS** |
| `TEST-P5-12` | Mechanism 11 (Helical Gear Pair) | Helix angle $\beta = 20^\circ$, axial thrust vectoring | $i = 2.0:1$, Parallel shafts | **PASS** |
| `TEST-P5-13` | Mechanism 12 (Herringbone Gear Pair) | Double-helical chevrons, net axial thrust cancellation | $F_{a,\text{net}} = 0.0\text{ N}$ | **PASS** |
| `TEST-P5-14` | Mechanism 13 (Automotive Differential) | Straight ($40/40\text{ RPM}$), Turn ($20/60\text{ RPM}$), Willis law | $(\omega_L + \omega_R)/2 = 40\text{ RPM}$ | **PASS** |
| `TEST-P5-15` | Mechanism 14 (Harmonic Drive) | Strain wave $i = 24.0:1$, counter-rotating $5.0\text{ RPM}$ | Zero backlash verified | **PASS** |
| `TEST-P5-16` | Full 14-Mechanism Catalog | 14 / 14 mechanisms placed and operational | $100\%$ operational | **PASS** |

### 1.5 Phase 6 Expansion & Kinematic Verification Tests (TEST-P6-01 to TEST-P6-14)
| Test ID | Test Description | Measured Value | Requirement | Status |
|:---:|:---|:---:|:---:|:---:|
| `TEST-P6-01` | Mechanism 15 (Four-Stroke IC Engine) | $720^\circ$ cycle, 4 strokes, Camshaft $= 60.0\text{ RPM}$ | $N_{\text{cam}} = 0.5 \cdot N_{\text{crank}}$ | **PASS** |
| `TEST-P6-02` | Mechanism 16 (Slider-Crank Mechanism) | $r=40\text{ mm}, l=120\text{ mm}$, Stroke $= 80\text{ mm}$, $\lambda = 0.333$ | Exact trigonometric loop | **PASS** |
| `TEST-P6-03` | Mechanism 17 (Cam and Follower) | $R_b=30\text{ mm}$, Max Lift $h=16\text{ mm}$, SHM profile | Smooth harmonic rise/fall | **PASS** |
| `TEST-P6-04` | Mechanism 18 (Reciprocating Pump) | Bore $= 45\text{ mm}$, Stroke $= 50\text{ mm}$, $Q_{\text{th}} = 9.54\text{ L/min}$ | Flow rate $Q = \frac{\pi D^2 L N}{240}$ | **PASS** |
| `TEST-P6-05` | Mechanism 19 (Four-Bar Linkage) | Grashof Class I: $s+l=185 \le p+q=230\text{ mm}$ | Freudenstein loop solved | **PASS** |
| `TEST-P6-06` | Mechanism 20 (Crank-Rocker Mechanism) | Quick-Return Ratio $Q = 1.28:1$, Swing $\psi = 48.6^\circ$ | Asymmetric cycle time | **PASS** |
| `TEST-P6-07` | Mechanism 21 (Geneva Mechanism) | Maltese Cross 4-slot, $25\%$ motion, $75\%$ dwell | $N_{\text{out, avg}} = 30.0\text{ RPM}$ | **PASS** |
| `TEST-P6-08` | Mechanism 22 (Scotch Yoke Mechanism) | Pure SHM, Stroke $S = 2r = 80\text{ mm}$ | Zero 2nd harmonic unbalance | **PASS** |
| `TEST-P6-09` | Mechanism 23 (Universal Joint) | Misalignment $\alpha = 25^\circ$, Peak ratio $= 1.103\times$, Ripple $k_v = 19.7\%$ | $\tan\theta_2 = \tan\theta_1\cos\alpha$ | **PASS** |
| `TEST-P6-10` | Mechanism 24 (Chain & Sprocket Drive) | $Z_1=15, Z_2=30, i=2.0:1$, Linear $v=0.38\text{ m/s}$, Chordal $= 2.19\%$ | ANSI #40 pitch polygon | **PASS** |
| `TEST-P6-11` | Mechanism 25 (Ratchet and Pawl) | 24 Sawtooth teeth, $15.0^\circ$ pitch step, $i = 24:1$ | One-way indexing $5.0\text{ RPM}$ | **PASS** |
| `TEST-P6-12` | Mechanism 26 (Lead Screw and Nut) | Lead $L=6\text{ mm}$, Acme $29^\circ$, Self-locking $=$ True, Thrust $= 2162\text{ N}$ | Thrust multiplication $432\times$ | **PASS** |
| `TEST-P6-13` | Library Category Filtering System | Gears$=14/14$, Engines$=4/4$, Linkages$=4/4$, Drives$=4/4$, All$=26/26$ | Instantaneous dynamic filtering | **PASS** |
| `TEST-P6-14` | Full 26-Mechanism Catalog Verification | 26 / 26 mechanisms verified in catalog, AR placement, and kinematics | $100\%$ operational (26/26) | **PASS** |

### 1.6 Phase 6.1 Android Library Touch Scrolling Verification Tests (TEST-P6.1-01 to TEST-P6.1-10)
| Test ID | Test Description | Measured Value | Requirement | Status |
|:---:|:---|:---:|:---:|:---:|
| `TEST-P6.1-01` | ScrollRect Configuration | Vertical=True, Horizontal=False, MovementType=Elastic, Viewport=Viewport, Content=Content | Dedicated touch ScrollRect | **PASS** |
| `TEST-P6.1-02` | Content RectTransform & Layout | Pivot=(0.5, 1.0), VerticalLayoutGroup=True, ContentSizeFitter=PreferredSize | Dynamic vertical expansion | **PASS** |
| `TEST-P6.1-03` | Content Expansion vs Viewport | Content Height $= 4398\text{ px}$, Viewport $= 979\text{ px}$, Scroll Range $= 3419\text{ px}$ | Content $\ge 4000\text{ px} >$ Viewport | **PASS** |
| `TEST-P6.1-04` | Card Background Raycast Enablement | 26 / 26 mechanism cards have `Image.raycastTarget = true` | $100\%$ card drag touch capture | **PASS** |
| `TEST-P6.1-05` | Viewport & Content Margins Raycast | Viewport `Image.raycastTarget = true`, Content `Image.raycastTarget = true` | Touches in padding register drag | **PASS** |
| `TEST-P6.1-06` | Explore Buttons Touch Clickability | 26 / 26 `EXPLORE ▶` buttons verified with active raycastTarget and click events | Clickable without blocking drag | **PASS** |
| `TEST-P6.1-07` | Simulated Vertical Touch Drag Downwards | Drag upward across screen: normalized position sweeps from $1.000$ to $-0.143 < 0.50$ | Reveals all bottom cards | **PASS** |
| `TEST-P6.1-08` | Simulated Vertical Touch Drag Upwards | Drag downward across screen: normalized position restores from $-0.143$ to $1.120 \ge 0.80$ | Reveals card 0 at top | **PASS** |
| `TEST-P6.1-09` | Category Filter Dynamic Height & Reset | Engines $= 790\text{ px} <$ All $= 4398\text{ px}$, normalized scroll resets to $1.00$ | Dynamic resize & top-reset | **PASS** |
| `TEST-P6.1-10` | AR Navigation Return & Touch Readiness | AR selection hides library; return to library restores ScrollRect & all 26 cards | Complete state integrity | **PASS** |

---

## 2. PRODUCTION APK ARTIFACT SPECIFICATIONS

| Artifact Parameter | Value | Verification Tool / Command |
|:---|:---|:---|
| **File Path** | `Builds/MECH-XR-PHASE6-1.apk` | System File IO |
| **File Size** | $28,678,787\text{ bytes}$ ($27.35\text{ MB}$) | PowerShell `Get-Item` |
| **Package Name** | `com.mechxr.gearlearning` | `aapt dump badging` |
| **Version Code / Name** | `versionCode='1'`, `versionName='1.0'` | `aapt dump badging` |
| **Target Architecture** | `arm64-v8a` (64-bit ARM) | `aapt dump badging` |
| **Minimum SDK** | API 26 (Android 8.0 Oreo) | `aapt dump badging` |
| **Target SDK / Compile SDK**| API 36 (Android 16) | `aapt dump badging` |
| **Signature Scheme** | Scheme v2 (`APK Signature Scheme v2: true`) | `apksigner verify --verbose` |
| **AR Core Compatibility** | `android.hardware.camera.ar`, `com.google.ar.core.depth` | `aapt dump badging` |

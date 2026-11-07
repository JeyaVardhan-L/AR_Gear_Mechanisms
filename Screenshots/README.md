# MECH-XR — Application Visuals & Screen Gallery

This directory houses high-resolution captures of the **MECH-XR** Augmented Reality mobile interface and interactive 3D simulations.

---

## Showcase Screenshot Trio

The repository's root [`README.md`](../README.md) showcases three core application views:

| Filename | View Description | Key UI Elements Visible |
| :--- | :--- | :--- |
| **`01_catalog_browser.png`** | **Mechanism Library** | Category filter chips (`ALL`, `GEARS`, `ENGINES`, `LINKAGES`, `DRIVES`), vertical mechanism cards, tooth counts, kinematics badges. |
| **`02_ar_placement.png`** | **AR Plane Placement** | Real-world surface tracking, dynamic targeting reticle, placed 3D mechanical assembly, rotation and pinch-scale feedback. |
| **`03_experiment_lab.png`** | **Experiment Lab** | Live parameter sliders (RPM, Torque, Efficiency), thermodynamic Power/Loss channel plots, live SI telemetry readout. |

---

## How to Add or Update Screenshots

### Option 1: Direct File Copy from Smartphone
1. Capture screenshots on your Android smartphone (e.g. **Power + Volume Down**).
2. Transfer the captures from your phone's `Pictures/Screenshots/` folder to this directory (`Screenshots/`).
3. Rename the 3 files to match:
   - `01_catalog_browser.png`
   - `02_ar_placement.png`
   - `03_experiment_lab.png`

### Option 2: Android Debug Bridge (ADB)
With your Android phone connected via USB (**USB Debugging** enabled):

```bash
# 1. Capture and pull Catalog Browser
adb shell screencap -p /sdcard/01_catalog_browser.png
adb pull /sdcard/01_catalog_browser.png Screenshots/
adb shell rm /sdcard/01_catalog_browser.png

# 2. Capture and pull AR Surface Placement
adb shell screencap -p /sdcard/02_ar_placement.png
adb pull /sdcard/02_ar_placement.png Screenshots/
adb shell rm /sdcard/02_ar_placement.png

# 3. Capture and pull Experiment Lab
adb shell screencap -p /sdcard/03_experiment_lab.png
adb pull /sdcard/03_experiment_lab.png Screenshots/
adb shell rm /sdcard/03_experiment_lab.png
```



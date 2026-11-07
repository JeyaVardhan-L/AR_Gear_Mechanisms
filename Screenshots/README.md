# MECH-XR — Application Visuals & Screen Gallery

This directory houses high-resolution captures of the **MECH-XR** Augmented Reality mobile interface and interactive 3D simulations.

---

## Capturing In-App Visuals on Android

To capture real-time AR sessions from physical Android hardware:

### Option 1: Android Debug Bridge (ADB)
Connect your Android smartphone via USB with **USB Debugging** enabled:

```bash
# Capture full-resolution screen
adb shell screencap -p /sdcard/mechxr_capture.png

# Pull image to local Screenshots directory
adb pull /sdcard/mechxr_capture.png Screenshots/

# Remove temporary file from device
adb shell rm /sdcard/mechxr_capture.png
```

### Option 2: Physical Device Shortcut
- Press **Power + Volume Down** simultaneously.
- Transfer captures from your device's `Pictures/Screenshots/` directory.

---

## Recommended Showcase Views

1. **`01_catalog_browser.png`**: The 26-mechanism catalog view with category filters (`ALL`, `GEARS`, `ENGINES`, `LINKAGES`, `DRIVES`).
2. **`02_ar_surface_placement.png`**: ARCore plane detection highlighting the dynamic targeting reticle on a tabletop surface.
3. **`03_kinematic_simulation.png`**: A placed mechanical assembly (e.g. Planetary Gearbox or Four-Stroke Engine) running at live RPM.
4. **`04_component_inspection.png`**: An isolated gear or piston detached along its mechanical axis with the engineering metadata card.
5. **`05_experiment_lab_telemetry.png`**: The collapsible Experiment Lab drawer open, showing live RPM, torque, efficiency sliders, and dynamic Power/Loss graphs.


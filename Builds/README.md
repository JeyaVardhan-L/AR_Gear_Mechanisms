# MECH-XR Android Releases

This directory houses verified production release builds of the MECH-XR Augmented Reality Mechanical Engineering Laboratory.

---

## Latest Verified Production Build: Phase 6.1 (v1.1)

- **File**: `MECH-XR-PHASE6-1-v2.apk`
- **File Size**: 28,678,771 bytes (~27.35 MB)
- **Package ID**: `com.mechxr.gearlearning`
- **Version Code**: `2`
- **Version Name**: `1.1`
- **Architecture**: `arm64-v8a` (64-bit ARM)
- **Minimum Android SDK**: API 26 (Android 8.0 Oreo)
- **Target Android SDK**: API 36 (Android 16)
- **Unity Version**: Unity 6.3 LTS (`6000.3.27f2`)
- **AR Framework**: AR Foundation 6.0.8 + Google ARCore XR Plugin 6.0.8
- **Signature Scheme**: APK Signature Scheme v2 (Verified)
- **Key Features Included**:
  - Full 26-mechanism CAD catalog across 4 categories (Gears, Engines, Linkages, Drives).
  - Scientific Experiment Lab drawer with live RPM, Torque, and Efficiency controls.
  - Real-time thermodynamic Power/Loss, RPM, Torque, and Acceleration graphs.
  - Phase 6.1 physical Android mobile touch scrolling repair (dynamic layout rebuilder, `raycastTarget = true` event-routing).
  - Component inspection, axial detachment, and exploded views.

---

## Installation via Android Debug Bridge (ADB)

With your Android device connected via USB with Developer Mode & USB Debugging enabled:

```bash
# Push directly to internal Downloads folder
adb push Builds/MECH-XR-PHASE6-1-v2.apk /sdcard/Download/

# Or install directly via ADB (allows reinstall preserving app data)
adb install -r Builds/MECH-XR-PHASE6-1-v2.apk
```

---

## Sideloading via Mobile Browser / File Manager

1. Download or copy `MECH-XR-PHASE6-1-v2.apk` to your phone's `Downloads` folder.
2. Open your device's **Files** / **File Manager** app.
3. Tap `MECH-XR-PHASE6-1-v2.apk`.
4. If prompted, allow **Install unknown apps** for your file manager.
5. Tap **Install** (or **Update**).
6. Ensure **Google Play Services for AR (ARCore)** is installed on the target device.


# MECH-XR — ANDROID BUILD & DEPLOYMENT GUIDE (PHASE 6)

This guide documents the procedures for building, testing, verifying, and deploying the MECH-XR Android AR application with its full 26-mechanism catalog.

---

## 1. PREREQUISITES & ENVIRONMENT

- **Host Operating System**: Windows 11 (or 10) 64-bit
- **Game Engine**: Unity 6.3 LTS (`6000.3.27f2`)
- **Unity Path**: `C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Unity.exe`
- **Android Target**: Android 16 (API Level 36), Target SDK: 36, Min SDK: 26 (Android 8.0 Oreo)
- **Target Hardware**: Xiaomi POCO X6 5G (Redmi Note 13 Pro 5G / `23122PCD1I`)
- **Native ABI**: `arm64-v8a` (64-bit ARM)
- **Graphics API**: Vulkan (Primary) / OpenGLES3 (Fallback)
- **Render Pipeline**: Unity Built-in Render Pipeline
- **Signing**: APK Signature Scheme v2

---

## 2. PRODUCTION BUILD PROCEDURES

### 2.1 Automated Batchmode Build (Phase 6)
To compile `Builds/MECH-XR-PHASE6.apk` headlessly from PowerShell:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\Users\user\Downloads\AR" `
  -executeMethod MechXR.Editor.AndroidBuildTool.BuildAndroidApkPhase6 `
  -logFile "Builds/build_phase6.log"
```

### 2.2 Automated Kinematic & UI Test Suite
To execute the complete 61-test validation harness headlessly:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\Users\user\Downloads\AR" `
  -executeMethod MechXR.Editor.SceneSetupTool.BuildMainSceneAndTest `
  -logFile "Builds/test_phase6.log"
```

---

## 3. APK VERIFICATION PROCEDURES

### 3.1 Verify APK Metadata with AAPT
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\build-tools\36.0.0\aapt.exe" `
  dump badging "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6.apk"
```
**Expected Output:**
- `package: name='com.mechxr.gearlearning' versionCode='1' versionName='1.0'`
- `sdkVersion:'26'`
- `targetSdkVersion:'36'`
- `application-label:'MECH-XR'`
- `native-code: 'arm64-v8a'`

### 3.2 Verify Signature Scheme v2 with APKSigner
```powershell
$env:JAVA_HOME = "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK"
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\build-tools\36.0.0\apksigner.bat" `
  verify --verbose "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6.apk"
```
**Expected Output:**
- `Verifies: true`
- `Verified using v2 scheme (APK Signature Scheme v2): true`

---

## 4. PHYSICAL DEVICE DEPLOYMENT (POCO X6 5G)

### 4.1 Push APK to Device Downloads
Connect your POCO X6 5G via USB with USB Debugging enabled, then execute:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" `
  push "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6.apk" /sdcard/Download/MECH-XR-PHASE6.apk
```

### 4.2 Direct Install via ADB
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" `
  install -r "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6.apk"
```

### 4.3 Launch Application via ADB
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" `
  shell am start -n com.mechxr.gearlearning/com.unity3d.player.UnityPlayerGameActivity
```

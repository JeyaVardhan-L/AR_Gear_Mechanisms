# MECH-XR — PHASE 6.1: ANDROID LIBRARY TOUCH SCROLLING REPAIR REPORT

**Project**: MECH-XR Augmented Reality Mechanical Engineering Learning Application  
**Engine**: Unity 6.3 LTS (`6000.3.27f2`)  
**Target Hardware**: Xiaomi POCO X6 5G (Redmi Note 13 Pro 5G / `23122PCD1I`, Android 16 / API 36, `arm64-v8a`)  
**Production APK**: `Builds/MECH-XR-PHASE6-1.apk` ($28,678,787\text{ bytes}$, $27.35\text{ MB}$)  
**Verification Date**: November 5, 2026  
**Status**: COMPLETE — ALL 71 AUTOMATED TESTS PASSED (100% SUCCESS)

---

## 1. EXECUTIVE SUMMARY & BUG DIAGNOSIS

During physical testing of MECH-XR Phase 6 on the Xiaomi POCO X6 5G, the 26-mechanism library rendered clearly, but **vertical touchscreen drag gestures were completely unresponsive**:
1. Swiping up/down on the mechanism cards did not move the card list.
2. Only the first 6–7 mechanism cards were visible; the remaining 19–20 mechanisms could not be scrolled into view.
3. Category filters (`ALL`, `GEARS`, `ENGINES`, `LINKAGES`, `DRIVES`) stayed in place, but switching categories did not adapt the scrollable container dynamically.

### Root Cause Analysis (Physical Touch Architecture)

Unity's UI `ScrollRect` does not rely on raw screen touches; instead, it relies strictly on `EventSystem` raycasts processed through `GraphicRaycaster`:
1. **Pass-Through Touch Fall-Through on Cards**: In `UIManager.CreateMenuCard`, the card container was initialized with `img.raycastTarget = false;`. Because the card background had no raycast target, touching 80% of the screen area (the card background, text labels, borders) produced **zero hits** in `GraphicRaycaster.Raycast()`. The `EventSystem` concluded no UI element was touched, so `OnBeginDrag` and `OnDrag` were never invoked on `ScrollRect`.
2. **Missing Raycast Target on Viewport & Margins**: The `Viewport` and `Content` container had no transparent raycast target graphics. Touches landing in the 14px padding between cards or in margins fell through completely.
3. **EditMode & Pre-serialization Content Height Collapse**: `ContentSizeFitter` calculates layout lazily during late update. When cards were added during editor setup, `contentRt.sizeDelta` was saved as `{0, 0}` in `MainScene.unity`. Upon launch, `ScrollRect` evaluated content height as $\le$ viewport height, locking vertical bounds.
4. **Category Filtering Scroll Desynchronization**: When filtering down from 26 cards to 4 cards (Engines), `verticalNormalizedPosition` was not reset and layout was not rebuilt immediately, causing the user to remain stranded in out-of-bounds negative space.

---

## 2. SURGICAL CODE REPAIRS

### 2.1 Fixed Header & Fixed Category Filter Bar with Independent ScrollView (`UIManager.cs`)
The library hierarchy was structured with strict vertical separation:
- **Fixed Top Header**: Spans $y = [0.88, 1.00]$ (App title, subtitle, status badges).
- **Fixed Category Filter Bar**: Spans $y = [0.815, 0.865]$ (`ALL (26)`, `GEARS (14)`, `ENGINES (4)`, `LINKAGES (4)`, `DRIVES (4)`).
- **Scrollable Mechanism Cards Viewport**: Spans $y = [0.020, 0.805]$, providing an expansive 78.5% screen viewport dedicated strictly to vertical card scrolling.

### 2.2 Complete Raycast Surface Coverage (`UIManager.cs`)
1. **Card Backgrounds**: Set `card.GetComponent<Image>().raycastTarget = true;` across all 26 cards. Every touch anywhere on a card now reliably triggers `ScrollRect.OnBeginDrag` and `ScrollRect.OnDrag`.
2. **Viewport & Content Margins**: Added transparent `Image` graphics (`color = Color.clear`, `raycastTarget = true`) to both `LibraryScrollView/Viewport` and `LibraryScrollView/Viewport/Content`. Touches in card padding and margins are now captured and routed to the scroll controller.
3. **Independent Button Dispatch**: `EXPLORE ▶` buttons retain their own `Button` component with `raycastTarget = true`. On mobile touchscreens, Unity UI automatically distinguishes between a stationary tap (which fires `Button.onClick`) and a dragging motion (which transfers control to `ScrollRect.OnDrag`).

### 2.3 Layout Bounds & Dynamic Rebuilding (`UIManager.cs`)
1. In `BuildLibraryScreen()`:
   - Configured `VerticalLayoutGroup` with `padding = RectOffset(10, 10, 14, 20)`, `spacing = 14f`, `childControlHeight = false`, `childForceExpandHeight = false`.
   - Added `ContentSizeFitter` with `verticalFit = FitMode.PreferredSize`.
   - Explicitly invoked `LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect)` and `Canvas.ForceUpdateCanvases()`, expanding the content height to **$4398\text{ px}$** (well exceeding the $979\text{ px}$ viewport, producing a $3419\text{ px}$ scrollable travel range).
2. In `FilterLibraryCategory(int cat)`:
   - After toggling active cards (`cat == 0` for All, `cat == 1` for Gears, `cat == 2` for Engines, `cat == 3` for Linkages, `cat == 4` for Drives), immediately executes `LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect)`.
   - Invokes `libraryScrollRect.StopMovement()` and resets `libraryScrollRect.verticalNormalizedPosition = 1f` (smoothly returning the user to the first card of the selected category).

### 2.4 Automotive Differential Collider & EditMode Cleanup (`AutomotiveDifferential.cs` & `ARPlacementManager.cs`)
1. Added missing `MeshCollider` to `Carrier_Housing` before attaching `InspectableComponent` (which enforces `[RequireComponent(typeof(Collider))]`).
2. Implemented `SafeDestroy(GameObject go)` in `ARPlacementManager.cs` to prevent EditMode `Destroy` warnings during automated test runs.

---

## 3. AUTOMATED TEST SUITE VERIFICATION (PHASE 6.1)

All 10 dedicated Phase 6.1 touch scrolling test cases passed with zero errors:

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

**Total Suite Statistics**: **71 / 71 tests passing** across baseline kinematics (20), typography (1), screen state transitions (10), experiment lab (16), 26-mechanism kinematics (14), and mobile touch scrolling (10).

---

## 4. PRODUCTION APK SPECIFICATIONS & VERIFICATION

```
File Path:            Builds/MECH-XR-PHASE6-1.apk
File Size:            28,678,787 bytes (27.35 MB)
Last Modified:        November 5, 2026 13:30:24
Package Name:         com.mechxr.gearlearning
Target Architecture:  arm64-v8a (64-bit ARM)
Target SDK:           API 36 (Android 16)
Minimum SDK:          API 26 (Android 8.0 Oreo)
Signature:            Scheme v2 Verified (Apk Signature Scheme v2: true)
AR Capabilities:      android.hardware.camera.ar, com.google.ar.core.depth
```

---

## 5. PHYSICAL DEVICE DEPLOYMENT INSTRUCTIONS

To push the Phase 6.1 APK directly to the Xiaomi POCO X6 5G:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" push "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6-1.apk" /sdcard/Download/MECH-XR-PHASE6-1.apk
```

To install directly onto the device:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" install -r "C:\Users\user\Downloads\AR\Builds\MECH-XR-PHASE6-1.apk"
```


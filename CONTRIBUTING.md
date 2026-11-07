# Contributing to MECH-XR

Thank you for your interest in contributing to **MECH-XR**, an open-source Augmented Reality mechanical engineering platform built on Unity and Google ARCore.

We welcome contributions from mechanical engineers, graphics programmers, AR developers, and educators. This guide outlines development standards, architectural conventions, and the workflow for proposing changes.

---

## 1. Development Prerequisites

To ensure reproducible builds and prevent package discrepancies:
- **Unity Engine**: `Unity 6000.3.27f2` (Unity 6.3 LTS).
- **Target Platform**: Android Build Support (OpenJDK, Android SDK & NDK Tools via Unity Hub).
- **Target Architecture**: `arm64-v8a` (64-bit ARM).
- **IDE**: Visual Studio 2022, Visual Studio Code (with C# Dev Kit), or JetBrains Rider.
- **Git LFS**: Installed and initialized (`git lfs install`).

---

## 2. Repository Architecture & Conventions

### Key Directories
- `Assets/Scripts/Mechanisms/`: Standalone kinematic mechanism controllers inheriting from `MechanismBase.cs`.
- `Assets/Scripts/Procedural/`: Procedural mesh generation algorithms (`GearMeshBuilder.cs`).
- `Assets/Scripts/UI/`: Finite state machine, Experiment Lab drawer, graph drawers, and touch event routing.
- `Assets/Scripts/AR/`: AR Foundation surface tracking, plane managers, and 6-DoF placement.
- `Assets/Editor/`: Automated testing (`SceneSetupTool.cs`) and headless APK build tooling (`AndroidBuildTool.cs`).

### Code Style Guidelines
- **C# Language Level**: C# 9.0 (.NET Standard 2.1).
- **Naming Conventions**:
  - `PascalCase` for public classes, methods, properties, and enums.
  - `camelCase` with leading underscore (`_fieldName`) or simple `camelCase` for private fields.
  - Constants: `UPPER_CASE` or `k_PascalCase`.
- **Unity Best Practices**:
  - Cache component lookups (`GetComponent<T>()`) in `Awake()` or `Start()`.
  - Use `Time.deltaTime` for all time-integrated rotational velocities to ensure frame-rate independence.
  - Never instantiate heavy 3D models at runtime without pooling or procedural reuse.
  - Ensure every interactive sub-part has a valid `Collider` (MeshCollider or BoxCollider) before attaching `InspectableComponent`.

---

## 3. Adding a New Mechanism

To add a new mechanical assembly to the catalog:
1. **Create Controller**:
   Create a new class under `Assets/Scripts/Mechanisms/` deriving from `MechanismBase`:
   ```csharp
   public class CustomMechanism : MechanismBase
   {
       public override string MechanismName => "Custom Mechanism";
       public override string CategoryTitle => "Specialized Transmission";
       public override float GearRatio => 2.5f;
       public override float OutputRPM => currentInputRPM / GearRatio;

       public override void BuildAssembly() { ... }
       public override void ResetKinematics() { ... }
       public override string GetCalculationSummary() { ... }
       public override string GetEngineeringExplanation() { ... }
       public override string GetFormulaBreakdown() { ... }
   }
   ```
2. **Synthesize Procedural Geometry**:
   Implement any specialized gear teeth, linkages, or cams in `GearMeshBuilder.cs` using pure algorithmic vertices and triangles.
3. **Configure Colliders & Metadata**:
   Attach `InspectableComponent` to each moving part, specifying part name, material description, and explosion vector.
4. **Register in UI**:
   Add the mechanism type to the `mechanismPrefabs` array in `UIManager.cs`.
5. **Add Automated Test**:
   Register a test case in `SceneSetupTool.cs` verifying ratio, component count, and kinematic calculations.

---

## 4. Testing & Verification

Before submitting changes, all **71 automated tests** must pass without errors or warnings:

```powershell
# Execute automated test suite via Unity batchmode
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath . `
  -executeMethod MechXR.Editor.SceneSetupTool.BuildMainSceneAndTest `
  -logFile test.log

# Check return code (must be 0)
$LASTEXITCODE
```

---

## 5. Building the Android Release

To compile and verify the Android production APK:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.27f2\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath . `
  -executeMethod MechXR.Editor.AndroidBuildTool.BuildAndroidApkPhase6_1 `
  -logFile build.log
```

The resulting signed APK will be generated at `Builds/MECH-XR-PHASE6-1-v2.apk`.

---

## 6. Pull Request Process

1. Fork the repository and create your feature branch from `main`:
   ```bash
   git checkout -b feature/my-new-mechanism
   ```
2. Commit your changes with clear, descriptive commit messages:
   ```bash
   git commit -m "feat(mechanisms): implement epicyclic cycloidal drive"
   ```
3. Run the automated test suite and verify `0` failures.
4. Ensure `.meta` files for all added, modified, or deleted assets are committed.
5. Push to your fork and submit a Pull Request to `JeyaVardhan-L/AR_Gear_Mechanisms` on `main`.

---

## 7. License

By contributing to MECH-XR, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).


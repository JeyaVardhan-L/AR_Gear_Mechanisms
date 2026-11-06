using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Spur Gear Pair Mechanism.
    /// Demonstrates external meshing between two parallel shafts with opposite rotation directions.
    /// Features:
    /// - 20-tooth solid driver pinion (Electric Blue)
    /// - 40-tooth spoked driven gear (Industrial Gold, 4 spoke cutouts for unmistakable rotation visibility)
    /// - 2 ground steel transmission shafts
    /// - 2 cast pillow block bearing pedestals
    /// - 2 ball bearing assemblies
    /// - Precision machined aluminum machine bed
    /// - Exploded view & individual component detachment
    /// Gear Ratio: 2:1 (Speed Reduction, 2x Torque Multiplier).
    /// </summary>
    public class SpurGearPair : MechanismBase
    {
        [Header("Gear Parameters")]
        [SerializeField] private int driverTeeth = 20;
        [SerializeField] private int drivenTeeth = 40;
        [SerializeField] private float moduleMeters = 0.0035f; // 3.5mm module -> Driver: 70mm, Driven: 140mm
        [SerializeField] private float faceWidth = 0.016f;     // 16mm tooth width
        [SerializeField] private float boreRadius = 0.006f;    // 6mm shaft radius

        [Header("Transforms (Auto-generated if null)")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform drivenRotor;
        [SerializeField] private Material gearMaterialDriver;
        [SerializeField] private Material gearMaterialDriven;
        [SerializeField] private Material steelShaftMaterial;
        [SerializeField] private Material baseplateMaterial;

        private float currentDriverAngle = 0f;
        private float currentDrivenAngle = 0f;
        private float initialDrivenPhaseOffset = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => driverTeeth;
        public override int DrivenTeeth => drivenTeeth;
        public override string MechanismName => "Spur Gear Pair";
        public override string CategoryTitle => "Parallel Shaft Transmission";
        public override float GearRatio => (float)drivenTeeth / driverTeeth; // 40 / 20 = 2.0
        public override float OutputRPM => currentInputRPM / GearRatio;      // 120 / 2 = 60 RPM

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void LoadMaterials()
        {
            if (gearMaterialDriver == null)
                gearMaterialDriver = Resources.Load<Material>("Art/Materials/Mat_Gear_Driver") ??
                                     CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f);
            if (gearMaterialDriven == null)
                gearMaterialDriven = Resources.Load<Material>("Art/Materials/Mat_Gear_Driven") ??
                                     CreateMaterial(new Color(0.92f, 0.72f, 0.20f), 0.6f, 0.8f);
            if (steelShaftMaterial == null)
                steelShaftMaterial = Resources.Load<Material>("Art/Materials/Mat_Steel_Shaft") ??
                                     CreateMaterial(new Color(0.78f, 0.82f, 0.86f), 0.85f, 0.85f);
            if (baseplateMaterial == null)
                baseplateMaterial = Resources.Load<Material>("Art/Materials/Mat_Baseplate") ??
                                    CreateMaterial(new Color(0.14f, 0.16f, 0.20f), 0.35f, 0.5f);
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;
            LoadMaterials();
            float r1 = moduleMeters * driverTeeth * 0.5f; // 0.035m = 35mm
            float r2 = moduleMeters * drivenTeeth * 0.5f; // 0.070m = 70mm
            float centerDist = r1 + r2;                   // 105mm
            float elevationY = r2 + 0.015f;               // Elevate axes so driven gear clears base

            initialDrivenPhaseOffset = (180f / drivenTeeth);

            // 1. Machine Bedplate (Base)
            GameObject baseGo = new GameObject("Baseplate");
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localPosition = new Vector3(centerDist * 0.5f - r1, 0, 0);

            MeshFilter mfBase = baseGo.AddComponent<MeshFilter>();
            MeshRenderer mrBase = baseGo.AddComponent<MeshRenderer>();
            mfBase.mesh = GearMeshBuilder.BuildBasePlateMesh(centerDist + 0.10f, 0.11f, 0.014f);
            mrBase.material = baseplateMaterial;

            BoxCollider colBase = baseGo.AddComponent<BoxCollider>();
            colBase.size = new Vector3(centerDist + 0.10f, 0.014f, 0.11f);
            colBase.center = new Vector3(0, -0.007f, 0);

            InspectableComponent inspBase = baseGo.AddComponent<InspectableComponent>();
            inspBase.SetMetadata("Anodized Tooling Bedplate",
                "Precision-ground CNC machined datum bed maintaining exact center distance (a = 105.0 mm).",
                "Material: 6061-T6 Aluminum | Ground Flatness: ±0.01 mm | Black Anodized");
            inspBase.SetExplosionVector(Vector3.down, 0.05f, 0.08f);

            // 2. Driver Shaft & Bearing Pedestal
            Vector3 driverAxisPos = new Vector3(-r1, elevationY, 0);

            // Driver Pillow Block (Bearing Mount)
            GameObject dMountGo = new GameObject("Driver_PillowBlock");
            dMountGo.transform.SetParent(transform, false);
            dMountGo.transform.localPosition = new Vector3(driverAxisPos.x, 0, -0.024f);

            MeshFilter mfDMount = dMountGo.AddComponent<MeshFilter>();
            MeshRenderer mrDMount = dMountGo.AddComponent<MeshRenderer>();
            mfDMount.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.045f, elevationY, 0.016f, boreRadius * 1.5f);
            mrDMount.material = baseplateMaterial;
            BoxCollider colDMount = dMountGo.AddComponent<BoxCollider>();
            colDMount.size = new Vector3(0.045f, elevationY, 0.016f);
            colDMount.center = new Vector3(0, elevationY * 0.5f, 0);

            InspectableComponent inspDMount = dMountGo.AddComponent<InspectableComponent>();
            inspDMount.SetMetadata("Driver Pillow Block Pedestal",
                "Rigid bearing support resisting separating radial gear tooth reaction forces.",
                "Material: Cast Iron GGG40 | Precision H7 Bearing Seat");
            inspDMount.SetExplosionVector(new Vector3(-0.5f, 0, -1f), 0.07f, 0.10f);

            // Driver Ball Bearing
            GameObject dBearingGo = new GameObject("Driver_BallBearing");
            dBearingGo.transform.SetParent(transform, false);
            dBearingGo.transform.localPosition = new Vector3(driverAxisPos.x, elevationY, -0.024f);
            MeshFilter mfDBear = dBearingGo.AddComponent<MeshFilter>();
            MeshRenderer mrDBear = dBearingGo.AddComponent<MeshRenderer>();
            mfDBear.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrDBear.material = steelShaftMaterial;
            BoxCollider colDBear = dBearingGo.AddComponent<BoxCollider>();
            colDBear.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspDBear = dBearingGo.AddComponent<InspectableComponent>();
            inspDBear.SetMetadata("Driver Deep-Groove Ball Bearing",
                "Provides low-friction radial support for input shaft at 120 RPM.",
                "ABEC-5 Precision | Dynamic Radial Load: 4.8 kN");
            inspDBear.SetExplosionVector(Vector3.back, 0.09f, 0.12f);

            // Driver Rotor Container (Rotates around local Z)
            GameObject dRotorGo = new GameObject("Driver_Rotor");
            dRotorGo.transform.SetParent(transform, false);
            dRotorGo.transform.localPosition = driverAxisPos;
            driverRotor = dRotorGo.transform;

            // Driver Shaft
            GameObject dShaftGo = new GameObject("Driver_Shaft");
            dShaftGo.transform.SetParent(driverRotor, false);
            dShaftGo.transform.localPosition = new Vector3(0, 0, -0.012f);
            MeshFilter mfDShaft = dShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrDShaft = dShaftGo.AddComponent<MeshRenderer>();
            mfDShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, 0.075f, 24, false);
            mrDShaft.material = steelShaftMaterial;
            CapsuleCollider colDShaft = dShaftGo.AddComponent<CapsuleCollider>();
            colDShaft.radius = boreRadius;
            colDShaft.height = 0.075f;
            colDShaft.direction = 2; // Z-axis

            InspectableComponent inspDShaft = dShaftGo.AddComponent<InspectableComponent>();
            inspDShaft.SetMetadata("Input Drive Shaft",
                "Transmits input motor torque directly to the driver pinion.",
                "Material: 42CrMo4 Hardened Steel | Tolerance: h6 Ground Finish");
            inspDShaft.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Driver Pinion Gear (20T, Solid with Hub)
            GameObject dGearGo = new GameObject("Driver_Gear_20T");
            dGearGo.transform.SetParent(driverRotor, false);
            dGearGo.transform.localPosition = Vector3.zero;
            MeshFilter mfDGear = dGearGo.AddComponent<MeshFilter>();
            MeshRenderer mrDGear = dGearGo.AddComponent<MeshRenderer>();
            mfDGear.mesh = GearMeshBuilder.BuildSpurGearMesh(driverTeeth, r1, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrDGear.material = gearMaterialDriver;
            MeshCollider colDGear = dGearGo.AddComponent<MeshCollider>();
            colDGear.sharedMesh = mfDGear.mesh;

            InspectableComponent inspDGear = dGearGo.AddComponent<InspectableComponent>();
            inspDGear.SetMetadata("Driver Pinion (Z₁ = 20)",
                "External spur pinion transmitting motor power to driven gear. Rotates clockwise.",
                $"Teeth: {driverTeeth} | Module: {(moduleMeters * 1000f):F1} mm | Pitch Dia: {(r1 * 2000f):F1} mm | Face: {(faceWidth * 1000f):F0} mm");
            inspDGear.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 3. Driven Shaft & Bearing Pedestal
            Vector3 drivenAxisPos = new Vector3(r2, elevationY, 0);

            // Driven Pillow Block
            GameObject dnMountGo = new GameObject("Driven_PillowBlock");
            dnMountGo.transform.SetParent(transform, false);
            dnMountGo.transform.localPosition = new Vector3(drivenAxisPos.x, 0, -0.024f);

            MeshFilter mfDnMount = dnMountGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnMount = dnMountGo.AddComponent<MeshRenderer>();
            mfDnMount.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.045f, elevationY, 0.016f, boreRadius * 1.5f);
            mrDnMount.material = baseplateMaterial;
            BoxCollider colDnMount = dnMountGo.AddComponent<BoxCollider>();
            colDnMount.size = new Vector3(0.045f, elevationY, 0.016f);
            colDnMount.center = new Vector3(0, elevationY * 0.5f, 0);

            InspectableComponent inspDnMount = dnMountGo.AddComponent<InspectableComponent>();
            inspDnMount.SetMetadata("Driven Pillow Block Pedestal",
                "Heavy-duty bearing pedestal carrying high output torque load.",
                "Material: Cast Iron GGG40 | Machined Dowel Pin Anchoring");
            inspDnMount.SetExplosionVector(new Vector3(0.5f, 0, -1f), 0.07f, 0.10f);

            // Driven Ball Bearing
            GameObject dnBearingGo = new GameObject("Driven_BallBearing");
            dnBearingGo.transform.SetParent(transform, false);
            dnBearingGo.transform.localPosition = new Vector3(drivenAxisPos.x, elevationY, -0.024f);
            MeshFilter mfDnBear = dnBearingGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnBear = dnBearingGo.AddComponent<MeshRenderer>();
            mfDnBear.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrDnBear.material = steelShaftMaterial;
            BoxCollider colDnBear = dnBearingGo.AddComponent<BoxCollider>();
            colDnBear.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspDnBear = dnBearingGo.AddComponent<InspectableComponent>();
            inspDnBear.SetMetadata("Driven Heavy-Duty Ball Bearing",
                "High radial capacity bearing sustaining output drive shaft loads.",
                "ABEC-5 Precision | Dynamic Radial Load: 6.2 kN");
            inspDnBear.SetExplosionVector(Vector3.back, 0.09f, 0.12f);

            // Driven Rotor Container (Rotates around local Z)
            GameObject dnRotorGo = new GameObject("Driven_Rotor");
            dnRotorGo.transform.SetParent(transform, false);
            dnRotorGo.transform.localPosition = drivenAxisPos;
            drivenRotor = dnRotorGo.transform;

            // Driven Shaft
            GameObject dnShaftGo = new GameObject("Driven_Shaft");
            dnShaftGo.transform.SetParent(drivenRotor, false);
            dnShaftGo.transform.localPosition = new Vector3(0, 0, -0.012f);
            MeshFilter mfDnShaft = dnShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnShaft = dnShaftGo.AddComponent<MeshRenderer>();
            mfDnShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, 0.075f, 24, false);
            mrDnShaft.material = steelShaftMaterial;
            CapsuleCollider colDnShaft = dnShaftGo.AddComponent<CapsuleCollider>();
            colDnShaft.radius = boreRadius;
            colDnShaft.height = 0.075f;
            colDnShaft.direction = 2; // Z-axis

            InspectableComponent inspDnShaft = dnShaftGo.AddComponent<InspectableComponent>();
            inspDnShaft.SetMetadata("Output Load Shaft",
                "Delivers 2× multiplied output torque at halved rotational speed (60 RPM).",
                "Material: 42CrMo4 Alloy Steel | Keywayed Interface");
            inspDnShaft.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Driven Spur Gear (40T, Spoked with 4 Lightening Cutouts)
            GameObject dnGearGo = new GameObject("Driven_Gear_40T");
            dnGearGo.transform.SetParent(drivenRotor, false);
            dnGearGo.transform.localPosition = Vector3.zero;
            MeshFilter mfDnGear = dnGearGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnGear = dnGearGo.AddComponent<MeshRenderer>();
            mfDnGear.mesh = GearMeshBuilder.BuildSpurGearMesh(drivenTeeth, r2, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 4, spokeWidth: 0.009f);
            mrDnGear.material = gearMaterialDriven;
            MeshCollider colDnGear = dnGearGo.AddComponent<MeshCollider>();
            colDnGear.sharedMesh = mfDnGear.mesh;

            InspectableComponent inspDnGear = dnGearGo.AddComponent<InspectableComponent>();
            inspDnGear.SetMetadata("Driven Spur Gear (Z₂ = 40, Spoked)",
                "Meshes externally with driver pinion. Features 4 structural spokes to reduce rotational inertia and display rotation clearly.",
                $"Teeth: {drivenTeeth} | Ratio: 2.0:1 | Pitch Dia: {(r2 * 2000f):F1} mm | Output: {OutputRPM:F0} RPM");
            inspDnGear.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            ResetMechanism();
        }

        private Material CreateMaterial(Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Diffuse");
            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        protected override void UpdateKinematics(float inputAngularVelDegPerSec, float deltaTime)
        {
            currentDriverAngle += inputAngularVelDegPerSec * deltaTime;
            currentDriverAngle %= 360f;

            // Opposite rotation: omega_2 = -omega_1 * (Z1 / Z2)
            float speedRatio = (float)driverTeeth / drivenTeeth;
            currentDrivenAngle -= (inputAngularVelDegPerSec * speedRatio) * deltaTime;
            currentDrivenAngle %= 360f;

            if (driverRotor != null)
            {
                driverRotor.localRotation = Quaternion.Euler(0, 0, currentDriverAngle);
            }

            if (drivenRotor != null)
            {
                drivenRotor.localRotation = Quaternion.Euler(0, 0, currentDrivenAngle + initialDrivenPhaseOffset);
            }
        }

        public override void ResetMechanism()
        {
            currentDriverAngle = 0f;
            currentDrivenAngle = 0f;
            if (driverRotor != null) driverRotor.localRotation = Quaternion.identity;
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.Euler(0, 0, initialDrivenPhaseOffset);
        }

        public override string GetCalculationSummary()
        {
            return $"• Driver Teeth (Z₁): {driverTeeth}\n" +
                   $"• Driven Teeth (Z₂): {drivenTeeth}\n" +
                   $"• Gear Ratio (i): {GearRatio:F1}:1 (Reduction)\n" +
                   $"• Input Speed (n₁): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n₂): {OutputRPM:F0} RPM\n" +
                   $"• Torque Multiplier: {GearRatio:F1}×\n" +
                   $"• Center Distance (a): 105.0 mm\n" +
                   $"• Mesh Direction: Counter-Rotating (Opposite)";
        }

        public override string GetEngineeringExplanation()
        {
            return "Spur gears transmit mechanical power between parallel shafts using precision involute teeth cut parallel to the axis of rotation. " +
                   "Because the pitch circles engage in external contact, the driven gear always rotates in the opposite direction to the driver pinion. " +
                   "Here, the 20-tooth driver rotates the 40-tooth driven gear, cutting output velocity in half while doubling ideal mechanical torque (T₂ = 2·T₁). " +
                   "The 4-spoke cast architecture of the driven gear reduces rotational inertia while making motion immediately visible.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Velocity Ratio: i = Z₂ / Z₁ = 40 / 20 = 2.0\n" +
                   "Output Velocity: n₂ = n₁ / i = " + $"{currentInputRPM:F0} / 2.0 = {OutputRPM:F0} RPM\n" +
                   "Torque Multiplier: T₂ = T₁ · i = 2.0 · T₁\n" +
                   "Center Distance: a = m·(Z₁ + Z₂) / 2 = 3.5·(20 + 40) / 2 = 105.0 mm\n" +
                   "Pitch Diameters: d₁ = 20·3.5 = 70.0 mm | d₂ = 40·3.5 = 140.0 mm";
        }
    }
}

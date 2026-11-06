using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Bevel Gear Pair Mechanism.
    /// Demonstrates 90-degree intersecting shaft power transmission and 2:1 reduction ratio.
    /// Features:
    /// - 16-tooth conical bevel pinion (Electric Blue) rotating around horizontal Z-axis
    /// - 32-tooth spoked crown bevel gear (Industrial Gold, 4 spoke cutouts) rotating around vertical Y-axis
    /// - Dual intersecting transmission shafts at 90.0 degrees
    /// - Dual cast pillow block bearing pedestals
    /// - 2 tapered roller bearing assemblies
    /// - Precision machined L-shaped machine bed
    /// - Exploded view & individual component detachment
    /// Pitch cone angles: delta1 = 26.57 deg, delta2 = 63.43 deg (delta1 + delta2 = 90.0 deg).
    /// </summary>
    public class BevelGearPair : MechanismBase
    {
        [Header("Bevel Gear Parameters")]
        [SerializeField] private int driverTeeth = 16;
        [SerializeField] private int drivenTeeth = 32;
        [SerializeField] private float moduleMeters = 0.0035f; // 3.5mm module -> Pinion: 56mm, Crown: 112mm
        [SerializeField] private float faceWidth = 0.020f;     // 20mm tooth slant face
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
        public override string MechanismName => "Bevel Gear Pair";
        public override string CategoryTitle => "90° Intersecting Shaft Transmission";
        public override float GearRatio => (float)drivenTeeth / driverTeeth; // 32 / 16 = 2.0
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
            float r1 = moduleMeters * driverTeeth * 0.5f; // 0.028m = 28mm
            float r2 = moduleMeters * drivenTeeth * 0.5f; // 0.056m = 56mm

            float delta1 = Mathf.Atan2(driverTeeth, drivenTeeth); // 26.565 deg
            float delta2 = Mathf.Atan2(drivenTeeth, driverTeeth); // 63.435 deg

            // Apex distance along shafts from origin (0,0,0)
            float apexDistZ = r2; // Pinion back face distance from apex
            float apexDistY = r1; // Crown back face distance from apex

            initialDrivenPhaseOffset = (180f / drivenTeeth);

            // 1. Machine Bedplate (L-shaped / compact square)
            GameObject baseGo = new GameObject("Baseplate");
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localPosition = new Vector3(0, -0.045f, -0.02f);

            MeshFilter mfBase = baseGo.AddComponent<MeshFilter>();
            MeshRenderer mrBase = baseGo.AddComponent<MeshRenderer>();
            mfBase.mesh = GearMeshBuilder.BuildBasePlateMesh(0.14f, 0.16f, 0.014f);
            mrBase.material = baseplateMaterial;

            BoxCollider colBase = baseGo.AddComponent<BoxCollider>();
            colBase.size = new Vector3(0.14f, 0.014f, 0.16f);
            colBase.center = new Vector3(0, -0.007f, 0);

            InspectableComponent inspBase = baseGo.AddComponent<InspectableComponent>();
            inspBase.SetMetadata("Orthogonal Gearbox Bedplate",
                "Rigid datum base maintaining perpendicularity between horizontal and vertical shaft axes (Σ = 90.0°).",
                "Material: Cast Structural Alloy | Perpendicularity Tolerance: ±0.015 mm");
            inspBase.SetExplosionVector(Vector3.down, 0.05f, 0.08f);

            // 2. Horizontal Input Axis (Pinion, along Z)
            Vector3 pinionBackPos = new Vector3(0, 0, -apexDistZ);

            // Horizontal Pillow Block (Pedestal)
            GameObject dMountGo = new GameObject("Horizontal_PillowBlock");
            dMountGo.transform.SetParent(transform, false);
            dMountGo.transform.localPosition = new Vector3(0, -0.045f, -apexDistZ - 0.040f);

            MeshFilter mfDMount = dMountGo.AddComponent<MeshFilter>();
            MeshRenderer mrDMount = dMountGo.AddComponent<MeshRenderer>();
            mfDMount.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.048f, 0.045f, 0.018f, boreRadius * 1.5f);
            mrDMount.material = baseplateMaterial;
            BoxCollider colDMount = dMountGo.AddComponent<BoxCollider>();
            colDMount.size = new Vector3(0.048f, 0.045f, 0.018f);
            colDMount.center = new Vector3(0, 0.0225f, 0);

            InspectableComponent inspDMount = dMountGo.AddComponent<InspectableComponent>();
            inspDMount.SetMetadata("Horizontal Pinion Pillow Block",
                "Bearing support absorbing axial thrust pushing pinion away from the apex.",
                "Material: Cast Iron GGG40 | Thrust Resistant Design");
            inspDMount.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Horizontal Tapered Bearing
            GameObject dBearingGo = new GameObject("Pinion_TaperedBearing");
            dBearingGo.transform.SetParent(transform, false);
            dBearingGo.transform.localPosition = new Vector3(0, 0, -apexDistZ - 0.040f);
            MeshFilter mfDBear = dBearingGo.AddComponent<MeshFilter>();
            MeshRenderer mrDBear = dBearingGo.AddComponent<MeshRenderer>();
            mfDBear.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrDBear.material = steelShaftMaterial;
            BoxCollider colDBear = dBearingGo.AddComponent<BoxCollider>();
            colDBear.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspDBear = dBearingGo.AddComponent<InspectableComponent>();
            inspDBear.SetMetadata("Pinion Tapered Roller Bearing",
                "Handles combination radial and axial thrust loads along horizontal input axis.",
                "Single-Row Tapered Roller | Contact Angle: 14°");
            inspDBear.SetExplosionVector(Vector3.back, 0.09f, 0.12f);

            // Driver Rotor Container (Rotates around local Z)
            GameObject dRotorGo = new GameObject("Driver_Rotor");
            dRotorGo.transform.SetParent(transform, false);
            dRotorGo.transform.localPosition = Vector3.zero;
            driverRotor = dRotorGo.transform;

            // Driver Shaft (along Z)
            GameObject dShaftGo = new GameObject("Pinion_Shaft");
            dShaftGo.transform.SetParent(driverRotor, false);
            dShaftGo.transform.localPosition = new Vector3(0, 0, -apexDistZ - 0.025f);
            MeshFilter mfDShaft = dShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrDShaft = dShaftGo.AddComponent<MeshRenderer>();
            mfDShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, 0.075f, 24, false);
            mrDShaft.material = steelShaftMaterial;
            CapsuleCollider colDShaft = dShaftGo.AddComponent<CapsuleCollider>();
            colDShaft.radius = boreRadius;
            colDShaft.height = 0.075f;
            colDShaft.direction = 2; // Z-axis

            InspectableComponent inspDShaft = dShaftGo.AddComponent<InspectableComponent>();
            inspDShaft.SetMetadata("Horizontal Input Drive Shaft",
                "High-strength alloy steel drive shaft delivering 120 RPM input motion.",
                "Material: 42CrMo4 Hardened Steel | Ground Finish");
            inspDShaft.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Bevel Pinion Gear (16T, Conical)
            GameObject dGearGo = new GameObject("Bevel_Pinion_16T");
            dGearGo.transform.SetParent(driverRotor, false);
            dGearGo.transform.localPosition = pinionBackPos;
            MeshFilter mfDGear = dGearGo.AddComponent<MeshFilter>();
            MeshRenderer mrDGear = dGearGo.AddComponent<MeshRenderer>();
            mfDGear.mesh = GearMeshBuilder.BuildBevelGearMesh(driverTeeth, r1, delta1, faceWidth, moduleMeters, moduleMeters * 1.25f, boreRadius, spokeCount: 0);
            mrDGear.material = gearMaterialDriver;
            MeshCollider colDGear = dGearGo.AddComponent<MeshCollider>();
            colDGear.sharedMesh = mfDGear.mesh;

            InspectableComponent inspDGear = dGearGo.AddComponent<InspectableComponent>();
            inspDGear.SetMetadata("Bevel Pinion (Z₁ = 16)",
                "Conical driver gear transmitting power into 90° intersection.",
                $"Teeth: {driverTeeth} | Cone Angle: {(delta1 * Mathf.Rad2Deg):F1}° | Pitch Dia: {(r1 * 2000f):F1} mm | Face: {(faceWidth * 1000f):F0} mm");
            inspDGear.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            // 3. Vertical Output Axis (Crown Gear, along Y)
            Vector3 crownBackPos = new Vector3(0, apexDistY, 0);

            // Vertical Pillow Block (Pedestal)
            GameObject dnMountGo = new GameObject("Vertical_PillowBlock");
            dnMountGo.transform.SetParent(transform, false);
            dnMountGo.transform.localPosition = new Vector3(0, apexDistY + 0.040f, 0);
            dnMountGo.transform.localRotation = Quaternion.Euler(0, 0, 90f); // Mount vertically

            MeshFilter mfDnMount = dnMountGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnMount = dnMountGo.AddComponent<MeshRenderer>();
            mfDnMount.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.048f, 0.045f, 0.018f, boreRadius * 1.5f);
            mrDnMount.material = baseplateMaterial;
            BoxCollider colDnMount = dnMountGo.AddComponent<BoxCollider>();
            colDnMount.size = new Vector3(0.048f, 0.045f, 0.018f);

            InspectableComponent inspDnMount = dnMountGo.AddComponent<InspectableComponent>();
            inspDnMount.SetMetadata("Vertical Crown Pillow Block",
                "Rigid vertical bearing mount resisting crown gear axial separation thrust.",
                "Material: Cast Iron GGG40 | Bore: 16 mm");
            inspDnMount.SetExplosionVector(Vector3.up, 0.07f, 0.10f);

            // Vertical Tapered Bearing
            GameObject dnBearingGo = new GameObject("Crown_TaperedBearing");
            dnBearingGo.transform.SetParent(transform, false);
            dnBearingGo.transform.localPosition = new Vector3(0, apexDistY + 0.040f, 0);
            dnBearingGo.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Flat on X-Z plane
            MeshFilter mfDnBear = dnBearingGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnBear = dnBearingGo.AddComponent<MeshRenderer>();
            mfDnBear.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrDnBear.material = steelShaftMaterial;
            BoxCollider colDnBear = dnBearingGo.AddComponent<BoxCollider>();
            colDnBear.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspDnBear = dnBearingGo.AddComponent<InspectableComponent>();
            inspDnBear.SetMetadata("Crown Tapered Roller Bearing",
                "Carries heavy vertical thrust generated by crown gear cone angle (63.4°).",
                "Single-Row Tapered Roller | High Thrust Rating");
            inspDnBear.SetExplosionVector(Vector3.up, 0.09f, 0.12f);

            // Driven Rotor Container (Rotates around local Y)
            GameObject dnRotorGo = new GameObject("Driven_Rotor");
            dnRotorGo.transform.SetParent(transform, false);
            dnRotorGo.transform.localPosition = Vector3.zero;
            drivenRotor = dnRotorGo.transform;

            // Driven Shaft (along Y)
            GameObject dnShaftGo = new GameObject("Crown_Shaft");
            dnShaftGo.transform.SetParent(drivenRotor, false);
            dnShaftGo.transform.localPosition = new Vector3(0, apexDistY + 0.025f, 0);
            dnShaftGo.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            MeshFilter mfDnShaft = dnShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnShaft = dnShaftGo.AddComponent<MeshRenderer>();
            mfDnShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, 0.075f, 24, false);
            mrDnShaft.material = steelShaftMaterial;
            CapsuleCollider colDnShaft = dnShaftGo.AddComponent<CapsuleCollider>();
            colDnShaft.radius = boreRadius;
            colDnShaft.height = 0.075f;
            colDnShaft.direction = 2; // Along local Z which is world Y

            InspectableComponent inspDnShaft = dnShaftGo.AddComponent<InspectableComponent>();
            inspDnShaft.SetMetadata("Vertical Output Shaft",
                "Transmits right-angle redirected output power at 60 RPM.",
                "Material: 42CrMo4 Alloy Steel | Keywayed Interface");
            inspDnShaft.SetExplosionVector(Vector3.up, 0.07f, 0.10f);

            // Crown Bevel Gear (32T, Conical with 4 Spokes)
            GameObject dnGearGo = new GameObject("Bevel_Crown_32T");
            dnGearGo.transform.SetParent(drivenRotor, false);
            dnGearGo.transform.localPosition = crownBackPos;
            dnGearGo.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Face downward toward apex
            MeshFilter mfDnGear = dnGearGo.AddComponent<MeshFilter>();
            MeshRenderer mrDnGear = dnGearGo.AddComponent<MeshRenderer>();
            mfDnGear.mesh = GearMeshBuilder.BuildBevelGearMesh(drivenTeeth, r2, delta2, faceWidth, moduleMeters, moduleMeters * 1.25f, boreRadius, spokeCount: 4);
            mrDnGear.material = gearMaterialDriven;
            MeshCollider colDnGear = dnGearGo.AddComponent<MeshCollider>();
            colDnGear.sharedMesh = mfDnGear.mesh;

            InspectableComponent inspDnGear = dnGearGo.AddComponent<InspectableComponent>();
            inspDnGear.SetMetadata("Crown Bevel Gear (Z₂ = 32, Spoked)",
                "Conical crown gear receiving motion at right angle. Delivers 2:1 speed reduction.",
                $"Teeth: {drivenTeeth} | Cone Angle: {(delta2 * Mathf.Rad2Deg):F1}° | Pitch Dia: {(r2 * 2000f):F1} mm | Output: {OutputRPM:F0} RPM");
            inspDnGear.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

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
            // Pinion rotates around its horizontal Z-axis
            currentDriverAngle += inputAngularVelDegPerSec * deltaTime;
            currentDriverAngle %= 360f;

            // Crown gear rotates around its vertical Y-axis with 2:1 reduction
            float speedRatio = (float)driverTeeth / drivenTeeth;
            currentDrivenAngle += (inputAngularVelDegPerSec * speedRatio) * deltaTime;
            currentDrivenAngle %= 360f;

            if (driverRotor != null)
            {
                // Clean AngleAxis around Z - Zero gimbal lock!
                driverRotor.localRotation = Quaternion.AngleAxis(currentDriverAngle, Vector3.forward);
            }

            if (drivenRotor != null)
            {
                // Clean AngleAxis around Y - Zero gimbal lock!
                drivenRotor.localRotation = Quaternion.AngleAxis(currentDrivenAngle + initialDrivenPhaseOffset, Vector3.up);
            }
        }

        public override void ResetMechanism()
        {
            currentDriverAngle = 0f;
            currentDrivenAngle = 0f;
            if (driverRotor != null) driverRotor.localRotation = Quaternion.identity;
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.AngleAxis(initialDrivenPhaseOffset, Vector3.up);
        }

        public override string GetCalculationSummary()
        {
            return $"• Pinion Teeth (Z₁): {driverTeeth}\n" +
                   $"• Crown Teeth (Z₂): {drivenTeeth}\n" +
                   $"• Shaft Angle (Σ): 90.0° (Perpendicular)\n" +
                   $"• Pinion Cone Angle (δ₁): 26.6°\n" +
                   $"• Crown Cone Angle (δ₂): 63.4° (δ₁ + δ₂ = 90°)\n" +
                   $"• Gear Ratio (i): {GearRatio:F1}:1 (Reduction)\n" +
                   $"• Input Speed (n₁): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n₂): {OutputRPM:F0} RPM\n" +
                   $"• Torque Multiplier: {GearRatio:F1}×";
        }

        public override string GetEngineeringExplanation()
        {
            return "Bevel gears feature conical pitch surfaces and teeth cut along the cone generators. " +
                   "They transmit power between intersecting shafts, most commonly at 90°. " +
                   "The pitch cones share a common apex at the intersection of the two shaft axes, ensuring pure rolling contact along the pitch line without sliding scuffing. " +
                   "Here, the 16-tooth horizontal pinion drives the 32-tooth vertical crown gear, cutting speed to 60 RPM while doubling torque.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Pinion Cone Angle: tan(δ₁) = Z₁ / Z₂ = 16 / 32 = 0.50 => δ₁ = 26.57°\n" +
                   "Crown Cone Angle: tan(δ₂) = Z₂ / Z₁ = 32 / 16 = 2.00 => δ₂ = 63.43°\n" +
                   "Sum of Cone Angles: δ₁ + δ₂ = 26.57° + 63.43° = 90.00°\n" +
                   "Output Velocity: n₂ = n₁ · (Z₁ / Z₂) = " + $"{currentInputRPM:F0} · (16/32) = {OutputRPM:F0} RPM\n" +
                   "Apex Slant Distance: R = d₁ / (2·sin(δ₁)) = 28 / sin(26.57°) ≈ 62.6 mm";
        }
    }
}

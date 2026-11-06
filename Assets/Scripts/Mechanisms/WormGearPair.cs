using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Worm and Throated Worm Wheel Gear Mechanism.
    /// Demonstrates high-reduction non-intersecting perpendicular transmission (30:1 reduction)
    /// and explains mechanical self-locking (non-backdrivability).
    /// Features:
    /// - Single-start hardened steel helical worm screw (Electric Blue) along horizontal X-axis
    /// - 30-tooth throated phosphor bronze worm wheel (Industrial Gold, 4 spoke cutouts) along Z-axis
    /// - High elevated worm bearing pedestals & lateral wheel pillow blocks
    /// - 4 precision bearing assemblies
    /// - Elevated clearance machine bedplate ensuring complete visibility of both components
    /// - Exploded view & individual component detachment
    /// Reduction Ratio: 30:1 (Output Speed: 4.0 RPM at 120 RPM input).
    /// </summary>
    public class WormGearPair : MechanismBase
    {
        [Header("Worm Mechanism Parameters")]
        [SerializeField] private int wormStarts = 1;        // Single-start helical worm (z1 = 1)
        [SerializeField] private int wheelTeeth = 30;       // Worm wheel teeth (z2 = 30)
        [SerializeField] private float moduleMeters = 0.0035f;
        [SerializeField] private float wormLength = 0.085f;
        [SerializeField] private float wormCoreRadius = 0.010f;
        [SerializeField] private float wheelFaceWidth = 0.016f;
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Transforms (Auto-generated if null)")]
        [SerializeField] private Transform wormRotor;
        [SerializeField] private Transform wheelRotor;
        [SerializeField] private Material wormMaterial;
        [SerializeField] private Material wheelMaterial;
        [SerializeField] private Material steelShaftMaterial;
        [SerializeField] private Material baseplateMaterial;

        private float currentWormAngle = 0f;
        private float currentWheelAngle = 0f;
        private bool isAssemblyBuilt = false;

        public int WormStarts => wormStarts;
        public int WheelTeeth => wheelTeeth;
        public override string MechanismName => "Worm & Wheel Gear";
        public override string CategoryTitle => "Skew Shaft High-Reduction Drive";
        public override float GearRatio => (float)wheelTeeth / wormStarts; // 30 / 1 = 30.0
        public override float OutputRPM => currentInputRPM / GearRatio;    // 120 / 30 = 4.0 RPM

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void LoadMaterials()
        {
            if (wormMaterial == null)
                wormMaterial = Resources.Load<Material>("Art/Materials/Mat_Gear_Driver") ??
                               CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f);
            if (wheelMaterial == null)
                wheelMaterial = Resources.Load<Material>("Art/Materials/Mat_Gear_Driven") ??
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
            float lead = Mathf.PI * moduleMeters * wormStarts; // ~11.0 mm
            float wormPitchRadius = wormCoreRadius + moduleMeters; // 13.5 mm
            float wormTipRadius = wormPitchRadius + moduleMeters;  // 17.0 mm
            float wormRootRadius = wormCoreRadius;                 // 10.0 mm

            float wheelPitchRadius = moduleMeters * wheelTeeth * 0.5f; // 0.0525m = 52.5 mm
            float centerDist = wormPitchRadius + wheelPitchRadius;     // 66.0 mm

            // Elevate the assembly so the bottom of the worm wheel is at least 15mm ABOVE the baseplate
            // Wheel center is at (0, 0, 0); bottom of wheel is at y = -0.0525m - 0.0035m = -0.056m
            // Baseplate top is at y = -0.070m. Total clearance: 14mm!
            float baseTopY = -0.070f;
            float wormPosY = centerDist * 0.88f; // Worm positioned tangentially above wheel

            // Lead angle gamma = atan(lead / (pi * d_worm))
            float leadAngleRad = Mathf.Atan2(lead, 2f * Mathf.PI * wormPitchRadius);

            // 1. Machine Bedplate (Base)
            GameObject baseGo = new GameObject("Baseplate");
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localPosition = new Vector3(0, baseTopY, 0);

            MeshFilter mfBase = baseGo.AddComponent<MeshFilter>();
            MeshRenderer mrBase = baseGo.AddComponent<MeshRenderer>();
            mfBase.mesh = GearMeshBuilder.BuildBasePlateMesh(0.15f, 0.14f, 0.015f);
            mrBase.material = baseplateMaterial;

            BoxCollider colBase = baseGo.AddComponent<BoxCollider>();
            colBase.size = new Vector3(0.15f, 0.015f, 0.14f);
            colBase.center = new Vector3(0, -0.0075f, 0);

            InspectableComponent inspBase = baseGo.AddComponent<InspectableComponent>();
            inspBase.SetMetadata("Heavy Duty Enclosed Reduction Bedplate",
                "Rigid cast casing maintaining precision 90° non-intersecting skew shaft alignment (Center Distance a = 66.0 mm).",
                "Material: Cast Structural Iron GGG50 | High Torsional Rigidity");
            inspBase.SetExplosionVector(Vector3.down, 0.06f, 0.09f);

            // 2. High Elevated Worm Shaft (Input along X)
            Vector3 wormCenterPos = new Vector3(0, wormPosY, 0);

            // High Worm Bearing Towers (Pillow Blocks)
            float towerHeight = wormPosY - baseTopY;
            float towerXOffset = 0.052f;

            // Left Worm Pillow Block
            GameObject wMountLGo = new GameObject("Worm_PillowBlock_Left");
            wMountLGo.transform.SetParent(transform, false);
            wMountLGo.transform.localPosition = new Vector3(-towerXOffset, baseTopY, 0);
            wMountLGo.transform.localRotation = Quaternion.Euler(0, 90f, 0); // Face along X
            MeshFilter mfWML = wMountLGo.AddComponent<MeshFilter>();
            MeshRenderer mrWML = wMountLGo.AddComponent<MeshRenderer>();
            mfWML.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.040f, towerHeight, 0.016f, boreRadius * 1.5f);
            mrWML.material = baseplateMaterial;
            BoxCollider colWML = wMountLGo.AddComponent<BoxCollider>();
            colWML.size = new Vector3(0.040f, towerHeight, 0.016f);
            colWML.center = new Vector3(0, towerHeight * 0.5f, 0);

            InspectableComponent inspWML = wMountLGo.AddComponent<InspectableComponent>();
            inspWML.SetMetadata("Worm Thrust Bearing Pedestal (Left)",
                "Rigid elevated tower absorbing immense axial screw thrust generated under load.",
                "Material: Cast Iron GGG50 | Pre-loaded Dual Bearing Seat");
            inspWML.SetExplosionVector(Vector3.left, 0.07f, 0.10f);

            // Left Worm Bearing
            GameObject wBearLGo = new GameObject("Worm_Bearing_Left");
            wBearLGo.transform.SetParent(transform, false);
            wBearLGo.transform.localPosition = new Vector3(-towerXOffset, wormPosY, 0);
            wBearLGo.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            MeshFilter mfWBL = wBearLGo.AddComponent<MeshFilter>();
            MeshRenderer mrWBL = wBearLGo.AddComponent<MeshRenderer>();
            mfWBL.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrWBL.material = steelShaftMaterial;
            BoxCollider colWBL = wBearLGo.AddComponent<BoxCollider>();
            colWBL.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspWBL = wBearLGo.AddComponent<InspectableComponent>();
            inspWBL.SetMetadata("Worm Angular-Contact Thrust Bearing",
                "Absorbs heavy axial screw thrust in 120 RPM direction.",
                "Angular Contact 7200 Series | High Axial Thrust Rating");
            inspWBL.SetExplosionVector(Vector3.left, 0.09f, 0.12f);

            // Right Worm Pillow Block
            GameObject wMountRGo = new GameObject("Worm_PillowBlock_Right");
            wMountRGo.transform.SetParent(transform, false);
            wMountRGo.transform.localPosition = new Vector3(towerXOffset, baseTopY, 0);
            wMountRGo.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            MeshFilter mfWMR = wMountRGo.AddComponent<MeshFilter>();
            MeshRenderer mrWMR = wMountRGo.AddComponent<MeshRenderer>();
            mfWMR.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.040f, towerHeight, 0.016f, boreRadius * 1.5f);
            mrWMR.material = baseplateMaterial;
            BoxCollider colWMR = wMountRGo.AddComponent<BoxCollider>();
            colWMR.size = new Vector3(0.040f, towerHeight, 0.016f);
            colWMR.center = new Vector3(0, towerHeight * 0.5f, 0);

            InspectableComponent inspWMR = wMountRGo.AddComponent<InspectableComponent>();
            inspWMR.SetMetadata("Worm Drive Bearing Pedestal (Right)",
                "Elevated support maintaining precise worm-to-wheel center distance (66.0 mm).",
                "Material: Cast Iron GGG50 | Tolerance: ±0.01 mm");
            inspWMR.SetExplosionVector(Vector3.right, 0.07f, 0.10f);

            // Right Worm Bearing
            GameObject wBearRGo = new GameObject("Worm_Bearing_Right");
            wBearRGo.transform.SetParent(transform, false);
            wBearRGo.transform.localPosition = new Vector3(towerXOffset, wormPosY, 0);
            wBearRGo.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            MeshFilter mfWBR = wBearRGo.AddComponent<MeshFilter>();
            MeshRenderer mrWBR = wBearRGo.AddComponent<MeshRenderer>();
            mfWBR.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrWBR.material = steelShaftMaterial;
            BoxCollider colWBR = wBearRGo.AddComponent<BoxCollider>();
            colWBR.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspWBR = wBearRGo.AddComponent<InspectableComponent>();
            inspWBR.SetMetadata("Worm Radial Bearing (Right)",
                "Carries radial reaction loads for the input drive motor shaft.",
                "ABEC-5 Deep Groove Ball Bearing");
            inspWBR.SetExplosionVector(Vector3.right, 0.09f, 0.12f);

            // Worm Rotor Container (Rotates around local X)
            GameObject wRotorGo = new GameObject("Worm_Rotor");
            wRotorGo.transform.SetParent(transform, false);
            wRotorGo.transform.localPosition = wormCenterPos;
            wormRotor = wRotorGo.transform;

            // Worm Shaft
            GameObject wShaftGo = new GameObject("Worm_Shaft");
            wShaftGo.transform.SetParent(wormRotor, false);
            wShaftGo.transform.localPosition = Vector3.zero;
            MeshFilter mfWShaft = wShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrWShaft = wShaftGo.AddComponent<MeshRenderer>();
            mfWShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, wormLength + 0.045f, 24, alongX: true);
            mrWShaft.material = steelShaftMaterial;
            CapsuleCollider colWShaft = wShaftGo.AddComponent<CapsuleCollider>();
            colWShaft.radius = boreRadius;
            colWShaft.height = wormLength + 0.045f;
            colWShaft.direction = 0; // X-axis

            InspectableComponent inspWShaft = wShaftGo.AddComponent<InspectableComponent>();
            inspWShaft.SetMetadata("Worm Input Transmission Shaft",
                "High-torque motor shaft transmitting continuous 120 RPM input rotation.",
                "Material: 42CrMo4 Hardened Steel | Ground Precision h6");
            inspWShaft.SetExplosionVector(Vector3.up, 0.07f, 0.10f);

            // Helical Worm Screw (z1 = 1)
            GameObject wMeshGo = new GameObject("Helical_Worm_Screw");
            wMeshGo.transform.SetParent(wormRotor, false);
            wMeshGo.transform.localPosition = Vector3.zero;
            MeshFilter mfWMesh = wMeshGo.AddComponent<MeshFilter>();
            MeshRenderer mrWMesh = wMeshGo.AddComponent<MeshRenderer>();
            mfWMesh.mesh = GearMeshBuilder.BuildWormMesh(wormStarts, lead, wormPitchRadius, wormRootRadius, wormTipRadius, wormLength);
            mrWMesh.material = wormMaterial;
            MeshCollider colWMesh = wMeshGo.AddComponent<MeshCollider>();
            colWMesh.sharedMesh = mfWMesh.mesh;

            InspectableComponent inspWMesh = wMeshGo.AddComponent<InspectableComponent>();
            inspWMesh.SetMetadata("Helical Worm Screw (z₁ = 1)",
                "Single-start trapezoidal Acme thread driving the worm wheel through conjugate sliding action. Self-locking prevents reverse driving.",
                $"Starts: {wormStarts} | Axial Lead: {(lead * 1000f):F2} mm | Pitch Dia: {(wormPitchRadius * 2000f):F1} mm | Lead Angle: {(leadAngleRad * Mathf.Rad2Deg):F1}°");
            inspWMesh.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // 3. Worm Wheel (Driven Gear, centered at (0, 0, 0), along Z)
            Vector3 wheelCenterPos = Vector3.zero;

            // Lateral Wheel Bearing Pedestals
            float wheelMountZOffset = 0.026f;
            float wheelTowerH = -baseTopY;

            // Front Wheel Pillow Block
            GameObject whMountFGo = new GameObject("Wheel_PillowBlock_Front");
            whMountFGo.transform.SetParent(transform, false);
            whMountFGo.transform.localPosition = new Vector3(0, baseTopY, wheelMountZOffset);
            MeshFilter mfWhMF = whMountFGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhMF = whMountFGo.AddComponent<MeshRenderer>();
            mfWhMF.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.045f, wheelTowerH, 0.016f, boreRadius * 1.5f);
            mrWhMF.material = baseplateMaterial;
            BoxCollider colWhMF = whMountFGo.AddComponent<BoxCollider>();
            colWhMF.size = new Vector3(0.045f, wheelTowerH, 0.016f);
            colWhMF.center = new Vector3(0, wheelTowerH * 0.5f, 0);

            InspectableComponent inspWhMF = whMountFGo.AddComponent<InspectableComponent>();
            inspWhMF.SetMetadata("Wheel Output Pillow Block (Front)",
                "Heavy cast pedestal supporting high output torque shaft at 4 RPM.",
                "Material: Cast Iron GGG50 | High Torque Rating");
            inspWhMF.SetExplosionVector(Vector3.forward, 0.07f, 0.10f);

            // Front Wheel Bearing
            GameObject whBearFGo = new GameObject("Wheel_Bearing_Front");
            whBearFGo.transform.SetParent(transform, false);
            whBearFGo.transform.localPosition = new Vector3(0, 0, wheelMountZOffset);
            MeshFilter mfWhBF = whBearFGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhBF = whBearFGo.AddComponent<MeshRenderer>();
            mfWhBF.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrWhBF.material = steelShaftMaterial;
            BoxCollider colWhBF = whBearFGo.AddComponent<BoxCollider>();
            colWhBF.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspWhBF = whBearFGo.AddComponent<InspectableComponent>();
            inspWhBF.SetMetadata("Wheel Heavy-Duty Ball Bearing",
                "Heavy-duty radial bearing supporting output load shaft.",
                "ABEC-5 Deep Groove Bearing | High Radial Load Rating");
            inspWhBF.SetExplosionVector(Vector3.forward, 0.09f, 0.12f);

            // Back Wheel Pillow Block
            GameObject whMountBGo = new GameObject("Wheel_PillowBlock_Back");
            whMountBGo.transform.SetParent(transform, false);
            whMountBGo.transform.localPosition = new Vector3(0, baseTopY, -wheelMountZOffset);
            MeshFilter mfWhMB = whMountBGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhMB = whMountBGo.AddComponent<MeshRenderer>();
            mfWhMB.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.045f, wheelTowerH, 0.016f, boreRadius * 1.5f);
            mrWhMB.material = baseplateMaterial;
            BoxCollider colWhMB = whMountBGo.AddComponent<BoxCollider>();
            colWhMB.size = new Vector3(0.045f, wheelTowerH, 0.016f);
            colWhMB.center = new Vector3(0, wheelTowerH * 0.5f, 0);

            InspectableComponent inspWhMB = whMountBGo.AddComponent<InspectableComponent>();
            inspWhMB.SetMetadata("Wheel Output Pillow Block (Back)",
                "Bearing pedestal resisting radial tooth separation reaction.",
                "Material: Cast Iron GGG50 | Rigid Dowel Pin Mounting");
            inspWhMB.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Back Wheel Bearing
            GameObject whBearBGo = new GameObject("Wheel_Bearing_Back");
            whBearBGo.transform.SetParent(transform, false);
            whBearBGo.transform.localPosition = new Vector3(0, 0, -wheelMountZOffset);
            MeshFilter mfWhBB = whBearBGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhBB = whBearBGo.AddComponent<MeshRenderer>();
            mfWhBB.mesh = GearMeshBuilder.BuildBearingRingMesh(boreRadius * 1.5f, boreRadius, 0.010f);
            mrWhBB.material = steelShaftMaterial;
            BoxCollider colWhBB = whBearBGo.AddComponent<BoxCollider>();
            colWhBB.size = new Vector3(boreRadius * 3f, boreRadius * 3f, 0.010f);

            InspectableComponent inspWhBB = whBearBGo.AddComponent<InspectableComponent>();
            inspWhBB.SetMetadata("Wheel Radial Bearing (Back)",
                "Radial bearing guiding output shaft under heavy loads.",
                "ABEC-5 Deep Groove Bearing");
            inspWhBB.SetExplosionVector(Vector3.back, 0.09f, 0.12f);

            // Wheel Rotor Container (Rotates around local Z)
            GameObject whRotorGo = new GameObject("Wheel_Rotor");
            whRotorGo.transform.SetParent(transform, false);
            whRotorGo.transform.localPosition = wheelCenterPos;
            wheelRotor = whRotorGo.transform;

            // Wheel Shaft (along Z)
            GameObject whShaftGo = new GameObject("Wheel_Shaft");
            whShaftGo.transform.SetParent(wheelRotor, false);
            whShaftGo.transform.localPosition = Vector3.zero;
            MeshFilter mfWhShaft = whShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhShaft = whShaftGo.AddComponent<MeshRenderer>();
            mfWhShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius * 0.95f, 0.080f, 24, alongX: false);
            mrWhShaft.material = steelShaftMaterial;
            CapsuleCollider colWhShaft = whShaftGo.AddComponent<CapsuleCollider>();
            colWhShaft.radius = boreRadius;
            colWhShaft.height = 0.080f;
            colWhShaft.direction = 2; // Z-axis

            InspectableComponent inspWhShaft = whShaftGo.AddComponent<InspectableComponent>();
            inspWhShaft.SetMetadata("Output Heavy Drive Shaft",
                "Delivers 30× multiplied mechanical torque at reduced speed of 4.0 RPM.",
                "Material: 42CrMo4 Alloy Steel | Keywayed Interface");
            inspWhShaft.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // Throated Worm Wheel (30T, Spoked with 4 Lightening Cutouts)
            GameObject whMeshGo = new GameObject("Throated_Worm_Wheel_30T");
            whMeshGo.transform.SetParent(wheelRotor, false);
            whMeshGo.transform.localPosition = Vector3.zero;
            MeshFilter mfWhMesh = whMeshGo.AddComponent<MeshFilter>();
            MeshRenderer mrWhMesh = whMeshGo.AddComponent<MeshRenderer>();
            mfWhMesh.mesh = GearMeshBuilder.BuildThroatedWormWheelMesh(wheelTeeth, wheelPitchRadius, wormPitchRadius, wheelFaceWidth, moduleMeters, boreRadius, leadAngleRad, spokeCount: 4);
            mrWhMesh.material = wheelMaterial;
            MeshCollider colWhMesh = whMeshGo.AddComponent<MeshCollider>();
            colWhMesh.sharedMesh = mfWhMesh.mesh;

            InspectableComponent inspWhMesh = whMeshGo.AddComponent<InspectableComponent>();
            inspWhMesh.SetMetadata("Phosphor Bronze Worm Wheel (z₂ = 30, Spoked)",
                "Toothed wheel meshing with the worm screw. Features concave throated rim and 4 spokes to reveal continuous 4 RPM output rotation.",
                $"Teeth: {wheelTeeth} | Material: CuSn12 Phosphor Bronze | Pitch Dia: {(wheelPitchRadius * 2000f):F1} mm | Ratio: 30:1 | Output: {OutputRPM:F1} RPM");
            inspWhMesh.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

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
            // Worm rotates continuously around horizontal X-axis
            currentWormAngle += inputAngularVelDegPerSec * deltaTime;
            currentWormAngle %= 360f;

            // One full turn (360 deg) of 1-start worm advances wheel by 1 tooth (360 deg / 30 = 12 deg)
            // Ratio is 30:1. At 120 RPM input, output is 4.0 RPM (24 deg/sec)
            float speedRatio = (float)wormStarts / wheelTeeth;
            currentWheelAngle += (inputAngularVelDegPerSec * speedRatio) * deltaTime;
            currentWheelAngle %= 360f;

            if (wormRotor != null)
            {
                wormRotor.localRotation = Quaternion.AngleAxis(currentWormAngle, Vector3.right);
            }

            if (wheelRotor != null)
            {
                wheelRotor.localRotation = Quaternion.AngleAxis(currentWheelAngle, Vector3.forward);
            }
        }

        public override void ResetMechanism()
        {
            currentWormAngle = 0f;
            currentWheelAngle = 0f;
            if (wormRotor != null) wormRotor.localRotation = Quaternion.identity;
            if (wheelRotor != null) wheelRotor.localRotation = Quaternion.identity;
        }

        public override string GetCalculationSummary()
        {
            return $"• Worm Thread Starts (z₁): {wormStarts}\n" +
                   $"• Wheel Teeth (z₂): {wheelTeeth}\n" +
                   $"• Gear Ratio (i): {GearRatio:F0}:1 (Massive Reduction)\n" +
                   $"• Input Speed (n₁): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n₂): {OutputRPM:F1} RPM\n" +
                   $"• Torque Multiplier: 30× Mechanical Advantage\n" +
                   $"• Center Distance (a): 66.0 mm\n" +
                   $"• Drive Property: Self-Locking (Non-Backdrivable)";
        }

        public override string GetEngineeringExplanation()
        {
            return "A worm drive consists of a cylinder with a screw thread (worm) meshing with a toothed conjugate wheel. " +
                   "The shafts are non-intersecting and oriented at 90°. " +
                   "Because each 360° revolution of a single-start worm advances the wheel by only one tooth pitch, it achieves massive reduction (30:1) in a compact footprint. " +
                   "Crucially, because the static friction angle φ between phosphor bronze and case-hardened steel exceeds the helix lead angle γ (7.1°), the wheel cannot back-drive the worm, providing an inherent fail-safe holding brake.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Reduction Ratio: i = z₂ / z₁ = 30 / 1 = 30:1\n" +
                   "Output Speed: n₂ = n₁ / i = " + $"{currentInputRPM:F0} / 30 = {OutputRPM:F1} RPM\n" +
                   "Axial Lead: L = π·m·z₁ = π·3.5·1 ≈ 11.00 mm\n" +
                   "Lead Angle: tan(γ) = L / (π·d₁) = 11.0 / (π·27.0) ≈ 0.1297 => γ = 7.39°\n" +
                   "Self-Locking Condition: γ < arctan(μ) where μ ≈ 0.15 => 7.39° < 8.53° (Locking holds)";
        }
    }
}

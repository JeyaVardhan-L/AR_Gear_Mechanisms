using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 22: Scotch Yoke Mechanism.
    /// Demonstrates pure, mathematically ideal Simple Harmonic Motion (SHM) without higher-order rod angle harmonics:
    /// Displacement: x(theta) = r * cos(theta)
    /// Velocity:     v(theta) = -r * omega * sin(theta)
    /// Acceleration: a(theta) = -r * omega^2 * cos(theta)
    /// Components:
    /// - Continuous Rotating Crank with hardened pin (radius r = 35 mm)
    /// - Sliding Bronze Die-Block (translates vertically along the yoke slot)
    /// - Slotted Yoke Crosshead Frame with dual horizontal linear guide rods
    /// - Precision Ground Baseplate with linear guide bushings
    /// </summary>
    public class ScotchYokeMechanism : MechanismBase
    {
        [Header("Scotch Yoke Geometry")]
        [SerializeField] private float crankRadiusMeters = 0.040f;  // r = 40 mm (Total stroke S = 80 mm)
        [SerializeField] private float slotHeightMeters = 0.090f;   // Vertical slot height = 90 mm
        [SerializeField] private float slotWidthMeters = 0.016f;    // Vertical slot width = 16 mm
        [SerializeField] private float guideRodLength = 0.080f;

        [Header("Transforms")]
        [SerializeField] private Transform crankTransform;
        [SerializeField] private Transform yokeFrameTransform;
        [SerializeField] private Transform sliderBlockTransform;

        private float currentCrankAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public float CrankRadius => crankRadiusMeters;
        public float CrankAngleDeg => currentCrankAngleDeg;
        public float StrokeMeters => 2f * crankRadiusMeters;

        /// <summary>
        /// Exact sinusoidal displacement from center along horizontal X axis.
        /// Pure SHM: x(theta) = r * cos(theta)
        /// </summary>
        public float DisplacementX => crankRadiusMeters * Mathf.Cos(currentCrankAngleDeg * Mathf.Deg2Rad);

        /// <summary>
        /// Pure harmonic linear velocity: v(theta) = -r * omega * sin(theta) [m/s]
        /// </summary>
        public float VelocityMps => -crankRadiusMeters * InputAngularVelocityRadPerSec * Mathf.Sin(currentCrankAngleDeg * Mathf.Deg2Rad);

        /// <summary>
        /// Pure harmonic linear acceleration: a(theta) = -r * omega^2 * cos(theta) [m/s^2]
        /// </summary>
        public float AccelerationMps2 => -crankRadiusMeters * (InputAngularVelocityRadPerSec * InputAngularVelocityRadPerSec) * Mathf.Cos(currentCrankAngleDeg * Mathf.Deg2Rad);

        public override string MechanismName => "Scotch Yoke Mechanism";
        public override string CategoryTitle => "Pure Harmonic Reciprocating Converter";
        public override float GearRatio => 1.0f;
        public override float OutputRPM => currentInputRPM;

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r = crankRadiusMeters;

            // 1. Bedplate with Linear Bushing Stanchions
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0, -0.065f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.24f, 0.16f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.024f, 0.16f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Ground Mounting Bedplate",
                "Rigid foundation supporting crankshaft pillow blocks and twin horizontal guide bushings for the yoke.",
                "Material: Cast Carbon Steel | Precision Surface Ground");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // Left & Right Horizontal Guide Bushings (at X = -0.090m and X = +0.090m)
            float[] xBushings = new float[] { -0.095f, 0.095f };
            for (int i = 0; i < 2; i++)
            {
                GameObject bushGo = new GameObject($"GuideBushing_{i}");
                bushGo.transform.SetParent(transform, false);
                bushGo.transform.localPosition = new Vector3(xBushings[i], 0f, 0f);
                MeshFilter mfB = bushGo.AddComponent<MeshFilter>();
                MeshRenderer mrB = bushGo.AddComponent<MeshRenderer>();
                mfB.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.026f, 0.046f, 0.024f, 0.0065f);
                mrB.material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
                bushGo.AddComponent<BoxCollider>().size = new Vector3(0.026f, 0.046f, 0.024f);
            }

            // 2. Crankshaft Main Bearing Pillow Block (at 0,0,-0.025m)
            GameObject pbCrank = new GameObject("Crank_PillowBlock");
            pbCrank.transform.SetParent(transform, false);
            pbCrank.transform.localPosition = new Vector3(0, -0.022f, -0.035f);
            MeshFilter mfPb = pbCrank.AddComponent<MeshFilter>();
            MeshRenderer mrPb = pbCrank.AddComponent<MeshRenderer>();
            mfPb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.032f, 0.048f, 0.024f, 0.007f);
            mrPb.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            pbCrank.AddComponent<BoxCollider>().size = new Vector3(0.032f, 0.048f, 0.024f);

            // 3. Rotating Crank Assembly (Rotates around local Z at origin)
            GameObject crankRotor = new GameObject("Crank_Rotor");
            crankRotor.transform.SetParent(transform, false);
            crankRotor.transform.localPosition = Vector3.zero;
            crankTransform = crankRotor.transform;

            // Crank disk
            GameObject crankDisk = new GameObject("Crank_Disk");
            crankDisk.transform.SetParent(crankTransform, false);
            MeshFilter mfCrank = crankDisk.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankDisk.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildCylinderMesh(r + 0.012f, 0.014f, 32, false);
            mrCrank.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crankDisk.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankDisk.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Rotating Drive Crank Disk",
                "Spins continuously, driving crankpin on circular trajectory of radius r = 35 mm.",
                $"Radius r: {r * 1000f:F1} mm | Total Stroke: {StrokeMeters * 1000f:F1} mm | Speed: {currentInputRPM:F0} RPM");
            inspCrank.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            // Crank Pin (at local X = r, Y = 0 on crank disk)
            GameObject pinGo = new GameObject("Crank_Pin");
            pinGo.transform.SetParent(crankTransform, false);
            pinGo.transform.localPosition = new Vector3(r, 0f, 0.012f);
            MeshFilter mfPin = pinGo.AddComponent<MeshFilter>();
            MeshRenderer mrPin = pinGo.AddComponent<MeshRenderer>();
            mfPin.mesh = GearMeshBuilder.BuildCylinderMesh(0.006f, 0.024f, 20, false);
            mrPin.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            pinGo.AddComponent<CapsuleCollider>().height = 0.024f;

            // 4. Sliding Bronze Die-Block (rides on crankpin, slides inside yoke slot)
            GameObject blockGo = new GameObject("Sliding_Die_Block");
            blockGo.transform.SetParent(transform, false);
            sliderBlockTransform = blockGo.transform;
            MeshFilter mfBlock = blockGo.AddComponent<MeshFilter>();
            MeshRenderer mrBlock = blockGo.AddComponent<MeshRenderer>();
            mfBlock.mesh = GearMeshBuilder.BuildBasePlateMesh(slotWidthMeters * 0.95f, 0.018f, 0.028f);
            mrBlock.material = CreateMaterial(new Color(0.85f, 0.60f, 0.20f), 0.6f, 0.7f); // Bronze
            blockGo.AddComponent<BoxCollider>().size = new Vector3(slotWidthMeters, 0.028f, 0.018f);
            InspectableComponent inspBlock = blockGo.AddComponent<InspectableComponent>();
            inspBlock.SetMetadata("Sliding Bronze Die-Block",
                "Mounts over the rotating crankpin and slides vertically along the yoke slot, eliminating connecting rod angularity.",
                "Material: SAE 660 Bearing Bronze | Low Friction Sliding");
            inspBlock.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

            // 5. Slotted Yoke Crosshead Frame (Reciprocates horizontally along X axis)
            GameObject yokeGo = new GameObject("Slotted_Yoke_Frame");
            yokeGo.transform.SetParent(transform, false);
            yokeFrameTransform = yokeGo.transform;
            MeshFilter mfYoke = yokeGo.AddComponent<MeshFilter>();
            MeshRenderer mrYoke = yokeGo.AddComponent<MeshRenderer>();
            mfYoke.mesh = GearMeshBuilder.BuildScotchYokeFrameMesh(slotHeightMeters, slotWidthMeters, 0.016f, guideRodLength, 0.006f);
            mrYoke.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            yokeGo.AddComponent<MeshCollider>().sharedMesh = mfYoke.mesh;
            InspectableComponent inspYoke = yokeGo.AddComponent<InspectableComponent>();
            inspYoke.SetMetadata("Slotted Yoke Crosshead",
                "Translates purely along horizontal X axis in mathematically exact Simple Harmonic Motion (x = r·cosθ). Dual guide rods ensure zero tilting.",
                $"Stroke: {StrokeMeters * 1000f:F1} mm | Max Velocity: {crankRadiusMeters * InputAngularVelocityRadPerSec:F2} m/s | Forged Steel");
            inspYoke.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

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
            currentCrankAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCrankAngleDeg %= 360f;
            if (currentCrankAngleDeg < 0f) currentCrankAngleDeg += 360f;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
            float r = crankRadiusMeters;

            // 1. Crank Rotates around Z
            if (crankTransform != null)
            {
                crankTransform.localRotation = Quaternion.Euler(0f, 0f, currentCrankAngleDeg);
            }

            // 2. Yoke Translates Horizontally: x(theta) = r * cos(theta)
            float xYoke = r * Mathf.Cos(rad);
            if (yokeFrameTransform != null)
            {
                yokeFrameTransform.localPosition = new Vector3(xYoke, 0f, 0.012f);
            }

            // 3. Slider Die-Block follows crankpin: X = xYoke, Y = r * sin(theta)
            float yPin = r * Mathf.Sin(rad);
            if (sliderBlockTransform != null)
            {
                sliderBlockTransform.localPosition = new Vector3(xYoke, yPin, 0.012f);
            }
        }

        public override void ResetMechanism()
        {
            currentCrankAngleDeg = 0f; // Start at Maximum Positive Stroke (x = +r)
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Harmonic Motion: PURE SIMPLE HARMONIC MOTION (SHM) 〰️\n" +
                   $"• Crank Angle (θ): {currentCrankAngleDeg:F1}°\n" +
                   $"• Displacement (x): {DisplacementX * 1000f:F1} mm / {StrokeMeters * 1000f:F1} mm\n" +
                   $"• Velocity (v): {VelocityMps:F2} m/s\n" +
                   $"• Acceleration (a): {AccelerationMps2:F1} m/s²\n" +
                   $"• Crank Radius (r): {crankRadiusMeters * 1000f:F1} mm\n" +
                   $"• Drive Speed: {currentInputRPM:F0} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Scotch Yoke converts continuous rotary motion into strictly sinusoidal linear reciprocation. " +
                   "Unlike slider-crank linkages that introduce secondary harmonics due to connecting rod angularity, " +
                   "the transverse slot directly enforces pure Simple Harmonic Motion: x(θ) = r·cosθ with zero oblique side thrust on the piston.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Pure SHM Kinematics:\n" +
                   "Displacement: x(θ) = r·cosθ\n" +
                   $"x({currentCrankAngleDeg:F0}°) = {DisplacementX * 1000f:F1} mm\n" +
                   "Velocity: v(θ) = -r·ω·sinθ\n" +
                   $"v = {VelocityMps:F2} m/s (Max: {crankRadiusMeters * InputAngularVelocityRadPerSec:F2} m/s)\n" +
                   "Acceleration: a(θ) = -r·ω²·cosθ\n" +
                   $"a = {AccelerationMps2:F1} m/s² (Max: {crankRadiusMeters * InputAngularVelocityRadPerSec * InputAngularVelocityRadPerSec:F1} m/s²)";
        }
    }
}

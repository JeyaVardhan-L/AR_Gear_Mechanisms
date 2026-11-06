using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 24: Roller Chain and Sprocket Drive.
    /// Demonstrates positive-engagement power transmission without slip over long center distances.
    /// Driver Sprocket Z1 = 15 teeth, Driven Sprocket Z2 = 30 teeth (Velocity ratio 2:1 reduction).
    /// Both shafts rotate in the SAME direction.
    /// Analyzes chordal action (polygonal effect) caused by discrete roller links seating on pitch polygons.
    /// </summary>
    public class ChainAndSprocketDrive : MechanismBase
    {
        [Header("Sprocket & Chain Specifications")]
        [SerializeField] private int driverTeeth = 15;            // Z1 = 15 teeth
        [SerializeField] private int drivenTeeth = 30;            // Z2 = 30 teeth
        [SerializeField] private float chainPitchMeters = 0.0127f;// ANSI #40 / ISO 08B (0.500 in = 12.7 mm)
        [SerializeField] private float centerDistanceMeters = 0.160f; // C = 160 mm
        [SerializeField] private float sprocketThickness = 0.007f;
        [SerializeField] private float rollerRadius = 0.004f;

        [Header("Rotors & Kinematic Transforms")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform drivenRotor;
        [SerializeField] private MeshRenderer chainRenderer;

        private float currentDriverAngleDeg = 0f;
        private float currentDrivenAngleDeg = 0f;
        private float chainScrollOffset = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => driverTeeth;
        public override int DrivenTeeth => drivenTeeth;
        public float PitchMeters => chainPitchMeters;
        public float CenterDistance => centerDistanceMeters;

        // Pitch Radii: R = p / (2 * sin(pi / Z))
        public float DriverPitchRadius => (chainPitchMeters / Mathf.Sin(Mathf.PI / driverTeeth)) * 0.5f;
        public float DrivenPitchRadius => (chainPitchMeters / Mathf.Sin(Mathf.PI / drivenTeeth)) * 0.5f;

        /// <summary>
        /// Total geometric chain length in meters.
        /// L ≈ 2C + pi*(R1 + R2) + (R2 - R1)^2 / C
        /// </summary>
        public float ChainLengthMeters
        {
            get
            {
                float r1 = DriverPitchRadius;
                float r2 = DrivenPitchRadius;
                float c = centerDistanceMeters;
                return (2f * c) + (Mathf.PI * (r1 + r2)) + (((r2 - r1) * (r2 - r1)) / c);
            }
        }

        public int TotalChainLinks => Mathf.RoundToInt(ChainLengthMeters / chainPitchMeters);

        /// <summary>
        /// Mean linear chain velocity in m/s: v = (Z1 * p * N1) / 60
        /// </summary>
        public float ChainLinearSpeedMps => (driverTeeth * chainPitchMeters * currentInputRPM) / 60f;

        /// <summary>
        /// Chordal action speed variation percentage: (1 - cos(180° / Z1)) * 100%
        /// For Z1 = 15, fluctuation is 2.19%.
        /// </summary>
        public float ChordalActionVariationPercent => (1f - Mathf.Cos(Mathf.PI / driverTeeth)) * 100f;

        public override string MechanismName => "Chain & Sprocket Drive";
        public override string CategoryTitle => "Power Transmission & Mechanical Systems";
        public override float GearRatio => (float)drivenTeeth / driverTeeth; // 30 / 15 = 2.0
        public override float OutputRPM => currentInputRPM / GearRatio; // 60 RPM at 120 RPM input

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = DriverPitchRadius;
            float r2 = DrivenPitchRadius;
            float c = centerDistanceMeters;

            // 1. Machine Bedplate
            float bedLength = c + r1 + r2 + 0.08f;
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(c * 0.5f, -r2 - 0.025f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(bedLength, 0.12f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(bedLength, 0.020f, 0.12f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Ground Cast Bedplate",
                "Rigid foundation supporting driver and driven bearing pillow blocks at fixed center distance.",
                $"Center Distance C: {c * 1000f:F1} mm | Cast Iron ASTM A48");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Driver Pillow Block Mount (at X = 0)
            GameObject pbDriver = new GameObject("Driver_PillowBlock");
            pbDriver.transform.SetParent(transform, false);
            pbDriver.transform.localPosition = new Vector3(0f, -0.025f, -0.020f);
            MeshFilter mfPb1 = pbDriver.AddComponent<MeshFilter>();
            MeshRenderer mrPb1 = pbDriver.AddComponent<MeshRenderer>();
            mfPb1.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.032f, 0.046f, 0.020f, 0.008f);
            mrPb1.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            pbDriver.AddComponent<BoxCollider>().size = new Vector3(0.032f, 0.046f, 0.020f);
            InspectableComponent inspPb1 = pbDriver.AddComponent<InspectableComponent>();
            inspPb1.SetMetadata("Driver Bearing Block",
                "Deep groove ball bearing supporting the 15-tooth input driver shaft.",
                "Bore: 16mm | C0: 5.4 kN");
            inspPb1.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // 3. Driven Pillow Block Mount (at X = c)
            GameObject pbDriven = new GameObject("Driven_PillowBlock");
            pbDriven.transform.SetParent(transform, false);
            pbDriven.transform.localPosition = new Vector3(c, -0.025f, -0.020f);
            MeshFilter mfPb2 = pbDriven.AddComponent<MeshFilter>();
            MeshRenderer mrPb2 = pbDriven.AddComponent<MeshRenderer>();
            mfPb2.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.036f, 0.046f, 0.020f, 0.010f);
            mrPb2.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            pbDriven.AddComponent<BoxCollider>().size = new Vector3(0.036f, 0.046f, 0.020f);
            InspectableComponent inspPb2 = pbDriven.AddComponent<InspectableComponent>();
            inspPb2.SetMetadata("Driven Bearing Block",
                "Heavy-duty pillow block supporting the 30-tooth output driven shaft.",
                "Bore: 20mm | C0: 8.2 kN");
            inspPb2.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // 4. Driver Sprocket Rotor (Rotates around local Z at 0,0,0)
            GameObject dRotorGo = new GameObject("Driver_Sprocket_Rotor");
            dRotorGo.transform.SetParent(transform, false);
            dRotorGo.transform.localPosition = Vector3.zero;
            driverRotor = dRotorGo.transform;

            GameObject dSpGo = new GameObject("Driver_Sprocket");
            dSpGo.transform.SetParent(driverRotor, false);
            MeshFilter mfDSp = dSpGo.AddComponent<MeshFilter>();
            MeshRenderer mrDSp = dSpGo.AddComponent<MeshRenderer>();
            mfDSp.mesh = GearMeshBuilder.BuildSprocketMesh(driverTeeth, chainPitchMeters, rollerRadius, sprocketThickness, 0.008f);
            mrDSp.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            dSpGo.AddComponent<MeshCollider>().sharedMesh = mfDSp.mesh;
            InspectableComponent inspDSp = dSpGo.AddComponent<InspectableComponent>();
            inspDSp.SetMetadata("Input Driving Sprocket",
                "Hardened steel sprocket driving the roller chain with positive tooth engagement.",
                $"Teeth Z1: {driverTeeth} | Pitch Dia D1: {r1 * 2000f:F1} mm | ANSI #40");
            inspDSp.SetExplosionVector(Vector3.forward, 0.07f, 0.11f);

            // Driver Shaft
            GameObject dShaftGo = new GameObject("Driver_Shaft");
            dShaftGo.transform.SetParent(driverRotor, false);
            dShaftGo.transform.localPosition = new Vector3(0f, 0f, -0.015f);
            dShaftGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MeshFilter mfDShaft = dShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrDShaft = dShaftGo.AddComponent<MeshRenderer>();
            mfDShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.008f, 0.050f, 20, true);
            mrDShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 5. Driven Sprocket Rotor (Rotates around local Z at c,0,0)
            GameObject outRotorGo = new GameObject("Driven_Sprocket_Rotor");
            outRotorGo.transform.SetParent(transform, false);
            outRotorGo.transform.localPosition = new Vector3(c, 0f, 0f);
            drivenRotor = outRotorGo.transform;

            GameObject outSpGo = new GameObject("Driven_Sprocket");
            outSpGo.transform.SetParent(drivenRotor, false);
            MeshFilter mfOutSp = outSpGo.AddComponent<MeshFilter>();
            MeshRenderer mrOutSp = outSpGo.AddComponent<MeshRenderer>();
            mfOutSp.mesh = GearMeshBuilder.BuildSprocketMesh(drivenTeeth, chainPitchMeters, rollerRadius, sprocketThickness, 0.010f);
            mrOutSp.material = CreateMaterial(new Color(0.20f, 0.60f, 0.85f), 0.7f, 0.65f); // Tech Blue
            outSpGo.AddComponent<MeshCollider>().sharedMesh = mfOutSp.mesh;
            InspectableComponent inspOutSp = outSpGo.AddComponent<InspectableComponent>();
            inspOutSp.SetMetadata("Output Driven Sprocket",
                "Large precision sprocket delivering high output torque at reduced rotational speed.",
                $"Teeth Z2: {drivenTeeth} | Pitch Dia D2: {r2 * 2000f:F1} mm | Ratio: 2.0:1");
            inspOutSp.SetExplosionVector(Vector3.forward, 0.07f, 0.11f);

            // Driven Shaft
            GameObject outShaftGo = new GameObject("Driven_Shaft");
            outShaftGo.transform.SetParent(drivenRotor, false);
            outShaftGo.transform.localPosition = new Vector3(0f, 0f, -0.015f);
            outShaftGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MeshFilter mfOutShaft = outShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrOutShaft = outShaftGo.AddComponent<MeshRenderer>();
            mfOutShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.010f, 0.050f, 20, true);
            mrOutShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 6. Continuous Looping Roller Chain
            GameObject chainGo = new GameObject("Roller_Chain_Loop");
            chainGo.transform.SetParent(transform, false);
            chainGo.transform.localPosition = Vector3.zero;
            MeshFilter mfChain = chainGo.AddComponent<MeshFilter>();
            chainRenderer = chainGo.AddComponent<MeshRenderer>();
            mfChain.mesh = GearMeshBuilder.BuildRollerChainLoopMesh(c, r1, r2, 0.005f, 0.012f);
            chainRenderer.material = CreateMaterial(new Color(0.70f, 0.72f, 0.76f), 0.8f, 0.70f); // Polished Steel Link Plate
            chainGo.AddComponent<MeshCollider>().sharedMesh = mfChain.mesh;
            InspectableComponent inspChain = chainGo.AddComponent<InspectableComponent>();
            inspChain.SetMetadata("Precision Roller Chain Loop",
                "Continuous linked bushing chain transmitting tension forces directly without slip.",
                $"Pitch p: {chainPitchMeters * 1000f:F1} mm | Total Links: {TotalChainLinks} | Length: {ChainLengthMeters * 1000f:F1} mm");
            inspChain.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

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
            float deltaDeg = inputAngularVelDegPerSec * deltaTime;
            currentDriverAngleDeg += deltaDeg;
            currentDriverAngleDeg %= 360f;
            if (currentDriverAngleDeg < 0f) currentDriverAngleDeg += 360f;

            // Both sprockets rotate in the same direction!
            // Ratio = Z2 / Z1 = 2.0 -> driven speed = input speed / 2.0
            float ratio = GearRatio;
            currentDrivenAngleDeg += (deltaDeg / ratio);
            currentDrivenAngleDeg %= 360f;
            if (currentDrivenAngleDeg < 0f) currentDrivenAngleDeg += 360f;

            // Scroll chain texture / UV offset proportional to chain travel
            float linearTravelMeters = (deltaDeg * Mathf.Deg2Rad) * DriverPitchRadius;
            chainScrollOffset += (linearTravelMeters / ChainLengthMeters);
            chainScrollOffset %= 1f;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            if (driverRotor != null)
            {
                driverRotor.localRotation = Quaternion.Euler(0f, 0f, -currentDriverAngleDeg);
            }

            if (drivenRotor != null)
            {
                drivenRotor.localRotation = Quaternion.Euler(0f, 0f, -currentDrivenAngleDeg);
            }

            if (chainRenderer != null && chainRenderer.material != null)
            {
                chainRenderer.material.mainTextureOffset = new Vector2(chainScrollOffset * TotalChainLinks, 0f);
            }
        }

        public override void ResetMechanism()
        {
            currentDriverAngleDeg = 0f;
            currentDrivenAngleDeg = 0f;
            chainScrollOffset = 0f;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Driver Sprocket (Z₁): {driverTeeth} teeth | Pitch Dia: {DriverPitchRadius * 2000f:F1} mm\n" +
                   $"• Driven Sprocket (Z₂): {drivenTeeth} teeth | Pitch Dia: {DrivenPitchRadius * 2000f:F1} mm\n" +
                   $"• Center Distance (C): {centerDistanceMeters * 1000f:F1} mm\n" +
                   $"• Velocity Ratio (i): {GearRatio:F2}:1 (Speed Reduction)\n" +
                   $"• Chain Linear Speed: {ChainLinearSpeedMps:F2} m/s ({ChainLinearSpeedMps * 3.6f:F1} km/h)\n" +
                   $"• Total Chain Length: {ChainLengthMeters * 1000f:F1} mm ({TotalChainLinks} pitches)\n" +
                   $"• Chordal Speed Variation: {ChordalActionVariationPercent:F2}%\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM | Output Speed: {OutputRPM:F0} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Roller Chain and Sprocket drive transmits mechanical power across moderate center distances with 100% positive synchronization and zero slip. " +
                   "Because the chain seats on the straight chordal segments of the sprocket polygon rather than a true circle, the effective radius fluctuates cyclically as each tooth engages. " +
                   "This phenomenon—termed Chordal Action or the Polygonal Effect—causes a 2.19% velocity ripple on the 15-tooth driver, exciting structural vibrations at high speeds. " +
                   "To minimize chordal action, high-speed industrial drives utilize sprockets with at least 17 to 21 teeth.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Speed Ratio: i = Z₂ / Z₁ = 30 / 15 = 2.0:1\n" +
                   $"Output Speed: N₂ = N₁ / i = {OutputRPM:F1} RPM\n" +
                   "Chain Pitch Velocity: v = (Z₁·p·N₁) / 60\n" +
                   $"v = ({driverTeeth} × {chainPitchMeters * 1000f:F1}mm × {currentInputRPM:F0} RPM) / 60 = {ChainLinearSpeedMps:F2} m/s\n" +
                   "Chordal Variation: Δv/v = 1 - cos(180° / Z₁)\n" +
                   $"Δv/v = 1 - cos({180f / driverTeeth:F1}°) = {ChordalActionVariationPercent:F2}%";
        }
    }
}
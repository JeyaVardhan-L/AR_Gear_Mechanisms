using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 23: Universal Joint (Cardan / Hooke's Joint).
    /// Transmits rotary motion between two non-collinear intersecting shafts at operating angle beta.
    /// Demonstrates the classic Cardan kinematic velocity fluctuation:
    /// tan(theta2) = tan(theta1) * cos(beta)
    /// omega2 / omega1 = cos(beta) / (1 - sin^2(beta) * cos^2(theta1))
    /// Output velocity oscillates twice per revolution with peak speed sec(beta) and trough speed cos(beta).
    /// </summary>
    public class UniversalJoint : MechanismBase
    {
        [Header("Universal Joint Geometry")]
        [Range(0f, 35f)]
        [SerializeField] private float operatingAngleDeg = 20.0f; // Beta angle (default 20°)
        [SerializeField] private float shaftRadius = 0.008f;
        [SerializeField] private float shaftLength = 0.070f;
        [SerializeField] private float spiderSpan = 0.038f;
        [SerializeField] private float spiderPinRadius = 0.004f;
        [SerializeField] private float forkSpan = 0.034f;
        [SerializeField] private float forkThickness = 0.010f;

        [Header("Rotors & Kinematic Transforms")]
        [SerializeField] private Transform inputShaftRotor;
        [SerializeField] private Transform outputShaftRotor;
        [SerializeField] private Transform spiderTransform;

        private float currentInputAngleDeg = 0f;
        private float currentOutputAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public float OperatingAngleDeg
        {
            get => operatingAngleDeg;
            set
            {
                operatingAngleDeg = Mathf.Clamp(value, 0f, 35f);
                UpdateOutputShaftMountAngle();
                UpdateComponentTransforms();
                NotifyKinematicsUpdated();
            }
        }

        public float InputAngleDeg => currentInputAngleDeg;
        public float OutputAngleDeg => currentOutputAngleDeg;

        /// <summary>
        /// Instantaneous angular velocity ratio omega2 / omega1.
        /// Peaks at theta1 = 0, 180°: omega2/omega1 = 1 / cos(beta).
        /// Drops at theta1 = 90, 270°: omega2/omega1 = cos(beta).
        /// </summary>
        public float InstantaneousVelocityRatio
        {
            get
            {
                float betaRad = operatingAngleDeg * Mathf.Deg2Rad;
                float theta1Rad = currentInputAngleDeg * Mathf.Deg2Rad;
                float cosBeta = Mathf.Cos(betaRad);
                float sinBeta = Mathf.Sin(betaRad);
                float cosTheta1 = Mathf.Cos(theta1Rad);
                float denom = 1f - (sinBeta * sinBeta * cosTheta1 * cosTheta1);
                return denom > 0.0001f ? (cosBeta / denom) : 1f;
            }
        }

        public float MaxSpeedRatio => 1f / Mathf.Cos(operatingAngleDeg * Mathf.Deg2Rad);
        public float MinSpeedRatio => Mathf.Cos(operatingAngleDeg * Mathf.Deg2Rad);
        public float SpeedFluctuationPercent => (MaxSpeedRatio - MinSpeedRatio) / 1.0f * 100f;

        public override string MechanismName => "Universal Joint (Cardan)";
        public override string CategoryTitle => "Power Transmission & Mechanical Systems";
        public override float GearRatio => 1.0f; // 1:1 average ratio
        public override float OutputRPM => currentInputRPM * InstantaneousVelocityRatio;

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Ground_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0.02f, -0.045f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.24f, 0.14f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.020f, 0.14f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Machined Baseplate",
                "Rigid foundation supporting input and angled output bearing pillow blocks.",
                "Material: Cast Iron ASTM A48 | Black Oxide");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Input Shaft & Yoke Mount (Input Axis along +Z, located at -Z)
            GameObject inMount = new GameObject("Input_BearingMount");
            inMount.transform.SetParent(transform, false);
            inMount.transform.localPosition = new Vector3(0f, -0.020f, -shaftLength * 0.7f);
            MeshFilter mfInM = inMount.AddComponent<MeshFilter>();
            MeshRenderer mrInM = inMount.AddComponent<MeshRenderer>();
            mfInM.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.034f, 0.045f, 0.020f, shaftRadius);
            mrInM.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            inMount.AddComponent<BoxCollider>().size = new Vector3(0.034f, 0.045f, 0.020f);
            InspectableComponent inspInM = inMount.AddComponent<InspectableComponent>();
            inspInM.SetMetadata("Input Journal Bearing Block",
                "Deep groove ball bearing supporting the driving input shaft.",
                "Bore: 16mm | C0: 4.8 kN");
            inspInM.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            // Input Shaft Rotor (rotates around local Z)
            GameObject inRotorGo = new GameObject("Input_Shaft_Rotor");
            inRotorGo.transform.SetParent(transform, false);
            inRotorGo.transform.localPosition = Vector3.zero;
            inputShaftRotor = inRotorGo.transform;

            // Input Shaft Cylinder
            GameObject inShaftGo = new GameObject("Input_Shaft");
            inShaftGo.transform.SetParent(inputShaftRotor, false);
            inShaftGo.transform.localPosition = new Vector3(0f, 0f, -shaftLength * 0.5f);
            inShaftGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MeshFilter mfInShaft = inShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrInShaft = inShaftGo.AddComponent<MeshRenderer>();
            mfInShaft.mesh = GearMeshBuilder.BuildCylinderMesh(shaftRadius, shaftLength, 24, true);
            mrInShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            inShaftGo.AddComponent<BoxCollider>().size = new Vector3(shaftRadius * 2f, shaftLength, shaftRadius * 2f);

            // Input Driving Fork Yoke
            GameObject inYokeGo = new GameObject("Input_Yoke");
            inYokeGo.transform.SetParent(inputShaftRotor, false);
            inYokeGo.transform.localPosition = Vector3.zero;
            MeshFilter mfInYoke = inYokeGo.AddComponent<MeshFilter>();
            MeshRenderer mrInYoke = inYokeGo.AddComponent<MeshRenderer>();
            mfInYoke.mesh = GearMeshBuilder.BuildCardanYokeMesh(shaftRadius * 1.6f, 0.020f, forkSpan, forkThickness, spiderPinRadius);
            mrInYoke.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            inYokeGo.AddComponent<MeshCollider>().sharedMesh = mfInYoke.mesh;
            InspectableComponent inspInYoke = inYokeGo.AddComponent<InspectableComponent>();
            inspInYoke.SetMetadata("Input Driving Fork Yoke",
                "Forged chrome-moly steel yoke coupled directly to the input driving shaft.",
                "Material: 4140 Alloy Steel | Prongs aligned along X axis");
            inspInYoke.SetExplosionVector(Vector3.back, 0.08f, 0.12f);

            // 3. Output Shaft Pivot Root & Bearing Mount (Tilted by operatingAngleDeg)
            GameObject outRootGo = new GameObject("Output_Assembly_Root");
            outRootGo.transform.SetParent(transform, false);
            outRootGo.transform.localPosition = Vector3.zero;
            outputShaftRotor = outRootGo.transform;

            GameObject outMount = new GameObject("Output_BearingMount");
            outMount.transform.SetParent(outputShaftRotor, false);
            outMount.transform.localPosition = new Vector3(0f, -0.020f, shaftLength * 0.7f);
            MeshFilter mfOutM = outMount.AddComponent<MeshFilter>();
            MeshRenderer mrOutM = outMount.AddComponent<MeshRenderer>();
            mfOutM.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.034f, 0.045f, 0.020f, shaftRadius);
            mrOutM.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            outMount.AddComponent<BoxCollider>().size = new Vector3(0.034f, 0.045f, 0.020f);
            InspectableComponent inspOutM = outMount.AddComponent<InspectableComponent>();
            inspOutM.SetMetadata("Output Journal Bearing Block",
                "Tilted support housing maintaining angular alignment for the driven shaft.",
                $"Operating Angle β: {operatingAngleDeg:F1}°");
            inspOutM.SetExplosionVector(Vector3.forward, 0.07f, 0.11f);

            // Output Driven Fork Yoke & Shaft (Rotates around output root's local Z)
            GameObject outShaftGo = new GameObject("Output_Shaft");
            outShaftGo.transform.SetParent(outputShaftRotor, false);
            outShaftGo.transform.localPosition = new Vector3(0f, 0f, shaftLength * 0.5f);
            outShaftGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MeshFilter mfOutShaft = outShaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrOutShaft = outShaftGo.AddComponent<MeshRenderer>();
            mfOutShaft.mesh = GearMeshBuilder.BuildCylinderMesh(shaftRadius, shaftLength, 24, true);
            mrOutShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            outShaftGo.AddComponent<BoxCollider>().size = new Vector3(shaftRadius * 2f, shaftLength, shaftRadius * 2f);

            // Output Driven Yoke (Prongs rotated 90° relative to input yoke)
            GameObject outYokeGo = new GameObject("Output_Yoke");
            outYokeGo.transform.SetParent(outputShaftRotor, false);
            outYokeGo.transform.localPosition = Vector3.zero;
            outYokeGo.transform.localRotation = Quaternion.Euler(0f, 180f, 90f); // Points towards origin, prongs along Y
            MeshFilter mfOutYoke = outYokeGo.AddComponent<MeshFilter>();
            MeshRenderer mrOutYoke = outYokeGo.AddComponent<MeshRenderer>();
            mfOutYoke.mesh = GearMeshBuilder.BuildCardanYokeMesh(shaftRadius * 1.6f, 0.020f, forkSpan, forkThickness, spiderPinRadius);
            mrOutYoke.material = CreateMaterial(new Color(0.20f, 0.60f, 0.85f), 0.7f, 0.65f); // Tech Blue
            outYokeGo.AddComponent<MeshCollider>().sharedMesh = mfOutYoke.mesh;
            InspectableComponent inspOutYoke = outYokeGo.AddComponent<InspectableComponent>();
            inspOutYoke.SetMetadata("Output Driven Fork Yoke",
                "Receives fluctuating torque from cross spider, exhibiting 2nd harmonic velocity ripple.",
                "Material: 4140 Alloy Steel | Prongs oriented at 90° to driver");
            inspOutYoke.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 4. Central Spider Cross Trunnion
            GameObject spiderGo = new GameObject("Cardan_Spider_Cross");
            spiderGo.transform.SetParent(transform, false);
            spiderGo.transform.localPosition = Vector3.zero;
            spiderTransform = spiderGo.transform;
            MeshFilter mfSpider = spiderGo.AddComponent<MeshFilter>();
            MeshRenderer mrSpider = spiderGo.AddComponent<MeshRenderer>();
            mfSpider.mesh = GearMeshBuilder.BuildCardanSpiderMesh(spiderPinRadius, spiderSpan);
            mrSpider.material = CreateMaterial(new Color(0.95f, 0.75f, 0.25f), 0.8f, 0.75f); // Polished Brass/Bronze
            spiderGo.AddComponent<BoxCollider>().size = new Vector3(spiderSpan, spiderSpan, spiderPinRadius * 3f);
            InspectableComponent inspSpider = spiderGo.AddComponent<InspectableComponent>();
            inspSpider.SetMetadata("Cardan Spider Cross Trunnion",
                "Cruciform trunnion fitted with needle roller bearings transmitting torque between non-collinear yokes.",
                "Journal Span: 38mm | Needle Roller Bearings");
            inspSpider.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

            UpdateOutputShaftMountAngle();
            ResetMechanism();
        }

        private void UpdateOutputShaftMountAngle()
        {
            if (outputShaftRotor != null)
            {
                // Tilt output shaft around Y axis by operating angle beta
                outputShaftRotor.localRotation = Quaternion.Euler(0f, operatingAngleDeg, 0f);
            }
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
            currentInputAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentInputAngleDeg %= 360f;
            if (currentInputAngleDeg < 0f) currentInputAngleDeg += 360f;

            // Calculate output shaft angle from Cardan equation:
            // tan(theta2) = tan(theta1) * cos(beta)
            float betaRad = operatingAngleDeg * Mathf.Deg2Rad;
            float theta1Rad = currentInputAngleDeg * Mathf.Deg2Rad;
            float cosBeta = Mathf.Cos(betaRad);

            // Compute principal angle in [0, 2pi)
            float theta2Principal = Mathf.Atan2(Mathf.Sin(theta1Rad) * cosBeta, Mathf.Cos(theta1Rad));
            if (theta2Principal < 0f) theta2Principal += 2f * Mathf.PI;
            currentOutputAngleDeg = theta2Principal * Mathf.Rad2Deg;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            if (inputShaftRotor != null)
            {
                inputShaftRotor.localRotation = Quaternion.Euler(0f, 0f, currentInputAngleDeg);
            }

            if (outputShaftRotor != null)
            {
                // Output shaft is tilted around Y by operatingAngleDeg, and spins around its tilted local Z by currentOutputAngleDeg
                outputShaftRotor.localRotation = Quaternion.Euler(0f, operatingAngleDeg, 0f) * Quaternion.Euler(0f, 0f, currentOutputAngleDeg);
            }

            if (spiderTransform != null)
            {
                // Kinematic constraint: Spider's X-axis is aligned with input yoke (which rotates around Z)
                // Spider's Y-axis is aligned with output yoke (which is tilted and rotated)
                Vector3 inputCrossPin = inputShaftRotor != null ? inputShaftRotor.TransformDirection(Vector3.right) : Vector3.right;
                Vector3 outputCrossPin = outputShaftRotor != null ? outputShaftRotor.TransformDirection(Vector3.up) : Vector3.up;

                Vector3 normal = Vector3.Cross(inputCrossPin, outputCrossPin).normalized;
                if (normal.sqrMagnitude > 0.001f)
                {
                    spiderTransform.rotation = Quaternion.LookRotation(normal, outputCrossPin);
                }
            }
        }

        public override void ResetMechanism()
        {
            currentInputAngleDeg = 0f;
            currentOutputAngleDeg = 0f;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Operating Shaft Angle (β): {operatingAngleDeg:F1}°\n" +
                   $"• Input Angle (θ₁): {currentInputAngleDeg:F1}°\n" +
                   $"• Output Angle (θ₂): {currentOutputAngleDeg:F1}°\n" +
                   $"• Instantaneous Speed Ratio (ω₂/ω₁): {InstantaneousVelocityRatio:F3}\n" +
                   $"• Peak Speed Ratio [sec(β)]: {MaxSpeedRatio:F3}x\n" +
                   $"• Trough Speed Ratio [cos(β)]: {MinSpeedRatio:F3}x\n" +
                   $"• Speed Fluctuation: ±{SpeedFluctuationPercent * 0.5f:F1}%\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM | Output Speed: {OutputRPM:F0} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Cardan / Universal Joint transmits rotary power across intersecting, non-collinear shaft axes. " +
                   "Because the cross spider tilts through an angle β relative to the driver yoke, the instantaneous output angular velocity fluctuates twice per revolution (2nd harmonic). " +
                   "When the input yoke pins lie in the plane of the shaft bend (θ₁ = 0°, 180°), the output spins faster at ω₂ = ω₁·sec(β). " +
                   "When perpendicular (θ₁ = 90°, 270°), it spins slower at ω₂ = ω₁·cos(β). " +
                   "In automotive driveshafts, dual Cardan joints with matched operating angles are arranged in series to cancel out this cyclic speed fluctuation.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Kinematic Relationship: tan(θ₂) = tan(θ₁)·cos(β)\n" +
                   $"θ₂({currentInputAngleDeg:F0}°) = {currentOutputAngleDeg:F1}° (Phase error: {currentOutputAngleDeg - currentInputAngleDeg:F1}°)\n" +
                   "Instantaneous Ratio: ω₂/ω₁ = cos(β) / (1 - sin²(β)·cos²(θ₁))\n" +
                   $"Current Ratio: {InstantaneousVelocityRatio:F3}\n" +
                   $"Speed Fluctuation: Δω/ω = sec(β) - cos(β) = {MaxSpeedRatio - MinSpeedRatio:F3}";
        }
    }
}
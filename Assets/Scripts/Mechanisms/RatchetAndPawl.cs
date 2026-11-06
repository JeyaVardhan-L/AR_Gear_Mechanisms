using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    public enum RatchetCycleState
    {
        DrivingStroke = 0, // Driving pawl pushes wheel forward by 1 pitch (15°)
        ReturnStroke = 1   // Driving pawl slips backward; holding pawl locks wheel stationary
    }

    /// <summary>
    /// Mechanism 25: Ratchet and Pawl Indexing Mechanism.
    /// Converts oscillating or continuous eccentric input into unidirectional, non-reversible step-wise rotation.
    /// Contains:
    /// - 24-tooth asymmetric sawtooth Ratchet Wheel (15° pitch angle per tooth)
    /// - Driving Rocker Arm with Spring-Loaded Driving Pawl (advances wheel on forward stroke)
    /// - Stationary Frame with Spring-Loaded Holding/Detent Pawl (prevents reverse motion)
    /// - Mechanical Ratio: 24:1 (24 input cycles per 1 complete 360° output revolution)
    /// </summary>
    public class RatchetAndPawl : MechanismBase
    {
        [Header("Ratchet Wheel Geometry")]
        [SerializeField] private int toothCount = 24;                  // Z = 24 teeth (15° pitch)
        [SerializeField] private float wheelRadius = 0.050f;           // R = 50 mm
        [SerializeField] private float toothDepth = 0.007f;            // 7 mm tooth height
        [SerializeField] private float wheelThickness = 0.012f;
        [SerializeField] private float boreRadius = 0.010f;

        [Header("Pawl & Arm Geometry")]
        [SerializeField] private float armLength = 0.065f;
        [SerializeField] private float pawlLength = 0.028f;
        [SerializeField] private float pawlWidth = 0.007f;

        [Header("Transforms")]
        [SerializeField] private Transform ratchetWheelRotor;
        [SerializeField] private Transform drivingArmRotor;
        [SerializeField] private Transform drivingPawlTransform;
        [SerializeField] private Transform holdingPawlTransform;

        private float currentCycleAngleDeg = 0f;
        private float currentWheelAngleDeg = 0f;
        private int totalStepsIndexed = 0;
        private bool isAssemblyBuilt = false;

        public int ToothCount => toothCount;
        public float WheelRadiusMeters => wheelRadius;
        public float PitchAngleDeg => 360f / toothCount; // 15°
        public int TotalSteps => totalStepsIndexed;
        public float CumulativeWheelAngleDeg => currentWheelAngleDeg;

        public bool IsDrivingStroke => (currentCycleAngleDeg < 180f);
        public RatchetCycleState CurrentState => IsDrivingStroke ? RatchetCycleState.DrivingStroke : RatchetCycleState.ReturnStroke;

        public override string MechanismName => "Ratchet & Pawl Mechanism";
        public override string CategoryTitle => "Mechanical Drives & Clutches";
        public override float GearRatio => toothCount; // 24:1 reduction
        public override float OutputRPM => currentInputRPM / toothCount; // 5 RPM at 120 RPM input

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r = wheelRadius;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0.01f, -r - 0.025f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.18f, 0.12f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.18f, 0.020f, 0.12f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Machined Baseplate",
                "Rigid foundation supporting ratchet axle and stationary holding pawl pivot bracket.",
                "Material: Cast Iron ASTM A48 | Planed Mounting Pads");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Main Bearing Pillow Block
            GameObject pbGo = new GameObject("Main_PillowBlock");
            pbGo.transform.SetParent(transform, false);
            pbGo.transform.localPosition = new Vector3(0f, -0.025f, -0.022f);
            MeshFilter mfPb = pbGo.AddComponent<MeshFilter>();
            MeshRenderer mrPb = pbGo.AddComponent<MeshRenderer>();
            mfPb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.034f, 0.046f, 0.020f, boreRadius);
            mrPb.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            pbGo.AddComponent<BoxCollider>().size = new Vector3(0.034f, 0.046f, 0.020f);
            InspectableComponent inspPb = pbGo.AddComponent<InspectableComponent>();
            inspPb.SetMetadata("Axle Journal Bearing",
                "Deep groove ball bearing mounting the unidirectional ratchet output shaft.",
                "Bore: 20mm | Load Rating: 6.4 kN");
            inspPb.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // 3. Central Output Shaft & Ratchet Wheel (Rotates around local Z)
            GameObject wheelRotorGo = new GameObject("Ratchet_Wheel_Rotor");
            wheelRotorGo.transform.SetParent(transform, false);
            wheelRotorGo.transform.localPosition = Vector3.zero;
            ratchetWheelRotor = wheelRotorGo.transform;

            GameObject wheelGo = new GameObject("Sawtooth_Ratchet_Wheel");
            wheelGo.transform.SetParent(ratchetWheelRotor, false);
            MeshFilter mfWheel = wheelGo.AddComponent<MeshFilter>();
            MeshRenderer mrWheel = wheelGo.AddComponent<MeshRenderer>();
            mfWheel.mesh = GearMeshBuilder.BuildRatchetWheelMesh(toothCount, wheelRadius, toothDepth, wheelThickness, boreRadius);
            mrWheel.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            wheelGo.AddComponent<MeshCollider>().sharedMesh = mfWheel.mesh;
            InspectableComponent inspWheel = wheelGo.AddComponent<InspectableComponent>();
            inspWheel.SetMetadata("Asymmetric Sawtooth Ratchet Wheel",
                "Precision machined wheel featuring 24 undercut teeth for positive one-way locking.",
                $"Teeth Z: {toothCount} | Pitch Angle: {PitchAngleDeg:F1}° | Hardness: 58 HRC");
            inspWheel.SetExplosionVector(Vector3.forward, 0.07f, 0.11f);

            // Output Shaft
            GameObject shaftGo = new GameObject("Output_Shaft");
            shaftGo.transform.SetParent(ratchetWheelRotor, false);
            shaftGo.transform.localPosition = new Vector3(0f, 0f, -0.015f);
            shaftGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MeshFilter mfShaft = shaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrShaft = shaftGo.AddComponent<MeshRenderer>();
            mfShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.055f, 20, true);
            mrShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 4. Oscillating Driving Rocker Arm (Coaxial with wheel, oscillates ±7.5° to advance 15° each cycle)
            GameObject armRotorGo = new GameObject("Driving_Rocker_Arm");
            armRotorGo.transform.SetParent(transform, false);
            armRotorGo.transform.localPosition = new Vector3(0f, 0f, 0.015f);
            drivingArmRotor = armRotorGo.transform;

            GameObject armBody = new GameObject("Arm_Body");
            armBody.transform.SetParent(drivingArmRotor, false);
            armBody.transform.localPosition = new Vector3(0f, armLength * 0.5f, 0f);
            MeshFilter mfArm = armBody.AddComponent<MeshFilter>();
            MeshRenderer mrArm = armBody.AddComponent<MeshRenderer>();
            mfArm.mesh = GearMeshBuilder.BuildLinkageBarMesh(armLength, 0.014f, 0.008f, 0.005f);
            mrArm.material = CreateMaterial(new Color(0.20f, 0.60f, 0.85f), 0.7f, 0.65f); // Tech Blue
            armBody.AddComponent<BoxCollider>().size = new Vector3(0.014f, armLength, 0.008f);
            InspectableComponent inspArm = armBody.AddComponent<InspectableComponent>();
            inspArm.SetMetadata("Oscillating Driving Rocker Arm",
                "Input lever oscillated by eccentric crank, sweeping through one tooth pitch (15°) per stroke.",
                $"Sweep Angle: {PitchAngleDeg:F1}° | Arm Length: {armLength * 1000f:F1} mm");
            inspArm.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

            // Driving Pawl mounted on rocker arm tip
            GameObject drivePawlGo = new GameObject("Driving_Pawl");
            drivePawlGo.transform.SetParent(drivingArmRotor, false);
            drivePawlGo.transform.localPosition = new Vector3(0f, armLength, 0f);
            drivingPawlTransform = drivePawlGo.transform;
            MeshFilter mfDPawl = drivePawlGo.AddComponent<MeshFilter>();
            MeshRenderer mrDPawl = drivePawlGo.AddComponent<MeshRenderer>();
            mfDPawl.mesh = GearMeshBuilder.BuildPawlMesh(pawlLength, pawlWidth, 0.004f, 0.006f);
            mrDPawl.material = CreateMaterial(new Color(0.95f, 0.75f, 0.25f), 0.8f, 0.75f); // Brass/Bronze
            drivePawlGo.AddComponent<BoxCollider>().size = new Vector3(pawlLength, pawlWidth, 0.006f);
            InspectableComponent inspDPawl = drivePawlGo.AddComponent<InspectableComponent>();
            inspDPawl.SetMetadata("Driving Push Pawl",
                "Spring-loaded hardened steel finger that engages the vertical tooth face on the forward stroke.",
                "Case Hardened 8620 Steel | Torsion Spring Loaded");
            inspDPawl.SetExplosionVector(Vector3.right, 0.06f, 0.10f);

            // 5. Stationary Holding / Detent Pawl (Mounted to bedplate at bottom right)
            GameObject holdBracket = new GameObject("Holding_Pawl_Bracket");
            holdBracket.transform.SetParent(transform, false);
            holdBracket.transform.localPosition = new Vector3(r * 0.95f, -0.015f, 0f);

            GameObject holdPawlGo = new GameObject("Holding_Pawl");
            holdPawlGo.transform.SetParent(holdBracket.transform, false);
            holdingPawlTransform = holdPawlGo.transform;
            MeshFilter mfHPawl = holdPawlGo.AddComponent<MeshFilter>();
            MeshRenderer mrHPawl = holdPawlGo.AddComponent<MeshRenderer>();
            mfHPawl.mesh = GearMeshBuilder.BuildPawlMesh(pawlLength, pawlWidth, 0.004f, 0.006f);
            mrHPawl.material = CreateMaterial(new Color(0.95f, 0.75f, 0.25f), 0.8f, 0.75f); // Brass/Bronze
            holdPawlGo.AddComponent<BoxCollider>().size = new Vector3(pawlLength, pawlWidth, 0.006f);
            InspectableComponent inspHPawl = holdPawlGo.AddComponent<InspectableComponent>();
            inspHPawl.SetMetadata("Stationary Holding Detent Pawl",
                "Prevents reverse rotation during the rocker arm's return stroke by locking against tooth shoulder.",
                "Anti-Backlash Safety Stop | Positive Mechanical Lock");
            inspHPawl.SetExplosionVector(Vector3.right, 0.07f, 0.11f);

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
            float prevCycle = currentCycleAngleDeg;
            currentCycleAngleDeg += inputAngularVelDegPerSec * deltaTime;

            // Handle full 360° cycle wrap
            while (currentCycleAngleDeg >= 360f)
            {
                currentCycleAngleDeg -= 360f;
                totalStepsIndexed++;
            }
            while (currentCycleAngleDeg < 0f)
            {
                currentCycleAngleDeg += 360f;
            }

            // Kinematic indexing logic:
            // 0° to 180° (Forward stroke): Driving arm sweeps from 0° to 15°, pushing the wheel forward by 15°
            // 180° to 360° (Return stroke): Wheel stays locked at current full step; arm returns from 15° to 0°
            float strokeProgress = 0f;
            if (currentCycleAngleDeg < 180f)
            {
                // Smooth sine-based advance: 0.5 * (1 - cos(theta)) from 0 to 1
                strokeProgress = 0.5f * (1f - Mathf.Cos(currentCycleAngleDeg * Mathf.Deg2Rad));
                currentWheelAngleDeg = (totalStepsIndexed * PitchAngleDeg) + (strokeProgress * PitchAngleDeg);
            }
            else
            {
                // Return stroke: Wheel remains locked at next tooth step
                strokeProgress = 1f;
                currentWheelAngleDeg = (totalStepsIndexed + 1) * PitchAngleDeg;
            }

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            // 1. Ratchet Wheel local rotation around local Z
            if (ratchetWheelRotor != null)
            {
                ratchetWheelRotor.localRotation = Quaternion.Euler(0f, 0f, -currentWheelAngleDeg);
            }

            // 2. Rocker Arm sweep: sweeps from 0° to 15° during cycle
            float armAngle = 0f;
            if (currentCycleAngleDeg < 180f)
            {
                float prog = 0.5f * (1f - Mathf.Cos(currentCycleAngleDeg * Mathf.Deg2Rad));
                armAngle = prog * PitchAngleDeg;
            }
            else
            {
                // Arm returns from 15° back to 0°
                float prog = 0.5f * (1f + Mathf.Cos((currentCycleAngleDeg - 180f) * Mathf.Deg2Rad));
                armAngle = prog * PitchAngleDeg;
            }

            if (drivingArmRotor != null)
            {
                drivingArmRotor.localRotation = Quaternion.Euler(0f, 0f, -armAngle);
            }

            // 3. Driving Pawl click angle on rocker arm
            if (drivingPawlTransform != null)
            {
                // On return stroke (180..360), pawl rides up the ramp (lifts by 12°)
                float liftDeg = (currentCycleAngleDeg >= 180f) ? Mathf.Sin((currentCycleAngleDeg - 180f) * Mathf.Deg2Rad) * 14f : 0f;
                drivingPawlTransform.localRotation = Quaternion.Euler(0f, 0f, -65f + liftDeg);
            }

            // 4. Holding Pawl click angle
            if (holdingPawlTransform != null)
            {
                // On forward stroke (0..180), holding pawl rides up the rotating tooth ramp (lifts by 12°)
                float liftDeg = (currentCycleAngleDeg < 180f) ? Mathf.Sin(currentCycleAngleDeg * Mathf.Deg2Rad) * 14f : 0f;
                holdingPawlTransform.localRotation = Quaternion.Euler(0f, 0f, 135f + liftDeg);
            }
        }

        public override void ResetMechanism()
        {
            currentCycleAngleDeg = 0f;
            currentWheelAngleDeg = 0f;
            totalStepsIndexed = 0;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Tooth Count (Z): {toothCount} teeth (Asymmetric Sawtooth)\n" +
                   $"• Pitch Angle (Δθ): {PitchAngleDeg:F1}° per tooth\n" +
                   $"• Cycle Phase Angle: {currentCycleAngleDeg:F1}°\n" +
                   $"• Cumulative Output Rotation: {currentWheelAngleDeg:F1}°\n" +
                   $"• Total Increments Indexed: {totalStepsIndexed} steps\n" +
                   $"• Current Operating State: {(IsDrivingStroke ? "Forward Drive Advance" : "Stationary Locked Dwell")}\n" +
                   $"• Velocity Ratio (i): {GearRatio:F0}:1 (Unidirectional Reduction)\n" +
                   $"• Input Stroke Frequency: {currentInputRPM:F0} cpm | Output Speed: {OutputRPM:F1} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Ratchet and Pawl mechanism enforces strict unidirectional step-wise rotation, converting bidirectional oscillating input into non-reversible forward indexing. " +
                   "During the driving stroke (0°–180°), the rocker arm advances the driving pawl against the radial face of a sawtooth tooth, advancing the wheel by exactly 15°. " +
                   "Simultaneously, the spring-loaded holding pawl rides up the inclined ramp and snaps down into the root pocket. " +
                   "During the return stroke (180°–360°), the holding pawl jams against the tooth shoulder to prevent backward movement while the driving pawl slips back into position for the next cycle. " +
                   "Widely employed in winches, hand tools, clocks, and mechanical counters.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Index Pitch Angle: Δθ = 360° / Z\n" +
                   $"Δθ = 360° / {toothCount} = {PitchAngleDeg:F1}°\n" +
                   "Average Speed Ratio: i = Z : 1\n" +
                   $"N_out = N_in / {toothCount} = {currentInputRPM:F0} / {toothCount} = {OutputRPM:F1} RPM\n" +
                   $"Current Phase: {currentCycleAngleDeg:F0}° ({(IsDrivingStroke ? "Drive Push" : "Pawl Return Slip")})\n" +
                   $"Cumulative Travel: θ_total = {currentWheelAngleDeg:F1}° ({totalStepsIndexed} teeth indexed)";
        }
    }
}
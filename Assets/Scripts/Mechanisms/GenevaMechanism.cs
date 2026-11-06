using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    public enum GenevaState
    {
        Dwell = 0,    // Output locked stationary by crescent ring (75% of cycle)
        Indexing = 1  // Pin engages radial slot, advancing wheel by 90° (25% of cycle)
    }

    /// <summary>
    /// Mechanism 21: Four-Slot Geneva Drive (Maltese Cross).
    /// Generates true intermittent indexing motion from continuous input rotation:
    /// - 4 Radial Slots (90° indexing per cycle, 4 stations)
    /// - Dwell Fraction = (360° - 90°) / 360° = 75% stationary dwell
    /// - Indexing Fraction = 90° / 360° = 25% active advance
    /// - Raised locking crescent physically locks Geneva wheel during dwell without backlash.
    /// </summary>
    public class GenevaMechanism : MechanismBase
    {
        [Header("Geneva Geometry")]
        [SerializeField] private int slotCount = 4;                 // n = 4 slots (90° indexing)
        [SerializeField] private float crankPinRadiusMeters = 0.040f;// R1 = 40 mm
        [SerializeField] private float slotWidthMeters = 0.008f;
        [SerializeField] private float wheelThickness = 0.016f;

        [Header("Transforms")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform genevaWheelRotor;
        [SerializeField] private Transform drivePinTransform;

        private float currentInputAngleDeg = 0f;
        private float currentWheelAngleDeg = 0f;
        private int currentStationIndex = 0;
        private bool isAssemblyBuilt = false;

        public int SlotCount => slotCount;
        public float CrankRadius => crankPinRadiusMeters;
        public float CenterDistance => crankPinRadiusMeters / Mathf.Sin(Mathf.PI / slotCount); // C = R1 / sin(45°) = 40 * sqrt(2) = 56.57 mm
        public float WheelRadius => crankPinRadiusMeters * (1f / Mathf.Tan(Mathf.PI / slotCount)); // R2 = 40 mm

        public float InputAngleDeg => currentInputAngleDeg;
        public float WheelAngleDeg => currentWheelAngleDeg;
        public int CurrentStation => (currentStationIndex % slotCount) + 1;

        public bool IsIndexing
        {
            get
            {
                // Drive pin enters slot at theta = 315° (-45°) and exits at theta = 45°
                float ang = currentInputAngleDeg;
                return (ang >= 315f || ang <= 45f);
            }
        }

        public GenevaState CurrentState => IsIndexing ? GenevaState.Indexing : GenevaState.Dwell;

        public override string MechanismName => "Geneva Mechanism";
        public override string CategoryTitle => "Intermittent Indexing Drive";
        public override float GearRatio => slotCount; // 4 input revolutions per 1 full wheel revolution (4:1)
        public override float OutputRPM => currentInputRPM / slotCount; // Average output speed

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = crankPinRadiusMeters;
            float r2 = WheelRadius;
            float cDist = CenterDistance;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(cDist * 0.5f, -0.045f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(cDist + 0.14f, 0.16f, 0.022f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(cDist + 0.14f, 0.022f, 0.16f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Ground Mounting Bedplate",
                $"Provides rigid center distance C = {cDist * 1000f:F1} mm between continuous driver axis and intermittent Geneva wheel axis.",
                "Material: Cast Carbon Steel");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Driver Pillow Block (at 0,0,0)
            GameObject pbDriver = new GameObject("Driver_PillowBlock");
            pbDriver.transform.SetParent(transform, false);
            pbDriver.transform.localPosition = new Vector3(0, -0.022f, -0.015f);
            pbDriver.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.030f, 0.046f, 0.020f, 0.007f);
            pbDriver.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            pbDriver.AddComponent<BoxCollider>().size = new Vector3(0.030f, 0.046f, 0.020f);

            // 3. Geneva Wheel Pillow Block (at cDist, 0, 0)
            GameObject pbWheel = new GameObject("Wheel_PillowBlock");
            pbWheel.transform.SetParent(transform, false);
            pbWheel.transform.localPosition = new Vector3(cDist, -0.022f, -0.015f);
            pbWheel.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.030f, 0.046f, 0.020f, 0.007f);
            pbWheel.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            pbWheel.AddComponent<BoxCollider>().size = new Vector3(0.030f, 0.046f, 0.020f);

            // 4. Continuous Input Driver Rotor (at 0,0,0)
            GameObject driverGo = new GameObject("Driver_Rotor");
            driverGo.transform.SetParent(transform, false);
            driverGo.transform.localPosition = Vector3.zero;
            driverRotor = driverGo.transform;

            GameObject driverDisc = new GameObject("Driver_Locking_Disc");
            driverDisc.transform.SetParent(driverRotor, false);
            MeshFilter mfDisc = driverDisc.AddComponent<MeshFilter>();
            MeshRenderer mrDisc = driverDisc.AddComponent<MeshRenderer>();
            mfDisc.mesh = GearMeshBuilder.BuildGenevaDriverMesh(r1 + 0.008f, 0.004f, 0.014f);
            mrDisc.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            driverDisc.AddComponent<MeshCollider>().sharedMesh = mfDisc.mesh;
            InspectableComponent inspDriver = driverDisc.AddComponent<InspectableComponent>();
            inspDriver.SetMetadata("Continuous Drive Wheel & Locking Ring",
                "Spins continuously at motor speed. The circular locking crescent seats into Geneva wheel dwell cutouts, preventing wheel rotation during dwell.",
                $"Crank Radius R₁: {r1 * 1000f:F1} mm | Input: {currentInputRPM:F0} RPM");
            inspDriver.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            // Drive Pin (mounted at X = r1, Y = 0 on driver)
            GameObject pinGo = new GameObject("Drive_Pin");
            pinGo.transform.SetParent(driverRotor, false);
            pinGo.transform.localPosition = new Vector3(r1, 0f, 0.010f);
            drivePinTransform = pinGo.transform;
            MeshFilter mfPin = pinGo.AddComponent<MeshFilter>();
            MeshRenderer mrPin = pinGo.AddComponent<MeshRenderer>();
            mfPin.mesh = GearMeshBuilder.BuildCylinderMesh(0.004f, 0.020f, 20, false);
            mrPin.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            pinGo.AddComponent<CapsuleCollider>().height = 0.020f;
            InspectableComponent inspPin = pinGo.AddComponent<InspectableComponent>();
            inspPin.SetMetadata("Hardened Drive Pin",
                "Enters the radial slot smoothly without shock, drives the Geneva wheel through 90°, and exits tangential to the slot centerline.",
                "Material: Case-Hardened Tool Steel 60 HRC | Low Wear");
            inspPin.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

            // 5. Intermittent Geneva Wheel (Maltese Cross, at cDist, 0, 0)
            GameObject wheelGo = new GameObject("Geneva_Wheel_Rotor");
            wheelGo.transform.SetParent(transform, false);
            wheelGo.transform.localPosition = new Vector3(cDist, 0, 0);
            genevaWheelRotor = wheelGo.transform;

            GameObject wheelCross = new GameObject("Maltese_Cross_Wheel");
            wheelCross.transform.SetParent(genevaWheelRotor, false);
            MeshFilter mfWheel = wheelCross.AddComponent<MeshFilter>();
            MeshRenderer mrWheel = wheelCross.AddComponent<MeshRenderer>();
            mfWheel.mesh = GearMeshBuilder.BuildGenevaWheelMesh(slotCount, r2 + 0.012f, slotWidthMeters, r2 * 0.70f, r1 * 0.90f, wheelThickness, 0.007f);
            mrWheel.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            wheelCross.AddComponent<MeshCollider>().sharedMesh = mfWheel.mesh;
            InspectableComponent inspWheel = wheelCross.AddComponent<InspectableComponent>();
            inspWheel.SetMetadata("4-Slot Geneva Wheel (Maltese Cross)",
                "Undergoes genuine intermittent motion: advances exactly 90° during pin engagement (25% of cycle) and remains locked stationary during dwell (75% of cycle).",
                $"Slots: {slotCount} | Indexing Angle: {360f / slotCount:F0}° | Dwell: 75%");
            inspWheel.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

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
            float prevAngle = currentInputAngleDeg;
            currentInputAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentInputAngleDeg %= 360f;
            if (currentInputAngleDeg < 0f) currentInputAngleDeg += 360f;

            // Track station increment when passing 0°
            if (prevAngle > 300f && currentInputAngleDeg < 60f)
            {
                currentStationIndex++;
            }

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            // 1. Continuous Driver rotates around Z
            if (driverRotor != null)
            {
                driverRotor.localRotation = Quaternion.Euler(0f, 0f, currentInputAngleDeg);
            }

            // 2. Intermittent Geneva Wheel Kinematics
            // Pin engagement sector: theta1 in [-45°, +45°]
            float theta1 = currentInputAngleDeg;
            float normAngle = (theta1 > 180f) ? (theta1 - 360f) : theta1; // -180 to +180

            float wheelAngleOffset = 0f;
            if (Mathf.Abs(normAngle) <= 45f)
            {
                // Active Indexing Phase:
                // tan(theta2) = (R1 * sin(theta1)) / (C - R1 * cos(theta1))
                float rad1 = normAngle * Mathf.Deg2Rad;
                float r1 = crankPinRadiusMeters;
                float cDist = CenterDistance;
                float num = r1 * Mathf.Sin(rad1);
                float den = cDist - r1 * Mathf.Cos(rad1);
                float theta2Rad = Mathf.Atan2(num, den);
                wheelAngleOffset = -theta2Rad * Mathf.Rad2Deg; // Counter-rotation
            }
            else
            {
                // Dwell Phase: Wheel remains locked at 45° station alignment
                wheelAngleOffset = (normAngle > 45f) ? -45f : 45f;
            }

            float baseStationAngle = (currentStationIndex * 90f);
            currentWheelAngleDeg = baseStationAngle + wheelAngleOffset;

            if (genevaWheelRotor != null)
            {
                genevaWheelRotor.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngleDeg);
            }
        }

        public override void ResetMechanism()
        {
            currentInputAngleDeg = 180f; // Start in middle of Dwell phase
            currentStationIndex = 0;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Current State: {(IsIndexing ? "INDEXING (ADVANCING ⚙️)" : "DWELL (LOCKED 🔒)")}\n" +
                   $"• Current Station: Station #{CurrentStation} / {slotCount}\n" +
                   $"• Input Angle (θ₁): {currentInputAngleDeg:F1}°\n" +
                   $"• Output Wheel Angle: {currentWheelAngleDeg:F1}°\n" +
                   $"• Dwell Period: 75% of cycle (270°)\n" +
                   $"• Indexing Period: 25% of cycle (90°)\n" +
                   $"• Center Distance (C): {CenterDistance * 1000f:F1} mm";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Geneva mechanism converts continuous rotational motion into discrete, intermittent rotary indexing. " +
                   "A rotating drive pin engages radial slots in the Geneva wheel to advance it by exactly 360°/n = 90° per turn, " +
                   "while a concentric locking crescent engages circular dwell scallops to lock the wheel rigidly in place between advances.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Center Distance: C = R₁ / sin(π/n) = 40.0 / sin(45°) = 56.6 mm\n" +
                   "Wheel Radius: R₂ = R₁ / tan(π/n) = 40.0 / tan(45°) = 40.0 mm\n" +
                   "Indexing Angle: Δθ = 360° / 4 = 90.0°\n" +
                   "Indexing Kinematics: tanθ₂ = (R₁·sinθ₁) / (C - R₁·cosθ₁)\n" +
                   "Cycle Allocation: Dwell = 75% (270°) | Motion = 25% (90°)";
        }
    }
}

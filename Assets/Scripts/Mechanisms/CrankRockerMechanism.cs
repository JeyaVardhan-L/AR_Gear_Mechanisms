using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 20: Crank-Rocker Mechanism.
    /// Demonstrates continuous crank rotation transformed into oscillating rocker sweeps with quick-return characteristics:
    /// - Crank length a = 32 mm
    /// - Coupler length b = 105 mm
    /// - Rocker length c = 75 mm
    /// - Ground distance d = 110 mm
    /// Outputs live rocker angular displacement, angular velocity, and quick-return timing ratio.
    /// </summary>
    public class CrankRockerMechanism : MechanismBase
    {
        [Header("Kinematic Link Dimensions")]
        [SerializeField] private float groundLengthD = 0.110f;   // d = 110 mm
        [SerializeField] private float crankLengthA = 0.032f;    // a = 32 mm (Shortest link s)
        [SerializeField] private float couplerLengthB = 0.105f;  // b = 105 mm
        [SerializeField] private float rockerLengthC = 0.075f;   // c = 75 mm

        [Header("Transforms")]
        [SerializeField] private Transform crankTransform;
        [SerializeField] private Transform couplerTransform;
        [SerializeField] private Transform rockerTransform;

        private float currentCrankAngleDeg = 0f;
        private float previousRockerAngleDeg = 0f;
        private float currentRockerAngularVelocityDegPerSec = 0f;
        private bool isAssemblyBuilt = false;

        public float CrankAngleDeg => currentCrankAngleDeg;

        public float RockerAngleDeg
        {
            get
            {
                CalculatePivots(out _, out _, out float theta4);
                return theta4 * Mathf.Rad2Deg;
            }
        }

        public float RockerAngularVelocityDegPerSec => currentRockerAngularVelocityDegPerSec;
        public float RockerAngularVelocityRadPerSec => currentRockerAngularVelocityDegPerSec * Mathf.Deg2Rad;

        /// <summary>
        /// Quick-Return Ratio Q = (180 + alpha) / (180 - alpha)
        /// </summary>
        public float QuickReturnRatio
        {
            get
            {
                // Extreme angles occur when crank and coupler are collinear:
                // r1 = b - a, r2 = b + a
                float cosThetaMin = Mathf.Clamp((groundLengthD * groundLengthD + rockerLengthC * rockerLengthC - (couplerLengthB + crankLengthA) * (couplerLengthB + crankLengthA)) / (2f * groundLengthD * rockerLengthC), -1f, 1f);
                float cosThetaMax = Mathf.Clamp((groundLengthD * groundLengthD + rockerLengthC * rockerLengthC - (couplerLengthB - crankLengthA) * (couplerLengthB - crankLengthA)) / (2f * groundLengthD * rockerLengthC), -1f, 1f);
                float deltaAngle = Mathf.Abs(Mathf.Acos(cosThetaMin) - Mathf.Acos(cosThetaMax)) * Mathf.Rad2Deg;
                return (180f + deltaAngle * 0.25f) / Mathf.Max(1f, (180f - deltaAngle * 0.25f));
            }
        }

        public override string MechanismName => "Crank-Rocker Mechanism";
        public override string CategoryTitle => "Oscillating Quick-Return Linkage";
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

            float d = groundLengthD;
            float a = crankLengthA;
            float b = couplerLengthB;
            float c = rockerLengthC;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(d * 0.5f, -0.045f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(d + 0.12f, 0.14f, 0.022f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(d + 0.12f, 0.022f, 0.14f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Crank-Rocker Base Frame",
                $"Provides rigid baseline distance d = {d * 1000f:F0} mm between fixed input and output axes.",
                "Material: Cast Carbon Steel");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // Ground Stanchions
            GameObject stanchionA = new GameObject("Ground_Pivot_A");
            stanchionA.transform.SetParent(transform, false);
            stanchionA.transform.localPosition = new Vector3(0, -0.022f, -0.015f);
            stanchionA.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.028f, 0.046f, 0.020f, 0.006f);
            stanchionA.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            stanchionA.AddComponent<BoxCollider>().size = new Vector3(0.028f, 0.046f, 0.020f);

            GameObject stanchionD = new GameObject("Ground_Pivot_D");
            stanchionD.transform.SetParent(transform, false);
            stanchionD.transform.localPosition = new Vector3(d, -0.022f, -0.015f);
            stanchionD.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.028f, 0.046f, 0.020f, 0.006f);
            stanchionD.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            stanchionD.AddComponent<BoxCollider>().size = new Vector3(0.028f, 0.046f, 0.020f);

            // 2. Input Crank (Link 2)
            GameObject crankGo = new GameObject("Input_Crank");
            crankGo.transform.SetParent(transform, false);
            crankGo.transform.localPosition = Vector3.zero;
            crankTransform = crankGo.transform;
            MeshFilter mfCrank = crankGo.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankGo.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildLinkageBarMesh(a, 0.018f, 0.012f, 0.005f);
            mrCrank.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crankGo.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankGo.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Input Crank Arm",
                "Full-revolving 360° driver converting rotary torque into linkage oscillation.",
                $"Crank Radius a: {a * 1000f:F1} mm | Input: {currentInputRPM:F0} RPM");
            inspCrank.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            // 3. Output Rocker (Link 4)
            GameObject rockerGo = new GameObject("Output_Rocker");
            rockerGo.transform.SetParent(transform, false);
            rockerGo.transform.localPosition = new Vector3(d, 0, 0);
            rockerTransform = rockerGo.transform;
            MeshFilter mfRocker = rockerGo.AddComponent<MeshFilter>();
            MeshRenderer mrRocker = rockerGo.AddComponent<MeshRenderer>();
            mfRocker.mesh = GearMeshBuilder.BuildLinkageBarMesh(c, 0.018f, 0.012f, 0.005f);
            mrRocker.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            rockerGo.AddComponent<MeshCollider>().sharedMesh = mfRocker.mesh;
            InspectableComponent inspRocker = rockerGo.AddComponent<InspectableComponent>();
            inspRocker.SetMetadata("Oscillating Rocker Arm",
                "Executes harmonic sweep through fixed angular stroke with fast return phase.",
                $"Rocker Length c: {c * 1000f:F1} mm | Angle: {RockerAngleDeg:F1}°");
            inspRocker.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 4. Coupler Link (Link 3)
            GameObject couplerGo = new GameObject("Coupler_Rod");
            couplerGo.transform.SetParent(transform, false);
            couplerTransform = couplerGo.transform;
            MeshFilter mfCoupler = couplerGo.AddComponent<MeshFilter>();
            MeshRenderer mrCoupler = couplerGo.AddComponent<MeshRenderer>();
            mfCoupler.mesh = GearMeshBuilder.BuildLinkageBarMesh(b, 0.018f, 0.012f, 0.005f);
            mrCoupler.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            couplerGo.AddComponent<MeshCollider>().sharedMesh = mfCoupler.mesh;
            InspectableComponent inspCoupler = couplerGo.AddComponent<InspectableComponent>();
            inspCoupler.SetMetadata("Connecting Coupler Bar",
                "Couples crank pin B to rocker pin C, transmitting dynamic forces through varying transmission angles.",
                $"Coupler Length b: {b * 1000f:F1} mm");
            inspCoupler.SetExplosionVector(Vector3.up, 0.07f, 0.11f);

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
            float prevAngle = RockerAngleDeg;

            currentCrankAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCrankAngleDeg %= 360f;
            if (currentCrankAngleDeg < 0f) currentCrankAngleDeg += 360f;

            UpdateComponentTransforms();

            if (deltaTime > 0.0001f)
            {
                float newAngle = RockerAngleDeg;
                currentRockerAngularVelocityDegPerSec = Mathf.DeltaAngle(prevAngle, newAngle) / deltaTime;
            }
        }

        private void CalculatePivots(out Vector3 ptB, out Vector3 ptC, out float theta4)
        {
            float d = groundLengthD;
            float a = crankLengthA;
            float b = couplerLengthB;
            float c = rockerLengthC;

            float theta2Rad = currentCrankAngleDeg * Mathf.Deg2Rad;

            float bx = a * Mathf.Cos(theta2Rad);
            float by = a * Mathf.Sin(theta2Rad);
            ptB = new Vector3(bx, by, 0.012f);

            float dx = d - bx;
            float dy = -by;
            float delta = Mathf.Sqrt(dx * dx + dy * dy);
            float psi = Mathf.Atan2(dy, dx);

            float cosAlpha = (c * c + delta * delta - b * b) / (2f * c * delta);
            float alpha = Mathf.Acos(Mathf.Clamp(cosAlpha, -1f, 1f));

            theta4 = psi + alpha;
            float cx = d + c * Mathf.Cos(theta4);
            float cy = c * Mathf.Sin(theta4);
            ptC = new Vector3(cx, cy, 0.012f);
        }

        private void UpdateComponentTransforms()
        {
            CalculatePivots(out Vector3 ptB, out Vector3 ptC, out float theta4);

            if (crankTransform != null) crankTransform.localRotation = Quaternion.Euler(0f, 0f, currentCrankAngleDeg);
            if (rockerTransform != null) rockerTransform.localRotation = Quaternion.Euler(0f, 0f, theta4 * Mathf.Rad2Deg);
            if (couplerTransform != null)
            {
                couplerTransform.localPosition = ptB;
                Vector3 toC = ptC - ptB;
                float angleZ = Mathf.Atan2(toC.y, toC.x) * Mathf.Rad2Deg;
                couplerTransform.localRotation = Quaternion.Euler(0f, 0f, angleZ);
            }
        }

        public override void ResetMechanism()
        {
            currentCrankAngleDeg = 0f;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Crank Angle (θ₂): {currentCrankAngleDeg:F1}°\n" +
                   $"• Rocker Angle (θ₄): {RockerAngleDeg:F1}°\n" +
                   $"• Rocker Ang. Speed: {currentRockerAngularVelocityDegPerSec:F1}°/s\n" +
                   $"• Quick-Return Ratio: {QuickReturnRatio:F2}:1\n" +
                   $"• Crank Length (a): {crankLengthA * 1000f:F0} mm\n" +
                   $"• Coupler (b): {couplerLengthB * 1000f:F0} mm\n" +
                   $"• Rocker (c): {rockerLengthC * 1000f:F0} mm";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Crank-Rocker mechanism achieves continuous harmonic angular oscillation from continuous 360° input rotation. " +
                   "Because the forward and return stroke crank angles differ, the return sweep occurs at higher angular velocity, " +
                   "forming the basis of mechanical shapers, windshield wipers, and feeder linkages.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Loop Closure: a·e^(iθ₂) + b·e^(iθ₃) = d + c·e^(iθ₄)\n" +
                   $"Rocker Velocity: ω₄ = {RockerAngularVelocityRadPerSec:F2} rad/s ({currentRockerAngularVelocityDegPerSec:F0}°/s)\n" +
                   $"Quick-Return Ratio: Q = {QuickReturnRatio:F2}:1";
        }
    }
}

using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 19: Four-Bar Planar Linkage.
    /// Demonstrates closed-loop 1-DOF planar kinematic transmission governed by Grashof's Law:
    /// Link 1 (Ground): d = 120 mm
    /// Link 2 (Crank):  a = 35 mm (Shortest link s, continuous 360° rotation)
    /// Link 3 (Coupler): b = 110 mm
    /// Link 4 (Rocker):  c = 80 mm (Oscillating rocker)
    /// Closed-form Freudenstein & Law of Cosines position solutions determine exact coupler and rocker poses.
    /// </summary>
    public class FourBarLinkage : MechanismBase
    {
        [Header("Four-Bar Link Lengths (Grashof Crank-Rocker)")]
        [SerializeField] private float groundLengthD = 0.120f;   // Link 1 (Fixed base between A and D)
        [SerializeField] private float crankLengthA = 0.035f;    // Link 2 (Input crank around A)
        [SerializeField] private float couplerLengthB = 0.110f;  // Link 3 (Connecting rod BC)
        [SerializeField] private float rockerLengthC = 0.080f;   // Link 4 (Output rocker around D)

        [Header("Transforms")]
        [SerializeField] private Transform crankTransform;
        [SerializeField] private Transform couplerTransform;
        [SerializeField] private Transform rockerTransform;

        private float currentCrankAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public float GroundLength => groundLengthD;
        public float CrankLength => crankLengthA;
        public float CouplerLength => couplerLengthB;
        public float RockerLength => rockerLengthC;
        public float CrankAngleDeg => currentCrankAngleDeg;

        public bool IsGrashof
        {
            get
            {
                float[] lens = new float[] { groundLengthD, crankLengthA, couplerLengthB, rockerLengthC };
                Array.Sort(lens);
                return (lens[0] + lens[3]) <= (lens[1] + lens[2]);
            }
        }

        /// <summary>
        /// Output Rocker angular position relative to ground axis in degrees.
        /// </summary>
        public float RockerAngleDeg
        {
            get
            {
                CalculatePivots(out _, out _, out float theta4);
                return theta4 * Mathf.Rad2Deg;
            }
        }

        public override string MechanismName => "Four-Bar Linkage";
        public override string CategoryTitle => "Planar 1-DOF Kinematic Chain";
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

            // 1. Machine Bedplate with Fixed Ground Pivots A(0,0) and D(d,0)
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(d * 0.5f, -0.045f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(d + 0.12f, 0.14f, 0.022f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(d + 0.12f, 0.022f, 0.14f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Linkage Bedplate (Ground Link 1)",
                $"Fixed reference frame establishing rigid ground distance d = {d * 1000f:F0} mm between input crank pivot A and rocker pivot D.",
                "Material: Ground Cast Iron | Planed Reference Datum");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // Ground Pivot Stanchion A (at 0,0,0)
            GameObject stanchionA = new GameObject("Ground_Pivot_A");
            stanchionA.transform.SetParent(transform, false);
            stanchionA.transform.localPosition = new Vector3(0, -0.022f, -0.015f);
            stanchionA.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.028f, 0.046f, 0.020f, 0.006f);
            stanchionA.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            stanchionA.AddComponent<BoxCollider>().size = new Vector3(0.028f, 0.046f, 0.020f);

            // Ground Pivot Stanchion D (at d,0,0)
            GameObject stanchionD = new GameObject("Ground_Pivot_D");
            stanchionD.transform.SetParent(transform, false);
            stanchionD.transform.localPosition = new Vector3(d, -0.022f, -0.015f);
            stanchionD.AddComponent<MeshFilter>().mesh = GearMeshBuilder.BuildPillowBlockMesh(0.028f, 0.046f, 0.020f, 0.006f);
            stanchionD.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.35f);
            stanchionD.AddComponent<BoxCollider>().size = new Vector3(0.028f, 0.046f, 0.020f);

            // 2. Link 2: Input Crank (Pivots at A(0,0,0), Length a)
            GameObject crankGo = new GameObject("Input_Crank_Link2");
            crankGo.transform.SetParent(transform, false);
            crankGo.transform.localPosition = Vector3.zero;
            crankTransform = crankGo.transform;
            MeshFilter mfCrank = crankGo.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankGo.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildLinkageBarMesh(a, 0.018f, 0.012f, 0.005f);
            mrCrank.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crankGo.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankGo.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Input Crank (Link 2)",
                "Driven by motor at continuous angular velocity. Shortest link in Grashof chain, capable of full 360° rotation.",
                $"Length a: {a * 1000f:F1} mm | Drive Speed: {currentInputRPM:F0} RPM | Hardened 4140 Steel");
            inspCrank.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            // 3. Link 4: Output Rocker (Pivots at D(d,0,0), Length c)
            GameObject rockerGo = new GameObject("Output_Rocker_Link4");
            rockerGo.transform.SetParent(transform, false);
            rockerGo.transform.localPosition = new Vector3(d, 0, 0);
            rockerTransform = rockerGo.transform;
            MeshFilter mfRocker = rockerGo.AddComponent<MeshFilter>();
            MeshRenderer mrRocker = rockerGo.AddComponent<MeshRenderer>();
            mfRocker.mesh = GearMeshBuilder.BuildLinkageBarMesh(c, 0.018f, 0.012f, 0.005f);
            mrRocker.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            rockerGo.AddComponent<MeshCollider>().sharedMesh = mfRocker.mesh;
            InspectableComponent inspRocker = rockerGo.AddComponent<InspectableComponent>();
            inspRocker.SetMetadata("Output Rocker (Link 4)",
                "Constrained by ground pivot D to oscillate smoothly through a limited angular range, converting full rotation into harmonic sweep.",
                $"Length c: {c * 1000f:F1} mm | Rocker Angle: {RockerAngleDeg:F1}° | ASTM A36 Steel");
            inspRocker.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 4. Link 3: Coupler (Floating bar connecting crank pin B to rocker pin C, Length b)
            GameObject couplerGo = new GameObject("Floating_Coupler_Link3");
            couplerGo.transform.SetParent(transform, false);
            couplerTransform = couplerGo.transform;
            MeshFilter mfCoupler = couplerGo.AddComponent<MeshFilter>();
            MeshRenderer mrCoupler = couplerGo.AddComponent<MeshRenderer>();
            mfCoupler.mesh = GearMeshBuilder.BuildLinkageBarMesh(b, 0.018f, 0.012f, 0.005f);
            mrCoupler.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            couplerGo.AddComponent<MeshCollider>().sharedMesh = mfCoupler.mesh;
            InspectableComponent inspCoupler = couplerGo.AddComponent<InspectableComponent>();
            inspCoupler.SetMetadata("Floating Coupler (Link 3)",
                "Undergoes complex general planar motion (simultaneous translation and rotation), transferring force from crank to rocker.",
                $"Length b: {b * 1000f:F1} mm | Forged High-Strength Alloy");
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
            currentCrankAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCrankAngleDeg %= 360f;
            if (currentCrankAngleDeg < 0f) currentCrankAngleDeg += 360f;

            UpdateComponentTransforms();
        }

        private void CalculatePivots(out Vector3 ptB, out Vector3 ptC, out float theta4)
        {
            float d = groundLengthD;
            float a = crankLengthA;
            float b = couplerLengthB;
            float c = rockerLengthC;

            float theta2Rad = currentCrankAngleDeg * Mathf.Deg2Rad;

            // Crank pin B
            float bx = a * Mathf.Cos(theta2Rad);
            float by = a * Mathf.Sin(theta2Rad);
            ptB = new Vector3(bx, by, 0.012f);

            // Distance from B to ground pivot D(d, 0)
            float dx = d - bx;
            float dy = -by;
            float delta = Mathf.Sqrt(dx * dx + dy * dy);
            float psi = Mathf.Atan2(dy, dx);

            // Law of Cosines for angle BDC (alpha)
            float cosAlpha = (c * c + delta * delta - b * b) / (2f * c * delta);
            float alpha = Mathf.Acos(Mathf.Clamp(cosAlpha, -1f, 1f));

            // Rocker orientation theta4
            theta4 = psi + alpha;
            float cx = d + c * Mathf.Cos(theta4);
            float cy = c * Mathf.Sin(theta4);
            ptC = new Vector3(cx, cy, 0.012f);
        }

        private void UpdateComponentTransforms()
        {
            CalculatePivots(out Vector3 ptB, out Vector3 ptC, out float theta4);

            // Crank rotates at A(0,0,0)
            if (crankTransform != null)
            {
                crankTransform.localRotation = Quaternion.Euler(0f, 0f, currentCrankAngleDeg);
            }

            // Rocker rotates at D(d,0,0)
            if (rockerTransform != null)
            {
                rockerTransform.localRotation = Quaternion.Euler(0f, 0f, theta4 * Mathf.Rad2Deg);
            }

            // Coupler connects ptB to ptC
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
            return $"• Grashof Condition: {(IsGrashof ? "GRASHOF SATISFIED (s+l ≤ p+q) ✅" : "NON-GRASHOF")}\n" +
                   $"• Crank Angle (θ₂): {currentCrankAngleDeg:F1}°\n" +
                   $"• Rocker Angle (θ₄): {RockerAngleDeg:F1}°\n" +
                   $"• Ground Link (d): {groundLengthD * 1000f:F0} mm\n" +
                   $"• Input Crank (a): {crankLengthA * 1000f:F0} mm\n" +
                   $"• Coupler (b): {couplerLengthB * 1000f:F0} mm\n" +
                   $"• Output Rocker (c): {rockerLengthC * 1000f:F0} mm";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Four-Bar Linkage is the fundamental single degree-of-freedom mechanism in mechanical design. " +
                   "Because the sum of the shortest link (crank a = 35mm) and longest link (ground d = 120mm) is less than the intermediate links (35+120=155 ≤ 80+110=190), " +
                   "Grashof's theorem guarantees continuous 360° input crank rotation while driving link 4 into smooth harmonic oscillation.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Grashof Criterion: s + l ≤ p + q (35 + 120 = 155 ≤ 110 + 80 = 190 mm)\n" +
                   "Freudenstein Loop: a·cosθ₂ + b·cosθ₃ - c·cosθ₄ = d\n" +
                   "Rocker Angle: θ₄ = ψ + arccos[(c² + δ² - b²)/(2·c·δ)]\n" +
                   $"Current Rocker Angle: θ₄ = {RockerAngleDeg:F1}°";
        }
    }
}

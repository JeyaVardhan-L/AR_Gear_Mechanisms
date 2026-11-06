using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 16: Slider-Crank Mechanism.
    /// Demonstrates authentic rotary-to-reciprocating conversion governed by exact kinematic equations:
    /// x(theta) = r*cos(theta) + sqrt(l^2 - r^2*sin^2(theta))
    /// v(theta) = -r*omega*(sin(theta) + (r*sin(2*theta))/(2*sqrt(l^2 - r^2*sin^2(theta))))
    /// Components:
    /// - Crank Disk & Pin (radius r = 40 mm)
    /// - Forged I-beam Connecting Rod (length l = 120 mm, lambda = 0.333)
    /// - Crosshead Slider with bronze guide pads
    /// - Precision Ground Bedplate with dual linear guide rails
    /// </summary>
    public class SliderCrankMechanism : MechanismBase
    {
        [Header("Slider-Crank Geometry")]
        [SerializeField] private float crankRadiusMeters = 0.040f;   // r = 40 mm
        [SerializeField] private float rodLengthMeters = 0.120f;     // l = 120 mm
        [SerializeField] private float sliderWidth = 0.045f;
        [SerializeField] private float sliderHeight = 0.035f;

        [Header("Rotors & Kinematic Transforms")]
        [SerializeField] private Transform crankTransform;
        [SerializeField] private Transform rodTransform;
        [SerializeField] private Transform sliderTransform;

        private float currentCrankAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public float CrankRadius => crankRadiusMeters;
        public float RodLength => rodLengthMeters;
        public float CrankAngleDeg => currentCrankAngleDeg;

        // Kinematic Calculations
        public float StrokeMeters => 2f * crankRadiusMeters;
        public float RodCrankRatio => crankRadiusMeters / rodLengthMeters;

        /// <summary>
        /// Exact distance from crankshaft axis (0,0,0) to wrist pin center along X axis.
        /// </summary>
        public float PistonDisplacementX
        {
            get
            {
                float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
                float sin = Mathf.Sin(rad);
                float cos = Mathf.Cos(rad);
                float underRad = (rodLengthMeters * rodLengthMeters) - (crankRadiusMeters * crankRadiusMeters * sin * sin);
                return (crankRadiusMeters * cos) + Mathf.Sqrt(Mathf.Max(0.0001f, underRad));
            }
        }

        /// <summary>
        /// Piston displacement from Top Dead Center (TDC, maximum extension).
        /// </summary>
        public float DisplacementFromTDC => (crankRadiusMeters + rodLengthMeters) - PistonDisplacementX;

        /// <summary>
        /// Piston instantaneous linear velocity in m/s.
        /// </summary>
        public float PistonVelocityMps
        {
            get
            {
                float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
                float sin = Mathf.Sin(rad);
                float sin2 = Mathf.Sin(2f * rad);
                float omega = InputAngularVelocityRadPerSec;
                float underRad = Mathf.Sqrt(Mathf.Max(0.0001f, (rodLengthMeters * rodLengthMeters) - (crankRadiusMeters * crankRadiusMeters * sin * sin)));
                return -crankRadiusMeters * omega * (sin + (crankRadiusMeters * sin2) / (2f * underRad));
            }
        }

        /// <summary>
        /// Piston instantaneous linear acceleration in m/s^2.
        /// </summary>
        public float PistonAccelerationMps2
        {
            get
            {
                float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad);
                float cos2 = Mathf.Cos(2f * rad);
                float omega = InputAngularVelocityRadPerSec;
                float lambda = RodCrankRatio;
                return -crankRadiusMeters * (omega * omega) * (cos + lambda * cos2);
            }
        }

        public override string MechanismName => "Slider-Crank Mechanism";
        public override string CategoryTitle => "Reciprocating Kinematic Chain";
        public override float GearRatio => 1.0f; // 1 crank revolution = 1 reciprocating cycle
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
            float l = rodLengthMeters;
            float totalBedLength = r + l + 0.12f;

            // 1. Heavy Machine Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3((r + l) * 0.45f, -0.045f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(totalBedLength, 0.14f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(totalBedLength, 0.020f, 0.14f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Ground Machine Bedplate",
                "Rigid cast-iron bed supporting crankshaft main bearing and precision linear crosshead guides.",
                "Material: ASTM A48 Class 40 Gray Iron | Planed Finish");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Dual Linear Guide Rails (along X axis for slider)
            float railStart = l - r - 0.02f;
            float railEnd = l + r + 0.06f;
            float railLen = railEnd - railStart;
            float[] zRails = new float[] { -0.030f, 0.030f };
            for (int i = 0; i < 2; i++)
            {
                GameObject railGo = new GameObject($"GuideRail_{i}");
                railGo.transform.SetParent(transform, false);
                railGo.transform.localPosition = new Vector3((railStart + railEnd) * 0.5f, 0f, zRails[i]);
                MeshFilter mfR = railGo.AddComponent<MeshFilter>();
                MeshRenderer mrR = railGo.AddComponent<MeshRenderer>();
                mfR.mesh = GearMeshBuilder.BuildCylinderMesh(0.006f, railLen, 16, true);
                mrR.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
                railGo.AddComponent<BoxCollider>().size = new Vector3(railLen, 0.012f, 0.012f);
            }

            // 3. Main Bearing Pillow Block for Crankshaft
            GameObject pbGo = new GameObject("Crank_PillowBlock");
            pbGo.transform.SetParent(transform, false);
            pbGo.transform.localPosition = new Vector3(0, -0.022f, -0.025f);
            MeshFilter mfPb = pbGo.AddComponent<MeshFilter>();
            MeshRenderer mrPb = pbGo.AddComponent<MeshRenderer>();
            mfPb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.032f, 0.046f, 0.022f, 0.007f);
            mrPb.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            pbGo.AddComponent<BoxCollider>().size = new Vector3(0.032f, 0.046f, 0.022f);
            InspectableComponent inspPb = pbGo.AddComponent<InspectableComponent>();
            inspPb.SetMetadata("Main Journal Pillow Block",
                "Houses heavy-duty rolling bearing supporting crankshaft input torque.",
                "Bore: 14mm | Static Load: 6.2 kN");
            inspPb.SetExplosionVector(Vector3.back, 0.07f, 0.10f);

            // 4. Rotating Crankshaft Assembly (Rotates around local Z at origin 0,0,0)
            GameObject crankRoot = new GameObject("Crank_Rotor");
            crankRoot.transform.SetParent(transform, false);
            crankRoot.transform.localPosition = Vector3.zero;
            crankTransform = crankRoot.transform;

            // Crank disk
            GameObject crankDisk = new GameObject("Crank_Disk");
            crankDisk.transform.SetParent(crankTransform, false);
            MeshFilter mfCrank = crankDisk.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankDisk.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildCylinderMesh(r + 0.012f, 0.014f, 32, false);
            mrCrank.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crankDisk.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankDisk.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Crank Disk & Counterweight",
                "Converts rotary motor motion into cyclic eccentric pin translation.",
                $"Radius r: {r * 1000f:F1} mm | Stroke: {r * 2000f:F1} mm | Speed: {currentInputRPM:F0} RPM");
            inspCrank.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            // Crank pin (located at local X = r, Y = 0 on crank)
            GameObject crankPin = new GameObject("Crank_Pin");
            crankPin.transform.SetParent(crankTransform, false);
            crankPin.transform.localPosition = new Vector3(r, 0f, 0.012f);
            MeshFilter mfPin = crankPin.AddComponent<MeshFilter>();
            MeshRenderer mrPin = crankPin.AddComponent<MeshRenderer>();
            mfPin.mesh = GearMeshBuilder.BuildCylinderMesh(0.007f, 0.025f, 24, false);
            mrPin.material = CreateMaterial(new Color(0.88f, 0.88f, 0.90f), 0.9f, 0.85f);
            crankPin.AddComponent<CapsuleCollider>().height = 0.025f;

            // 5. Connecting Rod
            GameObject rodGo = new GameObject("Connecting_Rod");
            rodGo.transform.SetParent(transform, false);
            rodTransform = rodGo.transform;
            MeshFilter mfRod = rodGo.AddComponent<MeshFilter>();
            MeshRenderer mrRod = rodGo.AddComponent<MeshRenderer>();
            mfRod.mesh = GearMeshBuilder.BuildConnectingRodMesh(l, 0.0075f, 0.006f, 0.012f, 0.012f);
            mrRod.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            rodGo.AddComponent<MeshCollider>().sharedMesh = mfRod.mesh;
            InspectableComponent inspRod = rodGo.AddComponent<InspectableComponent>();
            inspRod.SetMetadata("Forged I-Beam Connecting Rod",
                "Transmits force between crank pin and linear crosshead slider. Undergoes combined tension/compression and bending.",
                $"Length l: {l * 1000f:F1} mm | λ = r/l: {r / l:F3} | Forged 42CrMo4");
            inspRod.SetExplosionVector(Vector3.up, 0.07f, 0.11f);

            // 6. Crosshead Slider (Piston)
            GameObject sliderGo = new GameObject("Crosshead_Slider");
            sliderGo.transform.SetParent(transform, false);
            sliderTransform = sliderGo.transform;
            MeshFilter mfSlider = sliderGo.AddComponent<MeshFilter>();
            MeshRenderer mrSlider = sliderGo.AddComponent<MeshRenderer>();
            mfSlider.mesh = GearMeshBuilder.BuildBasePlateMesh(sliderWidth, 0.070f, sliderHeight);
            mrSlider.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            sliderGo.AddComponent<BoxCollider>().size = new Vector3(sliderWidth, sliderHeight, 0.070f);
            InspectableComponent inspSlider = sliderGo.AddComponent<InspectableComponent>();
            inspSlider.SetMetadata("Reciprocating Crosshead Slider",
                "Constrained by linear guide rails to translate horizontally with sinusoidal harmonic motion.",
                $"Displacement: {PistonDisplacementX * 1000f:F1} mm | Max Stroke: {StrokeMeters * 1000f:F1} mm");
            inspSlider.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // Wrist pin inside slider
            GameObject wristPin = new GameObject("Wrist_Pin");
            wristPin.transform.SetParent(sliderTransform, false);
            wristPin.transform.localPosition = Vector3.zero;
            MeshFilter mfWp = wristPin.AddComponent<MeshFilter>();
            MeshRenderer mrWp = wristPin.AddComponent<MeshRenderer>();
            mfWp.mesh = GearMeshBuilder.BuildCylinderMesh(0.0055f, 0.065f, 20, false);
            mrWp.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

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
            float theta = currentCrankAngleDeg * Mathf.Deg2Rad;
            float r = crankRadiusMeters;
            float l = rodLengthMeters;

            // Crank rotates around Z axis
            if (crankTransform != null)
            {
                crankTransform.localRotation = Quaternion.Euler(0f, 0f, currentCrankAngleDeg);
            }

            // Crank pin position in world/assembly coordinates
            Vector3 crankPinPos = new Vector3(Mathf.Cos(theta) * r, Mathf.Sin(theta) * r, 0.012f);

            // Wrist pin (slider) position along X axis
            float xSlider = PistonDisplacementX;
            Vector3 wristPinPos = new Vector3(xSlider, 0f, 0.012f);

            if (sliderTransform != null)
            {
                sliderTransform.localPosition = new Vector3(xSlider, 0f, 0f);
            }

            // Connecting rod connects crankPinPos to wristPinPos
            if (rodTransform != null)
            {
                rodTransform.localPosition = crankPinPos;
                Vector3 toWrist = wristPinPos - crankPinPos;
                float angleZ = Mathf.Atan2(toWrist.y, toWrist.x) * Mathf.Rad2Deg;
                // Connecting rod was modeled along +Y, so rotate -90 to align Y with toWrist
                rodTransform.localRotation = Quaternion.Euler(0f, 0f, angleZ - 90f);
            }
        }

        public override void ResetMechanism()
        {
            currentCrankAngleDeg = 0f; // Start at Top Dead Center
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Crank Radius (r): {crankRadiusMeters * 1000f:F1} mm\n" +
                   $"• Rod Length (l): {rodLengthMeters * 1000f:F1} mm\n" +
                   $"• Stroke (2r): {StrokeMeters * 1000f:F1} mm\n" +
                   $"• Crank Angle (θ): {currentCrankAngleDeg:F1}°\n" +
                   $"• Piston Displacement (s): {DisplacementFromTDC * 1000f:F1} mm\n" +
                   $"• Piston Velocity (v): {PistonVelocityMps:F2} m/s\n" +
                   $"• Acceleration (a): {PistonAccelerationMps2:F1} m/s²\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Slider-Crank mechanism is the foundational kinematic inversion converting continuous rotational energy into reciprocating linear thrust. " +
                   "Because the connecting rod tilts at oblique angle φ, piston displacement deviates from pure sinusoidal harmonic motion by higher-order harmonics (λ = r/l = 0.333), " +
                   "causing the piston to accelerate faster near Top Dead Center than near Bottom Dead Center.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Piston Position: x(θ) = r·cosθ + √(l² - r²·sin²θ)\n" +
                   $"x({currentCrankAngleDeg:F0}°) = {PistonDisplacementX * 1000f:F1} mm\n" +
                   "Displacement from TDC: s(θ) = (r + l) - x(θ)\n" +
                   "Velocity: v(θ) = -r·ω·(sinθ + (r·sin2θ)/(2√(l² - r²·sin²θ)))\n" +
                   "Acceleration: a(θ) ≈ -r·ω²·(cosθ + λ·cos2θ)";
        }
    }
}

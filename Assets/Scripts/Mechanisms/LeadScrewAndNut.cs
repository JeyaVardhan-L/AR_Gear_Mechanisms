using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 26: Trapezoidal Lead Screw and Traveling Nut.
    /// Demonstrates high-thrust rotary-to-linear conversion governed by precision helical mechanics:
    /// - Nominal Diameter d = 24 mm, Pitch P = 6 mm, Single-start Lead L = 6 mm/rev
    /// - Linear velocity: v = (L * N) / 60 m/s
    /// - Axial thrust: F_axial = (2*pi*eta*T_in) / L (Force multiplication factor ~471x)
    /// - Self-locking: Lead angle lambda (4.8°) < Friction angle phi (8.5°), preventing backdriving.
    /// </summary>
    public class LeadScrewAndNut : MechanismBase
    {
        [Header("Lead Screw Specifications")]
        [SerializeField] private float screwRadiusMeters = 0.012f;      // d = 24 mm (r = 12 mm)
        [SerializeField] private float leadMeters = 0.006f;             // L = 6 mm/rev (single start)
        [SerializeField] private float screwLengthMeters = 0.220f;       // Length = 220 mm
        [SerializeField] private float strokeLengthMeters = 0.140f;      // Usable linear travel = 140 mm
        [SerializeField] private float guideRailRadius = 0.005f;        // 10 mm diameter guide shafts
        [SerializeField] private float threadFrictionCoeff = 0.15f;     // Bronze-on-steel friction

        [Header("Transforms")]
        [SerializeField] private Transform screwRotor;
        [SerializeField] private Transform carriageTransform;

        private float currentScrewAngleDeg = 0f;
        private float currentLinearPosMeters = 0f;
        private bool isReversingTravel = false;
        private bool isAssemblyBuilt = false;

        public float ScrewDiameterMeters => screwRadiusMeters * 2f;
        public float LeadMeters => leadMeters;
        public float PitchRadiusMeters => screwRadiusMeters - (leadMeters * 0.25f);

        /// <summary>
        /// Helix lead angle lambda in degrees: tan(lambda) = L / (pi * dm)
        /// </summary>
        public float LeadAngleDeg
        {
            get
            {
                float dm = 2f * PitchRadiusMeters;
                return Mathf.Atan(leadMeters / (Mathf.PI * dm)) * Mathf.Rad2Deg;
            }
        }

        public float FrictionAngleDeg => Mathf.Atan(threadFrictionCoeff) * Mathf.Rad2Deg;
        public bool IsSelfLocking => LeadAngleDeg < FrictionAngleDeg;

        /// <summary>
        /// Linear velocity in m/s: v = (L * N) / 60
        /// </summary>
        public float LinearSpeedMps => (leadMeters * currentInputRPM) / 60f;

        /// <summary>
        /// Theoretical axial thrust force in Newtons: F = (2 * pi * eta * T) / L
        /// </summary>
        public float AxialThrustNewtons => (2f * Mathf.PI * efficiency * inputTorqueNm) / leadMeters;

        /// <summary>
        /// Mechanical Advantage / Force multiplication ratio (N / N·m)
        /// </summary>
        public float ThrustPerTorqueRatio => (2f * Mathf.PI * efficiency) / leadMeters;

        public float CarriageDisplacementMm => currentLinearPosMeters * 1000f;

        public override string MechanismName => "Lead Screw & Nut";
        public override string CategoryTitle => "Mechanical Drives & Linear Actuators";
        public override float GearRatio => 1.0f; // 1 screw revolution = 6 mm linear advance
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

            float l = screwLengthMeters;
            float r = screwRadiusMeters;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(l * 0.5f, -0.040f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(l + 0.08f, 0.12f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(l + 0.08f, 0.020f, 0.12f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Ground Machine Bedplate",
                "Heavily ribbed cast-iron machine base supporting fixed and floating lead screw bearings.",
                $"Length: {(l + 0.08f) * 1000f:F1} mm | Cast Iron ASTM A48");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Fixed End Thrust Bearing Block (at X = 0)
            GameObject fixBlock = new GameObject("Fixed_Thrust_BearingBlock");
            fixBlock.transform.SetParent(transform, false);
            fixBlock.transform.localPosition = new Vector3(0f, -0.015f, 0f);
            MeshFilter mfFb = fixBlock.AddComponent<MeshFilter>();
            MeshRenderer mrFb = fixBlock.AddComponent<MeshRenderer>();
            mfFb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.038f, 0.055f, 0.030f, r);
            mrFb.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            fixBlock.AddComponent<BoxCollider>().size = new Vector3(0.038f, 0.055f, 0.030f);
            InspectableComponent inspFb = fixBlock.AddComponent<InspectableComponent>();
            inspFb.SetMetadata("Fixed Thrust Bearing Mount",
                "Contains paired angular contact bearings absorbing axial drilling and positioning thrust loads.",
                "Back-to-back 7201 Angular Contact Bearings | Axial Rigidity: 250 N/μm");
            inspFb.SetExplosionVector(Vector3.left, 0.07f, 0.11f);

            // 3. Floating End Radial Bearing Block (at X = l)
            GameObject floatBlock = new GameObject("Floating_Radial_BearingBlock");
            floatBlock.transform.SetParent(transform, false);
            floatBlock.transform.localPosition = new Vector3(l, -0.015f, 0f);
            MeshFilter mfFlb = floatBlock.AddComponent<MeshFilter>();
            MeshRenderer mrFlb = floatBlock.AddComponent<MeshRenderer>();
            mfFlb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.032f, 0.055f, 0.025f, r * 0.8f);
            mrFlb.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
            floatBlock.AddComponent<BoxCollider>().size = new Vector3(0.032f, 0.055f, 0.025f);
            InspectableComponent inspFlb = floatBlock.AddComponent<InspectableComponent>();
            inspFlb.SetMetadata("Floating Radial Bearing Mount",
                "Deep groove ball bearing accommodating thermal expansion of the steel screw shaft.",
                "Axially floating 6000 Bearing | Prevents thermal buckling");
            inspFlb.SetExplosionVector(Vector3.right, 0.07f, 0.11f);

            // 4. Dual Hardened Ground Guide Shafts (at Z = -0.035 and +0.035)
            float[] zRails = new float[] { -0.035f, 0.035f };
            for (int i = 0; i < 2; i++)
            {
                GameObject railGo = new GameObject($"Linear_Guide_Rail_{i}");
                railGo.transform.SetParent(transform, false);
                railGo.transform.localPosition = new Vector3(l * 0.5f, 0f, zRails[i]);
                railGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                MeshFilter mfR = railGo.AddComponent<MeshFilter>();
                MeshRenderer mrR = railGo.AddComponent<MeshRenderer>();
                mfR.mesh = GearMeshBuilder.BuildCylinderMesh(guideRailRadius, l, 20, true);
                mrR.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
                railGo.AddComponent<BoxCollider>().size = new Vector3(guideRailRadius * 2f, l, guideRailRadius * 2f);
            }

            // 5. Lead Screw Shaft Rotor (Rotates around local X axis)
            GameObject screwRotorGo = new GameObject("Lead_Screw_Rotor");
            screwRotorGo.transform.SetParent(transform, false);
            screwRotorGo.transform.localPosition = Vector3.zero;
            screwRotor = screwRotorGo.transform;

            GameObject screwBody = new GameObject("Acme_Threaded_Shaft");
            screwBody.transform.SetParent(screwRotor, false);
            screwBody.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // Helical mesh was along Z, orient along X
            MeshFilter mfScrew = screwBody.AddComponent<MeshFilter>();
            MeshRenderer mrScrew = screwBody.AddComponent<MeshRenderer>();
            mfScrew.mesh = GearMeshBuilder.BuildLeadScrewMesh(r, leadMeters, l);
            mrScrew.material = CreateMaterial(new Color(0.88f, 0.88f, 0.90f), 0.85f, 0.80f); // Polished Steel Acme Screw
            screwBody.AddComponent<MeshCollider>().sharedMesh = mfScrew.mesh;
            InspectableComponent inspScrew = screwBody.AddComponent<InspectableComponent>();
            inspScrew.SetMetadata("Trapezoidal Acme Lead Screw",
                "Hardened alloy steel screw with single-start 30° trapezoidal Acme thread for high load capacity.",
                $"Nominal Dia: {ScrewDiameterMeters * 1000f:F1} mm | Lead L: {leadMeters * 1000f:F1} mm/rev | Self-Locking");
            inspScrew.SetExplosionVector(Vector3.up, 0.07f, 0.11f);

            // Drive handwheel / pulley at input end
            GameObject hwGo = new GameObject("Input_Coupler_Handwheel");
            hwGo.transform.SetParent(screwRotor, false);
            hwGo.transform.localPosition = new Vector3(-0.018f, 0f, 0f);
            hwGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            MeshFilter mfHw = hwGo.AddComponent<MeshFilter>();
            MeshRenderer mrHw = hwGo.AddComponent<MeshRenderer>();
            mfHw.mesh = GearMeshBuilder.BuildCylinderMesh(0.022f, 0.014f, 24, true);
            mrHw.material = CreateMaterial(new Color(0.20f, 0.60f, 0.85f), 0.7f, 0.65f); // Tech Blue

            // 6. Traveling Nut Carriage (Translates linearly along X axis)
            GameObject carriageGo = new GameObject("Traveling_Nut_Carriage");
            carriageGo.transform.SetParent(transform, false);
            carriageGo.transform.localPosition = Vector3.zero;
            carriageTransform = carriageGo.transform;

            // Flanged bronze nut body
            GameObject nutBody = new GameObject("Flanged_Bronze_Nut");
            nutBody.transform.SetParent(carriageTransform, false);
            MeshFilter mfNut = nutBody.AddComponent<MeshFilter>();
            MeshRenderer mrNut = nutBody.AddComponent<MeshRenderer>();
            mfNut.mesh = GearMeshBuilder.BuildBasePlateMesh(0.045f, 0.085f, 0.035f);
            mrNut.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            nutBody.AddComponent<BoxCollider>().size = new Vector3(0.045f, 0.035f, 0.085f);
            InspectableComponent inspNut = nutBody.AddComponent<InspectableComponent>();
            inspNut.SetMetadata("Phosphor Bronze Traveling Nut Carriage",
                "Self-lubricating CuSn12 phosphor bronze nut mated to dual linear ball bushings.",
                $"Material: ASTM B505 C93200 Bronze | Thread Fit: Class 2G | Max Axial Thrust: {AxialThrustNewtons:F0} N");
            inspNut.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

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
            currentScrewAngleDeg += deltaDeg;

            // Linear travel advance: dx = (L / 360°) * dtheta
            float dir = isReversingTravel ? -1f : 1f;
            float deltaX = (leadMeters / 360f) * Mathf.Abs(deltaDeg) * dir;
            currentLinearPosMeters += deltaX;

            // Auto-reversing limit boundaries:
            float minX = 0.030f;
            float maxX = minX + strokeLengthMeters;
            if (currentLinearPosMeters >= maxX)
            {
                currentLinearPosMeters = maxX;
                isReversingTravel = true;
            }
            else if (currentLinearPosMeters <= minX)
            {
                currentLinearPosMeters = minX;
                isReversingTravel = false;
            }

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            if (screwRotor != null)
            {
                screwRotor.localRotation = Quaternion.Euler(currentScrewAngleDeg, 0f, 0f);
            }

            if (carriageTransform != null)
            {
                carriageTransform.localPosition = new Vector3(currentLinearPosMeters, 0f, 0f);
            }
        }

        public override void ResetMechanism()
        {
            currentScrewAngleDeg = 0f;
            currentLinearPosMeters = 0.030f;
            isReversingTravel = false;
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Screw Nominal Diameter (d): {ScrewDiameterMeters * 1000f:F1} mm\n" +
                   $"• Thread Pitch / Lead (L): {leadMeters * 1000f:F1} mm/rev (Single Start)\n" +
                   $"• Helix Lead Angle (λ): {LeadAngleDeg:F2}° (Self-Locking: {IsSelfLocking})\n" +
                   $"• Current Carriage Position: {CarriageDisplacementMm:F1} mm / {strokeLengthMeters * 1000f:F0} mm\n" +
                   $"• Linear Travel Speed: {LinearSpeedMps * 1000f:F1} mm/s ({LinearSpeedMps:F3} m/s)\n" +
                   $"• Axial Thrust at {inputTorqueNm:F1} N·m: {AxialThrustNewtons:F0} N ({AxialThrustNewtons / 9.81f:F0} kgf)\n" +
                   $"• Force Multiplication Ratio: {ThrustPerTorqueRatio:F0} N / (N·m)\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM | Direction: {(isReversingTravel ? "Reverse Return" : "Forward Feed")}";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Lead Screw and Traveling Nut converts rotational motor torque into massive linear thrust force with high positional accuracy. " +
                   "Because the helix lead angle λ (4.8°) is substantially smaller than the bronze-steel friction angle φ (8.5°), the system is inherently self-locking: " +
                   "no amount of axial payload force on the carriage can cause the screw to backspin. " +
                   $"With a 6 mm lead, 10 N·m of input torque generates over {AxialThrustNewtons:F0} N (~{AxialThrustNewtons / 9.81f:F0} kgf) of axial clamping force, " +
                   "making it the prime linear actuator for lathe beds, machine presses, and heavy positioning stages.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Helix Lead Angle: tan(λ) = L / (π·d_m)\n" +
                   $"λ = atan({leadMeters * 1000f:F1}mm / (π × {2f * PitchRadiusMeters * 1000f:F1}mm)) = {LeadAngleDeg:F2}°\n" +
                   "Linear Velocity: v = (L·N) / 60\n" +
                   $"v = ({leadMeters * 1000f:F1}mm × {currentInputRPM:F0} RPM) / 60 = {LinearSpeedMps * 1000f:F1} mm/s\n" +
                   "Axial Thrust: F = (2·π·η·T_in) / L\n" +
                   $"F = (2·π × {efficiency:F2} × {inputTorqueNm:F1} N·m) / {leadMeters:F3}m = {AxialThrustNewtons:F0} N";
        }
    }
}

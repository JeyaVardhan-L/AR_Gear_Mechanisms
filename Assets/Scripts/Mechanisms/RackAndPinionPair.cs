using System;
using UnityEngine;
using MechXR.Interaction;
using MechXR.Procedural;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Precision Rack and Pinion Motion Conversion Assembly.
    /// Converts continuous rotational motion into reciprocating linear displacement.
    /// Architecture:
    /// - Drive Pinion: Z = 20 teeth, module m = 3.5 mm, pitch diameter = 70.0 mm.
    /// - Linear Gear Rack: Length = 260.0 mm, linear tooth pitch = 11.0 mm (pi * m), 22 teeth.
    /// Kinematics:
    /// - Linear velocity: v = omega * r_pitch = (2*pi*n/60) * 0.035 m = 0.440 m/s (440 mm/s at 120 RPM).
    /// - Linear travel per turn: S_rev = pi * m * Z = 220.0 mm/rev.
    /// </summary>
    public class RackAndPinionPair : MechanismBase
    {
        public const int PinionTeeth = 20;
        private const float Module = 0.0035f; // 3.5 mm module
        private const float PinionPitchRadius = (PinionTeeth * Module) * 0.5f; // 35.0 mm
        private const float Addendum = 0.0035f;
        private const float Dedendum = 0.0042f;
        private const float FaceWidth = 0.020f; // 20.0 mm
        private const float RackLength = 0.260f; // 260.0 mm
        private const float RackHeight = 0.022f; // 22.0 mm
        private const float StrokeLimit = 0.060f; // +/- 60 mm travel

        [Header("Mechanism Transforms")]
        [SerializeField] private Transform pinionRotor;
        [SerializeField] private Transform rackTransform;

        private float currentPinionAngle = 0f;
        private float currentRackX = 0f;
        private float rackDirection = 1f;
        private bool isAssemblyBuilt = false;

        public override string MechanismName => "RACK AND PINION";
        public override string CategoryTitle => "ROTARY-TO-LINEAR CONVERTER";
        public override float GearRatio => PinionTeeth; // 20.0 ratio factor (linear travel 220 mm/rev)
        public override float OutputRPM => (InputRPM * PinionPitchRadius * 2f * Mathf.PI / 60f) * 1000f; // Linear velocity in mm/s

        protected override void Awake()
        {
            base.Awake();
            if (!isAssemblyBuilt)
            {
                BuildMechanismAssembly();
            }
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            Shader standardShader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Diffuse");
            Material matDriver = Resources.Load<Material>("Art/Materials/Mat_Gear_Driver") ??
                                 new Material(standardShader) { color = new Color(0.12f, 0.58f, 0.95f, 1f) };
            Material matDriven = Resources.Load<Material>("Art/Materials/Mat_Gear_Driven") ??
                                 new Material(standardShader) { color = new Color(0.85f, 0.88f, 0.92f, 1f) };
            Material matShaft = Resources.Load<Material>("Art/Materials/Mat_Steel_Shaft") ??
                                new Material(standardShader) { color = new Color(0.75f, 0.78f, 0.82f, 1f) };
            Material matBase = Resources.Load<Material>("Art/Materials/Mat_Baseplate") ??
                               new Material(standardShader) { color = new Color(0.11f, 0.12f, 0.15f, 1f) };

            // 1. Bedplate with Linear Dovetail Guide Ways
            GameObject bedGo = new GameObject("Part_01_DovetailBedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0f, -0.045f, 0f);
            MeshFilter bedMf = bedGo.AddComponent<MeshFilter>();
            bedMf.sharedMesh = GearMeshBuilder.BuildBasePlateMesh(0.300f, 0.140f, 0.022f);
            MeshRenderer bedMr = bedGo.AddComponent<MeshRenderer>();
            bedMr.sharedMaterial = matBase;
            MeshCollider bedCol = bedGo.AddComponent<MeshCollider>();
            bedCol.sharedMesh = bedMf.sharedMesh;
            InspectableComponent bedComp = bedGo.AddComponent<InspectableComponent>();
            bedComp.Initialize("Linear Guide Bedplate", "Precision-ground cast bedplate with linear guideway supporting reciprocating rack travel.",
                "Toolroom cast iron EN-GJL-250 (GG25). Ground guideway flatness 0.010 mm.", new Vector3(0f, -0.08f, 0f));

            // 2. Drive Pinion Gear
            GameObject pinionGo = new GameObject("Part_02_DrivePinion");
            pinionGo.transform.SetParent(transform, false);
            pinionGo.transform.localPosition = new Vector3(0f, PinionPitchRadius, 0f);

            GameObject pinionRotorGo = new GameObject("Rotor_Pinion");
            pinionRotorGo.transform.SetParent(pinionGo.transform, false);
            pinionRotor = pinionRotorGo.transform;

            MeshFilter pinionMf = pinionRotorGo.AddComponent<MeshFilter>();
            pinionMf.sharedMesh = GearMeshBuilder.BuildSpurGearMesh(PinionTeeth, PinionPitchRadius, Addendum, Dedendum, FaceWidth, 0.005f, 0);
            MeshRenderer pinionMr = pinionRotorGo.AddComponent<MeshRenderer>();
            pinionMr.sharedMaterial = matDriver;
            MeshCollider pinionCol = pinionGo.AddComponent<MeshCollider>();
            pinionCol.sharedMesh = pinionMf.sharedMesh;
            InspectableComponent pinionComp = pinionGo.AddComponent<InspectableComponent>();
            pinionComp.Initialize("Drive Pinion Gear (20T)", "Circular spur gear converting motor torque into tangential linear thrust.",
                "Case-hardened 16MnCr5 steel, module 3.5 mm, precision ground teeth.", new Vector3(0f, 0.07f, 0.08f));

            // 3. Linear Gear Rack
            GameObject rackGo = new GameObject("Part_03_LinearRack");
            rackGo.transform.SetParent(transform, false);
            rackGo.transform.localPosition = new Vector3(0f, -0.003f, 0f);
            rackTransform = rackGo.transform;

            MeshFilter rackMf = rackGo.AddComponent<MeshFilter>();
            float toothPitch = Mathf.PI * Module;
            rackMf.sharedMesh = GearMeshBuilder.BuildRackMesh(RackLength, RackHeight, FaceWidth, 22, toothPitch, Addendum + Dedendum);
            MeshRenderer rackMr = rackGo.AddComponent<MeshRenderer>();
            rackMr.sharedMaterial = matDriven;
            MeshCollider rackCol = rackGo.AddComponent<MeshCollider>();
            rackCol.sharedMesh = rackMf.sharedMesh;
            InspectableComponent rackComp = rackGo.AddComponent<InspectableComponent>();
            rackComp.Initialize("Machined Gear Rack (22T)", "Linear toothed bar meshing with pinion. Translates horizontally along guide ways.",
                "Quenched alloy steel C45, induction-hardened teeth (55 HRC), length 260 mm.", new Vector3(0f, -0.04f, 0.08f));

            // 4. Pinion Ground Shaft
            GameObject shaftGo = new GameObject("Part_04_PinionShaft");
            shaftGo.transform.SetParent(transform, false);
            shaftGo.transform.localPosition = new Vector3(0f, PinionPitchRadius, 0f);
            MeshFilter shaftMf = shaftGo.AddComponent<MeshFilter>();
            shaftMf.sharedMesh = GearMeshBuilder.BuildCylinderMesh(0.005f, 0.085f, 20, false);
            MeshRenderer shaftMr = shaftGo.AddComponent<MeshRenderer>();
            shaftMr.sharedMaterial = matShaft;
            MeshCollider shaftCol = shaftGo.AddComponent<MeshCollider>();
            shaftCol.sharedMesh = shaftMf.sharedMesh;
            InspectableComponent shaftComp = shaftGo.AddComponent<InspectableComponent>();
            shaftComp.Initialize("Pinion Drive Shaft", "Precision ground transmission shaft carrying the drive pinion gear.",
                "42CrMo4 alloy steel, diameter 10 mm, h6 tolerance.", new Vector3(0f, 0f, 0.09f));

            // 5. Bearing Pedestal Left
            GameObject pedLGo = new GameObject("Part_05_Pedestal_Left");
            pedLGo.transform.SetParent(transform, false);
            pedLGo.transform.localPosition = new Vector3(0f, PinionPitchRadius - 0.025f, -0.035f);
            MeshFilter pedLMf = pedLGo.AddComponent<MeshFilter>();
            pedLMf.sharedMesh = GearMeshBuilder.BuildPillowBlockMesh(0.055f, 0.055f, 0.018f, 0.010f);
            MeshRenderer pedLMr = pedLGo.AddComponent<MeshRenderer>();
            pedLMr.sharedMaterial = matBase;
            MeshCollider pedLCol = pedLGo.AddComponent<MeshCollider>();
            pedLCol.sharedMesh = pedLMf.sharedMesh;
            InspectableComponent pedLComp = pedLGo.AddComponent<InspectableComponent>();
            pedLComp.Initialize("Pillow Block Mount (Rear)", "Rigid bearing pedestal carrying pinion radial separating forces.",
                "Ductile cast iron EN-GJS-400-15.", new Vector3(0f, 0.04f, -0.07f));

            // 6. Bearing Pedestal Right / Front
            GameObject pedRGo = new GameObject("Part_06_Pedestal_Right");
            pedRGo.transform.SetParent(transform, false);
            pedRGo.transform.localPosition = new Vector3(0f, PinionPitchRadius - 0.025f, 0.035f);
            MeshFilter pedRMf = pedRGo.AddComponent<MeshFilter>();
            pedRMf.sharedMesh = GearMeshBuilder.BuildPillowBlockMesh(0.055f, 0.055f, 0.018f, 0.010f);
            MeshRenderer pedRMr = pedRGo.AddComponent<MeshRenderer>();
            pedRMr.sharedMaterial = matBase;
            MeshCollider pedRCol = pedRGo.AddComponent<MeshCollider>();
            pedRCol.sharedMesh = pedRMf.sharedMesh;
            InspectableComponent pedRComp = pedRGo.AddComponent<InspectableComponent>();
            pedRComp.Initialize("Pillow Block Mount (Front)", "Front bearing pedestal supporting pinion cantilever load.",
                "Ductile cast iron EN-GJS-400-15.", new Vector3(0f, 0.04f, 0.07f));

            // 7. Ball Bearings
            GameObject b1Go = new GameObject("Part_07_BallBearing_Rear");
            b1Go.transform.SetParent(transform, false);
            b1Go.transform.localPosition = new Vector3(0f, PinionPitchRadius, -0.035f);
            MeshFilter b1Mf = b1Go.AddComponent<MeshFilter>();
            b1Mf.sharedMesh = GearMeshBuilder.BuildBearingRingMesh(0.010f, 0.005f, 0.008f);
            MeshRenderer b1Mr = b1Go.AddComponent<MeshRenderer>();
            b1Mr.sharedMaterial = matShaft;
            MeshCollider b1Col = b1Go.AddComponent<MeshCollider>();
            b1Col.sharedMesh = b1Mf.sharedMesh;
            InspectableComponent b1Comp = b1Go.AddComponent<InspectableComponent>();
            b1Comp.Initialize("Deep-Groove Ball Bearing", "Radial rolling bearing absorbing pinion torque reactions.",
                "100Cr6 bearing steel, ABEC-5 precision rating.", new Vector3(0f, 0f, -0.06f));

            GameObject b2Go = new GameObject("Part_08_BallBearing_Front");
            b2Go.transform.SetParent(transform, false);
            b2Go.transform.localPosition = new Vector3(0f, PinionPitchRadius, 0.035f);
            MeshFilter b2Mf = b2Go.AddComponent<MeshFilter>();
            b2Mf.sharedMesh = GearMeshBuilder.BuildBearingRingMesh(0.010f, 0.005f, 0.008f);
            MeshRenderer b2Mr = b2Go.AddComponent<MeshRenderer>();
            b2Mr.sharedMaterial = matShaft;
            MeshCollider b2Col = b2Go.AddComponent<MeshCollider>();
            b2Col.sharedMesh = b2Mf.sharedMesh;
            InspectableComponent b2Comp = b2Go.AddComponent<InspectableComponent>();
            b2Comp.Initialize("Deep-Groove Ball Bearing", "Front radial rolling bearing supporting drive shaft.",
                "100Cr6 bearing steel, ABEC-5 precision rating.", new Vector3(0f, 0f, 0.06f));

            isAssemblyBuilt = true;
        }

        protected override void UpdateKinematics(float inputAngularVelDegPerSec, float deltaTime)
        {
            // Pinion rotation
            float dAngle = inputAngularVelDegPerSec * deltaTime;
            currentPinionAngle += dAngle;

            if (pinionRotor != null)
            {
                pinionRotor.localRotation = Quaternion.AngleAxis(currentPinionAngle, Vector3.forward);
            }

            // Linear rack displacement: dx = (dAngle in rad) * r_pitch
            float dAngleRad = dAngle * Mathf.Deg2Rad;
            float dx = dAngleRad * PinionPitchRadius;
            currentRackX += dx * rackDirection;

            // Reciprocate when reaching stroke limits to keep rack engaged on bedplate
            if (currentRackX > StrokeLimit)
            {
                currentRackX = StrokeLimit;
                rackDirection = -1f;
            }
            else if (currentRackX < -StrokeLimit)
            {
                currentRackX = -StrokeLimit;
                rackDirection = 1f;
            }

            if (rackTransform != null)
            {
                rackTransform.localPosition = new Vector3(currentRackX, -0.003f, 0f);
            }
        }

        public override void ResetMechanism()
        {
            currentPinionAngle = 0f;
            currentRackX = 0f;
            rackDirection = 1f;

            if (pinionRotor != null) pinionRotor.localRotation = Quaternion.identity;
            if (rackTransform != null) rackTransform.localPosition = new Vector3(0f, -0.003f, 0f);
        }

        public override string GetCalculationSummary()
        {
            return $"• Pinion Teeth (Z): {PinionTeeth}\n" +
                   $"• Module (m): 3.5 mm\n" +
                   $"• Linear Pitch (p): 11.0 mm (π·m)\n" +
                   $"• Pinion Pitch Radius: 35.0 mm\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM\n" +
                   $"• Linear Speed (v): {OutputRPM:F1} mm/s\n" +
                   $"• Travel per Turn: 220.0 mm/rev\n" +
                   $"• Motion: Rotary-to-Linear Reciprocating Slide";
        }

        public override string GetEngineeringExplanation()
        {
            return "A rack and pinion converts rotational torque into linear thrust. " +
                   "As the drive pinion rotates, each tooth engagement displaces the linear rack by exactly one tooth pitch (p = π·m = 11.0 mm). " +
                   "At 120 RPM input velocity, tangential velocity at the pitch circle drives the rack at 440 mm/s. " +
                   "Dovetail guide ways absorb transverse separating forces to ensure smooth linear motion.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Linear Pitch: p = π · m = 3.5π = 11.00 mm\n" +
                   "Displacement per Revolution: S_rev = π · m · Z = 220.0 mm/rev\n" +
                   "Linear Velocity: v = ω · r_pitch = (2π·n / 60) · 0.035 m = 0.440 m/s\n" +
                   "Tangential Thrust Force: F_t = 2 · T_in / d_pinion\n" +
                   "Radial Separating Force: F_r = F_t · tan(α) = F_t · tan(20°)";
        }
    }
}

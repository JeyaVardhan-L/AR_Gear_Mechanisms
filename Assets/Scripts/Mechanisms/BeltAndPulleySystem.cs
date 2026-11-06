using System;
using UnityEngine;
using MechXR.Interaction;
using MechXR.Procedural;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Precision V-Belt and Pulley Speed Reduction System.
    /// Demonstrates parallel shaft power transmission via flexible friction drive.
    /// Architecture:
    /// - Driver Pulley: Pitch diameter D1 = 60.0 mm (r1 = 30.0 mm), V-groove profile.
    /// - Driven Pulley: Pitch diameter D2 = 120.0 mm (r2 = 60.0 mm), 4-spoke cast architecture with V-groove.
    /// - Center Distance: C = 160.0 mm.
    /// - Continuous Belt: Trapezoidal cross section continuous closed loop wrapping tangent around pulleys.
    /// Kinematics:
    /// - Speed Ratio i = D2 / D1 = 120 / 60 = 2.0 : 1.
    /// - Output RPM = Input RPM / 2.0 = 60.0 RPM (at 120 RPM input).
    /// - Same direction of rotation (open belt arrangement).
    /// - Belt linear speed v = pi * D1 * (n1 / 60) = 0.377 m/s (377 mm/s).
    /// </summary>
    public class BeltAndPulleySystem : MechanismBase
    {
        public const float DriverDiameter = 0.060f; // 60 mm
        public const float DrivenDiameter = 0.120f; // 120 mm
        public const float CenterDistance = 0.160f; // 160 mm

        private const float DriverRadius = DriverDiameter * 0.5f; // 30 mm
        private const float DrivenRadius = DrivenDiameter * 0.5f; // 60 mm
        private const float PulleyFaceWidth = 0.018f; // 18 mm
        private const float BeltWidth = 0.012f; // 12 mm
        private const float BeltThickness = 0.006f; // 6 mm

        [Header("Mechanism Rotors")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform drivenRotor;

        private float currentDriverAngle = 0f;
        private float currentDrivenAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override string MechanismName => "BELT AND PULLEY SYSTEM";
        public override string CategoryTitle => "FRICTION POWER TRANSMISSION";
        public override float GearRatio => DrivenDiameter / DriverDiameter; // 2.0 : 1
        public override float OutputRPM => InputRPM / GearRatio; // 60.0 RPM at 120 RPM input

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

            // Belt material: Anthracite matte rubber
            Material matBelt = new Material(standardShader)
            {
                color = new Color(0.14f, 0.15f, 0.17f, 1.0f),
                name = "Mat_RubberBelt"
            };
            if (matBelt.HasProperty("_Metallic")) matBelt.SetFloat("_Metallic", 0.05f);
            if (matBelt.HasProperty("_Smoothness")) matBelt.SetFloat("_Smoothness", 0.25f);

            float halfSpan = CenterDistance * 0.5f;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Part_01_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0f, -0.075f, 0f);
            MeshFilter bedMf = bedGo.AddComponent<MeshFilter>();
            bedMf.sharedMesh = GearMeshBuilder.BuildBasePlateMesh(0.260f, 0.120f, 0.016f);
            MeshRenderer bedMr = bedGo.AddComponent<MeshRenderer>();
            bedMr.sharedMaterial = matBase;
            MeshCollider bedCol = bedGo.AddComponent<MeshCollider>();
            bedCol.sharedMesh = bedMf.sharedMesh;
            InspectableComponent bedComp = bedGo.AddComponent<InspectableComponent>();
            bedComp.Initialize("Dual-Shaft Machine Bedplate", "Rigid datum plate maintaining center distance C = 160 mm between transmission shafts.",
                "Precision machined toolroom aluminum alloy 6061-T6.", new Vector3(0f, -0.08f, 0f));

            // 2. Driver Pulley (Left, x = -halfSpan)
            GameObject driverGo = new GameObject("Part_02_DriverPulley");
            driverGo.transform.SetParent(transform, false);
            driverGo.transform.localPosition = new Vector3(-halfSpan, 0f, 0f);

            GameObject driverRotorGo = new GameObject("Rotor_Driver");
            driverRotorGo.transform.SetParent(driverGo.transform, false);
            driverRotor = driverRotorGo.transform;

            MeshFilter driverMf = driverRotorGo.AddComponent<MeshFilter>();
            driverMf.sharedMesh = GearMeshBuilder.BuildPulleyMesh(DriverRadius, DriverRadius * 0.9f, 0.005f, PulleyFaceWidth, 0.006f, 0);
            MeshRenderer driverMr = driverRotorGo.AddComponent<MeshRenderer>();
            driverMr.sharedMaterial = matDriver;
            MeshCollider driverCol = driverGo.AddComponent<MeshCollider>();
            driverCol.sharedMesh = driverMf.sharedMesh;
            InspectableComponent driverComp = driverGo.AddComponent<InspectableComponent>();
            driverComp.Initialize("Driver Pulley (Dia 60 mm)", "Machined aluminum V-pulley coupled to high-speed input shaft (120 RPM).",
                "Alloy 7075-T6, hard anodized V-groove (angle 38 deg).", new Vector3(-0.06f, 0f, 0.08f));

            // 3. Driven Pulley (Right, x = +halfSpan, 4 spokes)
            GameObject drivenGo = new GameObject("Part_03_DrivenPulley");
            drivenGo.transform.SetParent(transform, false);
            drivenGo.transform.localPosition = new Vector3(halfSpan, 0f, 0f);

            GameObject drivenRotorGo = new GameObject("Rotor_Driven");
            drivenRotorGo.transform.SetParent(drivenGo.transform, false);
            drivenRotor = drivenRotorGo.transform;

            MeshFilter drivenMf = drivenRotorGo.AddComponent<MeshFilter>();
            drivenMf.sharedMesh = GearMeshBuilder.BuildPulleyMesh(DrivenRadius, DrivenRadius * 0.9f, 0.006f, PulleyFaceWidth, 0.006f, 4);
            MeshRenderer drivenMr = drivenRotorGo.AddComponent<MeshRenderer>();
            drivenMr.sharedMaterial = matDriven;
            MeshCollider drivenCol = drivenGo.AddComponent<MeshCollider>();
            drivenCol.sharedMesh = drivenMf.sharedMesh;
            InspectableComponent drivenComp = drivenGo.AddComponent<InspectableComponent>();
            drivenComp.Initialize("Driven Pulley (Dia 120 mm)", "Large cast V-pulley with 4 radial spokes. Outputs 60 RPM with 2:1 torque multiplication.",
                "Cast iron EN-GJL-200 with 4-spoke cast architecture.", new Vector3(0.06f, 0f, 0.08f));

            // 4. Continuous Wrapped Transmission Belt
            GameObject beltGo = new GameObject("Part_04_ContinuousBelt");
            beltGo.transform.SetParent(transform, false);
            beltGo.transform.localPosition = new Vector3(-halfSpan, 0f, 0f); // Offset to start at driver center
            MeshFilter beltMf = beltGo.AddComponent<MeshFilter>();
            beltMf.sharedMesh = GearMeshBuilder.BuildContinuousBeltMesh(DriverRadius * 0.92f, DrivenRadius * 0.92f, CenterDistance, BeltWidth, BeltThickness);
            MeshRenderer beltMr = beltGo.AddComponent<MeshRenderer>();
            beltMr.sharedMaterial = matBelt;
            MeshCollider beltCol = beltGo.AddComponent<MeshCollider>();
            beltCol.sharedMesh = beltMf.sharedMesh;
            InspectableComponent beltComp = beltGo.AddComponent<InspectableComponent>();
            beltComp.Initialize("Continuous V-Belt Loop", "Endless fiber-reinforced rubber V-belt transmitting friction power between shafts.",
                "Chloroprene rubber with high-modulus polyester tension cords.", new Vector3(0f, 0.07f, 0.07f));

            // 5. Driver Shaft
            GameObject shaft1Go = new GameObject("Part_05_DriverShaft");
            shaft1Go.transform.SetParent(transform, false);
            shaft1Go.transform.localPosition = new Vector3(-halfSpan, 0f, 0f);
            MeshFilter shaft1Mf = shaft1Go.AddComponent<MeshFilter>();
            shaft1Mf.sharedMesh = GearMeshBuilder.BuildCylinderMesh(0.005f, 0.075f, 20, false);
            MeshRenderer shaft1Mr = shaft1Go.AddComponent<MeshRenderer>();
            shaft1Mr.sharedMaterial = matShaft;
            MeshCollider shaft1Col = shaft1Go.AddComponent<MeshCollider>();
            shaft1Col.sharedMesh = shaft1Mf.sharedMesh;
            InspectableComponent shaft1Comp = shaft1Go.AddComponent<InspectableComponent>();
            shaft1Comp.Initialize("Driver Input Shaft", "Center-ground transmission shaft carrying driver pulley.",
                "42CrMo4 alloy steel, diameter 10 mm.", new Vector3(-0.06f, 0f, -0.07f));

            // 6. Driven Shaft
            GameObject shaft2Go = new GameObject("Part_06_DrivenShaft");
            shaft2Go.transform.SetParent(transform, false);
            shaft2Go.transform.localPosition = new Vector3(halfSpan, 0f, 0f);
            MeshFilter shaft2Mf = shaft2Go.AddComponent<MeshFilter>();
            shaft2Mf.sharedMesh = GearMeshBuilder.BuildCylinderMesh(0.006f, 0.075f, 20, false);
            MeshRenderer shaft2Mr = shaft2Go.AddComponent<MeshRenderer>();
            shaft2Mr.sharedMaterial = matShaft;
            MeshCollider shaft2Col = shaft2Go.AddComponent<MeshCollider>();
            shaft2Col.sharedMesh = shaft2Mf.sharedMesh;
            InspectableComponent shaft2Comp = shaft2Go.AddComponent<InspectableComponent>();
            shaft2Comp.Initialize("Driven Output Shaft", "Heavy output transmission shaft delivering 2:1 multiplied torque.",
                "42CrMo4 alloy steel, diameter 12 mm.", new Vector3(0.06f, 0f, -0.07f));

            // 7. Pillow Block Driver
            GameObject ped1Go = new GameObject("Part_07_Pedestal_Driver");
            ped1Go.transform.SetParent(transform, false);
            ped1Go.transform.localPosition = new Vector3(-halfSpan, -0.035f, -0.025f);
            MeshFilter ped1Mf = ped1Go.AddComponent<MeshFilter>();
            ped1Mf.sharedMesh = GearMeshBuilder.BuildPillowBlockMesh(0.050f, 0.065f, 0.018f, 0.010f);
            MeshRenderer ped1Mr = ped1Go.AddComponent<MeshRenderer>();
            ped1Mr.sharedMaterial = matBase;
            MeshCollider ped1Col = ped1Go.AddComponent<MeshCollider>();
            ped1Col.sharedMesh = ped1Mf.sharedMesh;
            InspectableComponent ped1Comp = ped1Go.AddComponent<InspectableComponent>();
            ped1Comp.Initialize("Driver Bearing Pedestal", "Cast iron pillow block bearing housing absorbing belt tension preload.",
                "Cast iron EN-GJL-250.", new Vector3(-0.05f, -0.04f, -0.05f));

            // 8. Pillow Block Driven
            GameObject ped2Go = new GameObject("Part_08_Pedestal_Driven");
            ped2Go.transform.SetParent(transform, false);
            ped2Go.transform.localPosition = new Vector3(halfSpan, -0.035f, -0.025f);
            MeshFilter ped2Mf = ped2Go.AddComponent<MeshFilter>();
            ped2Mf.sharedMesh = GearMeshBuilder.BuildPillowBlockMesh(0.050f, 0.065f, 0.018f, 0.012f);
            MeshRenderer ped2Mr = ped2Go.AddComponent<MeshRenderer>();
            ped2Mr.sharedMaterial = matBase;
            MeshCollider ped2Col = ped2Go.AddComponent<MeshCollider>();
            ped2Col.sharedMesh = ped2Mf.sharedMesh;
            InspectableComponent ped2Comp = ped2Go.AddComponent<InspectableComponent>();
            ped2Comp.Initialize("Driven Bearing Pedestal", "Cast iron pillow block absorbing output radial reaction loads.",
                "Cast iron EN-GJL-250.", new Vector3(0.05f, -0.04f, -0.05f));

            // 9. Ball Bearing Driver
            GameObject bb1Go = new GameObject("Part_09_Bearing_Driver");
            bb1Go.transform.SetParent(transform, false);
            bb1Go.transform.localPosition = new Vector3(-halfSpan, 0f, -0.025f);
            MeshFilter bb1Mf = bb1Go.AddComponent<MeshFilter>();
            bb1Mf.sharedMesh = GearMeshBuilder.BuildBearingRingMesh(0.010f, 0.005f, 0.008f);
            MeshRenderer bb1Mr = bb1Go.AddComponent<MeshRenderer>();
            bb1Mr.sharedMaterial = matShaft;
            MeshCollider bb1Col = bb1Go.AddComponent<MeshCollider>();
            bb1Col.sharedMesh = bb1Mf.sharedMesh;
            InspectableComponent bb1Comp = bb1Go.AddComponent<InspectableComponent>();
            bb1Comp.Initialize("Driver Ball Bearing", "Deep-groove rolling bearing supporting input shaft.",
                "100Cr6 steel, ABEC-5 rating.", new Vector3(-0.04f, 0f, -0.04f));

            // 10. Ball Bearing Driven
            GameObject bb2Go = new GameObject("Part_10_Bearing_Driven");
            bb2Go.transform.SetParent(transform, false);
            bb2Go.transform.localPosition = new Vector3(halfSpan, 0f, -0.025f);
            MeshFilter bb2Mf = bb2Go.AddComponent<MeshFilter>();
            bb2Mf.sharedMesh = GearMeshBuilder.BuildBearingRingMesh(0.012f, 0.006f, 0.008f);
            MeshRenderer bb2Mr = bb2Go.AddComponent<MeshRenderer>();
            bb2Mr.sharedMaterial = matShaft;
            MeshCollider bb2Col = bb2Go.AddComponent<MeshCollider>();
            bb2Col.sharedMesh = bb2Mf.sharedMesh;
            InspectableComponent bb2Comp = bb2Go.AddComponent<InspectableComponent>();
            bb2Comp.Initialize("Driven Ball Bearing", "Deep-groove rolling bearing supporting driven shaft.",
                "100Cr6 steel, ABEC-5 rating.", new Vector3(0.04f, 0f, -0.04f));

            isAssemblyBuilt = true;
        }

        protected override void UpdateKinematics(float inputAngularVelDegPerSec, float deltaTime)
        {
            // Driver rotation around local Z
            float dDriver = inputAngularVelDegPerSec * deltaTime;
            currentDriverAngle += dDriver;

            // Driven rotation: same direction!
            // omega_driven = omega_driver / 2.0
            float dDriven = (inputAngularVelDegPerSec / GearRatio) * deltaTime;
            currentDrivenAngle += dDriven;

            if (driverRotor != null)
            {
                driverRotor.localRotation = Quaternion.AngleAxis(currentDriverAngle, Vector3.forward);
            }

            if (drivenRotor != null)
            {
                drivenRotor.localRotation = Quaternion.AngleAxis(currentDrivenAngle, Vector3.forward);
            }
        }

        public override void ResetMechanism()
        {
            currentDriverAngle = 0f;
            currentDrivenAngle = 0f;

            if (driverRotor != null) driverRotor.localRotation = Quaternion.identity;
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.identity;
        }

        public override string GetCalculationSummary()
        {
            return $"• Driver Diameter (D₁): 60.0 mm\n" +
                   $"• Driven Diameter (D₂): 120.0 mm\n" +
                   $"• Center Distance (C): 160.0 mm\n" +
                   $"• Speed Ratio (i): {GearRatio:F1}:1 (Reduction)\n" +
                   $"• Input Speed (n₁): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n₂): {OutputRPM:F0} RPM\n" +
                   $"• Belt Linear Speed (v): 0.38 m/s (377 mm/s)\n" +
                   $"• Rotation: Same Direction (Open Belt)";
        }

        public override string GetEngineeringExplanation()
        {
            return "Belt and pulley drives transmit mechanical power between parallel shafts using friction between the flexible belt and pulley V-grooves. " +
                   "Because the belt wraps around the outside of both pulleys without crossing, both shafts rotate in the identical direction. " +
                   "The 60 mm driver turns the 120 mm driven pulley, cutting rotational speed in half (60 RPM) while doubling output torque via friction grip. " +
                   "The continuous belt loop provides vibration damping and smooth power transmission.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Speed Ratio: i = D₂ / D₁ = 120.0 / 60.0 = 2.0\n" +
                   "Output Velocity: n₂ = n₁ / i = " + $"{currentInputRPM:F0} / 2.0 = {OutputRPM:F0} RPM\n" +
                   "Belt Velocity: v = π · D₁ · (n₁ / 60) = π · 0.060 · 2 = 0.377 m/s\n" +
                   "Center Distance: C = 160.0 mm\n" +
                   "Belt Length: L ≈ 2C + π(D₁ + D₂)/2 + (D₂ - D₁)² / (4C) ≈ 608.5 mm";
        }
    }
}

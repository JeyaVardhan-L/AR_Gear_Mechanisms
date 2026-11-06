using System;
using UnityEngine;
using MechXR.Interaction;
using MechXR.Procedural;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Precision Planetary (Epicyclic) Gear System.
    /// Architecture:
    /// - Sun Gear (Driver): Z_s = 18 teeth, module m = 2.5 mm, pitch diameter = 45.0 mm.
    /// - Planet Gears (3x Idlers): Z_p = 18 teeth, module m = 2.5 mm, pitch diameter = 45.0 mm.
    /// - Ring Gear (Fixed Annulus): Z_r = 54 teeth, internal pitch diameter = 135.0 mm.
    /// - Planet Carrier (Output): 3-arm triangular spider plate with spindle pins at orbit radius = 45.0 mm.
    /// Kinematics:
    /// Ratio i = (Z_s + Z_r) / Z_s = (18 + 54) / 18 = 4.0 : 1.
    /// Output RPM = Input RPM / 4.0 = 30.0 RPM (at 120 RPM input).
    /// </summary>
    public class PlanetaryGearSystem : MechanismBase
    {
        public const int SunTeeth = 18;
        public const int PlanetTeeth = 18;
        public const int RingTeeth = 54;
        public const int NumPlanets = 3;

        private const float Module = 0.0025f; // 2.5 mm module
        private const float SunPitchRadius = (SunTeeth * Module) * 0.5f; // 22.5 mm
        private const float PlanetPitchRadius = (PlanetTeeth * Module) * 0.5f; // 22.5 mm
        private const float OrbitRadius = SunPitchRadius + PlanetPitchRadius; // 45.0 mm
        private const float RingPitchRadius = (RingTeeth * Module) * 0.5f; // 67.5 mm
        private const float RingOuterRadius = 0.082f; // 82.0 mm
        private const float GearFaceWidth = 0.016f; // 16.0 mm
        private const float Addendum = 0.0025f;
        private const float Dedendum = 0.0031f;

        [Header("Rotational Transforms")]
        [SerializeField] private Transform sunRotor;
        [SerializeField] private Transform carrierRotor;
        [SerializeField] private Transform[] planetRotors = new Transform[NumPlanets];

        private float currentSunAngle = 0f;
        private float currentCarrierAngle = 0f;
        private float currentPlanetRelativeAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override string MechanismName => "PLANETARY GEAR SYSTEM";
        public override string CategoryTitle => "EPICYCLIC TRANSMISSION";
        public override float GearRatio => (float)(SunTeeth + RingTeeth) / SunTeeth; // 4.0 : 1
        public override float OutputRPM => InputRPM / GearRatio; // 30.0 RPM at 120 RPM input

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

            // Clear any preexisting children
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

            // 1. Bedplate
            GameObject bedGo = new GameObject("Part_01_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0f, -0.090f, 0f);
            MeshFilter bedMf = bedGo.AddComponent<MeshFilter>();
            bedMf.sharedMesh = GearMeshBuilder.BuildBasePlateMesh(0.200f, 0.180f, 0.018f);
            MeshRenderer bedMr = bedGo.AddComponent<MeshRenderer>();
            bedMr.sharedMaterial = matBase;
            MeshCollider bedCol = bedGo.AddComponent<MeshCollider>();
            bedCol.sharedMesh = bedMf.sharedMesh;
            InspectableComponent bedComp = bedGo.AddComponent<InspectableComponent>();
            bedComp.Initialize("Bedplate & Stator Mount", "Structural datum mounting plate anchoring the fixed ring gear and bearing pedestals.",
                "Precision machined toolroom aluminum alloy 6061-T6. Surface flatness 0.015 mm.", new Vector3(0f, -0.08f, 0f));

            // 2. Fixed Outer Ring Gear (Annulus)
            GameObject ringGo = new GameObject("Part_02_RingGear_Annulus");
            ringGo.transform.SetParent(transform, false);
            ringGo.transform.localPosition = Vector3.zero;
            MeshFilter ringMf = ringGo.AddComponent<MeshFilter>();
            ringMf.sharedMesh = GearMeshBuilder.BuildInternalRingGearMesh(RingTeeth, RingPitchRadius, Addendum, Dedendum, RingOuterRadius, GearFaceWidth);
            MeshRenderer ringMr = ringGo.AddComponent<MeshRenderer>();
            ringMr.sharedMaterial = matBase;
            MeshCollider ringCol = ringGo.AddComponent<MeshCollider>();
            ringCol.sharedMesh = ringMf.sharedMesh;
            InspectableComponent ringComp = ringGo.AddComponent<InspectableComponent>();
            ringComp.Initialize("Internal Ring Gear (Annulus)", "Fixed outer internal gear (54T). Serves as stationary reaction member in this reduction configuration.",
                "Alloy steel 42CrMo4, internal induction-hardened teeth (58 HRC).", new Vector3(0f, 0.10f, 0f));

            // 3. Central Sun Gear (Driver)
            GameObject sunGo = new GameObject("Part_03_SunGear_Driver");
            sunGo.transform.SetParent(transform, false);
            sunGo.transform.localPosition = Vector3.zero;

            GameObject sunRotorGo = new GameObject("Rotor_Sun");
            sunRotorGo.transform.SetParent(sunGo.transform, false);
            sunRotor = sunRotorGo.transform;

            MeshFilter sunMf = sunRotorGo.AddComponent<MeshFilter>();
            sunMf.sharedMesh = GearMeshBuilder.BuildSpurGearMesh(SunTeeth, SunPitchRadius, Addendum, Dedendum, GearFaceWidth, 0.005f, 0);
            MeshRenderer sunMr = sunRotorGo.AddComponent<MeshRenderer>();
            sunMr.sharedMaterial = matDriver;
            MeshCollider sunCol = sunGo.AddComponent<MeshCollider>();
            sunCol.sharedMesh = sunMf.sharedMesh;
            InspectableComponent sunComp = sunGo.AddComponent<InspectableComponent>();
            sunComp.Initialize("Sun Gear (Input)", "Central high-speed input driver gear (18T). Directly driven by input motor shaft at 120 RPM.",
                "Case-hardened steel 16MnCr5, ground involute flank profile.", new Vector3(0f, 0f, 0.09f));

            // 4. Planet Carrier (Output) with 3 Spindle Pins
            GameObject carrierGo = new GameObject("Part_04_PlanetCarrier_Output");
            carrierGo.transform.SetParent(transform, false);
            carrierGo.transform.localPosition = new Vector3(0f, 0f, -0.016f);

            GameObject carrierRotorGo = new GameObject("Rotor_Carrier");
            carrierRotorGo.transform.SetParent(carrierGo.transform, false);
            carrierRotor = carrierRotorGo.transform;

            MeshFilter carrierMf = carrierRotorGo.AddComponent<MeshFilter>();
            carrierMf.sharedMesh = GearMeshBuilder.BuildPlanetaryCarrierMesh(NumPlanets, OrbitRadius, 0.004f, 0.020f, 0.056f, 0.006f, 0.006f);
            MeshRenderer carrierMr = carrierRotorGo.AddComponent<MeshRenderer>();
            carrierMr.sharedMaterial = matDriven;
            MeshCollider carrierCol = carrierGo.AddComponent<MeshCollider>();
            carrierCol.sharedMesh = carrierMf.sharedMesh;
            InspectableComponent carrierComp = carrierGo.AddComponent<InspectableComponent>();
            carrierComp.Initialize("Planet Carrier (Output)", "Rotary spider frame carrying 3 planet spindles. Outputs reduced speed (30 RPM, 4:1) with high torque.",
                "Ductile cast iron EN-GJS-600-3 (GGG60) with precision ground spindle pins.", new Vector3(0f, 0f, -0.08f));

            // 5. 3 Planet Gears (Mounted on Carrier)
            planetRotors = new Transform[NumPlanets];
            for (int p = 0; p < NumPlanets; p++)
            {
                float armAngle = (p * (360f / NumPlanets)) * Mathf.Deg2Rad;
                Vector3 planetPos = new Vector3(Mathf.Cos(armAngle) * OrbitRadius, Mathf.Sin(armAngle) * OrbitRadius, 0f);

                GameObject planetGo = new GameObject($"Part_{p + 5:00}_PlanetGear_{p + 1}");
                planetGo.transform.SetParent(carrierRotor, false);
                planetGo.transform.localPosition = planetPos;

                GameObject planetRotorGo = new GameObject("Rotor_Planet");
                planetRotorGo.transform.SetParent(planetGo.transform, false);
                planetRotors[p] = planetRotorGo.transform;

                MeshFilter planetMf = planetRotorGo.AddComponent<MeshFilter>();
                planetMf.sharedMesh = GearMeshBuilder.BuildSpurGearMesh(PlanetTeeth, PlanetPitchRadius, Addendum, Dedendum, GearFaceWidth * 0.9f, 0.0045f, 0);
                MeshRenderer planetMr = planetRotorGo.AddComponent<MeshRenderer>();
                planetMr.sharedMaterial = matDriven;
                MeshCollider planetCol = planetGo.AddComponent<MeshCollider>();
                planetCol.sharedMesh = planetMf.sharedMesh;

                InspectableComponent planetComp = planetGo.AddComponent<InspectableComponent>();
                planetComp.Initialize($"Planet Gear {p + 1} of 3", "Idler gear orbiting between central sun and fixed ring. Simultaneously meshes with both.",
                    "Nitrocarburized 42CrMo4 steel with needle roller bearings.", new Vector3(Mathf.Cos(armAngle) * 0.05f, Mathf.Sin(armAngle) * 0.05f, 0.07f));
            }

            // 8. Input Shaft
            GameObject inShaftGo = new GameObject("Part_08_InputShaft");
            inShaftGo.transform.SetParent(transform, false);
            inShaftGo.transform.localPosition = new Vector3(0f, 0f, 0.035f);
            MeshFilter inShaftMf = inShaftGo.AddComponent<MeshFilter>();
            inShaftMf.sharedMesh = GearMeshBuilder.BuildCylinderMesh(0.005f, 0.060f, 20, false);
            MeshRenderer inShaftMr = inShaftGo.AddComponent<MeshRenderer>();
            inShaftMr.sharedMaterial = matShaft;
            MeshCollider inShaftCol = inShaftGo.AddComponent<MeshCollider>();
            inShaftCol.sharedMesh = inShaftMf.sharedMesh;
            InspectableComponent inShaftComp = inShaftGo.AddComponent<InspectableComponent>();
            inShaftComp.Initialize("High-Speed Input Shaft", "Center ground input transmission shaft coupled to sun gear.",
                "42CrMo4 high-tensile alloy steel, diameter 10 mm.", new Vector3(0f, 0f, 0.08f));

            // 9. Output Shaft
            GameObject outShaftGo = new GameObject("Part_09_OutputShaft");
            outShaftGo.transform.SetParent(transform, false);
            outShaftGo.transform.localPosition = new Vector3(0f, 0f, -0.045f);
            MeshFilter outShaftMf = outShaftGo.AddComponent<MeshFilter>();
            outShaftMf.sharedMesh = GearMeshBuilder.BuildCylinderMesh(0.006f, 0.055f, 20, false);
            MeshRenderer outShaftMr = outShaftGo.AddComponent<MeshRenderer>();
            outShaftMr.sharedMaterial = matShaft;
            MeshCollider outShaftCol = outShaftGo.AddComponent<MeshCollider>();
            outShaftCol.sharedMesh = outShaftMf.sharedMesh;
            InspectableComponent outShaftComp = outShaftGo.AddComponent<InspectableComponent>();
            outShaftComp.Initialize("Heavy-Duty Output Shaft", "Low-speed, high-torque output shaft rigidly locked to planet carrier hub.",
                "Quenched and tempered 34CrNiMo6 steel, diameter 12 mm.", new Vector3(0f, 0f, -0.08f));

            // 10. Bearing Pedestal Mount
            GameObject pedestalGo = new GameObject("Part_10_PedestalMount");
            pedestalGo.transform.SetParent(transform, false);
            pedestalGo.transform.localPosition = new Vector3(0f, -0.045f, -0.045f);
            MeshFilter pedMf = pedestalGo.AddComponent<MeshFilter>();
            pedMf.sharedMesh = GearMeshBuilder.BuildPillowBlockMesh(0.065f, 0.085f, 0.022f, 0.012f);
            MeshRenderer pedMr = pedestalGo.AddComponent<MeshRenderer>();
            pedMr.sharedMaterial = matBase;
            MeshCollider pedCol = pedestalGo.AddComponent<MeshCollider>();
            pedCol.sharedMesh = pedMf.sharedMesh;
            InspectableComponent pedComp = pedestalGo.AddComponent<InspectableComponent>();
            pedComp.Initialize("Output Bearing Pedestal", "Rigid cast iron pillow block housing the output deep-groove ball bearing.",
                "Gray cast iron EN-GJL-250 (GG25).", new Vector3(0f, -0.05f, -0.04f));

            isAssemblyBuilt = true;
        }

        protected override void UpdateKinematics(float inputAngularVelDegPerSec, float deltaTime)
        {
            // Input Sun angular displacement
            float dSun = inputAngularVelDegPerSec * deltaTime;
            currentSunAngle += dSun;

            // Carrier angular displacement: omega_carrier = omega_sun / 4.0
            float dCarrier = (inputAngularVelDegPerSec / GearRatio) * deltaTime;
            currentCarrierAngle += dCarrier;

            // Planet spin relative to carrier: omega_rel = -(omega_sun - omega_carrier) * (Z_s / Z_p)
            // Since Z_s == Z_p, ratio is 1.0
            float dPlanetRel = -(dSun - dCarrier);
            currentPlanetRelativeAngle += dPlanetRel;

            // Apply rotations around local Z-axis
            if (sunRotor != null)
            {
                sunRotor.localRotation = Quaternion.AngleAxis(currentSunAngle, Vector3.forward);
            }

            if (carrierRotor != null)
            {
                carrierRotor.localRotation = Quaternion.AngleAxis(currentCarrierAngle, Vector3.forward);
            }

            if (planetRotors != null)
            {
                for (int p = 0; p < planetRotors.Length; p++)
                {
                    if (planetRotors[p] != null)
                    {
                        planetRotors[p].localRotation = Quaternion.AngleAxis(currentPlanetRelativeAngle, Vector3.forward);
                    }
                }
            }
        }

        public override void ResetMechanism()
        {
            currentSunAngle = 0f;
            currentCarrierAngle = 0f;
            currentPlanetRelativeAngle = 0f;

            if (sunRotor != null) sunRotor.localRotation = Quaternion.identity;
            if (carrierRotor != null) carrierRotor.localRotation = Quaternion.identity;
            if (planetRotors != null)
            {
                for (int p = 0; p < planetRotors.Length; p++)
                {
                    if (planetRotors[p] != null) planetRotors[p].localRotation = Quaternion.identity;
                }
            }
        }

        public override string GetCalculationSummary()
        {
            return $"• Sun Teeth (Z_s): {SunTeeth}\n" +
                   $"• Planet Teeth (Z_p): {PlanetTeeth} (x{NumPlanets})\n" +
                   $"• Ring Teeth (Z_r): {RingTeeth} (Fixed Annulus)\n" +
                   $"• Gear Ratio (i): {GearRatio:F1}:1 (Epicyclic Reduction)\n" +
                   $"• Input Speed (n_sun): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n_carrier): {OutputRPM:F0} RPM\n" +
                   $"• Torque Multiplier: {GearRatio:F1}×\n" +
                   $"• Orbit Radius: 45.0 mm\n" +
                   $"• Configuration: Fixed Ring / Carrier Output";
        }

        public override string GetEngineeringExplanation()
        {
            return "Planetary (epicyclic) gear systems achieve high torque density and compact coaxial speed reduction. " +
                   "In this stationary ring configuration, the central sun gear drives 3 planet gears that roll inside the fixed internal ring gear. " +
                   "The planet carrier orbits at reduced velocity (1/4 input speed), multiplying output torque by 4× while distributing mechanical loads across three conjugate tooth meshes simultaneously.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Epicyclic Ratio: i = (Z_s + Z_r) / Z_s = (18 + 54) / 18 = 4.0\n" +
                   "Carrier Output Velocity: n_carrier = n_sun / i = " + $"{currentInputRPM:F0} / 4.0 = {OutputRPM:F0} RPM\n" +
                   "Planet Relative Spin: n_rel = -(n_sun - n_carrier) · (Z_s / Z_p) = -90.0 RPM\n" +
                   "Pitch Diameters: d_sun = 45.0 mm | d_planet = 45.0 mm | d_ring = 135.0 mm\n" +
                   "Torque Multiplier: T_carrier = T_sun · i · η ≈ 3.88 · T_sun";
        }
    }
}

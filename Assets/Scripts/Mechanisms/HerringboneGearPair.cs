using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 12: Herringbone Gear Pair (Double-Helical).
    /// Demonstrates heavy-duty double-helical gear transmission:
    /// - Driver Pinion (Z1 = 20, Double-Helical Chevron, Electric Blue)
    /// - Driven Gear (Z2 = 40, Double-Helical Chevron, 4-Spoked Industrial Gold)
    /// Key engineering principles:
    /// 1. Axial Thrust Cancellation: The two opposing helix angles (+20° and -20°) produce equal
    ///    and opposite axial forces that cancel internally across the tooth apex (Net Fa = 0).
    /// 2. Extreme Power Density: Combines the smoothness of helical gears with zero axial bearing loads.
    /// Speed Ratio: i = Z2 / Z1 = 40 / 20 = 2.0 : 1.
    /// </summary>
    public class HerringboneGearPair : MechanismBase
    {
        [Header("Gear Parameters")]
        [SerializeField] private int driverTeeth = 20;
        [SerializeField] private int drivenTeeth = 40;
        [SerializeField] private float moduleMeters = 0.0035f;
        [SerializeField] private float faceWidth = 0.024f; // Symmetrical width for chevron halves
        [SerializeField] private float boreRadius = 0.006f;
        [SerializeField] private float helixAngleDeg = 20.0f;

        [Header("Rotors")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform drivenRotor;

        private float currentDriverAngle = 0f;
        private float currentDrivenAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => driverTeeth;
        public override int DrivenTeeth => drivenTeeth;
        public float HelixAngleDeg => helixAngleDeg;
        public override string MechanismName => "Herringbone Gear Pair";
        public override string CategoryTitle => "Double-Helical Thrust-Canceling Drive";
        public override float GearRatio => (float)drivenTeeth / driverTeeth; // 40 / 20 = 2.0
        public override float OutputRPM => currentInputRPM / GearRatio;      // 120 / 2 = 60 RPM

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = (driverTeeth * moduleMeters) * 0.5f; // 35.0 mm
            float r2 = (drivenTeeth * moduleMeters) * 0.5f; // 70.0 mm
            float centerDist = r1 + r2;                     // 105.0 mm

            Vector3 posDriver = new Vector3(-centerDist * 0.5f, 0f, 0f);
            Vector3 posDriven = new Vector3(centerDist * 0.5f, 0f, 0f);

            // 1. Machine Bed
            GameObject bedGo = new GameObject("Machine_Bed");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0f, -0.075f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.24f, 0.16f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.024f, 0.16f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Heavy Duty Machine Bed",
                "High-rigidity machine bed supporting heavy-duty parallel shaft bearings.",
                "Material: GG-25 Cast Iron | Zero Axial Bearing Thrust");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Driver Herringbone Pinion (Z1 = 20)
            GameObject driverGo = new GameObject("Driver_Rotor");
            driverGo.transform.SetParent(transform, false);
            driverGo.transform.localPosition = posDriver;
            driverRotor = driverGo.transform;

            GameObject drGear = new GameObject("Herringbone_Pinion_20T");
            drGear.transform.SetParent(driverRotor, false);
            MeshFilter mfDr = drGear.AddComponent<MeshFilter>();
            MeshRenderer mrDr = drGear.AddComponent<MeshRenderer>();
            mfDr.mesh = GearMeshBuilder.BuildHerringboneGearMesh(
                driverTeeth, r1, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, helixAngleDeg, spokeCount: 0);
            mrDr.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            drGear.AddComponent<MeshCollider>().sharedMesh = mfDr.mesh;
            InspectableComponent inspDr = drGear.AddComponent<InspectableComponent>();
            inspDr.SetMetadata("Driver Herringbone Pinion (Z₁ = 20)",
                "Double-helical pinion featuring opposed V-shaped chevron teeth. Cancels net axial thrust forces internally.",
                $"Teeth: {driverTeeth} | Chevron Angle: ±{helixAngleDeg:F0}° | Net Axial Thrust: 0.0 N | Pitch Dia: {r1*2000f:F1} mm");
            inspDr.SetExplosionVector(Vector3.forward, 0.07f, 0.11f);

            GameObject drShaft = new GameObject("Driver_Shaft");
            drShaft.transform.SetParent(driverRotor, false);
            MeshFilter mfDrShaft = drShaft.AddComponent<MeshFilter>();
            MeshRenderer mrDrShaft = drShaft.AddComponent<MeshRenderer>();
            mfDrShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrDrShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            drShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // 3. Driven Herringbone Gear (Z2 = 40, Spoked)
            GameObject drivenGo = new GameObject("Driven_Rotor");
            drivenGo.transform.SetParent(transform, false);
            drivenGo.transform.localPosition = posDriven;
            drivenRotor = drivenGo.transform;

            GameObject dnGear = new GameObject("Herringbone_Gear_40T");
            dnGear.transform.SetParent(drivenRotor, false);
            MeshFilter mfDn = dnGear.AddComponent<MeshFilter>();
            MeshRenderer mrDn = dnGear.AddComponent<MeshRenderer>();
            mfDn.mesh = GearMeshBuilder.BuildHerringboneGearMesh(
                drivenTeeth, r2, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, helixAngleDeg, spokeCount: 4, spokeWidth: 0.009f);
            mrDn.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            dnGear.AddComponent<MeshCollider>().sharedMesh = mfDn.mesh;
            InspectableComponent inspDn = dnGear.AddComponent<InspectableComponent>();
            inspDn.SetMetadata("Driven Herringbone Gear (Z₂ = 40, Spoked)",
                "Large double-helical spoked wheel. Eliminates specialized thrust collar requirements and transmits smooth, shock-free torque.",
                $"Teeth: {drivenTeeth} | Ratio: 2.0:1 | Pitch Dia: {r2*2000f:F1} mm | Output: {OutputRPM:F0} RPM");
            inspDn.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            GameObject dnShaft = new GameObject("Driven_Shaft");
            dnShaft.transform.SetParent(drivenRotor, false);
            MeshFilter mfDnShaft = dnShaft.AddComponent<MeshFilter>();
            MeshRenderer mrDnShaft = dnShaft.AddComponent<MeshRenderer>();
            mfDnShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrDnShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            dnShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // Bearing Pillow Blocks
            CreateBearingPillows(posDriver);
            CreateBearingPillows(posDriven);

            ResetMechanism();
        }

        private void CreateBearingPillows(Vector3 shaftPos)
        {
            float[] zOffsets = new float[] { -0.04f, 0.04f };
            for (int i = 0; i < zOffsets.Length; i++)
            {
                GameObject pb = new GameObject("PillowBlock");
                pb.transform.SetParent(transform, false);
                pb.transform.localPosition = new Vector3(shaftPos.x, -0.038f, zOffsets[i]);
                MeshFilter mf = pb.AddComponent<MeshFilter>();
                MeshRenderer mr = pb.AddComponent<MeshRenderer>();
                mf.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.024f, 0.052f, 0.016f, boreRadius);
                mr.material = CreateMaterial(new Color(0.24f, 0.26f, 0.30f), 0.5f, 0.3f);
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
            currentDriverAngle += inputAngularVelDegPerSec * deltaTime;
            currentDriverAngle %= 360f;

            // Counter-rotation on parallel shafts: omega_2 = -omega_1 * (Z1 / Z2)
            float ratio = (float)driverTeeth / drivenTeeth;
            currentDrivenAngle -= (inputAngularVelDegPerSec * ratio) * deltaTime;
            currentDrivenAngle %= 360f;

            if (driverRotor != null) driverRotor.localRotation = Quaternion.Euler(0, 0, currentDriverAngle);
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.Euler(0, 0, currentDrivenAngle);
        }

        public override void ResetMechanism()
        {
            currentDriverAngle = 0f;
            currentDrivenAngle = (360f / drivenTeeth) * 0.5f;

            if (driverRotor != null) driverRotor.localRotation = Quaternion.Euler(0, 0, currentDriverAngle);
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.Euler(0, 0, currentDrivenAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Herringbone Gear Pair\n" +
                   $"• Velocity Ratio (i): {GearRatio:F2}:1\n" +
                   $"• Input Speed (n₁): {currentInputRPM:F0} RPM\n" +
                   $"• Output Speed (n₂): {OutputRPM:F1} RPM\n" +
                   $"• Input Torque: {InputTorque:F1} N·m\n" +
                   $"• Output Torque: {OutputTorqueNm:F1} N·m\n" +
                   $"• Efficiency (η): {(Efficiency * 100f):F0}%\n" +
                   $"• Input Power: {InputPowerWatts:F1} W\n" +
                   $"• Output Power: {OutputPowerWatts:F1} W";
        }

        public override string GetEngineeringExplanation()
        {
            return "Double-helical chevron teeth cancel opposing axial thrust forces (F_axial = 0 N) while preserving smooth tooth engagement. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = Z₂ / Z₁ = 40 / 20 = 2.0:1 | Double Helical ±20°\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


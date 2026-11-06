using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 07: Simple Gear Train with Idler.
    /// Demonstrates 3-gear power transmission across 3 parallel shafts:
    /// - Driver Pinion (Z1 = 20, Electric Blue)
    /// - Intermediate Idler Gear (Z2 = 28, Dark Slate)
    /// - Driven Gear (Z3 = 40, Spoked Industrial Gold)
    /// Key engineering principle:
    /// The idler reverses rotation twice (output turns in the SAME direction as input),
    /// while the overall gear ratio i = Z3 / Z1 = 2.0 : 1 is strictly independent of idler tooth count.
    /// </summary>
    public class SimpleGearTrain : MechanismBase
    {
        [Header("Gear Train Parameters")]
        [SerializeField] private int driverTeeth = 20;
        [SerializeField] private int idlerTeeth = 28;
        [SerializeField] private int drivenTeeth = 40;
        [SerializeField] private float moduleMeters = 0.0035f; // 3.5mm module
        [SerializeField] private float faceWidth = 0.016f;
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Rotors")]
        [SerializeField] private Transform driverRotor;
        [SerializeField] private Transform idlerRotor;
        [SerializeField] private Transform drivenRotor;

        private float currentDriverAngle = 0f;
        private float currentIdlerAngle = 0f;
        private float currentDrivenAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => driverTeeth;
        public int IdlerTeeth => idlerTeeth;
        public override int DrivenTeeth => drivenTeeth;
        public override string MechanismName => "Simple Gear Train";
        public override string CategoryTitle => "Multi-Shaft Transmission with Idler";
        public override float GearRatio => (float)drivenTeeth / driverTeeth; // 40 / 20 = 2.0
        public override float OutputRPM => currentInputRPM / GearRatio;      // 120 / 2 = 60 RPM (Same direction)

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = (driverTeeth * moduleMeters) * 0.5f; // 35 mm
            float r2 = (idlerTeeth * moduleMeters) * 0.5f;  // 49 mm
            float r3 = (drivenTeeth * moduleMeters) * 0.5f; // 70 mm

            float centerDist12 = r1 + r2; // 84 mm
            float centerDist23 = r2 + r3; // 119 mm

            // Shaft positions: Idler at origin (0,0,0), Driver at -centerDist12, Driven at +centerDist23
            Vector3 posDriver = new Vector3(-centerDist12, 0f, 0f);
            Vector3 posIdler = Vector3.zero;
            Vector3 posDriven = new Vector3(centerDist23, 0f, 0f);

            // 1. Machine Bed
            float bedWidth = centerDist12 + centerDist23 + 0.12f;
            GameObject bedGo = new GameObject("Machine_Bed");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3((posDriven.x + posDriver.x) * 0.5f, -0.075f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(bedWidth, 0.16f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            BoxCollider colBed = bedGo.AddComponent<BoxCollider>();
            colBed.size = new Vector3(bedWidth, 0.024f, 0.16f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Precision Machine Bed",
                "High-rigidity ground bed supporting 3 parallel shaft axes with zero torsional deflection.",
                "Material: GG-25 Cast Iron | Surface Ground");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Driver Rotor (Pinion Z1 = 20)
            GameObject driverGo = new GameObject("Driver_Rotor");
            driverGo.transform.SetParent(transform, false);
            driverGo.transform.localPosition = posDriver;
            driverRotor = driverGo.transform;

            GameObject drGear = new GameObject("Driver_Gear_20T");
            drGear.transform.SetParent(driverRotor, false);
            MeshFilter mfDr = drGear.AddComponent<MeshFilter>();
            MeshRenderer mrDr = drGear.AddComponent<MeshRenderer>();
            mfDr.mesh = GearMeshBuilder.BuildSpurGearMesh(driverTeeth, r1, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrDr.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            drGear.AddComponent<MeshCollider>().sharedMesh = mfDr.mesh;
            InspectableComponent inspDr = drGear.AddComponent<InspectableComponent>();
            inspDr.SetMetadata("Driver Pinion (Z₁ = 20)",
                "Input drive gear keyed to primary motor shaft.",
                $"Teeth: {driverTeeth} | Module: {moduleMeters*1000f:F1} mm | Pitch Dia: {r1*2000f:F1} mm");
            inspDr.SetExplosionVector(Vector3.forward, 0.06f, 0.10f);

            GameObject drShaft = new GameObject("Driver_Shaft");
            drShaft.transform.SetParent(driverRotor, false);
            MeshFilter mfDrShaft = drShaft.AddComponent<MeshFilter>();
            MeshRenderer mrDrShaft = drShaft.AddComponent<MeshRenderer>();
            mfDrShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.08f, 24, false);
            mrDrShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            drShaft.AddComponent<CapsuleCollider>().height = 0.08f;

            // 3. Idler Rotor (Gear Z2 = 28)
            GameObject idlerGo = new GameObject("Idler_Rotor");
            idlerGo.transform.SetParent(transform, false);
            idlerGo.transform.localPosition = posIdler;
            idlerRotor = idlerGo.transform;

            GameObject idGear = new GameObject("Idler_Gear_28T");
            idGear.transform.SetParent(idlerRotor, false);
            MeshFilter mfId = idGear.AddComponent<MeshFilter>();
            MeshRenderer mrId = idGear.AddComponent<MeshRenderer>();
            mfId.mesh = GearMeshBuilder.BuildSpurGearMesh(idlerTeeth, r2, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 3, spokeWidth: 0.007f);
            mrId.material = CreateMaterial(new Color(0.35f, 0.38f, 0.44f), 0.4f, 0.6f); // Slate Graphite
            idGear.AddComponent<MeshCollider>().sharedMesh = mfId.mesh;
            InspectableComponent inspId = idGear.AddComponent<InspectableComponent>();
            inspId.SetMetadata("Intermediate Idler Gear (Z₂ = 28)",
                "Reverses rotation direction so output turns in the SAME direction as driver. Does not alter overall speed ratio.",
                $"Teeth: {idlerTeeth} | Ratio Contribution: 1.0 (Neutral) | Pitch Dia: {r2*2000f:F1} mm");
            inspId.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            GameObject idShaft = new GameObject("Idler_Shaft");
            idShaft.transform.SetParent(idlerRotor, false);
            MeshFilter mfIdShaft = idShaft.AddComponent<MeshFilter>();
            MeshRenderer mrIdShaft = idShaft.AddComponent<MeshRenderer>();
            mfIdShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.08f, 24, false);
            mrIdShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            idShaft.AddComponent<CapsuleCollider>().height = 0.08f;

            // 4. Driven Rotor (Gear Z3 = 40, Spoked)
            GameObject drivenGo = new GameObject("Driven_Rotor");
            drivenGo.transform.SetParent(transform, false);
            drivenGo.transform.localPosition = posDriven;
            drivenRotor = drivenGo.transform;

            GameObject dnGear = new GameObject("Driven_Gear_40T");
            dnGear.transform.SetParent(drivenRotor, false);
            MeshFilter mfDn = dnGear.AddComponent<MeshFilter>();
            MeshRenderer mrDn = dnGear.AddComponent<MeshRenderer>();
            mfDn.mesh = GearMeshBuilder.BuildSpurGearMesh(drivenTeeth, r3, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 4, spokeWidth: 0.009f);
            mrDn.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            dnGear.AddComponent<MeshCollider>().sharedMesh = mfDn.mesh;
            InspectableComponent inspDn = dnGear.AddComponent<InspectableComponent>();
            inspDn.SetMetadata("Driven Gear (Z₃ = 40, Spoked)",
                "Receives power from idler. Demonstrates 2:1 speed reduction with rotation matching driver.",
                $"Teeth: {drivenTeeth} | Ratio: 2.0:1 | Pitch Dia: {r3*2000f:F1} mm | Output: {OutputRPM:F0} RPM");
            inspDn.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            GameObject dnShaft = new GameObject("Driven_Shaft");
            dnShaft.transform.SetParent(drivenRotor, false);
            MeshFilter mfDnShaft = dnShaft.AddComponent<MeshFilter>();
            MeshRenderer mrDnShaft = dnShaft.AddComponent<MeshRenderer>();
            mfDnShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.08f, 24, false);
            mrDnShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            dnShaft.AddComponent<CapsuleCollider>().height = 0.08f;

            // Bearing Pillow Blocks for all 3 shafts (front and rear)
            CreateBearingPillows(posDriver);
            CreateBearingPillows(posIdler);
            CreateBearingPillows(posDriven);

            ResetMechanism();
        }

        private void CreateBearingPillows(Vector3 shaftPos)
        {
            float[] zOffsets = new float[] { -0.035f, 0.035f };
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
            // Driver: omega_1
            currentDriverAngle += inputAngularVelDegPerSec * deltaTime;
            currentDriverAngle %= 360f;

            // Idler: omega_2 = -omega_1 * (Z1 / Z2)
            float ratio12 = (float)driverTeeth / idlerTeeth;
            currentIdlerAngle -= (inputAngularVelDegPerSec * ratio12) * deltaTime;
            currentIdlerAngle %= 360f;

            // Driven: omega_3 = -omega_2 * (Z2 / Z3) = +omega_1 * (Z1 / Z3)
            float ratioOverall = (float)driverTeeth / drivenTeeth;
            currentDrivenAngle += (inputAngularVelDegPerSec * ratioOverall) * deltaTime;
            currentDrivenAngle %= 360f;

            if (driverRotor != null) driverRotor.localRotation = Quaternion.Euler(0, 0, currentDriverAngle);
            if (idlerRotor != null) idlerRotor.localRotation = Quaternion.Euler(0, 0, currentIdlerAngle);
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.Euler(0, 0, currentDrivenAngle);
        }

        public override void ResetMechanism()
        {
            currentDriverAngle = 0f;
            currentIdlerAngle = (360f / idlerTeeth) * 0.5f; // half pitch phase offset for meshing
            currentDrivenAngle = 0f;

            if (driverRotor != null) driverRotor.localRotation = Quaternion.Euler(0, 0, currentDriverAngle);
            if (idlerRotor != null) idlerRotor.localRotation = Quaternion.Euler(0, 0, currentIdlerAngle);
            if (drivenRotor != null) drivenRotor.localRotation = Quaternion.Euler(0, 0, currentDrivenAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Simple Gear Train\n" +
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
            return "Intermediate idler gear isolates shaft spacing while maintaining input-to-output rotation direction. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = Z₃ / Z₁ = 40 / 20 = 2.0:1 (Independent of Idler Z₂=28)\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


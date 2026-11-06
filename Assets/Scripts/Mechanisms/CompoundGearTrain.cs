using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 08: Compound Gear Train.
    /// Demonstrates multi-stage gear reduction using compound shafts:
    /// - Stage 1: Driver Pinion 1 (Z1 = 18) meshing with Driven Gear 1 (Z2 = 36) -> 2:1 reduction.
    /// - Stage 2: Pinion 2 (Z3 = 18) rigidly keyed on the same countershaft as Z2,
    ///   meshing with Output Gear 2 (Z4 = 54) -> 3:1 reduction.
    /// Overall Ratio: i = i1 * i2 = (36/18) * (54/18) = 2.0 * 3.0 = 6.0 : 1 (Massive torque multiplier).
    /// </summary>
    public class CompoundGearTrain : MechanismBase
    {
        [Header("Gear Train Parameters")]
        [SerializeField] private int z1 = 18;
        [SerializeField] private int z2 = 36;
        [SerializeField] private int z3 = 18;
        [SerializeField] private int z4 = 54;
        [SerializeField] private float moduleMeters = 0.0035f;
        [SerializeField] private float faceWidth = 0.014f;
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Rotors")]
        [SerializeField] private Transform inputRotor;
        [SerializeField] private Transform compoundRotor;
        [SerializeField] private Transform outputRotor;

        private float currentInputAngle = 0f;
        private float currentCompoundAngle = 0f;
        private float currentOutputAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => z1;
        public override int DrivenTeeth => z4;
        public int Z1 => z1;
        public int Z2 => z2;
        public int Z3 => z3;
        public int Z4 => z4;
        public override string MechanismName => "Compound Gear Train";
        public override string CategoryTitle => "Dual-Stage Speed Reduction";
        public override float GearRatio => ((float)z2 / z1) * ((float)z4 / z3); // 2.0 * 3.0 = 6.0
        public override float OutputRPM => currentInputRPM / GearRatio;         // 120 / 6 = 20 RPM

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = (z1 * moduleMeters) * 0.5f; // 31.5 mm
            float r2 = (z2 * moduleMeters) * 0.5f; // 63.0 mm
            float r3 = (z3 * moduleMeters) * 0.5f; // 31.5 mm
            float r4 = (z4 * moduleMeters) * 0.5f; // 94.5 mm

            float dist12 = r1 + r2; // 94.5 mm
            float dist23 = r3 + r4; // 126.0 mm

            Vector3 posInput = new Vector3(-dist12, 0f, 0f);
            Vector3 posCompound = Vector3.zero;
            Vector3 posOutput = new Vector3(dist23, 0f, 0f);

            float zOffsetStage1 = 0.016f;
            float zOffsetStage2 = -0.016f;

            // 1. Machine Bed
            float bedWidth = dist12 + dist23 + 0.14f;
            GameObject bedGo = new GameObject("Machine_Bed");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3((posOutput.x + posInput.x) * 0.5f, -0.075f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(bedWidth, 0.18f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(bedWidth, 0.024f, 0.18f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Dual-Stage Machine Bed",
                "High-stiffness cast frame with 3 precision-bored parallel shaft housings.",
                "Material: GG-25 Gray Cast Iron | Stress Relieved");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Input Shaft & Pinion Z1
            GameObject inRotorGo = new GameObject("Input_Rotor");
            inRotorGo.transform.SetParent(transform, false);
            inRotorGo.transform.localPosition = posInput;
            inputRotor = inRotorGo.transform;

            GameObject gearZ1 = new GameObject("Gear_Z1_18T");
            gearZ1.transform.SetParent(inputRotor, false);
            gearZ1.transform.localPosition = new Vector3(0, 0, zOffsetStage1);
            MeshFilter mfZ1 = gearZ1.AddComponent<MeshFilter>();
            MeshRenderer mrZ1 = gearZ1.AddComponent<MeshRenderer>();
            mfZ1.mesh = GearMeshBuilder.BuildSpurGearMesh(z1, r1, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrZ1.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            gearZ1.AddComponent<MeshCollider>().sharedMesh = mfZ1.mesh;
            InspectableComponent inspZ1 = gearZ1.AddComponent<InspectableComponent>();
            inspZ1.SetMetadata("Primary Driver Pinion (Z₁ = 18)",
                "Stage 1 input pinion on high-speed motor shaft.",
                $"Teeth: {z1} | Pitch Dia: {r1*2000f:F1} mm | Input: {currentInputRPM:F0} RPM");
            inspZ1.SetExplosionVector(Vector3.forward, 0.06f, 0.10f);

            GameObject inShaft = new GameObject("Input_Shaft");
            inShaft.transform.SetParent(inputRotor, false);
            MeshFilter mfInShaft = inShaft.AddComponent<MeshFilter>();
            MeshRenderer mrInShaft = inShaft.AddComponent<MeshRenderer>();
            mfInShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrInShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            inShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // 3. Compound Countershaft (Carries Z2 and Z3 locked together)
            GameObject compRotorGo = new GameObject("Compound_Rotor");
            compRotorGo.transform.SetParent(transform, false);
            compRotorGo.transform.localPosition = posCompound;
            compoundRotor = compRotorGo.transform;

            GameObject gearZ2 = new GameObject("Compound_Gear_Z2_36T");
            gearZ2.transform.SetParent(compoundRotor, false);
            gearZ2.transform.localPosition = new Vector3(0, 0, zOffsetStage1);
            MeshFilter mfZ2 = gearZ2.AddComponent<MeshFilter>();
            MeshRenderer mrZ2 = gearZ2.AddComponent<MeshRenderer>();
            mfZ2.mesh = GearMeshBuilder.BuildSpurGearMesh(z2, r2, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 4, spokeWidth: 0.008f);
            mrZ2.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            gearZ2.AddComponent<MeshCollider>().sharedMesh = mfZ2.mesh;
            InspectableComponent inspZ2 = gearZ2.AddComponent<InspectableComponent>();
            inspZ2.SetMetadata("Compound Driven Gear (Z₂ = 36)",
                "Stage 1 driven gear. Reduces speed by 2:1 and drives compound countershaft.",
                $"Teeth: {z2} | Stage 1 Ratio: 2.0:1 | Intermediate Speed: {currentInputRPM / 2f:F0} RPM");
            inspZ2.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            GameObject gearZ3 = new GameObject("Compound_Pinion_Z3_18T");
            gearZ3.transform.SetParent(compoundRotor, false);
            gearZ3.transform.localPosition = new Vector3(0, 0, zOffsetStage2);
            MeshFilter mfZ3 = gearZ3.AddComponent<MeshFilter>();
            MeshRenderer mrZ3 = gearZ3.AddComponent<MeshRenderer>();
            mfZ3.mesh = GearMeshBuilder.BuildSpurGearMesh(z3, r3, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrZ3.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            gearZ3.AddComponent<MeshCollider>().sharedMesh = mfZ3.mesh;
            InspectableComponent inspZ3 = gearZ3.AddComponent<InspectableComponent>();
            inspZ3.SetMetadata("Compound Driver Pinion (Z₃ = 18)",
                "Stage 2 driver pinion. Keyed to same shaft as Z2, rotating at identical intermediate speed.",
                $"Teeth: {z3} | Keyed Compound Interface | Speed: {currentInputRPM / 2f:F0} RPM");
            inspZ3.SetExplosionVector(Vector3.back, 0.08f, 0.12f);

            GameObject compShaft = new GameObject("Compound_Shaft");
            compShaft.transform.SetParent(compoundRotor, false);
            MeshFilter mfCompShaft = compShaft.AddComponent<MeshFilter>();
            MeshRenderer mrCompShaft = compShaft.AddComponent<MeshRenderer>();
            mfCompShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrCompShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            compShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // 4. Output Shaft & Gear Z4
            GameObject outRotorGo = new GameObject("Output_Rotor");
            outRotorGo.transform.SetParent(transform, false);
            outRotorGo.transform.localPosition = posOutput;
            outputRotor = outRotorGo.transform;

            GameObject gearZ4 = new GameObject("Output_Gear_Z4_54T");
            gearZ4.transform.SetParent(outputRotor, false);
            gearZ4.transform.localPosition = new Vector3(0, 0, zOffsetStage2);
            MeshFilter mfZ4 = gearZ4.AddComponent<MeshFilter>();
            MeshRenderer mrZ4 = gearZ4.AddComponent<MeshRenderer>();
            mfZ4.mesh = GearMeshBuilder.BuildSpurGearMesh(z4, r4, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 6, spokeWidth: 0.009f);
            mrZ4.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            gearZ4.AddComponent<MeshCollider>().sharedMesh = mfZ4.mesh;
            InspectableComponent inspZ4 = gearZ4.AddComponent<InspectableComponent>();
            inspZ4.SetMetadata("Final Output Gear (Z₄ = 54)",
                "Stage 2 driven gear. Delivers 6.0× overall torque multiplication at 1/6th rotational speed.",
                $"Teeth: {z4} | Total Ratio: 6.0:1 | Output: {OutputRPM:F0} RPM | Torque Factor: {GearRatio:F1}×");
            inspZ4.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            GameObject outShaft = new GameObject("Output_Shaft");
            outShaft.transform.SetParent(outputRotor, false);
            MeshFilter mfOutShaft = outShaft.AddComponent<MeshFilter>();
            MeshRenderer mrOutShaft = outShaft.AddComponent<MeshRenderer>();
            mfOutShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrOutShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            outShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // Bearing Pillow Blocks for all 3 shafts
            CreateBearingPillows(posInput);
            CreateBearingPillows(posCompound);
            CreateBearingPillows(posOutput);

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
            // Input: omega_1
            currentInputAngle += inputAngularVelDegPerSec * deltaTime;
            currentInputAngle %= 360f;

            // Compound countershaft: omega_2 = -omega_1 * (Z1 / Z2)
            float ratio1 = (float)z1 / z2;
            currentCompoundAngle -= (inputAngularVelDegPerSec * ratio1) * deltaTime;
            currentCompoundAngle %= 360f;

            // Output shaft: omega_4 = -omega_2 * (Z3 / Z4) = +omega_1 * (Z1 * Z3) / (Z2 * Z4)
            float ratioTotal = ((float)z1 / z2) * ((float)z3 / z4);
            currentOutputAngle += (inputAngularVelDegPerSec * ratioTotal) * deltaTime;
            currentOutputAngle %= 360f;

            if (inputRotor != null) inputRotor.localRotation = Quaternion.Euler(0, 0, currentInputAngle);
            if (compoundRotor != null) compoundRotor.localRotation = Quaternion.Euler(0, 0, currentCompoundAngle);
            if (outputRotor != null) outputRotor.localRotation = Quaternion.Euler(0, 0, currentOutputAngle);
        }

        public override void ResetMechanism()
        {
            currentInputAngle = 0f;
            currentCompoundAngle = (360f / z2) * 0.5f;
            currentOutputAngle = 0f;

            if (inputRotor != null) inputRotor.localRotation = Quaternion.Euler(0, 0, currentInputAngle);
            if (compoundRotor != null) compoundRotor.localRotation = Quaternion.Euler(0, 0, currentCompoundAngle);
            if (outputRotor != null) outputRotor.localRotation = Quaternion.Euler(0, 0, currentOutputAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Compound Gear Train\n" +
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
            return "Two-stage compounded transmission achieving high speed reduction in a compact parallel footprint. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = (Z₂/Z₁) · (Z₄/Z₃) = (36/18) · (54/18) = 2.0 · 3.0 = 6.0:1\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


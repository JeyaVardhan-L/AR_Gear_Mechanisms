using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 09: Reverted Gear Train.
    /// Demonstrates coaxial input and output shaft alignment via parallel countershaft:
    /// - Stage 1: Input Pinion (Z1 = 20) meshes with Countershaft Gear (Z2 = 40). Center dist = 105 mm.
    /// - Stage 2: Countershaft Pinion (Z3 = 24) meshes with Coaxial Output Gear (Z4 = 36). Center dist = 105 mm.
    /// Constraint: r1 + r2 = r3 + r4 = 105.0 mm (Equal center distance allows coaxial transmission).
    /// Overall Ratio: i = (40/20) * (36/24) = 2.0 * 1.5 = 3.0 : 1.
    /// </summary>
    public class RevertedGearTrain : MechanismBase
    {
        [Header("Gear Parameters")]
        [SerializeField] private int z1 = 20;
        [SerializeField] private int z2 = 40;
        [SerializeField] private int z3 = 24;
        [SerializeField] private int z4 = 36;
        [SerializeField] private float moduleMeters = 0.0035f;
        [SerializeField] private float faceWidth = 0.014f;
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Rotors")]
        [SerializeField] private Transform inputRotor;
        [SerializeField] private Transform counterRotor;
        [SerializeField] private Transform outputRotor;

        private float currentInputAngle = 0f;
        private float currentCounterAngle = 0f;
        private float currentOutputAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => z1;
        public override int DrivenTeeth => z4;
        public int Z1 => z1;
        public int Z2 => z2;
        public int Z3 => z3;
        public int Z4 => z4;
        public override string MechanismName => "Reverted Gear Train";
        public override string CategoryTitle => "Coaxial Transmission Reducer";
        public override float GearRatio => ((float)z2 / z1) * ((float)z4 / z3); // 2.0 * 1.5 = 3.0
        public override float OutputRPM => currentInputRPM / GearRatio;         // 120 / 3 = 40 RPM

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r1 = (z1 * moduleMeters) * 0.5f; // 35.0 mm
            float r2 = (z2 * moduleMeters) * 0.5f; // 70.0 mm
            float r3 = (z3 * moduleMeters) * 0.5f; // 42.0 mm
            float r4 = (z4 * moduleMeters) * 0.5f; // 63.0 mm

            float centerDist = r1 + r2; // 105.0 mm (identical to r3 + r4!)

            Vector3 posCoaxial = Vector3.zero;
            Vector3 posCounter = new Vector3(0f, centerDist, 0f);

            float zOffsetStage1 = 0.016f;
            float zOffsetStage2 = -0.016f;

            // 1. Transmission Housing / Frame
            GameObject frameGo = new GameObject("Transmission_Housing");
            frameGo.transform.SetParent(transform, false);
            frameGo.transform.localPosition = new Vector3(0f, centerDist * 0.5f - 0.075f, 0f);
            MeshFilter mfFrame = frameGo.AddComponent<MeshFilter>();
            MeshRenderer mrFrame = frameGo.AddComponent<MeshRenderer>();
            mfFrame.mesh = GearMeshBuilder.BuildBasePlateMesh(0.18f, 0.16f, 0.024f);
            mrFrame.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            frameGo.AddComponent<BoxCollider>().size = new Vector3(0.18f, 0.024f, 0.16f);
            InspectableComponent inspFrame = frameGo.AddComponent<InspectableComponent>();
            inspFrame.SetMetadata("Coaxial Transmission Frame",
                "Rigid casing housing coaxial input and output bearings with parallel countershaft journal.",
                "Equal Center Distance: a₁ = a₂ = 105.0 mm | Cast Aluminum A380");
            inspFrame.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Input Shaft & Pinion Z1 (at coaxial axis x=0, y=0, z=+zOffsetStage1)
            GameObject inRotorGo = new GameObject("Input_Rotor");
            inRotorGo.transform.SetParent(transform, false);
            inRotorGo.transform.localPosition = posCoaxial;
            inputRotor = inRotorGo.transform;

            GameObject gearZ1 = new GameObject("Input_Pinion_Z1_20T");
            gearZ1.transform.SetParent(inputRotor, false);
            gearZ1.transform.localPosition = new Vector3(0, 0, zOffsetStage1);
            MeshFilter mfZ1 = gearZ1.AddComponent<MeshFilter>();
            MeshRenderer mrZ1 = gearZ1.AddComponent<MeshRenderer>();
            mfZ1.mesh = GearMeshBuilder.BuildSpurGearMesh(z1, r1, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrZ1.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            gearZ1.AddComponent<MeshCollider>().sharedMesh = mfZ1.mesh;
            InspectableComponent inspZ1 = gearZ1.AddComponent<InspectableComponent>();
            inspZ1.SetMetadata("Input Pinion (Z₁ = 20)",
                "Coaxial primary drive pinion. Meshes with countershaft gear Z2.",
                $"Teeth: {z1} | Pitch Dia: {r1*2000f:F1} mm | Input Speed: {currentInputRPM:F0} RPM");
            inspZ1.SetExplosionVector(Vector3.forward, 0.06f, 0.10f);

            GameObject inShaft = new GameObject("Input_Shaft");
            inShaft.transform.SetParent(inputRotor, false);
            inShaft.transform.localPosition = new Vector3(0, 0, 0.035f);
            MeshFilter mfInShaft = inShaft.AddComponent<MeshFilter>();
            MeshRenderer mrInShaft = inShaft.AddComponent<MeshRenderer>();
            mfInShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.06f, 24, false);
            mrInShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            inShaft.AddComponent<CapsuleCollider>().height = 0.06f;

            // 3. Countershaft Assembly (at y = centerDist)
            GameObject countRotorGo = new GameObject("Countershaft_Rotor");
            countRotorGo.transform.SetParent(transform, false);
            countRotorGo.transform.localPosition = posCounter;
            counterRotor = countRotorGo.transform;

            GameObject gearZ2 = new GameObject("Counter_Gear_Z2_40T");
            gearZ2.transform.SetParent(counterRotor, false);
            gearZ2.transform.localPosition = new Vector3(0, 0, zOffsetStage1);
            MeshFilter mfZ2 = gearZ2.AddComponent<MeshFilter>();
            MeshRenderer mrZ2 = gearZ2.AddComponent<MeshRenderer>();
            mfZ2.mesh = GearMeshBuilder.BuildSpurGearMesh(z2, r2, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 4, spokeWidth: 0.008f);
            mrZ2.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            gearZ2.AddComponent<MeshCollider>().sharedMesh = mfZ2.mesh;
            InspectableComponent inspZ2 = gearZ2.AddComponent<InspectableComponent>();
            inspZ2.SetMetadata("Countershaft Gear (Z₂ = 40, Spoked)",
                "Stage 1 driven gear. Reduces speed by 2:1 from input shaft.",
                $"Teeth: {z2} | Pitch Dia: {r2*2000f:F1} mm | Intermediate Speed: {currentInputRPM / 2f:F0} RPM");
            inspZ2.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            GameObject gearZ3 = new GameObject("Counter_Pinion_Z3_24T");
            gearZ3.transform.SetParent(counterRotor, false);
            gearZ3.transform.localPosition = new Vector3(0, 0, zOffsetStage2);
            MeshFilter mfZ3 = gearZ3.AddComponent<MeshFilter>();
            MeshRenderer mrZ3 = gearZ3.AddComponent<MeshRenderer>();
            mfZ3.mesh = GearMeshBuilder.BuildSpurGearMesh(z3, r3, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrZ3.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            gearZ3.AddComponent<MeshCollider>().sharedMesh = mfZ3.mesh;
            InspectableComponent inspZ3 = gearZ3.AddComponent<InspectableComponent>();
            inspZ3.SetMetadata("Countershaft Pinion (Z₃ = 24)",
                "Stage 2 driver pinion keyed to countershaft. Meshes with coaxial output gear Z4.",
                $"Teeth: {z3} | Pitch Dia: {r3*2000f:F1} mm | Speed: {currentInputRPM / 2f:F0} RPM");
            inspZ3.SetExplosionVector(Vector3.back, 0.08f, 0.12f);

            GameObject countShaft = new GameObject("Counter_Shaft");
            countShaft.transform.SetParent(counterRotor, false);
            MeshFilter mfCountShaft = countShaft.AddComponent<MeshFilter>();
            MeshRenderer mrCountShaft = countShaft.AddComponent<MeshRenderer>();
            mfCountShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.09f, 24, false);
            mrCountShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            countShaft.AddComponent<CapsuleCollider>().height = 0.09f;

            // 4. Output Shaft & Gear Z4 (coaxial at x=0, y=0, z=-zOffsetStage2)
            GameObject outRotorGo = new GameObject("Output_Rotor");
            outRotorGo.transform.SetParent(transform, false);
            outRotorGo.transform.localPosition = posCoaxial;
            outputRotor = outRotorGo.transform;

            GameObject gearZ4 = new GameObject("Output_Gear_Z4_36T");
            gearZ4.transform.SetParent(outputRotor, false);
            gearZ4.transform.localPosition = new Vector3(0, 0, zOffsetStage2);
            MeshFilter mfZ4 = gearZ4.AddComponent<MeshFilter>();
            MeshRenderer mrZ4 = gearZ4.AddComponent<MeshRenderer>();
            mfZ4.mesh = GearMeshBuilder.BuildSpurGearMesh(z4, r4, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 4, spokeWidth: 0.008f);
            mrZ4.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            gearZ4.AddComponent<MeshCollider>().sharedMesh = mfZ4.mesh;
            InspectableComponent inspZ4 = gearZ4.AddComponent<InspectableComponent>();
            inspZ4.SetMetadata("Coaxial Output Gear (Z₄ = 36)",
                "Delivers 3.0× overall torque multiplication. Coaxial with input shaft, rotating in identical direction.",
                $"Teeth: {z4} | Total Ratio: 3.0:1 | Output Speed: {OutputRPM:F0} RPM | Coaxial Alignment: 0.0 mm radial offset");
            inspZ4.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            GameObject outShaft = new GameObject("Output_Shaft");
            outShaft.transform.SetParent(outputRotor, false);
            outShaft.transform.localPosition = new Vector3(0, 0, -0.035f);
            MeshFilter mfOutShaft = outShaft.AddComponent<MeshFilter>();
            MeshRenderer mrOutShaft = outShaft.AddComponent<MeshRenderer>();
            mfOutShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.06f, 24, false);
            mrOutShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            outShaft.AddComponent<CapsuleCollider>().height = 0.06f;

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
            // Input: omega_1
            currentInputAngle += inputAngularVelDegPerSec * deltaTime;
            currentInputAngle %= 360f;

            // Countershaft: omega_2 = -omega_1 * (Z1 / Z2)
            float ratio1 = (float)z1 / z2;
            currentCounterAngle -= (inputAngularVelDegPerSec * ratio1) * deltaTime;
            currentCounterAngle %= 360f;

            // Output: omega_4 = -omega_2 * (Z3 / Z4) = +omega_1 * (Z1 * Z3) / (Z2 * Z4)
            float ratioTotal = ((float)z1 / z2) * ((float)z3 / z4);
            currentOutputAngle += (inputAngularVelDegPerSec * ratioTotal) * deltaTime;
            currentOutputAngle %= 360f;

            if (inputRotor != null) inputRotor.localRotation = Quaternion.Euler(0, 0, currentInputAngle);
            if (counterRotor != null) counterRotor.localRotation = Quaternion.Euler(0, 0, currentCounterAngle);
            if (outputRotor != null) outputRotor.localRotation = Quaternion.Euler(0, 0, currentOutputAngle);
        }

        public override void ResetMechanism()
        {
            currentInputAngle = 0f;
            currentCounterAngle = (360f / z2) * 0.5f;
            currentOutputAngle = 0f;

            if (inputRotor != null) inputRotor.localRotation = Quaternion.Euler(0, 0, currentInputAngle);
            if (counterRotor != null) counterRotor.localRotation = Quaternion.Euler(0, 0, currentCounterAngle);
            if (outputRotor != null) outputRotor.localRotation = Quaternion.Euler(0, 0, currentOutputAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Reverted Gear Train\n" +
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
            return "Coaxial input and output shafts with equal center distance condition: a₁ = a₂ = m·(Z₁+Z₂)/2 = m·(Z₃+Z₄)/2 = 105 mm. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = (Z₂/Z₁) · (Z₄/Z₃) = (40/20) · (36/24) = 2.0 · 1.5 = 3.0:1\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


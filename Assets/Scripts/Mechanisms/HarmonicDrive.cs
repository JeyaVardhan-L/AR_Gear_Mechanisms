using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 14: Harmonic Drive (Strain Wave Gearing).
    /// Demonstrates high-ratio zero-backlash robotic actuator kinematics:
    /// - Wave Generator: Elliptical cam core with ball bearings on high-speed input shaft.
    /// - Flexspline: Thin flexible cup with external teeth (Zf = 48).
    /// - Circular Spline: Rigid outer ring with internal teeth (Zc = 50) grounded to frame.
    /// Kinematic Relationship:
    /// Teeth Difference: delta_Z = Zc - Zf = 50 - 48 = 2 teeth.
    /// Gear Reduction Ratio: i = Zf / (Zc - Zf) = 48 / 2 = 24.0 : 1.
    /// Direction: Output flexspline rotates in OPPOSITE direction to wave generator:
    /// omega_flexspline = -omega_wave_generator / 24.
    /// </summary>
    public class HarmonicDrive : MechanismBase
    {
        [Header("Harmonic Drive Parameters")]
        [SerializeField] private int flexsplineTeeth = 48;
        [SerializeField] private int circularSplineTeeth = 50;
        [SerializeField] private float moduleMeters = 0.0025f; // 2.5 mm module
        [SerializeField] private float toothFaceWidth = 0.016f;
        [SerializeField] private float cupLength = 0.065f;
        [SerializeField] private float wallThickness = 0.0022f; // Thin elastic cup wall
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Rotors")]
        [SerializeField] private Transform waveGeneratorRotor;
        [SerializeField] private Transform flexsplineRotor;
        [SerializeField] private Transform circularSplineStatic;

        private float currentWaveAngle = 0f;
        private float currentFlexAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => flexsplineTeeth;
        public override int DrivenTeeth => circularSplineTeeth;
        public int Zf => flexsplineTeeth;
        public int Zc => circularSplineTeeth;
        public override string MechanismName => "Harmonic Drive";
        public override string CategoryTitle => "Zero-Backlash Strain Wave Gear";
        public override float GearRatio => (float)flexsplineTeeth / (circularSplineTeeth - flexsplineTeeth); // 48 / 2 = 24.0
        public override float OutputRPM => currentInputRPM / GearRatio; // 120 / 24 = 5.0 RPM (Counter-rotating)

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float rFlex = (flexsplineTeeth * moduleMeters) * 0.5f;     // 60.0 mm
            float rCirc = (circularSplineTeeth * moduleMeters) * 0.5f; // 62.5 mm
            float circOuterR = rCirc + 0.022f;                         // 84.5 mm

            float majorRadius = rFlex + 0.0025f; // 62.5 mm (engages circular spline at apex)
            float minorRadius = rFlex - 0.0025f; // 57.5 mm (clears circular spline completely)

            // 1. Actuator Machine Bed & Housing
            GameObject bedGo = new GameObject("Actuator_Housing_Bed");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0f, -0.095f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.24f, 0.20f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.024f, 0.20f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Robotic Actuator Base & Flange",
                "High-precision robotic joint flange mount providing coaxial support for input motor and output harmonic drive.",
                "Material: 7075-T6 Aerospace Aluminum | Hard Anodized");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Rigid Circular Spline (Zc = 50, Grounded to Frame)
            GameObject circGo = new GameObject("Circular_Spline_Static");
            circGo.transform.SetParent(transform, false);
            circGo.transform.localPosition = new Vector3(0f, 0f, 0.012f);
            circularSplineStatic = circGo.transform;

            GameObject circMesh = new GameObject("Circular_Spline_50T");
            circMesh.transform.SetParent(circularSplineStatic, false);
            MeshFilter mfCirc = circMesh.AddComponent<MeshFilter>();
            MeshRenderer mrCirc = circMesh.AddComponent<MeshRenderer>();
            mfCirc.mesh = GearMeshBuilder.BuildInternalRingGearMesh(
                circularSplineTeeth, rCirc, moduleMeters, moduleMeters * 1.25f, circOuterR, toothFaceWidth * 1.2f);
            mrCirc.material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.5f); // Dark Slate
            circMesh.AddComponent<MeshCollider>().sharedMesh = mfCirc.mesh;
            InspectableComponent inspCirc = circMesh.AddComponent<InspectableComponent>();
            inspCirc.SetMetadata("Rigid Circular Spline (Z_c = 50)",
                "Non-rotating outer ring with internal teeth. Provides fixed reaction frame for strain wave engagement.",
                $"Internal Teeth: {circularSplineTeeth} | Module: {moduleMeters*1000f:F1} mm | Static Frame Reference");
            inspCirc.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 3. High-Speed Wave Generator (Elliptical Cam, inside flexspline mouth)
            GameObject waveGo = new GameObject("Wave_Generator_Rotor");
            waveGo.transform.SetParent(transform, false);
            waveGo.transform.localPosition = new Vector3(0f, 0f, 0.012f);
            waveGeneratorRotor = waveGo.transform;

            GameObject waveCam = new GameObject("Wave_Generator_Cam");
            waveCam.transform.SetParent(waveGeneratorRotor, false);
            MeshFilter mfWave = waveCam.AddComponent<MeshFilter>();
            MeshRenderer mrWave = waveCam.AddComponent<MeshRenderer>();
            mfWave.mesh = GearMeshBuilder.BuildHarmonicDriveWaveGeneratorMesh(
                majorRadius - wallThickness - 0.001f, minorRadius - wallThickness - 0.001f, toothFaceWidth, boreRadius);
            mrWave.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            waveCam.AddComponent<MeshCollider>().sharedMesh = mfWave.mesh;
            InspectableComponent inspWave = waveCam.AddComponent<InspectableComponent>();
            inspWave.SetMetadata("Elliptical Wave Generator Cam",
                "High-speed elliptical plug surrounded by flexible ball bearings. Imposes continuous radial traveling wave into flexspline.",
                $"Speed: {currentInputRPM:F0} RPM | Major Dia: {(majorRadius*2000f):F1} mm | Minor Dia: {(minorRadius*2000f):F1} mm");
            inspWave.SetExplosionVector(Vector3.forward, 0.12f, 0.16f);

            GameObject inShaft = new GameObject("Motor_Input_Shaft");
            inShaft.transform.SetParent(waveGeneratorRotor, false);
            inShaft.transform.localPosition = new Vector3(0f, 0f, 0.035f);
            MeshFilter mfInShaft = inShaft.AddComponent<MeshFilter>();
            MeshRenderer mrInShaft = inShaft.AddComponent<MeshRenderer>();
            mfInShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.06f, 24, false);
            mrInShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 4. Elastic Flexspline Cup (Zf = 48, Output Member)
            GameObject flexGo = new GameObject("Flexspline_Rotor");
            flexGo.transform.SetParent(transform, false);
            flexGo.transform.localPosition = new Vector3(0f, 0f, 0f);
            flexsplineRotor = flexGo.transform;

            GameObject flexCup = new GameObject("Flexspline_Cup_48T");
            flexCup.transform.SetParent(flexsplineRotor, false);
            MeshFilter mfFlex = flexCup.AddComponent<MeshFilter>();
            MeshRenderer mrFlex = flexCup.AddComponent<MeshRenderer>();
            mfFlex.mesh = GearMeshBuilder.BuildHarmonicFlexsplineMesh(
                flexsplineTeeth, rFlex, moduleMeters, moduleMeters * 1.25f, toothFaceWidth, cupLength, wallThickness, 0.012f);
            mrFlex.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            flexCup.AddComponent<MeshCollider>().sharedMesh = mfFlex.mesh;
            InspectableComponent inspFlex = flexCup.AddComponent<InspectableComponent>();
            inspFlex.SetMetadata("Thin-Walled Elastic Flexspline (Z_f = 48)",
                "Flexible steel cup with external teeth. Deforms elliptically to mesh at major axis lobes and disengage at minor axis.",
                $"External Teeth: {flexsplineTeeth} | Reduction: 24:1 | Output: {OutputRPM:F1} RPM (CCW) | Zero Backlash");
            inspFlex.SetExplosionVector(Vector3.back, 0.08f, 0.12f);

            GameObject outHub = new GameObject("Robotic_Output_Hub");
            outHub.transform.SetParent(flexsplineRotor, false);
            outHub.transform.localPosition = new Vector3(0f, 0f, -cupLength - 0.015f);
            MeshFilter mfOutHub = outHub.AddComponent<MeshFilter>();
            MeshRenderer mrOutHub = outHub.AddComponent<MeshRenderer>();
            mfOutHub.mesh = GearMeshBuilder.BuildCylinderMesh(0.024f, 0.030f, 32, false);
            mrOutHub.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            outHub.AddComponent<MeshCollider>().sharedMesh = mfOutHub.mesh;
            InspectableComponent inspHub = outHub.AddComponent<InspectableComponent>();
            inspHub.SetMetadata("Robotic Joint Output Flange",
                "High-torsional-stiffness output drive flange delivering 24.0× multiplied motor torque.",
                $"Speed: {OutputRPM:F1} RPM | Torque Multiplier: {GearRatio:F0}× | Zero Backlash");
            inspHub.SetExplosionVector(Vector3.back, 0.12f, 0.16f);

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
            // High-speed wave generator rotation: omega_in
            currentWaveAngle += inputAngularVelDegPerSec * deltaTime;
            currentWaveAngle %= 360f;

            // Low-speed flexspline output:
            // omega_out = -omega_in / i = -omega_in / 24.0 (Counter-rotation!)
            float flexVel = -inputAngularVelDegPerSec / GearRatio;
            currentFlexAngle += flexVel * deltaTime;
            currentFlexAngle %= 360f;

            if (waveGeneratorRotor != null) waveGeneratorRotor.localRotation = Quaternion.Euler(0, 0, currentWaveAngle);
            if (flexsplineRotor != null) flexsplineRotor.localRotation = Quaternion.Euler(0, 0, currentFlexAngle);
        }

        public override void ResetMechanism()
        {
            currentWaveAngle = 0f;
            currentFlexAngle = 0f;

            if (waveGeneratorRotor != null) waveGeneratorRotor.localRotation = Quaternion.Euler(0, 0, currentWaveAngle);
            if (flexsplineRotor != null) flexsplineRotor.localRotation = Quaternion.Euler(0, 0, currentFlexAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Harmonic Drive\n" +
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
            return "Strain wave gearing with elliptical wave generator and elastic flexspline achieving zero backlash and huge reduction. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = Z_flex / (Z_circular - Z_flex) = 48 / (50 - 48) = 24.0:1\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


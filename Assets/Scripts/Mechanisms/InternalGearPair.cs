using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 10: Internal Gear Pair.
    /// Demonstrates an external spur pinion meshing internally inside an annular ring gear:
    /// - External Pinion (Z1 = 20, Electric Blue, mounted at x = -70 mm)
    /// - Internal Ring Gear (Z2 = 60, Spoked Outer Housing, mounted at x = 0 mm)
    /// Key kinematic principles:
    /// 1. Both gears rotate in the SAME direction (unlike external mesh which counter-rotates).
    /// 2. Center distance: a = (Z2 - Z1) * m / 2 = (60 - 20) * 3.5 / 2 = 70.0 mm.
    /// 3. Speed ratio: i = Z2 / Z1 = 60 / 20 = 3.0 : 1.
    /// </summary>
    public class InternalGearPair : MechanismBase
    {
        [Header("Gear Parameters")]
        [SerializeField] private int pinionTeeth = 20;
        [SerializeField] private int ringTeeth = 60;
        [SerializeField] private float moduleMeters = 0.0035f;
        [SerializeField] private float faceWidth = 0.018f;
        [SerializeField] private float boreRadius = 0.006f;

        [Header("Rotors")]
        [SerializeField] private Transform pinionRotor;
        [SerializeField] private Transform ringRotor;

        private float currentPinionAngle = 0f;
        private float currentRingAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => pinionTeeth;
        public override int DrivenTeeth => ringTeeth;
        public override string MechanismName => "Internal Gear Pair";
        public override string CategoryTitle => "Annular Ring Transmission";
        public override float GearRatio => (float)ringTeeth / pinionTeeth; // 60 / 20 = 3.0
        public override float OutputRPM => currentInputRPM / GearRatio;    // 120 / 3 = 40 RPM (Same direction!)

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float rPinion = (pinionTeeth * moduleMeters) * 0.5f; // 35.0 mm
            float rRing = (ringTeeth * moduleMeters) * 0.5f;     // 105.0 mm
            float ringOuterR = rRing + 0.022f;                   // 127.0 mm

            float centerDist = rRing - rPinion; // 70.0 mm

            Vector3 posPinion = new Vector3(-centerDist, 0f, 0f);
            Vector3 posRing = Vector3.zero;

            // 1. Machine Bed
            GameObject bedGo = new GameObject("Machine_Bed");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(-centerDist * 0.5f, -0.14f, 0f);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.32f, 0.16f, 0.024f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.32f, 0.024f, 0.16f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Internal Drive Foundation",
                "Machine bed providing rigid cantilevered support for internal pinion shaft and ring bearing race.",
                "Material: GG-25 Gray Cast Iron | Anodized");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Internal Ring Gear (Z2 = 60)
            GameObject ringGo = new GameObject("Ring_Rotor");
            ringGo.transform.SetParent(transform, false);
            ringGo.transform.localPosition = posRing;
            ringRotor = ringGo.transform;

            GameObject ringMeshGo = new GameObject("Ring_Gear_60T");
            ringMeshGo.transform.SetParent(ringRotor, false);
            MeshFilter mfRing = ringMeshGo.AddComponent<MeshFilter>();
            MeshRenderer mrRing = ringMeshGo.AddComponent<MeshRenderer>();
            mfRing.mesh = GearMeshBuilder.BuildInternalRingGearMesh(
                ringTeeth, rRing, moduleMeters, moduleMeters * 1.25f, ringOuterR, faceWidth);
            mrRing.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            ringMeshGo.AddComponent<MeshCollider>().sharedMesh = mfRing.mesh;
            InspectableComponent inspRing = ringMeshGo.AddComponent<InspectableComponent>();
            inspRing.SetMetadata("Internal Annular Ring Gear (Z₂ = 60)",
                "Annular gear with internal involute teeth. Rotates in the SAME direction as driver pinion.",
                $"Teeth: {ringTeeth} | Ratio: 3.0:1 | Pitch Dia: {rRing*2000f:F1} mm | Output: {OutputRPM:F0} RPM");
            inspRing.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // Outer Bearing Support Ring for the ring gear
            GameObject bearingRing = new GameObject("Ring_Support_Bearing");
            bearingRing.transform.SetParent(transform, false);
            bearingRing.transform.localPosition = new Vector3(0, 0, -0.015f);
            MeshFilter mfBrg = bearingRing.AddComponent<MeshFilter>();
            MeshRenderer mrBrg = bearingRing.AddComponent<MeshRenderer>();
            mfBrg.mesh = GearMeshBuilder.BuildBearingRingMesh(ringOuterR + 0.012f, ringOuterR, 0.014f);
            mrBrg.material = CreateMaterial(new Color(0.7f, 0.72f, 0.76f), 0.9f, 0.85f);

            // 3. Pinion Rotor (Z1 = 20) at x = -centerDist
            GameObject pinionGo = new GameObject("Pinion_Rotor");
            pinionGo.transform.SetParent(transform, false);
            pinionGo.transform.localPosition = posPinion;
            pinionRotor = pinionGo.transform;

            GameObject pinionGear = new GameObject("Pinion_Gear_20T");
            pinionGear.transform.SetParent(pinionRotor, false);
            MeshFilter mfPinion = pinionGear.AddComponent<MeshFilter>();
            MeshRenderer mrPinion = pinionGear.AddComponent<MeshRenderer>();
            mfPinion.mesh = GearMeshBuilder.BuildSpurGearMesh(pinionTeeth, rPinion, moduleMeters, moduleMeters * 1.25f, faceWidth, boreRadius, spokeCount: 0);
            mrPinion.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            pinionGear.AddComponent<MeshCollider>().sharedMesh = mfPinion.mesh;
            InspectableComponent inspPinion = pinionGear.AddComponent<InspectableComponent>();
            inspPinion.SetMetadata("External Drive Pinion (Z₁ = 20)",
                "External spur pinion meshing internally along the pitch contact line.",
                $"Teeth: {pinionTeeth} | Pitch Dia: {rPinion*2000f:F1} mm | Center Offset: {centerDist*1000f:F1} mm");
            inspPinion.SetExplosionVector(Vector3.back, 0.08f, 0.12f);

            GameObject pinShaft = new GameObject("Pinion_Shaft");
            pinShaft.transform.SetParent(pinionRotor, false);
            pinShaft.transform.localPosition = new Vector3(0, 0, -0.025f);
            MeshFilter mfPinShaft = pinShaft.AddComponent<MeshFilter>();
            MeshRenderer mrPinShaft = pinShaft.AddComponent<MeshRenderer>();
            mfPinShaft.mesh = GearMeshBuilder.BuildCylinderMesh(boreRadius, 0.08f, 24, false);
            mrPinShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            pinShaft.AddComponent<CapsuleCollider>().height = 0.08f;

            // Pillow Block mount for pinion shaft
            GameObject pb = new GameObject("Pinion_PillowBlock");
            pb.transform.SetParent(transform, false);
            pb.transform.localPosition = new Vector3(posPinion.x, -0.075f, -0.045f);
            MeshFilter mfPb = pb.AddComponent<MeshFilter>();
            MeshRenderer mrPb = pb.AddComponent<MeshRenderer>();
            mfPb.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.026f, 0.065f, 0.018f, boreRadius);
            mrPb.material = CreateMaterial(new Color(0.24f, 0.26f, 0.30f), 0.5f, 0.3f);

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
            // Pinion: omega_1
            currentPinionAngle += inputAngularVelDegPerSec * deltaTime;
            currentPinionAngle %= 360f;

            // Internal Ring Gear: omega_2 = +omega_1 * (Z1 / Z2) -> SAME DIRECTION!
            float ratio = (float)pinionTeeth / ringTeeth;
            currentRingAngle += (inputAngularVelDegPerSec * ratio) * deltaTime;
            currentRingAngle %= 360f;

            if (pinionRotor != null) pinionRotor.localRotation = Quaternion.Euler(0, 0, currentPinionAngle);
            if (ringRotor != null) ringRotor.localRotation = Quaternion.Euler(0, 0, currentRingAngle);
        }

        public override void ResetMechanism()
        {
            currentPinionAngle = 0f;
            currentRingAngle = 0f;

            if (pinionRotor != null) pinionRotor.localRotation = Quaternion.Euler(0, 0, currentPinionAngle);
            if (ringRotor != null) ringRotor.localRotation = Quaternion.Euler(0, 0, currentRingAngle);
        }
    
        public override string GetCalculationSummary()
        {
            return $"• Mechanism: Internal Gear Pair\n" +
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
            return "Internal ring tooth engagement produces highly compact parallel transmission with identical direction of rotation. " +
                   $"Velocity ratio is {GearRatio:F2}:1 under an efficiency of {(Efficiency * 100f):F0}%.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i = Z₂ / Z₁ = 60 / 20 = 3.0:1 (Same direction)\n" +
                   $"Output Speed: n₂ = {OutputRPM:F1} RPM\n" +
                   $"Output Torque: T₂ = {OutputTorqueNm:F1} N·m";
        }
}
}


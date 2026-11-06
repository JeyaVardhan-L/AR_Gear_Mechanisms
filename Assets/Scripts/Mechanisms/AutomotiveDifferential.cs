using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Mechanism 13: Automotive Differential.
    /// Demonstrates an open differential mechanism with variable left/right axle speeds:
    /// - Drive Pinion (Zp = 12, Hypoid/Bevel input along Z)
    /// - Ring / Crown Bevel Gear (Zr = 36, Final Drive 3:1)
    /// - Differential Carrier Housing with internal cross-pin
    /// - Dual Spider Bevel Pinions (Zs = 14) free to rotate on carrier cross-pin
    /// - Left & Right Side Bevel Gears (Zw = 20) splined to output half-shafts
    /// Key Willis Kinematics:
    /// omega_carrier = (omega_Left + omega_Right) / 2
    /// In straight-line drive: omega_L = omega_R = omega_carrier (spiders do not rotate on pin).
    /// During cornering: Inner wheel slows down, outer wheel speeds up proportionally,
    /// and spider pinions spin on the cross-pin to satisfy the epicyclic constraint.
    /// </summary>
    public class AutomotiveDifferential : MechanismBase
    {
        [Header("Differential Gear Parameters")]
        [SerializeField] private int pinionTeeth = 12;
        [SerializeField] private int ringTeeth = 36;
        [SerializeField] private int spiderTeeth = 14;
        [SerializeField] private int sideGearTeeth = 20;
        [SerializeField] private float moduleMeters = 0.0035f;

        [Header("Cornering / Turning State")]
        [Range(-0.8f, 0.8f)]
        [SerializeField] private float turnBias = 0.0f; // -0.5 = Sharp Left, 0 = Straight, +0.5 = Sharp Right

        [Header("Rotors")]
        [SerializeField] private Transform drivePinionRotor;
        [SerializeField] private Transform carrierRotor;
        [SerializeField] private Transform leftAxleRotor;
        [SerializeField] private Transform rightAxleRotor;
        [SerializeField] private Transform topSpiderRotor;
        [SerializeField] private Transform bottomSpiderRotor;

        private float currentPinionAngle = 0f;
        private float currentCarrierAngle = 0f;
        private float currentLeftAngle = 0f;
        private float currentRightAngle = 0f;
        private float currentSpiderAngle = 0f;
        private bool isAssemblyBuilt = false;

        public override int DriverTeeth => pinionTeeth;
        public override int DrivenTeeth => ringTeeth;
        public int SpiderTeeth => spiderTeeth;
        public int SideGearTeeth => sideGearTeeth;
        public float TurnBias
        {
            get => turnBias;
            set
            {
                turnBias = Mathf.Clamp(value, -0.8f, 0.8f);
                NotifyKinematicsUpdated();
            }
        }

        public override string MechanismName => "Automotive Differential";
        public override string CategoryTitle => "Epicyclic Torque-Vectoring Axle";
        public override float GearRatio => (float)ringTeeth / pinionTeeth; // 36 / 12 = 3.0 (Crown reduction)
        public override float OutputRPM => currentInputRPM / GearRatio;    // Mean carrier speed

        public float LeftAxleRPM => OutputRPM * (1f + turnBias);
        public float RightAxleRPM => OutputRPM * (1f - turnBias);
        public float CarrierRPM => OutputRPM;

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float rPinion = (pinionTeeth * moduleMeters) * 0.5f;   // 21.0 mm
            float rRing = (ringTeeth * moduleMeters) * 0.5f;       // 63.0 mm
            float rSpider = (spiderTeeth * moduleMeters) * 0.5f;   // 24.5 mm
            float rSide = (sideGearTeeth * moduleMeters) * 0.5f;   // 35.0 mm

            float spiderPitchCone = Mathf.Atan2(rSpider, rSide);
            float sidePitchCone = Mathf.Atan2(rSide, rSpider);

            // 1. Differential Axle Housing / Base
            GameObject baseGo = new GameObject("Axle_Carrier_Base");
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localPosition = new Vector3(0f, -0.085f, 0f);
            MeshFilter mfBase = baseGo.AddComponent<MeshFilter>();
            MeshRenderer mrBase = baseGo.AddComponent<MeshRenderer>();
            mfBase.mesh = GearMeshBuilder.BuildBasePlateMesh(0.28f, 0.20f, 0.024f);
            mrBase.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.4f);
            baseGo.AddComponent<BoxCollider>().size = new Vector3(0.28f, 0.024f, 0.20f);
            InspectableComponent inspBase = baseGo.AddComponent<InspectableComponent>();
            inspBase.SetMetadata("Rear Axle Housing & Carrier Mount",
                "High-strength nodular iron axle center section supporting pinion bearings and differential carrier journals.",
                "Material: EN-GJS-450 Ductile Iron | Banjo Type Casing");
            inspBase.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Input Drive Pinion (along Z-axis, meshing with Crown Gear at X = -0.04m)
            GameObject pinRotorGo = new GameObject("Drive_Pinion_Rotor");
            pinRotorGo.transform.SetParent(transform, false);
            pinRotorGo.transform.localPosition = new Vector3(-0.04f, 0f, 0.075f);
            drivePinionRotor = pinRotorGo.transform;

            GameObject pinGear = new GameObject("Drive_Pinion_12T");
            pinGear.transform.SetParent(drivePinionRotor, false);
            pinGear.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            MeshFilter mfPin = pinGear.AddComponent<MeshFilter>();
            MeshRenderer mrPin = pinGear.AddComponent<MeshRenderer>();
            mfPin.mesh = GearMeshBuilder.BuildBevelGearMesh(
                pinionTeeth, rPinion, Mathf.Atan2(rPinion, rRing), 0.016f, moduleMeters, moduleMeters * 1.25f, 0.006f);
            mrPin.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            pinGear.AddComponent<MeshCollider>().sharedMesh = mfPin.mesh;
            InspectableComponent inspPin = pinGear.AddComponent<InspectableComponent>();
            inspPin.SetMetadata("Hypoid Drive Pinion (Z_p = 12)",
                "Driveshaft input pinion delivering engine torque to crown ring gear at 90° angle.",
                $"Teeth: {pinionTeeth} | Final Drive Ratio: 3.0:1 | Speed: {currentInputRPM:F0} RPM");
            inspPin.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            GameObject pinShaft = new GameObject("Pinion_Shaft");
            pinShaft.transform.SetParent(drivePinionRotor, false);
            MeshFilter mfPinShaft = pinShaft.AddComponent<MeshFilter>();
            MeshRenderer mrPinShaft = pinShaft.AddComponent<MeshRenderer>();
            mfPinShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.006f, 0.08f, 24, false);
            mrPinShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 3. Differential Carrier Assembly (Rotates around X-axis at origin)
            GameObject carrierGo = new GameObject("Carrier_Rotor");
            carrierGo.transform.SetParent(transform, false);
            carrierGo.transform.localPosition = Vector3.zero;
            carrierRotor = carrierGo.transform;

            // Crown / Ring Gear (mounted on carrier flange at X = -0.04m)
            GameObject crownGear = new GameObject("Crown_Ring_Gear_36T");
            crownGear.transform.SetParent(carrierRotor, false);
            crownGear.transform.localPosition = new Vector3(-0.04f, 0f, 0f);
            crownGear.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            MeshFilter mfCrown = crownGear.AddComponent<MeshFilter>();
            MeshRenderer mrCrown = crownGear.AddComponent<MeshRenderer>();
            mfCrown.mesh = GearMeshBuilder.BuildBevelGearMesh(
                ringTeeth, rRing, Mathf.Atan2(rRing, rPinion), 0.016f, moduleMeters, moduleMeters * 1.25f, 0.038f, spokeCount: 6);
            mrCrown.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crownGear.AddComponent<MeshCollider>().sharedMesh = mfCrown.mesh;
            InspectableComponent inspCrown = crownGear.AddComponent<InspectableComponent>();
            inspCrown.SetMetadata("Crown Ring Bevel Gear (Z_r = 36)",
                "Bolted to differential carrier flange. Provides primary 3.0:1 axle reduction.",
                $"Teeth: {ringTeeth} | Pitch Dia: {rRing*2000f:F1} mm | Carrier Speed: {OutputRPM:F0} RPM");
            inspCrown.SetExplosionVector(Vector3.left, 0.08f, 0.12f);

            // Carrier Housing with viewing windows
            GameObject carrierHousing = new GameObject("Carrier_Housing");
            carrierHousing.transform.SetParent(carrierRotor, false);
            MeshFilter mfHousing = carrierHousing.AddComponent<MeshFilter>();
            MeshRenderer mrHousing = carrierHousing.AddComponent<MeshRenderer>();
            mfHousing.mesh = GearMeshBuilder.BuildDifferentialCarrierMesh(
                0.052f, 0.076f, 0.018f, 0.035f, 0.062f, 0.008f, 0.006f);
            mrHousing.material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.5f);
            carrierHousing.AddComponent<MeshCollider>().sharedMesh = mfHousing.mesh;
            InspectableComponent inspHousing = carrierHousing.AddComponent<InspectableComponent>();
            inspHousing.SetMetadata("Differential Carrier Case",
                "Houses the spider cross-pin and supports both side gears. Dual apertures permit internal inspection.",
                "Material: High-Toughness Cast Steel | Balanced for High RPM");
            inspHousing.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

            // Spider Cross-Pin (along Y axis inside carrier)
            GameObject spiderPin = new GameObject("Spider_Cross_Pin");
            spiderPin.transform.SetParent(carrierRotor, false);
            MeshFilter mfSpiderPin = spiderPin.AddComponent<MeshFilter>();
            MeshRenderer mrSpiderPin = spiderPin.AddComponent<MeshRenderer>();
            mfSpiderPin.mesh = GearMeshBuilder.BuildCylinderMesh(0.005f, 0.088f, 24, false);
            mrSpiderPin.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            spiderPin.transform.localRotation = Quaternion.Euler(90f, 0, 0);

            // Top Spider Bevel Pinion (at Y = +0.032m on spider pin)
            GameObject topSpider = new GameObject("Spider_Top_Rotor");
            topSpider.transform.SetParent(carrierRotor, false);
            topSpider.transform.localPosition = new Vector3(0f, 0.032f, 0f);
            topSpiderRotor = topSpider.transform;

            GameObject topSpiderMesh = new GameObject("Spider_Gear_Top_14T");
            topSpiderMesh.transform.SetParent(topSpiderRotor, false);
            topSpiderMesh.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            MeshFilter mfTopSpider = topSpiderMesh.AddComponent<MeshFilter>();
            MeshRenderer mrTopSpider = topSpiderMesh.AddComponent<MeshRenderer>();
            mfTopSpider.mesh = GearMeshBuilder.BuildBevelGearMesh(
                spiderTeeth, rSpider, spiderPitchCone, 0.012f, moduleMeters, moduleMeters * 1.25f, 0.005f);
            mrTopSpider.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson
            topSpiderMesh.AddComponent<MeshCollider>().sharedMesh = mfTopSpider.mesh;
            InspectableComponent inspTopSpider = topSpiderMesh.AddComponent<InspectableComponent>();
            inspTopSpider.SetMetadata("Upper Spider Pinion Gear (Z_s = 14)",
                "Rotates freely on cross-pin only during cornering to differentiate left and right wheel speeds.",
                $"Teeth: {spiderTeeth} | Straight Speed: 0.0 RPM (relative to carrier) | Active in turns");
            inspTopSpider.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // Bottom Spider Bevel Pinion (at Y = -0.032m on spider pin)
            GameObject botSpider = new GameObject("Spider_Bottom_Rotor");
            botSpider.transform.SetParent(carrierRotor, false);
            botSpider.transform.localPosition = new Vector3(0f, -0.032f, 0f);
            bottomSpiderRotor = botSpider.transform;

            GameObject botSpiderMesh = new GameObject("Spider_Gear_Bottom_14T");
            botSpiderMesh.transform.SetParent(bottomSpiderRotor, false);
            botSpiderMesh.transform.localRotation = Quaternion.Euler(-90f, 0, 0);
            MeshFilter mfBotSpider = botSpiderMesh.AddComponent<MeshFilter>();
            MeshRenderer mrBotSpider = botSpiderMesh.AddComponent<MeshRenderer>();
            mfBotSpider.mesh = GearMeshBuilder.BuildBevelGearMesh(
                spiderTeeth, rSpider, spiderPitchCone, 0.012f, moduleMeters, moduleMeters * 1.25f, 0.005f);
            mrBotSpider.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson
            botSpiderMesh.AddComponent<MeshCollider>().sharedMesh = mfBotSpider.mesh;

            // 4. Left Axle & Side Gear (at X = -0.024m)
            GameObject leftAxleGo = new GameObject("Left_Axle_Rotor");
            leftAxleGo.transform.SetParent(transform, false);
            leftAxleGo.transform.localPosition = new Vector3(-0.024f, 0f, 0f);
            leftAxleRotor = leftAxleGo.transform;

            GameObject leftSideGear = new GameObject("Left_Side_Gear_20T");
            leftSideGear.transform.SetParent(leftAxleRotor, false);
            leftSideGear.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            MeshFilter mfLeftSide = leftSideGear.AddComponent<MeshFilter>();
            MeshRenderer mrLeftSide = leftSideGear.AddComponent<MeshRenderer>();
            mfLeftSide.mesh = GearMeshBuilder.BuildBevelGearMesh(
                sideGearTeeth, rSide, sidePitchCone, 0.014f, moduleMeters, moduleMeters * 1.25f, 0.008f);
            mrLeftSide.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            leftSideGear.AddComponent<MeshCollider>().sharedMesh = mfLeftSide.mesh;
            InspectableComponent inspLeft = leftSideGear.AddComponent<InspectableComponent>();
            inspLeft.SetMetadata("Left Side Bevel Gear (Z_w = 20)",
                "Splined to left drive axle. Receives half the differential torque and drives left road wheel.",
                $"Teeth: {sideGearTeeth} | Left Axle Speed: {LeftAxleRPM:F1} RPM | Bias: {(1f + turnBias):F2}×");
            inspLeft.SetExplosionVector(Vector3.left, 0.08f, 0.12f);

            GameObject leftShaft = new GameObject("Left_Half_Shaft");
            leftShaft.transform.SetParent(leftAxleRotor, false);
            leftShaft.transform.localPosition = new Vector3(-0.05f, 0f, 0f);
            leftShaft.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            MeshFilter mfLeftShaft = leftShaft.AddComponent<MeshFilter>();
            MeshRenderer mrLeftShaft = leftShaft.AddComponent<MeshRenderer>();
            mfLeftShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.008f, 0.09f, 24, false);
            mrLeftShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

            // 5. Right Axle & Side Gear (at X = +0.024m)
            GameObject rightAxleGo = new GameObject("Right_Axle_Rotor");
            rightAxleGo.transform.SetParent(transform, false);
            rightAxleGo.transform.localPosition = new Vector3(0.024f, 0f, 0f);
            rightAxleRotor = rightAxleGo.transform;

            GameObject rightSideGear = new GameObject("Right_Side_Gear_20T");
            rightSideGear.transform.SetParent(rightAxleRotor, false);
            rightSideGear.transform.localRotation = Quaternion.Euler(0, -90f, 0);
            MeshFilter mfRightSide = rightSideGear.AddComponent<MeshFilter>();
            MeshRenderer mrRightSide = rightSideGear.AddComponent<MeshRenderer>();
            mfRightSide.mesh = GearMeshBuilder.BuildBevelGearMesh(
                sideGearTeeth, rSide, sidePitchCone, 0.014f, moduleMeters, moduleMeters * 1.25f, 0.008f);
            mrRightSide.material = CreateMaterial(new Color(0.12f, 0.58f, 0.95f), 0.5f, 0.7f); // Electric Blue
            rightSideGear.AddComponent<MeshCollider>().sharedMesh = mfRightSide.mesh;
            InspectableComponent inspRight = rightSideGear.AddComponent<InspectableComponent>();
            inspRight.SetMetadata("Right Side Bevel Gear (Z_w = 20)",
                "Splined to right drive axle. Compensates speed during left and right turns.",
                $"Teeth: {sideGearTeeth} | Right Axle Speed: {RightAxleRPM:F1} RPM | Bias: {(1f - turnBias):F2}×");
            inspRight.SetExplosionVector(Vector3.right, 0.08f, 0.12f);

            GameObject rightShaft = new GameObject("Right_Half_Shaft");
            rightShaft.transform.SetParent(rightAxleRotor, false);
            rightShaft.transform.localPosition = new Vector3(0.05f, 0f, 0f);
            rightShaft.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            MeshFilter mfRightShaft = rightShaft.AddComponent<MeshFilter>();
            MeshRenderer mrRightShaft = rightShaft.AddComponent<MeshRenderer>();
            mfRightShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.008f, 0.09f, 24, false);
            mrRightShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);

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
            // Drive Pinion: omega_pinion around Z
            currentPinionAngle += inputAngularVelDegPerSec * deltaTime;
            currentPinionAngle %= 360f;

            // Crown / Carrier: omega_c = -omega_pinion * (Z_p / Z_r) around X
            float finalDriveRatio = (float)pinionTeeth / ringTeeth; // 12 / 36 = 1/3
            float carrierVel = -inputAngularVelDegPerSec * finalDriveRatio;
            currentCarrierAngle += carrierVel * deltaTime;
            currentCarrierAngle %= 360f;

            // Left & Right Axles:
            // omega_L = omega_c * (1 + turnBias)
            // omega_R = omega_c * (1 - turnBias)
            // Notice: (omega_L + omega_R) / 2 = omega_c (Strict open differential law!)
            float leftVel = carrierVel * (1f + turnBias);
            float rightVel = carrierVel * (1f - turnBias);

            currentLeftAngle += leftVel * deltaTime;
            currentLeftAngle %= 360f;

            currentRightAngle += rightVel * deltaTime;
            currentRightAngle %= 360f;

            // Spider Pinion Relative Rotation:
            // Spiders spin on cross-pin when omega_L != omega_R:
            // omega_spider_rel = ((omega_L - omega_R) / 2) * (Z_w / Z_s)
            float spiderRelVel = ((leftVel - rightVel) * 0.5f) * ((float)sideGearTeeth / spiderTeeth);
            currentSpiderAngle += spiderRelVel * deltaTime;
            currentSpiderAngle %= 360f;

            if (drivePinionRotor != null) drivePinionRotor.localRotation = Quaternion.Euler(0, 0, currentPinionAngle);
            if (carrierRotor != null) carrierRotor.localRotation = Quaternion.Euler(currentCarrierAngle, 0, 0);
            if (leftAxleRotor != null) leftAxleRotor.localRotation = Quaternion.Euler(currentLeftAngle, 0, 0);
            if (rightAxleRotor != null) rightAxleRotor.localRotation = Quaternion.Euler(currentRightAngle, 0, 0);

            if (topSpiderRotor != null) topSpiderRotor.localRotation = Quaternion.Euler(0, currentSpiderAngle, 0);
            if (bottomSpiderRotor != null) bottomSpiderRotor.localRotation = Quaternion.Euler(0, -currentSpiderAngle, 0);
        }

        public override void ResetMechanism()
        {
            currentPinionAngle = 0f;
            currentCarrierAngle = 0f;
            currentLeftAngle = 0f;
            currentRightAngle = 0f;
            currentSpiderAngle = 0f;

            if (drivePinionRotor != null) drivePinionRotor.localRotation = Quaternion.Euler(0, 0, currentPinionAngle);
            if (carrierRotor != null) carrierRotor.localRotation = Quaternion.Euler(currentCarrierAngle, 0, 0);
            if (leftAxleRotor != null) leftAxleRotor.localRotation = Quaternion.Euler(currentLeftAngle, 0, 0);
            if (rightAxleRotor != null) rightAxleRotor.localRotation = Quaternion.Euler(currentRightAngle, 0, 0);
            if (topSpiderRotor != null) topSpiderRotor.localRotation = Quaternion.Euler(0, currentSpiderAngle, 0);
            if (bottomSpiderRotor != null) bottomSpiderRotor.localRotation = Quaternion.Euler(0, -currentSpiderAngle, 0);
        }

        public override string GetCalculationSummary()
        {
            return $"  Mechanism: Automotive Differential\n" +
                   $"  Final Drive Ratio (i0): {GearRatio:F2}:1\n" +
                   $"  Pinion Speed: {currentInputRPM:F0} RPM\n" +
                   $"  Carrier Speed: {CarrierRPM:F1} RPM\n" +
                   $"  Left Axle Speed: {LeftAxleRPM:F1} RPM\n" +
                   $"  Right Axle Speed: {RightAxleRPM:F1} RPM\n" +
                   $"  Turn Bias: {turnBias:F2}\n" +
                   $"  Efficiency: {(Efficiency * 100f):F0}%\n" +
                   $"  Input Power: {InputPowerWatts:F1} W\n" +
                   $"  Output Power: {OutputPowerWatts:F1} W";
        }

        public override string GetEngineeringExplanation()
        {
            return "Open automotive differential splitting input torque equally across drive axles while allowing different wheel speeds during cornering. " +
                   $"Carrier rotates at {CarrierRPM:F1} RPM governed by Willis epicyclic law (omega_c = (omega_L + omega_R)/2).";
        }

        public override string GetFormulaBreakdown()
        {
            return "Governing Kinematics:\n" +
                   "i0 = Z_ring / Z_pinion = 36 / 12 = 3.0:1\n" +
                   $"omega_carrier = (omega_left + omega_right) / 2 = {CarrierRPM:F1} RPM\n" +
                   $"Left Axle: {LeftAxleRPM:F1} RPM | Right Axle: {RightAxleRPM:F1} RPM";
        }
    }
}

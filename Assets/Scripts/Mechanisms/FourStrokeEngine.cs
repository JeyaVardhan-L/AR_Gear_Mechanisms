using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    public enum EngineStroke
    {
        Intake = 0,       // 0° - 180° (TDC to BDC, Intake Valve Open)
        Compression = 1,  // 180° - 360° (BDC to TDC, Both Valves Closed)
        Power = 2,        // 360° - 540° (TDC to BDC, Spark Fires, Both Valves Closed)
        Exhaust = 3       // 540° - 720° (BDC to TDC, Exhaust Valve Open)
    }

    /// <summary>
    /// Mechanism 15: Four-Stroke Internal Combustion Engine.
    /// Demonstrates the full 720° thermodynamic Otto cycle with authentic physical slider-crank kinematics,
    /// 2:1 camshaft timing drive, reciprocating poppet valves, and dynamic stroke phase telemetry.
    /// Note: This is an authentic kinematic educational simulation, not a CFD/thermodynamic simulator.
    /// </summary>
    public class FourStrokeEngine : MechanismBase
    {
        [Header("Engine Architecture")]
        [SerializeField] private float boreRadiusMeters = 0.035f;       // Bore diameter = 70 mm
        [SerializeField] private float strokeMeters = 0.070f;           // Stroke S = 70 mm (r = 35 mm)
        [SerializeField] private float rodLengthMeters = 0.115f;        // Connecting rod l = 115 mm
        [SerializeField] private float valveMaxLiftMeters = 0.007f;     // Valve lift = 7 mm

        [Header("Transforms")]
        [SerializeField] private Transform crankshaftTransform;
        [SerializeField] private Transform connectingRodTransform;
        [SerializeField] private Transform pistonTransform;
        [SerializeField] private Transform camshaftTransform;
        [SerializeField] private Transform intakeValveTransform;
        [SerializeField] private Transform exhaustValveTransform;
        [SerializeField] private Transform flywheelTransform;
        [SerializeField] private MeshRenderer sparkIndicatorRenderer;

        private float currentCycleAngleDeg = 0f; // 0° to 720° (Two full crank revolutions per 4-stroke cycle)
        private bool isAssemblyBuilt = false;

        public float CrankshaftAngleDeg => currentCycleAngleDeg % 360f;
        public float CycleAngleDeg => currentCycleAngleDeg;
        public float CrankRadius => strokeMeters * 0.5f;
        public float RodLength => rodLengthMeters;

        public EngineStroke CurrentStroke
        {
            get
            {
                if (currentCycleAngleDeg < 180f) return EngineStroke.Intake;
                if (currentCycleAngleDeg < 360f) return EngineStroke.Compression;
                if (currentCycleAngleDeg < 540f) return EngineStroke.Power;
                return EngineStroke.Exhaust;
            }
        }

        public string StrokeName
        {
            get
            {
                switch (CurrentStroke)
                {
                    case EngineStroke.Intake: return "INTAKE (1/4)";
                    case EngineStroke.Compression: return "COMPRESSION (2/4)";
                    case EngineStroke.Power: return "POWER / COMBUSTION (3/4)";
                    case EngineStroke.Exhaust: return "EXHAUST (4/4)";
                    default: return "INTAKE";
                }
            }
        }

        public bool IsIntakeValveOpen => currentCycleAngleDeg >= 10f && currentCycleAngleDeg <= 170f;
        public bool IsExhaustValveOpen => currentCycleAngleDeg >= 550f && currentCycleAngleDeg <= 710f;
        public bool IsSparkActive => Mathf.Abs(currentCycleAngleDeg - 360f) < 15f;

        /// <summary>
        /// Exact distance from crankshaft axis to wrist pin center along Y axis.
        /// </summary>
        public float PistonPositionY
        {
            get
            {
                float r = CrankRadius;
                float l = rodLengthMeters;
                float thetaRad = (CrankshaftAngleDeg) * Mathf.Deg2Rad;
                float sin = Mathf.Sin(thetaRad);
                float cos = Mathf.Cos(thetaRad);
                float underRad = (l * l) - (r * r * sin * sin);
                return (r * cos) + Mathf.Sqrt(Mathf.Max(0.0001f, underRad));
            }
        }

        /// <summary>
        /// Piston displacement from Top Dead Center (TDC, maximum Y height).
        /// </summary>
        public float DisplacementFromTDC => (CrankRadius + rodLengthMeters) - PistonPositionY;

        /// <summary>
        /// Piston instantaneous linear velocity in m/s.
        /// </summary>
        public float PistonVelocityMps
        {
            get
            {
                float r = CrankRadius;
                float l = rodLengthMeters;
                float thetaRad = CrankshaftAngleDeg * Mathf.Deg2Rad;
                float sin = Mathf.Sin(thetaRad);
                float sin2 = Mathf.Sin(2f * thetaRad);
                float omega = InputAngularVelocityRadPerSec;
                float underRad = Mathf.Sqrt(Mathf.Max(0.0001f, (l * l) - (r * r * sin * sin)));
                return -r * omega * (sin + (r * sin2) / (2f * underRad));
            }
        }

        public override string MechanismName => "Four-Stroke Engine";
        public override string CategoryTitle => "Internal Combustion Otto Cycle";
        public override float GearRatio => 2.0f; // Camshaft 2:1 reduction relative to crankshaft
        public override float OutputRPM => currentInputRPM * 0.5f; // Camshaft RPM = Crankshaft RPM / 2

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r = CrankRadius;
            float l = rodLengthMeters;
            float tdcY = r + l;
            float cylHeight = strokeMeters + 0.055f;

            // 1. Crankcase Machine Base
            GameObject crankcaseGo = new GameObject("Crankcase_Base");
            crankcaseGo.transform.SetParent(transform, false);
            crankcaseGo.transform.localPosition = new Vector3(0, -0.055f, 0);
            MeshFilter mfCase = crankcaseGo.AddComponent<MeshFilter>();
            MeshRenderer mrCase = crankcaseGo.AddComponent<MeshRenderer>();
            mfCase.mesh = GearMeshBuilder.BuildBasePlateMesh(0.18f, 0.16f, 0.030f);
            mrCase.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.4f);
            crankcaseGo.AddComponent<BoxCollider>().size = new Vector3(0.18f, 0.030f, 0.16f);
            InspectableComponent inspCase = crankcaseGo.AddComponent<InspectableComponent>();
            inspCase.SetMetadata("Engine Crankcase Block",
                "High-strength cast aluminum-silicon alloy crankcase housing main bearings, oil sump, and crankshaft.",
                "Material: AlSi9Cu3 Die-Cast Aluminum | Sump Capacity: 0.8 L");
            inspCase.SetExplosionVector(Vector3.down, 0.09f, 0.13f);

            // 2. Finned Cylinder Block with Open 120° Cutaway Section
            GameObject cylGo = new GameObject("Cylinder_Block_Cutaway");
            cylGo.transform.SetParent(transform, false);
            cylGo.transform.localPosition = new Vector3(0, tdcY - strokeMeters * 0.5f, 0);
            MeshFilter mfCyl = cylGo.AddComponent<MeshFilter>();
            MeshRenderer mrCyl = cylGo.AddComponent<MeshRenderer>();
            mfCyl.mesh = GearMeshBuilder.BuildCylinderBlockMesh(boreRadiusMeters, boreRadiusMeters + 0.012f, cylHeight, 130f, 7);
            mrCyl.material = CreateMaterial(new Color(0.30f, 0.32f, 0.36f), 0.5f, 0.45f);
            cylGo.AddComponent<MeshCollider>().sharedMesh = mfCyl.mesh;
            InspectableComponent inspCyl = cylGo.AddComponent<InspectableComponent>();
            inspCyl.SetMetadata("Cylinder Bore & Cooling Fins",
                "Cross-sectioned cylinder liner revealing internal reciprocating piston motion. Exterior fins dissipate combustion heat.",
                $"Bore: {boreRadiusMeters * 2000f:F0} mm | Displacement: {Mathf.PI * boreRadiusMeters * boreRadiusMeters * strokeMeters * 1e6f:F0} cc | Honed Iron Liner");
            inspCyl.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 3. Precision Crankshaft (Rotates around local Z at 0,0,0)
            GameObject crankGo = new GameObject("Crankshaft_Rotor");
            crankGo.transform.SetParent(transform, false);
            crankGo.transform.localPosition = Vector3.zero;
            crankshaftTransform = crankGo.transform;

            GameObject crankModel = new GameObject("Crankshaft_Shaft_Webs");
            crankModel.transform.SetParent(crankshaftTransform, false);
            MeshFilter mfCrank = crankModel.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankModel.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildCrankshaftMesh(0.009f, 0.008f, strokeMeters, 0.012f, 0.034f, 0.046f);
            mrCrank.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.85f, 0.8f);
            crankModel.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankModel.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Forged Counterweighted Crankshaft",
                "Converts reciprocating piston thrust into smooth output torque. Curved counterweights balance primary reciprocating inertia.",
                $"Stroke: {strokeMeters * 1000f:F1} mm | Journal Dia: 18 mm | 42CrMo4 Forged Steel");
            inspCrank.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            // 4. Inertia Flywheel (Mounted on rear crankshaft journal)
            GameObject flywheelGo = new GameObject("Inertia_Flywheel");
            flywheelGo.transform.SetParent(crankshaftTransform, false);
            flywheelGo.transform.localPosition = new Vector3(0, 0, 0.065f);
            flywheelTransform = flywheelGo.transform;
            MeshFilter mfFly = flywheelGo.AddComponent<MeshFilter>();
            MeshRenderer mrFly = flywheelGo.AddComponent<MeshRenderer>();
            mfFly.mesh = GearMeshBuilder.BuildCylinderMesh(0.060f, 0.014f, 32, false);
            mrFly.material = CreateMaterial(new Color(0.22f, 0.23f, 0.26f), 0.6f, 0.5f);
            flywheelGo.AddComponent<MeshCollider>().sharedMesh = mfFly.mesh;
            InspectableComponent inspFly = flywheelGo.AddComponent<InspectableComponent>();
            inspFly.SetMetadata("Heavy Inertia Flywheel",
                "Stores rotational kinetic energy during the power stroke to smoothly carry the crankshaft through non-power strokes (exhaust, intake, compression).",
                "Diameter: 120 mm | Mass: 1.4 kg | Cast Nodular Iron");
            inspFly.SetExplosionVector(Vector3.forward, 0.09f, 0.13f);

            // 5. Forged Connecting Rod
            GameObject rodGo = new GameObject("Connecting_Rod");
            rodGo.transform.SetParent(transform, false);
            connectingRodTransform = rodGo.transform;
            MeshFilter mfRod = rodGo.AddComponent<MeshFilter>();
            MeshRenderer mrRod = rodGo.AddComponent<MeshRenderer>();
            mfRod.mesh = GearMeshBuilder.BuildConnectingRodMesh(l, 0.0085f, 0.006f, 0.012f, 0.012f);
            mrRod.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            rodGo.AddComponent<MeshCollider>().sharedMesh = mfRod.mesh;
            InspectableComponent inspRod = rodGo.AddComponent<InspectableComponent>();
            inspRod.SetMetadata("H-Beam Forged Connecting Rod",
                "Articulates between crankpin and piston wristpin. Precision split big-end cap with replaceable shell bearings.",
                $"Center-to-Center: {l * 1000f:F1} mm | Weight: 240g | Forged 4340 Chromoly");
            inspRod.SetExplosionVector(Vector3.right, 0.07f, 0.11f);

            // 6. Trunk Piston with Compression Rings
            GameObject pistonGo = new GameObject("Trunk_Piston");
            pistonGo.transform.SetParent(transform, false);
            pistonTransform = pistonGo.transform;
            MeshFilter mfPiston = pistonGo.AddComponent<MeshFilter>();
            MeshRenderer mrPiston = pistonGo.AddComponent<MeshRenderer>();
            mfPiston.mesh = GearMeshBuilder.BuildPistonMesh(boreRadiusMeters - 0.0008f, 0.040f, 0.006f);
            mrPiston.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            pistonGo.AddComponent<MeshCollider>().sharedMesh = mfPiston.mesh;
            InspectableComponent inspPiston = pistonGo.AddComponent<InspectableComponent>();
            inspPiston.SetMetadata("Lightweight Alloy Trunk Piston",
                "Transfers combustion pressure to connecting rod. Features 2 compression ring grooves and oil scraper relief skirt.",
                $"Bore: {boreRadiusMeters * 2000f:F0} mm | Height: 40 mm | Forged High-Silicon Aluminum");
            inspPiston.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // 7. Overhead Cylinder Head with Poppet Valves & Camshaft
            float headY = tdcY + 0.035f;

            // Intake Valve (at X = -0.016m, Y = headY)
            GameObject inValveGo = new GameObject("Intake_Poppet_Valve");
            inValveGo.transform.SetParent(transform, false);
            inValveGo.transform.localPosition = new Vector3(-0.016f, headY, 0);
            intakeValveTransform = inValveGo.transform;
            MeshFilter mfInV = inValveGo.AddComponent<MeshFilter>();
            MeshRenderer mrInV = inValveGo.AddComponent<MeshRenderer>();
            mfInV.mesh = GearMeshBuilder.BuildPoppetValveMesh(0.012f, 0.003f, 0.040f);
            mrInV.material = CreateMaterial(new Color(0.20f, 0.80f, 0.50f), 0.8f, 0.7f); // Emerald Green
            inValveGo.AddComponent<MeshCollider>().sharedMesh = mfInV.mesh;
            InspectableComponent inspInV = inValveGo.AddComponent<InspectableComponent>();
            inspInV.SetMetadata("Intake Poppet Valve",
                "Opens during the intake stroke to admit the fresh air-fuel charge into the cylinder chamber.",
                "Head Dia: 24 mm | Lift: 7.0 mm | Austenitic Stainless Steel");
            inspInV.SetExplosionVector(new Vector3(-0.08f, 0.06f, 0), 0.10f, 0.14f);

            // Exhaust Valve (at X = +0.016m, Y = headY)
            GameObject exValveGo = new GameObject("Exhaust_Poppet_Valve");
            exValveGo.transform.SetParent(transform, false);
            exValveGo.transform.localPosition = new Vector3(0.016f, headY, 0);
            exhaustValveTransform = exValveGo.transform;
            MeshFilter mfExV = exValveGo.AddComponent<MeshFilter>();
            MeshRenderer mrExV = exValveGo.AddComponent<MeshRenderer>();
            mfExV.mesh = GearMeshBuilder.BuildPoppetValveMesh(0.010f, 0.003f, 0.040f);
            mrExV.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.8f, 0.7f); // Crimson Accent
            exValveGo.AddComponent<MeshCollider>().sharedMesh = mfExV.mesh;
            InspectableComponent inspExV = exValveGo.AddComponent<InspectableComponent>();
            inspExV.SetMetadata("Exhaust Poppet Valve",
                "Opens during the exhaust stroke to evacuate high-temperature combustion gases from the cylinder.",
                "Head Dia: 20 mm | Lift: 7.0 mm | Nimonic Superalloy");
            inspExV.SetExplosionVector(new Vector3(0.08f, 0.06f, 0), 0.10f, 0.14f);

            // Overhead Camshaft (at Y = headY + 0.045m, rotates at 1/2 crank speed)
            GameObject camGo = new GameObject("Overhead_Camshaft_Rotor");
            camGo.transform.SetParent(transform, false);
            camGo.transform.localPosition = new Vector3(0, headY + 0.045f, 0);
            camshaftTransform = camGo.transform;

            GameObject camModel = new GameObject("Camshaft_Lobe_Model");
            camModel.transform.SetParent(camshaftTransform, false);
            MeshFilter mfCam = camModel.AddComponent<MeshFilter>();
            MeshRenderer mrCam = camModel.AddComponent<MeshRenderer>();
            mfCam.mesh = GearMeshBuilder.BuildCamMesh(0.014f, 0.008f, 0.050f, 0, 0.006f);
            mrCam.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.8f);
            camModel.AddComponent<MeshCollider>().sharedMesh = mfCam.mesh;
            InspectableComponent inspCam = camModel.AddComponent<InspectableComponent>();
            inspCam.SetMetadata("Overhead Camshaft (OHC, 2:1 Timing)",
                "Rotates at exactly half crankshaft speed (N_cam = N_crank / 2) to actuate intake and exhaust valves in precise synchronization.",
                "Speed Ratio: 2.0:1 | Chilled Cast Iron | Precision Ground Lobes");
            inspCam.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // Spark Plug Representation at top center (X=0, Y = headY)
            GameObject sparkGo = new GameObject("Spark_Plug");
            sparkGo.transform.SetParent(transform, false);
            sparkGo.transform.localPosition = new Vector3(0, headY + 0.010f, 0);
            MeshFilter mfSpark = sparkGo.AddComponent<MeshFilter>();
            sparkIndicatorRenderer = sparkGo.AddComponent<MeshRenderer>();
            mfSpark.mesh = GearMeshBuilder.BuildCylinderMesh(0.004f, 0.022f, 16, false);
            sparkIndicatorRenderer.material = CreateMaterial(new Color(0.95f, 0.95f, 0.95f), 0.2f, 0.9f); // Ceramic white
            sparkGo.AddComponent<MeshCollider>().sharedMesh = mfSpark.mesh;
            InspectableComponent inspSpark = sparkGo.AddComponent<InspectableComponent>();
            inspSpark.SetMetadata("High-Voltage Spark Plug",
                "Discharges 25 kV electrical spark across electrode gap near 360° TDC to ignite compressed fuel charge.",
                "Thread: M10x1.0 | Iridium Electrode | Heat Range: 7");
            inspSpark.SetExplosionVector(Vector3.up, 0.06f, 0.10f);

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
            // Crankshaft rotation (720° cycle period)
            currentCycleAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCycleAngleDeg %= 720f;
            if (currentCycleAngleDeg < 0f) currentCycleAngleDeg += 720f;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            float crankAngle = CrankshaftAngleDeg;
            float crankRad = crankAngle * Mathf.Deg2Rad;
            float r = CrankRadius;
            float l = rodLengthMeters;

            // 1. Crankshaft & Flywheel Rotate around Z
            if (crankshaftTransform != null)
            {
                crankshaftTransform.localRotation = Quaternion.Euler(0f, 0f, crankAngle);
            }

            // 2. Crankpin Position in world coordinates
            Vector3 crankPinPos = new Vector3(Mathf.Sin(crankRad) * r, Mathf.Cos(crankRad) * r, 0.015f);

            // 3. Piston Position along Y axis
            float yPiston = PistonPositionY;
            Vector3 wristPinPos = new Vector3(0f, yPiston, 0.015f);

            if (pistonTransform != null)
            {
                pistonTransform.localPosition = new Vector3(0f, yPiston, 0f);
            }

            // 4. Connecting Rod connects crankPinPos to wristPinPos
            if (connectingRodTransform != null)
            {
                connectingRodTransform.localPosition = crankPinPos;
                Vector3 toWrist = wristPinPos - crankPinPos;
                float angleZ = Mathf.Atan2(toWrist.y, toWrist.x) * Mathf.Rad2Deg;
                connectingRodTransform.localRotation = Quaternion.Euler(0f, 0f, angleZ - 90f);
            }

            // 5. Camshaft Rotates at 1/2 Crankshaft Speed (360° cam rotation per 720° crank cycle)
            float camAngle = currentCycleAngleDeg * 0.5f;
            if (camshaftTransform != null)
            {
                camshaftTransform.localRotation = Quaternion.Euler(0f, 0f, camAngle);
            }

            // 6. Valve Lifts
            float headY = (r + l) + 0.035f;

            // Intake valve opens during Intake stroke (10° to 170°)
            float inLift = 0f;
            if (currentCycleAngleDeg >= 10f && currentCycleAngleDeg <= 170f)
            {
                float frac = Mathf.Sin((currentCycleAngleDeg - 10f) / 160f * Mathf.PI);
                inLift = frac * valveMaxLiftMeters;
            }
            if (intakeValveTransform != null)
            {
                intakeValveTransform.localPosition = new Vector3(-0.016f, headY - inLift, 0f);
            }

            // Exhaust valve opens during Exhaust stroke (550° to 710°)
            float exLift = 0f;
            if (currentCycleAngleDeg >= 550f && currentCycleAngleDeg <= 710f)
            {
                float frac = Mathf.Sin((currentCycleAngleDeg - 550f) / 160f * Mathf.PI);
                exLift = frac * valveMaxLiftMeters;
            }
            if (exhaustValveTransform != null)
            {
                exhaustValveTransform.localPosition = new Vector3(0.016f, headY - exLift, 0f);
            }

            // 7. Spark Ignition Flash at 360° TDC
            if (sparkIndicatorRenderer != null && sparkIndicatorRenderer.material != null)
            {
                if (IsSparkActive)
                {
                    sparkIndicatorRenderer.material.color = new Color(1f, 0.85f, 0.2f); // Brilliant spark yellow
                }
                else
                {
                    sparkIndicatorRenderer.material.color = new Color(0.95f, 0.95f, 0.95f); // Normal ceramic
                }
            }
        }

        public override void ResetMechanism()
        {
            currentCycleAngleDeg = 0f; // Start at Beginning of Intake Stroke (0° TDC)
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Stroke Phase: {StrokeName}\n" +
                   $"• Cycle Angle: {currentCycleAngleDeg:F0}° / 720°\n" +
                   $"• Crankshaft Speed: {currentInputRPM:F0} RPM\n" +
                   $"• Camshaft Speed: {OutputRPM:F0} RPM (2:1)\n" +
                   $"• Piston Displacement: {DisplacementFromTDC * 1000f:F1} mm / {strokeMeters * 1000f:F1} mm\n" +
                   $"• Piston Velocity: {PistonVelocityMps:F2} m/s\n" +
                   $"• Intake Valve: {(IsIntakeValveOpen ? "OPEN 🟢" : "CLOSED ⚪")}\n" +
                   $"• Exhaust Valve: {(IsExhaustValveOpen ? "OPEN 🔴" : "CLOSED ⚪")}";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Four-Stroke Internal Combustion Engine executes the thermodynamic Otto cycle across two complete crankshaft revolutions (720°). " +
                   "A dedicated overhead camshaft driven at half crankshaft speed (2:1 timing) synchronizes intake poppet valves (drawing fuel-air mixture) " +
                   "and exhaust valves (purging combustion gases) with piston reciprocation. Spark ignition occurs near 360° TDC to initiate the power stroke.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Thermodynamic Cycle: 720° Crank Rotation (2 Revolutions)\n" +
                   "Camshaft Timing: N_cam = N_crank / 2 = " + $"{currentInputRPM:F0} / 2 = {OutputRPM:F0} RPM\n" +
                   "Kinematic Displacement: y(θ) = r·cosθ + √(l² - r²·sin²θ)\n" +
                   $"Piston Speed: v = {PistonVelocityMps:F2} m/s | Stroke: {strokeMeters * 1000f:F1} mm\n" +
                   "Stroke Sequence: Intake (0-180°) ➔ Compression (180-360°) ➔ Power (360-540°) ➔ Exhaust (540-720°)";
        }
    }
}

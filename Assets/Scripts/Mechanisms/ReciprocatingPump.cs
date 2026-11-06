using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    public enum PumpStroke
    {
        Suction = 0,   // 0° - 180° (Piston retreats, Suction Valve OPEN, Delivery Valve CLOSED)
        Delivery = 1   // 180° - 360° (Piston advances, Suction Valve CLOSED, Delivery Valve OPEN)
    }

    /// <summary>
    /// Mechanism 18: Reciprocating Positive-Displacement Pump.
    /// Demonstrates positive-displacement pumping action through physical slider-crank kinematics:
    /// - Suction Stroke (0° - 180°): Piston retreats, creating chamber vacuum; suction non-return valve opens.
    /// - Delivery Stroke (180° - 360°): Piston advances, pressurizing liquid; delivery non-return valve opens.
    /// Note: This is an authentic kinematic/fluid-flow educational simulation, not a CFD simulation.
    /// </summary>
    public class ReciprocatingPump : MechanismBase
    {
        [Header("Pump Geometry")]
        [SerializeField] private float cylinderBoreRadius = 0.030f; // Bore = 60 mm (D = 0.060 m)
        [SerializeField] private float crankRadiusMeters = 0.030f;   // Crank r = 30 mm (Stroke S = 60 mm)
        [SerializeField] private float rodLengthMeters = 0.100f;     // Connecting rod l = 100 mm

        [Header("Transforms")]
        [SerializeField] private Transform crankshaftTransform;
        [SerializeField] private Transform connectingRodTransform;
        [SerializeField] private Transform pistonTransform;
        [SerializeField] private Transform suctionValveTransform;
        [SerializeField] private Transform deliveryValveTransform;
        [SerializeField] private MeshRenderer suctionFlowRenderer;
        [SerializeField] private MeshRenderer deliveryFlowRenderer;

        private float currentCrankAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public float CrankRadius => crankRadiusMeters;
        public float RodLength => rodLengthMeters;
        public float CrankAngleDeg => currentCrankAngleDeg;
        public float StrokeMeters => 2f * crankRadiusMeters;

        public PumpStroke CurrentStroke => (currentCrankAngleDeg < 180f) ? PumpStroke.Suction : PumpStroke.Delivery;

        public bool IsSuctionValveOpen => currentCrankAngleDeg >= 15f && currentCrankAngleDeg <= 165f;
        public bool IsDeliveryValveOpen => currentCrankAngleDeg >= 195f && currentCrankAngleDeg <= 345f;

        /// <summary>
        /// Piston displacement from cylinder head (maximum compression position) along X axis in meters.
        /// </summary>
        public float PistonDisplacementX
        {
            get
            {
                float r = crankRadiusMeters;
                float l = rodLengthMeters;
                float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
                float sin = Mathf.Sin(rad);
                float cos = Mathf.Cos(rad);
                float underRad = (l * l) - (r * r * sin * sin);
                return (r * cos) + Mathf.Sqrt(Mathf.Max(0.0001f, underRad));
            }
        }

        /// <summary>
        /// Instantaneous piston velocity in m/s.
        /// </summary>
        public float PistonVelocityMps
        {
            get
            {
                float r = crankRadiusMeters;
                float l = rodLengthMeters;
                float rad = currentCrankAngleDeg * Mathf.Deg2Rad;
                float sin = Mathf.Sin(rad);
                float sin2 = Mathf.Sin(2f * rad);
                float omega = InputAngularVelocityRadPerSec;
                float underRad = Mathf.Sqrt(Mathf.Max(0.0001f, (l * l) - (r * r * sin * sin)));
                return -r * omega * (sin + (r * sin2) / (2f * underRad));
            }
        }

        /// <summary>
        /// Theoretical volumetric discharge rate in liters per minute (L/min).
        /// Q = (pi * D^2 / 4) * S * N
        /// </summary>
        public float TheoreticalDischargeLpm
        {
            get
            {
                float area = Mathf.PI * cylinderBoreRadius * cylinderBoreRadius;
                float stroke = StrokeMeters;
                float m3PerSec = area * stroke * (currentInputRPM / 60f);
                return m3PerSec * 1000f * 60f; // Convert m^3/s to L/min
            }
        }

        public override string MechanismName => "Reciprocating Pump";
        public override string CategoryTitle => "Positive-Displacement Fluid Machine";
        public override float GearRatio => 1.0f;
        public override float OutputRPM => currentInputRPM;

        protected override void Awake()
        {
            base.Awake();
            BuildMechanismAssembly();
        }

        public void BuildMechanismAssembly()
        {
            if (isAssemblyBuilt) return;
            isAssemblyBuilt = true;

            float r = crankRadiusMeters;
            float l = rodLengthMeters;
            float totalBedLength = r + l + 0.15f;

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3((r + l) * 0.45f, -0.045f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(totalBedLength, 0.15f, 0.022f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.20f, 0.23f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(totalBedLength, 0.022f, 0.15f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Pump Skidded Bedplate",
                "High-rigidity cast foundation mounting drive motor bearings, pump fluid head, and suction/delivery manifolds.",
                "Material: Cast Carbon Steel ASTM A216 WCB");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Crankshaft Assembly (at origin 0,0,0)
            GameObject crankGo = new GameObject("Crank_Rotor");
            crankGo.transform.SetParent(transform, false);
            crankGo.transform.localPosition = Vector3.zero;
            crankshaftTransform = crankGo.transform;

            GameObject crankDisk = new GameObject("Crank_Web_Disk");
            crankDisk.transform.SetParent(crankshaftTransform, false);
            MeshFilter mfCrank = crankDisk.AddComponent<MeshFilter>();
            MeshRenderer mrCrank = crankDisk.AddComponent<MeshRenderer>();
            mfCrank.mesh = GearMeshBuilder.BuildCylinderMesh(r + 0.012f, 0.014f, 32, false);
            mrCrank.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            crankDisk.AddComponent<MeshCollider>().sharedMesh = mfCrank.mesh;
            InspectableComponent inspCrank = crankDisk.AddComponent<InspectableComponent>();
            inspCrank.SetMetadata("Pump Drive Crank",
                "Rotates at input motor speed, establishing reciprocating pump stroke S = 2r = 60 mm.",
                $"Stroke: {StrokeMeters * 1000f:F1} mm | Drive Speed: {currentInputRPM:F0} RPM");
            inspCrank.SetExplosionVector(Vector3.back, 0.06f, 0.10f);

            // Crank pin
            GameObject crankPin = new GameObject("Crank_Pin");
            crankPin.transform.SetParent(crankshaftTransform, false);
            crankPin.transform.localPosition = new Vector3(r, 0f, 0.012f);
            MeshFilter mfPin = crankPin.AddComponent<MeshFilter>();
            MeshRenderer mrPin = crankPin.AddComponent<MeshRenderer>();
            mfPin.mesh = GearMeshBuilder.BuildCylinderMesh(0.007f, 0.025f, 20, false);
            mrPin.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            crankPin.AddComponent<CapsuleCollider>().height = 0.025f;

            // 3. Connecting Rod
            GameObject rodGo = new GameObject("Connecting_Rod");
            rodGo.transform.SetParent(transform, false);
            connectingRodTransform = rodGo.transform;
            MeshFilter mfRod = rodGo.AddComponent<MeshFilter>();
            MeshRenderer mrRod = rodGo.AddComponent<MeshRenderer>();
            mfRod.mesh = GearMeshBuilder.BuildConnectingRodMesh(l, 0.0075f, 0.006f, 0.012f, 0.012f);
            mrRod.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            rodGo.AddComponent<MeshCollider>().sharedMesh = mfRod.mesh;
            InspectableComponent inspRod = rodGo.AddComponent<InspectableComponent>();
            inspRod.SetMetadata("Forged Connecting Rod",
                "Translates rotary crank torque into linear piston reciprocation.",
                $"Length: {l * 1000f:F1} mm | Forged Steel");
            inspRod.SetExplosionVector(Vector3.up, 0.07f, 0.11f);

            // 4. Pump Cylinder Housing (Positioned at X = l + r + 0.035m)
            float cylCenterX = l + r + 0.035f;
            GameObject cylGo = new GameObject("Pump_Cylinder_Chamber");
            cylGo.transform.SetParent(transform, false);
            cylGo.transform.localPosition = new Vector3(cylCenterX, 0f, 0f);
            MeshFilter mfCyl = cylGo.AddComponent<MeshFilter>();
            MeshRenderer mrCyl = cylGo.AddComponent<MeshRenderer>();
            mfCyl.mesh = GearMeshBuilder.BuildCylinderBlockMesh(cylinderBoreRadius, cylinderBoreRadius + 0.012f, StrokeMeters + 0.045f, 130f, 5);
            mrCyl.material = CreateMaterial(new Color(0.30f, 0.32f, 0.36f), 0.5f, 0.45f);
            cylGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // Cylinder along X axis
            cylGo.AddComponent<MeshCollider>().sharedMesh = mfCyl.mesh;
            InspectableComponent inspCyl = cylGo.AddComponent<InspectableComponent>();
            inspCyl.SetMetadata("Pump Working Barrel & Manifold",
                "Precision bored working barrel with transparent viewing window revealing fluid displacement.",
                $"Bore: {cylinderBoreRadius * 2000f:F0} mm | Displacement: {Mathf.PI * cylinderBoreRadius * cylinderBoreRadius * StrokeMeters * 1e6f:F1} mL/stroke");
            inspCyl.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 5. Pump Piston with Packing Seals
            GameObject pistonGo = new GameObject("Pump_Piston");
            pistonGo.transform.SetParent(transform, false);
            pistonTransform = pistonGo.transform;
            MeshFilter mfPiston = pistonGo.AddComponent<MeshFilter>();
            MeshRenderer mrPiston = pistonGo.AddComponent<MeshRenderer>();
            mfPiston.mesh = GearMeshBuilder.BuildPistonMesh(cylinderBoreRadius - 0.0006f, 0.035f, 0.006f);
            mrPiston.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.7f, 0.65f); // Crimson Accent
            pistonGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f); // Face along +X
            pistonGo.AddComponent<MeshCollider>().sharedMesh = mfPiston.mesh;
            InspectableComponent inspPiston = pistonGo.AddComponent<InspectableComponent>();
            inspPiston.SetMetadata("Pump Plunger Piston",
                "Displaces fluid during forward stroke and generates suction during retreat. Fitted with PTFE chevron packing rings.",
                $"Diameter: {cylinderBoreRadius * 2000f:F0} mm | Corrosion-Resistant 316 Stainless Steel");
            inspPiston.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // 6. Suction (Inlet) Valve & Pipe (Bottom-right port at X = cylCenterX + 0.05m, Y = -0.040m)
            float portX = cylCenterX + 0.040f;
            GameObject inPipeGo = new GameObject("Suction_Inlet_Pipe");
            inPipeGo.transform.SetParent(transform, false);
            inPipeGo.transform.localPosition = new Vector3(portX, -0.040f, 0f);
            MeshFilter mfInPipe = inPipeGo.AddComponent<MeshFilter>();
            suctionFlowRenderer = inPipeGo.AddComponent<MeshRenderer>();
            mfInPipe.mesh = GearMeshBuilder.BuildCylinderMesh(0.012f, 0.045f, 16, false);
            suctionFlowRenderer.material = CreateMaterial(new Color(0.20f, 0.60f, 0.95f), 0.4f, 0.7f); // Water Blue
            inPipeGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            inPipeGo.AddComponent<CapsuleCollider>().height = 0.045f;
            InspectableComponent inspInPipe = inPipeGo.AddComponent<InspectableComponent>();
            inspInPipe.SetMetadata("Suction Pipe & Port",
                "Draws fluid into cylinder barrel from reservoir during suction stroke when pressure drops below atmospheric.",
                "Suction Port: 1.0 inch NPT | Max Vacuum: -0.85 bar");
            inspInPipe.SetExplosionVector(Vector3.down, 0.07f, 0.11f);

            // Suction Non-Return Valve Ball/Poppet (inside inlet pipe)
            GameObject sucValveGo = new GameObject("Suction_Check_Valve");
            sucValveGo.transform.SetParent(transform, false);
            sucValveGo.transform.localPosition = new Vector3(portX, -0.022f, 0f);
            suctionValveTransform = sucValveGo.transform;
            MeshFilter mfSucV = sucValveGo.AddComponent<MeshFilter>();
            MeshRenderer mrSucV = sucValveGo.AddComponent<MeshRenderer>();
            mfSucV.mesh = GearMeshBuilder.BuildCylinderMesh(0.008f, 0.008f, 16, false);
            mrSucV.material = CreateMaterial(new Color(0.20f, 0.85f, 0.50f), 0.8f, 0.7f); // Emerald
            sucValveGo.AddComponent<CapsuleCollider>().height = 0.008f;
            InspectableComponent inspSucV = sucValveGo.AddComponent<InspectableComponent>();
            inspSucV.SetMetadata("Suction Non-Return Valve",
                "Automatically lifts open during suction stroke (0°-180°) and seats firmly during delivery (180°-360°) to prevent backflow.",
                "Type: Spring-Loaded Disc Valve | Seating: Viton Elastomer");
            inspSucV.SetExplosionVector(new Vector3(0.05f, -0.05f, 0f), 0.08f, 0.12f);

            // 7. Delivery (Outlet) Valve & Pipe (Top-right port at X = portX, Y = +0.040m)
            GameObject outPipeGo = new GameObject("Delivery_Outlet_Pipe");
            outPipeGo.transform.SetParent(transform, false);
            outPipeGo.transform.localPosition = new Vector3(portX, 0.040f, 0f);
            MeshFilter mfOutPipe = outPipeGo.AddComponent<MeshFilter>();
            deliveryFlowRenderer = outPipeGo.AddComponent<MeshRenderer>();
            mfOutPipe.mesh = GearMeshBuilder.BuildCylinderMesh(0.010f, 0.045f, 16, false);
            deliveryFlowRenderer.material = CreateMaterial(new Color(0.95f, 0.45f, 0.20f), 0.4f, 0.7f); // Amber / Pressure Orange
            outPipeGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            outPipeGo.AddComponent<CapsuleCollider>().height = 0.045f;
            InspectableComponent inspOutPipe = outPipeGo.AddComponent<InspectableComponent>();
            inspOutPipe.SetMetadata("Delivery Discharge Pipe",
                "Delivers pressurized fluid to downstream piping system under positive displacement thrust.",
                "Discharge Port: 0.75 inch NPT | Rated Pressure: 16 bar");
            inspOutPipe.SetExplosionVector(Vector3.up, 0.07f, 0.11f);

            // Delivery Non-Return Valve
            GameObject delValveGo = new GameObject("Delivery_Check_Valve");
            delValveGo.transform.SetParent(transform, false);
            delValveGo.transform.localPosition = new Vector3(portX, 0.022f, 0f);
            deliveryValveTransform = delValveGo.transform;
            MeshFilter mfDelV = delValveGo.AddComponent<MeshFilter>();
            MeshRenderer mrDelV = delValveGo.AddComponent<MeshRenderer>();
            mfDelV.mesh = GearMeshBuilder.BuildCylinderMesh(0.008f, 0.008f, 16, false);
            mrDelV.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.8f, 0.7f);
            delValveGo.AddComponent<CapsuleCollider>().height = 0.008f;
            InspectableComponent inspDelV = delValveGo.AddComponent<InspectableComponent>();
            inspDelV.SetMetadata("Delivery Non-Return Valve",
                "Forced open by pressurized fluid during delivery stroke (180°-360°), closing instantly when piston reverses.",
                "Type: Poppet Valve | Differential Cracking Pressure: 0.2 bar");
            inspDelV.SetExplosionVector(new Vector3(0.05f, 0.05f, 0f), 0.08f, 0.12f);

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
            currentCrankAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCrankAngleDeg %= 360f;
            if (currentCrankAngleDeg < 0f) currentCrankAngleDeg += 360f;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            float theta = currentCrankAngleDeg * Mathf.Deg2Rad;
            float r = crankRadiusMeters;
            float l = rodLengthMeters;

            // 1. Crankshaft Rotates around Z
            if (crankshaftTransform != null)
            {
                crankshaftTransform.localRotation = Quaternion.Euler(0f, 0f, currentCrankAngleDeg);
            }

            // 2. Crankpin Position
            Vector3 crankPinPos = new Vector3(Mathf.Cos(theta) * r, Mathf.Sin(theta) * r, 0.012f);

            // 3. Piston Position along X axis
            float xPiston = PistonDisplacementX;
            Vector3 wristPinPos = new Vector3(xPiston, 0f, 0.012f);

            if (pistonTransform != null)
            {
                pistonTransform.localPosition = new Vector3(xPiston, 0f, 0f);
            }

            // 4. Connecting Rod connects crankPinPos to wristPinPos
            if (connectingRodTransform != null)
            {
                connectingRodTransform.localPosition = crankPinPos;
                Vector3 toWrist = wristPinPos - crankPinPos;
                float angleZ = Mathf.Atan2(toWrist.y, toWrist.x) * Mathf.Rad2Deg;
                connectingRodTransform.localRotation = Quaternion.Euler(0f, 0f, angleZ - 90f);
            }

            // 5. Valve Animations
            float portX = l + r + 0.035f + 0.040f;
            // Suction valve lifts downward into cylinder during suction (0°-180°)
            float sucLift = IsSuctionValveOpen ? 0.005f : 0f;
            if (suctionValveTransform != null)
            {
                suctionValveTransform.localPosition = new Vector3(portX, -0.022f + sucLift, 0f);
            }

            // Delivery valve lifts upward during delivery (180°-360°)
            float delLift = IsDeliveryValveOpen ? 0.005f : 0f;
            if (deliveryValveTransform != null)
            {
                deliveryValveTransform.localPosition = new Vector3(portX, 0.022f + delLift, 0f);
            }

            // 6. Fluid Flow Indicators (Color pulse intensity)
            if (suctionFlowRenderer != null && suctionFlowRenderer.material != null)
            {
                float a = IsSuctionValveOpen ? 0.9f : 0.25f;
                Color col = suctionFlowRenderer.material.color;
                col.a = a;
                suctionFlowRenderer.material.color = col;
            }

            if (deliveryFlowRenderer != null && deliveryFlowRenderer.material != null)
            {
                float a = IsDeliveryValveOpen ? 0.9f : 0.25f;
                Color col = deliveryFlowRenderer.material.color;
                col.a = a;
                deliveryFlowRenderer.material.color = col;
            }
        }

        public override void ResetMechanism()
        {
            currentCrankAngleDeg = 0f; // Start of Suction Stroke
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Operating Phase: {(CurrentStroke == PumpStroke.Suction ? "SUCTION STROKE 💧" : "DELIVERY STROKE ⚡")}\n" +
                   $"• Crank Angle (θ): {currentCrankAngleDeg:F1}°\n" +
                   $"• Plunger Speed: {PistonVelocityMps:F2} m/s\n" +
                   $"• Theoretical Flow: {TheoreticalDischargeLpm:F1} L/min\n" +
                   $"• Suction Check Valve: {(IsSuctionValveOpen ? "OPEN 🟢" : "SEATED ⚪")}\n" +
                   $"• Delivery Check Valve: {(IsDeliveryValveOpen ? "OPEN 🔴" : "SEATED ⚪")}\n" +
                   $"• Bore / Stroke: {cylinderBoreRadius * 2000f:F0} mm / {StrokeMeters * 1000f:F1} mm";
        }

        public override string GetEngineeringExplanation()
        {
            return "The Reciprocating Pump is a positive-displacement machine that moves a fixed volume of liquid per cycle. " +
                   "During the suction stroke, plunger retraction induces a partial vacuum, unseating the suction check valve. " +
                   "During the forward delivery stroke, fluid pressure instantly forces the suction valve closed while driving the delivery check valve open.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Displacement Volume: V_d = (π·D²/4)·S\n" +
                   $"V_d = (π·{cylinderBoreRadius * 2000f:F0}²/4)·{StrokeMeters * 1000f:F1} mm³ = {Mathf.PI * cylinderBoreRadius * cylinderBoreRadius * StrokeMeters * 1e6f:F1} mL\n" +
                   "Theoretical Discharge: Q = V_d · N\n" +
                   $"Q = {TheoreticalDischargeLpm:F1} L/min at {currentInputRPM:F0} RPM\n" +
                   "Plunger Kinematics: x(θ) = r·cosθ + √(l² - r²·sin²θ)";
        }
    }
}

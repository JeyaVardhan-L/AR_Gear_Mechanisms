using System;
using UnityEngine;
using MechXR.Procedural;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    public enum CamProfileType
    {
        SimpleHarmonicPear = 0,
        CircularEccentric = 1
    }

    /// <summary>
    /// Mechanism 17: Cam and Follower.
    /// Demonstrates rotary-to-linear conversion using custom kinematic cam profiles:
    /// - Simple Harmonic Motion (SHM) Pear Cam: Dwell-Rise-Dwell-Return
    /// - Circular Eccentric Cam: Continuous sinusoidal reciprocating motion
    /// Includes roller follower, return spring, guide sleeve, and live lift/dwell measurement.
    /// </summary>
    public class CamAndFollower : MechanismBase
    {
        [Header("Cam Parameters")]
        [SerializeField] private float baseRadiusMeters = 0.035f;   // Base circle Rb = 35 mm
        [SerializeField] private float maxLiftMeters = 0.018f;      // Maximum lift h = 18 mm
        [SerializeField] private float camWidth = 0.022f;
        [SerializeField] private float rollerRadius = 0.009f;
        [SerializeField] private CamProfileType profileType = CamProfileType.SimpleHarmonicPear;

        [Header("Transforms")]
        [SerializeField] private Transform camshaftRotor;
        [SerializeField] private Transform followerTransform;
        [SerializeField] private Transform returnSpringTransform;
        [SerializeField] private Transform rollerTransform;

        private float currentCamAngleDeg = 0f;
        private bool isAssemblyBuilt = false;

        public CamProfileType ActiveProfile
        {
            get => profileType;
            set
            {
                profileType = value;
                NotifyKinematicsUpdated();
            }
        }

        public float CamAngleDeg => currentCamAngleDeg;
        public float BaseRadius => baseRadiusMeters;
        public float MaxLift => maxLiftMeters;

        /// <summary>
        /// Instantaneous follower lift (displacement from base circle) in meters.
        /// </summary>
        public float FollowerLiftMeters
        {
            get
            {
                float theta = currentCamAngleDeg;
                float h = maxLiftMeters;

                if (profileType == CamProfileType.SimpleHarmonicPear)
                {
                    // Harmonic Cam profile:
                    // 0° - 60°: Dwell at base circle (s = 0)
                    // 60° - 180° (span 120°): Simple Harmonic Rise
                    // 180° - 220° (span 40°): Dwell at peak lift (s = h)
                    // 220° - 320° (span 100°): Simple Harmonic Return
                    // 320° - 360°: Dwell at base circle (s = 0)
                    if (theta < 60f) return 0f;
                    if (theta <= 180f)
                    {
                        float frac = (theta - 60f) / 120f;
                        return (h * 0.5f) * (1f - Mathf.Cos(frac * Mathf.PI));
                    }
                    if (theta <= 220f) return h;
                    if (theta <= 320f)
                    {
                        float frac = (theta - 220f) / 100f;
                        return (h * 0.5f) * (1f + Mathf.Cos(frac * Mathf.PI));
                    }
                    return 0f;
                }
                else
                {
                    // Circular Eccentric Cam (center offset e = h / 2)
                    float rad = theta * Mathf.Deg2Rad;
                    float e = h * 0.5f;
                    return e * (1f - Mathf.Cos(rad));
                }
            }
        }

        /// <summary>
        /// Follower linear velocity in m/s.
        /// </summary>
        public float FollowerVelocityMps
        {
            get
            {
                float omega = InputAngularVelocityRadPerSec;
                float theta = currentCamAngleDeg;
                float h = maxLiftMeters;

                if (profileType == CamProfileType.SimpleHarmonicPear)
                {
                    if (theta >= 60f && theta <= 180f)
                    {
                        float frac = (theta - 60f) / 120f;
                        float beta = 120f * Mathf.Deg2Rad;
                        return (Mathf.PI * h / (2f * beta)) * Mathf.Sin(frac * Mathf.PI) * (omega / Mathf.Rad2Deg);
                    }
                    if (theta >= 220f && theta <= 320f)
                    {
                        float frac = (theta - 220f) / 100f;
                        float beta = 100f * Mathf.Deg2Rad;
                        return -(Mathf.PI * h / (2f * beta)) * Mathf.Sin(frac * Mathf.PI) * (omega / Mathf.Rad2Deg);
                    }
                    return 0f; // Dwell
                }
                else
                {
                    float rad = theta * Mathf.Deg2Rad;
                    return (h * 0.5f) * omega * Mathf.Sin(rad);
                }
            }
        }

        public string CurrentPhaseName
        {
            get
            {
                if (profileType == CamProfileType.CircularEccentric) return "Continuous Harmonic Rise/Fall";
                float theta = currentCamAngleDeg;
                if (theta < 60f || theta > 320f) return "Base Dwell (Lift = 0)";
                if (theta <= 180f) return "Simple Harmonic Rise";
                if (theta <= 220f) return "Top Dwell (Peak Lift)";
                return "Simple Harmonic Return";
            }
        }

        public override string MechanismName => "Cam and Follower";
        public override string CategoryTitle => "Rotary-to-Reciprocating Cam Drive";
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

            // 1. Bedplate
            GameObject bedGo = new GameObject("Machine_Bedplate");
            bedGo.transform.SetParent(transform, false);
            bedGo.transform.localPosition = new Vector3(0, -0.050f, 0);
            MeshFilter mfBed = bedGo.AddComponent<MeshFilter>();
            MeshRenderer mrBed = bedGo.AddComponent<MeshRenderer>();
            mfBed.mesh = GearMeshBuilder.BuildBasePlateMesh(0.18f, 0.14f, 0.020f);
            mrBed.material = CreateMaterial(new Color(0.18f, 0.19f, 0.22f), 0.6f, 0.35f);
            bedGo.AddComponent<BoxCollider>().size = new Vector3(0.18f, 0.020f, 0.14f);
            InspectableComponent inspBed = bedGo.AddComponent<InspectableComponent>();
            inspBed.SetMetadata("Precision Machine Bed",
                "Supports camshaft main bearings and rigid vertical follower guide column.",
                "Material: GG-25 Cast Iron | Surface Ground");
            inspBed.SetExplosionVector(Vector3.down, 0.08f, 0.12f);

            // 2. Camshaft Pillow Blocks (Front & Back)
            float[] zPBs = new float[] { -0.035f, 0.035f };
            for (int i = 0; i < 2; i++)
            {
                GameObject pbGo = new GameObject($"Cam_PillowBlock_{i}");
                pbGo.transform.SetParent(transform, false);
                pbGo.transform.localPosition = new Vector3(0, -0.022f, zPBs[i]);
                MeshFilter mf = pbGo.AddComponent<MeshFilter>();
                MeshRenderer mr = pbGo.AddComponent<MeshRenderer>();
                mf.mesh = GearMeshBuilder.BuildPillowBlockMesh(0.030f, 0.046f, 0.018f, 0.007f);
                mr.material = CreateMaterial(new Color(0.25f, 0.27f, 0.30f), 0.5f, 0.35f);
                pbGo.AddComponent<BoxCollider>().size = new Vector3(0.030f, 0.046f, 0.018f);
            }

            // 3. Camshaft & Cam Rotor (Rotates around local Z at origin)
            GameObject camRotorGo = new GameObject("Camshaft_Rotor");
            camRotorGo.transform.SetParent(transform, false);
            camRotorGo.transform.localPosition = Vector3.zero;
            camshaftRotor = camRotorGo.transform;

            // Drive shaft
            GameObject shaftGo = new GameObject("Cam_Shaft");
            shaftGo.transform.SetParent(camshaftRotor, false);
            MeshFilter mfShaft = shaftGo.AddComponent<MeshFilter>();
            MeshRenderer mrShaft = shaftGo.AddComponent<MeshRenderer>();
            mfShaft.mesh = GearMeshBuilder.BuildCylinderMesh(0.0065f, 0.090f, 24, false);
            mrShaft.material = CreateMaterial(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.85f);
            shaftGo.AddComponent<CapsuleCollider>().height = 0.090f;

            // Cam Body
            GameObject camBody = new GameObject("Cam_Profile_Body");
            camBody.transform.SetParent(camshaftRotor, false);
            MeshFilter mfCam = camBody.AddComponent<MeshFilter>();
            MeshRenderer mrCam = camBody.AddComponent<MeshRenderer>();
            mfCam.mesh = GearMeshBuilder.BuildCamMesh(baseRadiusMeters, maxLiftMeters, camWidth, (int)profileType, 0.0065f);
            mrCam.material = CreateMaterial(new Color(0.92f, 0.72f, 0.15f), 0.7f, 0.65f); // Industrial Gold
            camBody.AddComponent<MeshCollider>().sharedMesh = mfCam.mesh;
            InspectableComponent inspCam = camBody.AddComponent<InspectableComponent>();
            inspCam.SetMetadata("Precision Cam Lobe",
                "Converts continuous uniform rotation into programmed reciprocating follower displacement with dwell intervals.",
                $"Base Radius: {baseRadiusMeters * 1000f:F1} mm | Max Lift: {maxLiftMeters * 1000f:F1} mm | Ground Hardened Alloy Steel");
            inspCam.SetExplosionVector(Vector3.back, 0.07f, 0.11f);

            // 4. Vertical Follower Guide Sleeve (Grounded above camshaft)
            float guideCenterY = baseRadiusMeters + rollerRadius + 0.060f;
            GameObject guideGo = new GameObject("Follower_Guide_Sleeve");
            guideGo.transform.SetParent(transform, false);
            guideGo.transform.localPosition = new Vector3(0, guideCenterY, 0);
            MeshFilter mfGuide = guideGo.AddComponent<MeshFilter>();
            MeshRenderer mrGuide = guideGo.AddComponent<MeshRenderer>();
            mfGuide.mesh = GearMeshBuilder.BuildCylinderMesh(0.012f, 0.055f, 24, false);
            mrGuide.material = CreateMaterial(new Color(0.28f, 0.30f, 0.35f), 0.5f, 0.4f);
            guideGo.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Cylinder along Y
            guideGo.AddComponent<CapsuleCollider>().height = 0.055f;
            InspectableComponent inspGuide = guideGo.AddComponent<InspectableComponent>();
            inspGuide.SetMetadata("Follower Guide Bushing",
                "Constrains follower stem to pure vertical translation without lateral deflection.",
                "Bushing: Sintered Bronze SAE 841 | Self-Lubricating");
            inspGuide.SetExplosionVector(Vector3.forward, 0.08f, 0.12f);

            // 5. Reciprocating Roller Follower
            GameObject followerGo = new GameObject("Roller_Follower");
            followerGo.transform.SetParent(transform, false);
            followerTransform = followerGo.transform;

            // Follower Stem (Sliding rod)
            GameObject stemGo = new GameObject("Follower_Stem");
            stemGo.transform.SetParent(followerTransform, false);
            stemGo.transform.localPosition = new Vector3(0, 0.050f, 0);
            MeshFilter mfStem = stemGo.AddComponent<MeshFilter>();
            MeshRenderer mrStem = stemGo.AddComponent<MeshRenderer>();
            mfStem.mesh = GearMeshBuilder.BuildCylinderMesh(0.0055f, 0.090f, 20, false);
            mrStem.material = CreateMaterial(new Color(0.20f, 0.55f, 0.90f), 0.7f, 0.65f); // Precision Blue
            stemGo.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            stemGo.AddComponent<CapsuleCollider>().height = 0.090f;
            InspectableComponent inspFollower = stemGo.AddComponent<InspectableComponent>();
            inspFollower.SetMetadata("Reciprocating Roller Follower",
                "Rolls along cam surface with low friction rolling contact, translating vertically according to the cam profile law.",
                $"Max Travel: {maxLiftMeters * 1000f:F1} mm | Hardened 52100 Bearing Steel");
            inspFollower.SetExplosionVector(Vector3.up, 0.08f, 0.12f);

            // Roller Wheel (mounted at bottom of stem)
            GameObject rollerGo = new GameObject("Roller_Wheel");
            rollerGo.transform.SetParent(followerTransform, false);
            rollerGo.transform.localPosition = Vector3.zero;
            rollerTransform = rollerGo.transform;
            MeshFilter mfRoller = rollerGo.AddComponent<MeshFilter>();
            MeshRenderer mrRoller = rollerGo.AddComponent<MeshRenderer>();
            mfRoller.mesh = GearMeshBuilder.BuildCylinderMesh(rollerRadius, 0.016f, 24, false);
            mrRoller.material = CreateMaterial(new Color(0.90f, 0.22f, 0.27f), 0.8f, 0.7f); // Crimson Accent
            rollerGo.AddComponent<CapsuleCollider>().height = 0.016f;

            // 6. Return Compression Spring (Surrounds stem between guide and follower shoulder)
            GameObject springGo = new GameObject("Return_Spring");
            springGo.transform.SetParent(followerTransform, false);
            springGo.transform.localPosition = new Vector3(0, 0.035f, 0);
            returnSpringTransform = springGo.transform;
            MeshFilter mfSpring = springGo.AddComponent<MeshFilter>();
            MeshRenderer mrSpring = springGo.AddComponent<MeshRenderer>();
            mfSpring.mesh = GearMeshBuilder.BuildCylinderMesh(0.009f, 0.040f, 16, false);
            mrSpring.material = CreateMaterial(new Color(0.60f, 0.62f, 0.65f), 0.7f, 0.4f);
            springGo.transform.localRotation = Quaternion.Euler(90f, 0, 0);

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
            currentCamAngleDeg += inputAngularVelDegPerSec * deltaTime;
            currentCamAngleDeg %= 360f;
            if (currentCamAngleDeg < 0f) currentCamAngleDeg += 360f;

            UpdateComponentTransforms();
        }

        private void UpdateComponentTransforms()
        {
            // 1. Camshaft Rotates around Z
            if (camshaftRotor != null)
            {
                camshaftRotor.localRotation = Quaternion.Euler(0f, 0f, currentCamAngleDeg);
            }

            // 2. Follower Translates Vertically along Y
            float lift = FollowerLiftMeters;
            float contactY = baseRadiusMeters + rollerRadius + lift;

            if (followerTransform != null)
            {
                followerTransform.localPosition = new Vector3(0f, contactY, 0f);
            }

            // 3. Roller wheel spins freely as it tracks cam periphery
            if (rollerTransform != null)
            {
                float rollerSpinDeg = (currentCamAngleDeg * (baseRadiusMeters / rollerRadius)) % 360f;
                rollerTransform.localRotation = Quaternion.Euler(0f, 0f, rollerSpinDeg);
            }
        }

        public override void ResetMechanism()
        {
            currentCamAngleDeg = 0f; // Start at Base Dwell
            UpdateComponentTransforms();
        }

        public override string GetCalculationSummary()
        {
            return $"• Profile Type: {profileType}\n" +
                   $"• Cam Angle (θ): {currentCamAngleDeg:F1}°\n" +
                   $"• Current Phase: {CurrentPhaseName}\n" +
                   $"• Follower Lift: {FollowerLiftMeters * 1000f:F1} mm / {maxLiftMeters * 1000f:F1} mm\n" +
                   $"• Follower Velocity: {FollowerVelocityMps:F2} m/s\n" +
                   $"• Base Radius (Rb): {baseRadiusMeters * 1000f:F1} mm\n" +
                   $"• Input Speed: {currentInputRPM:F0} RPM";
        }

        public override string GetEngineeringExplanation()
        {
            return "Cams transform rotary motion into precisely scheduled linear follower displacement. " +
                   "The Simple Harmonic Motion (SHM) profile ensures zero infinite jerk at the rise/return transitions, " +
                   "delivering smooth acceleration and extended dwell intervals impossible to achieve with circular eccentrics.";
        }

        public override string GetFormulaBreakdown()
        {
            return "Base Circle Radius: Rb = 35.0 mm | Max Lift: h = 18.0 mm\n" +
                   "Harmonic Rise (60°-180°): s(θ) = (h/2)·[1 - cos(π·(θ - 60°)/120°)]\n" +
                   "Top Dwell (180°-220°): s(θ) = h\n" +
                   "Harmonic Return (220°-320°): s(θ) = (h/2)·[1 + cos(π·(θ - 220°)/100°)]\n" +
                   $"Current Lift: s = {FollowerLiftMeters * 1000f:F1} mm | v = {FollowerVelocityMps:F2} m/s";
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MechXR.Mechanisms;
using MechXR.Interaction;

namespace MechXR.AR
{
    public enum ARPlacementState
    {
        Idle,
        SearchingForSurfaces,
        SurfaceDetected,
        ObjectPlaced
    }

    /// <summary>
    /// Coordinates markerless AR surface detection, reticle projection, and 3D gear assembly placement.
    /// Strictly requires real AR Foundation plane detection on Android devices.
    /// Desktop virtual desk fallback is isolated strictly to the Unity Editor for simulation.
    /// </summary>
    public class ARPlacementManager : MonoBehaviour
    {
        [Header("AR Foundation References")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARRaycastManager arRaycastManager;
        [SerializeField] private ARPlaneManager arPlaneManager;

        [Header("Placement Reticle")]
        [SerializeField] private PlacementReticle placementReticle;

        [Header("Mechanism Prefabs / Prototypes")]
        [SerializeField] private GameObject spurGearPrefab;
        [SerializeField] private GameObject bevelGearPrefab;
        [SerializeField] private GameObject wormGearPrefab;
        [SerializeField] private GameObject planetaryGearPrefab;
        [SerializeField] private GameObject rackAndPinionPrefab;
        [SerializeField] private GameObject beltAndPulleyPrefab;

        private ARPlacementState currentState = ARPlacementState.Idle;
        private Pose currentPlacementPose;
        private bool hasValidPlacementPose = false;
        private bool isScanningActive = false;
        private GameObject currentPlacedInstance;
        private MechanismBase currentMechanism;
        private ModelInteractionController currentInteractionController;
        private int selectedMechanismIndex = 0; // 0: Spur, 1: Bevel, 2: Worm

        private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        public event Action<ARPlacementState> OnPlacementStateChanged;
        public event Action<MechanismBase> OnMechanismPlaced;
        public event Action OnMechanismRemoved;

        public ARPlacementState CurrentState => currentState;
        public MechanismBase CurrentMechanism => currentMechanism;
        public bool HasPlacedObject => currentPlacedInstance != null;
        public bool IsScanningActive => isScanningActive;
        public int SelectedMechanismIndex => selectedMechanismIndex;

        private void Awake()
        {
            if (arRaycastManager == null)
            {
                arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
            }
            if (arPlaneManager == null)
            {
                arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
            }
            if (arSession == null)
            {
                arSession = FindFirstObjectByType<ARSession>();
            }

            if (placementReticle == null)
            {
                placementReticle = FindFirstObjectByType<PlacementReticle>();
            }
        }

        private void Start()
        {
            // Start in Idle state while on Menu screen; do NOT scan until user selects a mechanism
            SetState(ARPlacementState.Idle);
            if (placementReticle != null)
            {
                placementReticle.SetVisible(false);
            }
        }

        private void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }

        public void StartScanning(int mechanismIndex)
        {
            selectedMechanismIndex = Mathf.Clamp(mechanismIndex, 0, 25);
            isScanningActive = true;
            hasValidPlacementPose = false;

            if (currentPlacedInstance != null)
            {
                SafeDestroy(currentPlacedInstance);
                currentPlacedInstance = null;
                currentMechanism = null;
                currentInteractionController = null;
            }

            SetState(ARPlacementState.SearchingForSurfaces);
            if (placementReticle != null)
            {
                placementReticle.SetVisible(false);
            }
        }

        public void StopScanning()
        {
            isScanningActive = false;
            hasValidPlacementPose = false;

            if (currentPlacedInstance != null)
            {
                SafeDestroy(currentPlacedInstance);
                currentPlacedInstance = null;
                currentMechanism = null;
                currentInteractionController = null;
            }

            if (placementReticle != null)
            {
                placementReticle.SetVisible(false);
            }

            SetState(ARPlacementState.Idle);
            OnMechanismRemoved?.Invoke();
        }

        private void Update()
        {
            // Do nothing if not in scanning mode or if model is already placed
            if (!isScanningActive || currentState == ARPlacementState.ObjectPlaced)
            {
                if (placementReticle != null)
                {
                    placementReticle.SetVisible(false);
                }
                return;
            }

            UpdatePlacementPose();

            if (hasValidPlacementPose)
            {
                if (currentState != ARPlacementState.SurfaceDetected)
                {
                    SetState(ARPlacementState.SurfaceDetected);
                }

                if (placementReticle != null)
                {
                    placementReticle.SetVisible(true);
                    placementReticle.transform.SetPositionAndRotation(currentPlacementPose.position, currentPlacementPose.rotation);
                }

                // Check for user tap to place
                CheckUserTapToPlace();
            }
            else
            {
                if (currentState != ARPlacementState.SearchingForSurfaces)
                {
                    SetState(ARPlacementState.SearchingForSurfaces);
                }

                if (placementReticle != null)
                {
                    placementReticle.SetVisible(false);
                }
            }
        }

        private void UpdatePlacementPose()
        {
            hasValidPlacementPose = false;
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // 1. Physical AR Raycast via ARCore (Only on device / when raycast manager is active)
            if (arRaycastManager != null && arRaycastManager.enabled)
            {
                if (arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    currentPlacementPose = s_Hits[0].pose;
                    hasValidPlacementPose = true;
                    return;
                }
            }

#if UNITY_EDITOR
            // 2. Desktop Editor Simulator Fallback ONLY (Never executed on Android)
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane virtualDesk = new Plane(Vector3.up, new Vector3(0, -0.3f, 0));
            if (virtualDesk.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 cameraForward = cam.transform.forward;
                cameraForward.y = 0;
                Quaternion targetRot = Quaternion.LookRotation(cameraForward != Vector3.zero ? cameraForward : Vector3.forward);

                currentPlacementPose = new Pose(hitPoint, targetRot);
                hasValidPlacementPose = true;
            }
#endif
        }

        private void CheckUserTapToPlace()
        {
            // Ignore touches that hit any UI elements
            if (EventSystem.current != null)
            {
                if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;
                if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
                    return;
            }

            bool tapTriggered = false;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    tapTriggered = true;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tapTriggered = true;
            }

            if (tapTriggered && hasValidPlacementPose)
            {
                PlaceSelectedMechanism();
            }
        }

        public void PlaceSelectedMechanism()
        {
            if (currentPlacedInstance != null)
            {
                SafeDestroy(currentPlacedInstance);
            }

            GameObject instance = null;

            switch (selectedMechanismIndex)
            {
                case 0:
                    if (spurGearPrefab != null) instance = Instantiate(spurGearPrefab);
                    else
                    {
                        instance = new GameObject("SpurGearPair_Assembly");
                        instance.AddComponent<SpurGearPair>().BuildMechanismAssembly();
                    }
                    break;

                case 1:
                    if (bevelGearPrefab != null) instance = Instantiate(bevelGearPrefab);
                    else
                    {
                        instance = new GameObject("BevelGearPair_Assembly");
                        instance.AddComponent<BevelGearPair>().BuildMechanismAssembly();
                    }
                    break;

                case 2:
                    if (wormGearPrefab != null) instance = Instantiate(wormGearPrefab);
                    else
                    {
                        instance = new GameObject("WormGearPair_Assembly");
                        instance.AddComponent<WormGearPair>().BuildMechanismAssembly();
                    }
                    break;

                case 3:
                    if (planetaryGearPrefab != null) instance = Instantiate(planetaryGearPrefab);
                    else
                    {
                        instance = new GameObject("PlanetaryGearSystem_Assembly");
                        instance.AddComponent<PlanetaryGearSystem>().BuildMechanismAssembly();
                    }
                    break;

                case 4:
                    if (rackAndPinionPrefab != null) instance = Instantiate(rackAndPinionPrefab);
                    else
                    {
                        instance = new GameObject("RackAndPinionPair_Assembly");
                        instance.AddComponent<RackAndPinionPair>().BuildMechanismAssembly();
                    }
                    break;

                case 5:
                    if (beltAndPulleyPrefab != null) instance = Instantiate(beltAndPulleyPrefab);
                    else
                    {
                        instance = new GameObject("BeltAndPulleySystem_Assembly");
                        instance.AddComponent<BeltAndPulleySystem>().BuildMechanismAssembly();
                    }
                    break;

                case 6:
                    instance = new GameObject("SimpleGearTrain_Assembly");
                    instance.AddComponent<SimpleGearTrain>().BuildMechanismAssembly();
                    break;

                case 7:
                    instance = new GameObject("CompoundGearTrain_Assembly");
                    instance.AddComponent<CompoundGearTrain>().BuildMechanismAssembly();
                    break;

                case 8:
                    instance = new GameObject("RevertedGearTrain_Assembly");
                    instance.AddComponent<RevertedGearTrain>().BuildMechanismAssembly();
                    break;

                case 9:
                    instance = new GameObject("InternalGearPair_Assembly");
                    instance.AddComponent<InternalGearPair>().BuildMechanismAssembly();
                    break;

                case 10:
                    instance = new GameObject("HelicalGearPair_Assembly");
                    instance.AddComponent<HelicalGearPair>().BuildMechanismAssembly();
                    break;

                case 11:
                    instance = new GameObject("HerringboneGearPair_Assembly");
                    instance.AddComponent<HerringboneGearPair>().BuildMechanismAssembly();
                    break;

                case 12:
                    instance = new GameObject("AutomotiveDifferential_Assembly");
                    instance.AddComponent<AutomotiveDifferential>().BuildMechanismAssembly();
                    break;

                case 13:
                    instance = new GameObject("HarmonicDrive_Assembly");
                    instance.AddComponent<HarmonicDrive>().BuildMechanismAssembly();
                    break;

                case 14:
                    instance = new GameObject("FourStrokeEngine_Assembly");
                    instance.AddComponent<FourStrokeEngine>().BuildMechanismAssembly();
                    break;

                case 15:
                    instance = new GameObject("SliderCrankMechanism_Assembly");
                    instance.AddComponent<SliderCrankMechanism>().BuildMechanismAssembly();
                    break;

                case 16:
                    instance = new GameObject("CamAndFollower_Assembly");
                    instance.AddComponent<CamAndFollower>().BuildMechanismAssembly();
                    break;

                case 17:
                    instance = new GameObject("ReciprocatingPump_Assembly");
                    instance.AddComponent<ReciprocatingPump>().BuildMechanismAssembly();
                    break;

                case 18:
                    instance = new GameObject("FourBarLinkage_Assembly");
                    instance.AddComponent<FourBarLinkage>().BuildMechanismAssembly();
                    break;

                case 19:
                    instance = new GameObject("CrankRockerMechanism_Assembly");
                    instance.AddComponent<CrankRockerMechanism>().BuildMechanismAssembly();
                    break;

                case 20:
                    instance = new GameObject("GenevaMechanism_Assembly");
                    instance.AddComponent<GenevaMechanism>().BuildMechanismAssembly();
                    break;

                case 21:
                    instance = new GameObject("ScotchYokeMechanism_Assembly");
                    instance.AddComponent<ScotchYokeMechanism>().BuildMechanismAssembly();
                    break;

                case 22:
                    instance = new GameObject("UniversalJoint_Assembly");
                    instance.AddComponent<UniversalJoint>().BuildMechanismAssembly();
                    break;

                case 23:
                    instance = new GameObject("ChainAndSprocketDrive_Assembly");
                    instance.AddComponent<ChainAndSprocketDrive>().BuildMechanismAssembly();
                    break;

                case 24:
                    instance = new GameObject("RatchetAndPawl_Assembly");
                    instance.AddComponent<RatchetAndPawl>().BuildMechanismAssembly();
                    break;

                case 25:
                    instance = new GameObject("LeadScrewAndNut_Assembly");
                    instance.AddComponent<LeadScrewAndNut>().BuildMechanismAssembly();
                    break;
            }

            if (instance != null)
            {
                instance.transform.SetPositionAndRotation(currentPlacementPose.position, currentPlacementPose.rotation);
                instance.transform.localScale = Vector3.one;

                currentInteractionController = instance.GetComponent<ModelInteractionController>();
                if (currentInteractionController == null)
                {
                    currentInteractionController = instance.AddComponent<ModelInteractionController>();
                }

                currentPlacedInstance = instance;
                currentMechanism = instance.GetComponent<MechanismBase>();

                isScanningActive = false;
                if (placementReticle != null)
                {
                    placementReticle.SetVisible(false);
                }

                SetState(ARPlacementState.ObjectPlaced);
                OnMechanismPlaced?.Invoke(currentMechanism);
            }
        }

        public void RepositionMechanism()
        {
            if (currentPlacedInstance != null)
            {
                SafeDestroy(currentPlacedInstance);
                currentPlacedInstance = null;
                currentMechanism = null;
                currentInteractionController = null;
            }

            isScanningActive = true;
            hasValidPlacementPose = false;
            if (placementReticle != null)
            {
                placementReticle.SetVisible(false);
            }

            SetState(ARPlacementState.SearchingForSurfaces);
            OnMechanismRemoved?.Invoke();
        }

        public void RemovePlacedMechanism()
        {
            StopScanning();
        }

        private void SetState(ARPlacementState state)
        {
            currentState = state;
            OnPlacementStateChanged?.Invoke(currentState);
        }
    }
}

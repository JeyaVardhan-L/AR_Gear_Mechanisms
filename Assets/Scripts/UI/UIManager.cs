using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MechXR.AR;
using MechXR.Mechanisms;
using MechXR.Interaction;

namespace MechXR.UI
{
    public enum AppScreenState
    {
        Library,
        ARScanning,
        ARPlaced,
        ComponentInspection,
        ExplodedView
    }

    /// <summary>
    /// Master UI Manager for MECH-XR Phase 5: Scientific Experiment Lab, Control Repair & Mechanism Expansion.
    /// Implements:
    /// 1. Reliable input velocity controls (slider + steppers + presets) with immediate angular speed updates.
    /// 2. Natural two-finger pinch-to-zoom scaling with transient floating scale indicator pill (scale slider removed).
    /// 3. Collapsible Scientific Experiment Lab with parameter modulation, SI calculations, live dynamic graphs, presets, and A/B comparison.
    /// 4. 14 CAD-grade mechanisms in a scrollable Library catalog.
    /// 5. macOS-inspired graphite/void/white/crimson aesthetic.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private ARPlacementManager placementManager;

        [Header("Typography Fonts")]
        [SerializeField] private TMP_FontAsset fontRegular;
        [SerializeField] private TMP_FontAsset fontSemiBold;

        [Header("Screen State Hierarchy")]
        [SerializeField] private GameObject safeAreaRoot;
        [SerializeField] private GameObject libraryScreen;
        [SerializeField] private ScrollRect libraryScrollRect;
        [SerializeField] private RectTransform libraryContentRect;
        [SerializeField] private GameObject arHudScreen;
        [SerializeField] private GameObject arTopBar;
        [SerializeField] private GameObject arBottomBar;
        [SerializeField] private GameObject scanningPanel;
        [SerializeField] private TextMeshProUGUI scanningPromptText;
        [SerializeField] private GameObject statusPill;
        [SerializeField] private TextMeshProUGUI statusIndicatorText;
        [SerializeField] private GameObject inspectionCard;
        [SerializeField] private GameObject calculationsCard;
        [SerializeField] private GameObject experimentLabPanel;
        [SerializeField] private GameObject scalePill;
        [SerializeField] private TextMeshProUGUI scalePillText;
        [SerializeField] private GameObject diagnosticPanel;

        [Header("AR HUD Controls")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI headerSubtitleText;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Button repositionButton;

        [SerializeField] private Button playPauseButton;
        [SerializeField] private TextMeshProUGUI playPauseButtonText;
        [SerializeField] private Button reverseButton;
        [SerializeField] private Button explodeButton;
        [SerializeField] private TextMeshProUGUI explodeButtonText;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button toggleLabButton;
        [SerializeField] private Button toggleCalculationsButton;

        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI speedValueText;
        [SerializeField] private Button btnMinus10;
        [SerializeField] private Button btnPlus10;

        [Header("Inspection Card Controls")]
        [SerializeField] private TextMeshProUGUI inspectTitleText;
        [SerializeField] private TextMeshProUGUI inspectRoleText;
        [SerializeField] private TextMeshProUGUI inspectSpecsText;
        [SerializeField] private Button inspectDetachButton;
        [SerializeField] private TextMeshProUGUI inspectDetachButtonText;
        [SerializeField] private Button inspectCloseButton;

        [Header("Calculations Card Controls")]
        [SerializeField] private TextMeshProUGUI calculationsContentText;
        [SerializeField] private TextMeshProUGUI formulasContentText;

        [Header("Experiment Lab Controls")]
        [SerializeField] private Slider expSpeedSlider;
        [SerializeField] private TextMeshProUGUI expSpeedValueText;
        [SerializeField] private Slider expTorqueSlider;
        [SerializeField] private TextMeshProUGUI expTorqueValueText;
        [SerializeField] private Slider expEfficiencySlider;
        [SerializeField] private TextMeshProUGUI expEfficiencyValueText;
        [SerializeField] private Slider expTurnBiasSlider;
        [SerializeField] private TextMeshProUGUI expTurnBiasValueText;
        [SerializeField] private TextMeshProUGUI expTelemetryText;
        [SerializeField] private Button captureExpAButton;
        [SerializeField] private TextMeshProUGUI comparisonText;

        // Dynamic Graph Elements
        [SerializeField] private RectTransform graphBarInput;
        [SerializeField] private RectTransform graphBarOutput;
        [SerializeField] private RectTransform graphBarLoss;
        [SerializeField] private TextMeshProUGUI graphBarInputLabel;
        [SerializeField] private TextMeshProUGUI graphBarOutputLabel;
        [SerializeField] private TextMeshProUGUI graphBarLossLabel;
        [SerializeField] private TextMeshProUGUI graphTitleText;
        private int currentGraphTab = 0; // 0: RPM, 1: Torque, 2: Speed/Time, 3: Power/Loss
        private readonly List<float> speedHistory = new List<float>();

        // A/B Experiment Comparison Snapshot
        private struct ExperimentSnapshot
        {
            public float inputRPM;
            public float torque;
            public float efficiency;
            public float outputRPM;
            public float outputTorque;
            public float outputPower;
            public float powerLoss;
            public bool isCaptured;
        }
        private ExperimentSnapshot snapshotA;

        [Header("Library Cards & Buttons (26 Mechanisms)")]
        [SerializeField] private Button[] cardLaunchButtons = new Button[26];
        [SerializeField] private GameObject[] libraryCards = new GameObject[26];

        private Button btnCatAll;
        private Button btnCatGears;
        private Button btnCatEngines;
        private Button btnCatLinkages;
        private Button btnCatDrives;
        private int currentSelectedCategory = 0; // 0: All, 1: Gears, 2: Engines, 3: Linkages, 4: Drives

        private AppScreenState currentScreenState = AppScreenState.Library;
        private MechanismBase activeMechanism;
        private InspectableComponent currentlyInspectedComponent;
        private bool isCalculationsVisible = false;
        private bool isLabVisible = false;
        private bool isUIBuilt = false;
        private int selectedMechanismIndex = 0;
        private float scalePillTimer = 0f;

        // macOS Engineering Laboratory Color Palette
        private readonly Color colBackground = new Color(0.067f, 0.071f, 0.078f, 1.0f);     // #111214
        private readonly Color colSurfaceCard = new Color(0.110f, 0.114f, 0.125f, 0.96f);    // #1C1D20
        private readonly Color colElevatedPanel = new Color(0.161f, 0.169f, 0.188f, 0.98f);  // #292B30
        private readonly Color colBorderSubtle = new Color(0.227f, 0.239f, 0.271f, 1.0f);   // #3A3D45
        private readonly Color colTextPrimary = new Color(0.961f, 0.961f, 0.961f, 1.0f);    // #F5F5F5
        private readonly Color colTextSecondary = new Color(0.631f, 0.631f, 0.667f, 1.0f);  // #A1A1AA
        private readonly Color colAccentCrimson = new Color(0.898f, 0.224f, 0.271f, 1.0f);  // #E53945
        private readonly Color colAccentEmerald = new Color(0.204f, 0.827f, 0.600f, 1.0f);  // #34D399
        private readonly Color colButtonNormal = new Color(0.137f, 0.145f, 0.165f, 1.0f);   // #23252A

        public AppScreenState CurrentScreenState => currentScreenState;
        public GameObject LibraryScreen => libraryScreen;
        public GameObject ARHudScreen => arHudScreen;
        public GameObject ARTopBar => arTopBar;
        public GameObject ARBottomBar => arBottomBar;
        public GameObject ScanningPanel => scanningPanel;
        public GameObject InspectionCard => inspectionCard;
        public GameObject CalculationsCard => calculationsCard;
        public GameObject ExperimentLabPanel => experimentLabPanel;
        public TextMeshProUGUI HeaderTitleText => headerTitleText;
        public TextMeshProUGUI ScanningPromptText => scanningPromptText;
        public TextMeshProUGUI StatusIndicatorText => statusIndicatorText;
        public TextMeshProUGUI ExplodeButtonText => explodeButtonText;
        public Button[] CardLaunchButtons => cardLaunchButtons;
        public GameObject[] LibraryCards => libraryCards;

        public TMP_FontAsset FontRegular => fontRegular;
        public TMP_FontAsset FontSemiBold => fontSemiBold;
        public ScrollRect LibraryScrollRect => libraryScrollRect;
        public RectTransform LibraryContentRect => libraryContentRect;

        private void Awake()
        {
            if (placementManager == null)
            {
                placementManager = FindFirstObjectByType<ARPlacementManager>();
            }

            ResolveFonts();

            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                // Optimize touch drag threshold for high-DPI Android displays (POCO X6 5G @ 446 DPI)
                UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = Mathf.Max(16, (int)(Screen.dpi / 20f));
            }

            // 1. Check if hierarchy is already serialized and assigned in scene
            if (libraryScreen != null && arHudScreen != null && libraryScreen.transform.parent != null)
            {
                isUIBuilt = true;
                BindButtonListeners();
                SetScreenState(AppScreenState.Library);
                return;
            }

            // 2. Check if a child SafeAreaContainer already exists from serialized scene
            Canvas canvas = GetComponentInParent<Canvas>();
            GameObject canvasGo = canvas != null ? canvas.gameObject : gameObject;
            Transform existingSafe = canvasGo.transform.Find("SafeAreaContainer");
            if (existingSafe != null)
            {
                BindExistingHierarchy(existingSafe.gameObject);
                isUIBuilt = true;
                BindButtonListeners();
                SetScreenState(AppScreenState.Library);
                return;
            }

            // 3. Otherwise build fresh
            BuildUIHierarchy();
            SetScreenState(AppScreenState.Library);
        }

        public void ResolveFonts()
        {
            if (fontRegular == null)
            {
                fontRegular = Resources.Load<TMP_FontAsset>("Fonts & Materials/Inter-Regular SDF");
                if (fontRegular == null)
                {
                    fontRegular = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                }
                if (fontRegular == null && TMP_Settings.defaultFontAsset != null)
                {
                    fontRegular = TMP_Settings.defaultFontAsset;
                }
            }

            if (fontSemiBold == null)
            {
                fontSemiBold = Resources.Load<TMP_FontAsset>("Fonts & Materials/Inter-SemiBold SDF") ?? fontRegular;
            }
        }

        private void OnEnable()
        {
            InitializeEventBindings();
        }

        public void InitializeEventBindings()
        {
            if (placementManager == null)
            {
                placementManager = FindFirstObjectByType<ARPlacementManager>();
            }

            if (placementManager != null)
            {
                placementManager.OnPlacementStateChanged -= HandlePlacementStateChanged;
                placementManager.OnMechanismPlaced -= HandleMechanismPlaced;
                placementManager.OnMechanismRemoved -= HandleMechanismRemoved;

                placementManager.OnPlacementStateChanged += HandlePlacementStateChanged;
                placementManager.OnMechanismPlaced += HandleMechanismPlaced;
                placementManager.OnMechanismRemoved += HandleMechanismRemoved;
            }
        }

        private void OnDisable()
        {
            if (placementManager != null)
            {
                placementManager.OnPlacementStateChanged -= HandlePlacementStateChanged;
                placementManager.OnMechanismPlaced -= HandleMechanismPlaced;
                placementManager.OnMechanismRemoved -= HandleMechanismRemoved;
            }

            if (activeMechanism != null)
            {
                activeMechanism.OnKinematicsUpdated -= UpdateCalculationsDisplay;
                activeMechanism.OnExplodedViewChanged -= HandleExplodedViewChanged;
            }
        }

        private void Start()
        {
            SetScreenState(AppScreenState.Library);
        }

        private void Update()
        {
            // Transient floating scale indicator pill fade out
            if (scalePillTimer > 0f)
            {
                scalePillTimer -= Time.deltaTime;
                if (scalePillTimer <= 0f && scalePill != null)
                {
                    scalePill.SetActive(false);
                }
            }

            // Update live graph telemetry if experiment lab is open
            if (isLabVisible && activeMechanism != null)
            {
                UpdateGraphTelemetry();
            }
        }

        #region Screen State Controller (Priority 1)

        public void SetScreenState(AppScreenState newState)
        {
            currentScreenState = newState;

            switch (newState)
            {
                case AppScreenState.Library:
                    if (libraryScreen != null) libraryScreen.SetActive(true);
                    if (arHudScreen != null) arHudScreen.SetActive(false);
                    if (arTopBar != null) arTopBar.SetActive(false);
                    if (scanningPanel != null) scanningPanel.SetActive(false);
                    if (arBottomBar != null) arBottomBar.SetActive(false);
                    if (inspectionCard != null) inspectionCard.SetActive(false);
                    if (calculationsCard != null) calculationsCard.SetActive(false);
                    if (experimentLabPanel != null) experimentLabPanel.SetActive(false);
                    if (statusPill != null) statusPill.SetActive(false);
                    if (scalePill != null) scalePill.SetActive(false);
                    isLabVisible = false;
                    isCalculationsVisible = false;

                    if (placementManager != null)
                    {
                        placementManager.StopScanning();
                    }

                    if (libraryScrollRect != null)
                    {
                        if (libraryContentRect != null)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect);
                        }
                        libraryScrollRect.StopMovement();
                    }
                    break;

                case AppScreenState.ARScanning:
                    if (libraryScreen != null) libraryScreen.SetActive(false);
                    if (arHudScreen != null) arHudScreen.SetActive(true);
                    if (arTopBar != null) arTopBar.SetActive(true);
                    if (repositionButton != null) repositionButton.gameObject.SetActive(false);
                    if (statusPill != null)
                    {
                        statusPill.SetActive(true);
                        if (statusIndicatorText != null) statusIndicatorText.text = "SCANNING SURFACE";
                    }
                    if (scanningPanel != null)
                    {
                        scanningPanel.SetActive(true);
                        if (scanningPromptText != null)
                            scanningPromptText.text = "Slowly pan device across table or desk to detect flat surfaces";
                    }
                    if (arBottomBar != null) arBottomBar.SetActive(false);
                    if (inspectionCard != null) inspectionCard.SetActive(false);
                    if (calculationsCard != null) calculationsCard.SetActive(false);
                    if (experimentLabPanel != null) experimentLabPanel.SetActive(false);
                    if (scalePill != null) scalePill.SetActive(false);
                    isLabVisible = false;
                    isCalculationsVisible = false;
                    break;

                case AppScreenState.ARPlaced:
                    if (libraryScreen != null) libraryScreen.SetActive(false);
                    if (arHudScreen != null) arHudScreen.SetActive(true);
                    if (arTopBar != null) arTopBar.SetActive(true);
                    if (repositionButton != null) repositionButton.gameObject.SetActive(true);
                    if (statusPill != null)
                    {
                        statusPill.SetActive(true);
                        if (statusIndicatorText != null) statusIndicatorText.text = "AR TRACKING ACTIVE";
                    }
                    if (scanningPanel != null) scanningPanel.SetActive(false);
                    if (arBottomBar != null) arBottomBar.SetActive(true);
                    if (inspectionCard != null) inspectionCard.SetActive(false);
                    if (calculationsCard != null) calculationsCard.SetActive(false);
                    UpdateExplodeButtonState();
                    UpdatePlayPauseButtonState();
                    break;

                case AppScreenState.ComponentInspection:
                    if (libraryScreen != null) libraryScreen.SetActive(false);
                    if (arHudScreen != null) arHudScreen.SetActive(true);
                    if (arTopBar != null) arTopBar.SetActive(true);
                    if (repositionButton != null) repositionButton.gameObject.SetActive(true);
                    if (statusPill != null) statusPill.SetActive(false);
                    if (scanningPanel != null) scanningPanel.SetActive(false);
                    if (arBottomBar != null) arBottomBar.SetActive(false);
                    if (inspectionCard != null) inspectionCard.SetActive(true);
                    if (calculationsCard != null) calculationsCard.SetActive(false);
                    if (experimentLabPanel != null) experimentLabPanel.SetActive(false);
                    isLabVisible = false;
                    break;

                case AppScreenState.ExplodedView:
                    if (libraryScreen != null) libraryScreen.SetActive(false);
                    if (arHudScreen != null) arHudScreen.SetActive(true);
                    if (arTopBar != null) arTopBar.SetActive(true);
                    if (repositionButton != null) repositionButton.gameObject.SetActive(true);
                    if (statusPill != null) statusPill.SetActive(true);
                    if (scanningPanel != null) scanningPanel.SetActive(false);
                    if (arBottomBar != null) arBottomBar.SetActive(true);
                    if (inspectionCard != null) inspectionCard.SetActive(false);
                    if (calculationsCard != null) calculationsCard.SetActive(false);
                    UpdateExplodeButtonState();
                    break;
            }
        }

        public void ShowMenuScreen() => SetScreenState(AppScreenState.Library);
        public void ShowARHudScreen() => SetScreenState(AppScreenState.ARScanning);
        public void OnReturnToLibrary() => SetScreenState(AppScreenState.Library);

        public void OnReplaceMechanism()
        {
            SetScreenState(AppScreenState.ARScanning);
            if (placementManager != null) placementManager.RepositionMechanism();
        }

        public void FilterLibraryCategory(int cat)
        {
            currentSelectedCategory = cat;

            SetButtonActiveStyle(btnCatAll, cat == 0);
            SetButtonActiveStyle(btnCatGears, cat == 1);
            SetButtonActiveStyle(btnCatEngines, cat == 2);
            SetButtonActiveStyle(btnCatLinkages, cat == 3);
            SetButtonActiveStyle(btnCatDrives, cat == 4);

            if (libraryCards != null)
            {
                for (int i = 0; i < libraryCards.Length; i++)
                {
                    if (libraryCards[i] == null) continue;
                    bool visible = false;
                    if (cat == 0) visible = true;
                    else if (cat == 1 && i <= 13) visible = true;
                    else if (cat == 2 && i >= 14 && i <= 17) visible = true;
                    else if (cat == 3 && i >= 18 && i <= 21) visible = true;
                    else if (cat == 4 && i >= 22 && i <= 25) visible = true;

                    libraryCards[i].SetActive(visible);
                }
            }

            if (libraryScrollRect != null)
            {
                if (libraryContentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect);
                }
                Canvas.ForceUpdateCanvases();
                libraryScrollRect.StopMovement();
                libraryScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void SetButtonActiveStyle(Button btn, bool isActive)
        {
            if (btn == null) return;
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? colAccentCrimson : colButtonNormal;
            }
        }

        public void OnSelectMechanismFromMenu(int mechanismIndex)
        {
            selectedMechanismIndex = Mathf.Clamp(mechanismIndex, 0, 25);

            if (headerTitleText != null) headerTitleText.text = GetMechanismName(selectedMechanismIndex);
            if (headerSubtitleText != null) headerSubtitleText.text = "POINT CAMERA AT HORIZONTAL SURFACE";

            SetScreenState(AppScreenState.ARScanning);

            if (placementManager != null)
            {
                placementManager.StartScanning(selectedMechanismIndex);
            }
        }

        public static string GetMechanismName(int index)
        {
            switch (index)
            {
                case 0: return "SPUR GEAR PAIR";
                case 1: return "BEVEL GEAR PAIR";
                case 2: return "WORM & WHEEL DRIVE";
                case 3: return "PLANETARY GEAR SYSTEM";
                case 4: return "RACK AND PINION";
                case 5: return "BELT AND PULLEY SYSTEM";
                case 6: return "SIMPLE GEAR TRAIN";
                case 7: return "COMPOUND GEAR TRAIN";
                case 8: return "REVERTED GEAR TRAIN";
                case 9: return "INTERNAL GEAR PAIR";
                case 10: return "HELICAL GEAR PAIR";
                case 11: return "HERRINGBONE GEAR PAIR";
                case 12: return "AUTOMOTIVE DIFFERENTIAL";
                case 13: return "HARMONIC DRIVE";
                case 14: return "FOUR-STROKE IC ENGINE";
                case 15: return "SLIDER-CRANK MECHANISM";
                case 16: return "CAM AND FOLLOWER";
                case 17: return "RECIPROCATING PUMP";
                case 18: return "FOUR-BAR LINKAGE";
                case 19: return "CRANK-ROCKER MECHANISM";
                case 20: return "GENEVA MECHANISM";
                case 21: return "SCOTCH YOKE MECHANISM";
                case 22: return "UNIVERSAL JOINT (CARDAN)";
                case 23: return "CHAIN & SPROCKET DRIVE";
                case 24: return "RATCHET & PAWL";
                case 25: return "LEAD SCREW & NUT";
                default: return "MECHANISM ASSEMBLY";
            }
        }

        #endregion

        public bool IsUIBuilt => isUIBuilt;

        #region UI Hierarchy Construction

        public void BuildUIHierarchy()
        {
            if (isUIBuilt) return;
            ResolveFonts();

            Canvas canvas = GetComponentInParent<Canvas>();
            GameObject canvasGo;

            if (canvas == null)
            {
                canvasGo = gameObject;
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasGo = canvas.gameObject;
            }

            RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();
            if (canvasRt == null) canvasRt = canvasGo.AddComponent<RectTransform>();

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Remove any pre-existing duplicate SafeAreaContainer
            Transform existingSafe = canvasGo.transform.Find("SafeAreaContainer");
            if (existingSafe != null)
            {
                if (Application.isPlaying) Destroy(existingSafe.gameObject);
                else DestroyImmediate(existingSafe.gameObject);
            }

            // Fresh Safe Area Container
            GameObject safeGo = new GameObject("SafeAreaContainer");
            safeGo.transform.SetParent(canvasGo.transform, false);
            safeAreaRoot = safeGo;
            RectTransform safeRt = safeGo.AddComponent<RectTransform>();
            safeRt.anchorMin = Vector2.zero;
            safeRt.anchorMax = Vector2.one;
            safeRt.offsetMin = Vector2.zero;
            safeRt.offsetMax = Vector2.zero;
            ApplySafeArea(safeRt);

            // 1. Build Library Screen (ScrollRect, all 14 mechanism cards)
            BuildLibraryScreen(safeGo.transform);

            // 2. Build AR HUD Screen (Top Bar, Scanning Panel, Ergonomic Speed Controls Dock, Scale Pill)
            BuildARHudScreen(safeGo.transform);

            // 3. Build Inspection Card
            BuildInspectionCard(safeGo.transform);

            // 4. Build Calculations Card
            BuildCalculationsCard(safeGo.transform);

            // 5. Build Scientific Experiment Lab Panel
            BuildExperimentLabPanel(safeGo.transform);

            // 6. Build Diagnostic Acceptance Panel
            BuildDiagnosticAcceptancePanel(safeGo.transform);

            // Bind Button Listeners and Placement Events
            BindButtonListeners();
            InitializeEventBindings();

            isUIBuilt = true;
        }

        private void ApplySafeArea(RectTransform rt)
        {
            Rect safeArea = Screen.safeArea;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rt.anchorMin = minAnchor;
            rt.anchorMax = maxAnchor;
        }

        private void BuildLibraryScreen(Transform parent)
        {
            libraryScreen = CreatePanel("LibraryScreen", parent, colBackground, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);

            // Fixed Top Header Container: y = 0.88 to 0.985
            GameObject header = CreatePanel("LibraryHeader", libraryScreen.transform, Color.clear,
                new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.985f), Vector2.zero, Vector2.zero, false);

            CreateText("TitleText", header.transform, "MECH-XR", 44, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Left,
                new Vector2(0f, 0.44f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            CreateText("SubText", header.transform, "INTERACTIVE MECHANICAL ENGINEERING LABORATORY", 13, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0f, 0.05f), new Vector2(1f, 0.42f), Vector2.zero, Vector2.zero);

            // Divider line
            CreatePanel("Divider", libraryScreen.transform, colBorderSubtle,
                new Vector2(0.04f, 0.875f), new Vector2(0.96f, 0.877f), Vector2.zero, Vector2.zero, false);

            // Category Filter Bar: y = 0.815 to 0.865
            GameObject catBar = CreatePanel("CategoryFilterBar", libraryScreen.transform, Color.clear,
                new Vector2(0.03f, 0.815f), new Vector2(0.97f, 0.865f), Vector2.zero, Vector2.zero, false);

            btnCatAll = CreateButton("Cat_All", catBar.transform, "ALL (26)",
                new Vector2(0.00f, 0.05f), new Vector2(0.18f, 0.95f), colAccentCrimson, colTextPrimary, 13);
            btnCatGears = CreateButton("Cat_Gears", catBar.transform, "GEARS (14)",
                new Vector2(0.20f, 0.05f), new Vector2(0.39f, 0.95f), colButtonNormal, colTextPrimary, 13);
            btnCatEngines = CreateButton("Cat_Engines", catBar.transform, "ENGINES (4)",
                new Vector2(0.41f, 0.05f), new Vector2(0.59f, 0.95f), colButtonNormal, colTextPrimary, 13);
            btnCatLinkages = CreateButton("Cat_Linkages", catBar.transform, "LINKAGES (4)",
                new Vector2(0.61f, 0.05f), new Vector2(0.79f, 0.95f), colButtonNormal, colTextPrimary, 13);
            btnCatDrives = CreateButton("Cat_Drives", catBar.transform, "DRIVES (4)",
                new Vector2(0.81f, 0.05f), new Vector2(1.00f, 0.95f), colButtonNormal, colTextPrimary, 13);

            btnCatAll.onClick.AddListener(() => FilterLibraryCategory(0));
            btnCatGears.onClick.AddListener(() => FilterLibraryCategory(1));
            btnCatEngines.onClick.AddListener(() => FilterLibraryCategory(2));
            btnCatLinkages.onClick.AddListener(() => FilterLibraryCategory(3));
            btnCatDrives.onClick.AddListener(() => FilterLibraryCategory(4));

            // Scroll View spanning from y = 0.02 to y = 0.805 (with raycastTarget = true)
            GameObject scrollView = CreatePanel("LibraryScrollView", libraryScreen.transform, Color.clear,
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.805f), Vector2.zero, Vector2.zero, true);

            libraryScrollRect = scrollView.AddComponent<ScrollRect>();
            libraryScrollRect.horizontal = false;
            libraryScrollRect.vertical = true;
            libraryScrollRect.scrollSensitivity = 28f;
            libraryScrollRect.movementType = ScrollRect.MovementType.Elastic;
            libraryScrollRect.elasticity = 0.1f;
            libraryScrollRect.inertia = true;
            libraryScrollRect.decelerationRate = 0.135f;

            // Viewport with RectMask2D (raycastTarget = true ensures margins/padding drag properly)
            GameObject viewport = CreatePanel("Viewport", scrollView.transform, Color.clear,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            viewport.AddComponent<RectMask2D>();
            libraryScrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Content Container with VerticalLayoutGroup
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            Image contentImg = content.AddComponent<Image>();
            contentImg.color = Color.clear;
            contentImg.raycastTarget = true;

            libraryContentRect = content.GetComponent<RectTransform>();
            libraryContentRect.anchorMin = new Vector2(0f, 1f);
            libraryContentRect.anchorMax = new Vector2(1f, 1f);
            libraryContentRect.pivot = new Vector2(0.5f, 1f);
            libraryContentRect.offsetMin = Vector2.zero;
            libraryContentRect.offsetMax = Vector2.zero;
            libraryContentRect.sizeDelta = new Vector2(0f, 4350f);
            libraryScrollRect.content = libraryContentRect;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 14, 20);
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            string[] titles = new string[]
            {
                "01. SPUR GEAR PAIR",
                "02. BEVEL GEAR PAIR",
                "03. WORM & WHEEL",
                "04. PLANETARY GEAR SYSTEM",
                "05. RACK AND PINION",
                "06. BELT AND PULLEY SYSTEM",
                "07. SIMPLE GEAR TRAIN",
                "08. COMPOUND GEAR TRAIN",
                "09. REVERTED GEAR TRAIN",
                "10. INTERNAL GEAR PAIR",
                "11. HELICAL GEAR PAIR",
                "12. HERRINGBONE GEAR PAIR",
                "13. AUTOMOTIVE DIFFERENTIAL",
                "14. HARMONIC DRIVE",
                "15. FOUR-STROKE IC ENGINE",
                "16. SLIDER-CRANK MECHANISM",
                "17. CAM AND FOLLOWER",
                "18. RECIPROCATING PUMP",
                "19. FOUR-BAR LINKAGE",
                "20. CRANK-ROCKER MECHANISM",
                "21. GENEVA MECHANISM",
                "22. SCOTCH YOKE MECHANISM",
                "23. UNIVERSAL JOINT (CARDAN)",
                "24. CHAIN & SPROCKET DRIVE",
                "25. RATCHET & PAWL",
                "26. LEAD SCREW & NUT"
            };

            string[] badges = new string[]
            {
                "PARALLEL TRANSMISSION",
                "RIGHT-ANGLE DRIVE",
                "HIGH REDUCTION DRIVE",
                "EPICYCLIC DRIVE",
                "LINEAR CONVERTER",
                "FRICTION POWER DRIVE",
                "MULTI-SHAFT IDLER DRIVE",
                "DUAL-STAGE REDUCER",
                "COAXIAL REDUCER",
                "ANNULAR TRANSMISSION",
                "SMOOTH PARALLEL DRIVE",
                "DOUBLE-HELICAL DRIVE",
                "TORQUE-VECTORING AXLE",
                "STRAIN WAVE ROBOTICS",
                "720° FOUR-STROKE CYCLE",
                "ROTARY-RECIPROCATING",
                "HARMONIC VALVE MOTION",
                "POSITIVE DISPLACEMENT",
                "GRASHOF KINEMATIC CHAIN",
                "LIMIT-ANGLE OSCILLATOR",
                "INTERMITTENT INDEXING",
                "PURE HARMONIC SHM",
                "CARDAN SPEED RIPPLE",
                "POSITIVE ROLLER DRIVE",
                "ONE-WAY INDEXING DETENT",
                "HIGH-THRUST ACME DRIVE"
            };

            string[] descs = new string[]
            {
                "Parallel shaft power transmission • 20T Pinion to 40T Spoked Gear • 2:1 Speed Reduction",
                "90° Intersecting shaft power transfer • Conical pitch cones • 16T Pinion to 32T Crown Gear",
                "Non-intersecting skew axis drive • 1-Start Acme Worm to 30T Throated Wheel • 30:1 Self-Locking",
                "Epicyclic high-torque transmission • Sun 18T, 3x Planets 18T, Ring 54T • 4:1 Coaxial Reduction",
                "Rotary to linear motion conversion • 20T Pinion with 22T Linear Slide Rack • Reciprocating Slide",
                "Parallel friction drive • Dia 60 mm Driver to Dia 120 mm Driven • 2:1 Speed Reduction",
                "3-Shaft power train • 20T Driver, 28T Intermediate Idler, 40T Driven • Same Direction Output",
                "Compound countershaft transmission • Stage 1 (18T:36T) & Stage 2 (18T:54T) • 6:1 Massive Reduction",
                "Coaxial input & output shafts • Equal center distances (a₁ = a₂ = 105 mm) • 3:1 Coaxial Reduction",
                "External 20T Pinion meshing internally with 60T Annular Ring • Identical Direction • 3:1 Reduction",
                "Angled helical teeth (±20°) • Progressive engagement • High speed & axial thrust vectoring",
                "Double-helical chevron profile • Opposed angles cancel net axial thrust (Net Fa = 0) • Extreme Load",
                "Open automotive differential • 3:1 Hypoid Final Drive, Carrier, Spiders & Dual Axles • Cornering",
                "Zero-backlash precision drive • Elliptical Wave Generator, Flexspline (48T), Circular Spline (50T) • 24:1",
                "Single-cylinder 4-stroke cycle • Intake, Compression, Power, Exhaust • 1/2-speed Camshaft & Poppet Valves",
                "Classic slider-crank kinematic inversion • r = 40 mm Crank, l = 120 mm I-Beam Rod • Exact Slider Velocity",
                "Eccentric plate cam & reciprocating roller follower • 16 mm Lift • Harmonic displacement & velocity",
                "Industrial positive displacement plunger pump • Crank, Rod, Crosshead & Dual Check Valves • Suction & Discharge Flow",
                "Planar 4-bar kinematic chain • Crank, Coupler, Rocker & Fixed Ground Link • Grashof Condition s + l ≤ p + q verification",
                "Grashof Crank-Rocker mechanism • Continuous 360° input crank driving 48.2° oscillating rocker • Quick-Return Action",
                "Four-slot Maltese Cross indexing drive • 75% Dwell with locking crescent • 90° pin indexing engagement • 4:1 Ratio",
                "Slotted yoke mechanism • Translates continuous crank rotation into mathematically exact Simple Harmonic Motion (SHM)",
                "Cardan Hooke's joint • Intersecting shafts at 20° operating angle • 4-armed spider trunnion • 2nd harmonic velocity ripple",
                "ANSI #40 roller chain & sprockets • 15T Driver, 30T Driven (2:1 Reduction) • Chordal action polygonal effect analysis",
                "24-Tooth asymmetric sawtooth wheel • Oscillating driving arm & pawl with stationary holding detent • 24:1 Unidirectional",
                "Trapezoidal Acme lead screw (d = 24 mm, L = 6 mm) & bronze nut • 471x Force Multiplication • Self-Locking Acme Thread"
            };

            cardLaunchButtons = new Button[26];
            libraryCards = new GameObject[26];

            for (int i = 0; i < 26; i++)
            {
                int index = i;
                GameObject card = CreateMenuCard(content.transform, index, titles[index], descs[index], badges[index]);
                libraryCards[index] = card;
                Button launchBtn = card.GetComponentInChildren<Button>();
                cardLaunchButtons[index] = launchBtn;
            }

            // Footer tip in scrollable area
            GameObject footer = new GameObject("FooterTip");
            footer.transform.SetParent(content.transform, false);
            LayoutElement footerLe = footer.AddComponent<LayoutElement>();
            footerLe.minHeight = 44f;
            footerLe.preferredHeight = 44f;
            footerLe.flexibleWidth = 1f;
            CreateText("FooterText", footer.transform, "Select any mechanism to begin tabletop AR simulation & dynamic laboratory experiments.",
                13, FontStyles.Italic, colTextSecondary, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Canvas.ForceUpdateCanvases();
            if (libraryContentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect);
            }
            if (libraryScrollRect != null)
            {
                libraryScrollRect.verticalNormalizedPosition = 1f;
                libraryScrollRect.velocity = Vector2.zero;
            }
        }

        private GameObject CreateMenuCard(Transform parent, int mechanismIndex, string title, string description, string badge)
        {
            GameObject card = new GameObject($"Card_{mechanismIndex}");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 150f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.minHeight = 150f;
            le.preferredHeight = 150f;
            le.flexibleWidth = 1f;

            Image img = card.AddComponent<Image>();
            img.color = colSurfaceCard;
            img.raycastTarget = true;

            CreateBorder(card.transform, colBorderSubtle, 1.5f);

            // Left Crimson Accent Tag
            CreatePanel("AccentTag", card.transform, colAccentCrimson,
                new Vector2(0f, 0f), new Vector2(0.015f, 1f), Vector2.zero, Vector2.zero, false);

            // Category Badge
            GameObject badgeGo = CreatePanel("Badge", card.transform, new Color(0.161f, 0.169f, 0.188f, 1f),
                new Vector2(0.035f, 0.74f), new Vector2(0.56f, 0.94f), Vector2.zero, Vector2.zero, false);
            CreateBorder(badgeGo.transform, colBorderSubtle, 1f);
            CreateText("BadgeText", badgeGo.transform, badge, 12, FontStyles.Bold, colAccentCrimson, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Title & Description
            CreateText("CardTitle", card.transform, title, 20, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.035f, 0.44f), new Vector2(0.68f, 0.72f), Vector2.zero, Vector2.zero);

            CreateText("CardDesc", card.transform, description, 13, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.TopLeft,
                new Vector2(0.035f, 0.06f), new Vector2(0.68f, 0.42f), Vector2.zero, Vector2.zero);

            // Launch Action Button
            Button launchBtn = CreateButton("LaunchBtn", card.transform, "EXPLORE ▶",
                new Vector2(0.70f, 0.22f), new Vector2(0.97f, 0.78f),
                colAccentCrimson, colTextPrimary, 16, true);

            return card;
        }

        private void BuildARHudScreen(Transform parent)
        {
            arHudScreen = CreatePanel("ARHudScreen", parent, Color.clear, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);

            // 1. Top Header Bar (macOS Laboratory styling)
            arTopBar = CreatePanel("TopBar", arHudScreen.transform, colSurfaceCard,
                new Vector2(0f, 0.90f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, false);
            CreateBorder(arTopBar.transform, colBorderSubtle, 1.5f);

            returnToMenuButton = CreateButton("MenuBtn", arTopBar.transform, "◀ LIBRARY",
                new Vector2(0.03f, 0.18f), new Vector2(0.24f, 0.82f), colButtonNormal, colTextPrimary, 16);

            repositionButton = CreateButton("RepositionBtn", arTopBar.transform, "REPLACE",
                new Vector2(0.76f, 0.18f), new Vector2(0.97f, 0.82f), colButtonNormal, colAccentCrimson, 16);

            headerTitleText = CreateText("HeaderTitle", arTopBar.transform, "MECHANISM NAME", 22, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.46f), new Vector2(0.75f, 0.92f), Vector2.zero, Vector2.zero);
            headerSubtitleText = CreateText("HeaderSubtitle", arTopBar.transform, "KINEMATICS SIMULATION", 13, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.46f), Vector2.zero, Vector2.zero);

            // Status Indicator Pill (top left below bar)
            statusPill = CreatePanel("StatusPill", arHudScreen.transform, colSurfaceCard,
                new Vector2(0.04f, 0.855f), new Vector2(0.38f, 0.890f), Vector2.zero, Vector2.zero, false);
            CreateBorder(statusPill.transform, colBorderSubtle, 1f);
            statusIndicatorText = CreateText("StatusText", statusPill.transform, "SCANNING SURFACE", 13, FontStyles.Bold, colAccentCrimson, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Floating Transient Scale Indicator Pill (Center-top below bar)
            scalePill = CreatePanel("ScalePill", arHudScreen.transform, colElevatedPanel,
                new Vector2(0.42f, 0.855f), new Vector2(0.64f, 0.890f), Vector2.zero, Vector2.zero, false);
            CreateBorder(scalePill.transform, colBorderSubtle, 1f);
            scalePillText = CreateText("ScaleText", scalePill.transform, "SCALE: 1.0×", 13, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scalePill.SetActive(false);

            // 2. Dedicated AR Scanning Instruction Panel
            scanningPanel = CreatePanel("ScanningPanel", arHudScreen.transform, colElevatedPanel,
                new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.83f), Vector2.zero, Vector2.zero, false);
            CreateBorder(scanningPanel.transform, colAccentCrimson, 1.5f);
            scanningPromptText = CreateText("PromptText", scanningPanel.transform, "Slowly pan device across table or desk to detect flat surfaces",
                15, FontStyles.Normal, colTextPrimary, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            scanningPanel.SetActive(false);

            // 3. Ergonomic Bottom Controls Dock (raycastTarget = true to prevent pass-through!)
            arBottomBar = CreatePanel("BottomBar", arHudScreen.transform, colSurfaceCard,
                new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero, true);
            CreateBorder(arBottomBar.transform, colBorderSubtle, 1.5f);

            // Row 1: Action Controls (PAUSE, REV, EXPLODE, RESET, LAB)
            playPauseButton = CreateButton("PlayPauseBtn", arBottomBar.transform, "PAUSE ❚❚",
                new Vector2(0.02f, 0.65f), new Vector2(0.19f, 0.95f), colButtonNormal, colTextPrimary, 15);
            playPauseButtonText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();

            reverseButton = CreateButton("ReverseBtn", arBottomBar.transform, "REV ⇄",
                new Vector2(0.21f, 0.65f), new Vector2(0.38f, 0.95f), colButtonNormal, colTextPrimary, 15);

            explodeButton = CreateButton("ExplodeBtn", arBottomBar.transform, "EXPLODE ⤢",
                new Vector2(0.40f, 0.65f), new Vector2(0.59f, 0.95f), colButtonNormal, colTextPrimary, 15);
            explodeButtonText = explodeButton.GetComponentInChildren<TextMeshProUGUI>();

            resetButton = CreateButton("ResetBtn", arBottomBar.transform, "RESET ⟲",
                new Vector2(0.61f, 0.65f), new Vector2(0.78f, 0.95f), colButtonNormal, colTextPrimary, 15);

            toggleLabButton = CreateButton("LabBtn", arBottomBar.transform, "LAB ⚗",
                new Vector2(0.80f, 0.65f), new Vector2(0.98f, 0.95f), colButtonNormal, colAccentCrimson, 15, true);

            // Row 2: Speed Controls Header with Steppers & Quick Presets
            btnMinus10 = CreateButton("Minus10Btn", arBottomBar.transform, "-10",
                new Vector2(0.02f, 0.38f), new Vector2(0.12f, 0.58f), colButtonNormal, colTextPrimary, 14);

            CreateText("SpeedLabel", arBottomBar.transform, "INPUT VELOCITY", 13, FontStyles.Bold, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.13f, 0.38f), new Vector2(0.36f, 0.58f), Vector2.zero, Vector2.zero);

            btnPlus10 = CreateButton("Plus10Btn", arBottomBar.transform, "+10",
                new Vector2(0.37f, 0.38f), new Vector2(0.47f, 0.58f), colButtonNormal, colTextPrimary, 14);

            // Quick Preset Pills: 30, 60, 120, 240, 300 RPM
            Button btnP30 = CreateButton("P30Btn", arBottomBar.transform, "30",
                new Vector2(0.49f, 0.38f), new Vector2(0.57f, 0.58f), colButtonNormal, colTextSecondary, 12);
            Button btnP60 = CreateButton("P60Btn", arBottomBar.transform, "60",
                new Vector2(0.58f, 0.38f), new Vector2(0.66f, 0.58f), colButtonNormal, colTextSecondary, 12);
            Button btnP120 = CreateButton("P120Btn", arBottomBar.transform, "120",
                new Vector2(0.67f, 0.38f), new Vector2(0.76f, 0.58f), colButtonNormal, colTextSecondary, 12);
            Button btnP240 = CreateButton("P240Btn", arBottomBar.transform, "240",
                new Vector2(0.77f, 0.38f), new Vector2(0.85f, 0.58f), colButtonNormal, colTextSecondary, 12);

            btnP30.onClick.AddListener(() => SetDirectRPM(30f));
            btnP60.onClick.AddListener(() => SetDirectRPM(60f));
            btnP120.onClick.AddListener(() => SetDirectRPM(120f));
            btnP240.onClick.AddListener(() => SetDirectRPM(240f));

            speedValueText = CreateText("SpeedValue", arBottomBar.transform, "120 RPM", 16, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Right,
                new Vector2(0.86f, 0.38f), new Vector2(0.98f, 0.58f), Vector2.zero, Vector2.zero);

            // Row 3: Generously Sized Ergonomic Speed Slider (Immediate response across full width)
            speedSlider = CreateSlider("SpeedSlider", arBottomBar.transform,
                new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.32f), 10f, 300f, 120f);

            arBottomBar.SetActive(false);
        }

        private void BuildInspectionCard(Transform parent)
        {
            inspectionCard = CreatePanel("InspectionCard", parent, colElevatedPanel,
                new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero, true);
            CreateBorder(inspectionCard.transform, colBorderSubtle, 1.5f);

            // Top Crimson Header Pill
            GameObject topPill = CreatePanel("InspPill", inspectionCard.transform, new Color(0.898f, 0.224f, 0.271f, 0.18f),
                new Vector2(0.03f, 0.83f), new Vector2(0.48f, 0.95f), Vector2.zero, Vector2.zero, false);
            CreateBorder(topPill.transform, colAccentCrimson, 1f);
            CreateText("PillText", topPill.transform, "COMPONENT INSPECTION", 13, FontStyles.Bold, colAccentCrimson, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            inspectCloseButton = CreateButton("CloseBtn", inspectionCard.transform, "✕",
                new Vector2(0.88f, 0.82f), new Vector2(0.97f, 0.96f), colButtonNormal, colTextSecondary, 18);

            inspectTitleText = CreateText("InspTitle", inspectionCard.transform, "Component Name", 22, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.65f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);

            inspectRoleText = CreateText("InspRole", inspectionCard.transform, "Mechanical role description goes here.", 14, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.64f), Vector2.zero, Vector2.zero);

            inspectSpecsText = CreateText("InspSpecs", inspectionCard.transform, "Material specifications and tolerances.", 13, FontStyles.Italic, colTextSecondary, TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.38f), Vector2.zero, Vector2.zero);

            inspectDetachButton = CreateButton("DetachBtn", inspectionCard.transform, "DETACH ⤢",
                new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.16f),
                colAccentCrimson, colTextPrimary, 16, true);
            inspectDetachButtonText = inspectDetachButton.GetComponentInChildren<TextMeshProUGUI>();

            inspectionCard.SetActive(false);
        }

        private void BuildCalculationsCard(Transform parent)
        {
            calculationsCard = CreatePanel("CalculationsCard", parent, colElevatedPanel,
                new Vector2(0.05f, 0.23f), new Vector2(0.95f, 0.68f), Vector2.zero, Vector2.zero, true);
            CreateBorder(calculationsCard.transform, colBorderSubtle, 1.5f);

            GameObject headerPill = CreatePanel("CalcPill", calculationsCard.transform, new Color(0.898f, 0.224f, 0.271f, 0.18f),
                new Vector2(0.04f, 0.91f), new Vector2(0.55f, 0.97f), Vector2.zero, Vector2.zero, false);
            CreateBorder(headerPill.transform, colAccentCrimson, 1f);
            CreateText("PillText", headerPill.transform, "ENGINEERING TELEMETRY", 13, FontStyles.Bold, colAccentCrimson, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Button closeCalcBtn = CreateButton("CloseCalcBtn", calculationsCard.transform, "✕",
                new Vector2(0.88f, 0.90f), new Vector2(0.97f, 0.98f), colButtonNormal, colTextSecondary, 18);
            closeCalcBtn.onClick.AddListener(ToggleCalculationsCard);

            calculationsContentText = CreateText("CalculationsContent", calculationsCard.transform, "Real-time mechanical data telemetry.",
                15, FontStyles.Normal, colTextPrimary, TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.48f), new Vector2(0.96f, 0.89f), Vector2.zero, Vector2.zero);

            formulasContentText = CreateText("FormulasContent", calculationsCard.transform, "Governing mechanical kinematics formulas.",
                14, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.46f), Vector2.zero, Vector2.zero);

            calculationsCard.SetActive(false);
        }

        private void BuildExperimentLabPanel(Transform parent)
        {
            // Collapsible Scientific Experiment Lab Drawer (spanning middle of screen so AR remains visible)
            experimentLabPanel = CreatePanel("ExperimentLabPanel", parent, new Color(0.10f, 0.11f, 0.12f, 0.95f),
                new Vector2(0.03f, 0.14f), new Vector2(0.97f, 0.88f), Vector2.zero, Vector2.zero, true);
            CreateBorder(experimentLabPanel.transform, colBorderSubtle, 1.5f);

            // 1. Header
            GameObject header = CreatePanel("LabHeader", experimentLabPanel.transform, Color.clear,
                new Vector2(0.03f, 0.92f), new Vector2(0.97f, 0.985f), Vector2.zero, Vector2.zero, false);

            CreateText("LabTitle", header.transform, "SCIENTIFIC EXPERIMENT LAB", 20, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Left,
                new Vector2(0f, 0.45f), new Vector2(0.70f, 1f), Vector2.zero, Vector2.zero);
            CreateText("LabSubtitle", header.transform, "DYNAMIC KINEMATICS, SI POWER & TORQUE EXPERIMENTS", 11, FontStyles.Normal, colAccentCrimson, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0.70f, 0.45f), Vector2.zero, Vector2.zero);

            Button closeLabBtn = CreateButton("CloseLabBtn", header.transform, "✕",
                new Vector2(0.88f, 0.15f), new Vector2(1f, 0.95f), colButtonNormal, colTextSecondary, 18);
            closeLabBtn.onClick.AddListener(ToggleExperimentLab);

            // 2. Telemetry Summary Card (y = 0.74 to 0.91)
            GameObject telemetryCard = CreatePanel("TelemetryCard", experimentLabPanel.transform, colElevatedPanel,
                new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.91f), Vector2.zero, Vector2.zero, false);
            CreateBorder(telemetryCard.transform, colBorderSubtle, 1f);

            expTelemetryText = CreateText("ExpTelemetry", telemetryCard.transform, "Real-time mechanical SI telemetry.",
                13, FontStyles.Normal, colTextPrimary, TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.96f), Vector2.zero, Vector2.zero);

            // 3. Sliders Section (y = 0.50 to 0.73)
            // Slider 1: Input Torque Tin (1.0 to 100.0 N·m)
            CreateText("TorqueLabel", experimentLabPanel.transform, "INPUT TORQUE (T_in)", 12, FontStyles.Bold, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.68f), new Vector2(0.40f, 0.73f), Vector2.zero, Vector2.zero);
            expTorqueValueText = CreateText("TorqueValue", experimentLabPanel.transform, "10.0 N·m", 13, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Right,
                new Vector2(0.70f, 0.68f), new Vector2(0.96f, 0.73f), Vector2.zero, Vector2.zero);
            expTorqueSlider = CreateSlider("ExpTorqueSlider", experimentLabPanel.transform,
                new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.68f), 1f, 100f, 10f);
            expTorqueSlider.onValueChanged.AddListener(OnExpTorqueChanged);

            // Slider 2: Transmission Efficiency (70% to 100%)
            CreateText("EffLabel", experimentLabPanel.transform, "TRANSMISSION EFFICIENCY (η)", 12, FontStyles.Bold, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.56f), new Vector2(0.45f, 0.61f), Vector2.zero, Vector2.zero);
            expEfficiencyValueText = CreateText("EffValue", experimentLabPanel.transform, "95.0 %", 13, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Right,
                new Vector2(0.70f, 0.56f), new Vector2(0.96f, 0.61f), Vector2.zero, Vector2.zero);
            expEfficiencySlider = CreateSlider("ExpEfficiencySlider", experimentLabPanel.transform,
                new Vector2(0.04f, 0.50f), new Vector2(0.96f, 0.56f), 0.70f, 1.0f, 0.95f);
            expEfficiencySlider.onValueChanged.AddListener(OnExpEfficiencyChanged);

            // 4. Live Visual Graphs Container (y = 0.22 to 0.49)
            GameObject graphContainer = CreatePanel("GraphContainer", experimentLabPanel.transform, new Color(0.08f, 0.09f, 0.10f, 1f),
                new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.49f), Vector2.zero, Vector2.zero, false);
            CreateBorder(graphContainer.transform, colBorderSubtle, 1f);

            // Graph Tabs: [RPM] [TORQUE] [SPEED/TIME] [POWER & LOSS]
            Button tabRPM = CreateButton("TabRPM", graphContainer.transform, "RPM",
                new Vector2(0.02f, 0.84f), new Vector2(0.24f, 0.98f), colButtonNormal, colTextPrimary, 11);
            Button tabTorque = CreateButton("TabTorque", graphContainer.transform, "TORQUE",
                new Vector2(0.26f, 0.84f), new Vector2(0.48f, 0.98f), colButtonNormal, colTextPrimary, 11);
            Button tabSpeedTime = CreateButton("TabSpeedTime", graphContainer.transform, "SPEED/t",
                new Vector2(0.50f, 0.84f), new Vector2(0.72f, 0.98f), colButtonNormal, colTextPrimary, 11);
            Button tabPower = CreateButton("TabPower", graphContainer.transform, "POWER/LOSS",
                new Vector2(0.74f, 0.84f), new Vector2(0.98f, 0.98f), colButtonNormal, colAccentCrimson, 11);

            tabRPM.onClick.AddListener(() => SwitchGraphTab(0));
            tabTorque.onClick.AddListener(() => SwitchGraphTab(1));
            tabSpeedTime.onClick.AddListener(() => SwitchGraphTab(2));
            tabPower.onClick.AddListener(() => SwitchGraphTab(3));

            graphTitleText = CreateText("GraphTitle", graphContainer.transform, "LIVE DYNAMIC GRAPH", 12, FontStyles.Bold, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);

            // Graph comparative dynamic bars
            GameObject barInGo = CreatePanel("BarInput", graphContainer.transform, new Color(0.12f, 0.58f, 0.95f),
                new Vector2(0.08f, 0.18f), new Vector2(0.32f, 0.65f), Vector2.zero, Vector2.zero, false);
            graphBarInput = barInGo.GetComponent<RectTransform>();
            graphBarInputLabel = CreateText("BarInLabel", graphContainer.transform, "IN: 120", 11, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.04f), new Vector2(0.32f, 0.16f), Vector2.zero, Vector2.zero);

            GameObject barOutGo = CreatePanel("BarOutput", graphContainer.transform, new Color(0.92f, 0.72f, 0.15f),
                new Vector2(0.38f, 0.18f), new Vector2(0.62f, 0.65f), Vector2.zero, Vector2.zero, false);
            graphBarOutput = barOutGo.GetComponent<RectTransform>();
            graphBarOutputLabel = CreateText("BarOutLabel", graphContainer.transform, "OUT: 60", 11, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.38f, 0.04f), new Vector2(0.62f, 0.16f), Vector2.zero, Vector2.zero);

            GameObject barLossGo = CreatePanel("BarLoss", graphContainer.transform, colAccentCrimson,
                new Vector2(0.68f, 0.18f), new Vector2(0.92f, 0.65f), Vector2.zero, Vector2.zero, false);
            graphBarLoss = barLossGo.GetComponent<RectTransform>();
            graphBarLossLabel = CreateText("BarLossLabel", graphContainer.transform, "LOSS: 5%", 11, FontStyles.Bold, colAccentCrimson, TextAlignmentOptions.Center,
                new Vector2(0.68f, 0.04f), new Vector2(0.92f, 0.16f), Vector2.zero, Vector2.zero);

            // 5. Scientific Presets Bar (y = 0.12 to 0.20)
            CreateText("PresetLabel", experimentLabPanel.transform, "EXPERIMENT PRESETS:", 11, FontStyles.Bold, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.16f), new Vector2(0.32f, 0.21f), Vector2.zero, Vector2.zero);

            Button btnPresetHighSpeed = CreateButton("PreHS", experimentLabPanel.transform, "HIGH SPEED",
                new Vector2(0.03f, 0.11f), new Vector2(0.24f, 0.16f), colButtonNormal, colTextPrimary, 11);
            Button btnPresetHighTorque = CreateButton("PreHT", experimentLabPanel.transform, "HIGH TORQUE",
                new Vector2(0.26f, 0.11f), new Vector2(0.48f, 0.16f), colButtonNormal, colTextPrimary, 11);
            Button btnPresetIndustrial = CreateButton("PreInd", experimentLabPanel.transform, "INDUSTRIAL",
                new Vector2(0.50f, 0.11f), new Vector2(0.72f, 0.16f), colButtonNormal, colTextPrimary, 11);
            Button btnPresetLowEff = CreateButton("PreLowEff", experimentLabPanel.transform, "LOW EFF (75%)",
                new Vector2(0.74f, 0.11f), new Vector2(0.97f, 0.16f), colButtonNormal, colAccentCrimson, 11);

            btnPresetHighSpeed.onClick.AddListener(() => ApplyPreset(240f, 5f, 0.96f));
            btnPresetHighTorque.onClick.AddListener(() => ApplyPreset(60f, 50f, 0.94f));
            btnPresetIndustrial.onClick.AddListener(() => ApplyPreset(150f, 30f, 0.92f));
            btnPresetLowEff.onClick.AddListener(() => ApplyPreset(120f, 20f, 0.75f));

            // 6. A/B Comparison Section (y = 0.02 to 0.10)
            captureExpAButton = CreateButton("CaptureABtn", experimentLabPanel.transform, "CAPTURE EXPERIMENT A 📌",
                new Vector2(0.03f, 0.02f), new Vector2(0.40f, 0.09f), colButtonNormal, colAccentCrimson, 12, true);
            captureExpAButton.onClick.AddListener(CaptureExperimentA);

            comparisonText = CreateText("ComparisonText", experimentLabPanel.transform, "Tap Capture A to compare live parameters vs snapshot.",
                11, FontStyles.Normal, colTextSecondary, TextAlignmentOptions.Left,
                new Vector2(0.42f, 0.01f), new Vector2(0.97f, 0.10f), Vector2.zero, Vector2.zero);

            experimentLabPanel.SetActive(false);
        }

        private void BuildDiagnosticAcceptancePanel(Transform parent)
        {
            diagnosticPanel = CreatePanel("DiagnosticTypographyPanel", parent, colBackground, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);

            CreateText("Diag_Header", diagnosticPanel.transform, "MECH-XR", 42, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.90f), new Vector2(0.9f, 0.98f), Vector2.zero, Vector2.zero);

            string[] testStrings = new string[]
            {
                "SPUR GEAR PAIR",
                "BEVEL GEAR PAIR",
                "WORM GEAR",
                "PLANETARY GEAR",
                "RUN",
                "PAUSE",
                "RESET",
                "INSPECT",
                "EXPLODE",
                "REASSEMBLE"
            };

            for (int i = 0; i < testStrings.Length; i++)
            {
                float yTop = 0.86f - i * 0.075f;
                float yBot = yTop - 0.065f;

                GameObject row = CreatePanel($"DiagRow_{i}", diagnosticPanel.transform, colSurfaceCard,
                    new Vector2(0.08f, yBot), new Vector2(0.92f, yTop), Vector2.zero, Vector2.zero, false);
                CreateBorder(row.transform, colBorderSubtle, 1f);
                CreateText($"DiagText_{i}", row.transform, testStrings[i], 22, FontStyles.Bold, colTextPrimary, TextAlignmentOptions.Center,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            diagnosticPanel.SetActive(false);
        }

        public void BindButtonListeners()
        {
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveAllListeners();
                returnToMenuButton.onClick.AddListener(OnReturnToLibrary);
            }

            if (repositionButton != null)
            {
                repositionButton.onClick.RemoveAllListeners();
                repositionButton.onClick.AddListener(OnReplaceMechanism);
            }

            if (playPauseButton != null)
            {
                playPauseButton.onClick.RemoveAllListeners();
                playPauseButton.onClick.AddListener(OnTogglePlayPause);
            }

            if (reverseButton != null)
            {
                reverseButton.onClick.RemoveAllListeners();
                reverseButton.onClick.AddListener(OnToggleReverse);
            }

            if (explodeButton != null)
            {
                explodeButton.onClick.RemoveAllListeners();
                explodeButton.onClick.AddListener(OnToggleExplode);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(OnResetMechanism);
            }

            if (toggleLabButton != null)
            {
                toggleLabButton.onClick.RemoveAllListeners();
                toggleLabButton.onClick.AddListener(ToggleExperimentLab);
            }

            if (toggleCalculationsButton != null)
            {
                toggleCalculationsButton.onClick.RemoveAllListeners();
                toggleCalculationsButton.onClick.AddListener(ToggleCalculationsCard);
            }

            if (speedSlider != null)
            {
                speedSlider.onValueChanged.RemoveAllListeners();
                speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
            }

            if (btnMinus10 != null)
            {
                btnMinus10.onClick.RemoveAllListeners();
                btnMinus10.onClick.AddListener(() => StepRPM(-10f));
            }

            if (btnPlus10 != null)
            {
                btnPlus10.onClick.RemoveAllListeners();
                btnPlus10.onClick.AddListener(() => StepRPM(+10f));
            }

            if (inspectCloseButton != null)
            {
                inspectCloseButton.onClick.RemoveAllListeners();
                inspectCloseButton.onClick.AddListener(CloseInspectionCard);
            }

            if (inspectDetachButton != null)
            {
                inspectDetachButton.onClick.RemoveAllListeners();
                inspectDetachButton.onClick.AddListener(OnToggleDetachCurrentComponent);
            }

            if (cardLaunchButtons != null)
            {
                for (int i = 0; i < cardLaunchButtons.Length; i++)
                {
                    int index = i;
                    if (cardLaunchButtons[i] != null)
                    {
                        cardLaunchButtons[i].onClick.RemoveAllListeners();
                        cardLaunchButtons[i].onClick.AddListener(() => OnSelectMechanismFromMenu(index));
                    }
                }
            }

            if (btnCatAll != null) { btnCatAll.onClick.RemoveAllListeners(); btnCatAll.onClick.AddListener(() => FilterLibraryCategory(0)); }
            if (btnCatGears != null) { btnCatGears.onClick.RemoveAllListeners(); btnCatGears.onClick.AddListener(() => FilterLibraryCategory(1)); }
            if (btnCatEngines != null) { btnCatEngines.onClick.RemoveAllListeners(); btnCatEngines.onClick.AddListener(() => FilterLibraryCategory(2)); }
            if (btnCatLinkages != null) { btnCatLinkages.onClick.RemoveAllListeners(); btnCatLinkages.onClick.AddListener(() => FilterLibraryCategory(3)); }
            if (btnCatDrives != null) { btnCatDrives.onClick.RemoveAllListeners(); btnCatDrives.onClick.AddListener(() => FilterLibraryCategory(4)); }
        }

        private void BindExistingHierarchy(GameObject safeRoot)
        {
            safeAreaRoot = safeRoot;

            Transform libT = safeRoot.transform.Find("LibraryScreen");
            if (libT != null)
            {
                libraryScreen = libT.gameObject;

                Transform catBarT = libT.Find("CategoryFilterBar");
                if (catBarT != null)
                {
                    Transform bAll = catBarT.Find("Cat_All");
                    if (bAll != null) btnCatAll = bAll.GetComponent<Button>();
                    Transform bGears = catBarT.Find("Cat_Gears");
                    if (bGears != null) btnCatGears = bGears.GetComponent<Button>();
                    Transform bEng = catBarT.Find("Cat_Engines");
                    if (bEng != null) btnCatEngines = bEng.GetComponent<Button>();
                    Transform bLink = catBarT.Find("Cat_Linkages");
                    if (bLink != null) btnCatLinkages = bLink.GetComponent<Button>();
                    Transform bDrv = catBarT.Find("Cat_Drives");
                    if (bDrv != null) btnCatDrives = bDrv.GetComponent<Button>();

                    if (btnCatAll != null) { btnCatAll.onClick.RemoveAllListeners(); btnCatAll.onClick.AddListener(() => FilterLibraryCategory(0)); }
                    if (btnCatGears != null) { btnCatGears.onClick.RemoveAllListeners(); btnCatGears.onClick.AddListener(() => FilterLibraryCategory(1)); }
                    if (btnCatEngines != null) { btnCatEngines.onClick.RemoveAllListeners(); btnCatEngines.onClick.AddListener(() => FilterLibraryCategory(2)); }
                    if (btnCatLinkages != null) { btnCatLinkages.onClick.RemoveAllListeners(); btnCatLinkages.onClick.AddListener(() => FilterLibraryCategory(3)); }
                    if (btnCatDrives != null) { btnCatDrives.onClick.RemoveAllListeners(); btnCatDrives.onClick.AddListener(() => FilterLibraryCategory(4)); }
                }

                Transform scrollT = libT.Find("LibraryScrollView");
                if (scrollT != null)
                {
                    libraryScrollRect = scrollT.GetComponent<ScrollRect>();
                    Image scrollImg = scrollT.GetComponent<Image>();
                    if (scrollImg != null) scrollImg.raycastTarget = true;

                    Transform vpT = scrollT.Find("Viewport");
                    if (vpT != null)
                    {
                        Image vpImg = vpT.GetComponent<Image>();
                        if (vpImg != null) vpImg.raycastTarget = true;
                    }
                }

                Transform contentT = libT.Find("LibraryScrollView/Viewport/Content");
                if (contentT != null)
                {
                    libraryContentRect = contentT.GetComponent<RectTransform>();
                    Image contentImg = contentT.GetComponent<Image>();
                    if (contentImg == null)
                    {
                        contentImg = contentT.gameObject.AddComponent<Image>();
                        contentImg.color = Color.clear;
                    }
                    contentImg.raycastTarget = true;

                    cardLaunchButtons = new Button[26];
                    libraryCards = new GameObject[26];
                    for (int i = 0; i < 26; i++)
                    {
                        Transform cardT = contentT.Find($"Card_{i}");
                        if (cardT != null)
                        {
                            libraryCards[i] = cardT.gameObject;
                            Image cardImg = cardT.GetComponent<Image>();
                            if (cardImg != null) cardImg.raycastTarget = true;
                            cardLaunchButtons[i] = cardT.GetComponentInChildren<Button>();
                        }
                    }

                    LayoutRebuilder.ForceRebuildLayoutImmediate(libraryContentRect);
                    if (libraryScrollRect != null)
                    {
                        libraryScrollRect.verticalNormalizedPosition = 1f;
                    }
                }
            }

            Transform arT = safeRoot.transform.Find("ARHudScreen");
            if (arT != null)
            {
                arHudScreen = arT.gameObject;
                Transform topT = arT.Find("TopBar");
                if (topT != null)
                {
                    arTopBar = topT.gameObject;
                    Transform menuBtnT = topT.Find("MenuBtn");
                    if (menuBtnT != null) returnToMenuButton = menuBtnT.GetComponent<Button>();
                    Transform repBtnT = topT.Find("RepositionBtn");
                    if (repBtnT != null) repositionButton = repBtnT.GetComponent<Button>();
                    Transform titleT = topT.Find("HeaderTitle");
                    if (titleT != null) headerTitleText = titleT.GetComponent<TextMeshProUGUI>();
                    Transform subT = topT.Find("HeaderSubtitle");
                    if (subT != null) headerSubtitleText = subT.GetComponent<TextMeshProUGUI>();
                }

                Transform pillT = arT.Find("StatusPill");
                if (pillT != null)
                {
                    statusPill = pillT.gameObject;
                    Transform stT = pillT.Find("StatusText");
                    if (stT != null) statusIndicatorText = stT.GetComponent<TextMeshProUGUI>();
                }

                Transform scalePillT = arT.Find("ScalePill");
                if (scalePillT != null)
                {
                    scalePill = scalePillT.gameObject;
                    Transform stT = scalePillT.Find("ScaleText");
                    if (stT != null) scalePillText = stT.GetComponent<TextMeshProUGUI>();
                }

                Transform scanT = arT.Find("ScanningPanel");
                if (scanT != null)
                {
                    scanningPanel = scanT.gameObject;
                    Transform promptT = scanT.Find("PromptText");
                    if (promptT != null) scanningPromptText = promptT.GetComponent<TextMeshProUGUI>();
                }

                Transform botT = arT.Find("BottomBar");
                if (botT != null)
                {
                    arBottomBar = botT.gameObject;
                    Transform ppT = botT.Find("PlayPauseBtn");
                    if (ppT != null)
                    {
                        playPauseButton = ppT.GetComponent<Button>();
                        playPauseButtonText = ppT.GetComponentInChildren<TextMeshProUGUI>();
                    }
                    Transform revT = botT.Find("ReverseBtn");
                    if (revT != null) reverseButton = revT.GetComponent<Button>();
                    Transform expT = botT.Find("ExplodeBtn");
                    if (expT != null)
                    {
                        explodeButton = expT.GetComponent<Button>();
                        explodeButtonText = expT.GetComponentInChildren<TextMeshProUGUI>();
                    }
                    Transform resT = botT.Find("ResetBtn");
                    if (resT != null) resetButton = resT.GetComponent<Button>();
                    Transform labT = botT.Find("LabBtn");
                    if (labT != null) toggleLabButton = labT.GetComponent<Button>();

                    Transform spValT = botT.Find("SpeedValue");
                    if (spValT != null) speedValueText = spValT.GetComponent<TextMeshProUGUI>();
                    Transform spSliderT = botT.Find("SpeedSlider");
                    if (spSliderT != null) speedSlider = spSliderT.GetComponent<Slider>();
                    Transform minusT = botT.Find("Minus10Btn");
                    if (minusT != null) btnMinus10 = minusT.GetComponent<Button>();
                    Transform plusT = botT.Find("Plus10Btn");
                    if (plusT != null) btnPlus10 = plusT.GetComponent<Button>();
                }
            }

            Transform expLabT = safeRoot.transform.Find("ExperimentLabPanel");
            if (expLabT != null)
            {
                experimentLabPanel = expLabT.gameObject;
            }

            Transform inspT = safeRoot.transform.Find("InspectionCard");
            if (inspT != null)
            {
                inspectionCard = inspT.gameObject;
                Transform closeT = inspT.Find("CloseBtn");
                if (closeT != null) inspectCloseButton = closeT.GetComponent<Button>();
                Transform titleT = inspT.Find("InspTitle");
                if (titleT != null) inspectTitleText = titleT.GetComponent<TextMeshProUGUI>();
                Transform roleT = inspT.Find("InspRole");
                if (roleT != null) inspectRoleText = roleT.GetComponent<TextMeshProUGUI>();
                Transform specsT = inspT.Find("InspSpecs");
                if (specsT != null) inspectSpecsText = specsT.GetComponent<TextMeshProUGUI>();
                Transform detachT = inspT.Find("DetachBtn");
                if (detachT != null)
                {
                    inspectDetachButton = detachT.GetComponent<Button>();
                    inspectDetachButtonText = detachT.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            Transform calcT = safeRoot.transform.Find("CalculationsCard");
            if (calcT != null)
            {
                calculationsCard = calcT.gameObject;
                Transform calcContentT = calcT.Find("CalculationsContent");
                if (calcContentT != null) calculationsContentText = calcContentT.GetComponent<TextMeshProUGUI>();
                Transform formContentT = calcT.Find("FormulasContent");
                if (formContentT != null) formulasContentText = formContentT.GetComponent<TextMeshProUGUI>();
            }

            Transform diagT = safeRoot.transform.Find("DiagnosticTypographyPanel");
            if (diagT != null)
            {
                diagnosticPanel = diagT.gameObject;
            }
        }

        #endregion

        #region Event Handlers

        private void HandlePlacementStateChanged(ARPlacementState state)
        {
            if (currentScreenState == AppScreenState.Library) return;

            switch (state)
            {
                case ARPlacementState.SearchingForSurfaces:
                    if (currentScreenState == AppScreenState.ARScanning)
                    {
                        if (scanningPanel != null) scanningPanel.SetActive(true);
                        if (scanningPromptText != null)
                            scanningPromptText.text = "Slowly pan device across table or desk to detect flat surfaces";
                        if (statusIndicatorText != null) statusIndicatorText.text = "SEARCHING SURFACE";
                    }
                    break;

                case ARPlacementState.SurfaceDetected:
                    if (currentScreenState == AppScreenState.ARScanning)
                    {
                        if (scanningPanel != null) scanningPanel.SetActive(true);
                        if (scanningPromptText != null)
                            scanningPromptText.text = "Surface verified! Tap screen to place mechanical assembly";
                        if (statusIndicatorText != null) statusIndicatorText.text = "SURFACE DETECTED";
                    }
                    break;

                case ARPlacementState.ObjectPlaced:
                    break;

                case ARPlacementState.Idle:
                default:
                    if (scanningPanel != null) scanningPanel.SetActive(false);
                    break;
            }
        }

        private void HandleMechanismPlaced(MechanismBase mechanism)
        {
            if (activeMechanism != null)
            {
                activeMechanism.OnKinematicsUpdated -= UpdateCalculationsDisplay;
                activeMechanism.OnExplodedViewChanged -= HandleExplodedViewChanged;
                ModelInteractionController prevCtrl = activeMechanism.GetComponent<ModelInteractionController>();
                if (prevCtrl != null)
                {
                    prevCtrl.OnComponentSelected -= HandleComponentSelected;
                    prevCtrl.OnComponentDeselected -= HandleComponentDeselected;
                    prevCtrl.OnScaleChanged -= HandleScaleChanged;
                }
            }

            activeMechanism = mechanism;
            if (activeMechanism != null)
            {
                activeMechanism.OnKinematicsUpdated += UpdateCalculationsDisplay;
                activeMechanism.OnExplodedViewChanged += HandleExplodedViewChanged;

                if (headerTitleText != null) headerTitleText.text = activeMechanism.MechanismName;
                if (headerSubtitleText != null) headerSubtitleText.text = activeMechanism.CategoryTitle;

                if (speedSlider != null) speedSlider.value = activeMechanism.InputRPM;
                if (speedValueText != null) speedValueText.text = $"{Mathf.RoundToInt(activeMechanism.InputRPM)} RPM";

                UpdateCalculationsDisplay();
                UpdatePlayPauseButtonState();
                UpdateExplodeButtonState();

                ModelInteractionController controller = activeMechanism.GetComponent<ModelInteractionController>();
                if (controller != null)
                {
                    controller.OnComponentSelected += HandleComponentSelected;
                    controller.OnComponentDeselected += HandleComponentDeselected;
                    controller.OnScaleChanged += HandleScaleChanged;
                }
            }

            SetScreenState(AppScreenState.ARPlaced);
        }

        private void HandleScaleChanged(float newScale)
        {
            if (scalePill != null && scalePillText != null)
            {
                scalePill.SetActive(true);
                scalePillText.text = $"SCALE: {newScale:F1}×";
                scalePillTimer = 1.5f;
            }
        }

        private void HandleMechanismRemoved()
        {
            if (activeMechanism != null)
            {
                activeMechanism.OnKinematicsUpdated -= UpdateCalculationsDisplay;
                activeMechanism.OnExplodedViewChanged -= HandleExplodedViewChanged;
                ModelInteractionController prevCtrl = activeMechanism.GetComponent<ModelInteractionController>();
                if (prevCtrl != null)
                {
                    prevCtrl.OnComponentSelected -= HandleComponentSelected;
                    prevCtrl.OnComponentDeselected -= HandleComponentDeselected;
                    prevCtrl.OnScaleChanged -= HandleScaleChanged;
                }
                activeMechanism = null;
            }

            currentlyInspectedComponent = null;
            if (inspectionCard != null) inspectionCard.SetActive(false);
            if (calculationsCard != null) calculationsCard.SetActive(false);
            if (experimentLabPanel != null) experimentLabPanel.SetActive(false);
            isLabVisible = false;
        }

        private void HandleExplodedViewChanged(bool isExploded)
        {
            UpdateExplodeButtonState();
        }

        private void HandleComponentSelected(InspectableComponent comp)
        {
            currentlyInspectedComponent = comp;
            if (comp != null)
            {
                if (inspectTitleText != null) inspectTitleText.text = comp.ComponentName;
                if (inspectRoleText != null) inspectRoleText.text = comp.RoleDescription;
                if (inspectSpecsText != null) inspectSpecsText.text = comp.TechnicalSpecs;

                if (inspectDetachButtonText != null)
                    inspectDetachButtonText.text = comp.IsDetached ? "RETURN TO ASSEMBLY" : "DETACH ⤢";

                SetScreenState(AppScreenState.ComponentInspection);
            }
        }

        private void HandleComponentDeselected()
        {
            CloseInspectionCard();
        }

        #endregion

        #region HUD & Experiment Action Callbacks

        public void StepRPM(float delta)
        {
            if (speedSlider != null)
            {
                float target = Mathf.Clamp(speedSlider.value + delta, speedSlider.minValue, speedSlider.maxValue);
                speedSlider.value = target;
            }
        }

        public void SetDirectRPM(float rpm)
        {
            if (speedSlider != null)
            {
                speedSlider.value = Mathf.Clamp(rpm, speedSlider.minValue, speedSlider.maxValue);
            }
        }

        private void OnTogglePlayPause()
        {
            if (activeMechanism != null)
            {
                activeMechanism.TogglePause();
                UpdatePlayPauseButtonState();
            }
        }

        private void UpdatePlayPauseButtonState()
        {
            if (playPauseButtonText != null && activeMechanism != null)
            {
                playPauseButtonText.text = activeMechanism.IsPaused ? "RUN ▶" : "PAUSE ❚❚";
            }
        }

        private void OnToggleReverse()
        {
            if (activeMechanism != null)
            {
                activeMechanism.ToggleReverse();
                UpdateCalculationsDisplay();
            }
        }

        private void OnToggleExplode()
        {
            if (activeMechanism != null)
            {
                activeMechanism.ToggleExplodedView();
                if (activeMechanism.IsExploded)
                {
                    SetScreenState(AppScreenState.ExplodedView);
                }
                else
                {
                    SetScreenState(AppScreenState.ARPlaced);
                }
            }
        }

        private void UpdateExplodeButtonState()
        {
            if (explodeButtonText != null && activeMechanism != null)
            {
                explodeButtonText.text = activeMechanism.IsExploded ? "REASSEMBLE ⤡" : "EXPLODE ⤢";
            }
        }

        private void OnResetMechanism()
        {
            if (activeMechanism != null)
            {
                activeMechanism.ResetMechanism();
                activeMechanism.ReassembleAll();
                activeMechanism.InputRPM = 120f;
                if (speedSlider != null) speedSlider.value = 120f;
                if (speedValueText != null) speedValueText.text = "120 RPM";
                UpdateExplodeButtonState();
                UpdateCalculationsDisplay();
            }
        }

        private void OnSpeedSliderChanged(float val)
        {
            if (activeMechanism != null)
            {
                activeMechanism.InputRPM = val;
            }
            if (speedValueText != null)
            {
                speedValueText.text = $"{Mathf.RoundToInt(val)} RPM";
            }
            if (expSpeedValueText != null)
            {
                expSpeedValueText.text = $"{Mathf.RoundToInt(val)} RPM";
            }
        }

        private void OnExpTorqueChanged(float val)
        {
            if (activeMechanism != null)
            {
                activeMechanism.InputTorque = val;
            }
            if (expTorqueValueText != null)
            {
                expTorqueValueText.text = $"{val:F1} N·m";
            }
            UpdateCalculationsDisplay();
        }

        private void OnExpEfficiencyChanged(float val)
        {
            if (activeMechanism != null)
            {
                activeMechanism.Efficiency = val;
            }
            if (expEfficiencyValueText != null)
            {
                expEfficiencyValueText.text = $"{(val * 100f):F1} %";
            }
            UpdateCalculationsDisplay();
        }

        public void ApplyPreset(float rpm, float torque, float efficiency)
        {
            if (activeMechanism != null)
            {
                activeMechanism.InputRPM = rpm;
                activeMechanism.InputTorque = torque;
                activeMechanism.Efficiency = efficiency;

                if (speedSlider != null) speedSlider.value = rpm;
                if (expTorqueSlider != null) expTorqueSlider.value = torque;
                if (expEfficiencySlider != null) expEfficiencySlider.value = efficiency;

                UpdateCalculationsDisplay();
            }
        }

        public void CaptureExperimentA()
        {
            if (activeMechanism == null) return;

            snapshotA.inputRPM = activeMechanism.InputRPM;
            snapshotA.torque = activeMechanism.InputTorque;
            snapshotA.efficiency = activeMechanism.Efficiency;
            snapshotA.outputRPM = activeMechanism.OutputRPM;
            snapshotA.outputTorque = activeMechanism.OutputTorqueNm;
            snapshotA.outputPower = activeMechanism.OutputPowerWatts;
            snapshotA.powerLoss = activeMechanism.PowerLossWatts;
            snapshotA.isCaptured = true;

            UpdateCalculationsDisplay();
        }

        public void ToggleExperimentLab()
        {
            isLabVisible = !isLabVisible;
            if (experimentLabPanel != null)
            {
                experimentLabPanel.SetActive(isLabVisible);
                if (isLabVisible)
                {
                    if (activeMechanism != null)
                    {
                        if (expTorqueSlider != null) expTorqueSlider.value = activeMechanism.InputTorque;
                        if (expEfficiencySlider != null) expEfficiencySlider.value = activeMechanism.Efficiency;
                    }
                    UpdateCalculationsDisplay();
                }
            }
        }

        public void SwitchGraphTab(int tabIndex)
        {
            currentGraphTab = tabIndex;
            UpdateGraphVisuals();
        }

        private void UpdateGraphTelemetry()
        {
            if (activeMechanism == null) return;

            float currentSpeed = activeMechanism.InputRPM;
            speedHistory.Add(currentSpeed);
            if (speedHistory.Count > 30) speedHistory.RemoveAt(0);

            UpdateGraphVisuals();
        }

        private void UpdateGraphVisuals()
        {
            if (activeMechanism == null) return;

            switch (currentGraphTab)
            {
                case 0: // RPM: Input vs Output
                    if (graphTitleText != null) graphTitleText.text = "GRAPH: INPUT VELOCITY vs OUTPUT VELOCITY (RPM)";
                    SetBarHeight(graphBarInput, Mathf.Clamp01(activeMechanism.InputRPM / 300f));
                    SetBarHeight(graphBarOutput, Mathf.Clamp01(Mathf.Abs(activeMechanism.OutputRPM) / 300f));
                    SetBarHeight(graphBarLoss, 0f);
                    if (graphBarInputLabel != null) graphBarInputLabel.text = $"IN: {activeMechanism.InputRPM:F0}";
                    if (graphBarOutputLabel != null) graphBarOutputLabel.text = $"OUT: {activeMechanism.OutputRPM:F0}";
                    if (graphBarLossLabel != null) graphBarLossLabel.text = $"RATIO: {activeMechanism.GearRatio:F2}:1";
                    break;

                case 1: // TORQUE: Input vs Multiplied Output
                    if (graphTitleText != null) graphTitleText.text = "GRAPH: INPUT TORQUE vs OUTPUT TORQUE (N·m)";
                    SetBarHeight(graphBarInput, Mathf.Clamp01(activeMechanism.InputTorque / 100f));
                    SetBarHeight(graphBarOutput, Mathf.Clamp01(activeMechanism.OutputTorqueNm / 200f));
                    SetBarHeight(graphBarLoss, 0f);
                    if (graphBarInputLabel != null) graphBarInputLabel.text = $"T_in: {activeMechanism.InputTorque:F1}";
                    if (graphBarOutputLabel != null) graphBarOutputLabel.text = $"T_out: {activeMechanism.OutputTorqueNm:F1}";
                    if (graphBarLossLabel != null) graphBarLossLabel.text = $"η: {(activeMechanism.Efficiency * 100f):F0}%";
                    break;

                case 2: // SPEED vs TIME
                    if (graphTitleText != null) graphTitleText.text = "GRAPH: SPEED TELEMETRY OVER TIME (30 SAMPLES)";
                    SetBarHeight(graphBarInput, Mathf.Clamp01(activeMechanism.InputRPM / 300f));
                    SetBarHeight(graphBarOutput, Mathf.Clamp01(Mathf.Abs(activeMechanism.OutputRPM) / 300f));
                    SetBarHeight(graphBarLoss, Mathf.Clamp01(activeMechanism.InputAngularVelocityRadPerSec / 32f));
                    if (graphBarInputLabel != null) graphBarInputLabel.text = $"N: {activeMechanism.InputRPM:F0}";
                    if (graphBarOutputLabel != null) graphBarOutputLabel.text = $"ω: {activeMechanism.InputAngularVelocityRadPerSec:F1}";
                    if (graphBarLossLabel != null) graphBarLossLabel.text = $"SAMPLES: {speedHistory.Count}";
                    break;

                case 3: // POWER & LOSS (W)
                default:
                    if (graphTitleText != null) graphTitleText.text = "GRAPH: POWER TRANSMISSION & HEAT DISSIPATION (WATTS)";
                    float maxP = Mathf.Max(activeMechanism.InputPowerWatts, 1f);
                    SetBarHeight(graphBarInput, Mathf.Clamp01(activeMechanism.InputPowerWatts / maxP));
                    SetBarHeight(graphBarOutput, Mathf.Clamp01(activeMechanism.OutputPowerWatts / maxP));
                    SetBarHeight(graphBarLoss, Mathf.Clamp01(activeMechanism.PowerLossWatts / maxP));
                    if (graphBarInputLabel != null) graphBarInputLabel.text = $"P_in: {activeMechanism.InputPowerWatts:F0}W";
                    if (graphBarOutputLabel != null) graphBarOutputLabel.text = $"P_out: {activeMechanism.OutputPowerWatts:F0}W";
                    if (graphBarLossLabel != null) graphBarLossLabel.text = $"LOSS: {activeMechanism.PowerLossWatts:F0}W";
                    break;
            }
        }

        private void SetBarHeight(RectTransform rt, float normalizedHeight)
        {
            if (rt == null) return;
            Vector2 aMax = rt.anchorMax;
            aMax.y = Mathf.Lerp(0.18f, 0.76f, normalizedHeight);
            rt.anchorMax = aMax;
        }

        private void UpdateCalculationsDisplay()
        {
            if (activeMechanism == null) return;

            float ratio = activeMechanism.GearRatio;
            float inRPM = activeMechanism.InputRPM;
            float outRPM = activeMechanism.OutputRPM;
            float inOmega = activeMechanism.InputAngularVelocityRadPerSec;
            float outOmega = activeMechanism.OutputAngularVelocityRadPerSec;
            float inPower = activeMechanism.InputPowerWatts;
            float outPower = activeMechanism.OutputPowerWatts;
            float outTorque = activeMechanism.OutputTorqueNm;
            float powerLoss = activeMechanism.PowerLossWatts;
            string dirStr = activeMechanism.IsReversed ? "Counter-Clockwise (CCW)" : "Clockwise (CW)";

            // Update bottom HUD calculations text
            if (calculationsContentText != null)
            {
                calculationsContentText.text =
                    $"<b>Kinematic Telemetry:</b>\n" +
                    $"• Input Velocity (n₁): <b>{inRPM:F1} RPM</b> ({inOmega:F1} rad/s) [{dirStr}]\n" +
                    $"• Output Velocity (n₂): <b>{outRPM:F1} RPM</b> ({outOmega:F1} rad/s)\n" +
                    $"• Speed Ratio (i): <b>{ratio:F2} : 1</b>\n" +
                    $"• Power Transmission: <b>P_in: {inPower:F1} W</b> → <b>P_out: {outPower:F1} W</b>\n" +
                    $"• Output Torque (T₂): <b>{outTorque:F1} N·m</b> (Loss: {powerLoss:F1} W)\n" +
                    $"• Simulation State: <b>{(activeMechanism.IsPaused ? "PAUSED" : "RUNNING")}</b>";
            }

            // Update Experiment Lab comprehensive telemetry text
            if (expTelemetryText != null)
            {
                expTelemetryText.text =
                    $"<b>INPUT:</b>  n₁ = <b>{inRPM:F0} RPM</b> | ω₁ = <b>{inOmega:F1} rad/s</b> | T₁ = <b>{activeMechanism.InputTorque:F1} N·m</b> | P₁ = <b>{inPower:F1} W</b>\n" +
                    $"<b>OUTPUT:</b> n₂ = <b>{outRPM:F1} RPM</b> | ω₂ = <b>{outOmega:F1} rad/s</b> | T₂ = <b>{outTorque:F1} N·m</b> | P₂ = <b>{outPower:F1} W</b>\n" +
                    $"<b>DYNAMICS:</b> Ratio i = <b>{ratio:F2}:1</b> | Efficiency η = <b>{(activeMechanism.Efficiency * 100f):F1}%</b> | Dissipated Loss = <b>{powerLoss:F1} W</b>";
            }

            // Update A/B Comparison text
            if (comparisonText != null)
            {
                if (snapshotA.isCaptured)
                {
                    float dRPM = inRPM - snapshotA.inputRPM;
                    float dOutRPM = outRPM - snapshotA.outputRPM;
                    float dTorque = outTorque - snapshotA.outputTorque;
                    float dLoss = powerLoss - snapshotA.powerLoss;

                    comparisonText.text =
                        $"<b>EXP A:</b> {snapshotA.inputRPM:F0} RPM → {snapshotA.outputRPM:F1} RPM | {snapshotA.outputTorque:F1} N·m | Loss: {snapshotA.powerLoss:F1}W\n" +
                        $"<b>EXP B:</b> {inRPM:F0} RPM → {outRPM:F1} RPM | {outTorque:F1} N·m | Loss: {powerLoss:F1}W\n" +
                        $"<b>DELTAS:</b> Δn₁: {(dRPM >= 0 ? "+" : "")}{dRPM:F0} | Δn₂: {(dOutRPM >= 0 ? "+" : "")}{dOutRPM:F1} | ΔT₂: {(dTorque >= 0 ? "+" : "")}{dTorque:F1} N·m | ΔLoss: {(dLoss >= 0 ? "+" : "")}{dLoss:F1} W";
                }
                else
                {
                    comparisonText.text = "Tap 'CAPTURE EXPERIMENT A' to freeze a baseline snapshot and compare live differences.";
                }
            }

            // Update Governing Equations text across all 14 mechanisms
            if (formulasContentText != null)
            {
                if (activeMechanism is SpurGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Spur Gear Pair):</b>\n" +
                        $"• Velocity Ratio: i = Z₂ / Z₁ = 40 / 20 = 2.00\n" +
                        $"• Output Speed: n₂ = -n₁ · (Z₁ / Z₂) = -0.50 · n₁\n" +
                        $"• Center Distance: a = m · (Z₁ + Z₂) / 2 = 105.0 mm\n" +
                        $"• Power: P = T · ω | T₂ = η · T₁ · i = {outTorque:F1} N·m";
                }
                else if (activeMechanism is BevelGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Bevel Gear Pair):</b>\n" +
                        $"• Shaft Angle: Σ = δ₁ + δ₂ = 26.57° + 63.43° = 90.00°\n" +
                        $"• Pitch Cone: tan(δ₁) = Z₁ / Z₂ = 16 / 32 = 0.50\n" +
                        $"• Velocity Ratio: i = Z₂ / Z₁ = 32 / 16 = 2.00\n" +
                        $"• Power Transmission: Orthogonal angular torque transfer";
                }
                else if (activeMechanism is WormGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Worm & Wheel):</b>\n" +
                        $"• Lead Angle: tan(γ) = p_z / (π · d₁) → γ = 7.39°\n" +
                        $"• Friction Angle: ρ' = arctan(μ / cos α) = 8.53°\n" +
                        $"• Self-Locking Condition: γ (7.39°) < ρ' (8.53°) [VERIFIED]\n" +
                        $"• Ratio: i = z₂ / z₁ = 30 / 1 = 30.00 : 1";
                }
                else if (activeMechanism is PlanetaryGearSystem)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Planetary System):</b>\n" +
                        $"• Willis Epicyclic Law: (ω_s - ω_c) / (ω_r - ω_c) = -Z_r / Z_s\n" +
                        $"• Fixed Ring Ratio: i = (Z_s + Z_r) / Z_s = (18 + 54) / 18 = 4.00 : 1\n" +
                        $"• Output Carrier Speed: n_carrier = n_sun / 4.0\n" +
                        $"• High power density coaxial speed reduction";
                }
                else if (activeMechanism is RackAndPinionPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Rack & Pinion):</b>\n" +
                        $"• Linear Pitch: p = π · m = 3.5π = 11.00 mm\n" +
                        $"• Linear Travel: S_rev = π · m · Z = 220.0 mm / rev\n" +
                        $"• Tangential Velocity: v = ω · r_pitch = {inOmega * 0.035f:F3} m/s\n" +
                        $"• Motion Conversion: Continuous rotary to reciprocating linear slide";
                }
                else if (activeMechanism is BeltAndPulleySystem)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Belt & Pulley):</b>\n" +
                        $"• Speed Ratio: i = D₂ / D₁ = 120 / 60 = 2.00 : 1\n" +
                        $"• Belt Velocity: v = π · D₁ · (n₁ / 60) = {Mathf.PI * 0.06f * (inRPM / 60f):F3} m/s\n" +
                        $"• Direction: Identical parallel rotation (open loop)";
                }
                else if (activeMechanism is SimpleGearTrain)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Simple Gear Train with Idler):</b>\n" +
                        $"• Intermediate Idler: n₂ = -n₁ · (Z₁ / Z₂) = -n₁ · (20 / 28)\n" +
                        $"• Output Driven Gear: n₃ = -n₂ · (Z₂ / Z₃) = +n₁ · (Z₁ / Z₃) = +0.50 · n₁\n" +
                        $"• Overall Ratio: i = Z₃ / Z₁ = 40 / 20 = 2.00 : 1 (Independent of Idler Z₂!)\n" +
                        $"• Direction: Identical rotation (Idler reverses motion twice)";
                }
                else if (activeMechanism is CompoundGearTrain)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Compound Gear Train):</b>\n" +
                        $"• Stage 1 Ratio: i₁ = Z₂ / Z₁ = 36 / 18 = 2.00\n" +
                        $"• Stage 2 Ratio: i₂ = Z₄ / Z₃ = 54 / 18 = 3.00\n" +
                        $"• Total Multiplied Ratio: i = i₁ · i₂ = 2.0 · 3.0 = 6.00 : 1\n" +
                        $"• Massive torque multiplier in compact footprint";
                }
                else if (activeMechanism is RevertedGearTrain)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Reverted Gear Train):</b>\n" +
                        $"• Equal Center Distance: a₁ = r₁ + r₂ = a₂ = r₃ + r₄ = 105.0 mm\n" +
                        $"• Stage Ratios: i₁ = 40/20 = 2.00 | i₂ = 36/24 = 1.50\n" +
                        $"• Overall Ratio: i = i₁ · i₂ = 3.00 : 1\n" +
                        $"• Alignment: Coaxial input and output shafts (0.0 mm radial offset)";
                }
                else if (activeMechanism is InternalGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Internal Gear Pair):</b>\n" +
                        $"• Center Distance: a = m · (Z₂ - Z₁) / 2 = 3.5 · (60 - 20) / 2 = 70.0 mm\n" +
                        $"• Speed Ratio: i = Z₂ / Z₁ = 60 / 20 = 3.00 : 1\n" +
                        $"• Kinematic Direction: SAME ROTATIONAL DIRECTION (CW → CW)\n" +
                        $"• Higher tooth contact ratio and lower sliding friction";
                }
                else if (activeMechanism is HelicalGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Helical Gear Pair):</b>\n" +
                        $"• Helix Angles: β₁ = +20.0° (Right-hand) | β₂ = -20.0° (Left-hand)\n" +
                        $"• Axial Thrust: F_a = F_t · tan(β) = F_t · tan(20°) = 0.364 · F_t\n" +
                        $"• Ratio: i = Z₂ / Z₁ = 40 / 20 = 2.00 : 1\n" +
                        $"• Progressive tooth engagement eliminates impact shocks";
                }
                else if (activeMechanism is HerringboneGearPair)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Herringbone Double-Helical):</b>\n" +
                        $"• Chevron Opposed Helices: +20.0° (front) and -20.0° (back)\n" +
                        $"• Thrust Cancellation: F_a1 + F_a2 = +F_t·tan(20°) - F_t·tan(20°) = 0.0 N\n" +
                        $"• Ratio: i = Z₂ / Z₁ = 40 / 20 = 2.00 : 1\n" +
                        $"• Eliminates thrust bearings; extreme heavy-duty power capacity";
                }
                else if (activeMechanism is AutomotiveDifferential)
                {
                    AutomotiveDifferential diff = activeMechanism as AutomotiveDifferential;
                    float bias = diff != null ? diff.TurnBias : 0f;
                    formulasContentText.text =
                        $"<b>Governing Equations (Automotive Differential):</b>\n" +
                        $"• Willis Axle Law: ω_carrier = (ω_Left + ω_Right) / 2\n" +
                        $"• Final Drive: i_d = Z_r / Z_p = 36 / 12 = 3.00 : 1\n" +
                        $"• Left Wheel: {diff?.LeftAxleRPM:F1} RPM | Right Wheel: {diff?.RightAxleRPM:F1} RPM\n" +
                        $"• Turn Bias: {bias:F2} ({(bias < 0 ? "Left Turn" : bias > 0 ? "Right Turn" : "Straight")})";
                }
                else if (activeMechanism is HarmonicDrive)
                {
                    formulasContentText.text =
                        $"<b>Governing Equations (Harmonic Strain Wave Drive):</b>\n" +
                        $"• Teeth Differential: ΔZ = Z_c - Z_f = 50 - 48 = 2 teeth\n" +
                        $"• Reduction Ratio: i = Z_f / (Z_c - Z_f) = 48 / 2 = 24.00 : 1\n" +
                        $"• Output Speed: n_flex = -n_wave / 24.0 = {outRPM:F1} RPM (Counter-rotating!)\n" +
                        $"• Zero backlash precision robotic joint actuator";
                }
                else if (activeMechanism is FourStrokeEngine)
                {
                    FourStrokeEngine eng = activeMechanism as FourStrokeEngine;
                    string inValve = (eng != null && eng.IsIntakeValveOpen) ? "OPEN" : "CLOSED";
                    string exValve = (eng != null && eng.IsExhaustValveOpen) ? "OPEN" : "CLOSED";
                    formulasContentText.text =
                        $"<b>Governing Equations (Four-Stroke IC Engine):</b>\n" +
                        $"• Kinematic Cycle: 720° (2 full crankshaft revolutions per power stroke)\n" +
                        $"• Camshaft Speed: N_cam = N_crank / 2 = {eng?.InputRPM * 0.5f:F1} RPM\n" +
                        $"• Current Stroke: {eng?.CurrentStroke} (θ = {eng?.CrankshaftAngleDeg:F0}°)\n" +
                        $"• Piston Displacement: s = {eng?.DisplacementFromTDC * 1000f:F1} mm | v = {eng?.PistonVelocityMps:F2} m/s\n" +
                        $"• Valves: Intake {inValve}, Exhaust {exValve}";
                }
                else if (activeMechanism is SliderCrankMechanism)
                {
                    SliderCrankMechanism sc = activeMechanism as SliderCrankMechanism;
                    formulasContentText.text =
                        $"<b>Governing Equations (Slider-Crank Mechanism):</b>\n" +
                        $"• Slider Displacement: x(θ) = r·cosθ + √(l² - r²·sin²θ)\n" +
                        $"• Connecting Rod Ratio: λ = r / l = {sc?.RodCrankRatio:F3} (r={sc?.CrankRadius * 1000f:F0}mm, l={sc?.RodLength * 1000f:F0}mm)\n" +
                        $"• Velocity: v(θ) = -r·ω·(sinθ + (r·sin2θ)/(2√(l² - r²·sin²θ)))\n" +
                        $"• Piston Velocity: {sc?.PistonVelocityMps:F2} m/s | Accel: {sc?.PistonAccelerationMps2:F1} m/s²";
                }
                else if (activeMechanism is CamAndFollower)
                {
                    CamAndFollower cf = activeMechanism as CamAndFollower;
                    formulasContentText.text =
                        $"<b>Governing Equations (Cam and Roller Follower):</b>\n" +
                        $"• Cam Profile: Base radius Rb = {cf?.BaseRadius * 1000f:F0} mm, Max lift h = {cf?.MaxLift * 1000f:F0} mm\n" +
                        $"• Follower Displacement: s(θ) = {cf?.FollowerLiftMeters * 1000f:F1} mm\n" +
                        $"• Follower Velocity: v(θ) = {cf?.FollowerVelocityMps:F2} m/s\n" +
                        $"• Harmonic rise, dwell and return profiles";
                }
                else if (activeMechanism is ReciprocatingPump)
                {
                    ReciprocatingPump rp = activeMechanism as ReciprocatingPump;
                    string sucValve = (rp != null && rp.IsSuctionValveOpen) ? "OPEN (Intake)" : "CLOSED";
                    string delValve = (rp != null && rp.IsDeliveryValveOpen) ? "OPEN (Delivery)" : "CLOSED";
                    formulasContentText.text =
                        $"<b>Governing Equations (Reciprocating Plunger Pump):</b>\n" +
                        $"• Stroke Length: S = {rp?.StrokeMeters * 1000f:F1} mm (Crank r = {rp?.CrankRadius * 1000f:F0} mm)\n" +
                        $"• Theoretical Flow Rate: Q = {rp?.TheoreticalDischargeLpm:F2} L/min\n" +
                        $"• Operating Phase: {rp?.CurrentStroke} (θ = {rp?.CrankAngleDeg:F0}°)\n" +
                        $"• Valves: Suction {sucValve}, Delivery {delValve}";
                }
                else if (activeMechanism is FourBarLinkage)
                {
                    FourBarLinkage fb = activeMechanism as FourBarLinkage;
                    string grashofStr = (fb != null && fb.IsGrashof) ? "GRASHOF SATISFIED" : "NON-GRASHOF";
                    formulasContentText.text =
                        $"<b>Governing Equations (Planar Four-Bar Linkage):</b>\n" +
                        $"• Grashof Law: s + l ≤ p + q -> {grashofStr}\n" + 
                        $"• Link Lengths: Ground={fb?.GroundLength * 1000f:F0}mm, Crank={fb?.CrankLength * 1000f:F0}mm, Coupler={fb?.CouplerLength * 1000f:F0}mm, Rocker={fb?.RockerLength * 1000f:F0}mm\n" +
                        $"• Output Rocker Angle: θ₄ = {fb?.RockerAngleDeg:F1}°\n" +
                        $"• Smooth 1-DOF planar kinematic loop closure";
                }
                else if (activeMechanism is CrankRockerMechanism)
                {
                    CrankRockerMechanism cr = activeMechanism as CrankRockerMechanism;
                    formulasContentText.text =
                        $"<b>Governing Equations (Crank-Rocker Mechanism):</b>\n" +
                        $"• Grashof Criterion: Input crank rotates continuously through 360°\n" +
                        $"• Quick-Return Ratio: Q = {cr?.QuickReturnRatio:F2}\n" +
                        $"• Output Rocker Angle: ψ = {cr?.RockerAngleDeg:F1}°\n" +
                        $"• Rocker Angular Velocity: {cr?.RockerAngularVelocityDegPerSec:F1}°/s";
                }
                else if (activeMechanism is GenevaMechanism)
                {
                    GenevaMechanism gen = activeMechanism as GenevaMechanism;
                    formulasContentText.text =
                        $"<b>Governing Equations (Four-Slot Geneva Drive):</b>\n" +
                        $"• Slot Count: n = {gen?.SlotCount} (Indexing angle: 360° / 4 = 90.0°)\n" +
                        $"• Dwell Angle: 270° (75% stationary dwell) | Index Angle: 90° (25% active advance)\n" +
                        $"• Current Station: Station #{gen?.CurrentStation} (Phase: {gen?.CurrentState})\n" +
                        $"• Center Distance: C = R₁ / sin(45°) = {gen?.CenterDistance * 1000f:F1} mm (R₁ = {gen?.CrankRadius * 1000f:F0} mm)\n" +
                        $"• Average Velocity Ratio: i = 4.00 : 1 (Output = {gen?.OutputRPM:F1} RPM)";
                }
                else if (activeMechanism is ScotchYokeMechanism)
                {
                    ScotchYokeMechanism sy = activeMechanism as ScotchYokeMechanism;
                    formulasContentText.text =
                        $"<b>Governing Equations (Scotch Yoke Mechanism):</b>\n" +
                        $"• Kinematic Model: Mathematically Exact Simple Harmonic Motion (SHM)\n" +
                        $"• Yoke Displacement: x(t) = r·cos(ωt) = {sy?.DisplacementX * 1000f:F1} mm (r = {sy?.CrankRadius * 1000f:F0} mm)\n" +
                        $"• Yoke Velocity: v(t) = -r·ω·sin(ωt) = {sy?.VelocityMps:F2} m/s\n" +
                        $"• Yoke Acceleration: a(t) = -r·ω²·cos(ωt) = {sy?.AccelerationMps2:F1} m/s²\n" +
                        $"• Total Stroke: S = {sy?.StrokeMeters * 1000f:F0} mm | Zero angularity harmonic distortion";
                }
                else if (activeMechanism is UniversalJoint)
                {
                    UniversalJoint uj = activeMechanism as UniversalJoint;
                    formulasContentText.text =
                        $"<b>Governing Equations (Universal / Cardan Joint):</b>\n" +
                        $"• Cardan Relationship: tan(θ₂) = tan(θ₁)·cos(β) (Operating Angle β = {uj?.OperatingAngleDeg:F1}°)\n" +
                        $"• Instantaneous Velocity Ratio: ω₂/ω₁ = cos(β) / (1 - sin²(β)·cos²(θ₁)) = {uj?.InstantaneousVelocityRatio:F3}\n" +
                        $"• Peak Speed Ratio [sec(β)]: {uj?.MaxSpeedRatio:F3}x | Trough [cos(β)]: {uj?.MinSpeedRatio:F3}x\n" +
                        $"• Speed Ripple: ±{uj?.SpeedFluctuationPercent * 0.5f:F1}% (2nd harmonic fluctuation at 2·ω₁)";
                }
                else if (activeMechanism is ChainAndSprocketDrive)
                {
                    ChainAndSprocketDrive ch = activeMechanism as ChainAndSprocketDrive;
                    formulasContentText.text =
                        $"<b>Governing Equations (Roller Chain & Sprocket Drive):</b>\n" +
                        $"• Speed Ratio: i = Z₂ / Z₁ = {ch?.DrivenTeeth} / {ch?.DriverTeeth} = {ch?.GearRatio:F2} : 1\n" +
                        $"• Chain Pitch Velocity: v = (Z₁·p·N₁) / 60 = {ch?.ChainLinearSpeedMps:F2} m/s ({ch?.ChainLinearSpeedMps * 3.6f:F1} km/h)\n" +
                        $"• Pitch Diameters: D₁ = {ch?.DriverPitchRadius * 2000f:F1} mm, D₂ = {ch?.DrivenPitchRadius * 2000f:F1} mm\n" +
                        $"• Chordal Action (Polygonal Effect): Δv/v = 1 - cos(180°/Z₁) = {ch?.ChordalActionVariationPercent:F2}%\n" +
                        $"• Both sprockets rotate in the SAME direction with positive mechanical lock";
                }
                else if (activeMechanism is RatchetAndPawl)
                {
                    RatchetAndPawl rp = activeMechanism as RatchetAndPawl;
                    formulasContentText.text =
                        $"<b>Governing Equations (Ratchet & Pawl Mechanism):</b>\n" +
                        $"• Asymmetric Teeth: Z = {rp?.ToothCount} teeth (Pitch step: Δθ = 360° / 24 = {rp?.PitchAngleDeg:F1}°)\n" +
                        $"• Average Velocity Ratio: i = {rp?.ToothCount} : 1 (Unidirectional step-down)\n" +
                        $"• State: {(rp != null && rp.IsDrivingStroke ? "Forward Drive Advance (0°–180°)" : "Stationary Locked Dwell (180°–360°)")}\n" +
                        $"• Cumulative Steps Indexed: {rp?.TotalSteps} steps ({rp?.CumulativeWheelAngleDeg:F0}°)\n" +
                        $"• Stationary holding pawl provides positive anti-backlash reverse arrest";
                }
                else if (activeMechanism is LeadScrewAndNut)
                {
                    LeadScrewAndNut ls = activeMechanism as LeadScrewAndNut;
                    formulasContentText.text =
                        $"<b>Governing Equations (Trapezoidal Lead Screw & Nut):</b>\n" +
                        $"• Screw Specifications: d = {ls?.ScrewDiameterMeters * 1000f:F0} mm, Lead L = {ls?.LeadMeters * 1000f:F0} mm/rev (Single start Acme)\n" +
                        $"• Helix Lead Angle: λ = {ls?.LeadAngleDeg:F2}° (Self-locking: {ls?.IsSelfLocking})\n" +
                        $"• Linear Carriage Velocity: v = (L·N) / 60 = {ls?.LinearSpeedMps * 1000f:F1} mm/s\n" +
                        $"• Axial Thrust at {ls?.InputTorque:F1} N·m: F = (2·π·η·T)/L = {ls?.AxialThrustNewtons:F0} N (~{ls?.AxialThrustNewtons / 9.81f:F0} kgf)\n" +
                        $"• Force Multiplication Ratio: {ls?.ThrustPerTorqueRatio:F0} N per N·m of input torque";
                }
            }
        }

        private void OnToggleDetachCurrentComponent()
        {
            if (currentlyInspectedComponent != null)
            {
                if (currentlyInspectedComponent.IsDetached)
                {
                    currentlyInspectedComponent.Attach();
                    if (inspectDetachButtonText != null) inspectDetachButtonText.text = "DETACH ⤢";
                }
                else
                {
                    currentlyInspectedComponent.Detach();
                    if (inspectDetachButtonText != null) inspectDetachButtonText.text = "RETURN TO ASSEMBLY";
                }
            }
        }

        public void CloseInspectionCard()
        {
            currentlyInspectedComponent = null;
            if (inspectionCard != null) inspectionCard.SetActive(false);

            if (currentScreenState == AppScreenState.ComponentInspection)
            {
                if (activeMechanism != null && activeMechanism.IsExploded)
                {
                    SetScreenState(AppScreenState.ExplodedView);
                }
                else
                {
                    SetScreenState(AppScreenState.ARPlaced);
                }
            }
        }

        public void ToggleCalculationsCard()
        {
            isCalculationsVisible = !isCalculationsVisible;
            if (calculationsCard != null)
            {
                calculationsCard.SetActive(isCalculationsVisible);
            }
        }

        public void ShowDiagnosticScreen(bool show)
        {
            if (diagnosticPanel != null)
            {
                diagnosticPanel.SetActive(show);
            }
        }

        #endregion

        #region UI Element Helpers (macOS Laboratory Architecture)

        private GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool raycastTarget = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        private void CreateBorder(Transform parent, Color borderColor, float width = 1.0f)
        {
            GameObject border = new GameObject("BorderOutline");
            border.transform.SetParent(parent, false);
            border.transform.SetAsFirstSibling();

            Image img = border.AddComponent<Image>();
            img.color = borderColor;
            img.raycastTarget = false;

            RectTransform rt = border.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-width, -width);
            rt.offsetMax = new Vector2(width, width);
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, FontStyles style, Color color,
            TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (fontRegular != null)
            {
                tmp.font = (style == FontStyles.Bold && fontSemiBold != null) ? fontSemiBold : fontRegular;
                tmp.fontSharedMaterial = tmp.font.material;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;

            return tmp;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
            Color bgColor, Color textColor, float fontSize = 16, bool isAccent = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            CreateBorder(go.transform, isAccent ? colAccentCrimson : colBorderSubtle, 1.0f);

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = isAccent ? new Color(0.95f, 0.30f, 0.35f, 1f) : bgColor * 1.3f;
            cb.pressedColor = isAccent ? new Color(0.75f, 0.15f, 0.20f, 1f) : bgColor * 0.7f;
            cb.selectedColor = bgColor;
            btn.colors = cb;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            if (fontRegular != null)
            {
                tmp.font = fontSemiBold != null ? fontSemiBold : fontRegular;
                tmp.fontSharedMaterial = tmp.font.material;
            }

            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4f, 2f);
            textRt.offsetMax = new Vector2(-4f, -2f);

            return btn;
        }

        /// <summary>
        /// Precision UI Slider with full raycast surface and enlarged tactile handle.
        /// Resolves the root cause of unresponsive touch controls on mobile devices.
        /// </summary>
        private Slider CreateSlider(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, float minVal, float maxVal, float defaultVal)
        {
            GameObject sliderGo = new GameObject(name);
            sliderGo.transform.SetParent(parent, false);

            // Container Image with raycastTarget = true intercepts touch down anywhere within the slider bounds!
            Image sliderRaycastReceiver = sliderGo.AddComponent<Image>();
            sliderRaycastReceiver.color = Color.clear;
            sliderRaycastReceiver.raycastTarget = true;

            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.value = defaultVal;

            // Track Background: raycastTarget = true ensures finger taps on track immediately set slider value!
            GameObject bgGo = CreatePanel("Background", sliderGo.transform, new Color(0.161f, 0.169f, 0.188f, 1f),
                new Vector2(0f, 0.30f), new Vector2(1f, 0.70f), Vector2.zero, Vector2.zero, true);
            CreateBorder(bgGo.transform, colBorderSubtle, 1f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.30f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.70f);
            fillAreaRt.offsetMin = Vector2.zero;
            fillAreaRt.offsetMax = Vector2.zero;

            GameObject fill = CreatePanel("Fill", fillArea.transform, colAccentCrimson,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            slider.fillRect = fill.GetComponent<RectTransform>();

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = Vector2.zero;
            handleAreaRt.offsetMax = Vector2.zero;

            // Handle: 48px wide prominent tactile knob with border
            GameObject handle = CreatePanel("Handle", handleArea.transform, colTextPrimary,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-24f, 0), new Vector2(24f, 0), true);
            CreateBorder(handle.transform, colAccentCrimson, 1.5f);

            // Center indicator dot on handle
            CreatePanel("HandleDot", handle.transform, colAccentCrimson,
                new Vector2(0.35f, 0.35f), new Vector2(0.65f, 0.65f), Vector2.zero, Vector2.zero, false);

            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();

            return slider;
        }

        #endregion
    }
}


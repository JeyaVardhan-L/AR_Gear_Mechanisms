using System;
using UnityEngine;
using MechXR.Interaction;

namespace MechXR.Mechanisms
{
    /// <summary>
    /// Base class for all mechanical gear assemblies.
    /// Coordinates kinematic simulation time, speed adjustment, pause/play, reversing,
    /// animated exploded view, component disassembly, and initial alignment resetting.
    /// </summary>
    public abstract class MechanismBase : MonoBehaviour
    {
        [Header("Kinematic Settings")]
        [SerializeField] protected float defaultInputRPM = 120f;
        [SerializeField] protected float minRPM = 10f;
        [SerializeField] protected float maxRPM = 300f;

        [Header("Scientific Experiment Parameters")]
        [SerializeField] protected float inputTorqueNm = 10.0f; // Default 10 N·m
        [SerializeField] protected float efficiency = 0.95f;    // Default 95% efficiency

        protected float currentInputRPM = 120f;
        protected bool isPaused = false;
        protected bool isReversed = false;

        protected bool isExploded = false;
        protected float currentExplosionProgress = 0f;
        private const float ExplosionTransitionSpeed = 4.0f;

        public event Action OnKinematicsUpdated;
        public event Action<bool> OnExplodedViewChanged;

        public void NotifyKinematicsUpdated() => OnKinematicsUpdated?.Invoke();

        public abstract string MechanismName { get; }
        public abstract string CategoryTitle { get; }
        public abstract float GearRatio { get; }
        public abstract float OutputRPM { get; }

        public virtual int DriverTeeth => 20;
        public virtual int DrivenTeeth => 40;

        public float InputRPM
        {
            get => currentInputRPM;
            set
            {
                currentInputRPM = Mathf.Clamp(value, minRPM, maxRPM);
                OnKinematicsUpdated?.Invoke();
            }
        }

        public float InputTorque
        {
            get => inputTorqueNm;
            set
            {
                inputTorqueNm = Mathf.Clamp(value, 0.1f, 500f);
                OnKinematicsUpdated?.Invoke();
            }
        }

        public float Efficiency
        {
            get => efficiency;
            set
            {
                efficiency = Mathf.Clamp(value, 0.50f, 1.0f);
                OnKinematicsUpdated?.Invoke();
            }
        }

        public float InputAngularVelocityRadPerSec => (currentInputRPM * 2f * Mathf.PI) / 60f;
        public float OutputAngularVelocityRadPerSec => (Mathf.Abs(OutputRPM) * 2f * Mathf.PI) / 60f;
        public float InputPowerWatts => inputTorqueNm * InputAngularVelocityRadPerSec;
        public float OutputPowerWatts => efficiency * InputPowerWatts;
        public float OutputTorqueNm => GearRatio > 0.001f ? (efficiency * inputTorqueNm * GearRatio) : 0f;
        public float PowerLossWatts => Mathf.Max(0f, InputPowerWatts - OutputPowerWatts);

        public float MinRPM => minRPM;
        public float MaxRPM => maxRPM;
        public bool IsPaused => isPaused;
        public bool IsReversed => isReversed;
        public bool IsExploded => isExploded;
        public float ExplosionProgress => currentExplosionProgress;

        protected virtual void Awake()
        {
            currentInputRPM = defaultInputRPM;
        }

        protected virtual void Update()
        {
            // Handle smooth animated exploded view transition
            float targetExplosion = isExploded ? 1.0f : 0.0f;
            if (Mathf.Abs(currentExplosionProgress - targetExplosion) > 0.001f)
            {
                currentExplosionProgress = Mathf.MoveTowards(currentExplosionProgress, targetExplosion, Time.deltaTime * ExplosionTransitionSpeed);
                UpdateChildExplosionFactors(currentExplosionProgress);
            }

            // Kinematic updates run when not paused and not fully exploded
            if (!isPaused && currentExplosionProgress < 0.95f)
            {
                float dt = Time.deltaTime;
                float dir = isReversed ? -1f : 1f;
                float inputAngularVelocityDegPerSec = (currentInputRPM * 360f / 60f) * dir;

                UpdateKinematics(inputAngularVelocityDegPerSec, dt);
            }
        }

        private void UpdateChildExplosionFactors(float progress)
        {
            InspectableComponent[] comps = GetComponentsInChildren<InspectableComponent>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                comps[i].SetExplodedFactor(progress);
            }
        }

        /// <summary>
        /// Perform frame-by-frame mechanical rotation updates.
        /// </summary>
        protected abstract void UpdateKinematics(float inputAngularVelDegPerSec, float deltaTime);

        /// <summary>
        /// Resets rotation angles to their initial meshing phase.
        /// </summary>
        public abstract void ResetMechanism();

        public virtual void TogglePause()
        {
            isPaused = !isPaused;
            OnKinematicsUpdated?.Invoke();
        }

        public virtual void SetPaused(bool paused)
        {
            isPaused = paused;
            OnKinematicsUpdated?.Invoke();
        }

        public virtual void ToggleReverse()
        {
            isReversed = !isReversed;
            OnKinematicsUpdated?.Invoke();
        }

        public virtual void SetReversed(bool reversed)
        {
            isReversed = reversed;
            OnKinematicsUpdated?.Invoke();
        }

        public virtual void ToggleExplodedView()
        {
            SetExplodedView(!isExploded);
        }

        public virtual void SetExplodedView(bool exploded)
        {
            isExploded = exploded;
            if (isExploded)
            {
                // Pause rotation while in exploded view so parts don't spin eccentrically
                isPaused = true;
            }
            OnExplodedViewChanged?.Invoke(isExploded);
            OnKinematicsUpdated?.Invoke();
        }

        public virtual void ReassembleAll()
        {
            isExploded = false;
            currentExplosionProgress = 0f;
            InspectableComponent[] comps = GetComponentsInChildren<InspectableComponent>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                comps[i].ResetToAssembly();
            }
            OnExplodedViewChanged?.Invoke(false);
            OnKinematicsUpdated?.Invoke();
        }

        public abstract string GetCalculationSummary();
        public abstract string GetEngineeringExplanation();
        public abstract string GetFormulaBreakdown();
    }
}

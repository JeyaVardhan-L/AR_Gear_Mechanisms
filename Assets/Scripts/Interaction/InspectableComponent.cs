using System;
using UnityEngine;

namespace MechXR.Interaction
{
    /// <summary>
    /// Attach to any 3D mechanical component to make it interactive, inspectable, detachable, and explodable.
    /// Provides engineering descriptions, technical specs, highlight feedback, and smooth animated detachment.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InspectableComponent : MonoBehaviour
    {
        [Header("Component Metadata")]
        [SerializeField] private string componentName = "Mechanical Component";
        [TextArea(2, 4)]
        [SerializeField] private string roleDescription = "Transmits mechanical motion.";
        [TextArea(2, 4)]
        [SerializeField] private string technicalSpecs = "Standard Engineering Grade";

        [Header("Visual Feedback")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color highlightColor = new Color(0.898f, 0.224f, 0.271f, 1.0f); // Restrained Crimson (#E53945)

        [Header("Disassembly & Inspection Vectors")]
        [SerializeField] private Vector3 explosionDirection = Vector3.up;
        [SerializeField] private float explosionDistance = 0.08f;
        [SerializeField] private float detachDistance = 0.12f;

        private Color originalColor = Color.white;
        private Material instanceMaterial;
        private bool isSelected = false;
        private bool isDetached = false;

        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;
        private Vector3 targetLocalPos;
        private Quaternion targetLocalRot;
        private float currentExplosionFactor = 0f;

        private const float LerpSpeed = 10f;

        public string ComponentName => componentName;
        public string RoleDescription => roleDescription;
        public string TechnicalSpecs => technicalSpecs;
        public bool IsSelected => isSelected;
        public bool IsDetached => isDetached;
        public Vector3 InitialLocalPos => initialLocalPos;
        public Quaternion InitialLocalRot => initialLocalRot;

        private void Awake()
        {
            initialLocalPos = transform.localPosition;
            initialLocalRot = transform.localRotation;
            targetLocalPos = initialLocalPos;
            targetLocalRot = initialLocalRot;

            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            if (targetRenderer != null)
            {
                instanceMaterial = targetRenderer.material;
                if (instanceMaterial.HasProperty("_Color"))
                {
                    originalColor = instanceMaterial.color;
                }
            }
        }

        private void Update()
        {
            // Smoothly glide toward target local position and rotation
            if (Vector3.Distance(transform.localPosition, targetLocalPos) > 0.0002f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * LerpSpeed);
            }
            else
            {
                transform.localPosition = targetLocalPos;
            }

            if (Quaternion.Angle(transform.localRotation, targetLocalRot) > 0.1f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, Time.deltaTime * LerpSpeed);
            }
            else
            {
                transform.localRotation = targetLocalRot;
            }
        }

        public void SetMetadata(string compName, string description, string specs)
        {
            componentName = compName;
            roleDescription = description;
            technicalSpecs = specs;
        }

        public void Initialize(string compName, string description, string specs, Vector3 explosionVector)
        {
            SetMetadata(compName, description, specs);
            float dist = explosionVector.magnitude;
            Vector3 dir = dist > 0.0001f ? explosionVector / dist : Vector3.up;
            SetExplosionVector(dir, dist, dist * 1.5f);
        }

        public void SetExplosionVector(Vector3 direction, float distance, float detachDist = 0.12f)
        {
            explosionDirection = direction.normalized;
            explosionDistance = distance;
            detachDistance = detachDist;
        }

        public void Select()
        {
            isSelected = true;
            if (instanceMaterial != null && instanceMaterial.HasProperty("_Color"))
            {
                instanceMaterial.color = highlightColor;
            }
        }

        public void Deselect()
        {
            isSelected = false;
            if (instanceMaterial != null && instanceMaterial.HasProperty("_Color"))
            {
                instanceMaterial.color = originalColor;
            }
        }

        public void Detach()
        {
            isDetached = true;
            targetLocalPos = initialLocalPos + explosionDirection * detachDistance;
        }

        public void Attach()
        {
            isDetached = false;
            // Return to assembled pose or exploded view pose depending on current explosion factor
            if (currentExplosionFactor > 0.001f)
            {
                targetLocalPos = initialLocalPos + explosionDirection * (explosionDistance * currentExplosionFactor);
            }
            else
            {
                targetLocalPos = initialLocalPos;
            }
            targetLocalRot = initialLocalRot;
        }

        public void RotateDetached(float deltaX, float deltaY)
        {
            if (!isDetached) return;
            // Rotate part around its own local axes for full 360 inspection
            Quaternion rotX = Quaternion.AngleAxis(-deltaX * 0.4f, Vector3.up);
            Quaternion rotY = Quaternion.AngleAxis(deltaY * 0.4f, Vector3.right);
            targetLocalRot = rotX * rotY * targetLocalRot;
        }

        public void SetExplodedFactor(float factor)
        {
            currentExplosionFactor = Mathf.Clamp01(factor);
            if (!isDetached)
            {
                targetLocalPos = initialLocalPos + explosionDirection * (explosionDistance * currentExplosionFactor);
            }
        }

        public void ResetToAssembly()
        {
            isDetached = false;
            currentExplosionFactor = 0f;
            targetLocalPos = initialLocalPos;
            targetLocalRot = initialLocalRot;
            transform.localPosition = initialLocalPos;
            transform.localRotation = initialLocalRot;
            Deselect();
        }

        private void OnDestroy()
        {
            if (instanceMaterial != null)
            {
                Destroy(instanceMaterial);
            }
        }
    }
}

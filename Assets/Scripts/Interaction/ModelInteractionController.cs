using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MechXR.Interaction
{
    /// <summary>
    /// Handles user gesture interactions on the placed AR model:
    /// - Tap to inspect mechanical components.
    /// - Single finger drag to rotate the assembly around Y axis (or rotate detached part when detached).
    /// - Two finger pinch to scale the model within safe limits (0.4x to 2.0x).
    /// </summary>
    public class ModelInteractionController : MonoBehaviour
    {
        [Header("Inspection Settings")]
        [SerializeField] private LayerMask inspectableLayer = ~0;

        [Header("Rotation & Scaling Settings")]
        [SerializeField] private float rotationSpeed = 0.4f;
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 2.5f;

        private InspectableComponent currentlySelected;
        private Vector2 lastTouchPos;
        private bool isDragging = false;
        private bool isPinching = false;

        public event Action<InspectableComponent> OnComponentSelected;
        public event Action OnComponentDeselected;
        public event Action<float> OnScaleChanged;

        public InspectableComponent CurrentlySelected => currentlySelected;
        public float CurrentScale => transform.localScale.x;
        public float MinScale => minScale;
        public float MaxScale => maxScale;

        private void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                if (isPinching)
                {
                    // Finger was lifted from pinch, reset drag to prevent sudden jump
                    isPinching = false;
                    isDragging = false;
                    return;
                }

                Touch touch = Input.GetTouch(0);

                if (IsPointerOverUI(touch.fingerId))
                    return;

                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPos = touch.position;
                    isDragging = false;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.position - lastTouchPos;
                    if (delta.sqrMagnitude > 25f) // Drag threshold
                    {
                        isDragging = true;
                        if (currentlySelected != null && currentlySelected.IsDetached)
                        {
                            // Rotate isolated detached component
                            currentlySelected.RotateDetached(delta.x, delta.y);
                        }
                        else
                        {
                            // Rotate entire assembly around Y
                            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
                        }
                        lastTouchPos = touch.position;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (!isDragging)
                    {
                        PerformRaycastInspection(touch.position);
                    }
                    isDragging = false;
                }
            }
            else if (Input.touchCount >= 2)
            {
                isPinching = true;
                isDragging = false;

                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                if (IsPointerOverUI(t0.fingerId) || IsPointerOverUI(t1.fingerId))
                    return;

                Vector2 prevPos0 = t0.position - t0.deltaPosition;
                Vector2 prevPos1 = t1.position - t1.deltaPosition;

                float prevDistance = (prevPos0 - prevPos1).magnitude;
                float currentDistance = (t0.position - t1.position).magnitude;

                if (prevDistance > 10f && currentDistance > 10f)
                {
                    float factor = currentDistance / prevDistance;
                    ApplyScale(factor);
                }
            }
            else
            {
                isPinching = false;
                isDragging = false;
            }
        }

        private void HandleMouseInput()
        {
            if (Input.touchCount > 0) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                lastTouchPos = Input.mousePosition;
                isDragging = false;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 currentPos = (Vector2)Input.mousePosition;
                Vector2 delta = currentPos - lastTouchPos;
                if (delta.sqrMagnitude > 16f)
                {
                    isDragging = true;
                    if (currentlySelected != null && currentlySelected.IsDetached)
                    {
                        currentlySelected.RotateDetached(delta.x, delta.y);
                    }
                    else
                    {
                        transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
                    }
                    lastTouchPos = currentPos;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!isDragging)
                {
                    PerformRaycastInspection(Input.mousePosition);
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ApplyScale(1f + scroll * 0.8f);
            }
        }

        public void ApplyScale(float factor)
        {
            Vector3 current = transform.localScale;
            float newScale = Mathf.Clamp(current.x * factor, minScale, maxScale);
            transform.localScale = Vector3.one * newScale;
            OnScaleChanged?.Invoke(newScale);
        }

        public void SetAbsoluteScale(float normalizedValue)
        {
            float target = Mathf.Lerp(minScale, maxScale, normalizedValue);
            transform.localScale = Vector3.one * target;
            OnScaleChanged?.Invoke(target);
        }

        private void PerformRaycastInspection(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, inspectableLayer))
            {
                InspectableComponent comp = hit.collider.GetComponentInParent<InspectableComponent>();
                if (comp != null)
                {
                    SelectComponent(comp);
                    return;
                }
            }

            DeselectCurrent();
        }

        public void SelectComponent(InspectableComponent comp)
        {
            if (currentlySelected != null && currentlySelected != comp)
            {
                currentlySelected.Deselect();
            }

            currentlySelected = comp;
            currentlySelected.Select();
            OnComponentSelected?.Invoke(currentlySelected);
        }

        public void DeselectCurrent()
        {
            if (currentlySelected != null)
            {
                currentlySelected.Deselect();
                currentlySelected = null;
                OnComponentDeselected?.Invoke();
            }
        }

        private bool IsPointerOverUI(int fingerId)
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        }
    }
}

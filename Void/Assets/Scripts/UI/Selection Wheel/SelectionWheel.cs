using System;
using UnityEngine;

public class SelectionWheel : MonoBehaviour
{
    [Header("Selection Wheel")]
    [SerializeField] private GameObject sectionPrefab;
    [Range(0, 360)]
    [SerializeField] private float wheelOffset;
    [SerializeField] private float radius;
    [SerializeField] private int count;
    [SerializeField] private float angleMargin;

    [Header("Options")]
    [SerializeField] private bool allowInnerSelection;

    private WheelSection[] wheelSections;
    private WheelSection selectedWheelSection;
    private bool active;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        GenerateSelectionWheel();
        ActivateSelectionWheel();
    }

    public void ActivateSelectionWheel()
    {
        active = true;
    }

    public void DeactivateSelectionWheel()
    {
        if (selectedWheelSection) selectedWheelSection.Activate();
        active = false;
    }

    private void FixedUpdate()
    {
        if (!active) return;

        UpdateCurrentSelection();
    }

    private void ResetSelectionWheel()
    {
        if (wheelSections == null) return;

        for (int i = 0; i < wheelSections.Length; i++)
        {
            Destroy(wheelSections[i]);
        }
    }

    [ContextMenu("Generate Selection Wheel")]
    public void GenerateSelectionWheel()
    {
        ResetSelectionWheel();

        wheelSections = new WheelSection[count];
        float intervalAngle = 360f / count;
        for (int i = 0; i < count; i++)
        {
            GameObject section = Instantiate(sectionPrefab, transform);
            wheelSections[i] = section.GetComponent<WheelSection>();
            wheelSections[i].Initialize(i, intervalAngle, angleMargin);
        }

        rectTransform.eulerAngles = new Vector3(0, 0, -wheelOffset);
        rectTransform.localScale = Vector3.one * radius;
    }

    public void UpdateCurrentSelection()
    {
        if (wheelSections == null || wheelSections.Length == 0) return;

        // Get the current mouse position direction relative to the center
        Vector2 mousePos = Input.mousePosition;
        Vector2 direction = (mousePos - (Vector2)rectTransform.position).normalized;

        float bestMatch = float.MinValue; // For comparing the best section
        WheelSection closestSection = null;

        for (int i = 0; i < wheelSections.Length; i++)
        {
            Vector2 sectionPosition = GetSectionPosition(i);    

            float angleDifference = Vector2.Angle(direction, sectionPosition.normalized);
            float similarity = -angleDifference;

            if (similarity > bestMatch)
            {
                bestMatch = similarity;
                closestSection = wheelSections[i];
            }
        }

        // Update only if the selection changes
        if (closestSection != selectedWheelSection)
        {
            if (selectedWheelSection != null) selectedWheelSection.OnHoverEnd(); // Deactivate previous
            selectedWheelSection = closestSection; // Update selected
            if (selectedWheelSection != null) selectedWheelSection.OnHoverStart(); // Highlight new
        }
    }

    public Vector2 GetSectionPosition(int index)
    {
        float intervalAngle = 360f / count;
        float halfInterval = intervalAngle / 2f;
        float sectionMiddleAngle = -(intervalAngle * index - halfInterval + angleMargin / 2f);

        Vector2 sectionDirection = new Vector2(
            Mathf.Cos(sectionMiddleAngle * Mathf.Deg2Rad),
            Mathf.Sin(sectionMiddleAngle * Mathf.Deg2Rad)
        );

        return sectionDirection * radius;
    }
}
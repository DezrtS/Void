using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionWheel : MonoBehaviour
{
    [Header("Selection Wheel")]
    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float radius;
    [SerializeField] private int count;
    [SerializeField] private float angleMargin;

    [Header("References")]
    [SerializeField] private GameObject selectionWheelHolder;
    [SerializeField] private WheelSectionData[] sections = new WheelSectionData[0];
    [SerializeField] private RectTransform infoHolder;
    [SerializeField] private TextMeshProUGUI titleHolder;
    [SerializeField] private TextMeshProUGUI textHolder;

    [Header("Options")]
    [SerializeField] private bool allowInnerSelection;
    [SerializeField] private Color activeTextColor;
    //[SerializeField] private Color defaultTextColor;
    [SerializeField] private Color inactiveTextColor;
    //[SerializeField] private Transform mouseTracker;
    //[SerializeField] private Transform bestOptionTracker;

    private WheelSection[] wheelSections;
    private WheelSection selectedWheelSection;
    private bool active;

    private void Awake()
    {
        GenerateSelectionWheel();
        ActivateSelectionWheel();
    }

    public void ActivateSelectionWheel()
    {
        active = true;
        selectionWheelHolder.SetActive(active);
    }

    public void DeactivateSelectionWheel()
    {
        if (selectedWheelSection)
        {
            selectedWheelSection.Activate();
            selectedWheelSection.OnHoverEnd();
        }
        active = false;
        selectionWheelHolder.SetActive(active);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (active)
            {
                DeactivateSelectionWheel();
            }
            else
            {
                ActivateSelectionWheel();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!active) return;

        UpdateCurrentSelection();
    }

    private void ResetSelectionWheel()
    {
        if (wheelSections != null)
        {
            for (int i = 0; i < wheelSections.Length; i++)
            {
                Destroy(wheelSections[i].gameObject);
            }
        }

        selectedWheelSection = null;
        UpdateInfo();
    }

    public void GenerateSelectionWheel()
    {
        ResetSelectionWheel();
        infoHolder.sizeDelta = radius * Vector2.one;

        wheelSections = new WheelSection[count];
        float intervalAngle = 360f / count;
        for (int i = 0; i < count; i++)
        {
            GameObject section = Instantiate(sectionPrefab, selectionWheelHolder.transform);
            GameObject icon = Instantiate(iconPrefab, selectionWheelHolder.transform);

            wheelSections[i] = section.GetComponent<WheelSection>();
            wheelSections[i].Initialize(i, intervalAngle, angleMargin, radius);
            if (i < sections.Length)
            {
                wheelSections[i].InitializeData(sections[i]);
                icon.GetComponent<Image>().sprite = sections[i].SectionSprite;
                icon.transform.position = wheelSections[i].Direction * (radius - radius * 0.13f) + (Vector2)selectionWheelHolder.transform.position;
                icon.transform.parent = wheelSections[i].transform;
            }
        }
    }

    public void UpdateCurrentSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        //mouseTracker.position = mousePos;

        if (!allowInnerSelection)
        {
            if (Vector2.Distance(mousePos, transform.position) < radius)
            {
                if (selectedWheelSection)
                {
                    selectedWheelSection.OnHoverEnd();
                    selectedWheelSection = null;
                    UpdateInfo();
                }

                return;
            }
        }

        float bestMatch = float.MinValue;
        WheelSection closestSection = null;

        for (int i = 0; i < wheelSections.Length; i++)
        {
            Vector2 sectionPosition = wheelSections[i].Direction * radius + (Vector2)transform.position;

            float distanceDifference = Vector2.Distance(mousePos, sectionPosition);
            float similarity = -distanceDifference;

            if (similarity > bestMatch)
            {
                bestMatch = similarity;
                closestSection = wheelSections[i];
                //bestOptionTracker.position = sectionPosition;
            }
        }

        if (closestSection != selectedWheelSection)
        {
            if (selectedWheelSection != null) selectedWheelSection.OnHoverEnd();
            selectedWheelSection = closestSection;
            if (selectedWheelSection != null) selectedWheelSection.OnHoverStart();

            UpdateInfo();
        }
    }

    public void UpdateInfo()
    {
        if (!selectedWheelSection || !selectedWheelSection.Data)
        {
            titleHolder.text = string.Empty;
            textHolder.text = string.Empty;
            return;
        }

        if (selectedWheelSection.Active)
        {
            titleHolder.color = activeTextColor;
            textHolder.color = activeTextColor;
        }
        else
        {
            titleHolder.color = inactiveTextColor;
            textHolder.color = inactiveTextColor;
        }

        titleHolder.text = selectedWheelSection.Data.GetFormattedTitle();
        textHolder.text = selectedWheelSection.Data.GetFormattedDescription();
    }
}
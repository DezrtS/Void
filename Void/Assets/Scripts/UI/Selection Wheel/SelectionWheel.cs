using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectionWheel : MonoBehaviour
{
    [Header("Selection Wheel")]
    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private float radius;
    [SerializeField] private int count;
    [SerializeField] private float angleMargin;
    [SerializeField] private float selectionOffset = 0;

    [Header("References")]
    [SerializeField] private GameObject selectionWheelHolder;
    [SerializeField] private WheelSectionData[] sections = new WheelSectionData[0];
    [SerializeField] private RectTransform infoHolder;
    [SerializeField] private TextMeshProUGUI titleHolder;
    [SerializeField] private TextMeshProUGUI primaryTextHolder;
    [SerializeField] private TextMeshProUGUI secondaryTextHolder;

    [Header("Options")]
    [SerializeField] private bool allowInnerSelection;
    [SerializeField] private Color activeTextColor;
    //[SerializeField] private Color defaultTextColor;
    [SerializeField] private Color inactiveTextColor;
    
    [SerializeField] private Transform mouseTracker;
    [SerializeField] private Transform bestOptionTracker;

    private WheelSection[] wheelSections;
    private WheelSection selectedWheelSection;
    private bool active;

    private PlayerController playerController;
    private InputActionMap selectionInputAction;
    private InputAction mousePositionInputAction;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;

        if (playerController)
        {
            playerController.OnSelectionWheel += OnSelectionWheel;
        }

        selectionInputAction ??= InputSystem.actions.FindActionMap("SelectionWheel");
        mousePositionInputAction = selectionInputAction.FindAction("Screen Position");
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;

        if (playerController)
        {
            playerController.OnSelectionWheel -= OnSelectionWheel;
        }
    }

    public void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out PlayerController playerController))
        {
            AttachSelectionWheel(playerController);
        }
    }

    public void AttachSelectionWheel(PlayerController playerController)
    {
        this.playerController = playerController;
        playerController.OnSelectionWheel += OnSelectionWheel;

        foreach (var wheel in wheelSections)
        {
            if (wheel) wheel.OnSetupUI(playerController.gameObject);
        }
    }

    public void OnSelectionWheel(bool enable)
    {
        if (enable && !active)
        {
            ActivateSelectionWheel();
        }
        else if (!enable && active)
        {
            DeactivateSelectionWheel();
        }
    }

    private void Awake()
    {
        GenerateSelectionWheel();
        selectionWheelHolder.SetActive(active);
    }

    [ContextMenu("Activate Selection Wheel")]
    public void ActivateSelectionWheel()
    {
        active = true;
        selectionWheelHolder.SetActive(active);

        selectionInputAction.Enable();
        playerController.PlayerLook.LockCamera(true);
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

        selectionInputAction.Disable();
        playerController.PlayerLook.LockCamera(false);
    }

    private void FixedUpdate()
    {
        foreach (WheelSection wheel in wheelSections)
        {
            wheel.UpdateTimers(Time.fixedDeltaTime);
        }

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

            wheelSections[i] = section.GetComponent<WheelSection>();
            wheelSections[i].Initialize(i, intervalAngle, angleMargin, radius);
            if (playerController) wheelSections[i].OnSetupUI(playerController.gameObject);

            if (i < sections.Length)
            {
                wheelSections[i].InitializeData(sections[i]);
            }
        }
    }

    public void UpdateCurrentSelection()
    {
        Vector2 mousePos = mousePositionInputAction.ReadValue<Vector2>() - new Vector2(Screen.width, Screen.height) * 0.5f;
        //mouseTracker.localPosition = mousePos;

        if (!allowInnerSelection)
        {
            if (mousePos.magnitude < radius - selectionOffset)
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
            Vector2 sectionPosition = wheelSections[i].Direction * radius;

            float distanceDifference = Vector2.Distance(mousePos, sectionPosition);
            float similarity = -distanceDifference;

            if (similarity > bestMatch)
            {
                bestMatch = similarity;
                closestSection = wheelSections[i];
                //bestOptionTracker.position = sectionPosition + (Vector2)transform.position;
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
            primaryTextHolder.text = string.Empty;
            secondaryTextHolder.text = string.Empty;
            return;
        }

        if (selectedWheelSection.Active)
        {
            titleHolder.color = activeTextColor;
            primaryTextHolder.color = activeTextColor;
            secondaryTextHolder.color = activeTextColor;
        }
        else
        {
            titleHolder.color = inactiveTextColor;
            primaryTextHolder.color = inactiveTextColor;
            secondaryTextHolder.color = inactiveTextColor;
        }

        titleHolder.text = selectedWheelSection.Data.GetTitle();
        primaryTextHolder.text = selectedWheelSection.Data.GetPrimaryText();
        secondaryTextHolder.text = selectedWheelSection.Data.GetSecondaryText();
    }
}
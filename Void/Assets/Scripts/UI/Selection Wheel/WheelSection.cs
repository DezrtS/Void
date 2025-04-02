using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class WheelSection : MonoBehaviour
{
    [SerializeField] private float transitionDuration;
    [SerializeField] private RectTransform rotationPivot;
    [SerializeField] private RectTransform scalePivot;
    [SerializeField] private List<Image> selectionParts;
    [SerializeField] private RectTransform iconPivot;
    [SerializeField] private List<Image> iconImages;

    protected Animator animator;
    protected bool active;

    protected WheelSectionData data;

    private Vector2 direction;
    private float activeValue;

    public bool Active => active;
    public WheelSectionData Data => data;
    public Vector2 Direction => direction;

    public abstract void OnSetupUI(GameObject player);

    public void Initialize(int index, float intervalAngle, float angleMargin, float radius)
    {
        animator = GetComponent<Animator>();
        scalePivot.localScale = new Vector3(radius, radius, 1);
        float angle = intervalAngle * index + intervalAngle / 2f - angleMargin / 2f;
        rotationPivot.eulerAngles = new Vector3(0, 0, angle);

        float sectionAngle = intervalAngle * index;
        direction = new Vector2(
            -Mathf.Cos(sectionAngle * Mathf.Deg2Rad),
            -Mathf.Sin(sectionAngle * Mathf.Deg2Rad)
        );
        Debug.Log($"{index}: Angle: {angle} SectionAngle: {sectionAngle}, Direction: {direction}");

        foreach (Image image in selectionParts)
        {
            image.fillAmount = (1f / 360f) * (intervalAngle - angleMargin);
        }
        iconPivot.localPosition = Quaternion.Euler(0, 0, sectionAngle) * iconPivot.localPosition;
    }

    public virtual void InitializeData(WheelSectionData data)
    {
        this.data = data;
        foreach (Image iconImage in iconImages)
        {
            iconImage.sprite = data.SectionSprite;
        }
    }

    public virtual void UpdateTimers(float deltaTime)
    {
        float activeTime = (active) ? deltaTime : -deltaTime;
        activeValue = Mathf.Clamp(activeValue += activeTime / transitionDuration, 0, 1);
        animator.SetFloat("Active", activeValue);
    }

    public void OnHoverStart()
    {
        if (active || !CanActivate()) return;
        active = true;
    }

    public void OnHoverEnd()
    {
        if (!active) return;
        active = false;
    }

    public virtual bool CanActivate()
    {
        return data != null;
    }

    public abstract void Activate();
}
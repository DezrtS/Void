using UnityEngine;
using UnityEngine.UI;

public abstract class WheelSection : MonoBehaviour
{
    [SerializeField] private float transitionDuration;

    protected Animator animator;
    protected bool active;

    private float activeValue;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(int index, float intervalAngle, float angleMargin)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.eulerAngles = new Vector3(0, 0, -(intervalAngle * index + angleMargin / 2f));
        Image image = GetComponent<Image>();
        image.fillAmount = (1f / 360f) * (intervalAngle - angleMargin);
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = (active) ? Time.fixedDeltaTime : -Time.fixedDeltaTime;
        activeValue = Mathf.Clamp(activeValue += fixedDeltaTime / transitionDuration, 0, 1);
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

    public abstract bool CanActivate();
    public abstract void Activate();
}
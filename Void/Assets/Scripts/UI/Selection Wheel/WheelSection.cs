using UnityEngine;
using UnityEngine.UI;

public abstract class WheelSection : MonoBehaviour
{
    [SerializeField] private float transitionDuration;

    protected Animator animator;
    protected bool active;

    protected WheelSectionData data;

    private Vector2 direction;
    private float activeValue;

    public bool Active => active;
    public WheelSectionData Data => data;
    public Vector2 Direction => direction;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;
    }

    public abstract void OnSetupUI(GameObject player);

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(int index, float intervalAngle, float angleMargin, float radius)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = 2 * radius * Vector2.one;
        rectTransform.eulerAngles = new Vector3(0, 0, intervalAngle * index + angleMargin / 2f + intervalAngle / 2f);

        float sectionAngle = intervalAngle * index;
        direction = new Vector2(
            Mathf.Cos(sectionAngle * Mathf.Deg2Rad),
            Mathf.Sin(sectionAngle * Mathf.Deg2Rad)
        );

        Image image = GetComponent<Image>();
        image.fillAmount = (1f / 360f) * (intervalAngle - angleMargin);
    }

    public virtual void InitializeData(WheelSectionData data)
    {
        this.data = data;
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

    public virtual bool CanActivate()
    {
        return data != null;
    }

    public abstract void Activate();
}
using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : PlayerController
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    private InverseKinematicsObject inverseKinematicsObject;

    private Hotbar hotbar;
    private Inventory inventory;
    private Draggable draggable;

    private InputActionMap survivorActionMap;
    private AnimationController animationController;

    public Hotbar Hotbar => hotbar;
    public Inventory Inventory => inventory;

    public override void AssignControls()
    {
        survivorActionMap ??= InputSystem.actions.FindActionMap("Survivor");

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed += OnReload;
        base.AssignControls();
    }

    public override void EnableControls()
    {
        base.EnableControls();
        survivorActionMap.Enable();
    }

    public override void UnassignControls()
    {
        if (!IsOwner) return;

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed -= OnReload;
        base.UnassignControls();
    }

    public override void DisableControls()
    {
        base.DisableControls();
        survivorActionMap.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        draggable.CanDrag = false;

        if (IsOwner) UIManager.Instance.SetupUI(GameManager.PlayerRole.Survivor, gameObject);

        if (!IsServer) return;
        Item item = GameDataManager.SpawnItem(GameDataManager.Instance.GetItemData(0));
        hotbar.RequestPickUpItem(item);
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.RegenerateTaskInstructions();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        hotbar = GetComponent<Hotbar>();
        hotbar.OnSwitchItem += OnSwitchItem;
        hotbar.OnPickUpItem += OnPickUpItem;
        hotbar.OnDropItem += OnDropItem;
        inventory = GetComponent<Inventory>();
        draggable = GetComponent<Draggable>();
        animationController = GetComponent<AnimationController>();
    }

    private void Update()
    {
        if (inverseKinematicsObject != null)
        {
            leftHandTarget.position = inverseKinematicsObject.LeftHandTarget.position;
            rightHandTarget.position = inverseKinematicsObject.RightHandTarget.position;
        }
    }

    private void OnFireGun()
    {
        CameraManager.TriggerScreenShake(0.25f, 0.1f, 1f);
        playerLook.AddXRotation(-1.5f);
        playerLook.AddRandomYRotation();
    }

    private void OnPickUpItem(int index, Item item)
    {
        //Debug.Log($"PICKED UP: {item.ItemData.Name}");
        if (item is IAnimate)
        {
            IAnimate animate = item as IAnimate;
            animate.OnAnimationEvent += animationController.HandleAnimationEvent;
        }

        if (item is Gun)
        {
            Gun gun = item as Gun;
            gun.OnFire += OnFireGun;
            animationController.SetBool("IK", true);
            animationController.SetBool("hasGun", true);
        }

        if (item.TryGetComponent(out InverseKinematicsObject inverseKinematicsObject))
        {
            this.inverseKinematicsObject = inverseKinematicsObject;
            leftHandTarget.position = inverseKinematicsObject.LeftHandTarget.position;
            rightHandTarget.position = inverseKinematicsObject.RightHandTarget.position;
        }
    }

    private void OnDropItem(int index, Item item)
    {
        //Debug.Log($"DROPPED UP: {item.ItemData.Name}");
        if (item is IAnimate)
        {
            IAnimate animate = item as IAnimate;
            animate.OnAnimationEvent -= animationController.HandleAnimationEvent;
        }

        if (item is Gun)
        {
            Gun gun = item as Gun;
            gun.OnFire -= OnFireGun;
            animationController.SetBool("IK", false);
            animationController.SetBool("hasGun", false);
        }
    }

    private void OnSwitchItem(int fromIndex, int toIndex, Item fromItem, Item toItem)
    {
        if (toItem != null)
        {
            if (toItem is Gun)
            {
                animationController.SetBool("IK", true);
                animationController.SetBool("hasGun", true);
            }
            else
            {
                animationController.SetBool("IK", false);
                animationController.SetBool("hasGun", false);
            }

            if (toItem.TryGetComponent(out InverseKinematicsObject inverseKinematicsObject))
            {
                this.inverseKinematicsObject = inverseKinematicsObject;
                leftHandTarget.position = inverseKinematicsObject.LeftHandTarget.position;
                rightHandTarget.position = inverseKinematicsObject.RightHandTarget.position;
            }
        }
        else
        {
            animationController.SetBool("IK", false);
            animationController.SetBool("hasGun", false);
        }
    }

    public override void OnDeathStateChanged(Health health, bool isDead)
    {
        base.OnDeathStateChanged(health, isDead);
        draggable.CanDrag = isDead;
        if (!IsOwner) return;

        if (isDead)
        {
            animationController.SetTrigger("Die");
            hotbar.RequestDropEverything();
        }
        else
        {
            animationController.SetTrigger("Respawn");
            draggable.RequestStopUsing();
        }
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.RequestUse();
        }
        else if (context.canceled)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.RequestStopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;
        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        int newIndex = (hotbar.SelectedIndex + direction + hotbar.HotbarCapacity) % hotbar.HotbarCapacity;
        hotbar.RequestSwitchToItem(newIndex);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (hotbar.IsDragging)
        {
            hotbar.RequestStopDragging();
        }
        else
        {
            hotbar.RequestDropItem();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null)
            {
                if (activeItem.TryGetComponent(out IReload reload))
                {
                    reload.RequestReload();
                }
            }
        }
    }
}

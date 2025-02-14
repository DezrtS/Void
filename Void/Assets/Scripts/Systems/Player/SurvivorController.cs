using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : PlayerController
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    private InverseKinematicsObject inverseKinematicsObject;

    private Hotbar hotbar;
    private Inventory inventory;

    private InputActionMap survivorActionMap;

    public Hotbar Hotbar => hotbar;
    public Inventory Inventory => inventory;

    protected override void OnEnable()
    {
        base.OnEnable();

        survivorActionMap ??= InputSystem.actions.FindActionMap("Survivor");
        survivorActionMap.Enable();

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed += OnReload;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed -= OnReload;

        survivorActionMap.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
        Item item = ItemManager.SpawnItem(GameDataManager.Instance.GetItemData(0));
        hotbar.PickUpItem(item);
    }

    protected override void Awake()
    {
        base.Awake();
        hotbar = GetComponent<Hotbar>();
        hotbar.OnPickUpItem += OnPickUpItem;
        inventory = GetComponent<Inventory>();
    }

    private void OnPickUpItem(int index, Item item)
    {
        if (item.TryGetComponent(out InverseKinematicsObject inverseKinematicsObject))
        {
            this.inverseKinematicsObject = inverseKinematicsObject;
            leftHandTarget.position = inverseKinematicsObject.LeftHandTarget.position;
            rightHandTarget.position = inverseKinematicsObject.RightHandTarget.position;
        }
    }

    private void Update()
    {
        if (inverseKinematicsObject != null)
        {
            leftHandTarget.position = inverseKinematicsObject.LeftHandTarget.position;
            rightHandTarget.position = inverseKinematicsObject.RightHandTarget.position;
        }
    }

    public override void Die()
    {
        if (!IsServer) return;
        hotbar.DropAllItems();

        Respawn();
    }

    public override void Respawn()
    {
        health.SetHealth(health.GetMaxHealth());
        movementController.Teleport(SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Survivor));
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.Use();
        }
        else if (context.canceled)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.StopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        hotbar.SwitchItem(direction);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (hotbar.IsDragging)
        {
            hotbar.StopDragging();
        }
        else
        {
            hotbar.DropItem();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null)
            {
                if (activeItem.TryGetComponent(out IReload reload))
                {
                    reload.Reload();
                }
                
            }
        }
    }
}

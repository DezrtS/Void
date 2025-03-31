using FMODUnity;
using System.Collections;
using UnityEngine;

public class ItemDropOff : MonoBehaviour, INetworkUseable, IInteractable
{
    public event IUseable.UseHandler OnUsed;

    public delegate void ItemDropOffHandler(Item item, ItemDropOff itemDropOff, bool isAdded);
    public static event ItemDropOffHandler OnDropOff;

    [SerializeField] private float processingTime;
    [SerializeField] private float ejectionPower;

    [SerializeField] private GameObject outline;

    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference acceptSound;
    [SerializeField] private EventReference rejectSound;

    [SerializeField] private InteractableData dropOffInteractableData;
    [SerializeField] private InteractableData processingItemInteractableData;

    private NetworkItemDropOff networkItemDropOff;
    private bool isUsing;

    private Item item;
    private float processingTimer;

    public bool IsUsing => isUsing;

    public bool CanUse() => !isUsing;
    public bool CanStopUsing() => isUsing;
    public bool HasItem() => item != null;

    public void RequestUse() => networkItemDropOff.UseServerRpc();
    public void RequestStopUsing() => networkItemDropOff.StopUsingServerRpc();
    public void RequestProcessItem(Item item) => networkItemDropOff.ProcessItemServerRpc(item.NetworkItem.NetworkObjectId);
    public void RequestAcceptItem() => networkItemDropOff.AcceptItemServerRpc();
    public void RequestEjectItem() => networkItemDropOff.EjectItemServerRpc();

    private void Awake()
    {
        networkItemDropOff = GetComponent<NetworkItemDropOff>();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
        TaskManager.OnAllTasksCompleted += OnAllTasksCompleted;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GamePlaying)
        {
            outline.SetActive(true);
        }
        else if (state == GameManager.GameState.GameOver)
        {
            outline.SetActive(false);
        }
    }

    private void OnAllTasksCompleted()
    {
        outline.SetActive(false);
    }

    public void Use()
    {
        isUsing = true;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(closeSound, transform.position);
        processingTimer = processingTime;
    }

    public void StopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(openSound, transform.position);
        processingTimer = 0;
        item = null;
    }

    public void ProcessItem(Item item)
    {
        this.item = item;
        if (item.NetworkItem.IsOwner) item.transform.position = transform.position;
        StartCoroutine(ProcessItemCoroutine());
    }

    public void AcceptItem()
    {
        Debug.Log("Item Accepted");
        AudioManager.PlayOneShot(acceptSound, transform.position);
        StopAllCoroutines();
        if (networkItemDropOff.IsServer) RequestStopUsing();
    }

    public void EjectItem()
    {
        OnDropOff?.Invoke(item, this, false);
        AudioManager.PlayOneShot(rejectSound, transform.position);
        StopAllCoroutines();
        if (item.NetworkItem.IsOwner)
        {
            item.RequestDrop();
            item.Drop();
            Rigidbody rig = item.GetComponent<Rigidbody>();
            rig.AddForce(transform.rotation * new Vector3(0, 1, 1) * ejectionPower, ForceMode.Impulse);
            RequestStopUsing();
        }
    }

    public InteractableData GetInteractableData()
    {
        if (isUsing) return processingItemInteractableData;
        else return dropOffInteractableData;
    }

    public void Interact(GameObject interactor)
    {
        if (HasItem()) return;

        if (interactor.TryGetComponent(out SurvivorController survivorController))
        {
            Item item = survivorController.Hotbar.GetItem();
            if (item != null) RequestProcessItem(item);
        }
    }

    private IEnumerator ProcessItemCoroutine()
    {
        yield return new WaitForSeconds(processingTime / 2f);
        OnDropOff?.Invoke(item, this, true);
        yield return new WaitForSeconds(processingTime / 2f);
        if (networkItemDropOff.IsServer) RequestEjectItem();
    }
}

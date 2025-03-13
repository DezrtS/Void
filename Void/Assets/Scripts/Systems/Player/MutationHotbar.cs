using System.Collections.Generic;
using UnityEngine;

public class MutationHotbar : MonoBehaviour
{
    public delegate void MutationEventHandler(Mutation mutation, int index);
    public delegate void MutationSwitchEventHandler(int fromIndex, int toIndex);
    public event MutationEventHandler OnAddMutation;
    public event MutationEventHandler OnRemoveMutation;
    public event MutationSwitchEventHandler OnSwitchMutation;

    [SerializeField] private Transform activeTransform;

    private NetworkMutationHotbar networkMutationHotbar;
    private int selectedIndex;

    private List<Mutation> mutations;

    public Transform ActiveTransform => activeTransform;
    public NetworkMutationHotbar NetworkMutationHotbar => networkMutationHotbar;
    public int SelectedIndex => selectedIndex;
    public int MutationCount => mutations.Count;

    public void RequestSwitchToMutation(int index) => networkMutationHotbar.SwitchToMutationServerRpc(index);
    public void RequestAddMutation(ulong mutationNetworkObjectId) => networkMutationHotbar.AddMutationServerRpc(mutationNetworkObjectId);
    public void RequestRemoveMutation(int index) => networkMutationHotbar.RemoveMutationServerRpc(index);

    private void Awake()
    {
        networkMutationHotbar = GetComponent<NetworkMutationHotbar>();
        mutations = new List<Mutation>();
    }

    public Mutation GetMutation()
    {
        return GetMutation(selectedIndex);
    }

    public Mutation GetMutation(int index)
    {
        if (mutations.Count <= index) return null;
        return mutations[index];
    }

    public void SwitchToMutation(int index)
    {
        OnSwitchMutation?.Invoke(selectedIndex, index);
        selectedIndex = index;
        Debug.Log($"Index: {selectedIndex}, {GetMutation().MutationData.DisplayName}");

        if (networkMutationHotbar.IsOwner)
        {
            Mutation mutation = mutations[index];
            if (mutation != null)
            {
                UIManager.Instance.SetTutorialText(mutation.TutorialData);
            }
            else
            {
                UIManager.Instance.ResetTutorialText();
            }
        }
    }

    public void RequestAddMutation(MutationData mutationData)
    {
        Mutation newMutation = GameDataManager.SpawnMutation(mutationData);
        RequestAddMutation(newMutation);
    }

    public void RequestAddMutation(Mutation mutation) => RequestAddMutation(mutation.NetworkUseable.NetworkObjectId);

    public void AddMutation(Mutation mutation)
    {
        mutations.Add(mutation);
        OnAddMutation?.Invoke(mutation, mutations.Count - 1);
        mutation.SetupMutation(gameObject);

        if (networkMutationHotbar.IsOwner) mutation.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void RequestRemoveMutation(Mutation mutation)
    {
        if (mutations.Contains(mutation))
        {
            RequestRemoveMutation(mutations.IndexOf(mutation));
        }
    }

    public void RemoveMutation(int index)
    {
        Mutation mutation = mutations[index];
        mutations.RemoveAt(index);
        OnRemoveMutation?.Invoke(mutation, index);
    }
}
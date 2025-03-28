using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MutationSelectionManager : Singleton<MutationSelectionManager>
{
    public delegate void MutationSelectionHandler(int index, bool isSelected);
    public static event MutationSelectionHandler OnMutationDataSelected;

    [SerializeField] private int optionCount;
    [SerializeField] private int requiredSelectionCount;
    [SerializeField] private GameObject mutationSelectionHolder;
    [SerializeField] private List<MutationOption> mutationOptions;
    [SerializeField] private List<MutationData> avoidMutations = new List<MutationData>();

    private VoidMonsterController voidMonsterController;
    private MutationData[] mutationDatas;
    private List<MutationData> selectedMutationDatas = new List<MutationData>();

    public MutationData[] MuationDatas => mutationDatas;
    public List<MutationData> SelectedMutationData => selectedMutationDatas;

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnGameStateChanged += OnGameStateChanged;
        UIManager.OnSetupUI += OnSetupUI;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        UIManager.OnSetupUI -= OnSetupUI;
    }

    private void Awake()
    {
        mutationDatas = new MutationData[optionCount];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            RandomizeMutationOptions(avoidMutations);
        }
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (voidMonsterController == null) return;

        switch (gameState)
        {
            case GameManager.GameState.WaitingToStart:
                RandomizeMutationOptions(new List<MutationData>());
                ActivateMutationSelection();
                break;
            case GameManager.GameState.ReadyToStart:
                LockInMutationOptions();
                break;
            case GameManager.GameState.GamePlaying:
                ApplySelectedMutations();
                DeactivateMutationSelection();
                break;
            default:
                break;
        }
    }

    private void OnSetupUI(GameObject player)
    {
        voidMonsterController = player.GetComponent<VoidMonsterController>();
        OnGameStateChanged(GameManager.GameState.WaitingToStart);
    }

    private bool CanSelectMutationOption()
    {
        return selectedMutationDatas.Count < requiredSelectionCount;
    }

    public void SelectMutationOption(int index)
    {
        if (CanSelectMutationOption() && !selectedMutationDatas.Contains(mutationDatas[index]))
        {
            OnMutationDataSelected?.Invoke(index, true);
            selectedMutationDatas.Add(mutationDatas[index]);

            if (selectedMutationDatas.Count == requiredSelectionCount) PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, true);
        }
    }

    public void UnselectMutationOption(int index)
    {
        if (!selectedMutationDatas.Contains(mutationDatas[index])) return;

        if (selectedMutationDatas.Count == requiredSelectionCount) PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, false);

        OnMutationDataSelected?.Invoke(index, false);
        selectedMutationDatas.Remove(mutationDatas[index]);
    }

    public void LockInMutationOptions()
    {
        int difference = requiredSelectionCount - selectedMutationDatas.Count;
        if (difference > 0)
        {
            // Create a list of unselected mutations
            List<int> unselectedIndices = new List<int>();
            for (int i = 0; i < mutationDatas.Length; i++)
            {
                if (!selectedMutationDatas.Contains(mutationDatas[i]))
                {
                    unselectedIndices.Add(i);
                }
            }

            // Randomly select enough mutations to make up the difference
            for (int i = 0; i < difference; i++)
            {
                if (unselectedIndices.Count == 0) break; // No more mutations to select

                int randomIndex = UnityEngine.Random.Range(0, unselectedIndices.Count);
                int chosenIndex = unselectedIndices[randomIndex];
                SelectMutationOption(chosenIndex);
                unselectedIndices.RemoveAt(randomIndex);
            }
        }
    }

    private void ApplySelectedMutations()
    {
        foreach (MutationData mutationData in selectedMutationDatas)
        {
            voidMonsterController.MutationHotbar.RequestAddMutation(mutationData);
        }
    }

    public void ActivateMutationSelection()
    {
        mutationSelectionHolder.SetActive(true);

        voidMonsterController.PlayerLook.LockCamera(true);
        //voidMonsterController.PlayerLook.EnableDisableCameraControls(false);
    }

    public void DeactivateMutationSelection()
    {
        mutationSelectionHolder.SetActive(false);

        voidMonsterController.PlayerLook.LockCamera(false);
        //voidMonsterController.PlayerLook.EnableDisableCameraControls(true);
    }

    public void RandomizeMutationOptions(List<MutationData> avoidMutations)
    {
        Debug.Log("Randomizing Mutations");
        mutationDatas = new MutationData[optionCount];
        selectedMutationDatas.Clear();

        List<MutationData> mutations = GameDataManager.Instance.Mutations;
        List<int> indexOptions = new List<int>();

        for (int i = 0; i < mutations.Count; i++)
        {
            if (avoidMutations.Contains(mutations[i])) continue;
            indexOptions.Add(i);
        }

        for (int i = 0; i < optionCount; i++)
        {
            if (indexOptions.Count <= 0) break;

            int randomIndex = UnityEngine.Random.Range(0, indexOptions.Count);
            int chosenIndex = indexOptions[randomIndex];
            indexOptions.RemoveAt(randomIndex);

            mutationDatas[i] = (mutations[chosenIndex]);
        }

        foreach (MutationOption mutationOption in mutationOptions)
        {
            mutationOption.UpdateMutationData();
        }
    }
}
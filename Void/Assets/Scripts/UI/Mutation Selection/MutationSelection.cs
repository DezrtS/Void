using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationSelection : MonoBehaviour
{
    [SerializeField] private GameObject mutationSelectionHolder;
    [SerializeField] private MutationOption[] mutationOptions;

    private VoidMonsterController voidMonsterController;
    private bool active = false;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;
    }

    public void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out VoidMonsterController voidMonsterController))
        {
            AttachMutationSelection(voidMonsterController);
        }
    }

    public void AttachMutationSelection(VoidMonsterController voidMonsterController)
    {
        this.voidMonsterController = voidMonsterController;
    }

    private void Start()
    {
        RandomizeMutationOptions(new MutationData[0]);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (voidMonsterController == null) return;
            EnableDisableMutationSelection(!active);
        }
    }

    public void EnableDisableMutationSelection(bool enabled)
    {
        active = enabled;
        if (enabled)
        {
            ActivateMutationSelection();
        }
        else if (!enabled)
        {
            DeactivateMutationSelection();
        }
    }

    public void ActivateMutationSelection()
    {
        mutationSelectionHolder.SetActive(true);

        voidMonsterController.PlayerLook.LockCamera(false);
        voidMonsterController.PlayerLook.EnableDisableCameraControls(false);
    }

    public void DeactivateMutationSelection()
    {
        mutationSelectionHolder.SetActive(false);

        voidMonsterController.PlayerLook.LockCamera(true);
        voidMonsterController.PlayerLook.EnableDisableCameraControls(true);
    }

    public void RandomizeMutationOptions(MutationData[] avoidMutations)
    {
        List<MutationData> mutations = GameDataManager.Instance.Mutations;
        List<int> indexOptions = new List<int>();

        for (int i = 0; i < mutations.Count; i++)
        {
            if (avoidMutations.Contains(mutations[i])) continue;
            indexOptions.Add(i);
        }

        if (indexOptions.Count < mutationOptions.Length) return;

        for (int i = 0; i < mutationOptions.Length; i++)
        {
            int randomIndex = Random.Range(0, indexOptions.Count);
            int chosenIndex = indexOptions[randomIndex];
            indexOptions.RemoveAt(randomIndex);

            mutationOptions[i].SetMutationData(mutations[chosenIndex]);
        }
    }

    public void SelectMutation(int index)
    {
        if (voidMonsterController) voidMonsterController.MutationHotbar.AddMutation(mutationOptions[index].MutationData);
    }
}
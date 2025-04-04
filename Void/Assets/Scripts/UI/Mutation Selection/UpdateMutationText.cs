using UnityEngine;

public class UpdateMutationText : MonoBehaviour
{
    [SerializeField] private MutationOption mutationOption;
    
    public void UpdateText()
    {
        MutationSelectionManager.Instance.SetMutationText(mutationOption.MutationData);
    }

    public void ResetText()
    {
        MutationSelectionManager.Instance.ResetMutationText();
    }
}
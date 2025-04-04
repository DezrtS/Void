using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationOption : MonoBehaviour
{
    [SerializeField] private int optionIndex;
    [SerializeField] private Image mutationOptionImage;
    [SerializeField] private Animator animator;

    private MutationData mutationData;
    private bool isSelected;

    public MutationData MutationData => mutationData;
    public bool IsSelected => isSelected;

    private void OnEnable()
    {
        MutationSelectionManager.OnMutationDataSelected += OnMutationDataSelected;
    }

    private void OnDisable()
    {
        MutationSelectionManager.OnMutationDataSelected -= OnMutationDataSelected;
    }

    public void UpdateMutationData()
    {
        mutationData = MutationSelectionManager.Instance.MuationDatas[optionIndex];

        mutationOptionImage.sprite = mutationData.DisplaySprite;
    }

    private void OnMutationDataSelected(int index, bool isSelected)
    {
        if (index == optionIndex)
        {
            this.isSelected = isSelected;
            if (isSelected)
            {
                AudioManager.PlayOneShot(FMODEventManager.Instance.SelectMutationSound);
            }
        }
    }

    public void Interact()
    {
        if (!mutationData) return;
        
        if (isSelected)
        {
            //MutationSelectionManager.Instance.UnselectMutationOption(optionIndex);
            //animator.SetTrigger("Unselect");
        }
        else
        {
            MutationSelectionManager.Instance.SelectMutationOption(optionIndex);
            animator.SetTrigger("Select");
        }
    }
}
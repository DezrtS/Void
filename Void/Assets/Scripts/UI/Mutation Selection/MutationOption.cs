using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationOption : MonoBehaviour
{
    [SerializeField] private int optionIndex;
    [SerializeField] private Image mutationOptionImage;
    [SerializeField] private TextMeshProUGUI mutationOptionText;

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
        mutationOptionText.text = mutationData.Description;
    }

    private void OnMutationDataSelected(int index, bool isSelected)
    {
        if (index == optionIndex)
        {
            this.isSelected = isSelected;
        }
    }

    public void Interact()
    {
        if (!mutationData) return;
        
        if (isSelected) MutationSelectionManager.Instance.UnselectMutationOption(optionIndex);
        else MutationSelectionManager.Instance.SelectMutationOption(optionIndex);
    }
}
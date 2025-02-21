using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationOption : MonoBehaviour
{
    public delegate void MutationOptionEventHandler(MutationOption mutationOption);
    public event MutationOptionEventHandler OnMutationOptionSelected;

    [SerializeField] private Image mutationOptionImage;
    [SerializeField] private TextMeshProUGUI mutationOptionText;

    private MutationData mutationData;

    public MutationData MutationData => mutationData;

    public void SetMutationData(MutationData mutationData)
    {
        this.mutationData = mutationData;

        mutationOptionImage.sprite = mutationData.DisplaySprite;
        mutationOptionText.text = mutationData.Description;
    }

    public void SelectMutationOption()
    {
        if (!mutationData) return;
        OnMutationOptionSelected?.Invoke(this);
    }
}

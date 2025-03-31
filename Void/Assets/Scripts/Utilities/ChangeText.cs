using TMPro;
using UnityEngine;

public class ChangeText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private bool canDeselect;
    [SerializeField] private string selectedText;
    [SerializeField] private string deselectedText;

    private bool selected;

    public void Select()
    {
        if (selected && !canDeselect) return;
        selected = !selected;
        text.text = selected ? selectedText : deselectedText;
    }
}

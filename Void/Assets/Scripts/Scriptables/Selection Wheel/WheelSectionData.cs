using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WheelSectionData", menuName = "ScriptableObjects/SelectionWheel/WheelSectionData")]
public class WheelSectionData : ScriptableObject
{
    [SerializeField] private Sprite sectionSprite;
    [SerializeField] private string sectionTitle;
    [TextArea(2, 5)]
    [SerializeField] private string sectionText;

    public Sprite SectionSprite => sectionSprite;
    public string SectionTitle => sectionTitle;
    public string SectionText => sectionText;

    public virtual string GetFormattedTitle()
    {
        return sectionTitle;
    }

    public virtual string GetFormattedDescription()
    {
        return sectionText;
    }
}
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ButtonData", menuName = "ScriptableObjects/ButtonData", order = 1)]
public class ButtonData : ScriptableObject
{
    public string buttonName;  // The name of the button in the scene
    public string nextButtonName;
    public string previousButtonName;
    [HideInInspector] public Button button;
    [HideInInspector] public Button nextButton;
    [HideInInspector] public Button previousButton;
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FacilityGenerationManager))]
public class FacilityGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FacilityGenerationManager facilityGenerationManager = (FacilityGenerationManager)target;
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("GENERATE NEW FACILITY FLOOR"))
        {
            FacilityGenerationManager.Instance.GenerateFacilityFloor();
        }
        EditorGUI.EndDisabledGroup();
    }
}

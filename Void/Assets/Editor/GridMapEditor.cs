using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridMapManager))]
public class GridMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridMapManager gridMapManager = (GridMapManager)target;
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("GENERATE NEW GRID MAP"))
        {
            GridMapManager gridMapManagerInstance = GridMapManager.Instance;

            gridMapManagerInstance.InitializeGridMap();
            gridMapManagerInstance.GenerateRooms();
            gridMapManagerInstance.GenerateHallways();
            gridMapManagerInstance.SpawnTiles();
            gridMapManagerInstance.InitializeInteriorGridMap();
            gridMapManagerInstance.GenerateTasks();
            gridMapManagerInstance.GenerateInteriors();
        }
        EditorGUI.EndDisabledGroup();
    }
}

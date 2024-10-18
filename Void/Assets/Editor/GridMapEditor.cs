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
            GridMapManager.Instance.GenerateGridMap();
            GridMapManager.Instance.GenerateRooms();
            GridMapManager.Instance.GenerateRoomConnections();
            GridMapManager.Instance.SpawnTiles();
        }
        EditorGUI.EndDisabledGroup();
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    private RoomData roomData;
    private float buttonSize = 50f;

    private void OnEnable()
    {
        roomData = (RoomData)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (roomData.gridSize.x <= 0 || roomData.gridSize.y <= 0)
        {
            EditorGUILayout.HelpBox("Grid size must be greater than zero.", MessageType.Warning);
            return;
        }

        float inspectorWidth = EditorGUIUtility.currentViewWidth - roomData.gridSize.x * 5f;
        buttonSize = Mathf.Min(inspectorWidth / roomData.gridSize.x, 50f);

        for (int y = 0; y < roomData.gridSize.y; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < roomData.gridSize.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                bool contains = roomData.tilePositions.Contains(position);
                GUI.backgroundColor = GetColor(contains);
                if (GUILayout.Button(contains.ToString(), GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
                {
                    if (contains)
                    {
                        roomData.tilePositions.Remove(position);
                    }
                    else
                    {
                        roomData.tilePositions.Add(position);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(roomData);
        }
    }

    public Color GetColor(bool contains)
    {
        if (contains)
        {
            return Color.green;
        }
        else
        {
            return Color.white;
        }
    }
}

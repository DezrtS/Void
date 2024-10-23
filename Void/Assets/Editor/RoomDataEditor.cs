using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    private RoomData roomData;
    private CommandInvoker commandInvoker;
    private float gridButtonSize = 50f;

    private void OnEnable()
    {
        roomData = (RoomData)target;
        commandInvoker = new CommandInvoker();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (roomData.gridSize.x <= 0 || roomData.gridSize.y <= 0)
        {
            EditorGUILayout.HelpBox("Grid size must be greater than zero.", MessageType.Warning);
            return;
        }

        float inspectorWidth = EditorGUIUtility.currentViewWidth;
        gridButtonSize = Mathf.Min((inspectorWidth - roomData.gridSize.x * 5f) / roomData.gridSize.x, 50f);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Update Grid Size", GUILayout.Width(inspectorWidth - 26), GUILayout.Height(50f)))
        {
            ICommand command = new ChangeGridSize(roomData, roomData.GridSize);
            commandInvoker.ExecuteCommand(command);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        for (int y = 0; y < roomData.gridSize.y; y++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int x = 0; x < roomData.gridSize.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                bool contains = roomData.tilePositions.Contains(position);
                GUI.backgroundColor = GetColor(contains);
                if (GUILayout.Button($"{x},{y}", GUILayout.Width(gridButtonSize), GUILayout.Height(gridButtonSize)))
                {
                    if (contains)
                    {
                        ICommand command = new DeselectGridPosition(roomData, position);
                        commandInvoker.ExecuteCommand(command);
                    }
                    else
                    {
                        ICommand command = new SelectGridPosition(roomData, position);
                        commandInvoker.ExecuteCommand(command);
                    }
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(roomData);
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Undo", GUILayout.Width(inspectorWidth / 2 - 15), GUILayout.Height(50f)))
        {
            commandInvoker.Undo();
        }
        if (GUILayout.Button("Redo", GUILayout.Width(inspectorWidth / 2 - 15), GUILayout.Height(50f)))
        {
            commandInvoker.Redo();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset", GUILayout.Width(inspectorWidth - 26), GUILayout.Height(50f)))
        {
            ICommand command = new ResetGridPositions(roomData);
            commandInvoker.ExecuteCommand(command);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox($"Command History: {commandInvoker.CommandHistory.Count}, Redo History: {commandInvoker.RedoStack.Count}", MessageType.Info);
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

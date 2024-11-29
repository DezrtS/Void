using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomData), true)]
public class TilePositionsHolderEditor : Editor
{
    private IHoldTilePositions tilePositionsHolder;
    private CommandInvoker commandInvoker;
    private bool showSettings = false;
    private Vector2Int gridSize = Vector2Int.one;

    public virtual void OnEnable()
    {
        tilePositionsHolder = (RoomData)target;
        commandInvoker = new CommandInvoker();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        showSettings = EditorGUILayout.Foldout(showSettings, "Tile Position Settings");

        if (showSettings)
        {
            gridSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize);
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update Grid Size", GUILayout.Width(inspectorWidth - 26), GUILayout.Height(50f)))
            {
                ICommand command = new ChangeGridSize(tilePositionsHolder, gridSize);
                commandInvoker.ExecuteCommand(command);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (tilePositionsHolder.GridSize.x <= 0 || tilePositionsHolder.GridSize.y <= 0)
            {
                EditorGUILayout.HelpBox("Grid size must be greater than zero.", MessageType.Warning);
                return;
            }

            float gridButtonSize = Mathf.Min((inspectorWidth - tilePositionsHolder.GridSize.x * 5f) / tilePositionsHolder.GridSize.x, 50f);
            for (int y = 0; y < tilePositionsHolder.GridSize.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < tilePositionsHolder.GridSize.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    bool contains = tilePositionsHolder.TilePositions.Contains(position);
                    GUI.backgroundColor = GetColor(contains);
                    if (GUILayout.Button($"{x},{y}", GUILayout.Width(gridButtonSize), GUILayout.Height(gridButtonSize)))
                    {
                        if (contains)
                        {
                            ICommand command = new DeselectGridPosition(tilePositionsHolder, position);
                            commandInvoker.ExecuteCommand(command);
                        }
                        else
                        {
                            ICommand command = new SelectGridPosition(tilePositionsHolder, position);
                            commandInvoker.ExecuteCommand(command);
                        }
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;

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
                ICommand command = new ResetGridPositions(tilePositionsHolder);
                commandInvoker.ExecuteCommand(command);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox($"Command History: {commandInvoker.CommandHistory.Count}, Redo History: {commandInvoker.RedoStack.Count}", MessageType.Info);
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

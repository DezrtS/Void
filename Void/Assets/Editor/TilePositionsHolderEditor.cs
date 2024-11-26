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
            if (tilePositionsHolder.gridSize.x <= 0 || tilePositionsHolder.gridSize.y <= 0)
            {
                EditorGUILayout.HelpBox("Grid size must be greater than zero.", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Undo"))
                {
                    commandInvoker.Undo();
                }
                if (GUILayout.Button("Reset"))
                {
                    tilePositionsHolder.GridSize = Vector2Int.one;
                    ICommand command = new ChangeGridSize(tilePositionsHolder, tilePositionsHolder.GridSize);
                    commandInvoker.ExecuteCommand(command);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float gridButtonSize = Mathf.Min((inspectorWidth - tilePositionsHolder.gridSize.x * 5f) / tilePositionsHolder.gridSize.x, 50f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update Grid Size", GUILayout.Width(inspectorWidth - 26), GUILayout.Height(50f)))
            {
                ICommand command = new ChangeGridSize(tilePositionsHolder, tilePositionsHolder.GridSize);
                commandInvoker.ExecuteCommand(command);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (int y = 0; y < tilePositionsHolder.gridSize.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < tilePositionsHolder.gridSize.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    bool contains = tilePositionsHolder.tilePositions.Contains(position);
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

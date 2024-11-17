using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileSection), true)]
public class TileSectionEditor : Editor
{
    private TileSection tileSection;
    private CommandInvoker commandInvoker;
    private bool showSettings = false;

    public virtual void OnEnable()
    {
        tileSection = (TileSection)target;
        commandInvoker = new CommandInvoker();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        showSettings = EditorGUILayout.Foldout(showSettings, "Tile Position Settings");

        if (showSettings)
        {
            if (tileSection.gridSize.x <= 0 || tileSection.gridSize.y <= 0)
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
                    tileSection.GridSize = Vector2Int.one;
                    ICommand command = new ChangeGridSize(tileSection, tileSection.GridSize);
                    commandInvoker.ExecuteCommand(command);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float gridButtonSize = Mathf.Min((inspectorWidth - tileSection.gridSize.x * 5f) / tileSection.gridSize.x, 50f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update Grid Size", GUILayout.Width(inspectorWidth - 26), GUILayout.Height(50f)))
            {
                ICommand command = new ChangeGridSize(tileSection, tileSection.GridSize);
                commandInvoker.ExecuteCommand(command);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (int y = 0; y < tileSection.gridSize.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < tileSection.gridSize.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    bool contains = tileSection.tilePositions.Contains(position);
                    GUI.backgroundColor = GetColor(contains);
                    if (GUILayout.Button($"{x},{y}", GUILayout.Width(gridButtonSize), GUILayout.Height(gridButtonSize)))
                    {
                        if (contains)
                        {
                            ICommand command = new DeselectGridPosition(tileSection, position);
                            commandInvoker.ExecuteCommand(command);
                        }
                        else
                        {
                            ICommand command = new SelectGridPosition(tileSection, position);
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
                ICommand command = new ResetGridPositions(tileSection);
                commandInvoker.ExecuteCommand(command);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox($"Command History: {commandInvoker.CommandHistory.Count}, Redo History: {commandInvoker.RedoStack.Count}", MessageType.Info);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(tileSection);
            }
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
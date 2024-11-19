using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FixtureData), false)]
public class FixtureDataEditor : TileSectionEditor
{
    private FixtureData fixtureData;
    private RestrictionData restrictionData;
    private CommandInvoker fixtureCommandInvoker;
    private int currentRestriction = 0;
    private bool showRestictionSettings = false;
    private bool showRelationshipSettings = false;

    public override void OnEnable()
    {
        base.OnEnable();
        fixtureData = (FixtureData)target;
        restrictionData = fixtureData.RestrictionData;
        fixtureCommandInvoker = new CommandInvoker();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        showRestictionSettings = EditorGUILayout.Foldout(showRestictionSettings, "Restriction Settings");

        if (showRestictionSettings)
        {
            if (restrictionData != null && restrictionData.Restrictions.Count > 0)
            {
                EditorGUILayout.LabelField("Current Restriction: " + (currentRestriction + 1) + "/" + restrictionData.Restrictions.Count);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Restriction"))
                {
                    restrictionData.Restrictions.Add(new Restriction());
                    currentRestriction = restrictionData.Restrictions.Count - 1;
                }
                if (GUILayout.Button("Remove Restriction"))
                {
                    restrictionData.Restrictions.RemoveAt(currentRestriction);
                    currentRestriction = Mathf.Max(0, currentRestriction - 1);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                // Navigation buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Previous"))
                {
                    currentRestriction = Mathf.Max(0, currentRestriction - 1);
                }
                if (GUILayout.Button("Next"))
                {
                    currentRestriction = Mathf.Min(restrictionData.Restrictions.Count - 1, currentRestriction + 1);
                }
                EditorGUILayout.EndHorizontal();

                // Display current restriction fields
                Restriction restriction = restrictionData.Restrictions[currentRestriction];

                restriction.Type = (Restriction.RestrictionType)EditorGUILayout.EnumPopup("Type", restriction.Type);

                restriction.HasInteriorTileType = EditorGUILayout.Toggle("Has Interior Tile Type", restriction.HasInteriorTileType);
                if (restriction.HasInteriorTileType)
                {
                    restriction.InteriorTileType = (InteriorTile.InteriorTileType)EditorGUILayout.EnumPopup("Interior Tile Type", restriction.InteriorTileType);
                }
                restriction.HasFixtureType = EditorGUILayout.Toggle("Has Fixture Type", restriction.HasFixtureType);
                if (restriction.HasFixtureType)
                {
                    restriction.FixtureType = (FixtureData.FixtureType)EditorGUILayout.EnumPopup("Fixture Type", restriction.FixtureType);
                }
                restriction.HasPathToWalkableTile = EditorGUILayout.Toggle("Has Path To Walkable Tile", restriction.HasPathToWalkableTile);

                EditorGUILayout.LabelField("Positions");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Position"))
                {
                    restriction.Positions.Add(Vector2Int.zero);
                }
                EditorGUILayout.EndHorizontal();
                if (restriction.Positions != null)
                {
                    for (int i = 0; i < restriction.Positions.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        restriction.Positions[i] = EditorGUILayout.Vector2IntField("Position " + (i + 1), restriction.Positions[i]);
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            restriction.Positions.RemoveAt(i);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                // Save changes back to the list
                restrictionData.Restrictions[currentRestriction] = restriction;

                // Save the changes to the ScriptableObject
                EditorUtility.SetDirty(restrictionData);
            }
            else
            {
                EditorGUILayout.LabelField("No restrictions available.");
                if (GUILayout.Button("Add New Restriction"))
                {
                    restrictionData.Restrictions.Add(new Restriction());
                    currentRestriction = restrictionData.Restrictions.Count - 1;
                }
            }
        }

        showRelationshipSettings = EditorGUILayout.Foldout(showRestictionSettings, "Relationship Settings");

        if (showRelationshipSettings)
        {
            
        }
    }
}

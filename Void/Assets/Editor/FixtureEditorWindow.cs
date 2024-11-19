using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;

public class FixtureEditorWindow : EditorWindow
{
    private VisualElement selectionListContainer;
    private VisualElement fixtureDataEditorContainer;
    private VisualElement fixtureRelationshipsEditorContainer;
    private VisualElement tileGridScrollView;
    private VisualElement relationshiopVisualizationScrollView;

    private ProgressBar restrictionProgressBar;
    private ProgressBar relationshipProgressBar;

    // Data Editor
    private Button editRelationshipsButton;
    private Button updateGridSizeButton;
    private Button undoButton;
    private Button redoButton;
    private Button resetButton;

    private Button removeRestrictionButton;
    private Button newRestrictionButton;
    private Button previousRestrictionButton;
    private Button nextRestrictionButton;
    private Button removePositionButton;
    private Button newPositionButton;

    // Relationships Editor
    private Button backToFixturesButton;
    private Button previousRelationshipButton;
    private Button nextRelationshipButton;
    private Button removeRelationshipButton;
    private Button newRelationshipButton;

    private TextField fixtureNameField;
    private DropdownField fixtureTagsField;
    private Vector2IntField gridSizeField;
    private EnumField restrictionTypeField;
    private Toggle hasInteriorTileTypeToggle;
    private EnumField interiorTileTypeField;
    private Toggle hasFixtureTypeToggle;
    private ListView allowedFixtureTypeTagList;
    private Toggle hasPathToWalkableTileToggle;

    private FloatField weightField;
    private Vector2IntField positionField;
    private EnumField rotationPresetField;
    private Vector2IntField forwardField;

    private List<FixtureData> fixtures;
    private FixtureData selectedFixture;
    private FixtureData subSelectedFixture;

    private FixtureRelationshipData selectedRelationship;

    private Vector2Int gridSize = new Vector2Int(2, 2);
    List<FixtureRelationshipData> selectedFixtureRelationshipDatas = new();
    private int currentRelationship = 0;
    private int currentRestriction = 0;
    private CommandInvoker commandInvoker;

    [MenuItem("Window/Fixture Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<FixtureEditorWindow>();
        window.titleContent = new GUIContent("Fixture Editor");
        window.Show();
    }

    private void OnEnable()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/FixtureEditor.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UXML/FixtureEditor.uss");

        var root = visualTree.CloneTree();
        rootVisualElement.Add(root);
        //rootVisualElement.styleSheets.Add(styleSheet);

        commandInvoker = new CommandInvoker();

        selectionListContainer = rootVisualElement.Q<VisualElement>("SelectionListContainer");
        fixtureDataEditorContainer = rootVisualElement.Q<VisualElement>("FixtureDataEditorContainer");
        fixtureRelationshipsEditorContainer = rootVisualElement.Q<VisualElement>("FixtureRelationshipsEditorContainer");
        tileGridScrollView = rootVisualElement.Q<ScrollView>("TileGridScrollView");
        relationshiopVisualizationScrollView = rootVisualElement.Q<ScrollView>("RelationshipVisualizationScrollView");

        relationshipProgressBar = rootVisualElement.Q<ProgressBar>("RelationshipProgressBar");

        editRelationshipsButton = rootVisualElement.Q<Button>("EditRelationshipsButton");
        editRelationshipsButton.clicked += OnEditRelationships;
        updateGridSizeButton = rootVisualElement.Q<Button>("UpdateGridSizeButton");
        updateGridSizeButton.clicked += OnUpdateGridSize;
        undoButton = rootVisualElement.Q<Button>("UndoButton");
        undoButton.clicked += () =>
        {
            commandInvoker.Undo();
            EditorUtility.SetDirty(selectedFixture);
            GenerateTileGrid();
        };
        redoButton = rootVisualElement.Q<Button>("RedoButton");
        redoButton.clicked += () =>
        {
            commandInvoker.Redo();
            EditorUtility.SetDirty(selectedFixture);
            GenerateTileGrid();
        };
        resetButton = rootVisualElement.Q<Button>("ResetButton");
        resetButton.clicked += () =>
        {
            ICommand command = new ResetGridPositions(selectedFixture);
            commandInvoker.ExecuteCommand(command);
            EditorUtility.SetDirty(selectedFixture);
            GenerateTileGrid();
        };

        removeRestrictionButton = rootVisualElement.Q<Button>("RemoveRelationshipButton");
        removeRestrictionButton.clicked += OnRemoveRestriction;
        newRestrictionButton = rootVisualElement.Q<Button>("NewRestrictionButton");
        newRestrictionButton.clicked += OnNewRestriction;
        previousRestrictionButton = rootVisualElement.Q<Button>("PreviousRestrictionButton");
        previousRestrictionButton.clicked += () =>
        {
            currentRestriction = Mathf.Max(0, currentRestriction - 1);
        };
        nextRestrictionButton = rootVisualElement.Q<Button>("NextRestrictionButton");
        nextRestrictionButton.clicked += () =>
        {
            if (!selectedFixture)
            {
                return;
            }
            else if (!selectedFixture.RestrictionData)
            {
                return;
            }

            selectedFixture.RestrictionData.Restrictions ??= new();
            currentRestriction = Mathf.Min(selectedFixture.RestrictionData.Restrictions.Count() - 1, currentRestriction + 1);
        };
        removePositionButton = rootVisualElement.Q<Button>("RemovePositionButton");
        removePositionButton.clicked += OnRemovePosition;
        newPositionButton = rootVisualElement.Q<Button>("NewPositionButton");
        newPositionButton.clicked += OnNewPosition;

        backToFixturesButton = rootVisualElement.Q<Button>("BackToFixturesButton");
        backToFixturesButton.clicked += OnBackToFixtures;
        previousRelationshipButton = rootVisualElement.Q<Button>("PreviousRelationshipButton");
        previousRelationshipButton.clicked += () =>
        {
            currentRelationship = Mathf.Max(0, currentRelationship - 1);
            GenerateRelationshipVisualization();
        };
        nextRelationshipButton = rootVisualElement.Q<Button>("NextRelationshipButton");
        nextRelationshipButton.clicked += () =>
        {
            currentRelationship = Mathf.Min(selectedFixtureRelationshipDatas.Count() - 1, currentRelationship + 1);
            GenerateRelationshipVisualization();
        };
        removeRelationshipButton = rootVisualElement.Q<Button>("RemoveRelationshipButton");
        removeRelationshipButton.clicked += OnRemoveRelationship;
        newRelationshipButton = rootVisualElement.Q<Button>("NewRelationshipButton");
        newRelationshipButton.clicked += OnNewRelationship;

        fixtureNameField = rootVisualElement.Q<TextField>("FixtureNameField");
        fixtureNameField.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {
                selectedFixture.Name = evt.newValue;
                EditorUtility.SetDirty(selectedFixture);
            }
        });

        fixtureTagsField = rootVisualElement.Q<DropdownField>("FixtureTagsField");
        fixtureTagsField.RegisterValueChangedCallback(evt =>
        {
            
        });

        gridSizeField = rootVisualElement.Q<Vector2IntField>("GridSizeField");
        gridSizeField.RegisterValueChangedCallback(evt =>
        {
            gridSize = evt.newValue;
        });

        restrictionTypeField = rootVisualElement.Q<EnumField>("RestrictionTypeField");
        restrictionTypeField.Init(Restriction.RestrictionType.Manditory);
        restrictionTypeField.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {

            }
        });

        hasInteriorTileTypeToggle = rootVisualElement.Q<Toggle>("HasInteriorTileTypeToggle");
        hasInteriorTileTypeToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {

            }
        });

        interiorTileTypeField = rootVisualElement.Q<EnumField>("InteriorTileTypeField");
        interiorTileTypeField.Init(InteriorTile.InteriorTileType.None);
        interiorTileTypeField.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {

            }
        });

        hasFixtureTypeToggle = rootVisualElement.Q<Toggle>("HasFixtureTypeToggle");
        hasFixtureTypeToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {

            }
        });

        allowedFixtureTypeTagList = rootVisualElement.Q<ListView>("FixtureTagField");

        hasPathToWalkableTileToggle = rootVisualElement.Q<Toggle>("HasWalkablePathToggle");
        hasPathToWalkableTileToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedFixture)
            {

            }
        });

        weightField = rootVisualElement.Q<FloatField>("WeightField");
        weightField.RegisterValueChangedCallback(evt =>
        {
            if (selectedRelationship)
            {
                selectedRelationship.Weight = evt.newValue;
            }
        });

        positionField = rootVisualElement.Q<Vector2IntField>("PositionField");
        positionField.RegisterValueChangedCallback(evt =>
        {
            if (selectedRelationship)
            {
                selectedRelationship.Position = evt.newValue;
                selectedRelationship.OtherRelationship.Position = -evt.newValue;
                GenerateRelationshipVisualization();
            }
        });

        rotationPresetField = rootVisualElement.Q<EnumField>("RotationPresetField");
        rotationPresetField.Init(RotationPreset.Zero);
        rotationPresetField.RegisterValueChangedCallback(evt =>
        {
            if (selectedRelationship)
            { 
                selectedRelationship.RotationPreset = (RotationPreset)evt.newValue;
                //selectedRelationship.OtherRelationship.RotationPreset = (RotationPreset)(evt.newValue % 2);
                Matrix4x4 rotationMatrix = new Matrix4x4(
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                );

                if (selectedRelationship.RotationPreset == RotationPreset.Ninety)
                {
                    rotationMatrix = new Matrix4x4(
                        new Vector4(0, -1, 0, 0),
                        new Vector4(1, 0, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                    );

                    selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.TwoSeventy;
                    selectedRelationship.OtherRelationship.RotationMatrix = new Matrix4x4(
                        new Vector4(0, 1, 0, 0),
                        new Vector4(-1, 0, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                    );
                    Vector2 rotatedPosition = selectedRelationship.OtherRelationship.RotationMatrix * (Vector2)selectedRelationship.Position;
                    selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                }
                else if (selectedRelationship.RotationPreset == RotationPreset.OneEighty)
                {
                    rotationMatrix = new Matrix4x4(
                        new Vector4(-1, 0, 0, 0),
                        new Vector4(0, -1, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                    );

                    selectedRelationship.OtherRelationship.RotationPreset = selectedRelationship.RotationPreset;
                    selectedRelationship.OtherRelationship.RotationMatrix = rotationMatrix;
                    selectedRelationship.OtherRelationship.Position = selectedRelationship.Position;
                }
                else if (selectedRelationship.RotationPreset == RotationPreset.TwoSeventy)
                {
                    rotationMatrix = new Matrix4x4(
                        new Vector4(0, 1, 0, 0),
                        new Vector4(-1, 0, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                    );

                    selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.Ninety;
                    selectedRelationship.OtherRelationship.RotationMatrix = new Matrix4x4(
                        new Vector4(0, -1, 0, 0),
                        new Vector4(1, 0, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1)
                    );
                    Vector2 rotatedPosition = selectedRelationship.OtherRelationship.RotationMatrix * (Vector2)selectedRelationship.Position;
                    selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                }
                else
                {
                    selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.Zero;
                    selectedRelationship.OtherRelationship.RotationMatrix = rotationMatrix;
                    Vector2 rotatedPosition = rotationMatrix * (Vector2)selectedRelationship.Position;
                    selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                }

                selectedRelationship.RotationMatrix = rotationMatrix;
                GenerateRelationshipVisualization();
            }
        });

        // Fetch All Fixtures
        var guids = AssetDatabase.FindAssets("t:FixtureData");
        fixtures = new List<FixtureData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var fixture = AssetDatabase.LoadAssetAtPath<FixtureData>(path);
            fixtures.Add(fixture);
        }

        OnBackToFixtures();
    }

    private void OnEditRelationships()
    {
        ListView fixtureList = ResetSelectionList();

        fixtureDataEditorContainer.style.display = DisplayStyle.None;
        fixtureRelationshipsEditorContainer.style.display = DisplayStyle.Flex;

        fixtureList.itemsSource = fixtures;
        fixtureList.makeItem = () => new Label();
        fixtureList.bindItem = (element, i) =>
        {
            (element as Label).text = fixtures[i].name;
        };

        fixtureList.SetSelection(0);
        subSelectedFixture = fixtures[0];

        fixtureList.selectionChanged += objects =>
        {
            if (objects.Count() > 0)
            {
                subSelectedFixture = objects.First() as FixtureData;
                OnSelectNewSubFixture();
            }
        };

        GenerateRelationshipVisualization();
    }

    private void OnUpdateGridSize()
    {
        if (selectedFixture)
        {
            ICommand command = new ChangeGridSize(selectedFixture, gridSize);
            commandInvoker.ExecuteCommand(command);
            EditorUtility.SetDirty(selectedFixture);
            GenerateTileGrid();
            Debug.Log("Changed Grid Size");
        }
    }

    private void OnBackToFixtures()
    {
        ListView fixtureList = ResetSelectionList();

        currentRelationship = 0;
        selectedRelationship = null;
        selectedFixtureRelationshipDatas.Clear();

        fixtureRelationshipsEditorContainer.style.display = DisplayStyle.None;
        fixtureDataEditorContainer.style.display = DisplayStyle.Flex;

        fixtureList.itemsSource = fixtures;
        fixtureList.makeItem = () => new Label();
        fixtureList.bindItem = (element, i) =>
        {
            (element as Label).text = fixtures[i].name;
        };

        fixtureList.SetSelection(fixtures.IndexOf(selectedFixture));

        fixtureList.selectionChanged += objects =>
        {
            if (objects.Count() > 0)
            {
                selectedFixture = objects.First() as FixtureData;
                OnSelectNewFixture();
            }
        };

        GenerateTileGrid();
    }

    private void OnRemoveRelationship()
    {
        if (!subSelectedFixture || !selectedFixture || !selectedRelationship)
        {
            return;
        }

        // Get the selected asset path
        string assetPath = AssetDatabase.GetAssetPath(selectedRelationship);
        string otherAssetPath = AssetDatabase.GetAssetPath(selectedRelationship.OtherRelationship);

        if (!string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(otherAssetPath))
        {
            // Confirm deletion
            if (EditorUtility.DisplayDialog("Delete Asset",
                "Are you sure you want to delete this relationship?",
                "Yes", "No"))
            {
                selectedFixture.Relationships.Remove(selectedRelationship);
                subSelectedFixture.Relationships.Remove(selectedRelationship.OtherRelationship);
                // Delete the asset
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.DeleteAsset(otherAssetPath);
                AssetDatabase.SaveAssets();

                // Print a confirmation message
                Debug.Log("Relationship deleted: " + assetPath);
            }
        }
        else
        {
            Debug.LogError("No asset selected or invalid path.");
        }

        currentRelationship = Mathf.Max(0, currentRelationship - 1);
        GenerateRelationshipVisualization();
    }

    private void OnRemoveRestriction()
    {

    }

    private void OnRemovePosition()
    {

    }

    private void OnNewRelationship()
    {
        if (!subSelectedFixture || !selectedFixture)
        {
            return;
        }

        selectedFixture.Relationships ??= new List<FixtureRelationshipData>();
        subSelectedFixture.Relationships ??= new List<FixtureRelationshipData>();

        FixtureRelationshipData fixtureRelationshipData = CreateInstance<FixtureRelationshipData>();
        fixtureRelationshipData.OtherFixture = subSelectedFixture;
        FixtureRelationshipData otherFixtureRelationshipData = CreateInstance<FixtureRelationshipData>();
        otherFixtureRelationshipData.OtherFixture = selectedFixture;
        fixtureRelationshipData.OtherRelationship = otherFixtureRelationshipData;
        otherFixtureRelationshipData.OtherRelationship = fixtureRelationshipData;
        string folderPath = "Assets/Resources/Procedural Generation/Fixture Relationships";
        string assetName = $"{selectedFixture.name}-{subSelectedFixture.name}.asset";
        string assetPath = $"{folderPath}/{assetName}";
        string otherAssetName = $"{subSelectedFixture.name}-{selectedFixture.name}.asset";
        string otherAssetPath = $"{folderPath}/{otherAssetName}";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        otherAssetPath = AssetDatabase.GenerateUniqueAssetPath(otherAssetPath);
        AssetDatabase.CreateAsset(fixtureRelationshipData, assetPath);
        AssetDatabase.CreateAsset(otherFixtureRelationshipData, otherAssetPath);
        AssetDatabase.SaveAssets();

        selectedFixture.Relationships.Add(fixtureRelationshipData);
        subSelectedFixture.Relationships.Add(otherFixtureRelationshipData);
        EditorUtility.SetDirty(selectedFixture);
        EditorUtility.SetDirty(subSelectedFixture);
        GenerateRelationshipVisualization();
        Debug.Log("Created New Relationship");
    }

    private void OnNewRestriction()
    {

    }

    private void OnNewPosition()
    {

    }

    private void OnSelectNewFixture()
    {
        var selectedFixtureLabel = rootVisualElement.Q<Label>("SelectedItem");
        selectedFixtureLabel.text = selectedFixture.name;
        fixtureNameField.value = selectedFixture.Name;
        currentRelationship = 0;

        GenerateTileGrid();
        Debug.Log(selectedFixture.name);
    }

    private void OnSelectNewSubFixture()
    {
        currentRelationship = 0;
        GenerateRelationshipVisualization();
        Debug.Log(subSelectedFixture.name);
    }

    private void OnTileGridButtonPressed(int x, int y, Button button)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (selectedFixture.tilePositions.Contains(position))
        {
            
            ICommand command = new DeselectGridPosition(selectedFixture, position);
            commandInvoker.ExecuteCommand(command);
        }
        else
        {
            ICommand command = new SelectGridPosition(selectedFixture, position);
            commandInvoker.ExecuteCommand(command);
        }

        EditorUtility.SetDirty(selectedFixture);
        GenerateTileGrid();
    }

    public ListView ResetSelectionList()
    {
        selectionListContainer.Clear();
        VisualTreeAsset selectionList = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/SelectionList.uxml");
        selectionListContainer.Add(selectionList.CloneTree());
        ListView fixtureList = rootVisualElement.Q<ListView>("ItemList");
        if (selectedFixture)
        {
            Label selectedFixtureLabel = rootVisualElement.Q<Label>("SelectedItem");
            selectedFixtureLabel.text = selectedFixture.name;
        }
        return fixtureList;
    }

    public void GenerateTileGrid()
    {
        tileGridScrollView.Clear();
        VisualElement tileGridContainer = new VisualElement { name = "GridContainer" };
        tileGridScrollView.Add(tileGridContainer);

        if (!selectedFixture)
        {
            return;
        }

        for (int y = 0; y < selectedFixture.gridSize.y; y++)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            for (int x = 0; x < selectedFixture.gridSize.x; x++)
            {
                int localX = x;
                int localY = y;

                var button = new Button
                {
                    text = $"{x}, {y}",
                    style =
                    {
                        width = 50,
                        height = 50,
                        marginLeft = 2,
                        marginRight = 2,
                        marginTop = 2,
                        marginBottom = 2
                    }
                };

                Vector2Int position = new Vector2Int(localX, localY);

                if (selectedFixture.tilePositions.Contains(position))
                {
                    button.style.backgroundColor = Color.green;
                }
                else
                {
                    button.style.backgroundColor = Color.grey;
                }

                button.clicked += () => OnTileGridButtonPressed(localX, localY, button);
                row.Add(button);
            }
            tileGridContainer.Add(row);
        }
    }

    public void ResetRestrictions()
    {

    }

    public void GenerateRelationshipVisualization()
    {
        relationshiopVisualizationScrollView.Clear();
        VisualElement relationshiopVisualizationContainer = new VisualElement { name = "RelationshipContainer" };
        relationshiopVisualizationScrollView.Add(relationshiopVisualizationContainer);

        relationshipProgressBar.value = 100;
        relationshipProgressBar.title = "Create New Relationship";

        if (!subSelectedFixture || !selectedFixture)
        {
            return;
        }
        else if (selectedFixture.Relationships == null)
        {
            return;
        }
        else if (selectedFixture.Relationships.Count() == 0)
        {
            return;
        }

        selectedFixtureRelationshipDatas = selectedFixture.Relationships.FindAll(r =>
        {
            return r.OtherFixture == subSelectedFixture;
        });

        if (selectedFixtureRelationshipDatas.Count() == 0)
        {
            return;
        }


        relationshipProgressBar.value = ((currentRelationship + 1) / (float)selectedFixtureRelationshipDatas.Count()) * 100f;
        relationshipProgressBar.title = $"Relationship {currentRelationship + 1}/{selectedFixtureRelationshipDatas.Count()}";
        selectedRelationship = selectedFixtureRelationshipDatas[currentRelationship];

        weightField.value = selectedRelationship.Weight;
        positionField.value = selectedRelationship.Position;
        rotationPresetField.value = selectedRelationship.RotationPreset;

        int max = Mathf.Max(selectedRelationship.OtherFixture.gridSize.x, selectedRelationship.OtherFixture.gridSize.y);

        Vector2Int minBounds = new Vector2Int(Mathf.Min(0, -max + selectedRelationship.Position.x), Mathf.Min(0, -max + selectedRelationship.Position.y));
        Vector2Int maxBounds = new Vector2Int(Mathf.Max(selectedFixture.gridSize.x, max + 1 + selectedRelationship.Position.x), Mathf.Max(selectedFixture.gridSize.y, max + 1 + selectedRelationship.Position.y));

        for (int y = minBounds.y; y < maxBounds.y; y++)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            for (int x = minBounds.x; x < maxBounds.x; x++)
            {
                var image = new Image
                {
                    style =
                    {
                        width = 50,
                        height = 50,
                        marginLeft = 2,
                        marginRight = 2,
                        marginTop = 2,
                        marginBottom = 2
                    }
                };

                Vector2Int position = new Vector2Int(x, y);
                Vector2 rotatedPosition = (Vector2)(selectedRelationship.RotationMatrix * (Vector2)(position - selectedRelationship.Position)) + selectedRelationship.Position;

                bool containsMain = selectedFixture.tilePositions.Contains(position);
                bool containsSecondary = selectedRelationship.OtherFixture.tilePositions.Contains(new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y) - selectedRelationship.Position);

                if (containsMain && containsSecondary)
                {
                    image.style.backgroundColor = Color.red;
                }
                else if (containsMain)
                {
                    image.style.backgroundColor = Color.green;
                }
                else if (containsSecondary)
                {
                    image.style.backgroundColor = Color.blue;
                }
                else
                {
                    image.style.backgroundColor = Color.grey;
                }

                row.Add(image);
            }
            relationshiopVisualizationContainer.Add(row);
        }
    }
}
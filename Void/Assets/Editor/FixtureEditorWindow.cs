using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class FixtureEditorWindow : EditorWindow
{
    public Sprite leftArrowSprite;
    public Sprite downArrowSprite;
    public Sprite rightArrowSprite;
    public Sprite upArrowSprite;

    private VisualElement selectionListContainer;
    private VisualElement fixtureDataEditorContainer;
    private VisualElement fixtureRestrictionsEditorContainer;
    private VisualElement fixtureRelationshipsEditorContainer;
    private VisualElement tileGridScrollView;
    private VisualElement relationshiopVisualizationScrollView;

    private VisualElement positionListContainer;

    private ProgressBar restrictionGroupProgressBar;
    private ProgressBar restrictionProgressBar;
    private ProgressBar relationshipProgressBar;

    // Data Editor
    private Button editRestrictionsButton;
    private Button editRelationshipsButton;
    private Button updateGridSizeButton;
    private Button undoButton;
    private Button redoButton;
    private Button resetButton;

    // Restrictions Editor
    private Button backToFixturesButton1;
    private Button removeRestrictionGroupButton;
    private Button newRestrictionGroupButton;
    private Button previousRestrictionGroupButton;
    private Button nextRestrictionGroupButton;
    private Button removeRestrictionButton;
    private Button newRestrictionButton;
    private Button previousRestrictionButton;
    private Button nextRestrictionButton;
    private Button removePositionButton;
    private Button newPositionButton;

    // Relationships Editor
    private Button backToFixturesButton2;
    private Button previousRelationshipButton;
    private Button nextRelationshipButton;
    private Button removeRelationshipButton;
    private Button newRelationshipButton;

    private TextField fixtureNameField;
    //private DropdownField fixtureTagsField;
    private Vector2IntField gridSizeField;

    private Toggle restrictionGroupEnabledToggle;
    //private ListView restrictionGroupRoomTypeList;
    private EnumField restrictionTypeField;
    private Toggle hasInteriorTileTypeToggle;
    private EnumField interiorTileTypeField;
    //private ListView allowedFixtureTypeTagList;
    private Toggle hasPathToWalkableTileToggle;
    
    
    private Toggle relationshipEnabledToggle;
    private FloatField weightField;
    private Vector2IntField positionField;
    private EnumField rotationPresetField;
    private Vector2IntField forwardField;

    private List<FixtureData> fixtures;
    private FixtureData selectedFixture;
    private FixtureData subSelectedFixture;

    private RestrictionData selectedRestrictionGroup;
    private Restriction selectedRestriction = null;
    private FixtureRelationshipData selectedRelationship;

    private Vector2Int selectedPosition;

    private Vector2Int gridSize = new Vector2Int(2, 2);
    List<RestrictionData> selectedRestrictionGroupDatas = new();
    List<Restriction> selectedRestrictionDatas = new();
    List<FixtureRelationshipData> selectedFixtureRelationshipDatas = new();
    private int currentRelationship = 0;
    private int currentRestrictionGroup = 0;
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
        leftArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arrow_0.png");
        downArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arrow_90.png");
        rightArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arrow_180.png");
        upArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arrow_270.png");

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/FixtureEditor.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UXML/FixtureEditor.uss");

        var root = visualTree.CloneTree();
        rootVisualElement.Add(root);
        //rootVisualElement.styleSheets.Add(styleSheet);

        commandInvoker = new CommandInvoker();

        selectionListContainer = rootVisualElement.Q<VisualElement>("SelectionListContainer");
        positionListContainer = rootVisualElement.Q<VisualElement>("PositionListContainer");

        fixtureDataEditorContainer = rootVisualElement.Q<VisualElement>("FixtureDataEditorContainer");
        fixtureRestrictionsEditorContainer = rootVisualElement.Q<VisualElement>("FixtureRestrictionGroupsEditorContainer");
        fixtureRelationshipsEditorContainer = rootVisualElement.Q<VisualElement>("FixtureRelationshipsEditorContainer");
        tileGridScrollView = rootVisualElement.Q<ScrollView>("TileGridScrollView");
        relationshiopVisualizationScrollView = rootVisualElement.Q<ScrollView>("RelationshipVisualizationScrollView");

        restrictionGroupProgressBar = rootVisualElement.Q<ProgressBar>("RestrictionGroupProgressBar");
        restrictionProgressBar = rootVisualElement.Q<ProgressBar>("RestrictionProgressBar");
        relationshipProgressBar = rootVisualElement.Q<ProgressBar>("RelationshipProgressBar");

        editRestrictionsButton = rootVisualElement.Q<Button>("EditRestrictionsButton");
        editRestrictionsButton.clicked += OnEditRestrictions;
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


        backToFixturesButton1 = rootVisualElement.Q<Button>("BackToFixturesButton1");
        backToFixturesButton1.clicked += OnBackToFixtures;
        removeRestrictionGroupButton = rootVisualElement.Q<Button>("RemoveRestrictionGroupButton");
        removeRestrictionGroupButton.clicked += OnRemoveRestrictionGroup;
        newRestrictionGroupButton = rootVisualElement.Q<Button>("NewRestrictionGroupButton");
        newRestrictionGroupButton.clicked += OnNewRestrictionGroup;
        previousRestrictionGroupButton = rootVisualElement.Q<Button>("PreviousRestrictionGroupButton");
        previousRestrictionGroupButton.clicked += () =>
        {
            currentRestrictionGroup = Mathf.Max(0, currentRestrictionGroup - 1);
            ResetRestrictionGroup();
            ResetRestriction();
            ResetPositionList();
        };
        nextRestrictionGroupButton = rootVisualElement.Q<Button>("NextRestrictionGroupButton");
        nextRestrictionGroupButton.clicked += () =>
        {
            currentRestrictionGroup = Mathf.Min(selectedRestrictionGroupDatas.Count() - 1, currentRestrictionGroup + 1);
            ResetRestrictionGroup();
            ResetRestriction();
            ResetPositionList();
        };
        removeRestrictionButton = rootVisualElement.Q<Button>("RemoveRestrictionButton");
        removeRestrictionButton.clicked += OnRemoveRestriction;
        newRestrictionButton = rootVisualElement.Q<Button>("NewRestrictionButton");
        newRestrictionButton.clicked += OnNewRestriction;
        previousRestrictionButton = rootVisualElement.Q<Button>("PreviousRestrictionButton");
        previousRestrictionButton.clicked += () =>
        {
            currentRestriction = Mathf.Max(0, currentRestriction - 1);
            ResetRestriction();
            ResetPositionList();
        };
        nextRestrictionButton = rootVisualElement.Q<Button>("NextRestrictionButton");
        nextRestrictionButton.clicked += () =>
        {
            currentRestriction = Mathf.Min(selectedRestrictionDatas.Count() - 1, currentRestriction + 1);
            ResetRestriction();
            ResetPositionList();
        };
        removePositionButton = rootVisualElement.Q<Button>("RemovePositionButton");
        removePositionButton.clicked += OnRemovePosition;
        newPositionButton = rootVisualElement.Q<Button>("NewPositionButton");
        newPositionButton.clicked += OnNewPosition;

        backToFixturesButton2 = rootVisualElement.Q<Button>("BackToFixturesButton2");
        backToFixturesButton2.clicked += OnBackToFixtures;
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

        //fixtureTagsField = rootVisualElement.Q<DropdownField>("FixtureTagsField");
        //fixtureTagsField.RegisterValueChangedCallback(evt =>
        //{
            
        //});

        gridSizeField = rootVisualElement.Q<Vector2IntField>("GridSizeField");
        gridSizeField.RegisterValueChangedCallback(evt =>
        {
            gridSize = evt.newValue;
        });

        restrictionGroupEnabledToggle = rootVisualElement.Q<Toggle>("RestrictionEnabledField");
        restrictionGroupEnabledToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedRestrictionGroup)
            {
                selectedRestrictionGroup.Enabled = evt.newValue;
            }
        });

        restrictionTypeField = rootVisualElement.Q<EnumField>("RestrictionTypeField");
        restrictionTypeField.Init(Restriction.RestrictionType.Manditory);
        restrictionTypeField.RegisterValueChangedCallback(evt =>
        {
            if (selectedRestriction != null)
            {
                selectedRestriction.Type = (Restriction.RestrictionType)evt.newValue;
            }
        });

        hasInteriorTileTypeToggle = rootVisualElement.Q<Toggle>("HasInteriorTileTypeToggle");
        hasInteriorTileTypeToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedRestriction != null)
            {
                selectedRestriction.HasInteriorTileType = evt.newValue;
            }
        });

        interiorTileTypeField = rootVisualElement.Q<EnumField>("InteriorTileTypeField");
        interiorTileTypeField.Init(InteriorTile.InteriorTileType.None);
        interiorTileTypeField.RegisterValueChangedCallback(evt =>
        {
            if (selectedRestriction != null)
            {
                selectedRestriction.InteriorTileType = (InteriorTile.InteriorTileType)evt.newValue;
            }
        });

        hasPathToWalkableTileToggle = rootVisualElement.Q<Toggle>("HasWalkablePathToggle");
        hasPathToWalkableTileToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedRestriction != null)
            {
                selectedRestriction.HasPathToWalkableTile = evt.newValue;
            }
        });

        relationshipEnabledToggle = rootVisualElement.Q<Toggle>("RelationshipEnabledToggle");
        relationshipEnabledToggle.RegisterValueChangedCallback(evt =>
        {
            if (selectedRelationship)
            {
                selectedRelationship.Enabled = evt.newValue;
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
                RecalculateSubFixturePosition();
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
                //Matrix4x4 rotationMatrix = new Matrix4x4(
                //    new Vector4(1, 0, 0, 0),
                //    new Vector4(0, 1, 0, 0),
                //    new Vector4(0, 0, 1, 0),
                //    new Vector4(0, 0, 0, 1)
                //);

                //if (selectedRelationship.RotationPreset == RotationPreset.Ninety)
                //{
                //    rotationMatrix = new Matrix4x4(
                //        new Vector4(0, -1, 0, 0),
                //        new Vector4(1, 0, 0, 0),
                //        new Vector4(0, 0, 1, 0),
                //        new Vector4(0, 0, 0, 1)
                //    );
                //}
                //else if (selectedRelationship.RotationPreset == RotationPreset.OneEighty)
                //{
                //    rotationMatrix = new Matrix4x4(
                //        new Vector4(-1, 0, 0, 0),
                //        new Vector4(0, -1, 0, 0),
                //        new Vector4(0, 0, 1, 0),
                //        new Vector4(0, 0, 0, 1)
                //    );
                //}
                //else if (selectedRelationship.RotationPreset == RotationPreset.TwoSeventy)
                //{
                //    rotationMatrix = new Matrix4x4(
                //        new Vector4(0, 1, 0, 0),
                //        new Vector4(-1, 0, 0, 0),
                //        new Vector4(0, 0, 1, 0),
                //        new Vector4(0, 0, 0, 1)
                //    );
                //}
                selectedRelationship.RotationMatrix = GridMapManager.GetRotationMatrix(selectedRelationship.RotationPreset);
                RecalculateSubFixturePosition();
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

    private void OnEditRestrictions()
    {
        ListView fixtureList = ResetSelectionList();
        fixtureDataEditorContainer.style.display = DisplayStyle.None;
        fixtureRestrictionsEditorContainer.style.display = DisplayStyle.Flex;

        ResetRestrictionGroup();
        ResetRestriction();
        ResetPositionList();
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

        currentRestrictionGroup = 0;
        selectedRestrictionGroup = null;
        selectedRestrictionGroupDatas = new();
        currentRestriction = 0;
        selectedRestriction = null;
        selectedRestrictionDatas = new();
        selectedPosition = Vector2Int.zero;
        currentRelationship = 0;
        selectedRelationship = null;
        selectedFixtureRelationshipDatas = new();

        fixtureRelationshipsEditorContainer.style.display = DisplayStyle.None;
        fixtureRestrictionsEditorContainer.style.display = DisplayStyle.None;
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

    private void OnRemoveRestrictionGroup()
    {
        if (!selectedFixture || !selectedRestrictionGroup)
        {
            return;
        }

        // Get the selected asset path
        string assetPath = AssetDatabase.GetAssetPath(selectedRestrictionGroup);

        if (!string.IsNullOrEmpty(assetPath))
        {
            // Confirm deletion
            if (EditorUtility.DisplayDialog("Delete Asset",
                "Are you sure you want to delete this restriction group?",
                "Yes", "No"))
            {
                selectedFixture.Restrictions.Remove(selectedRestrictionGroup);
                // Delete the asset
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();

                // Print a confirmation message
                Debug.Log("Restriction deleted: " + assetPath);
            }
        }
        else
        {
            Debug.LogError("No asset selected or invalid path.");
        }

        currentRestrictionGroup = Mathf.Max(0, currentRestrictionGroup - 1);
        ResetRestrictionGroup();
        ResetRestriction();
        ResetPositionList();
    }

    private void OnRemoveRestriction()
    {
        if (EditorUtility.DisplayDialog("Delete Restriction",
            "Are you sure you want to delete this restriction?",
            "Yes", "No"))
        {
            selectedRestrictionGroup.Restrictions.Remove(selectedRestriction);
            selectedRestriction = null;
            currentRestriction = Mathf.Max(0, currentRestriction - 1);
            ResetRestriction();
            ResetPositionList();
        }
    }

    private void OnRemovePosition()
    {
        Debug.Log("REMOVING SOMETHING");
        Debug.Log(selectedPosition);
        if (selectedRestriction == null)
        {
            return;
        }
        else if (selectedPosition == Vector2Int.zero)
        {
            return;
        }

        selectedRestriction.Positions.Remove(selectedPosition);
        ResetPositionList();
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

    private void OnNewRestrictionGroup()
    {
        if (!selectedFixture)
        {
            return;
        }

        selectedFixture.Restrictions ??= new List<RestrictionData>();

        RestrictionData restrictionData = CreateInstance<RestrictionData>();
        string folderPath = "Assets/Resources/Procedural Generation/Fixture Restrictions";
        string assetName = $"{selectedFixture.name}-Restriction.asset";
        string assetPath = $"{folderPath}/{assetName}";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        AssetDatabase.CreateAsset(restrictionData, assetPath);
        AssetDatabase.SaveAssets();

        selectedFixture.Restrictions.Add(restrictionData);
        EditorUtility.SetDirty(selectedFixture);
        ResetRestrictionGroup();
        ResetRestriction();
        ResetPositionList();
        Debug.Log("Created New Restriction");
    }

    private void OnNewRestriction()
    {
        if (!selectedRestrictionGroup)
        {
            return;
        }

        selectedRestrictionGroup.Restrictions.Add(new Restriction());
        ResetRestriction();
        ResetPositionList();
    }

    private void OnNewPosition()
    {
        if (selectedRestriction == null)
        {
            return;
        }

        selectedRestriction.Positions.Add(Vector2Int.zero);
        ResetPositionList();
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

    private void RecalculateSubFixturePosition()
    {
        if (selectedRelationship)
        {
            if (selectedRelationship.RotationPreset == RotationPreset.Ninety)
            {
                selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.TwoSeventy;
                selectedRelationship.OtherRelationship.RotationMatrix = new Matrix4x4(
                    new Vector4(0, -1, 0, 0),
                    new Vector4(1, 0, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                );
                Vector2 rotatedPosition = selectedRelationship.OtherRelationship.RotationMatrix * (Vector2)selectedRelationship.Position;
                selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
            }
            else if (selectedRelationship.RotationPreset == RotationPreset.OneEighty)
            {
                selectedRelationship.OtherRelationship.RotationPreset = selectedRelationship.RotationPreset;
                selectedRelationship.OtherRelationship.RotationMatrix = new Matrix4x4(
                    new Vector4(-1, 0, 0, 0),
                    new Vector4(0, -1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                );
                selectedRelationship.OtherRelationship.Position = selectedRelationship.Position;
            }
            else if (selectedRelationship.RotationPreset == RotationPreset.TwoSeventy)
            {
                selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.Ninety;
                selectedRelationship.OtherRelationship.RotationMatrix = new Matrix4x4(
                    new Vector4(0, 1, 0, 0),
                    new Vector4(-1, 0, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                );
                Vector2 rotatedPosition = selectedRelationship.OtherRelationship.RotationMatrix * (Vector2)selectedRelationship.Position;
                selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
            }
            else
            {
                selectedRelationship.OtherRelationship.RotationPreset = RotationPreset.Zero;
                selectedRelationship.OtherRelationship.RotationMatrix = selectedRelationship.RotationMatrix;
                Matrix4x4 positionMatrix = new Matrix4x4(
                    new Vector4(-1, 0, 0, 0),
                    new Vector4(0, -1, 0, 0),
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1)
                );
                Vector2 rotatedPosition = positionMatrix * (Vector2)selectedRelationship.Position;
                selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
            }
            //selectedRelationship.OtherRelationship.Position = selectedRelationship.Position;

            //selectedRelationship.OtherRelationship.RotationPreset = (RotationPreset)(((int)selectedRelationship.RotationPreset + 2) % 4);
            //selectedRelationship.OtherRelationship.RotationMatrix = selectedRelationship.RotationMatrix.inverse;
            //Vector2 rotatedPosition = selectedRelationship.OtherRelationship.RotationMatrix * (Vector2)selectedRelationship.Position;
            //selectedRelationship.OtherRelationship.Position = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
        }
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

        for (int y = selectedFixture.gridSize.y - 1; y >= 0; y--)
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

    public void ResetRestrictionGroup()
    {
        restrictionGroupProgressBar.value = 100;
        restrictionGroupProgressBar.title = "Create New Restriction Group";
        selectedRestriction = null;
        currentRestriction = 0;

        if (!selectedFixture)
        {
            return;
        }
        else if (selectedFixture.Restrictions.Count() == 0)
        {
            return;
        }

        selectedRestrictionGroupDatas = selectedFixture.Restrictions;
        restrictionGroupProgressBar.value = ((currentRestrictionGroup + 1) / (float)selectedRestrictionGroupDatas.Count()) * 100f;
        restrictionGroupProgressBar.title = $"Restriction Group {currentRestrictionGroup + 1}/{selectedRestrictionGroupDatas.Count()}";
        selectedRestrictionGroup = selectedRestrictionGroupDatas[currentRestrictionGroup];

        restrictionGroupEnabledToggle.value = selectedRestrictionGroup.Enabled;
        //restrictionGroupRoomTypeList. = selectedRestrictionGroup.TileCollectionTypes;
    }

    public void ResetRestriction()
    {
        restrictionProgressBar.value = 100;
        restrictionProgressBar.title = "Create New Restriction";

        if (!selectedRestrictionGroup)
        {
            return;
        }
        else if (selectedRestrictionGroup.Restrictions.Count() == 0)
        {
            return;
        }

        selectedRestrictionDatas = selectedRestrictionGroup.Restrictions;
        restrictionProgressBar.value = ((currentRestriction + 1) / (float)selectedRestrictionDatas.Count()) * 100f;
        restrictionProgressBar.title = $"Restriction Group {currentRestriction + 1}/{selectedRestrictionDatas.Count()}";
        selectedRestriction = selectedRestrictionDatas[currentRestriction];

        restrictionTypeField.value = selectedRestriction.Type;
        hasInteriorTileTypeToggle.value = selectedRestriction.HasInteriorTileType;
        interiorTileTypeField.value = selectedRestriction.InteriorTileType;
        //allowedFixtureTypeTagList.
        hasPathToWalkableTileToggle.value = selectedRestriction.HasPathToWalkableTile;
    }

    public void ResetPositionList()
    {
        positionListContainer.Clear();
        ListView positionList = new ListView();
        positionList.name = "PositionList";
        positionListContainer.Add(positionList);

        if (selectedRestriction == null)
        {
            return;
        }

        List<Vector2Int> positions = selectedRestriction.Positions;

        positionList.itemsSource = positions;
        positionList.makeItem = () => new Vector2IntField();
        positionList.bindItem = (element, i) =>
        {
            Vector2IntField field = (element as Vector2IntField);
            field.value = positions[i];
            field.RegisterValueChangedCallback(evt =>
            {
                if (selectedRestriction != null)
                {
                    selectedRestriction.Positions[i] = evt.newValue;
                    positionList.SetSelection(i);
                    selectedPosition = evt.newValue;
                }
            });
        };

        positionList.selectionChanged += objects =>
        {
            if (objects.Count() > 0)
            {
                selectedPosition = (Vector2Int)objects.First();
            }
        };
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

        relationshipEnabledToggle.value = selectedRelationship.Enabled;
        weightField.value = selectedRelationship.Weight;
        positionField.value = selectedRelationship.Position;
        rotationPresetField.value = selectedRelationship.RotationPreset;

        int max = Mathf.Max(selectedRelationship.OtherFixture.gridSize.x, selectedRelationship.OtherFixture.gridSize.y);

        Vector2Int minBounds = new Vector2Int(Mathf.Min(0, -max + selectedRelationship.Position.x), Mathf.Min(0, -max + selectedRelationship.Position.y));
        Vector2Int maxBounds = new Vector2Int(Mathf.Max(selectedFixture.gridSize.x, max + 1 + selectedRelationship.Position.x), Mathf.Max(selectedFixture.gridSize.y, max + 1 + selectedRelationship.Position.y));

        for (int y = maxBounds.y - 1; y >= minBounds.y; y--)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            for (int x = minBounds.x; x < maxBounds.x; x++)
            {
                var image = new Image
                {
                    tintColor = Color.grey,
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
                Vector2 rotatedPosition = (Vector2)(selectedRelationship.RotationMatrix.inverse * (Vector2)(position - selectedRelationship.Position)) + selectedRelationship.Position;

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

                if (containsMain)
                {
                    image.sprite = leftArrowSprite;
                }
                else if (containsSecondary)
                {
                    switch (selectedRelationship.RotationPreset)
                    {
                        case RotationPreset.Zero:
                            image.sprite = leftArrowSprite;
                            break;
                        case RotationPreset.Ninety:
                            image.sprite = downArrowSprite;
                            break;
                        case RotationPreset.OneEighty:
                            image.sprite = rightArrowSprite;
                            break;
                        case RotationPreset.TwoSeventy:
                            image.sprite = upArrowSprite;
                            break;
                    }
                }

                row.Add(image);
            }
            relationshiopVisualizationContainer.Add(row);
        }
    }
}
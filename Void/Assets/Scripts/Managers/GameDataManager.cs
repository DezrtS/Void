using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager>
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();
    [SerializeField] private List<ResourceData> resources = new List<ResourceData>();

    [SerializeField] private List<MutationData> mutations = new List<MutationData>();
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();

    [SerializeField] private List<StatChangesData> statChanges = new List<StatChangesData>();

    [SerializeField] private List<DialogueData> dialogues = new List<DialogueData>();

    public List<ItemData> Items => items;
    public List<RecipeData> Recipes => recipes;
    public List<ResourceData> Resources => resources;

    public List<MutationData> Mutations => mutations;
    public List<TaskData> Tasks => tasks;

    public List<StatChangesData> StatChanges => statChanges;

    public int GetItemDataIndex(ItemData itemData)
    {
        return items.IndexOf(itemData);
    }

    public ItemData GetItemData(int index)
    {
        return items[index];
    }

    public int GetRecipeDataIndex(RecipeData recipeData)
    {
        return recipes.IndexOf(recipeData);
    }

    public RecipeData GetRecipeData(int index)
    {
        return recipes[index];
    }

    public int GetResourceDataIndex(ResourceData resourceData)
    {
        return resources.IndexOf(resourceData);
    }

    public ResourceData GetResourceData(int index)
    {
        return resources[index];
    }

    public int GetMutationDataIndex(MutationData mutationData)
    {
        return mutations.IndexOf(mutationData);
    }

    public MutationData GetMutationData(int index)
    {
        return mutations[index];
    }

    public int GetTaskDataIndex(TaskData taskData)
    {
        return tasks.IndexOf(taskData);
    }

    public TaskData GetTaskData(int index)
    {
        return tasks[index];
    }

    public int GetStatChangesDataIndex(StatChangesData statChangesData)
    {
        return statChanges.IndexOf(statChangesData);
    }

    public StatChangesData GetStatChangesData(int index)
    {
        return statChanges[index];
    }

    public int GetDialogueDataIndex(DialogueData dialogueData)
    {
        return dialogues.IndexOf(dialogueData);
    }

    public DialogueData GetDialogueData(int index)
    {
        return dialogues[index];
    }

    public static Item SpawnItem(ItemData itemData)
    {
        GameObject spawnedItem = Instantiate(itemData.ItemPrefab);
        Item item = spawnedItem.GetComponent<Item>();
        item.NetworkItem.NetworkObject.Spawn(true);
        return item;
    }

    public static Mutation SpawnMutation(int mutationDataIndex)
    {
        return SpawnMutation(Instance.GetMutationData(mutationDataIndex));
    }

    public static Mutation SpawnMutation(MutationData mutationData)
    {
        GameObject spawnedMutation = Instantiate(mutationData.MutationPrefab);
        Mutation mutation = spawnedMutation.GetComponent<Mutation>();
        mutation.NetworkUseable.NetworkObject.Spawn(true);
        return mutation;
    }
}

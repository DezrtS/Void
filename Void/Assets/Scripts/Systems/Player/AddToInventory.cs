using UnityEngine;

public class AddToInventory : MonoBehaviour
{
    [SerializeField] private bool dropIfInventoryFull;
    [SerializeField] private bool isNetworkedItems;
    [SerializeField] private ItemData[] itemDatas;

    private void Awake()
    {
        if (TryGetComponent(out Inventory inventory))
        {


            foreach (ItemData itemData in itemDatas)
            {
                //inventory.Pic
            }
        }

        Destroy(this);
    }
}

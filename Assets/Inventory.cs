using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxInventorySize;
    private List<Item> Items;
    public bool AddItem(Item item)
    {
        
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].name == item.name)
            {
                if (Items[i].isFull) continue;
                Items[i].amount += item.amount;
                return true;

            }
        }

        
        Items.Add(item);
        

        return true;
    }
}

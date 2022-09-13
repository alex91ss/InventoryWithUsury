using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Enums;


public class ItemClass : ScriptableObject
{
    [Header("Item")]
    public string itemName;
    public Sprite itemIcon;
    public bool isStackable = true;
    public int stackSize = 20;
    [SerializeField] public int itemUsury = 100;
    public SlotType slotType;
    
    public virtual void Use(InteractionHandler controller) {
        Debug.Log("Class: Item Class");
    
    }
    public virtual ItemClass GetItem() { return this; }
    public virtual ItemClass GetTool() { return null; }
    public virtual MiscClass GetMisc() { return null; }
    public virtual ConsumableClass GetConsumable() { return null; }

}


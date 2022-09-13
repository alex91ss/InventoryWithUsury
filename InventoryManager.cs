using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject slotHolder;
    [SerializeField] private GameObject hotbarSlotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;

    [SerializeField] private SlotClass[] startingItems;

    [SerializeField] private GameObject dropModel;
    [SerializeField] private Transform dropPosition;

    private SlotClass[] items;


    //inventory
    private GameObject[] slots;
    private GameObject[] hotbarSlots;


    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    bool isMovingItem;

    [SerializeField] private GameObject hotbarSelector;
    [SerializeField] private int selectedSlotIndex = 0;
    public ItemClass selectedItem;

    public InventoryHandler inventoryHandler;



    private void Start()
    {
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];


        hotbarSlots = new GameObject[hotbarSlotHolder.transform.childCount];

        //initializing lists

        for (int i = 0; i < hotbarSlots.Length; i++)
            hotbarSlots[i] = hotbarSlotHolder.transform.GetChild(i).gameObject;


        for (int i = 0; i < items.Length; i++)
            items[i] = new SlotClass();

        for (int i = 0; i < startingItems.Length; i++)
            items[i] = startingItems[i];


        //set all the slots
        for (int i = 0; i < slotHolder.transform.childCount; i++)
            slots[i] = slotHolder.transform.GetChild(i).gameObject;

        RefreshUI();

    }

    private void Update()
    {
        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Input.mousePosition;

        if (isMovingItem)
            itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

        if (Input.GetMouseButtonDown(0)) //left clicked
        {
            //find the closest slot 
            if (isMovingItem)
            {
                //end Item Move
                EndItemMove();
            }
            else
                BeginItemMove();
        }

        else if (Input.GetMouseButtonDown(1)) // right click
        {
            //TO DO REMOVE ITEM
            SlotClass deleteSlot = GetClosestSlot();
            if (deleteSlot.GetItem() != null)
                DropItem(deleteSlot);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0)) // shift + left click to split
        {
            //find the closest slot 
            if (isMovingItem)
            {
                //end Item Move
                //EndItemMove();
            }
            else
            {
                BeginItemMove_Half();

            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) // scroll up
        {
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex + 1, 0, hotbarSlots.Length - 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) //Scroll down
        {
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex - 1, 0, hotbarSlots.Length - 1);
        }

        hotbarSelector.transform.position = hotbarSlots[selectedSlotIndex].transform.position;
        selectedItem = items[selectedSlotIndex + (hotbarSlots.Length * 3)].GetItem();
    }
    #region Inventory Utils
    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            try
            {
                //slot image
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                //slot text
                if (items[i].GetItem().isStackable)
                    slots[i].transform.GetChild(2).GetComponent<Text>().text = items[i].GetQuantity() + "";
                else
                    slots[i].transform.GetChild(2).GetComponent<Text>().text = "";
                //item usury
                if (items[i].GetItem().slotType == Inventory.Enums.SlotType.weapon || items[i].GetItem().slotType == Inventory.Enums.SlotType.tool)
                {
                    Debug.Log("this is a weapon!");
                    //showing the item life bar
                    slots[i].transform.GetChild(1).gameObject.SetActive(true);
                    slots[i].transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = (float)items[i].GetItem().itemUsury / 100;

                }
              
               
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).gameObject.SetActive(false);
                slots[i].transform.GetChild(2).GetComponent<Text>().text = "";


            }

        }
        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            {
                try
                {
                    //slot image
                    hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i + (hotbarSlots.Length * 3)].GetItem().itemIcon;
                    //slot text
                    if (items[i + (hotbarSlots.Length * 3)].GetItem().isStackable)
                        hotbarSlots[i].transform.GetChild(2).GetComponent<Text>().text = items[i + (hotbarSlots.Length * 3)].GetQuantity() + "";
                    else
                        hotbarSlots[i].transform.GetChild(2).GetComponent<Text>().text = "";

                    if (items[i + (hotbarSlots.Length * 3)].GetItem().slotType == Inventory.Enums.SlotType.weapon || items[i + (hotbarSlots.Length * 3)].GetItem().slotType == Inventory.Enums.SlotType.tool)
                    {
                        Debug.Log("HotBar setting active!");
                        //showing the item life bar
                        hotbarSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                        hotbarSlots[i].transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = (float)items[i + (hotbarSlots.Length * 3)].GetItem().itemUsury / 100;


                    }
                }
                catch
                {
                    hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                    hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                    hotbarSlots[i].transform.GetChild(1).gameObject.SetActive(false);

                    hotbarSlots[i].transform.GetChild(2).GetComponent<Text>().text = "";
                }
            }
        }
    }

     
    public bool Add(ItemClass item, int quantity)
    {
        //check if inventory contains item
        SlotClass slot = Contains(item);

        if (slot != null && slot.item.isStackable && slot.quantity < item.stackSize)
        {

            var quantityCanAdd = slot.item.stackSize - slot.quantity;
            var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);

            var remainder = quantity - quantityCanAdd;

            slot.AddQuantity(quantityToAdd);
            if (remainder > 0) Add(item, remainder);
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].item == null) //this is an empty slot
                {
                    var quantityCanAdd = item.stackSize - items[i].quantity;
                    var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);

                    var remainder = quantity - quantityCanAdd;

                    items[i].AddItem(item, quantityToAdd);
                    if (remainder > 0) Add(item, remainder);
                    break;
                }
            }
        }

        RefreshUI();
        return true;
    }

    public bool Remove(ItemClass item)
    {

        SlotClass temp = Contains(item);

        if (temp != null)
        {
            if (temp.GetQuantity() >= 1)
                temp.SubQuantity(1);
            else
            {
                int slotToRemoveIndex = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        slotToRemoveIndex = i;
                        break;
                    }
                }
                items[slotToRemoveIndex].Clear();
            }
        }
        else
        {
            return false;
        }

        RefreshUI();
        return true;
    }

    public bool Remove(ItemClass item, int quantity)
    {

        SlotClass temp = Contains(item);

        if (temp != null)
        {
            if (temp.GetQuantity() >= 1)
                temp.SubQuantity(quantity);
            else
            {
                int slotToRemoveIndex = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        slotToRemoveIndex = i;
                        break;
                    }
                }
                items[slotToRemoveIndex].Clear();
            }
        }
        else
        {
            return false;
        }

        RefreshUI();
        return true;
    }
    public void UseSelected()
    {
        items[selectedSlotIndex + (hotbarSlots.Length * 3)].SubQuantity(1);
        RefreshUI();
    }

    public bool IsFull()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetItem() == null)
                return false;
        }
        return true;
    }
    public SlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == item /*&& items[i].item.isStackable && */)
                return items[i];
        }

        return null;
    }

    public bool Contains(ItemClass item, int quantity)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetItem() == item && items[i].GetQuantity() >= quantity)
            {
                return true;
            }
        }

        return false;
    }



    #endregion Inventory Utils

    #region Moving Stuff
    private bool BeginItemMove()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null)
            return false; //there is not item to move

        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool BeginItemMove_Half()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null)
            return false; //there is not item to move

        movingSlot = new SlotClass(originalSlot.GetItem(), Mathf.CeilToInt(originalSlot.GetQuantity() / 2f));
        originalSlot.SubQuantity(Mathf.CeilToInt(originalSlot.GetQuantity() / 2f));
        if (originalSlot.GetQuantity() == 0)
            originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }

    private bool EndItemMove()
    {
        originalSlot = GetClosestSlot();

        if (originalSlot == null)
        {
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
        }
        else
        {
            if (originalSlot.GetItem() != null) //slot già usato
            {
                if (originalSlot.GetItem() == movingSlot.GetItem() && originalSlot.GetItem().isStackable && originalSlot.quantity < originalSlot.item.stackSize) //stesso oggetto
                {
                    var quantityCanAdd = originalSlot.item.stackSize - originalSlot.quantity;
                    var quantityToAdd = Mathf.Clamp(movingSlot.quantity, 0, quantityCanAdd);

                    var remainder = movingSlot.quantity - quantityCanAdd;


                    originalSlot.AddQuantity(quantityCanAdd);
                    if (remainder == 0)
                        movingSlot.Clear();
                    else
                    {
                        movingSlot.SubQuantity(quantityCanAdd);
                        RefreshUI();
                        return false;
                    }

                }
                else //oggetto diverso
                {
                    tempSlot = new SlotClass(originalSlot); // a = b
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity()); // b = c
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity()); // a = c
                    RefreshUI();
                    return true;
                }
            }
            else //place item here
            {
                originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }

        isMovingItem = false;
        RefreshUI();
        return true;

    }

    private SlotClass GetClosestSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 32)
                return items[i];
        }

        return null;
    }

    private void DropItem(SlotClass item)
    {
        Pickup pickup = Instantiate(dropModel, dropPosition).AddComponent<Pickup>();
        pickup.transform.position = dropPosition.position;
        pickup.transform.SetParent(null);

        pickup.data = item.GetItem();
        pickup.stackSize = item.GetQuantity();

        if (item.GetQuantity() == 0)
            item.Clear();


        Remove(item.GetItem());
        item.Clear();

        RefreshUI();
    }
    #endregion Moving Stuff
}

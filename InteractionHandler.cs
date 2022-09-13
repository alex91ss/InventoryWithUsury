using Inventory.Enums;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;

    [SerializeField] private MenuUIHandler uiHandler;
    [SerializeField] public InventoryManager inventory;


    private void Update()
    {
        if (Input.GetKeyDown(interactionKey)) //take item
        {
            Interact();
        }

        if (!uiHandler.craftOpened && !uiHandler.inventoryOpened && Input.GetMouseButtonDown(0)) //use item
        {
            if (inventory.selectedItem != null)
            {
                inventory.selectedItem.Use(this);

                if (inventory.selectedItem.slotType != SlotType.def) //if not a default object, add usury
                {
                    inventory.selectedItem.AddUsury();
                    Debug.Log(inventory.selectedItem.itemUsury);
                }

                if (inventory.selectedItem.itemUsury <= 0) // if usury less then zero, then remove it from the inventory
                {
                    inventory.Remove(inventory.selectedItem, 1);
                }

                inventory.RefreshUI();
            }

        }
    }

    private void Interact()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange))
        {
            Pickup pickup = hit.transform.GetComponent<Pickup>();

            if (pickup != null)
            {
                inventory.Add(pickup.data, pickup.stackSize);
                GameObject.Destroy(pickup.gameObject);
            }
        }
    }
}

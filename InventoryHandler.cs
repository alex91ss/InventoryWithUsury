using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class InventoryHandler : MonoBehaviour
{
    public bool opened = false;
    public KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject craftPanel;


    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            opened = !opened;

        if (opened)
        {
            //closing craft
            if (craftPanel.activeSelf)
                craftPanel.SetActive(false);

            inventoryMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None; 
        }
        else
        {
            inventoryMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}


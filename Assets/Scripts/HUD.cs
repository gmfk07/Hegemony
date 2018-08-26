using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    public Inventory inventory;
    public Color selectedColor;
    public Color nonSelectedColor;
    public int selected = 0;
    private int slots;
    private IInventoryItem selectedItem;

    private void Start()
    {
        updateSlots();
        slots = inventory.slots;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int oldSelected = selected;
            if (scroll > 0)
            {
                selected++;
                if (selected >= slots) selected = 0;
                    updateSlots(oldSelected);
            }
            else
            {
                selected--;
                if (selected < 0) selected = slots - 1;
                    updateSlots(oldSelected);
            }
        }
    }

    public void setUpInventoryEvents()
    {
        inventory.ItemAdded += InventoryScript_ItemAdded;
    }

    //Update colors and determine selectedItem (triggering relevant onDeselect event)
    public void updateSlots(int deselected=-1)
    {
        Transform inventoryPanel = transform.Find("InventoryPanel");
        foreach (Transform slot in inventoryPanel)
        {
            if (slot.name == "Slot" + selected.ToString())
            {
                slot.GetComponent<Image>().color = selectedColor;
                if (selected < inventory.pItems.Count)
                {
                    selectedItem = inventory.pItems[selected];
                    selectedItem.OnSelected();
                }
            }
            else
            {
                slot.GetComponent<Image>().color = nonSelectedColor;
                if (slot.name == "Slot" + deselected.ToString())
                    if (deselected < inventory.pItems.Count)
                        inventory.pItems[deselected].OnDeselected();
            }
        }
        
    }

    public void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)
    {
        Transform inventoryPanel = transform.Find("InventoryPanel");
        foreach(Transform slot in inventoryPanel)
        {
            Image image = slot.GetChild(0).GetComponent<Image>();
            //Convoluted and terrible way to check if the slot is also the selected slot, calling OnSelected if so
            if (slot.name[slot.name.Length - 1] == selected.ToString()[0])
                e.Item.OnSelected();
            //Check if this is the first empty slot
            if (!image.enabled)
            {
                image.enabled = true;
                image.sprite = e.Item.Image;

                //TODO: store an item reference

                break;
            }
        }
    }
}

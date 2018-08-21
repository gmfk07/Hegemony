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

    private void Start()
    {
        updateSlotColors();
        slots = inventory.slots;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selected++;
                if (selected >= slots) selected = 0;
                updateSlotColors();
            }
            else
            {
                selected--;
                if (selected < 0) selected = slots - 1;
                updateSlotColors();
            }
        }
    }

    public void setUpInventoryEvents()
    {
        inventory.ItemAdded += InventoryScript_ItemAdded;
    }

    public void updateSlotColors()
    {
        Transform inventoryPanel = transform.Find("InventoryPanel");
        foreach (Transform slot in inventoryPanel)
        {
            if (slot.name == "Slot" + selected.ToString())
                slot.GetComponent<Image>().color = selectedColor;
            else
                slot.GetComponent<Image>().color = nonSelectedColor;
        }
    }

    public void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)
    {
        Transform inventoryPanel = transform.Find("InventoryPanel");
        foreach(Transform slot in inventoryPanel)
        {
            Image image = slot.GetChild(0).GetComponent<Image>();

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

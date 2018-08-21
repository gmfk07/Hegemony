using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private Inventory inventory;
    private IInventoryItem itemToPickup;

    private void Start()
    {
        inventory = gameObject.GetComponent<Inventory>();
        HUD hud = GameObject.Find("HUD").GetComponent<HUD>();
        hud.inventory = inventory;
        hud.setUpInventoryEvents();
    }

    private void Update()
    {
        if (itemToPickup != null && Input.GetKeyDown(KeyCode.E))
        {
            inventory.AddItem(itemToPickup);
            itemToPickup.OnPickup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInventoryItem item = other.GetComponent<IInventoryItem>();
        if (item != null)
        {
            itemToPickup = item;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInventoryItem item = other.GetComponent<IInventoryItem>();
        if (item != null)
        {
            itemToPickup = null;
        }
    }
}

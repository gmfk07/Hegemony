using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour {

    private Inventory inventory;
    private IInventoryItem itemToPickup;
    public Door nearestDoor;
    private HUD hud;
    private MissionStats ms;
    private FirstPersonController fpc;

    private Terminal terminalInRange;
    private float terminalSiphonComplete;

    public int currentPistolAmmo = 0;
    public float terminalSiphonTime = 1f;

    private void Start()
    {
        inventory = gameObject.GetComponent<Inventory>();
        hud = GameObject.Find("HUD").GetComponent<HUD>();
        ms = GameObject.Find("Controller").GetComponent<MissionStats>();
        hud.inventory = inventory;
        hud.setUpInventoryEvents();
        fpc = gameObject.GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        //Stop trying to siphon a terminal if something made it invalid
        if (fpc.enabled == false && (terminalInRange == null || terminalInRange.used == true))
            fpc.enabled = true;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
            TryTerminalSiphon();
            TryToggleDoor();
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            fpc.enabled = true;
        }
        if (Time.time >= terminalSiphonComplete && fpc.enabled == false)
        {
            terminalInRange.used = true;
            ms.energy = Mathf.Min(ms.energy + terminalInRange.energy, ms.maxEnergy);
        }
    }

    private void TryPickup()
    {
        if (itemToPickup != null)
            inventory.AddItem(itemToPickup);
    }

    private void TryTerminalSiphon()
    {
        if (terminalInRange != null && !terminalInRange.used)
        {
            fpc.enabled = false;
            terminalSiphonComplete = Time.time + terminalSiphonTime;
        }
    }

    private void TryToggleDoor()
    {
        if (nearestDoor != null)
        {
            if (nearestDoor.open)
                nearestDoor.CloseDoor();
            else
                nearestDoor.OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInventoryItem item = other.GetComponent<IInventoryItem>();
        if (item != null)
        {
            itemToPickup = item;
        }

        else if (other.gameObject.tag == "Terminal")
        {
            terminalInRange = other.gameObject.GetComponent<Terminal>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInventoryItem item = other.GetComponent<IInventoryItem>();
        if (item != null)
        {
            itemToPickup = null;
        }

        else if (other.gameObject.tag == "Terminal")
        {
            terminalInRange = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractor : MonoBehaviour {

    private Player player;

	// Use this for initialization
	void Start () {
        player = GetComponentInParent<Player>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            player.nearestDoor = other.gameObject.GetComponentInParent<Door>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            player.nearestDoor = null;
        }
    }
}

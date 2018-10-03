using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    private Animator anim;
    public bool open = false;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}

    //Open the door
    public void OpenDoor()
    {
        open = true;
        anim.SetTrigger("DoorOpen");
    }

    //Close the door
    public void CloseDoor()
    {
        open = false;
        anim.SetTrigger("DoorClose");
    }
}

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
        if (!open && anim.GetCurrentAnimatorStateInfo(0).IsName("Door"))
        {
            open = true;
            anim.Play("DoorOpen");
        }
    }

    //Close the door
    public void CloseDoor()
    {
        if (open && anim.GetCurrentAnimatorStateInfo(0).IsName("Door"))
        {
            open = false;
            anim.Play("DoorClose");
        }
    }

    public void CloseDoorWithDelay(float delayTime)
    {
        StartCoroutine(CloseDoorWithDelayEnum(delayTime));
    }

    private IEnumerator CloseDoorWithDelayEnum(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        CloseDoor();
    }
}

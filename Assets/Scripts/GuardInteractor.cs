using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardInteractor : MonoBehaviour {

    public float doorCloseDelayTime = 1.5f;

    //Once more, you open the door
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Door")
        {
            Door door = collider.gameObject.GetComponentInParent<Door>();
            door.OpenDoor();
            door.CloseDoorWithDelay(doorCloseDelayTime);
        }
    }
}

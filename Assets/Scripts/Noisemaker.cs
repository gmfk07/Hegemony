using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noisemaker : MonoBehaviour {

    public float noiseRadius;
    
    public void MakeNoise()
    {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, noiseRadius);
        foreach (Collider col in objectsInRange)
        {
            GuardVision guardVision = col.gameObject.GetComponent<GuardVision>();
            if (guardVision != null)
            {
                guardVision.heardNoise = true;

                if (guardVision.getState() == GuardVision.State.patrol)
                {
                    guardVision.setState(GuardVision.State.investigate);
                    guardVision.targetDestination = transform.position;
                }
            }
        }
    }
}

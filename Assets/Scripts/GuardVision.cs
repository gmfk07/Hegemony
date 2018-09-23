using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardVision : MonoBehaviour {
    
    public float fovAngle = 110f;
    public bool playerInSight;
    public Vector3 lastSighting;
    private GuardPatrol gp;
    private GuardInvestigate gi;

    private SphereCollider col;

    private void Start()
    {
        col = GetComponent<SphereCollider>();
        gp = GetComponent<GuardPatrol>();
        gi = GetComponent<GuardInvestigate>();
    }

    //When the player is within sight range
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInSight = false;
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle < fovAngle * 0.5f)
            {
                RaycastHit hit;
                Debug.DrawRay(transform.position, direction.normalized);
                if (Physics.Raycast(transform.position, direction.normalized, out hit, col.radius))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        playerInSight = true;
                        lastSighting = hit.collider.transform.position;
                        gp.enabled = false;
                        gi.enabled = true;
                        gi.inspectLocation = lastSighting;
                    }
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInSight = false;
        }
    }
}
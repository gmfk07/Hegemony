using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardVision : MonoBehaviour {
    
    public float fovAngle = 110f;
    public bool playerInSight;
    public Vector3 targetDestination;
    private GuardPatrol gp;
    private GuardInvestigate gi;
    public bool heardNoise;
    private State state = State.patrol;

    public enum State { patrol, investigate, chase }

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
                        targetDestination = hit.collider.transform.position;
                        
                        gi.inspectLocation = targetDestination;
                        if (state == State.patrol)
                            setState(State.investigate);
                    }
                }
            }
        }
    }

    public void setState(State inputState)
    {
        state = inputState;
        if (inputState == State.investigate)
        {
            gp.enabled = false;
            gi.enabled = true;
        }
        if (inputState == State.patrol)
        {
            gp.enabled = true;
            gi.enabled = false;
            gp.AssignWaypoint(0);
        }
    }

    public State getState()
    {
        return state;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInSight = false;
        }
    }
}
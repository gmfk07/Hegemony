using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardInvestigate : MonoBehaviour {

    public Vector3 inspectLocation;
    private NavMeshAgent agent;
    public float epsilon;
    private float lookAroundTime;
    private float lastlookAround;
    public bool looking = false;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = inspectLocation;
    }

    //Stop looking if your time is up
    private void Update()
    {
        if (looking && Time.time - lastlookAround >= lookAroundTime)
        {
            looking = false;
            GetComponent<GuardVision>().setState(GuardVision.State.patrol);
        }
    }

    //Start looking around
    void FixedUpdate () {
        agent.destination = inspectLocation;
        if (AtTarget(epsilon) && !looking)
        {
            looking = true;
            lastlookAround = Time.time;
        }
	}

    //Check if the guard has reached the investigation target, with some epsilon
    bool AtTarget(float epsilon)
    {
        Vector3 displacement = inspectLocation - agent.transform.position;
        if (displacement.magnitude < epsilon)
            return true;
        return false;
    }
}

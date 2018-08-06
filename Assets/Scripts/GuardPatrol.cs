using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardPatrol : MonoBehaviour {

    public List<Transform> waypoints;
    public float epsilon;
    private NavMeshAgent agent;
    private Transform target;
    private int index;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        AssignWaypoint(0);
    }

    private void FixedUpdate()
    {
        if (AtTarget(epsilon))
            AssignWaypoint(index + 1);
    }

    bool AtTarget(float epsilon)
    {
        Vector3 displacement = target.position - agent.transform.position;
        if (displacement.magnitude < epsilon)
            return true;
        return false;
    }

    private void AssignWaypoint(int inp)
    {
        if (inp < waypoints.Count)
            index = inp;
        else
            index = 0;
        target = waypoints[index];
        agent.destination = target.position;
    }
}

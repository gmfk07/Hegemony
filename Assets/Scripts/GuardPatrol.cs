using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardPatrol : MonoBehaviour {

    public List<Transform> waypoints;
    public float epsilon;
    public float speed = 3.5f;
    private NavMeshAgent agent;
    private Transform target;
    private int index;

	void OnEnable () {
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

    public void AssignWaypoint(int inp)
    {
        agent.speed = speed;
        if (inp < waypoints.Count)
            index = inp;
        else
            index = 0;
        target = waypoints[index];
        agent.destination = target.position;
    }
}

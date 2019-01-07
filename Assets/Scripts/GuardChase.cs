using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardChase : MonoBehaviour {

    public Vector3 chaseLocation;
    private NavMeshAgent agent;
    public float fireRate;
    public float speed = 4.5f;

    // Use this for initialization
    void OnEnable () {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = chaseLocation;
    }
	
	
}

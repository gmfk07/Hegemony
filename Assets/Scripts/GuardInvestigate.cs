using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardInvestigate : MonoBehaviour {

    public Vector3 inspectLocation;
    private NavMeshAgent agent;
    private float investigateTime;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = inspectLocation;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

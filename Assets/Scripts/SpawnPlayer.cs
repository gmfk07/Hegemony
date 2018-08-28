using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour {

    public GameObject player;
    public GameObject pistol;

	// Use this for initialization
	void Start () {
        //multiplier is how long 1/4 of the platform is
        float multiplier = transform.localScale.x/2;
        Vector3 pos = transform.position + (new Vector3(0.5f * multiplier, .5f, 0.5f * multiplier));
        Instantiate(player, pos, Quaternion.identity);
        Instantiate(pistol, pos, Quaternion.identity);
    }
}

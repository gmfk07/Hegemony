using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour {
    public bool used = false;
    public float energy = 10;
    public Material on;
    public Material off;
    private MeshRenderer mr;

    private void Start()
    {
        mr = gameObject.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (used)
            mr.material = off;
        else
            mr.material = on;
    }
}

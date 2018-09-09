using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour {

    public float damage = 10f;
    public float range = 100f;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        muzzleFlash.Play();

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}

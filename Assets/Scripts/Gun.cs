using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour {

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 4f;
    public float impactForce = 30f;
    public float reloadHoldTime = .5f;
    public float chargeCost = 1;
    public int maxAmmo = 4;

    private bool automatic = true;

    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;
    private float nextFullReload;
    private Camera cam;
    private MissionStats ms;
    public bool reloadPressed = false;
    private bool reloadAchieved = false;
    private Text ammoCounter;
    private Player playerStats;

    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        cam = Camera.main;
        ms = GameObject.Find("Controller").GetComponent<MissionStats>();
        ammoCounter = GameObject.Find("AmmoCounter").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update ()
    {
        ammoCounter.text = "Ammo: " + playerStats.currentPistolAmmo.ToString() + "/" + maxAmmo.ToString();
        if (Input.GetAxis("Reload") > 0 && !reloadPressed)
        {
            reloadPressed = true;
            reloadAchieved = false;
            nextFullReload = Time.time + reloadHoldTime;
        }

        if (Input.GetAxis("Reload") == 0 && reloadPressed)
        {
            reloadPressed = false;
            if (!reloadAchieved)
                ReloadOnce();
        }

        if (Time.time >= nextFullReload && reloadPressed && !reloadAchieved)
        {
            ReloadAll();
            reloadAchieved = true;
        }

        if (playerStats.currentPistolAmmo <= 0)
            return;

        bool isShooting = GetIsShooting();

        if (isShooting && Time.time > nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void ReloadOnce()
    {
        if (playerStats.currentPistolAmmo < maxAmmo && ms.energy >= chargeCost)
        {
            ms.energy -= chargeCost;
            playerStats.currentPistolAmmo++;
        }
    }

    void ReloadAll()
    {
        int AmtToReload = maxAmmo - playerStats.currentPistolAmmo;
        if (AmtToReload != 0 && AmtToReload*chargeCost <= ms.energy)
        {
            ms.energy -= AmtToReload*chargeCost;
            playerStats.currentPistolAmmo = maxAmmo;
        }
    }

    private bool GetIsShooting()
    {
        bool isShooting;
        if (automatic)
            isShooting = Input.GetButton("Fire1");
        else
            isShooting = Input.GetButtonDown("Fire1");
        return isShooting;
    }

    void Shoot()
    {
        playerStats.currentPistolAmmo--;
        muzzleFlash.Play();

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f);
        }
    }
}

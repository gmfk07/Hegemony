using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionStats : MonoBehaviour {

    public float energy = 100f;
    public float maxEnergy = 100f;
    public float alarm = 0f;
    public float maxAlarm = 100f;

    private Text statusCounter;

    private void Start()
    {
        statusCounter = GameObject.Find("StatusCounter").GetComponent<Text>();
    }

    private void Update()
    {
        statusCounter.text = energy.ToString() + "/" + maxEnergy.ToString() + " kW\nAlarm at " + alarm.ToString() + "%";
    }

}

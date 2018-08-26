using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, IInventoryItem {

    public string _Name = null;
    public string Name
    {
        get
        {
            return _Name;
        }
    }

    public Sprite _Image = null;
    public Sprite Image
    {
        get
        {
            return _Image;
        }
    }

    public GameObject _Equipment = null;
    public GameObject Equipment
    {
        get
        {
            return _Equipment;
        }
    }
    private GameObject created;

    public void OnPickup()
    {
        gameObject.SetActive(false);
    }

    public void OnSelected()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform parent = player.transform.GetChild(0);
        created = Instantiate(Equipment, parent);
    }

    public void OnDeselected()
    {
        Destroy(created);
    }
}

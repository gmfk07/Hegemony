﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryItem
{
    string Name { get; }

    Sprite Image { get; }

    GameObject Equipment { get; }

    void OnPickup();
    void OnSelected();
    void OnDeselected();
}

public class InventoryEventArgs : System.EventArgs
{
    public IInventoryItem Item;

    public InventoryEventArgs(IInventoryItem item)
    {
        Item = item;
    }
}

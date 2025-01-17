﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class Item : ScriptableObject {
    new public string name = "New Item";
    public Sprite icon = null;
}

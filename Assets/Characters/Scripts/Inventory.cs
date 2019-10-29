using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Inventory : MonoBehaviour {

    #region Singleton

    public static Inventory instance;

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("More than one instance of Inventory found.");
            return;
        }

        instance = this;
    }

    #endregion

    public delegate void onItemChanged();
    public onItemChanged onItemChangedCallback;

    public int space = 16;

    public List<Item> items = new List<Item>();

    public void Add(Item item) {
        if (items.Count >= space) {
            Debug.Log("Not enough room.");
        }
        items.Add(item);

        if (onItemChangedCallback != null) {
            onItemChangedCallback.Invoke();
        }
    }

    public void Remove(Item item) {
        items.Remove(item);

        if (onItemChangedCallback != null) {
            onItemChangedCallback.Invoke();
        }
    }
}

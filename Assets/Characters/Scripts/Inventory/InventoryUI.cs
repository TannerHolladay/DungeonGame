using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {
	public Transform itemsParent;

	Inventory inventory;

	InventorySlot[] slots; 
	GameObject inventoryUI;
	

	private bool inventoryEnabled;

	void Start() {
		inventoryUI = this.gameObject.transform.Find("Inventory").gameObject;

		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;

		slots = itemsParent.GetComponentsInChildren<InventorySlot>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.E)) {
			inventoryEnabled = !inventoryEnabled;
			inventoryUI.SetActive(inventoryEnabled);
		}
	}

	void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (i < inventory.items.Count) {
				slots[i].AddItem(inventory.items[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}

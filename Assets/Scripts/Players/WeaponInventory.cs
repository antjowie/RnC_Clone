using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInventory : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        public GameObject weaponPrefab;
        public GameObject slot;
        internal Text text;
    }

    public Item[] items;

    GameObject GetWeapon(int index)
    {
        return items[index].weaponPrefab;
    }

    void SetWeapon(int index, GameObject weapon, string name = "")
    {
        items[index].weaponPrefab = weapon;
        if (name.Length == 0)
            name = weapon.name;
        items[index].text.text = name;
    }

    // Start is called before the first frame update
    void Awake()
    {
        // TODO: This doesn't take name into consideration
        for (int i = 0; i < items.Length; i++)
        {
            items[i].text = items[i].slot.GetComponentInChildren<Text>();
            if (items[i].weaponPrefab)
                SetWeapon(i, items[i].weaponPrefab);
        }
    }
}

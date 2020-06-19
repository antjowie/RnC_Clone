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

    public GameObject GetWeapon(int index)
    {
        return items[index].weaponPrefab;
    }

    public void SetWeapon(int index, GameObject weapon, string name = "")
    {
        items[index].weaponPrefab = weapon;
        if (name.Length == 0)
            name = weapon.name;
        items[index].text.text = name;
    }

    private void Awake()
    {
        foreach(Item item in items)
        {
            item.text = item.slot.GetComponentInChildren<Text>();
        }
    }
}

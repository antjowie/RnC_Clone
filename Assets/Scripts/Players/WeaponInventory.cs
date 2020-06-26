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
        internal Image image;
    }

    public float fadeTime = 0.2f;
    public GameObject inventoryUI;
    public Item[] items;
    public int hovered;

    CanvasGroup group;
    Color origColor;
    Color highlightColor = Color.white;

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine("FadeIn");
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeIn()
    {
        while(group.alpha < 1f)
        {
            group.alpha = Mathf.Min(1f, group.alpha + Time.deltaTime / fadeTime);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        while(group.alpha > 0f)
        {
            group.alpha = Mathf.Max(0f, group.alpha - Time.deltaTime / fadeTime);
            yield return null;
        }
        //inventoryUI.SetActive(false);
    }

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

    public void Focus(int index)
    {
        hovered = index;
        //Unhover each item
        foreach (Item item in items)
        {
            item.image.color = origColor;   
        }
        items[index].image.color = highlightColor;
    }

    private void Awake()
    {
        group = inventoryUI.GetComponent<CanvasGroup>();
        group.alpha = 0;
        origColor = items[0].slot.GetComponent<Image>().color;
        foreach (Item item in items)
        {
            item.text = item.slot.GetComponentInChildren<Text>();
            item.image = item.slot.GetComponent<Image>();
        }
    }
}

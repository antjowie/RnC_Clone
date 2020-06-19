using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class WeaponSelect : MonoBehaviour
{
    public GameObject UIObject;
    public PlayerController player;
    public WeaponInventory inventory;

    CinemachineFreeLook playerCam;

    string origX;
    string origY;

    // Start is called before the first frame update
    void Start()
    {
        playerCam = player.playerCamera;

        UIObject.SetActive(false);

        origX = playerCam.m_XAxis.m_InputAxisName;
        origY = playerCam.m_YAxis.m_InputAxisName;


        // Set the weapons of the inventory
        var items = inventory.items;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].weaponPrefab)
                inventory.SetWeapon(i, items[i].weaponPrefab, string.Format("triple shot {0}",i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check user input
        if(Input.GetKey(KeyCode.E))
        {
            UIObject.SetActive(true);

            playerCam.m_XAxis.m_InputAxisName = "";
            playerCam.m_YAxis.m_InputAxisName = "";
            playerCam.m_XAxis.m_InputAxisValue = 0;
            playerCam.m_YAxis.m_InputAxisValue = 0;
        }
        else
        {
            UIObject.SetActive(false);

            playerCam.m_XAxis.m_InputAxisName = origX;
            playerCam.m_YAxis.m_InputAxisName = origY;
        }

        GameObject toCreate = null;
        Color color = Color.magenta;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            toCreate = inventory.GetWeapon(0);
            color = Color.white;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            toCreate = inventory.GetWeapon(1);
            color = Color.red;
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            toCreate = inventory.GetWeapon(2);
            color = Color.green;    
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            toCreate = inventory.GetWeapon(3);
            color = Color.blue;
        }

        if(toCreate)
        {
            DestroyImmediate(player.weaponPrefab);

            player.weaponPrefab = Instantiate(toCreate, player.weaponPoint);
            player.weaponPrefab.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", color);
            player.weaponBehavior = player.weaponPrefab.GetComponent<IWeapon>();
            player.weaponBehavior.playerOrientation = player.transform.gameObject;
        }
    }
}

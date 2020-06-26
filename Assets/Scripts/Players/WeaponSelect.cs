using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;

public class WeaponSelect : MonoBehaviour
{
    public PlayerController player;
    public WeaponInventory inventory;

    CinemachineFreeLook playerCam;

    string origX;
    string origY;

    Vector2 dir;

    InputAction inventoryAction = new InputAction();

    // Start is called before the first frame update
    void Start()
    {
        playerCam = player.playerCamera;

        origX = playerCam.m_XAxis.m_InputAxisName;
        origY = playerCam.m_YAxis.m_InputAxisName;


        // Set the weapons of the inventory
        var items = inventory.items;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].weaponPrefab)
                inventory.SetWeapon(i, items[i].weaponPrefab);
        }
    }

    void ResetDir()
    {
        dir = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Check user input
        inventoryAction.Update("Inventory");
        GameObject toCreate = null;

        if (inventoryAction.Down())
        {
            inventory.Show();

            playerCam.m_XAxis.m_InputAxisName = "";
            playerCam.m_YAxis.m_InputAxisName = "";
            playerCam.m_XAxis.m_InputAxisValue = 0;
            playerCam.m_YAxis.m_InputAxisValue = 0;
        }
        else if (inventoryAction.Up())
        {
            inventory.Hide();

            playerCam.m_XAxis.m_InputAxisName = origX;
            playerCam.m_YAxis.m_InputAxisName = origY;
            ResetDir();

            toCreate = inventory.GetWeapon(inventory.hovered);
        }

        if(inventoryAction)
        {
            // Calculate angle [0,360] to select weapons and convert it to weapon slot
            var move = new Vector2(Input.GetAxis(origX), Input.GetAxis(origY));
                        
            int slotCount = inventory.items.Length;
            float slotAngle = 360f / slotCount;

            Invoke("ResetDir", 0.01f);
            //ResetDir();
            if (move.magnitude != 0f)
            {
                CancelInvoke("ResetDir");
                dir += move;

                // We should probably offset the angle but I will polish this in the future
                var normal = dir.normalized;
                var angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f - slotAngle / 2f;
                if (angle < 0f) angle += 360f;
                angle = 360f - angle;
                //print(angle);

                for (int i = 0; i < slotCount; i++)
                {
                    angle -= slotAngle;
                    if (angle < 0f)
                    {
                        inventory.Focus(i);
                        break;
                    }
                }
            }
        }

        if (toCreate)
        {
            // Temp: testing purposes
            Color color = Color.magenta;
            switch (inventory.hovered)
            {
                case 0:
                    color = Color.white;
                    break;
                case 1:
                    color = Color.red;
                    break;
                case 2:
                    color = Color.green;
                    break;
                case 3:
                    color = Color.blue;
                    break;
            }

            // Do the weapon swap
            DestroyImmediate(player.weaponPrefab);

            player.weaponPrefab = Instantiate(toCreate, player.weaponPoint);
            player.weaponPrefab.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", color);
            player.weaponBehavior = player.weaponPrefab.GetComponent<IWeapon>();
            player.weaponBehavior.playerOrientation = player.transform.gameObject;
        }
    }
}

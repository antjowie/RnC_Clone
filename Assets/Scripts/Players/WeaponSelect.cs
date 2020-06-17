using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelect : MonoBehaviour
{
    public GameObject UIObject;
    public Cinemachine.CinemachineFreeLook playerCam;

    string origX;
    string origY;

    // Start is called before the first frame update
    void Start()
    {
        UIObject.SetActive(false);

        origX = playerCam.m_XAxis.m_InputAxisName;
        origY = playerCam.m_YAxis.m_InputAxisName;
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
    }
}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2000f;
    public float rotationsPerSecond = 360f;
    public CinemachineVirtualCameraBase playerCamera;


    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 desiredMovement = Vector3.Normalize(new Vector3(x, 0, y));

        if(desiredMovement != Vector3.zero)
        {
            // Get camera rotation
            var desiredForward = playerCamera.LookAt.position - playerCamera.transform.position;
            desiredForward.y = 0;
            desiredForward.Normalize();
            Quaternion camRotation = Quaternion.LookRotation(desiredForward);

            // Calculate the target rotation of the character
            RotateTowards(camRotation * desiredMovement);

            // Only apply movement if we are looking in the right direction
            if(Vector3.Dot(desiredMovement,rb.transform.forward) > 0f)
                rb.AddForce(desiredMovement * moveSpeed * Time.deltaTime);
        }
    }

    void RotateTowards(Vector3 forward)
    {        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(forward), rotationsPerSecond * Time.deltaTime);
    }
}

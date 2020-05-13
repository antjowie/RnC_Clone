using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 100;
    public CinemachineVirtualCameraBase playerCamera;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Calculate intended movement via the player view direction
        var forward = playerCamera.LookAt.position - playerCamera.transform.position;
        forward.y = 0;
        forward.Normalize();
        var right = Vector3.Cross(transform.up, forward);

        rb.AddForce(forward * y * moveSpeed * Time.deltaTime);
        rb.AddForce(right * x * moveSpeed * Time.deltaTime);

        // Should be force based
        //rb.MovePosition(rb.transform.position + new Vector3(x, 0, y) * Time.deltaTime * moveSpeed);
    }
}

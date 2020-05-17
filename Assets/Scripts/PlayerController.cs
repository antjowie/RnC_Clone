using Cinemachine;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraPoint;

    public CinemachineVirtualCameraBase playerCamera;
    public float movespeed = 1000f;
    public float moveDampTime = 0.1f;

    Vector2 input;
    Rigidbody rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Horizontal", input.x, moveDampTime, Time.deltaTime);
        anim.SetFloat("Vertical", input.y, moveDampTime, Time.deltaTime);
        input.Normalize();

        cameraPoint.transform.position = rb.transform.position;

        Rotate();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Rotate()
    {
        // Get new movement direction
        var moveDir = playerCamera.LookAt.position - playerCamera.transform.position;
        moveDir.y = 0; moveDir.Normalize();

        transform.rotation = Quaternion.LookRotation(moveDir);
    }

    void Movement()
    {
        rb.velocity = rb.rotation * new Vector3(input.x, 0, input.y) * movespeed * Time.deltaTime;
    }
}
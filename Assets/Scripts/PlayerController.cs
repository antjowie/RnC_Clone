using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2000f;
    public float rotationsPerSecond = 360f;
    public CinemachineVirtualCameraBase playerCamera;

    public float jumpForce = 100f;
    public float lowJumpMultiplier = 1.5f;
    public float dropMultiplier = 2f;
    public float doubleJumpImpulseMultiplier = 0.5f;

    bool isOnGround = true;
    bool hasDoubleJumped = false;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Movement();
        Jump();
    }

    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 input = Vector3.Normalize(new Vector3(x, 0, y));

        if(input != Vector3.zero)
        {
            // Get camera rotation
            var camForward = playerCamera.LookAt.position - playerCamera.transform.position;
            camForward.y = 0;
            camForward.Normalize();
            Quaternion camRotation = Quaternion.LookRotation(camForward);

            // Calculate the target rotation of the character
            var desiredForward = camRotation * input;
            RotateTowards(desiredForward);

            // Only apply movement if we are looking in the right direction
            if (Vector3.Dot(desiredForward, rb.transform.forward) > 0f)
            {
                rb.AddForce(rb.transform.forward * moveSpeed * Time.deltaTime);
            }
        }
    }

    void RotateTowards(Vector3 forward)
    {        
        rb.transform.rotation = Quaternion.RotateTowards(rb.transform.rotation, Quaternion.LookRotation(forward), rotationsPerSecond * Time.deltaTime);
    }

    // Used to treat get axis as a down function
    bool jumpWasPressed = false;

    void Jump()
    {
        // Handle whether jumping was pressed
        bool jumping = (Input.GetAxisRaw("Jump") == 1);
        bool jumpDown = !jumpWasPressed && jumping;
        jumpWasPressed = jumping;

        if(isOnGround)
        {
            hasDoubleJumped = false;

            if (jumpDown)
            {
                rb.AddForce(rb.transform.up * jumpForce, ForceMode.Impulse);
            }
        }
        // Is in air
        else
        {
            // When in low jump (going up)
            if(rb.velocity.y > 0f)
            {
                if(jumpDown && !hasDoubleJumped)
                {
                    rb.AddForce(rb.transform.up * jumpForce * doubleJumpImpulseMultiplier, ForceMode.Impulse);
                    hasDoubleJumped = true;
                }   
                if(jumping)
                {
                    rb.AddForce(-Physics.gravity * lowJumpMultiplier * Time.deltaTime);
                }
            }
            // When dropping    
            else
            {
                rb.AddForce(Physics.gravity * dropMultiplier * Time.deltaTime);
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        foreach(ContactPoint point in other.contacts)
        {
            // TODO: When implementing walljump, we want to change this probably
            if(Vector3.Dot(Vector3.up,point.normal) > 0.5f)
            {
                isOnGround = true;
                return;
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        foreach (ContactPoint point in other.contacts)
        {
            if (Vector3.Dot(Vector3.up, point.normal) > 0.5f)
            {
                isOnGround = false;
                return;
            }
        }
    }
}
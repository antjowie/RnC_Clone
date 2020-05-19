using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Transform cameraPoint;

    public CinemachineVirtualCameraBase playerCamera;
    public float movespeed = 1000f;
    public float moveDampTime = 0.1f;

    public float gravity = -20f;
    public float jumpHeigth = 10f;
    public float lowJumpModifier = 1.5f;
    public float fallingModifier = 2f;

    Vector2 input;
    float yVelocity = 0f;
    bool jumpingPressed = false;
    bool jumpingDown = false;
    bool onGround = false;

    Rigidbody rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Calculate movement input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        //anim.SetFloat("Horizontal", input.x, moveDampTime, Time.deltaTime);
        //anim.SetFloat("Vertical", input.y, moveDampTime, Time.deltaTime);
        input.Normalize();

        // Calculate if player pressed jump once
        bool oldState = jumpingPressed;
        jumpingDown = false;
        jumpingPressed = Input.GetAxisRaw("Jump") == 1;
        if (jumpingPressed && !oldState)
            jumpingDown = true;

        OnJump();


        // TODO: Only change Y if player is falling/going up
        //var planePos = rb.transform.position;
        //planePos.y = cameraPoint.transform.position.y;
        //cameraPoint.transform.position = planePos;
        cameraPoint.transform.position = rb.transform.position;

        anim.SetBool("Aiming", Input.GetMouseButton((int)MouseButton.RightMouse));
        anim.SetBool("OnGround", onGround);
        Rotate();

        // We want to know the actual executed movement for the animations so we inverse the rotation
        var moveDir = rb.velocity;
        moveDir.y = 0; 
        moveDir = Quaternion.Inverse(rb.rotation) * moveDir;
        moveDir.x = Mathf.Clamp(moveDir.x, -1, 1);
        moveDir.z = Mathf.Clamp(moveDir.z, -1, 1);
        moveDir.Scale(new Vector3(input.x, 0, input.y).Abs()); // We scale animation with out input
        anim.SetFloat("Horizontal", moveDir.x, moveDampTime, Time.deltaTime);
        anim.SetFloat("Vertical", moveDir.z, moveDampTime, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        UpdateGravity();
        Movement();
    }

    void OnJump()
    {
        if(jumpingDown && onGround)
        {
            //https://www.youtube.com/watch?v=v1V3T5BPd7E&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W
            yVelocity = Mathf.Sqrt(-2 * gravity * jumpHeigth);
        }
    }

    void UpdateGravity()
    {
        yVelocity += gravity * Time.deltaTime;

        // Modify the gravity based on player state
        if (!onGround)
        {
            bool falling = yVelocity < 0f;

            if(!falling)
            {
                if (!jumpingPressed)
                    yVelocity += gravity * (lowJumpModifier - 1f) * Time.deltaTime;
            }
            else
            {
                yVelocity += gravity * (fallingModifier - 1f) * Time.deltaTime;
            }
        }
        else
        {
            // Make the player fall to the ground constantly to ensure correct behavior
            // TODO: Verify if this doesn't break slopes
            if(yVelocity < 0f)
                yVelocity = -0.1f;
        }
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
        rb.velocity = new Vector3(rb.velocity.x, yVelocity, rb.velocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        //var contacts = new ContactPoint[collision.contactCount];
        //collision.GetContacts(contacts);
        //
        //foreach (var contact in contacts)
        //{
        //    Debug.DrawRay(contact.point, contact.normal * 10f, Color.red);
        //}
    }

    void OnCollisionStay(Collision collision)
    {
        var contacts = new ContactPoint[collision.contactCount];
        collision.GetContacts(contacts);
        
        foreach (var contact in contacts)
        {
            if(Vector3.Dot(contact.normal,rb.transform.up) > 0.5f)
            {
                onGround = true;
                return;
            }
        }

    }

    void OnCollisionExit(Collision collision)
    {
        // OnCollisionExit does not contain normal data
        onGround = false;

        //var contacts = new ContactPoint[collision.contactCount];
        //collision.GetContacts(contacts);
        //foreach (var contact in contacts)
        //{
        //    Debug.DrawRay(contact.point, contact.normal * 10f, Color.blue,5f);
        //}
    }

}
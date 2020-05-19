using Cinemachine;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Transform cameraPoint;

    public CinemachineVirtualCameraBase playerCamera;
    public float movespeed = 1000f;
    public float moveDampTime = 0.1f;

    public float jumpForce = 10f;
    //public float lowJumpModifier = 1.5f;
    //public float fallingModifier = 2f;

    Vector2 input;
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

        anim.SetFloat("Horizontal", input.x, moveDampTime, Time.deltaTime);
        anim.SetFloat("Vertical", input.y, moveDampTime, Time.deltaTime);
        input.Normalize();

        // Calculate if player pressed jump once
        bool oldState = jumpingPressed;
        jumpingDown = false;
        jumpingPressed = Input.GetAxisRaw("Jump") == 1;
        if (jumpingPressed && !oldState)
            jumpingDown = true;

        Jump();

        cameraPoint.transform.position = rb.transform.position;
        anim.SetBool("Aiming", Input.GetMouseButton((int)MouseButton.RightMouse));
        Rotate();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Jump()
    {
        if(jumpingDown && onGround)
            rb.AddForce(rb.transform.up * jumpForce, ForceMode.VelocityChange);
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
        float oldY = rb.velocity.y;
        rb.velocity = rb.rotation * new Vector3(input.x, 0, input.y) * movespeed * Time.deltaTime;
        rb.velocity = rb.velocity + new Vector3(0, oldY, 0);
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
using Cinemachine;
using Cinemachine.Utility;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraPoint;

    [Header("Movement")]
    public CinemachineVirtualCameraBase playerCamera;
    public float movespeed = 1000f;
    public float moveDampTime = 0.1f;

    [Header("Jumping")]
    public float gravity = -20f;
    public float jumpHeigth = 10f;
    public float lowJumpModifier = 1.5f;
    public float fallingModifier = 2f;

    [Header("Walljump")]
    public Vector2 wallJumpForce = new Vector2(100f,200f);
    public float wallJumpForceDuration = 0.5f;
    Vector3 lastWallNormal;

    [Header("Combat")]
    public GameObject weaponPrefab;
    public Transform weaponPoint;

    [Header("READ ONLY Inpsectables")]
    public bool onGround = true;
    public bool onWall = false;

    // Components
    Rigidbody rb;
    Animator anim;

    // Movement
    Vector2 input;
    Quaternion desRot;
    Vector3 desVel;
    Vector3 extVel;

    float yVelocity = 0f;
    bool jumpingPressed = false;
    bool jumpingDown = false;

    struct Force
    {
        public Vector3 vel;
        public float cancelTime;
        public float cancelTimer;
    }
    Force desForce;
    Force extForce;

    // Animations
    float blendWeigth = 0f;
    float blendTime = 0.2f;

    public void ApplyForce(Vector3 force, float cancelTime, bool desired = false)
    {
        if(desired)
        {
            desForce.vel = force;
            desForce.cancelTime = 0;
            desForce.cancelTimer = cancelTime;
        }
        else
        {
            extForce.vel = force;
            extForce.cancelTime = 0;
            extForce.cancelTimer = cancelTime;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        weaponPrefab = Instantiate(weaponPrefab, weaponPoint);
        weaponPrefab.transform.localPosition = Vector3.zero;
        weaponPrefab.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        // Calculate movement input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();

        // Calculate if player pressed jump once
        bool oldState = jumpingPressed;
        jumpingDown = false;
        jumpingPressed = Input.GetAxisRaw("Jump") == 1;
        if (jumpingPressed && !oldState)
            jumpingDown = true;

        if (jumpingDown)
            OnJump();

        // Update variables
        {
            // TODO: Only change Y if player is falling/going up
            //var planePos = rb.transform.position;
            //planePos.y = cameraPoint.transform.position.y;
            //cameraPoint.transform.position = planePos;
            cameraPoint.transform.position = rb.transform.position;
            
            var camForward = playerCamera.LookAt.position - playerCamera.transform.position;
            camForward.y = 0; camForward.Normalize();
            desRot = Quaternion.LookRotation(camForward);
        }

        WeaponUpdate();
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
        anim.SetBool("OnGround", onGround);
    }

    // This function calculates the velocity that a force applies
    private Vector3 UpdateForce(ref Force force)
    {
        if (force.cancelTime < force.cancelTimer)
        {
            force.cancelTime += Time.deltaTime;
            Vector3 offset = force.vel * (1f - force.cancelTime / force.cancelTimer);

            return offset * Time.deltaTime;
        }
        else
        {
            force.vel = Vector3.zero;
            force.cancelTime = force.cancelTimer = 0f;
        }
        return Vector3.zero;
    }


    private void FixedUpdate()
    {
        UpdateGravity();
        desVel = UpdateForce(ref desForce);
        extVel = UpdateForce(ref extForce);

        // Apply user input to des vel
        desVel += desRot * new Vector3(input.x, 0, input.y) * movespeed * Time.deltaTime;
        desVel.y += yVelocity;

        rb.velocity = desVel + extVel;
    }

    private void WeaponUpdate()
    {
        if (Input.GetMouseButton((int)MouseButton.RightMouse))
        {
            weaponPrefab.SetActive(true);
            blendWeigth = Mathf.Lerp(blendWeigth, 1.0f, Time.deltaTime / blendTime);
        }
        else
        {
            weaponPrefab.SetActive(false);
            blendWeigth = Mathf.Lerp(blendWeigth, 0f, Time.deltaTime / blendTime);
        }
        anim.SetLayerWeight(anim.GetLayerIndex("Weapon"), blendWeigth);

    }

    private void OnJump()
    {
        if(jumpingDown && onGround)
        {
            //https://www.youtube.com/watch?v=v1V3T5BPd7E&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W
            yVelocity = Mathf.Sqrt(-2 * gravity * jumpHeigth);
        }

        // Walljump
        if(jumpingDown && !onGround && onWall)
        {
            Vector3 force = lastWallNormal * wallJumpForce.x;
            force.y = wallJumpForce.y;
            ApplyForce(force, wallJumpForceDuration);
            yVelocity = 0;
        }
    }

    private void UpdateGravity()
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

    private void Rotate()
    {
        // The rotation depends on the des direction as well as the intended force direction (walljumping)
        // We should calculate the immidiate velocity

    }

    private void OnCollisionEnter(Collision collision)
    {
        //var contacts = new ContactPoint[collision.contactCount];
        //collision.GetContacts(contacts);
        //
        //foreach (var contact in contacts)
        //{
        //    Debug.DrawRay(contact.point, contact.normal * 10f, Color.red);
        //}
    }

    private void OnCollisionStay(Collision collision)
    {
        var contacts = new ContactPoint[collision.contactCount];
        collision.GetContacts(contacts);
        
        foreach (var contact in contacts)
        {
            float dot = Vector3.Dot(contact.normal, rb.transform.up);
            if (dot > 0.5f)
            {
                onGround = true;
            }

            if(dot > -0.1f && dot < 0.1f)
            {
                lastWallNormal = contact.normal;
                onWall = true;
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        // OnCollisionExit does not contain normal data
        onGround = false;
        onWall = false;

        //var contacts = new ContactPoint[collision.contactCount];
        //collision.GetContacts(contacts);
        //foreach (var contact in contacts)
        //{
        //    Debug.DrawRay(contact.point, contact.normal * 10f, Color.blue,5f);
        //}
    }
}
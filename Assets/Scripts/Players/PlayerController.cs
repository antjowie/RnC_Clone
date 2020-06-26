using Cinemachine;
using Cinemachine.Utility;
using System;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Animations.Rigging;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraPoint;
    public CinemachineFreeLook playerCamera;
    bool cameraYShouldMove = false;
    float camYOffset = 0f;

    [Header("Movement")]
    public float movespeed = 1000f;
    public float moveDampTime = 0.1f;
    public float rotateRate = 4f;
    public float horizontalRotateRate = 0.15f;
    Vector2 input;
    Quaternion camRot;
    Vector3 desVel;
    Vector3 extVel;
    bool strafing = false;

    [Header("Jumping")]
    public float gravity = -20f;
    public float jumpHeigth = 10f;
    public float lowJumpModifier = 1.5f;
    public float fallingModifier = 2f;

    [Header("Walljump")]
    public Vector2 wallJumpForce = new Vector2(100f,200f);
    public float wallJumpForceDuration = 0.5f;
    public float maxGravStep = 1f;
    public float gravityModifier = 1f;
    Vector3 lastWallNormal;
    float yVelocity = 0f;
    bool isWalljumping = false;
    float xRecTarget;
    Cinemachine.AxisState.Recentering xRec;

    [Header("Combat")]
    public Rig aimRig;
    public GameObject weaponPrefab;
    public Transform weaponPoint;
    bool isWeaponStocked = true;
    
    // Weapon is set in weaponSelect
    public IWeapon weaponBehavior;

    [Header("READ ONLY Inpsectables")]
    public bool onGround = true;
    public bool onWall = false;

    // Components
    Rigidbody rb;
    Animator anim;

    // Input
    InputAction weaponKeyAction = new InputAction();
    InputAction jumping = new InputAction();

    struct Force
    {
        public Vector3 vel;
        public float cancelTime;
        public float cancelTimer;
    }
    Force desForce;
    Force extForce;

    // Animations
    float weaponBlendWeigth = 0f;
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
        weaponBehavior = weaponPrefab.GetComponent<IWeapon>();
        weaponBehavior.playerOrientation = transform.gameObject;

        xRec = new AxisState.Recentering(true, 0f, 0.2f);
    }

    private void Start()
    {
        // For walljumping purposes. TODO: This breaks controller behavior maybe
        playerCamera.m_YAxisRecentering.m_RecenteringTime = 0.2f;
        //playerCamera.m_RecenterToTargetHeading.m_RecenteringTime = 0.2f;
    }

    private void Update()
    {
        // Update key actions
        jumping.Update("Jump");
        weaponKeyAction.Update("Fire1");

        // Calculate movement input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();

        if (jumping.Down())
            OnJump();

        // Update camRot
        {
            var camForward = playerCamera.LookAt.position - playerCamera.transform.position;
            camForward.y = 0; camForward.Normalize();
            camRot = Quaternion.LookRotation(camForward);
        }

        // Restore gravity modifier to normal value
        {
            var gravStep = Mathf.Clamp(1f - gravityModifier,-maxGravStep,maxGravStep);
            gravityModifier += (gravStep * Time.deltaTime);
        }

        UpdateWalljumping();
        UpdateWalkingOrientation();
        UpdateWeapon();
        UpdateAnimation();
        UpdateCameraY();
    }

    private void FixedUpdate()
    {
        UpdateGravity();
        desVel = UpdateForce(ref desForce);
        extVel = UpdateForce(ref extForce);

        // Apply user input to des vel
        desVel += camRot * new Vector3(input.x, 0, input.y) * movespeed * Time.deltaTime;
        desVel.y += yVelocity;

        // Rotate camera based on horizontal movement relative to camera
        // We rotate it before setting the velocity since we rely on Unity physics for collisions.
        if(!strafing)
        {
            var horizontal = (Quaternion.Inverse(camRot) * rb.velocity).x;// (Quaternion.Inverse(playerCamera.transform.rotation) * rb.velocity).x;
            horizontal = horizontal / (movespeed * Time.deltaTime);
            horizontal *= 360f * horizontalRotateRate;

            playerCamera.m_XAxis.Value += horizontal * Time.deltaTime;
        }

        rb.velocity = desVel + extVel;
    }

    void UpdateWalkingOrientation()
    {
        if(strafing)
        {
            // Rotate towards camera dir
            rb.rotation = Quaternion.RotateTowards(
                rb.rotation,
                camRot,
                360f * rotateRate * Time.deltaTime);

        }
        else
        {
            // Update rotation
            var desMove = desVel; desMove.y = 0;
            // We only rotate if we move
            if (desMove != Vector3.zero)
            {
                rb.rotation = Quaternion.RotateTowards(
                    rb.rotation, 
                    Quaternion.LookRotation(desMove.normalized, Vector3.up), 
                    360f * rotateRate * Time.deltaTime);
            }
        }
    }

    void UpdateAnimation()
    {
        // The rotation depends on the des direction as well as the intended force direction (walljumping)
        // We should calculate the immidiate velocity

        // Update animation
        var movement = new Vector3(input.x, 0, input.y);
        movement *= rb.velocity.magnitude / (movespeed * Time.deltaTime);

        var desMove = desVel; desMove.y = 0;
        var dot = Vector3.Dot(rb.rotation * Vector3.forward, desMove.normalized);
        anim.SetBool("OnGround", onGround);

        strafing = Input.GetAxisRaw("Strafe") == 1;
        anim.SetBool("Strafing", strafing);
        if(strafing)
        {
            anim.SetFloat("Vertical", input.y, moveDampTime, Time.deltaTime);
            anim.SetFloat("Horizontal", input.x, moveDampTime, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Vertical", movement.magnitude * Mathf.CeilToInt(dot), moveDampTime, Time.deltaTime);
        }
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

    private void UpdateWeapon()
    {
        // TODO: Add some kind of timer here or whatever makes sense
        // RnC does it that when u use ur melee weapon will be stocked be we have no melee yet
        isWeaponStocked = !weaponKeyAction;

        // Show or hide weapon
        if (!isWeaponStocked)
        {
            weaponPrefab.SetActive(true);
            weaponBlendWeigth = Mathf.Lerp(weaponBlendWeigth, 1.0f, Time.deltaTime / blendTime);
        }
        else 
        {
            weaponPrefab.SetActive(false);
            weaponBlendWeigth = Mathf.Lerp(weaponBlendWeigth, 0f, Time.deltaTime / blendTime);
        }
        //anim.SetLayerWeight(anim.GetLayerIndex("Weapon"), weaponBlendWeigth);
        aimRig.weight = weaponBlendWeigth;

        // Fire behavior
        if (weaponKeyAction.Down()) weaponBehavior.ShootDown();
        if (weaponKeyAction.Held()) weaponBehavior.Shoot();
        if (weaponKeyAction.Released()) weaponBehavior.ShootRelease();
    }

    float curCamYVel = 0f;
    void UpdateCameraY()
    {
        var camTargetPos = rb.position;
        camTargetPos.y = cameraPoint.position.y;

        if (camTargetPos.y > rb.position.y)
            cameraYShouldMove = true;

        if (cameraYShouldMove)
        {
            camTargetPos.y = Mathf.SmoothDamp(camTargetPos.y, rb.transform.position.y + camYOffset, ref curCamYVel, 0.2f);
        }

        cameraPoint.transform.position = camTargetPos;
    }

    private void OnJump()
    {
        if(jumping.Down() && onGround)
        {
            //https://www.youtube.com/watch?v=v1V3T5BPd7E&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W
            yVelocity = Mathf.Sqrt(-2 * gravity * jumpHeigth);

            cameraYShouldMove = false;
        }

        // Walljump
        if(jumping.Down() && !onGround && onWall)
        {
            // If this is our first walljump
            if (!isWalljumping)
                SetIsWalljumping(true);

            // Walljump response
            Vector3 force = lastWallNormal * wallJumpForce.x;
            force.y = wallJumpForce.y;
            ApplyForce(force, wallJumpForceDuration,true);
            gravityModifier = 0.5f;
            yVelocity = 0;
        }
    }

    private void UpdateGravity()
    {
        yVelocity += gravity * gravityModifier * Time.deltaTime;

        // Modify the gravity based on player state
        if (!onGround)
        {
            bool falling = yVelocity < 0f;

            if(!falling)
            {
                if (!jumping)
                    yVelocity += gravity * gravityModifier * (lowJumpModifier - 1f) * Time.deltaTime;
            }
            else
            {
                yVelocity += gravity * gravityModifier * (fallingModifier - 1f) * Time.deltaTime;
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
            // Ground hit
            if (dot > 0.5f && !onGround)
            {
                onGround = true;
                cameraYShouldMove = true;
                camYOffset = 0;

                SetIsWalljumping(false);
            }

            // Wall hit
            if (dot > -0.1f && dot < 0.1f)
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

    void SetIsWalljumping(bool value)
    {
        isWalljumping = value;
        if (value)
        {
            cameraYShouldMove = true;
            camYOffset = 1f;
            playerCamera.m_YAxisRecentering.m_enabled = true;
            //playerCamera.m_RecenterToTargetHeading.m_enabled = true;

            // -- Calculate heading rotation
            // The first part of the euqation reverses the direction of the tangent calculation
            // Since the definition of tangent is a ccw rotation starting from the right
            // Then, since our initial dir does not align with the tangent dir (negative vertical versus positive horizontal)
            // we do a negative rotation of 90 degrees on the final result to make them match (-90 in a clockwise direction makes 
            // negative vertical go to positive horizontal)
            xRecTarget = 360f - Mathf.Rad2Deg * Mathf.Atan2(lastWallNormal.z, lastWallNormal.x) - 90f;

            // Offset target rotation to be towards the old player direction
            float dot = Vector3.Dot(lastWallNormal, camRot * Vector3.right);
            xRecTarget += (dot / Mathf.Abs(dot)) * 90f;
            Invoke("SetIswalljumpingFalse", xRec.m_RecenteringTime);
        }
        else
        {
            playerCamera.m_YAxisRecentering.m_enabled = false;
            //playerCamera.m_RecenterToTargetHeading.m_enabled = false;
        }
    }

    // Used by invoke to stop camera x centering after centring is done
    void SetIswalljumpingFalse()
    {
        isWalljumping = false;
    }

    void UpdateWalljumping()
    {
        if (isWalljumping)
        {
            xRec.DoRecentering(ref playerCamera.m_XAxis, Time.deltaTime, xRecTarget);
        }
    }
}
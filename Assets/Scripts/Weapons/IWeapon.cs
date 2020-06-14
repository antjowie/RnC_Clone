using UnityEngine;

public abstract class IWeapon : MonoBehaviour
{
    public GameObject playerOrientation;
    Camera cam;

    // Variables used in all weapons
    protected Vector3 lookDir;
    protected Vector3 playerDir;

    protected virtual void OnShoot() { }
    protected virtual void OnShootDown() { }
    protected virtual void OnShootRelease() { }

    public void Shoot()
    {
        CalculateVariables();
        OnShoot();
    }
    public void ShootDown()
    {
        CalculateVariables();
        OnShootDown();
    }
    public void ShootRelease()
    {
        CalculateVariables();
        OnShootRelease();
    }

    public void Start()
    {
        cam = Camera.main;
    }

    void CalculateVariables()
    {
        lookDir = cam.transform.forward;
        playerDir = playerOrientation.transform.forward;
    }
}

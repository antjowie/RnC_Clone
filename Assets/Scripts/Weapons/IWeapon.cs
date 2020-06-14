using UnityEngine;

public abstract class IWeapon : MonoBehaviour
{
    // Variables used in all weapons
    internal Vector3 shootDir;
    Camera cam;

    internal virtual void OnShoot() { }
    internal virtual void OnShootDown() { }
    internal virtual void OnShootRelease() { }

    public void Shoot()
    {
        OnShoot();
    }
    public void ShootDown()
    {
        OnShootDown();
    }
    public void ShootRelease()
    {
        OnShootRelease();
    }

    public void Start()
    {
        cam = Camera.main;
    }

    public void Update()
    {
        shootDir = cam.transform.forward;
    }
}

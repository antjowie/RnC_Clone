using UnityEngine;

public class TripleShot : IWeapon
{
    public GameObject projectile;
    public GameObject shootOrigin;

    protected override void OnShoot()
    {
        //var proj = Instantiate(projectile);

        // temp
        var obj = Instantiate(projectile, shootOrigin.transform.position, shootOrigin.transform.rotation);

        obj.layer = LayerMask.NameToLayer("PlayerProjectile");
        var rb = obj.AddComponent<Rigidbody>();
        obj.AddComponent<CapsuleCollider>().isTrigger = true;

        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.angularVelocity = new Vector3(Mathf.PI * 2f, Mathf.PI * 4f, Mathf.PI);

        var shootDir = playerDir;
        shootDir.y = 0;

        rb.velocity = shootDir.normalized * 20f;
        rb.WakeUp();

        Destroy(obj, 5f);
    }
}

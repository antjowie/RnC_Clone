using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

public class Flamethrower : IWeapon
{
    public ParticleSystem particleSystem;
    public ParticleSystem.MainModule main;

    private void Awake()
    {
        main = particleSystem.main;
    }

    protected override void OnShoot()
    {
        particleSystem.transform.rotation = Quaternion.LookRotation(playerDir);
    }

    protected override void OnShootDown()
    {
        particleSystem.Play();
        particleSystem.transform.rotation = Quaternion.LookRotation(playerDir);
    }

    protected override void OnShootRelease()
    {
        particleSystem.Stop();
    }

    private void OnParticleTrigger()
    {
        print("OOF2");
    }
}

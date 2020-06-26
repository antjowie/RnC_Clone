using System.Collections;
using System.Data;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float modifier = 1f;
    public float CurrentHealth { get; private set; }

    public delegate void OnDeathCB();
    public OnDeathCB onDeath;
    
    public SkinnedMeshRenderer[] renderers;

    private void OnValidate()
    {
        CurrentHealth = maxHealth;
    }

    void Start()
    {
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    IEnumerator FlashHit(float time)
    {
        var elapsed = 0f;
        Color flash = Color.red;
        Color white = Color.white;

        while(elapsed < time)
        {
            elapsed += Time.deltaTime;

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                renderer.material.SetColor("_BaseColor", Color.Lerp(flash,white,elapsed/time));
            }
            yield return null;
        }
    }

    public void Damage(float damage)
    {
        StopCoroutine(FlashHit(0.2f));
        StartCoroutine(FlashHit(0.2f));


        if (damage > 0f)
            damage *= modifier;
        CurrentHealth -= damage;

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        if(CurrentHealth == 0f) onDeath();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectile") ||
            other.gameObject.layer == LayerMask.NameToLayer("TargetProjectile"))
        {
            Destroy(other.transform.root.gameObject);
            print(CurrentHealth);
            Damage(1);
        }        
    }

    private void OnParticleCollision(GameObject other)
    {
        Damage(1);
    }
}


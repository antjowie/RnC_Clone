using System.Data;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float modifier = 1f;
    public float CurrentHealth { get; private set; }

    public delegate void OnDeathCB();
    public OnDeathCB onDeath;

    private void OnValidate()
    {
        CurrentHealth = maxHealth;
    }

    public void Damage(float damage)
    {
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
}

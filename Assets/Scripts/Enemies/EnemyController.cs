using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += Init;
    }

    void OnDisable()
    {
        AssemblyReloadEvents.afterAssemblyReload -= Init;
    }

    private void Init()
    {
        GetComponent<Health>().onDeath = OnDeath;
        print("Reload!");
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, Mathf.PI * Time.deltaTime);   
    }

    void OnDeath()  
    {
        Destroy(gameObject);
    }
}

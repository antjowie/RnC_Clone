using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    TextMesh text;
    Health health;
    // Start is called before the first frame update
    void OnValidate()
    {
        text = GetComponent<TextMesh>();
        health = GetComponentInParent<Health>();

        if(text && health)
        {
            Update();
        }
        else
        {
            print("WARNING: HealthText can't find text or health component");
        }
    }

    // Update is called once per frame
    void Update()
    {
        text.text = Mathf.CeilToInt(health.CurrentHealth).ToString();
    }
}

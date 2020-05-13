using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SmoothFollow : MonoBehaviour
{
    public Transform follow;
    public float movespeed = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, follow.position, movespeed * Time.deltaTime);
    }
}

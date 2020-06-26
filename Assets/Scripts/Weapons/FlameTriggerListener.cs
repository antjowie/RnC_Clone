using Boo.Lang.Runtime.DynamicDispatching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;


// NOTE: This is not possible yet and trigger behavior is pretty weird in Unity
// since we have to manually keep track of which colliders we want to collide with
// So this code won't be used tho if the engine improves on it (which it kinda has in 
// 2020.2.0a15 but we still have to manually update the list of colliders)
public class FlameTriggerListener : MonoBehaviour
{

    private void OnParticleCollision(GameObject other)
    {
        print(other.name);
    }
    //ParticleSystem ps;
    //List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

    //private void Awake()
    //{
    //    ps = GetComponent<ParticleSystem>();
    //}

    //private void OnParticleTrigger()
    //{
    //    // TEMP UGLY
    //    var trigger = ps.trigger;
    //    print(trigger.maxColliderCount);


    //    // get
    //    int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

    //    if(numEnter != 0)
    //    print(numEnter);
    //    // iterate
    //    for (int i = 0; i < numEnter; i++)
    //    {
    //        ParticleSystem.Particle p = enter[i];
    //        p.startColor = new Color32(255, 0, 0, 255);
    //        enter[i] = p;
    //    }

    //    // set
    //    ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    //}
}

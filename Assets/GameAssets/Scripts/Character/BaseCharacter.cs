using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour 
{
    protected Health HealthReference { get; private set; }

    protected virtual void Awake() 
    {
        HealthReference = GetComponent<Health>();
        HealthReference.Initialize(OnDead);
    }

    //~==================================================
    // Virtual functions
    protected virtual void Update()                     { }
    protected virtual void LateUpdate()                 { }
    protected virtual void Start()                      { }
    protected virtual void OnAnimatorIK(int layerIndex) { }


    protected virtual void OnDead()                     { }
}

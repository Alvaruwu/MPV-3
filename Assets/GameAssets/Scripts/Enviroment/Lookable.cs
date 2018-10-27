using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lookable : MonoBehaviour 
{
    [SerializeField]Vector3 _offset;

#if UNITY_EDITOR
    [SerializeField] float _gizmoScale = .1f;
#endif

    public Vector3 GetLookableLocation
    {
        get { return transform.position + _offset; }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetLookableLocation, _gizmoScale);
    }
}

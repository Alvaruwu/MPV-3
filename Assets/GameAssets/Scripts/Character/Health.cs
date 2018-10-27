using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Health : MonoBehaviour
{
    [System.NonSerialized]
    System.Action onDeadEvent;


#if UNITY_EDITOR
    [SerializeField] bool _bIsInmortal;
#else
    const bool _bIsInmortal = false;
#endif

    [SerializeField] int _maxLife;
    int _currentLife;

    public void SetCurrentLife(int NewLife)
    {
        _currentLife = NewLife;
    }

    public int GetCurrentLife()
    {
        return _currentLife;
    }



    public void Initialize(System.Action onEvent)
    {
        onDeadEvent = onEvent;
        _currentLife = _maxLife;
    }

    public void Damage(int ammount)
    {
        _currentLife -= ammount;
        if(_currentLife <= 0)
        {
            onDeadEvent.Invoke();
        }
    }

    public void Heal(int ammount)
    {
        _currentLife = Mathf.Min(_currentLife + ammount, _maxLife);
    }

    public bool IsDead{ get { return _currentLife <= 0; }}
}

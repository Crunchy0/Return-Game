using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityLogic : MonoBehaviour
{
    //protected IEntity entity = null;
    protected float health;
    protected float maxHealth;
    //protected bool isDead = false;

    public bool IsDead { get => health <= 0; }

    // Start is called before the first frame update
    void Start()
    {
        SetupEntity();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsDead)
        {
            return;
        }
    }

    protected abstract void SetupEntity();

    public virtual void TakeHit(float damage)
    {
        if (IsDead) return;
        health -= damage;
        Debug.Log($"{gameObject.name}: entity has been hit with damage={damage}");
        if(IsDead)
        {
            Debug.Log($"{gameObject.name}: entity is dead now");
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterEntity : EntityLogic
{
    protected override void SetupEntity()
    {
        //entity = new Monster();
        maxHealth = 200;
        health = maxHealth;
    }

    public override void TakeHit(float damage)
    {
        if (IsDead) return;
        health -= damage;
        Debug.Log($"{gameObject}: entity has been hit with damage={damage} (health={health})");
        if (IsDead)
        {
            Debug.Log($"{gameObject}: entity is dead now");
            Destroy(GetComponent<UnityEngine.AI.NavMeshAgent>());
            Destroy(GetComponent<MonsterBT>());
            //GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.None;
            GameObject colObj = GetComponentInChildren<Collider>().gameObject;
            colObj.transform.SetParent(null);
            Rigidbody rb = colObj.AddComponent<Rigidbody>();
            rb.mass = 50;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Destroy(gameObject);
            Destroy(this);
        }
    }
}

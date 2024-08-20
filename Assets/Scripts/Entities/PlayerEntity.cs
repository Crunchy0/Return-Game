using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerEntity : EntityLogic
{
    public static event Action onPlayerDeath;
    [SerializeField] private AudioSource asource;

    protected override void SetupEntity()
    {
        //entity = new Monster();
        maxHealth = 50;
        health = maxHealth;
    }

    public override void TakeHit(float damage)
    {
        if (IsDead) return;
        health -= damage;
        asource.Play();
        Debug.Log($"{gameObject}: entity has been hit with damage={damage} (health={health})");
        if (IsDead)
        {
            Debug.Log($"Player is dead now");
            onPlayerDeath?.Invoke();
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationY;
        }
    }
}

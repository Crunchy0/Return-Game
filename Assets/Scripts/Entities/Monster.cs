using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : IEntity
{
    public EntityId Id { get; } = EntityId.Monster;
    public EntityType Type { get; } = EntityType.Enemy;
    public float Health { get; set; }
    public float MaxHealth { get; } = 200f;

    public Monster()
    {
        Health = MaxHealth;
    }
}

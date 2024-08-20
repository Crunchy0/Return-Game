using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : IEntity
{
    public EntityId Id { get; } = EntityId.EntityGeneral;
    public EntityType Type { get; } = EntityType.Obstacle;
    public float MaxHealth { get; } = 100f;
    public float Health { get; set; }

    public Obstacle()
    {
        Health = MaxHealth;
    }
}

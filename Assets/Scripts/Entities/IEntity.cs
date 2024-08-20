using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityId
{
    EntityGeneral,
    EntityPlank,
    Monster
}

public enum EntityType
{
    Player,
    Enemy,
    Obstacle
}

public interface IEntity
{
    EntityId Id { get; }
    EntityType Type { get; }
    float MaxHealth { get; }
    float Health { get; set; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleLogic : EntityLogic
{
    protected override void SetupEntity()
    {
        //entity = new Obstacle();
        maxHealth = 100f;
        health = maxHealth;
    }
}

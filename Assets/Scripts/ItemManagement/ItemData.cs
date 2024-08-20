using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ItemId
{
    LaserGun,
    GrenadeLauncher,
    LaserAmmo,
    Grenade,
    Medkit
}

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public ItemId itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public List<Type> defaultLogics;
}
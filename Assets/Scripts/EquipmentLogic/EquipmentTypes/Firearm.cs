using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public interface IFirearm
{
    bool AutoAllowed { get; }
    bool AutoEnabled { get; set; }
    ItemId AmmoType { get; }
    int MaxAmmo { get; }
    int AmmoLeft { get; set; }
    float StartVelocity { get; }
    float FireRate { get; set; }
    float ShotInterval { get; }
    float SpreadHorizontal { get; }
    float SpreadVertical { get; }
    float Recoil { get; }

    float Damage { get; }

    void Fire();
    int Reload(int amount);
}


public class LaserGun : ItemInstance, IFirearm
{
    private bool _autoEnabled = true;
    private float _fireRate = 10f;
    private int _ammoLeft = 0;

    public override int Max { get => 1; }
    public override uint BaseVolume { get; } = 8125;

    public bool AutoAllowed { get => true; }
    public bool AutoEnabled
    {
        get => _autoEnabled;
        set => _autoEnabled = AutoAllowed ? value : false;
    }

    public ItemId AmmoType { get => ItemId.LaserAmmo; }
    public int MaxAmmo { get => 30; }
    public int AmmoLeft {
        get => _ammoLeft;
        set => _ammoLeft = (value > MaxAmmo) ? MaxAmmo : value;
    }

    public float StartVelocity { get => 200.0f; }

    public float FireRate
    {
        get => _fireRate;
        set => _fireRate = (value <= 0) ? _fireRate : value;
    }
    public float ShotInterval => 1.0f / FireRate;

    public float SpreadHorizontal { get; } = 0.015f;
    public float SpreadVertical { get; } = 0.024f;

    public float Recoil { get; } = 0.5f;

    public float Damage { get => 25f; }

    public LaserGun(ItemData data)
    {
        _data = data;
        _data.defaultLogics = new List<Type>()
        {
            typeof(ShootingLogic)
        };
        Amount = Max;
        AutoEnabled = true;
        FireRate = 10f;
    }

    public override ItemInstance Clone()
    {
        LaserGun clone = new LaserGun(_data)
        {
            Amount = Amount,
            AutoEnabled = _autoEnabled,
            AmmoLeft = _ammoLeft,
            FireRate = _fireRate
        };
        return clone;
    }

    public void Fire()
    {
        if (AmmoLeft < 1) return;
        AmmoLeft--;
    }

    public int Reload(int amount)
    {
        int overflow = AmmoLeft + amount - MaxAmmo;
        AmmoLeft += amount;
        return overflow > 0 ? overflow : 0;
    }
}

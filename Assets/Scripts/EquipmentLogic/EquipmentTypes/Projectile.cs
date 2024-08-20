using System.Collections.Generic;
using System;

public interface IProjectile
{
    static float DmgScale { get; }
}

public class LaserAmmo : ItemInstance, IProjectile
{
    public LaserAmmo(ItemData data)
    {
        _data = data;
        _data.defaultLogics = new List<Type>();
        Amount = Max;
    }

    public static float DmgScale { get; } = 0.8f;

    public override int Max
    {
        get => 30;
    }

    public override uint BaseVolume { get => 6; }

    public override ItemInstance Clone()
    {
        LaserAmmo clone = new(_data)
        {
            Amount = Amount
        };
        return clone;
    }
}
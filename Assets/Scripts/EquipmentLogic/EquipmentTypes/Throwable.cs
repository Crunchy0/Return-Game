using System.Collections.Generic;
using System;
public class Grenade : ItemInstance
{
    public Grenade(ItemData data)
    {
        _data = data;
        _data.defaultLogics = new List<Type>()
        {
            typeof(ExplosiveItemLogic)
        };
        Amount = Max;
    }

    public override int Max { get => 3; }

    public override uint BaseVolume { get => 250; }

    public override ItemInstance Clone()
    {
        Grenade clone = new(_data)
        {
            Amount = Amount
        };
        return clone;
    }
}


public abstract class ItemInstance
{
    protected ItemData _data = null;
    protected int amount = 0;

    public ItemData Data { get => _data;}
    public abstract int Max { get; }
    public abstract uint BaseVolume { get; }
    public int Amount { get => amount; set => amount = (value > Max) ? Max : value; }
    public uint Volume { get => (uint)Amount * BaseVolume; } // milliliters

    public abstract ItemInstance Clone();
}

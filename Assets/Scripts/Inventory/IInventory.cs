public interface IInventory
{
    int ItemCount { get; }

    bool AddItem(ItemInstance item, out int idx);
    bool GetItem(int idx, out ItemInstance item);
    bool RemoveItem(int idx, out ItemInstance item);
}

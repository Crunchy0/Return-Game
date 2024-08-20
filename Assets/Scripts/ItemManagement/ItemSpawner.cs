using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-60)]
public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private List<ItemData> _startItemsData = null;
    private Dictionary<ItemId, ItemInstance> _prototypes = null;

    private void Start()
    {
        _prototypes = new Dictionary<ItemId, ItemInstance>();
        if (_startItemsData == null) return;

        foreach(ItemData data in _startItemsData)
        {
            AddPrototype(data);
        }
    }

    private void OnEnable()
    {
        ItemContainer.onItemRequest += createItem;
    }

    private void OnDisable()
    {
        ItemContainer.onItemRequest -= createItem;
    }

    public bool AddPrototype(ItemData data)
    {
        if (_prototypes.TryGetValue(data.itemId, out ItemInstance proto))
        {
            return false;
        }

        switch (data.itemId)
        {
            case ItemId.LaserGun:
                _prototypes.Add(data.itemId, new LaserGun(data));
                break;
            case ItemId.LaserAmmo:
                _prototypes.Add(data.itemId, new LaserAmmo(data));
                break;
            case ItemId.Grenade:
                _prototypes.Add(data.itemId, new Grenade(data));
                break;
            default:
                return false;
        }
        return true;
    }

    public bool RemovePrototype(ItemId id)
    {
        return _prototypes.Remove(id);
    }

    public ItemInstance createItem(ItemId id, int amount = 0)
    {
        if (_prototypes.TryGetValue(id, out ItemInstance proto))
        {
            ItemInstance newItem = proto.Clone();
            if (amount > 0)
            {
                newItem.Amount = amount;
            }
            return newItem;
        }
        return null;
    }
}

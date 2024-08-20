using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour, IInventory
{
    private List<ItemInstance> _items = new List<ItemInstance>();
    private int[] _lastIndices = new int[Enum.GetNames(typeof(ItemId)).Length];
    private uint _totalVolume = 0;
    [SerializeField] private uint _maxVolume;

    private int _equippedItemIdx = -1;

    public GameObject selfInventory;
    public GameObject slotPrefab;

    public event Action<int, ItemInstance> onEquip;
    public event Action<int, ItemInstance> onUnequip;
    public event Action<int, ItemInstance> onDrop;

    public int ItemCount { get => _items.Count; }
    public uint TotalVolume { get => _totalVolume; private set => _totalVolume = value; }
    public uint MaxVolume { get => _maxVolume; }

    public InputActionReference toggleAction;
    public InputActionReference quickAccessAction;
    public InputActionReference dropAction;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _lastIndices.Length; i++)
        {
            _lastIndices[i] = -1;
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
            
        //}
        //for (KeyCode kc = KeyCode.Alpha1; kc <= KeyCode.Alpha9; kc++)
        //{
        //    if (Input.GetKeyDown(kc))
        //    {
        //        _QuickSlotEquip(kc - KeyCode.Alpha1);
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.T) && _equippedItemIdx != -1)
        //{
        //    int idx = _equippedItemIdx;
        //    UnequipItem();
        //    dropItem(idx);
        //}
    }

    private void OnEnable()
    {
        toggleAction.action.performed += ToggleInventory;
        dropAction.action.performed += DropOnPress;
        quickAccessAction.action.performed += QuickSlotEquip;

        ItemLogic.onUsed += _ItemUsed;
        Interaction.onCollect += AddItemAction;
        PlayerEntity.onPlayerDeath += _DisableInventory;
        //GunBehaviour.onReload += RemoveItems;
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= ToggleInventory;
        dropAction.action.performed -= DropOnPress;
        quickAccessAction.action.performed -= QuickSlotEquip;

        ItemLogic.onUsed -= _ItemUsed;
        Interaction.onCollect -= AddItemAction;
        PlayerEntity.onPlayerDeath -= _DisableInventory;
        //GunBehaviour.onReload -= RemoveItems;
    }

    private void ToggleInventory(InputAction.CallbackContext ctx)
    {
        selfInventory.SetActive(!selfInventory.activeSelf);
        if (selfInventory.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void DropItem(int idx)
    {
        ItemInstance item;
        if (!UnequipItem() || !RemoveItem(idx, out item))
            return;
        onDrop?.Invoke(idx, item);
    }

    private void DropOnPress(InputAction.CallbackContext ctx)
    {
        DropItem(_equippedItemIdx);
        // Add dropping from inventory
    }

    private void QuickSlotEquip(InputAction.CallbackContext ctx)
    {
        int idx;
        if (!int.TryParse(ctx.control.name, out idx))
            return;
        idx--;
        int prevIdx = _equippedItemIdx;
        UnequipItem();
        if (idx != prevIdx)
        {
            EquipItem(idx);
        }
    }

    public bool GetItem(int idx, out ItemInstance item)
    {
        item = null;
        if (0 <= idx && idx < ItemCount)
            item = _items[idx];
        return item != null;
    }

    public int GetItemLastIdx(ItemId id)
    {
        return _lastIndices[(int)id];
    }

    public bool AddItem(ItemInstance item, out int idx)
    {
        int insertIdx = 0;

        int itemId = (int)item.Data.itemId;
        int curId = itemId;

        uint addVol = item.Volume;

        idx = -1;
        if (_maxVolume - _totalVolume < addVol)
        {
            return false;
        }

        while (curId >= 0 && _lastIndices[curId] == -1)
        {
            curId--;
        }

        if (curId >= 0)
        {
            insertIdx = _lastIndices[curId] + 1;
        }

        int cap = 0;
        if (curId == itemId)
        {
            ItemInstance last;
            GetItem(_lastIndices[curId], out last);
            idx = _lastIndices[curId];
            cap = last.Max - last.Amount;
            last.Amount += (item.Amount < cap) ? item.Amount : cap;
        }
        if (item.Amount > cap)
        {
            item.Amount -= cap;
            _lastIndices[(int)item.Data.itemId] = insertIdx;
            idx = insertIdx;
            for (int i = (int)item.Data.itemId + 1; i < _lastIndices.Length; i++)
            {
                if (_lastIndices[i] != -1)
                {
                    _lastIndices[i]++;
                }
            }
            _items.Insert(insertIdx, item);
        }

        _totalVolume += addVol;

        if (idx <= _equippedItemIdx)
            _equippedItemIdx++;

        _RefreshView();
        return true;
    }

    public void AddItemAction(ItemInstance item)
    {
        int idx;
        AddItem(item, out idx);
    }

    public int ReduceAmount(int idx, int amount)
    {
        ItemInstance item;
        if (amount < 1 || !GetItem(idx, out item))
            return 0;

        if (amount > item.Amount)
            amount = item.Amount;

        item.Amount -= amount;
        _totalVolume -= (uint)amount * item.BaseVolume;
        return item.Amount;
    }

    public bool RemoveItem(int idx, out ItemInstance item)
    {
        if(!GetItem(idx, out item))
            return false;

        _items.RemoveAt(idx);
        _totalVolume -= item.Volume;

        int removedId = (int)item.Data.itemId;
        int newIdx = --_lastIndices[removedId];
        if (newIdx >= 0 && (int)_items[newIdx].Data.itemId != removedId)
            _lastIndices[removedId] = -1;

        for (int i = removedId + 1; i < _lastIndices.Length; i++)
        {
            if (_lastIndices[i] != -1)
                _lastIndices[i]--;
        }

        _RefreshView();

        return true;
    }

    public int RemoveItems(ItemId id, int amount)
    {
        int curIdx = _lastIndices[(int)id];
        int gathered = 0;

        if (amount < 1)
            return 0;

        while (curIdx >= 0 && _items[curIdx].Data.itemId == id)
        {
            ItemInstance curItem = _items[curIdx];
            gathered += curItem.Amount;
            if (gathered <= amount)
            {
                ItemInstance tempItem;
                RemoveItem(curIdx, out tempItem);
            }
            else
            {
                _totalVolume -= (uint)curItem.Volume;
                curItem.Amount = gathered - amount;
                _totalVolume += (uint)curItem.Volume;
                break;
            }

            curIdx--;
        }

        if(gathered > 0)
            _RefreshView();

        return gathered < amount ? gathered : amount;
    }

    private void _DisableInventory()
    {
        ItemInstance item;
        RemoveItem(_equippedItemIdx, out item);
        _equippedItemIdx = -1;
        selfInventory.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.enabled = false;
    }

    private void _RefreshView()
    {
        Text volText = selfInventory.transform.GetChild(0).GetComponentInChildren<Text>();
        Transform grid = selfInventory.GetComponentInChildren<GridLayoutGroup>().transform;

        if (grid.childCount < ItemCount)
        {
            for (int i = 0; i < 5; i++)
            {
                Instantiate(slotPrefab, grid);
            }
        }
        else if (ItemCount <= grid.childCount - 5)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform removedChild = grid.GetChild(grid.childCount - 1);
                removedChild.SetParent(null);
                Destroy(removedChild.gameObject);
            }
        }

        volText.text = $"Volume: {TotalVolume}/{MaxVolume}";
        for (int i = 0; i < grid.childCount; i++)
        {
            Transform slot = grid.GetChild(i);
            Image icon = slot.GetChild(0).GetComponent<Image>();
            Text caption = slot.GetComponentInChildren<Text>();

            ItemInstance item;
            if (GetItem(i, out item))
            {
                icon.sprite = item.Data.icon;
                caption.text = $"{item.Amount}";
            }
            else
            {
                icon.sprite = null;
                caption.text = "";
            }
        }
    }

    private void _ItemUsed(ItemInstance item)
    {
        int curIdx = GetItemLastIdx(item.Data.itemId);
        if (curIdx < 0) return;

        ItemInstance curItem;
        do
        {
            GetItem(curIdx, out curItem);
            if(curItem == item)
            {
                break;
            }
            curIdx--;
        } while (curIdx >= 0 && curItem.Data.itemId == item.Data.itemId);
        
        if(curIdx < 0 || curItem.Data.itemId != item.Data.itemId)
        {
            return;
        }

        int itemsLeft = ReduceAmount(curIdx, 1);

        if(itemsLeft < 1)
        {
            ItemInstance tempItem;
            if (curIdx == _equippedItemIdx)
                UnequipItem();
            RemoveItem(curIdx, out tempItem);
        }
        _RefreshView();
    }

    private int QueryItems(ItemId id, int amount)
    {
        int gathered = 0;
        int idx = _lastIndices[(int)id];
        ItemInstance item;

        while (idx >= 0)
        {
            GetItem(idx, out item);
            if(item.Data.itemId != id)
            {
                break;
            }
            if (item.Amount >= amount - gathered)
            {
                return amount;
            }

            gathered += item.Amount;
            idx--;
        }
        return gathered;
    }

    public void EquipItem(int idx)
    {
        ItemInstance item;
        if (!GetItem(idx, out item))
            return;
        _equippedItemIdx = idx;
        onEquip?.Invoke(idx, item);
    }

    private bool UnequipItem()
    {
        ItemInstance item;
        if (!GetItem(_equippedItemIdx, out item))
            return false;
        onUnequip?.Invoke(_equippedItemIdx, item);
        _equippedItemIdx = -1;
        return true;
    }
}

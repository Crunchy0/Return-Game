using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(-50)]
public class ItemController : MonoBehaviour
{
    public ItemId itemId;
    protected ItemInstance itemInstance = null;
    protected List<ItemLogic> logicScripts = null;

    private void Awake()
    {
        ItemContainer container = GetComponent<ItemContainer>();
        if (container == null)
        {
            Debug.Log($"{gameObject.name}: Не удалось получить информацию о предмете");
            Destroy(this);
        }
        itemInstance = container.itemInstance;
        Debug.Log($"Item instance in controller: {itemInstance}");
    }

    protected int LogicIndex(Type logicType)
    {
        if (logicScripts == null || logicScripts.Count < 1) return -1;

        for(int i = 0; i < logicScripts.Count; i++)
        {
            if(logicScripts[i].GetType() == logicType)
            {
                return i;
            }
        }
        return -1;
    }

    public int AddLogic(Type logicType, bool enableOnStart=false)
    {
        if(logicType.BaseType != typeof(ItemLogic))
        {
            return -1;
        }

        int logIdx = LogicIndex(logicType);
        if (logIdx >= 0)
        {
            Debug.Log($"Object {gameObject} already has this logic: {logicType}");
            return logIdx;
        }

        if(logicScripts == null)
        {
            logicScripts = new List<ItemLogic>();
        }

        ItemLogic newLogic = (ItemLogic)gameObject.AddComponent(logicType);
        newLogic.enabled = enableOnStart;
        logicScripts.Add(newLogic);
        return logicScripts.Count - 1;
    }

    public void RemoveLogic(Type logicType)
    {
        int logIdx = LogicIndex(logicType);

        if (logIdx == -1) return;

        ItemLogic remLogic = logicScripts[logIdx];
        logicScripts.RemoveAt(logIdx);
        remLogic.stopLogic();
        Destroy(remLogic);
    }

    public void EnableLogic(Type logicType)
    {
        int logIdx = LogicIndex(logicType);

        if (logIdx == -1) return;

        logicScripts[logIdx].enabled = true;
    }

    public void DisableLogic(Type logicType)
    {
        int logIdx = LogicIndex(logicType);

        if (logIdx == -1) return;

        logicScripts[logIdx].stopLogic();
    }

    protected void EnableAllLogic(int idx, ItemInstance item)
    {
        if (itemInstance != item || logicScripts == null) return;
        for (int i = 0;  i < logicScripts.Count; i++)
        {
            logicScripts[i].enabled = true;
        }
    }

    protected void DisableAllLogic(int idx, ItemInstance item)
    {
        if (itemInstance != item) return;
        for (int i = 0; logicScripts != null && i < logicScripts.Count; i++)
        {
            logicScripts[i].stopLogic();
        }
    }

    public ItemInstance Item { get => itemInstance; }
}

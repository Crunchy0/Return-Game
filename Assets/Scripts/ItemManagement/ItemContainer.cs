using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemContainer : MonoBehaviour
{
    public ItemId itemId;
    [SerializeReference]
    public ItemInstance itemInstance;

    public static event Func<ItemId, int, ItemInstance> onItemRequest;

    void Start()
    {
        if (itemInstance != null) return;

        if (onItemRequest == null)
        {
            Debug.Log($"{gameObject.name}: Контейнер не может запросить экземпляр предмета");
            Destroy(this);
        }

        itemInstance = onItemRequest.Invoke(itemId, -1);
    }
}

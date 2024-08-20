using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(-40)]
public abstract class ItemLogic : MonoBehaviour
{
    protected ItemController controller = null;

    public static event Action<ItemInstance> onUsed;

    protected void InvokeOnUsed(ItemInstance item)
    {
        onUsed?.Invoke(item);
    }
    public abstract void stopLogic(bool disableOnStop=true);
}

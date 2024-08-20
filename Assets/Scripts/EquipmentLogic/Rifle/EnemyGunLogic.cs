using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGunLogic : MonoBehaviour
{
    private IFirearm weapon;

    public bool IsReady { get => weapon.AmmoLeft > 0; }
    // Start is called before the first frame update
    void Start()
    {
        ItemContainer container = GetComponent<ItemContainer>();
        if (container == null || container.itemInstance == null)
        {
            Destroy(this);
            return;
        }

        weapon = (IFirearm)container.itemInstance;
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}

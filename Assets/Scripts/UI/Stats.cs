using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    private Text stats;
    public IFirearm weapon;

    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("Canvas").transform.GetChild(1).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        stats.text = "";
        stats.text += string.Format("Equipped: {0}\n", weapon);
        stats.text += string.Format("Ammo: {0}\n", weapon == null ? 0 : weapon.AmmoLeft);
        stats.text += string.Format("Rate of fire: {0}\n", weapon == null ? 0 : weapon.FireRate);
        stats.text += string.Format("Auto fire: {0}\n", (weapon == null || weapon.AutoEnabled == false) ? "no" : "yes");
        //stats.text
        float y = Input.mouseScrollDelta.y;
        if (Input.GetKey(KeyCode.R))
        {
            stats.text += "Pressed R\n";
        }
        else if(Input.GetKey(KeyCode.Z))
        {
            stats.text += "Pressed Z\n";
        }
        else if (Input.GetKey(KeyCode.E))
        {
            stats.text += "Pressed E\n";
        }
        else if (Input.GetKey(KeyCode.T))
        {
            stats.text += "Pressed T\n";
        }
        else if (Input.GetMouseButton(0))
        {
            stats.text += "Pressed LMB\n";
        }
    }
}

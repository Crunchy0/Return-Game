using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class Interaction : MonoBehaviour
{
    private GameObject gunPos = null;
    private GameObject equippedObject;

    [Header("Player manipulativity")]
    public float ReachDistance = 5.0f;

    [Header("View settings")]
    //public GameObject camHolder;
    public GameObject crosshair;

    private GameObject cam;
    private GameObject objHeld;

    private GameObject targetObject;

    private Vector3 objHeldPrevPos;
    private Vector3 objHeldLinVel;

    private bool crosshair_detects = false;
    private bool toggleHoldObj = false;

    private Color[] detect_cols =
    {
        Color.red,      // for unavailable objects
        Color.green,    // for collectables
        Color.yellow,   // for movable objects
        Color.cyan      // for weapons
    };

    private const string COLLECTABLE_TAG = "Collectable";
    private const string MOVABLE_TAG = "Movable";
    private const string WEAPON_TAG = "Weapon";

    InventoryManager im;

    public static event Action<ItemInstance> onCollect;

    public InputActionReference interaction;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>().gameObject; // заглушка, убери нахуй и сделай нормально
        gunPos = cam.transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        interaction.action.performed += HandleInteraction;

        im = GetComponent<InventoryManager>();
        im.onEquip += equipItem;
        im.onUnequip += unequipItem;
        im.onDrop += dropItem;
        PlayerEntity.onPlayerDeath += PlayerDied;
    }

    private void OnDisable()
    {
        interaction.action.performed -= HandleInteraction;

        im.onEquip -= equipItem;
        im.onUnequip -= unequipItem;
        im.onDrop -= dropItem;
        PlayerEntity.onPlayerDeath -= PlayerDied;
    }

    private void PlayerDied()
    {
        if (equippedObject)
        {
            equippedObject.transform.SetParent(null);
            foreach (ItemLogic logic in equippedObject.GetComponents<ItemLogic>())
            {
                Destroy(logic);
            }
            Destroy(equippedObject.GetComponent<ItemController>());

            foreach (Collider col in equippedObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }
            equippedObject.GetComponent<Rigidbody>().isKinematic = false;
            //Destroy(equippedObject);
        }

        //Destroy(equippedObject);
        Destroy(this);
    }

    private GameObject detectHitObject(RaycastHit hit)
    {
        GameObject hitObj = hit.transform.gameObject;
        Color ch_col = detect_cols[0];

        if (!crosshair_detects) crosshair_detects = true;

        switch (hitObj.tag)
        {
            case COLLECTABLE_TAG:
                ch_col = detect_cols[1];
                break;
            case MOVABLE_TAG:
                ch_col = detect_cols[2];
                if (objHeld) ch_col = new Color32(0, 0, 0, 0);
                break;
            case WEAPON_TAG:
                ch_col = detect_cols[3];
                break;
        }

        if (crosshair.GetComponent<Image>().color != ch_col)
        {
            crosshair.GetComponent<Image>().color = ch_col;
        }

        return hitObj;
    }

    private GameObject instantiateFromItem(ItemInstance item)
    {
        GameObject gObj = new GameObject();
        ItemContainer container = gObj.AddComponent<ItemContainer>();
        container.itemId = item.Data.itemId;
        container.itemInstance = item;

        gObj.AddComponent<Rigidbody>();
        gObj.transform.position = gunPos.transform.position;
        gObj.transform.rotation = cam.transform.rotation;

        GameObject prefab = Instantiate(item.Data.prefab, gObj.transform);

        prefab.transform.position = gObj.transform.position;
        prefab.transform.rotation = gObj.transform.rotation;

        return gObj;
    }

    private void unequipItem(int idx, ItemInstance item)
    {
        Destroy(equippedObject);
        equippedObject = null;
    }

    private void equipItem(int idx, ItemInstance item)
    {
        GameObject gObj = instantiateFromItem(item);

        Rigidbody rb = gObj.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        foreach (Collider col in gObj.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        equippedObject = gObj;

        equippedObject.transform.SetParent(cam.transform);

        ItemController controller = gObj.AddComponent<ItemController>();
        controller.itemId = item.Data.itemId;
        foreach(Type logicType in item.Data.defaultLogics)
        {
            controller.AddLogic(logicType, true);
        }
    }

    private void dropItem(int idx, ItemInstance item)
    {
        GameObject gObj = instantiateFromItem(item);

        //Debug.Log($"Dropping {item.Data.itemId}");

        foreach (Collider col in gObj.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        Rigidbody rb = gObj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(cam.transform.forward * 5f, ForceMode.Impulse);
    }

    private void grabObj()
    {
        Rigidbody objRb = objHeld.GetComponent<Rigidbody>();
        objRb.isKinematic = true;

        objHeldPrevPos = objHeld.transform.position;
        objHeldLinVel = Vector3.zero;
        objHeld.transform.SetParent(cam.transform);

        toggleHoldObj = !toggleHoldObj;
    }

    private void dropObj()
    {
        toggleHoldObj = !toggleHoldObj;

        objHeld.transform.SetParent(null);
        Rigidbody objRb = objHeld.GetComponent<Rigidbody>();
        objHeld = null;

        if (objHeldLinVel.magnitude > 20)
        {
            objHeldLinVel = objHeldLinVel.normalized * 20;
        }
        objRb.isKinematic = false;
        objRb.AddForce(objHeldLinVel, ForceMode.VelocityChange);
        //objRb.velocity = objHeldLinVel;
    }

    private void HandleInteraction(InputAction.CallbackContext ctx)
    {
        if (targetObject == null || objHeld != null)
            return;

        ItemContainer container = targetObject.GetComponent<ItemContainer>();
        if(container != null && container.itemInstance != null)
        {
            ItemInstance item = container.itemInstance;
            Destroy(targetObject);
            //Debug.Log($"Item in container: {container.itemInstance.Data}");
            onCollect?.Invoke(container.itemInstance);
        }
        else if(targetObject.CompareTag(MOVABLE_TAG))
        {
            objHeld = targetObject;
            grabObj();
        }
    }

    private void processInput(GameObject obj)
    {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    HandleInteraction();
        //}
        //if (Input.GetKeyUp(KeyCode.E))
        //{
        //    if(toggleHoldObj)
        //    {
        //        dropObj();
        //    }
        //}
        //else if (Input.GetMouseButtonDown(1) && objHeld)
        //{
        //    objHeldLinVel = cam.transform.forward * 20;
        //    dropObj();
        //}
    }

    private void dragObjHeld()
    {
        if (objHeld == null) return;

        objHeldLinVel = (objHeld.transform.position - objHeldPrevPos)/Time.deltaTime;
        objHeldPrevPos = objHeld.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit;
        GameObject obj = null;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, ReachDistance))
        {
            obj = detectHitObject(hit);
            targetObject = obj;
        }
        else if (crosshair_detects)
        {
            targetObject = null;
            crosshair_detects = !crosshair_detects;
            crosshair.GetComponent<Image>().color = Color.white;
        }
        //processInput(obj);
        //dragObjHeld();
    }
}

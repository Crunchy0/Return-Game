using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosiveItemLogic : ItemLogic
{
    private ItemInstance grenade;
    private float cooldown = 1.5f;
    private bool canThrow = true;

    private InputActionReference attack;

    // Start is called before the first frame update
    void Awake()
    {
        ExplosivePreset preset = GetComponentInChildren<ExplosivePreset>();
        if(preset == null)
        {
            Debug.LogError($"{gameObject.name}: невозможно получить необходимые данные дл€ взрывчатки");
            Destroy(this);
        }
        attack = preset.attack;

        controller = GetComponent<ItemController>();
        if (controller == null || controller.Item == null)
        {
            Debug.LogError($"ќтсутствует контроллер дл€ {gameObject} или экземпл€р класса {typeof(ItemInstance)}");
            Destroy(this);
        }
        grenade = controller.Item;
    }

    private void OnEnable()
    {
        attack.action.started += ThrowGrenade;
    }

    private void OnDisable()
    {
        attack.action.started -= ThrowGrenade;
    }

    public IEnumerator waitCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        canThrow = true;
    }

    private void ThrowGrenade(InputAction.CallbackContext ctx)
    {
        if (!canThrow)
            return;

        GameObject prefab = grenade.Data.prefab;
        GameObject thrownGren = Instantiate(prefab);
        thrownGren.transform.position = transform.position;
        thrownGren.transform.Translate(Vector3.up);
        Rigidbody rb = thrownGren.AddComponent<Rigidbody>();
        Vector3 throwForce = (0.8f * transform.forward + 0.3f * transform.up).normalized;
        rb.AddForce(throwForce * 25f, ForceMode.Impulse);
        rb.AddTorque(transform.right*0.5f, ForceMode.Impulse);
        thrownGren.AddComponent<ExplosiveBehaviour>();
        thrownGren.AddComponent<AudioSource>();

        StartCoroutine(waitCooldown());
        InvokeOnUsed(grenade);

        canThrow = false;
    }

    public override void stopLogic(bool disableOnStop = true)
    {
        StopCoroutine(waitCooldown());
        enabled = disableOnStop ? false : true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingLogic : ItemLogic
{
    private IFirearm weapon = null;
    private bool isTriggerHeld = false;

    private GameObject bullet;
    private GameObject bulletSpawn;

    private AudioClip gunshot;
    private Camera playerCam;

    private InputActionReference attack;
    private InputActionReference reloadWeapon;
    private InputActionReference toggleFire;

    private void Awake()
    {
        FirearmPreset firearmData = GetComponentInChildren<FirearmPreset>();

        if (firearmData == null)
        {
            Debug.LogError($"{gameObject.name}: невозможно получить необходимые данные дл€ стрелкового оружи€");
            Destroy(this);
        }

        // ахуеьтб
        playerCam = GetComponentInParent<Camera>();

        bullet = firearmData.bullet;
        bulletSpawn = firearmData.bulletSpawn;
        gunshot = firearmData.gunshot;

        attack = firearmData.attack;
        reloadWeapon = firearmData.reloadWeapon;
        toggleFire = firearmData.toggleFire;

        controller = GetComponent<ItemController>();
        if (controller == null || controller.Item == null)
        {
            //Debug.Log($"Controller: {controller}, Item: {controller.Item}");
            Debug.LogError($"ќтсутствует контроллер дл€ {gameObject} или экземпл€р класса {typeof(ItemInstance)}");
            Destroy(this);
        }
        weapon = (IFirearm)controller.Item;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        attack.action.started += Attack;
        attack.action.canceled += Attack;
        reloadWeapon.action.performed += Reload;
        toggleFire.action.performed += ToggleFire;
    }

    private void OnDisable()
    {
        attack.action.started -= Attack;
        attack.action.canceled -= Attack;
        reloadWeapon.action.performed -= Reload;
        toggleFire.action.performed -= ToggleFire;
    }

    private void ToggleFire(InputAction.CallbackContext ctx)
    {
        if (isTriggerHeld || !weapon.AutoAllowed)
            return;
        weapon.AutoEnabled = !weapon.AutoEnabled;
    }

    private void Reload(InputAction.CallbackContext ctx)
    {
        if (isTriggerHeld)
            return;
        int ammoCount = 0;
        InventoryManager im = GameObject.Find("Player").GetComponent<InventoryManager>();
        ammoCount = im.RemoveItems(ItemId.LaserAmmo, weapon.MaxAmmo - weapon.AmmoLeft);
        weapon.Reload(ammoCount);
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
        {
            isTriggerHeld = true;
            IEnumerator coroutine = autoFire();
            StartCoroutine(coroutine);
        }
        if (ctx.canceled)
        {
            isTriggerHeld = false;
        }
    }

    private Vector3 SetBulletDirection()
    {
        // Where to send bullet
        Vector3 targetPoint;
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        // Direction and angle/axis of deviation
        Vector3 direction = (targetPoint - bulletSpawn.transform.position).normalized;
        Vector3 axis = Vector3.Cross(bulletSpawn.transform.forward, direction).normalized;
        float angle = Vector3.SignedAngle(bulletSpawn.transform.forward, direction, axis);
        //axis.Normalize();

        // ну ахуеать
        float xSpread = Random.Range(-weapon.SpreadHorizontal, weapon.SpreadHorizontal);
        float ySpread = Random.Range(0, weapon.SpreadVertical);
        Vector3 spreadVector = bulletSpawn.transform.forward + bulletSpawn.transform.right * xSpread + bulletSpawn.transform.up * ySpread;

        Vector3 shotDirection = Quaternion.AngleAxis(angle, axis) * spreadVector.normalized;
        return shotDirection;
    }

    IEnumerator autoFire()
    {
        while (isTriggerHeld)
        {
            if (weapon.AmmoLeft < 1)
            {
                break;
            }

            weapon.Fire();
            GameObject b = Instantiate(bullet, bulletSpawn.transform.position, bulletSpawn.transform.rotation);

            SMGBullet bb = b.AddComponent<SMGBullet>();
            bb.damage = weapon.Damage;
            b.GetComponent<Rigidbody>().AddForce(SetBulletDirection() * weapon.StartVelocity, ForceMode.VelocityChange);
            GetComponentInChildren<AudioSource>().PlayOneShot(gunshot, 0.5f);

            if (!weapon.AutoEnabled)
            {
                break;
            }

            yield return new WaitForSeconds(weapon.ShotInterval);
        }
        yield return null;
    }

    private void HandleInput()
    {
        //if(Input.GetMouseButtonDown(0))
        //{
        //    isTriggerHeld = true;
        //    IEnumerator coroutine = autoFire();
        //    StartCoroutine(coroutine);
        //}
        //else if(Input.GetMouseButtonUp(0))
        //{
        //    isTriggerHeld = false;
        //}

        //if (Input.GetKeyDown(KeyCode.R) && !isTriggerHeld)
        //{
        //    Reload();
        //}
        //else if (Input.GetKeyDown(KeyCode.Z) && !isTriggerHeld)
        //{
        //    if (weapon.AutoAllowed)
        //    {
        //        weapon.AutoEnabled = !weapon.AutoEnabled;
        //    }
        //}
        //float scroll = Input.mouseScrollDelta.y;
        //if (scroll != 0)
        //{
        //    weapon.FireRate += scroll * 0.25f;
        //    //IWeapon inst = (IWeapon)GetComponentInParent<ItemContainer>().itemInstance;
        //    //Debug.Log($"Rate of fire: in script: {laser.rateOfFire}, in instance: {inst.rateOfFire}");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    public override void stopLogic(bool disableOnStop = true)
    {
        StopCoroutine("autoFire");
        isTriggerHeld = false;
        enabled = disableOnStop ? false : true;
    }
}

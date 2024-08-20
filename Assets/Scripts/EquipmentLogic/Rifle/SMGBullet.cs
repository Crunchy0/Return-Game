using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGBullet : MonoBehaviour
{
    private bool hit = false;
    private bool drawContact = false;
    private Rigidbody rb;
    private Vector3 contact;

    public float damage = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        Destroy(gameObject, 1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hit) return;
        
        rb.useGravity = true;
        ContactPoint c = collision.contacts[0];
        contact = c.point;
        if (c.otherCollider.gameObject.CompareTag("BulletSensitive"))
        {
            drawContact = true;
        }
        hit = true;

        Transform parentTrans = collision.contacts[0].otherCollider.transform.parent;
        EntityLogic entityLogic = parentTrans == null ? null : parentTrans.GetComponent<EntityLogic>();

        float resultDamage = damage - 10 + Random.Range(0, 21);
        if (entityLogic != null)
        {
            Debug.Log($"Bullet hit Entity {parentTrans.gameObject}");
            entityLogic.TakeHit(LaserAmmo.DmgScale * resultDamage);
        }
    }

    private void OnDrawGizmos()
    {
        if (!hit || !drawContact) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(contact, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

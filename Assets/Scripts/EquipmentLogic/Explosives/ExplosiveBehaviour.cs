using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBehaviour : MonoBehaviour
{
    private float timer = 3f;
    private AudioClip boom;
    //Vector
    // Start is called before the first frame update
    void Start()
    {
        boom = Resources.Load<AudioClip>("boom");
        StartCoroutine(countdown());
    }

    private IEnumerator countdown()
    {
        yield return new WaitForSeconds(timer);

        Vector3 pos = transform.position;
        //Debug.Log($"Position: {pos}");
        Collider[] cols = Physics.OverlapSphere(pos, 20f);
        foreach(Collider hit in cols)
        {
            //Debug.Log($"Current collider: {hit.gameObject.name}");
            
            if (hit.gameObject == gameObject)
            {
                continue;
            }
            EntityLogic eLogic = hit.transform.GetComponentInParent<EntityLogic>();
            if (eLogic != null && !eLogic.IsDead)
            {
                eLogic.TakeHit((1 - Vector3.Distance(hit.transform.position, pos) / 20f) * 100);
            }
            Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
            //Rigidbody aRb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //rb.WakeUp();
                rb.AddExplosionForce(200.0f, pos, 20.0f, 4.0F, ForceMode.Impulse);
            }
            //else if(aRb != null)
            //{
            //    aRb.AddExplosionForce(200.0f, pos, 20.0f, 4.0F, ForceMode.Impulse);
            //}
        }
        
        GetComponent<AudioSource>().PlayOneShot(boom, 1);
        yield return new WaitForSeconds(boom.length);
        //Debug.Log("BOOM!");
        Destroy(gameObject);
    }
}

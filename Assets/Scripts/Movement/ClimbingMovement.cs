using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingMovement : MonoBehaviour
{
    private float verticalMove;
    public float climbingSpeed;
    public Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        verticalMove = 0;
    }

    private void OnEnable()
    {
        PlayerEntity.onPlayerDeath += StopClimbing;
    }

    private void OnDisable()
    {
        PlayerEntity.onPlayerDeath -= StopClimbing;
    }

    private void StopClimbing()
    {
        GetComponent<Rigidbody>().useGravity = true;
        Destroy(this);
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(0f, verticalMove, 0f);
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.body.gameObject.CompareTag("Ladder"))
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float input = Input.GetAxis("Vertical");
    }
}

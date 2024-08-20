using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float sensX = 1f, sensY = 1f;
    float rotX = 0f, rotY = 0f;

    private PlayerInput pInp;

    public Transform holder;
    public InputActionReference lookAction;

    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        if(Cursor.lockState == CursorLockMode.None)
        {
            return;
        }
        Vector2 delta = ctx.ReadValue<Vector2>();

        rotY += sensX * delta.x;
        rotX -= sensY * delta.y;
        rotX = Mathf.Clamp(rotX, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        holder.rotation = Quaternion.Euler(0, rotY, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisableCamera()
    {
        Destroy(this);
    }

    private void OnEnable()
    {
        lookAction.action.performed += OnMouseMove;
        PlayerEntity.onPlayerDeath += DisableCamera;
    }

    private void OnDisable()
    {
        lookAction.action.performed -= OnMouseMove;
        PlayerEntity.onPlayerDeath -= DisableCamera;
    }
}

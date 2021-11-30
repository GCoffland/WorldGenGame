using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraBehavior : MonoBehaviour
{
    public void ProcessLook(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Vector3 temp = transform.rotation.eulerAngles + (Vector3)input;
        while(temp.x < 0)
        {
            temp.x += 360f;
        }
        if (temp.x >= 180)
        {
            temp.x = Mathf.Clamp(temp.x, 270, 360);
        }
        else if (temp.x < 180)
        {
            temp.x = Mathf.Clamp(temp.x, 0, 90);
        }
        transform.rotation = Quaternion.Euler(temp);
    }

    private void LateUpdate()
    {
        transform.hasChanged = false;
    }
}

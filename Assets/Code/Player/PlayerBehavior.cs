using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerBehavior : MonoBehaviour
{
    public Rigidbody rb;
    public CapsuleCollider cc;
    public BlockSelector blockSelector;
    new public Camera camera;

    private bool internalNoClip = true;
    private bool noClip
    {
        get
        {
            return internalNoClip;
        }
        set
        {
            cc.enabled = !cc.enabled;
            rb.useGravity = !rb.useGravity;
            rb.freezeRotation = !rb.freezeRotation;
            internalNoClip = value;
        }
    }

    private const float SPEED = 10f;

    // Start is called before the first frame update
    void Start()
    {
        cc.enabled = !noClip;
        rb.useGravity = !noClip;
        rb.freezeRotation = !noClip;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        moveAction = GetComponent<PlayerInput>().actions.FindAction("Move", true);
        moveAction.Enable();
        jumpAction = GetComponent<PlayerInput>().actions.FindAction("Jump", true);
        jumpAction.Enable();
    }


    private void Update()
    {
        ProcessMovement();
    }

    InputAction moveAction;
    InputAction jumpAction;

    public void ProcessMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float jumpInput = jumpAction.ReadValue<float>();
        if (noClip)
        {
            rb.velocity = (camera.transform.rotation * new Vector3(moveInput.x, jumpInput, moveInput.y)) * SPEED;
        }
        else
        {
            Vector3 temp = new Vector3(moveInput.x, 0, moveInput.y) * SPEED;
            temp = camera.transform.rotation * temp;
            temp.y = rb.velocity.y;
            if (jumpAction.WasPressedThisFrame())
            {
                temp += Vector3.up * 10;
            }
            rb.velocity = temp;
        }
    }

    public void ProcessFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Debug.Log("Currently selected block: " + blockSelector.currentBlock.type);
        }
    }

    public void ProcessToggleNoClip(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            noClip = !noClip;
        }
    }
}

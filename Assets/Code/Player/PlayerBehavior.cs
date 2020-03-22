using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public Rigidbody rb;
    public CapsuleCollider cc;
    public Camera cam;
    public GameObject blockSelector;

    private GameObject selectedBlock;
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
    private const float INTERACTDISTANCE = 10f;

    // Start is called before the first frame update
    void Start()
    {
        cc.enabled = !noClip;
        rb.useGravity = !noClip;
        rb.freezeRotation = !noClip;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    // Update is called once per frame
    void Update()
    {
        lookAround();
        if (noClip)
        {
            moveNoClip();
        }
        else
        {
            move();
        }
        Vector3Int? v = selectBlock();
        if (Input.GetButtonDown("Fire1"))
        {
            if(selectedBlock != null)
            {
                selectedBlock.GetComponent<ChunkBehavior>().setVoxel((Vector3Int)v, VOXELTYPE.NONE);
            }
        }
        if (Input.GetButtonDown("NoClip"))
        {
            noClip = !noClip;
        }
        if (Input.GetButtonDown("Debug"))
        {
            Debug.Log("Average model creation time: " + RuntimeAnalysis.getAverageModelCreationTimes());
            Debug.Log("Average voxel creation time: " + RuntimeAnalysis.getAverageVoxelCreationTimes());
            Debug.Log("Average mesh creation time: " + RuntimeAnalysis.getAverageMeshCreationTimes());
            Debug.Log("Average mesh assigning time: " + RuntimeAnalysis.getAverageMeshAssigningTimes());
        }
    }

    private Vector3 rotation = new Vector3(0, 0, 0);

    private void lookAround()
    {
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x = Mathf.Clamp(rotation.x, -89.99f, 89.99f);
        transform.rotation = Quaternion.Euler(rotation);
    }

    private void moveNoClip()
    {
        rb.velocity = (transform.rotation * new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"))) * SPEED;
    }

    private void move()
    {
        Vector3 temp = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * SPEED;
        temp = transform.rotation * temp;
        temp.y = rb.velocity.y;
        rb.velocity = temp;
        if (Input.GetButtonDown("Jump"))
        {
            rb.velocity += Vector3.up * 10;
        }
    }

    Vector3Int? selectBlock()
    {
        RaycastHit rh;
        Physics.Raycast(transform.position, transform.rotation * Vector3.forward, out rh, INTERACTDISTANCE);
        if(rh.collider == null)
        {
            selectedBlock = null;
            blockSelector.SetActive(false);
            return null;
        }
        Vector3 insideblock = rh.point - (rh.normal * 0.001f);
        Vector3Int v = new Vector3Int(0,0,0);
        v.x = (int)Mathf.Floor(insideblock.x);
        v.y = (int)Mathf.Floor(insideblock.y);
        v.z = (int)Mathf.Floor(insideblock.z);
        selectedBlock = rh.collider.gameObject;
        blockSelector.SetActive(true);
        blockSelector.transform.position = v;
        return v;
    }
}

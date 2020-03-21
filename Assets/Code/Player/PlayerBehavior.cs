using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public Rigidbody rb;
    public Camera cam;
    public GameObject blockSelector;

    private GameObject selectedBlock;

    private const float SPEED = 10f;
    private const float INTERACTDISTANCE = 10f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = transform.eulerAngles + new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        rb.velocity = (transform.rotation * new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"))) * SPEED;
        Vector3Int? v = selectBlock();
        if (Input.GetButtonDown("Fire1"))
        {
            if(selectedBlock != null)
            {
                selectedBlock.GetComponent<ChunkBehavior>().setVoxel((Vector3Int)v, VOXELTYPE.NONE);
            }
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

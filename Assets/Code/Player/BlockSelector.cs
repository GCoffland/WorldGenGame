using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;

public class BlockSelector : MonoBehaviour
{
    [SerializeField]
    new private Transform camera;
    public int interactDistance;
    public BlockData currentBlock
    {
        get
        {
            return _currentBlock;
        }
    }
    private BlockData _currentBlock;

    new private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (camera.transform.hasChanged)
        {
            SelectBlock();
        }
    }

    void SelectBlock()
    {
        RaycastHit rh;
        if (!Physics.Raycast(camera.transform.position, camera.transform.rotation * Vector3.forward, out rh, interactDistance, layerMask: LayerMask.GetMask("Terrain")))
        {
            _currentBlock.type = VOXELTYPE.NONE;
            renderer.enabled = false;
            return;
        }
        Vector3Int globalBlockPosition = Vector3Int.FloorToInt(rh.point - (rh.normal * 0.001f));
        Vector3Int localBlockPosition = globalBlockPosition.WorldPosToBlockIndex();
        _currentBlock = rh.collider.GetComponent<ChunkBehavior>()[localBlockPosition.x, localBlockPosition.y, localBlockPosition.z];
        renderer.enabled = true;
        transform.position = globalBlockPosition + (Vector3.one / 2);
    }
}

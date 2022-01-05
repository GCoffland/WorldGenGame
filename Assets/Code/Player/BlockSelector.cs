using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;

public class BlockSelector : MonoBehaviour
{
    [SerializeField]
    new private Transform camera;
    public int interactDistance;
    public VOXELTYPE currentBlock
    {
        get;
        private set;
    }
    public Vector3Int currentBlockIndex
    {
        get;
        private set;
    }
    public ChunkBehavior currentChunk
    {
        get;
        private set;
    }

    new private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        SelectBlock();
    }

    void SelectBlock()
    {
        RaycastHit rh;
        if (!Physics.Raycast(camera.transform.position, camera.transform.rotation * Vector3.forward, out rh, interactDistance, layerMask: LayerMask.GetMask("Terrain")))
        {
            currentBlock = VOXELTYPE.NONE;
            renderer.enabled = false;
            return;
        }
        Vector3Int globalBlockPosition = Vector3Int.FloorToInt(rh.point - (rh.normal * 0.001f));
        Vector3Int localBlockPosition = globalBlockPosition.WorldPosToBlockIndex();
        currentChunk = rh.collider.GetComponent<ChunkBehavior>();
        currentBlockIndex = localBlockPosition;
        currentBlock = currentChunk[localBlockPosition.x, localBlockPosition.y, localBlockPosition.z];
        renderer.enabled = true;
        transform.position = globalBlockPosition + (Vector3.one / 2);
    }
}

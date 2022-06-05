using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block Data", menuName = "new Block Data")]
public class BlockData : ScriptableObject
{
    public int Id;
    public string Name;
    public string Description;
    public Cubemap Cubemap;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[GenerateHLSL(PackingRules.Exact, false)]
public enum DIRECTION
{
    X_NEG,
    X_POS,
    Y_NEG,
    Y_POS,
    Z_NEG,
    Z_POS,
};

public static class StaticDefinitions
{
    public static readonly Vector3Int[] Directions = new Vector3Int[]
    {
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.up,
            Vector3Int.back,
            Vector3Int.forward
    };

    public static readonly IReadOnlyDictionary<Vector3Int, DIRECTION> DirectionIndex = new Dictionary<Vector3Int, DIRECTION>(6)
    {
        {Vector3Int.left, DIRECTION.X_NEG},
        {Vector3Int.right, DIRECTION.X_POS},
        {Vector3Int.down, DIRECTION.Y_NEG},
        {Vector3Int.up, DIRECTION.Y_POS },
        {Vector3Int.back, DIRECTION.Z_NEG},
        {Vector3Int.forward, DIRECTION.Z_POS},
    };
}

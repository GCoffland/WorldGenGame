using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class FlipVector : InputProcessor<Vector2>
{
#if UNITY_EDITOR
    static FlipVector()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<FlipVector>();
    }

    [Tooltip("Flip vector contents so that x = y, and y = x.")]

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        value.x += value.y;
        value.y = value.x - value.y;
        value.x = value.x - value.y;
        return value;
    }
}

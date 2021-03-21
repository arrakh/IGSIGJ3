using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Clear the children of a Game Object's transform.
    /// </summary>
    /// <typeparam name="Transform"></typeparam>
    /// <param name="transform"></param>
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        transform.DetachChildren();
        return transform;
    }
}

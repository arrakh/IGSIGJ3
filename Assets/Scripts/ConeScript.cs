using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeScript : MonoBehaviour
{
    public List<GameObject> objInsideCollision;
    public LayerMask filter;

    private void OnTriggerEnter(Collider other)
    {
        if (filter == (filter | (1 << other.gameObject.layer)) && !objInsideCollision.Contains(other.gameObject))
        {
            Debug.Log("Added " +other.gameObject.name);
            objInsideCollision.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (filter == (filter | (1 << other.gameObject.layer)) && objInsideCollision.Contains(other.gameObject))
        {
            objInsideCollision.Remove(other.gameObject);
        }
    }
}

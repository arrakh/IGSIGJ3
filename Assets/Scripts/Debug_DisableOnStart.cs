using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_DisableOnStart : MonoBehaviour
{
    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
}

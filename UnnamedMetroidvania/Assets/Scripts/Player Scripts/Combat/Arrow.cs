using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private LayerMask DestroyArrow;
    void Update()
    {
        Destroy(gameObject, 3f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((DestroyArrow.value & (1 << collision.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
        
    }
}

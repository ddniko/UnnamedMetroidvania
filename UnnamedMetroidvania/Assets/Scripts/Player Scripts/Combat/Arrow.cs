using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private LayerMask DestroyBullet;
    void Update()
    {
        Destroy(gameObject, 3f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((DestroyBullet.value & (1 << collision.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
        
    }
}

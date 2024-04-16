using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordPogo : MonoBehaviour
{
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private float PogoForce;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && gameObject.transform.eulerAngles == new Vector3(0,0,180))
        {
            PlayerRB.velocity = Vector3.zero;
            PlayerRB.AddForce(Vector2.up * PogoForce, ForceMode2D.Impulse);
        }
    }
}


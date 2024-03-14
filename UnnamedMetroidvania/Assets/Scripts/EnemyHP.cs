using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    public float EHP = 3f;
    public Collider2D Sword;
    public PlayerControls Player;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == Sword)
        {
            EHP -= Player.MDamage;
        }
    }

    void Update() //måske fixedupdate
    {
        if (EHP <= 0)
        {
            Debug.Log("am dead now");
            GameObject.Destroy(gameObject);
        }
    }
}


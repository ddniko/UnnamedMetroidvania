using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    public float EHP = 3f;
    public PlayerDataWithDash Data;
    private float EnemyHitPoint;

    private void Start()
    {
        EnemyHitPoint = EHP;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            EHP -= Data.SDamage;
        }
    }

    void Update() //måske fixedupdate
    {
        if (EHP <= 0)
        {
            Debug.Log("am dead now");
            this.gameObject.SetActive(false);

            EHP = EnemyHitPoint;
        }
    }
}


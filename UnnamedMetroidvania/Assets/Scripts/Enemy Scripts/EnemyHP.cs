using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    public float EHP = 3f;
    public PlayerDataWithDash Data;
    public NewPlayerMovement PlayerData;
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
            if (PlayerData.MP < PlayerData.MaxMP)
            {
                PlayerData.MP += 2;
                if (PlayerData.MP >= PlayerData.MaxMP)
                {
                    PlayerData.MP = PlayerData.MaxMP;
                }
            }
        }

        if (collision.tag == "Arrow")
        {
            Rigidbody2D Arrow = collision.GetComponent<Rigidbody2D>();
            EHP -= Data.ArrowBaseDamage * (Arrow.velocity.magnitude / Data.ArrowSpeed);
            Destroy(collision.gameObject);
        }
    }

    void Update() //måske fixedupdate
    {
        if (EHP <= 0.1f)
        {
            Debug.Log("am dead now");
            gameObject.SetActive(false);

            EHP = EnemyHitPoint;
        }
    }
}


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

    Rigidbody2D EnemyRB;
    float ground;
    public float slamHeight = 5f;

    public int MPRevcory = 5;

    private void Start()
    {
        EnemyHitPoint = EHP;
        EnemyRB = this.gameObject.GetComponent<Rigidbody2D>();
        ground = EnemyRB.position.y;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            EHP -= Data.SDamage;
            if (PlayerData.MP < PlayerData.MaxMP)
            {
                PlayerData.MP += MPRevcory;
                if (PlayerData.MP >= PlayerData.MaxMP)
                {
                    PlayerData.MP = PlayerData.MaxMP;
                }
            }
        }

        if (collision.tag == "Fireball")
        {
            EHP -= Data.FireballDamage;
        }

        if (collision.tag == "Slam") //Hvis slammet gå opad
        {
            EHP -= Data.SlamDamage;
            EnemyRB.position = new Vector2(EnemyRB.position.x, EnemyRB.position.y + slamHeight);
            EnemyRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
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
        if (EnemyRB.position.y <= ground)
        {
            EnemyRB.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            EnemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}


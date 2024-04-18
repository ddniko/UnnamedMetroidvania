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
    private float ground;
    public float slamHeight = 5f;

    [SerializeField] private Material DamageFlash;
    private SpriteRenderer Rend;
    private Material OrigMaterial;
    [SerializeField] private float flashDur;

    public int MPRevcory = 5;

    private void Start()
    {
        EnemyHitPoint = EHP;
        EnemyRB = gameObject.GetComponent<Rigidbody2D>();
        ground = EnemyRB.position.y;

        Rend = gameObject.GetComponent<SpriteRenderer>();
        OrigMaterial = Rend.material;

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            EHP -= Data.SDamage;
            StartCoroutine(Ouch());
            if (PlayerData.MP < PlayerData.MaxMP)
            {
                PlayerData.MP += MPRevcory;
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
            StartCoroutine(Ouch());
            Destroy(collision.gameObject);

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
            StartCoroutine(Ouch());
        }

        if (collision.tag == "Slam") //Hvis slammet gå opad
        {
            EHP -= Data.SlamDamage;
            StartCoroutine(Ouch());
        }
    }

    IEnumerator Ouch()
    {
        if (gameObject != null)
        {
            Rend.material = DamageFlash;
            yield return new WaitForSeconds(flashDur);
            Rend.material = OrigMaterial;
        }
        yield return null;
    }

    void Update() //måske fixedupdate
    {
        if (EHP <= 0.1f)
        {
            Debug.Log("am dead now");
            gameObject.SetActive(false);

            EHP = EnemyHitPoint;
        }
        if (EnemyRB.position.y <= ground)
        {
            EnemyRB.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            EnemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}


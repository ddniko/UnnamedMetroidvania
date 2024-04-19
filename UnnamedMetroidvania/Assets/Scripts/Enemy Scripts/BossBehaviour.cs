using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BossBehaviour : MonoBehaviour
{
    private SpriteRenderer Rend;
    private GameObject Player;
    [SerializeField] private LayerMask GroundLayer;

    //Combat
    private float BHP = 150;
    private float BHPStart = 150;
    [SerializeField] private PlayerDataWithDash Data;
    [SerializeField] private NewPlayerMovement PlayerData;
    [HideInInspector] public int MPRevcory = 5;

    [SerializeField] private Material DamageFlash;
    private Material OrigMaterial;
    [SerializeField] private float flashDur;

    //Positions
    [SerializeField] private Transform TopRight;
    [SerializeField] private Transform BottomLeft;
    [SerializeField] private Transform SweepRight;
    [SerializeField] private Transform SweepLeft;
    private Vector2 TeleportPos;
    private Vector2 Gone;

    //First attack
    [SerializeField] private GameObject Projectile;
    private GameObject Yellow;
    private GameObject Green;
    private GameObject Blue;
    private Transform RedPos;
    private Transform OrangePos;
    private Transform GreenPos;
    private Transform BluePos;

    //Second attack
    private Rigidbody2D BossRB;
    private Vector2 SweepDir;
    private bool Sweeping;

    //third attack
    private bool Slamming;
    private bool Hovering;

    //fourth attack
    private Transform SlamRight;
    private Transform SlamLeft;
    private Vector3 HoverPos;

    bool startBoss;

    [HideInInspector] public int Attacknumber;

    void Start()
    {
        RedPos = transform.Find("RedPos");
        OrangePos = transform.Find("OrangePos");
        GreenPos = transform.Find("GreenPos");
        BluePos = transform.Find("BluePos");
        SlamRight = transform.Find("SlamRight");
        SlamLeft = transform.Find("SlamLeft");
        BossRB = gameObject.GetComponent<Rigidbody2D>();
        Rend = GetComponent<SpriteRenderer>();
        OrigMaterial = Rend.material;
        Player = GameObject.Find("Player");
        Rend.color = Color.red;
    }

    
    private void Update()
    {
        if (gameObject.activeSelf && startBoss == false)
        {
            startBoss = true;
            BHP = BHPStart;
            Debug.Log(startBoss);
            Debug.Log(BHP);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ChooseAttack();
        }

        if ((Vector2.Distance(gameObject.transform.position, SweepRight.transform.position) < 0.3f || Vector2.Distance(gameObject.transform.position, SweepLeft.transform.position) < 0.3f) && Sweeping)
        {
            BossRB.velocity = Vector2.zero;
            Sweeping = false;
        }

        if (Hovering)
        {
            HoverPos = new Vector3(Player.transform.position.x, TopRight.position.y, 0);
            Vector3 CurrentPos = gameObject.transform.position;
            Vector3 Velocity = Vector3.zero;
            gameObject.transform.position = Vector3.SmoothDamp(CurrentPos, HoverPos, ref Velocity, 0.03f);
        }

        if (Sweeping)
        {

        }

        if (BHP <= 0)
        {
            Debug.Log("Boss defeated");
            Destroy(gameObject);
        }
    }

    private void ChooseAttack()
    {
        Attacknumber = Random.Range(1, 4);
        //Attacknumber = 1;
        StartCoroutine("Attack" + Attacknumber);
    }

    #region ATTACKS
    IEnumerator Attack1()
    {
        Rend.color = Color.red;
        RandomPos();
        yield return new WaitForSeconds(1f);
        Instantiate(Projectile, RedPos.position,RedPos.rotation).SendMessage("ColorBlast");
        yield return new WaitForSeconds(0.5f);
        Yellow = Instantiate(Projectile, OrangePos.position, OrangePos.rotation);
        Yellow.GetComponent<SpriteRenderer>().color = Color.yellow;
        Yellow.SendMessage("ColorBlast");
        yield return new WaitForSeconds(0.5f);
        Green = Instantiate(Projectile, GreenPos.position, GreenPos.rotation);
        Green.GetComponent<SpriteRenderer>().color = Color.green;
        Green.SendMessage("ColorBlast");
        yield return new WaitForSeconds(0.5f);
        Blue = Instantiate(Projectile, BluePos.position, BluePos.rotation);
        Blue.GetComponent<SpriteRenderer>().color = Color.blue;
        Blue.SendMessage("ColorBlast");

        yield return new WaitForSeconds(5f);
        Disappear();
        yield return new WaitForSeconds(1f);
        ChooseAttack();
    }
    IEnumerator Attack2()
    {
        Sweeping = true;
        Rend.color = Color.blue;
        RandomPos();
        yield return new WaitForSeconds(1f);
        if (Vector2.Distance(Player.transform.position, SweepRight.transform.position) < Vector2.Distance(Player.transform.position, SweepLeft.transform.position))
        {
            if (Vector2.Distance(gameObject.transform.position, SweepRight.transform.position) > 35f)
            {
                BossRB.velocity = SweepDir.normalized * 25f * 1.3f;
            }
            SweepDir = SweepRight.transform.position - gameObject.transform.position;
            BossRB.velocity = SweepDir.normalized * 25f;
            if (Vector2.Distance(gameObject.transform.position, SweepRight.transform.position) < 20f)
            {
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }
            SweepDir = SweepLeft.transform.position - gameObject.transform.position;
            BossRB.velocity = SweepDir / 1.4f;
            yield return new WaitForSeconds(0.1f);
            Sweeping = true;
        }
        else
        {
            if (Vector2.Distance(gameObject.transform.position, SweepLeft.transform.position) > 35f)
            {
                BossRB.velocity = SweepDir.normalized * 25f * 1.3f;
            }
            SweepDir = SweepLeft.transform.position - gameObject.transform.position;
            BossRB.velocity = SweepDir.normalized * 25f;
            if (Vector2.Distance(gameObject.transform.position, SweepLeft.transform.position) < 20f)
            {
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }
            SweepDir = SweepRight.transform.position - gameObject.transform.position;
            BossRB.velocity = SweepDir / 1.4f;
            yield return new WaitForSeconds(0.1f);
            Sweeping = true;
        }
        yield return new WaitForSeconds(3f);
        Disappear();
        yield return new WaitForSeconds(1f);
        ChooseAttack();
    }
    IEnumerator Attack3()
    {
        Rend.color = Color.yellow;
        for (int i = 0; i < 3; i++)
        {
            gameObject.transform.position = HoverPos;
            BossRB.velocity = Vector3.zero;
            Hovering = true;
            yield return new WaitForSeconds(2f);
            Hovering = false;
            yield return new WaitForSeconds(0.2f);
            Slamming = true;
            BossRB.velocity = Vector2.down * 40f;
            yield return new WaitForSeconds(0.9f);
        }
        Disappear();
        yield return new WaitForSeconds(1.5f);
        ChooseAttack();

    }
    #endregion

    IEnumerator BossIntro()
    {
        // start en intro til bossen, f.eks. Navn kommer op på skærmen
        yield return null;
    }

    private void RandomPos()
    {
        float xPos = Random.Range(BottomLeft.position.x, TopRight.position.x);
        float yPos = Random.Range(BottomLeft.position.y, TopRight.position.y);

        TeleportPos = new Vector2(xPos, yPos);
        gameObject.transform.position = TeleportPos;
    }

    private void Disappear()
    {
        Gone = new Vector2(100, 100);
        gameObject.transform.position = Gone;
        BossRB.velocity = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "BossGround" && Slamming)
        {
            GameObject SlamRightproj = Instantiate(Projectile, SlamRight.position, SlamRight.rotation);
            GameObject SlamLeftproj = Instantiate(Projectile, SlamLeft.position, SlamLeft.rotation);
            SlamRightproj.SendMessage("SlamShot");
            SlamLeftproj.SendMessage("SlamShot");
            Slamming = false;
        }

        if (collision.tag == "Sword")
        {
            BHP -= Data.SDamage;
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
            BHP -= Data.ArrowBaseDamage * (Arrow.velocity.magnitude / Data.ArrowSpeed);
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
            StartCoroutine(Ouch());
            BHP -= Data.FireballDamage;
        }

        if (collision.tag == "Slam") //Hvis slammet gå opad
        {
            StartCoroutine(Ouch());
            BHP -= Data.SlamDamage;
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
    void DespawnBoss()
    {
        startBoss = false;
        gameObject.SetActive(false);
    }
}

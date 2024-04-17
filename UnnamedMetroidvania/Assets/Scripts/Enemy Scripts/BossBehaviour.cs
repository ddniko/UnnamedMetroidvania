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
    [SerializeField] private PlayerDataWithDash Data;
    [SerializeField] private NewPlayerMovement PlayerData;
    [HideInInspector] public int MPRevcory = 5;

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
    [SerializeField] private Transform SlamPos;
    private Transform SlamRight;
    private Transform SlamLeft;
    private Vector3 HoverPos;



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
        Player = GameObject.Find("ShootME");
    }

    
    private void Update()
    {
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
            //gameObject.transform.position = new Vector2(Player.transform.position.x, TopRight.position.y);
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
        /*
         * Spawn inden for bestemt område
         * Lav angreb
         * Teleporter/Dash væk igen
         * Kør nyt attack metode
         */
        Debug.Log("Attack1 Teleport");
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
        /*
         * Sweep
         * 
         * 
         * 
         */
        Debug.Log("Attack2");
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
        /*
         * Slam, hover lige over hovedet af spilleren til den slammer jorden og laver 1-2 projektiler på hver side
         */
        Rend.color = Color.yellow;
        Debug.Log("Attack3");

        for (int i = 0; i < 3; i++)
        {
            gameObject.transform.position = HoverPos;
            BossRB.velocity = Vector3.zero;
            Hovering = true;
            yield return new WaitForSeconds(2.2f);
            Slam();
            yield return new WaitForSeconds(0.8f);
        }
        Debug.Log("done");
        /*Hovering = true;
        yield return new WaitForSeconds(2f);
        Slam();
        yield return new WaitForSeconds(1f);
        Hovering = true;
        yield return new WaitForSeconds(2f);
        Slam();
        yield return new WaitForSeconds(1f);
        Hovering = true;
        yield return new WaitForSeconds(2f);
        Slam();
        yield return new WaitForSeconds(1f);*/

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

    private void Slam()
    {
        Slamming = true;
        Hovering = false;
        BossRB.velocity = Vector2.down * 70f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "BossGround" && Slamming)
        {
            Debug.Log("collision?");
            GameObject SlamRightproj = Instantiate(Projectile, SlamRight.position, SlamRight.rotation);
            GameObject SlamLeftproj = Instantiate(Projectile, SlamLeft.position, SlamLeft.rotation);
            SlamRightproj.SendMessage("SlamShot");
            SlamLeftproj.SendMessage("SlamShot");
            Slamming = false;
        }

        if (collision.tag == "Sword")
        {

            //Lav method for at vise de bliver ramt


            BHP -= Data.SDamage;
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

            //Lav method for at vise de bliver ramt


            Rigidbody2D Arrow = collision.GetComponent<Rigidbody2D>();
            BHP -= Data.ArrowBaseDamage * (Arrow.velocity.magnitude / Data.ArrowSpeed);
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
            //Lav method for at vise de bliver ramt

            BHP -= Data.FireballDamage;
        }

        if (collision.tag == "Slam") //Hvis slammet gå opad
        {
            //Lav method for at vise de bliver ramt

            BHP -= Data.SlamDamage;
        }
    }
}

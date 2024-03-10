using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerControls : MonoBehaviour
{
    public CharacterController2D Controller; //referee CharacterControllerens metode i det her script

    [SerializeField] private float ActiveFrames = 0.5f;
    [SerializeField] private float AttackSpeed = 3f;
    [SerializeField] private float runSpeed = 40f; //speed kan ændres efter behov
    [SerializeField] private float DashCooldown = 0.7f;

    float Cooldown = 0f;
    float IFrames = 0f;

    //defineret yderst så de kan bruges i fixedupdate
    float horizontalMove = 0f;
    bool jump = false;
    bool crouch;
    bool Dashing;
    public float MDamage = 1.2f;
    int Upgrade = 1;
    float DamageMulti = 1f;
    public int PHP = 5;
    float DashTimer = 0.7f;

    private Rigidbody2D enemyRB;
    private Rigidbody2D playerRB;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;
    private bool ignore = false;


    [Header("Events and references")]
    [Space]

    public UnityEvent A_Sword;
    public UnityEvent D_Sword;
    public GameObject Sword;


    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody2D>();
        if (A_Sword == null) //failstate for events, ellers er det kinda buggy
        {
            A_Sword = new UnityEvent();
        }
        if (D_Sword == null)
        {
            D_Sword = new UnityEvent();
        }
        OnUpgrade();
    }

    void Update()
    {
        DashTimer -= Time.deltaTime;
        Cooldown -= Time.deltaTime;
        IFrames -= Time.deltaTime;

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed; //bevæger ens karakter horisontant

        if (Input.GetButtonDown("Jump")) //Får ens karakter til at hoppe
        {
            jump = true;
        } 
        if (Input.GetButtonUp("Jump") && playerRB.velocity.y >= 0)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x,playerRB.velocity.y * 0.5f);
        }

        if (Input.GetButtonDown("Crouch")) //får ens karakter til at crouch og uncrouch
        {
            crouch = true;
        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }
        if (Input.GetButtonDown("Dash") && DashTimer <= 0)
        {
            Dashing = true;
            DashTimer = DashCooldown;
        }
        //Når man har Iframes kan man ikke rør en enemy
        if(IFrames <= 0)
        if (Input.GetButtonDown("Fire1") && Cooldown <= 0f) //sørger for at man først kan attack efter 3 sek
        {
            startAttack(); //kørere startattack metoden
            Cooldown = AttackSpeed; //resetter cooldown til attackspeed
            Invoke("endAttack", ActiveFrames); //fjerne sværet efter en bestemt mængde tid
        }

        if (IFrames <= 0) //Når man har Iframes kan man ikke rør en enemy
        {
            ignore = false;
        }
        Physics2D.IgnoreLayerCollision(7, 8, ignore); //ignorere collision, mellem lag 7 og 8 som er player og enemy
    }
    void startAttack()
    {
        A_Sword.Invoke(); //invoke kører starter et event, som gør sværet aktivt
    }
    void endAttack()
    {
        D_Sword.Invoke(); //starter event, som deaktivere sværet
    }
    private void FixedUpdate() //lavet i fixedupdate da den opdatere sammen med alle physics calculations
    {
        Controller.Move(horizontalMove, crouch, jump, Dashing);
        jump = false;
        Dashing = false;
    }
    void OnUpgrade()
    {
        Upgrade++;
        MDamage = MDamage * Upgrade * DamageMulti;
    }

    //Knockback og Iframes
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Hvis man har Iframes kører dette ikke
        if (IFrames <= 0f)
        {
            //Hvis spilleren rør en "enemy" tager de skade, for Iframs
            if (collision.gameObject.tag == "Enemy")
            {
                IFrames = 1.5f;
                ignore = true;
                PHP--;

                //Henter rigidbodyen af enemyen og playeren
                enemyRB = collision.gameObject.GetComponent<Rigidbody2D>();

                playerRB = gameObject.GetComponent<Rigidbody2D>(); 


                //Laver en normalvektor og scaler den op så spilleren tager knockback
                Vector2 knockback = new Vector2(enemyRB.position.x - playerRB.position.x, enemyRB.position.y - playerRB.position.y);
                playerRB.velocity = Vector2.zero;
                playerRB.AddForce(-knockback * 1000f);
            }
        }
    }
}


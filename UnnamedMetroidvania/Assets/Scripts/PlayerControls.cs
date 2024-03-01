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

    float Cooldown = 0f;

    //defineret yderst så de kan bruges i fixedupdate
    float horizontalMove = 0f;
    bool jump = false;
    bool crouch;
    public float MDamage = 1.2f;
    int Upgrade = 1;
    float DamageMulti = 1f;

    [Header("Events and references")]
    [Space]

    public UnityEvent A_Sword;
    public UnityEvent D_Sword;
    public GameObject Sword;

    void Start()
    {
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
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed; //bevæger ens karakter horisontant

        if (Input.GetButtonDown("Jump")) //Får ens karakter til at hoppe
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch")) //får ens karakter til at crouch og uncrouch
        {
            crouch = true;
        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        Cooldown -= Time.deltaTime;
        //Debug.Log(Cooldown);

        //Måske tiføj dette til fixedupdate, da det kører efter physics ticks
        if (Cooldown <= 0f) //sørger for at man først kan attack efter 3 sek
        {
            if (Input.GetButtonDown("Fire1")) //kan ændres senere
            {
                startAttack(); //kørere startattack metoden
                Cooldown = AttackSpeed; //resetter cooldown til attackspeed
                Invoke("endAttack", ActiveFrames); //fjerne sværet efter en bestemt mængde tid
            }
        }

    }
    void startAttack()
    {
        //Debug.Log("Started?");
        A_Sword.Invoke(); //invoke kører starter et event, som gør sværet aktivt
    }
    void endAttack()
    {
        //Debug.Log("Done?");
        D_Sword.Invoke(); //starter event, som deaktivere sværet
    }
    private void FixedUpdate() //lavet i fixedupdate da den opdatere sammen med alle physics calculations
    {
        Controller.Move(horizontalMove, crouch, jump);
        jump = false;
    }
    void OnUpgrade()
    {
        Upgrade++;
        MDamage = MDamage * Upgrade * DamageMulti;
    }
}


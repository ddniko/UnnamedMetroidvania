using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D Controller; //referee CharacterControllerens metode i det her script
    [SerializeField] private float runSpeed = 40f; //speed kan �ndres efter behov
    //defineret yderst s� de kan bruges i fixedupdate
    float horizontalMove = 0f;
    bool jump = false;
    bool crouch;

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed; //bev�ger ens karakter horisontant

        if (Input.GetButtonDown("Jump")) //F�r ens karakter til at hoppe
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch")) //f�r ens karakter til at crouch og uncrouch
        {
            crouch = true;
        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }
    }
    private void FixedUpdate() //lavet i fixedupdate da den opdatere sammen med alle physics calculations
    {
        Controller.Move(horizontalMove, crouch, jump);
        jump = false;
    }
}


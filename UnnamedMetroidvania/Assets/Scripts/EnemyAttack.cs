using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject attackRadiusRightCorner;
    public GameObject attackRadiusLeftCorner;

    public GameObject player;

    public bool DieNow = false;

    [SerializeField] private LayerMask m_WhatIsPlayer;


    // Update is called once per frame
    void Update()
    {
        Vector2 attackRight = attackRadiusRightCorner.transform.position;
        Vector2 attackLeft = attackRadiusLeftCorner.transform.position;


        if (Physics2D.OverlapArea(attackLeft, attackRight, m_WhatIsPlayer))
        {
            DieNow = true;
        }
        else
        {
            DieNow = false;
        }
    }


    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackRadiusRightCorner.transform.position, 0.1f);
        Gizmos.DrawWireSphere(attackRadiusLeftCorner.transform.position, 0.1f);
        Gizmos.DrawLine(attackRadiusLeftCorner.transform.position, attackRadiusRightCorner.transform.position);
    }
}

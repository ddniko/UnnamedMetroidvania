using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySimpelPatrol : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D RB;
    private Vector2 dir;
    private Transform currentPoint;
    public float speed = 0.5f;
    private GameObject GroundCheck;
    [SerializeField] private LayerMask Ground;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    private bool IsGround;
    [SerializeField] private float forceMulti;
    [SerializeField] private float Up;



    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
        IsGround = true;

        GroundCheck = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGround)
        {
            Vector2 point = currentPoint.position - transform.position;
            //Fortæller eneymen at den skal starte med at gå imod punktet B
            if (currentPoint == pointB.transform)
            {
                RB.velocity = new Vector2(speed, 0);
            }
            else
            {
                RB.velocity = new Vector2(-speed, 0);
            }
            //Når enemyen når hen til punktet B, går den imod punktet A
            if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == pointB.transform)
            {
                flip();
                currentPoint = pointA.transform;
            }
            //Når enemyen når hen til punktet A, går den imod punktet B
            if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == pointA.transform)
            {
                flip();
                currentPoint = pointB.transform;
            }
        }
    }
    //Vender enemyens x-vinkel ved at gange med -1
    private void flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    //Laver nogle cikler for punkterne A og B, hvor der laves en linje imellem dem
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            IsGround = false;
            dir = new Vector2(-Mathf.Sign(collision.transform.position.x - RB.position.x),Up);
            RB.velocity = Vector2.zero;
            RB.AddForce(dir * forceMulti, ForceMode2D.Impulse);

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((Ground.value & (1 << collision.gameObject.layer)) > 0)
        {
            IsGround = true;
        }
    }
}
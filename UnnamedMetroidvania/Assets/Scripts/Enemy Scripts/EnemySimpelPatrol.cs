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
    [SerializeField] private LayerMask Ground;
    private bool IsGround;
    [SerializeField] private float forceMulti;
    [SerializeField] private float Up;
    [SerializeField] private float KnockupForce;
    private bool right;
    private Vector3 localScale;



    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
        IsGround = true;
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
        }
        //Når enemyen når hen til punktet B, går den imod punktet A
        if (Vector2.Distance(transform.position, currentPoint.position) < 1.5f && currentPoint == pointB.transform)
        {
            if (IsGround)
            {
                flip();
            }
            currentPoint = pointA.transform;
            right = true;
        }
        //Når enemyen når hen til punktet A, går den imod punktet B
        if (Vector2.Distance(transform.position, currentPoint.position) < 1.5f && currentPoint == pointA.transform)
        {
            if (IsGround)
            {
                flip();
            }
            currentPoint = pointB.transform;
            right = false;
        }
    }
    //Vender enemyens x-vinkel ved at gange med -1
    private void flip()
    {
        localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    //Laver nogle cikler for punkterne A og B, hvor der laves en linje imellem dem
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 1f);
        Gizmos.DrawWireSphere(pointB.transform.position, 1f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
    #region KNOCKBACK
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            if (collision.transform.eulerAngles == new Vector3(0, 0, 180))
            {
                StartCoroutine(Stun());
            }
            else if (collision.transform.eulerAngles == new Vector3(0, 0, 0))
            {
                IsGround = false;
                RB.velocity = Vector2.zero;
                dir = Vector2.up * forceMulti;
                RB.AddForce(dir * forceMulti, ForceMode2D.Impulse);
            }
            else
            {
                IsGround = false;
                RB.velocity = Vector2.zero;
                dir = new Vector2(-Mathf.Sign(collision.transform.position.x - RB.position.x), Up);
                RB.AddForce(dir * forceMulti, ForceMode2D.Impulse);
            }
        }

        if (collision.tag == "Slam") //Hvis slammet gå opad
        {
            IsGround = false;
            RB.AddForce(Vector2.up * KnockupForce, ForceMode2D.Impulse);
            //EnemyRB.position = new Vector2(EnemyRB.position.x, EnemyRB.position.y + slamHeight); //ændre til force?
            RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        if (collision.tag == "Arrow") //Hvis slammet gå opad
        {
            IsGround = false;
            RB.velocity = Vector2.zero;
            dir = new Vector2(-Mathf.Sign(collision.transform.position.x - RB.position.x), Up);
            RB.AddForce(dir * collision.attachedRigidbody.velocity.magnitude / 20f * forceMulti, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((Ground.value & (1 << collision.gameObject.layer)) > 0)
        {
            IsGround = true;

            if (right && localScale.x >0)
            {
                flip();
            }
            else if (!right && localScale.x <0)
            {
                flip();
            }
        }
    }

    IEnumerator Stun()
    {
        IsGround = false;
        RB.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
        IsGround = true;
    }
    #endregion
}
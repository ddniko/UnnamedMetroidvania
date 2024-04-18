using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordPogo : MonoBehaviour
{
    public PlayerDataWithDash Data;
    [SerializeField] private GameObject Player;
    [SerializeField] private float PogoForce;
    private Rigidbody2D PlayerRB;
    [SerializeField] private NewPlayerMovement PlayerData;
    [SerializeField] private GameObject PogoPos;
    [SerializeField] private GameObject UpSlashPos;
    [SerializeField] private GameObject StreightSlashPos;

    private bool Pogo;
    private bool upSlash;


    private void Start()
    {
        PlayerRB = Player.GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (Pogo)
        {
            gameObject.transform.position = PogoPos.transform.position;
            gameObject.transform.eulerAngles = PogoPos.transform.eulerAngles;
        }
        else if (upSlash)
        {
            gameObject.transform.position = UpSlashPos.transform.position;
            gameObject.transform.eulerAngles = UpSlashPos.transform.eulerAngles;
        }
        else
        {
            gameObject.transform.position = StreightSlashPos.transform.position;
            gameObject.transform.eulerAngles = StreightSlashPos.transform.eulerAngles;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && gameObject.transform.eulerAngles == new Vector3(0,0,180))
        {
            PlayerRB.velocity = Vector3.zero;
            PlayerRB.AddForce(Vector2.up * PogoForce, ForceMode2D.Impulse);
        }
    }

    private void UpHit()
    {
        StartCoroutine(UpHitRoutine());
    }
    IEnumerator UpHitRoutine()
    {
        if (gameObject != null)
        {
            upSlash = true;
            yield return new WaitForSeconds(Data.ActiveFrames);
            upSlash = false;
            gameObject.SetActive(false);
        }
        yield return null;
    }

    private void PogoHit()
    {
        StartCoroutine(PogoRoutine());
    }

    IEnumerator PogoRoutine()
    {
        if (gameObject != null)
        {
            Pogo = true;
            yield return new WaitForSeconds(Data.ActiveFrames);
            Pogo = false;
            gameObject.SetActive(false);
        }
        yield return null;
    }
    private void NormalHit()
    {
        StartCoroutine(NormalRoutine());
    }

    IEnumerator NormalRoutine()
    {
        if (gameObject != null)
        {
            yield return new WaitForSeconds(Data.ActiveFrames);
            gameObject.SetActive(false);
        }
        yield return null;
    }
}


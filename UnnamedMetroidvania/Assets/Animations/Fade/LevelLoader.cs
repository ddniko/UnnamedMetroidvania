    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public Rigidbody2D Player;

    public Animator transitionCircleWipeRightToLeft;
    public Animator transitionCircleWipeLeftToRight;

    public GameObject[] EnemyLeft;
    public GameObject[] EnemyRight;

    public float transitionTime = 1f;
    public float transitionWarp = 5f;

    private bool SetAsleep = false;

    private Rigidbody2D rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Player.velocity.x > 0)
        {
            for (int i = 0; i < 6; i++)
            {
                EnemyLeft[i].SetActive(false);
                EnemyRight[i].SetActive(true);
                if (EnemyRight[i].tag == "Enemy" && EnemyRight[i].layer != 3)
                {
                    EnemyRight[i].SendMessage("SetMaterial");
                }
            }

            StartCoroutine(LoadLevelRightToLeft());
        }

        if (collision.CompareTag("Player") && Player.velocity.x < 0)
        {
            for (int i = 0; i < 6; i++)
            {
                EnemyLeft[i].SetActive(true);
                if (EnemyLeft[i].tag == "Enemy")
                {
                    EnemyLeft[i].SendMessage("SetMaterial");
                }
                EnemyRight[i].SetActive(false);
            }

            StartCoroutine(LoadLevelLeftToRight());
        }
    }

    IEnumerator LoadLevelRightToLeft()
    {
        transitionCircleWipeRightToLeft.SetTrigger("Start");

        SetAsleep = true;

        yield return new WaitForSeconds(transitionTime);

        Player.transform.position = new Vector3 (rb.transform.position.x + transitionWarp, transform.position.y);

        transitionCircleWipeRightToLeft.SetTrigger("End");

        SetAsleep = false;
    }
    IEnumerator LoadLevelLeftToRight()
    {
        transitionCircleWipeLeftToRight.SetTrigger("Start");

        SetAsleep = true;

        yield return new WaitForSeconds(transitionTime);

        Player.transform.position = new Vector3(rb.transform.position.x - transitionWarp, transform.position.y);

        transitionCircleWipeLeftToRight.SetTrigger("End");

        SetAsleep = false;
    }

    private void Update()
    {
        if (SetAsleep)
        {
            Player.Sleep();
        }
        else
        {
            Player.WakeUp();
        }
    }

    public void respawnEnemiesToRight(bool respawnEnemiesRight)
    {
        if (respawnEnemiesRight)
        {
            for (int i = 0; i < 6; i++)
            {
                EnemyLeft[i].SetActive(false);
                EnemyRight[i].SetActive(true);
                if (EnemyRight[i].tag == "Enemy")
                {
                    EnemyRight[i].SendMessage("SetMaterial");
                }
            }
            respawnEnemiesRight = false;
        }
    }
    public void respawnEnemiesToLeft(bool respawnEnemiesLeft)
    {
        if (respawnEnemiesLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                EnemyLeft[i].SetActive(true);
                EnemyRight[i].SetActive(false);
                if (EnemyLeft[i].tag == "Enemy")
                {
                    EnemyLeft[i].SendMessage("SetMaterial");
                }
            }
            respawnEnemiesLeft = false;
        }
    }
    public void deathSide(string i)
    {
        if (i == "Left")
        {
            respawnEnemiesToLeft(true);
        }
        else
        {
            respawnEnemiesToRight(true);
        }
    }
}

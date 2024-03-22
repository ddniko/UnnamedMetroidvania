    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public Rigidbody2D Player;

    public Animator transitionCircleWipeRightToLeft;
    public Animator transitionCircleWipeLeftToRight;

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
            StartCoroutine(LoadLevelRightToLeft());
        }

        if (collision.CompareTag("Player") && Player.velocity.x < 0)
        {
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
}

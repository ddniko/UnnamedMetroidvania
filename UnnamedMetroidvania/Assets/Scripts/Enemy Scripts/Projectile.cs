using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 ShootDir;
    private GameObject Player;
    private Rigidbody2D projectileRB;
    [SerializeField] private float speed = 15;
    [SerializeField] private LayerMask DestroyProjectile;
    [SerializeField] private BossBehaviour BossData;
    private bool ColorBlaster;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("ShootME");
        projectileRB = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 5);
        if (ColorBlaster)
        {
            ShootDir = Player.transform.position - gameObject.transform.position;
            StartCoroutine(ShootPlayer());
        }
    }

    IEnumerator ShootPlayer()
    {
        yield return new WaitForSeconds(3);
        projectileRB.velocity = ShootDir.normalized * speed * 0.8f;
        ColorBlaster = false;
        yield return null;
    }

    private void ColorBlast()
    {
        ColorBlaster = true;
    }

    private void SlamShot()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = gameObject.transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((DestroyProjectile.value & (1 << collision.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
    }
}

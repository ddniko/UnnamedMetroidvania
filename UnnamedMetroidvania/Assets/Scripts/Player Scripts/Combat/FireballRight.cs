using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float Speed = 9f;

    // Update is called once per frame
    void Update()
    {
        //Skyder til højre for ens position
        transform.position += transform.right * Time.deltaTime * Speed;

        //Destroiere fireball efter 1 sek.
        StartCoroutine(DestroyFireball(1f));
    }

    private IEnumerator DestroyFireball(float destroyTime)
    {
        yield return new WaitForSecondsRealtime(destroyTime);
        Destroy(this.gameObject);
        Debug.Log("Destroied");
        yield return null;
    }
}

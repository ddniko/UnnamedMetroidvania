using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballLeft : MonoBehaviour
{
    public float Speed = 9f;

    // Update is called once per frame
    void Update()
    {
        transform.position -= transform.right * Time.deltaTime * Speed;

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

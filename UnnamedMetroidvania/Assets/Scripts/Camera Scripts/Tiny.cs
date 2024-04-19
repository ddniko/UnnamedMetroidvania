using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Tiny : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().enabled = false;
            GameObject.Find("NoYFollowCamera").GetComponent<CinemachineVirtualCamera>().enabled = true;
        }
    }
}

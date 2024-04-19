using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Big : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            GameObject.Find("NoYFollowCamera").GetComponent<CinemachineVirtualCamera>().enabled = false;
            GameObject.Find("LockedPositionCamera").GetComponent<CinemachineVirtualCamera>().enabled = true;
        }

    }
}

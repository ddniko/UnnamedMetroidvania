using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq.Expressions;

public class PickUps : MonoBehaviour
{

    public NewPlayerMovement PlayerData;

    public TMP_Text PlayerDataText;


    bool GotItem;

    // Update is called once per frame
    void Update()
    {
        if (GotItem)
        {
            StartCoroutine(ShowUnlock());
        }
    }


    IEnumerator ShowUnlock()
    {
        PlayerDataText.text = $"You Unlocked {PlayerData.Picked.name}";
        yield return new WaitForSeconds(2);
        PlayerDataText.text = " ";
        GotItem = false;
    }

    private void itemPicked()
    {
        GotItem = true;
    }

}

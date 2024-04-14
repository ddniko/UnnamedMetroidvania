using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextManager : MonoBehaviour
{
    public NewPlayerMovement PlayerData;

    public TMP_Text PlayerDataText;


    private void Update()
    {
        PlayerDataText.text = $"Lives: {PlayerData.PHP}\nMana: {PlayerData.MP}/{PlayerData.MaxMP}";
    }
}

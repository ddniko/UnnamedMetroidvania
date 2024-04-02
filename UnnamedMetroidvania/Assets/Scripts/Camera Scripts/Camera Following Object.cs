using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowingObject : MonoBehaviour
{
    //https://www.youtube.com/watch?v=9dzBrLUIF8g (2:55-5:03)

    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;

    private CharacterController2D _player; //Playerens controls

    private bool _isFacingRight;

    //Starter når scenen bliver loadet
    private void Awake()
    {
        //Henter transform af gameobject referrencen
        _player = _playerTransform.gameObject.GetComponent<CharacterController2D>();

        //Henter variablen der beskriver om spilleren vender mod højre
        _isFacingRight = _player.m_FacingRight; //_player scripts FacingRight variabel
    }

    private void Update()
    {
        //Gøre så kameraet objektet følger spillerens position hele tiden
        transform.position = _player.transform.position;
    }

    //Når spilleren vender sig kører denne kode
    public void CallTurn()
    {
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }
    //Her startes en coroutine
    private IEnumerator FlipYLerp()
    {
        //Henter transformens y-rotation (Vinkel)
        float startRotation = transform.localEulerAngles.y;
        //Henter værdien hvor den stopper med at rotere
        float endRotationAmount = DetermineEndRotation();
        //Sætter variablen yRotation til 0
        float yRotation = 0f;

        //Laver en time variable
        float elapsedTime = 0f;
        //Sørger for at kameraet roter så lang tid som _flipYRotatiionTime er
        while (elapsedTime < _flipYRotationTime)
        {
            //Øger elapsedTime i forhold til tiden den kører
            elapsedTime += Time.deltaTime;

            //Finder retningen og hvor meget den skal roter i den retning
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
            //Sætter objektet til at dreje med y-aksen i tagt med yRotation
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        //Gør så objektet drejer rundt sammen med spilleren
        _isFacingRight = !_isFacingRight;

        //Bestemmer hvordan den skal dreje rundt
        if (_isFacingRight)
        {
            return 0f;
        }
        else
        {
            return 180f;
        }
    }
}

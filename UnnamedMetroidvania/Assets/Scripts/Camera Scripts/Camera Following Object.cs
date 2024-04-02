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

    //Starter n�r scenen bliver loadet
    private void Awake()
    {
        //Henter transform af gameobject referrencen
        _player = _playerTransform.gameObject.GetComponent<CharacterController2D>();

        //Henter variablen der beskriver om spilleren vender mod h�jre
        _isFacingRight = _player.m_FacingRight; //_player scripts FacingRight variabel
    }

    private void Update()
    {
        //G�re s� kameraet objektet f�lger spillerens position hele tiden
        transform.position = _player.transform.position;
    }

    //N�r spilleren vender sig k�rer denne kode
    public void CallTurn()
    {
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }
    //Her startes en coroutine
    private IEnumerator FlipYLerp()
    {
        //Henter transformens y-rotation (Vinkel)
        float startRotation = transform.localEulerAngles.y;
        //Henter v�rdien hvor den stopper med at rotere
        float endRotationAmount = DetermineEndRotation();
        //S�tter variablen yRotation til 0
        float yRotation = 0f;

        //Laver en time variable
        float elapsedTime = 0f;
        //S�rger for at kameraet roter s� lang tid som _flipYRotatiionTime er
        while (elapsedTime < _flipYRotationTime)
        {
            //�ger elapsedTime i forhold til tiden den k�rer
            elapsedTime += Time.deltaTime;

            //Finder retningen og hvor meget den skal roter i den retning
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
            //S�tter objektet til at dreje med y-aksen i tagt med yRotation
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        //G�r s� objektet drejer rundt sammen med spilleren
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
